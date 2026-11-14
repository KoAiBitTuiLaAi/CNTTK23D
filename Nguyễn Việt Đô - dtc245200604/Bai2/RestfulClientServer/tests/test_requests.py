"""Replay the actual .http files, including REST Client named references."""
import json
import os
import re
import socket
import subprocess
import unittest

import test_api as api


class RequestFileTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.host = type('OwnedApi', (api.ApiTests,), {})
        # Normally use an ephemeral port; set EDU_HTTP_PORT=5000 for documented replay.
        cls.host.port = int(os.environ.get('EDU_HTTP_PORT', '0')) or api.free_port()
        cls.host.base = f'http://localhost:{cls.host.port}'
        with socket.socket() as probe:
            probe.bind(('127.0.0.1', cls.host.port))
        api.EVIDENCE.mkdir(parents=True, exist_ok=True)
        cls.host.log = (api.EVIDENCE / f'http-files-{cls.host.port}.log').open('w', encoding='utf-8')
        cls.host.start_server()

    @classmethod
    def tearDownClass(cls):
        cls.host.tearDownClass()

    def test_http_files_exact_counts_status_body_and_named_references(self):
        expectations = {
            'requests.http': [200, 200, 404, 201, 200, 400, 404],
            'requests-errors.http': [400, 400, 400, 400, 415, 405, 404, 404],
            'requests-extension.http': [400, 201, 409, 204, 404, 201, 200],
        }
        transcript = []
        for filename, expected in expectations.items():
            source = (api.ROOT / filename).read_text(encoding='utf-8')
            blocks = re.split(r'^###.*$', source, flags=re.MULTILINE)[1:]
            self.assertEqual(len(expected), len(blocks), filename)
            named = {}
            for index, (block, status) in enumerate(zip(blocks, expected), 1):
                with self.subTest(file=filename, case=index):
                    name_match = re.search(r'^# @name (\w+)$', block, flags=re.MULTILINE)
                    match = re.search(r'^(GET|POST|PUT|DELETE) (\S+)\s*$', block, re.MULTILINE)
                    self.assertIsNotNone(match)
                    assert match is not None
                    method, url = match.groups()
                    url = url.replace('{{api}}', self.host.base)
                    url = re.sub(r'\{\{(\w+)\.response\.body\.\$\.id\}\}',
                                 lambda ref: named[ref.group(1)]['id'], url)
                    self.assertNotIn('{{', url)
                    remainder = block[match.end():].lstrip('\n')
                    content_type = re.search(r'^Content-Type: (.+)$', remainder, re.MULTILINE)
                    body = remainder.split('\n\n', 1)[1].strip().encode() if content_type else None
                    response = api.request(self.host.base, url[len(self.host.base):], method, body,
                                           content_type.group(1) if content_type else 'application/json')
                    self.assertEqual(status, response[0], response[3])
                    transcript.append({'file': filename, 'case': index, 'method': method,
                                       'url': url, 'status': response[0], 'headers': response[1],
                                       'body': response[3].decode('utf-8', errors='replace')})
                    if name_match:
                        self.assertEqual(201, response[0])
                        named[name_match.group(1)] = response[2]
                        followup = api.request(self.host.base, response[1]['Location'])
                        self.assertEqual(200, followup[0])
                        self.assertEqual(response[2], followup[2])
                    if status == 204:
                        self.assertEqual(b'', response[3])
                        self.assertEqual(404, api.request(self.host.base, url[len(self.host.base):])[0])
                    if filename == 'requests-extension.http' and index == 6:
                        self.assertNotEqual(named['createExtension']['id'], response[2]['id'])
                    if filename == 'requests-errors.http' and index in (6, 7):
                        self.assertEqual(b'', response[3])
                    if filename == 'requests.http' and index in (3, 6, 7):
                        self.assertEqual({3: 'ASSIGNMENT_NOT_FOUND', 6: 'INVALID_SUBMISSION',
                                          7: 'SUBMISSION_NOT_FOUND'}[index], response[2]['code'])
                    if filename == 'requests-errors.http' and index in (1, 2, 8):
                        self.assertEqual('ASSIGNMENT_NOT_FOUND' if index == 8 else 'INVALID_SUBMISSION',
                                         response[2]['code'])
                    if filename == 'requests-extension.http' and index in (1, 3, 5):
                        self.assertEqual({1: 'ASSIGNMENT_CLOSED', 3: 'DUPLICATE_SUBMISSION',
                                          5: 'SUBMISSION_NOT_FOUND'}[index], response[2]['code'])
        (api.EVIDENCE / f'http-file-results-{self.host.port}.json').write_text(json.dumps(transcript, indent=2), encoding='utf-8')
        self.assertEqual(22, len(transcript))

    def test_client_default_or_explicit_port_twice(self):
        args = [] if self.host.port == 5000 else [self.host.base]
        results = []
        for _ in range(2):
            results.append(subprocess.run(['dotnet', 'run', '--no-build', '--project', 'src/Submission.Client', '--', *args],
                                         cwd=api.ROOT, capture_output=True, text=True, encoding='utf-8', timeout=30))
        for index, result in enumerate(results, 1):
            (api.EVIDENCE / f'client-run-{self.host.port}-{index}.txt').write_text(
                result.stdout + result.stderr + f'\nExit: {result.returncode}\n', encoding='utf-8')
        self.assertEqual(0, results[0].returncode, results[0].stderr)
        self.assertIn('3. GET Location: 200', results[0].stdout)
        self.assertIn('4. POST rong: 400', results[0].stdout)
        self.assertEqual(1, results[1].returncode)
        self.assertIn('DUPLICATE_SUBMISSION', results[1].stderr)


if __name__ == '__main__':
    unittest.main(verbosity=2)
