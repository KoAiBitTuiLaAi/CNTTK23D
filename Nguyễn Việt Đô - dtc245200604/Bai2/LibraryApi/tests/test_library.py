"""Integration tests: real .NET processes and HTTP, Python standard library only."""
import concurrent.futures
import json
import os
from pathlib import Path
import socket
import subprocess
import tempfile
import time
import unittest
import urllib.error
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
SCRATCH = Path(os.environ.get('LIBRARY_TEST_LOG_DIR', str(Path(os.environ.get('TMPDIR', tempfile.gettempdir())) / 'library-tests')))
SCRATCH.mkdir(parents=True, exist_ok=True)
API = ROOT / 'src/Library.Api/bin/Debug/net10.0/Library.Api.dll'
CLIENT = ROOT / 'src/Library.Client/bin/Debug/net10.0/Library.Client.dll'


def free_port():
    with socket.socket() as sock:
        sock.bind(('127.0.0.1', 0))
        return sock.getsockname()[1]


class LibraryTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        build = subprocess.run(['dotnet', 'build', str(ROOT / 'LibraryApi.slnx'), '--nologo'], capture_output=True, text=True, encoding='utf-8', errors='replace')
        (SCRATCH / 'build.log').write_text(build.stdout + build.stderr, encoding='utf-8')
        if build.returncode:
            raise RuntimeError(build.stdout + build.stderr)

    def setUp(self):
        self.port = free_port()
        self.base = f'http://127.0.0.1:{self.port}'
        self.log = (SCRATCH / f'{self._testMethodName}-api.log').open('w', encoding='utf-8')
        env = os.environ.copy()
        env['ASPNETCORE_URLS'] = self.base
        env['ASPNETCORE_ENVIRONMENT'] = 'Production'
        self.server = subprocess.Popen(['dotnet', str(API)], cwd=ROOT / 'src/Library.Api', env=env, stdout=self.log, stderr=subprocess.STDOUT)
        self.addCleanup(self.stop_server)
        deadline = time.monotonic() + 20
        while time.monotonic() < deadline:
            if self.server.poll() is not None:
                self.fail('API exited before listening; inspect ' + str(self.log.name))
            try:
                self.request('GET', '/')
                return
            except urllib.error.URLError:
                time.sleep(0.1)
        self.fail('API startup timeout')

    def stop_server(self):
        if self.server.poll() is None:
            self.server.terminate()
            try:
                self.server.wait(timeout=10)
            except subprocess.TimeoutExpired:
                self.server.kill()
                self.server.wait(timeout=5)
        self.log.close()

    def request(self, method, path, data=None, content_type='application/json'):
        body = json.dumps(data).encode('utf-8') if data is not None else None
        req = urllib.request.Request(self.base + path, body, {'Content-Type': content_type}, method=method)
        try:
            response = urllib.request.urlopen(req, timeout=10)
        except urllib.error.HTTPError as error:
            response = error
        with response:
            raw = response.read()
            return response.status, dict(response.headers), raw

    def create(self, title='Clean Code'):
        status, headers, raw = self.request('POST', '/api/books', {'title': title, 'author': 'Robert Martin', 'price': 125.50})
        self.assertEqual(201, status, raw)
        return headers, json.loads(raw)

    def test_01_root_service(self):
        status, headers, raw = self.request('GET', '/')
        self.assertEqual(200, status)
        self.assertIn('application/json', headers.get('Content-Type', ''))
        self.assertEqual({'service': 'Library.Api'}, json.loads(raw))

    def test_02_list_books(self):
        status, headers, raw = self.request('GET', '/api/books')
        self.assertEqual(200, status)
        self.assertIn('application/json', headers.get('Content-Type', ''))
        self.assertEqual([], json.loads(raw))

    def test_03_create_with_server_ids(self):
        headers, book = self.create()
        self.assertEqual({'id', 'title', 'author', 'price'}, set(book))
        self.assertEqual({'id': 1, 'title': 'Clean Code', 'author': 'Robert Martin', 'price': 125.50}, book)
        self.assertEqual('/api/books/1', headers['Location'])
        with concurrent.futures.ThreadPoolExecutor(max_workers=12) as pool:
            created = list(pool.map(lambda i: self.create(f'Book {i}')[1], range(48)))
        ids = [b['id'] for b in created] + [book['id']]
        self.assertEqual(49, len(set(ids)))
        self.assertEqual(list(range(1, 50)), sorted(ids))
        status, _, raw = self.request('GET', '/api/books')
        self.assertEqual(200, status)
        self.assertEqual(49, len(json.loads(raw)))

    def test_04_read_by_location_and_not_found(self):
        headers, book = self.create()
        status, _, raw = self.request('GET', headers['Location'])
        self.assertEqual(200, status)
        self.assertEqual(book, json.loads(raw))
        status, _, raw = self.request('GET', '/api/books/999999')
        self.assertEqual(404, status)
        self.assertEqual({'code': 'BOOK_NOT_FOUND', 'message': 'Book not found.'}, json.loads(raw))

    def test_05_invalid_book(self):
        cases = [
            {'title': '', 'author': 'A', 'price': 1},
            {'title': '  ', 'author': 'A', 'price': 1},
            {'title': 'B', 'author': '', 'price': 1},
            {'title': 'B', 'author': '  ', 'price': 1},
            {'title': 'B', 'author': 'A', 'price': 0},
            {'title': 'B', 'author': 'A', 'price': -2},
            {'title': None, 'author': 'A', 'price': 1},
            {'title': 'B', 'author': None, 'price': 1},
            {},
        ]
        for data in cases:
            with self.subTest(data=data):
                status, _, raw = self.request('POST', '/api/books', data)
                self.assertEqual(400, status)
                self.assertEqual({'code': 'INVALID_BOOK', 'message': 'Title and author are required; price must be positive.'}, json.loads(raw))
        _, book = self.create()
        self.assertEqual(1, book['id'])

    def test_06_delete(self):
        headers, book = self.create()
        status, _, raw = self.request('DELETE', headers['Location'])
        self.assertEqual(204, status)
        self.assertEqual(b'', raw)
        for method in ['GET', 'DELETE']:
            status, _, raw = self.request(method, headers['Location'])
            self.assertEqual(404, status)
            self.assertEqual('BOOK_NOT_FOUND', json.loads(raw)['code'])
        _, _, raw = self.request('GET', '/api/books')
        self.assertEqual([], json.loads(raw))

    def run_client(self, base=None):
        result = subprocess.run(['dotnet', str(CLIENT), base or self.base], capture_output=True, text=True, encoding='utf-8', errors='replace', timeout=25)
        (SCRATCH / f'{self._testMethodName}-client.log').write_text(result.stdout + result.stderr, encoding='utf-8')
        return result

    def test_07_framework_http_contract(self):
        status, _, raw = self.request('POST', '/api/books', {'title': 'B'}, 'text/plain')
        self.assertEqual(415, status)
        self.assertEqual(b'', raw)
        status, headers, raw = self.request('PUT', '/api/books')
        self.assertEqual(405, status)
        self.assertEqual({'GET', 'POST'}, {s.strip() for s in headers['Allow'].split(',')})
        self.assertEqual(b'', raw)
        for method in ['GET', 'DELETE']:
            status, _, raw = self.request(method, '/api/books/not-an-integer')
            self.assertEqual(404, status)
            self.assertEqual(b'', raw)

    def test_08_client_list(self):
        result = self.run_client()
        self.assertEqual(0, result.returncode, result.stderr)
        self.assertIn('1. LIST 200', result.stdout)

    def test_09_client_create(self):
        result = self.run_client()
        self.assertEqual(0, result.returncode, result.stderr)
        self.assertIn('2. CREATE 201', result.stdout)
        self.assertIn('Location=/api/books/1', result.stdout)
        _, _, raw = self.request('GET', '/api/books')
        self.assertEqual('The Pragmatic Programmer', json.loads(raw)[0]['title'])

    def test_10_client_read_location(self):
        result = self.run_client()
        self.assertEqual(0, result.returncode, result.stderr)
        self.assertIn('3. READ 200: Id=1; The Pragmatic Programmer', result.stdout)

    def test_11_client_invalid(self):
        result = self.run_client()
        self.assertEqual(0, result.returncode, result.stderr)
        lines = result.stdout.splitlines()
        self.assertEqual(4, len(lines), result.stdout)
        self.assertIn('4. INVALID 400: INVALID_BOOK', lines[3])
        self.assertEqual(['1.', '2.', '3.', '4.'], [line.split()[0] for line in lines])

    def test_12_client_stopped_network(self):
        self.stop_server()
        result = self.run_client()
        self.assertEqual(1, result.returncode)
        self.assertIn('CLIENT_ERROR:', result.stderr)
        self.assertNotIn('Unhandled exception', result.stderr)
        self.assertNotIn(' at Program.', result.stderr)


if __name__ == '__main__':
    unittest.main(verbosity=2)
