"""Integration tests against real .NET processes; Python standard library only."""
import concurrent.futures
import datetime as dt
import http.server
import json
import os
from pathlib import Path
import socket
import subprocess
import tempfile
import threading
import time
import unittest
import urllib.error
import urllib.request
from typing import Any, cast
import uuid

ROOT = Path(__file__).resolve().parents[1]
EVIDENCE = Path(os.environ.get("EDU_EVIDENCE_DIR", str(Path(tempfile.gettempdir()) / "edusubmit-tests")))
API_DLL = ROOT / "src/Submission.Api/bin/Debug/net10.0/Submission.Api.dll"
CLIENT_DLL = ROOT / "src/Submission.Client/bin/Debug/net10.0/Submission.Client.dll"
REPO = "https://github.com/vietdotest/EduSubmit-DTC245200604"


def free_port():
    with socket.socket() as sock:
        sock.bind(("127.0.0.1", 0))
        return sock.getsockname()[1]


def request(base, path, method="GET", body=None, content_type="application/json") -> tuple[int, dict, Any, bytes]:
    data = json.dumps(body).encode() if isinstance(body, dict) else body
    headers = {"Content-Type": content_type} if data is not None else {}
    req = urllib.request.Request(base + path, data=data, method=method, headers=headers)
    try:
        response = urllib.request.urlopen(req, timeout=10)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        raw = response.read()
        try:
            payload = json.loads(raw)
        except (ValueError, UnicodeDecodeError):
            payload = None
        return cast(int, response.status), dict(response.headers), payload, cast(bytes, raw)


class ApiTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        EVIDENCE.mkdir(parents=True, exist_ok=True)
        cls.port = free_port()
        cls.base = f"http://127.0.0.1:{cls.port}"
        cls.log = (EVIDENCE / f"api-{cls.port}.log").open("w", encoding="utf-8")
        cls.start_server()

    @classmethod
    def start_server(cls):
        env = os.environ.copy()
        env["ASPNETCORE_ENVIRONMENT"] = "Production"
        cls.server = subprocess.Popen(["dotnet", str(API_DLL), "--urls", cls.base], cwd=ROOT,
                                      stdout=cls.log, stderr=subprocess.STDOUT, env=env)
        for _ in range(100):
            if cls.server.poll() is not None:
                cls.log.close()
                raise RuntimeError("API stopped before startup; inspect API log")
            try:
                request(cls.base, "/")
                return
            except (OSError, urllib.error.URLError):
                time.sleep(0.1)
        cls.server.terminate()
        cls.server.wait(timeout=10)
        cls.log.close()
        raise RuntimeError("API startup timeout")

    @classmethod
    def tearDownClass(cls):
        cls.server.terminate()
        cls.server.wait(timeout=10)
        cls.log.close()

    def post(self, code=None, assignment=1, **fields):
        return request(self.base, f"/api/assignments/{assignment}/submissions", "POST",
                       {"studentCode": code or str(uuid.uuid4()), "repositoryUrl": REPO, **fields})

    def assert_error(self, response, status, code, message=None):
        self.assertEqual(status, response[0], response[3])
        self.assertEqual(code, response[2]["code"])
        if message:
            self.assertEqual(message, response[2]["message"])

    def client(self, *args):
        return subprocess.run(["dotnet", str(CLIENT_DLL), *args], cwd=ROOT,
                              capture_output=True, text=True, encoding="utf-8", timeout=35)

    def test_assignment_catalog(self):
        status, _, items, _ = request(self.base, "/api/assignments")
        self.assertEqual(200, status)
        self.assertEqual([
            {"id": 1, "title": "Bai tap REST API", "description": "Thiet ke endpoint RESTful", "isOpen": True},
            {"id": 2, "title": "Bai tap Client", "description": "Viet console client HttpClient", "isOpen": True},
            {"id": 3, "title": "Bai tap da dong", "description": "Bai tap het han nop", "isOpen": False},
        ], items)
        for item in items:
            detail = request(self.base, f'/api/assignments/{item["id"]}')
            self.assertEqual(200, detail[0])
            self.assertEqual(item, detail[2])
        self.assert_error(request(self.base, "/api/assignments/999"), 404,
                          "ASSIGNMENT_NOT_FOUND", "Khong tim thay bai tap.")

    def test_create_then_get_exact_location(self):
        code = str(uuid.uuid4())
        before = dt.datetime.now(dt.timezone.utc)
        status, headers, item, raw = self.post('  ' + code + '  ', repositoryUrl='  ' + REPO + '  ')
        self.assertEqual(201, status, raw)
        self.assertIsNotNone(item)
        self.assertEqual({'id', 'assignmentId', 'studentCode', 'repositoryUrl', 'status', 'receivedAt'}, set(item))
        self.assertNotEqual(uuid.UUID(int=0), uuid.UUID(item['id']))
        self.assertEqual(1, item['assignmentId'])
        self.assertEqual(code, item['studentCode'])
        self.assertEqual(REPO, item['repositoryUrl'])
        self.assertEqual('received', item['status'])
        received = dt.datetime.fromisoformat(item['receivedAt'].replace('Z', '+00:00'))
        self.assertEqual(dt.timedelta(0), received.utcoffset())
        self.assertLessEqual(before, received)
        self.assertLessEqual(received, dt.datetime.now(dt.timezone.utc))
        self.assertEqual('/api/submissions/' + item['id'], headers['Location'])
        retrieved = request(self.base, headers['Location'])
        self.assertEqual(200, retrieved[0])
        self.assertEqual(item, retrieved[2])
        self.assert_error(request(self.base, '/api/submissions/' + str(uuid.UUID(int=0))),
                          404, 'SUBMISSION_NOT_FOUND', 'Khong tim thay bai nop.')

    def test_validation_order_and_invalid_dto(self):
        for assignment, status, code, message in [
            (999, 404, 'ASSIGNMENT_NOT_FOUND', 'Khong tim thay bai tap.'),
            (3, 400, 'ASSIGNMENT_CLOSED', 'Bai tap da dong.'),
            (1, 400, 'INVALID_SUBMISSION', 'Thieu ma sinh vien hoac kho ma nguon.'),
        ]:
            with self.subTest(assignment=assignment):
                self.assert_error(request(self.base, f'/api/assignments/{assignment}/submissions',
                                          'POST', {}), status, code, message)
        for body in [{}, {'studentCode': None, 'repositoryUrl': REPO},
                     {'studentCode': '  ', 'repositoryUrl': REPO},
                     {'studentCode': 'SV', 'repositoryUrl': None},
                     {'studentCode': 'SV', 'repositoryUrl': '  '}]:
            with self.subTest(body=body):
                self.assert_error(request(self.base, '/api/assignments/1/submissions', 'POST', body),
                                  400, 'INVALID_SUBMISSION')

    def test_duplicate_case_insensitive_per_assignment(self):
        code = 'Case-' + str(uuid.uuid4())
        self.assertEqual(201, self.post(code)[0])
        self.assert_error(self.post('  ' + code.upper() + '  '), 409,
                          'DUPLICATE_SUBMISSION', 'Sinh vien da nop bai tap nay.')
        self.assertEqual(201, self.post(code.lower(), assignment=2)[0])
        self.assert_error(self.post(code, repositoryUrl='  '), 400, 'INVALID_SUBMISSION')

    def test_concurrent_duplicate_one_created(self):
        code = str(uuid.uuid4())
        with concurrent.futures.ThreadPoolExecutor(max_workers=16) as pool:
            responses = list(pool.map(lambda _: self.post(code), range(24)))
        self.assertEqual(1, sum(response[0] == 201 for response in responses))
        self.assertEqual(23, sum(response[0] == 409 for response in responses))
        for response in responses:
            if response[0] != 201:
                self.assert_error(response, 409, 'DUPLICATE_SUBMISSION')

    def test_delete_then_resubmit_new_id(self):
        code = str(uuid.uuid4())
        created = self.post(code)
        self.assertEqual(201, created[0])
        location = created[1]['Location']
        deleted = request(self.base, location, 'DELETE')
        self.assertEqual(204, deleted[0])
        self.assertEqual(b'', deleted[3])
        self.assert_error(request(self.base, location), 404, 'SUBMISSION_NOT_FOUND')
        self.assert_error(request(self.base, location, 'DELETE'), 404, 'SUBMISSION_NOT_FOUND')
        new = self.post(code)
        self.assertEqual(201, new[0])
        self.assertNotEqual(created[2]['id'], new[2]['id'])
        self.assertEqual(new[2], request(self.base, new[1]['Location'])[2])

    def test_client_four_steps_and_repeat_conflict(self):
        first = self.client(self.base)
        (EVIDENCE / 'client-first.txt').write_text(first.stdout + first.stderr, encoding='utf-8')
        self.assertEqual(0, first.returncode, first.stderr)
        self.assertIn('1. Bai tap dang mo', first.stdout)
        self.assertIn('2. POST: 201', first.stdout)
        self.assertIn('3. GET Location: 200', first.stdout)
        self.assertIn('DTC245200604', first.stdout)
        self.assertIn('4. POST rong: 400', first.stdout)
        self.assertIn('INVALID_SUBMISSION', first.stdout)
        location = next(line.split('Location: ', 1)[1] for line in first.stdout.splitlines()
                        if line.startswith('Location: '))
        self.assertEqual(200, request(self.base, location)[0])
        repeated = self.client(self.base)
        (EVIDENCE / 'client-repeat.txt').write_text(repeated.stdout + repeated.stderr, encoding='utf-8')
        self.assertEqual(1, repeated.returncode)
        self.assertIn('409', repeated.stdout + repeated.stderr)
        self.assertIn('DUPLICATE_SUBMISSION', repeated.stdout + repeated.stderr)
        self.assertNotIn('Unhandled exception', repeated.stderr)

    def test_client_bad_base_address(self):
        for base in ['not-a-url', 'ftp://localhost:5000/', 'http://localhost:5000/x',
                     'http://localhost:5000/?q=1', 'http://user:pass@localhost/']:
            with self.subTest(base=base):
                result = self.client(base)
                self.assertEqual(2, result.returncode)
                self.assertIn('BaseAddress', result.stderr)

    def test_client_network_failure(self):
        result = self.client(f'http://127.0.0.1:{free_port()}')
        self.assertNotEqual(0, result.returncode)
        self.assertIn('ket noi', result.stderr.lower())
        self.assertNotIn('Unhandled exception', result.stderr)

    def test_framework_errors_and_retired_flat_post(self):
        for path in ['/api/assignments/abc', '/api/submissions/not-a-guid']:
            with self.subTest(path=path):
                response = request(self.base, path)
                self.assertEqual(404, response[0])
                self.assertEqual(b'', response[3])
        put = request(self.base, '/api/assignments/1', 'PUT')
        self.assertEqual(405, put[0])
        self.assertIn('GET', put[1]['Allow'])
        self.assertEqual(b'', put[3])
        self.assertEqual(404, request(self.base, '/api/submissions', 'POST', {})[0])
        malformed = request(self.base, '/api/assignments/1/submissions', 'POST', b'{')
        self.assertEqual(400, malformed[0])
        wrong_dto = request(self.base, '/api/assignments/1/submissions', 'POST',
                            {'studentCode': 123, 'repositoryUrl': REPO})
        self.assertEqual(400, wrong_dto[0])
        self.assertEqual(415, request(self.base, '/api/assignments/1/submissions', 'POST',
                                      b'{}', 'text/plain')[0])
        self.assertEqual(400, request(self.base, '/api/assignments/1/submissions',
                                      'POST', b'null')[0])
        # Framework body binding runs before business validation.
        self.assertEqual(400, request(self.base, '/api/assignments/999/submissions',
                                      'POST', b'{')[0])

    def test_server_owns_generated_fields(self):
        response = self.post(assignment=2, id=str(uuid.UUID(int=0)), assignmentId=999,
                             status='forged', receivedAt='2000-01-01T00:00:00Z')
        self.assertEqual(201, response[0])
        item = response[2]
        self.assertEqual(2, item['assignmentId'])
        self.assertNotEqual(str(uuid.UUID(int=0)), item['id'])
        self.assertEqual('received', item['status'])
        self.assertNotEqual('2000-01-01T00:00:00Z', item['receivedAt'])

    def test_z_restart_clears_memory(self):
        created = self.post()
        self.assertEqual(201, created[0])
        self.server.terminate()
        self.server.wait(timeout=10)
        type(self).start_server()
        self.assert_error(request(self.base, created[1]['Location']), 404, 'SUBMISSION_NOT_FOUND')
        self.assertEqual(3, len(request(self.base, '/api/assignments')[2]))

    def test_client_http_error_bodies_and_timeout(self):
        # Real HTTP transport fixture, separate from evidence for the .NET API.
        class Handler(http.server.BaseHTTPRequestHandler):
            mode = 'empty'

            def do_GET(inner):
                if Handler.mode == 'slow':
                    time.sleep(11)
                status, body = {
                    'empty': (404, b''), 'html': (502, b'<html>Bad gateway</html>'),
                    'problem': (400, b'{"title":"Bad request"}'),
                    'badjson': (200, b'not-json'), 'slow': (200, b'[]'),
                    'noopen': (200, b'[{"id":3,"title":"closed","isOpen":false}]'),
                }[Handler.mode]
                try:
                    inner.send_response(status)
                    inner.send_header('Content-Type', 'application/json')
                    inner.end_headers()
                    inner.wfile.write(body)
                except (BrokenPipeError, ConnectionResetError, ConnectionAbortedError):
                    pass

            def log_message(inner, format, *args):
                pass

        with http.server.ThreadingHTTPServer(('127.0.0.1', 0), Handler) as server:
            thread = threading.Thread(target=server.serve_forever, daemon=True)
            thread.start()
            try:
                for mode, expected, fragment in [('empty', 1, 'khong co body'),
                        ('html', 1, 'khong phai JSON'), ('problem', 1, 'khong theo dang'),
                        ('badjson', 4, 'JSON khong hop le'), ('noopen', 1, 'Khong co bai tap'),
                        ('slow', 3, 'Het thoi gian')]:
                    with self.subTest(mode=mode):
                        Handler.mode = mode
                        result = self.client(f'http://127.0.0.1:{server.server_port}')
                        self.assertEqual(expected, result.returncode, result.stderr)
                        self.assertIn(fragment, result.stderr)
                        self.assertNotIn('Unhandled exception', result.stderr)
            finally:
                server.shutdown()
                thread.join(timeout=5)

    def test_root(self):
        status, _, data, raw = request(self.base, "/")
        self.assertEqual(200, status)
        self.assertEqual({"service": "Submission.Api", "status": "running"}, data, raw)


if __name__ == "__main__":
    unittest.main(verbosity=2)
