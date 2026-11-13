"""Kiểm thử tích hợp qua HTTP thật; chỉ dừng tiến trình do bộ kiểm thử tạo."""
import argparse
import contextlib
from concurrent.futures import ThreadPoolExecutor
from datetime import datetime, timezone
import uuid
import io
import re
import json
import os
from pathlib import Path
import socket
import subprocess
import sys
import time
import unittest
import urllib.error
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
EVIDENCE = ROOT / "tests" / "evidence"
API_DLL = ROOT / "src/Submission.Api/bin/Debug/net10.0/Submission.Api.dll"
CLIENT_DLL = ROOT / "src/Submission.Client/bin/Debug/net10.0/Submission.Client.dll"
ENV = dict(os.environ, DOTNET_NOLOGO="1", DOTNET_CLI_TELEMETRY_OPTOUT="1",
           ASPNETCORE_ENVIRONMENT="Production")
REPOSITORY = "https://github.com/vietdotest/EduSubmit-DTC245200604"


def free_port():
    with socket.socket() as probe:
        probe.bind(("127.0.0.1", 0))
        return probe.getsockname()[1]


@contextlib.contextmanager
def running_api(port):
    base = f"http://localhost:{port}"
    log_path = EVIDENCE / f"api-{port}.log"
    with log_path.open("w", encoding="utf-8") as log:
        process = subprocess.Popen(
            ["dotnet", str(API_DLL), "--urls", base], cwd=ROOT,
            env=ENV, stdout=log, stderr=subprocess.STDOUT)
        try:
            for _ in range(200):
                if process.poll() is not None:
                    raise RuntimeError(f"API thoát sớm: {log_path.read_text(encoding='utf-8')}")
                try:
                    with socket.create_connection(("127.0.0.1", port), timeout=0.1):
                        break
                except OSError:
                    time.sleep(0.05)
            else:
                raise RuntimeError("API không mở cổng trong thời gian cho phép")
            print(f"API started: PID={process.pid}, URL={base}")
            yield base
        finally:
            if process.poll() is None:
                process.terminate()
                try:
                    process.wait(timeout=10)
                except subprocess.TimeoutExpired:
                    process.kill()
                    process.wait(timeout=10)
            print(f"API stopped: PID={process.pid}, exit={process.returncode}, alive={process.poll() is None}")


def request(base, path, data=None):
    body = None if data is None else json.dumps(data).encode("utf-8")
    req = urllib.request.Request(base + path, data=body,
                                 headers={"Content-Type": "application/json"})
    try:
        response = urllib.request.urlopen(req, timeout=10)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        raw = response.read().decode("utf-8")
        result = json.loads(raw) if raw else None
        print(f"HTTP {req.get_method()} {path} -> {response.status}: {raw}")
        return response.status, dict(response.headers), result


def submission(**changes):
    return dict(dict(assignmentId=1, studentCode="DTC245200604", repositoryUrl=REPOSITORY), **changes)


def run_client(*args):
    result = subprocess.run(["dotnet", str(CLIENT_DLL), *args], cwd=ROOT, env=ENV,
        capture_output=True, text=True, encoding="utf-8", timeout=20)
    print("CLIENT:", " ".join(args) if args else "(URL mặc định)")
    print(result.stdout, result.stderr, f"CLIENT_EXIT={result.returncode}")
    return result


class EduSubmitTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.server = running_api(free_port())
        cls.base = cls.server.__enter__()

    @classmethod
    def tearDownClass(cls):
        cls.server.__exit__(None, None, None)

    def test_root_health(self):
        status, _, body = request(self.base, "/")
        self.assertEqual(status, 200)
        self.assertEqual(body, {"service": "Submission.Api", "status": "running"})

    def test_assignments_list(self):
        status, _, body = request(self.base, "/api/assignments")
        self.assertEqual(status, 200)
        self.assertEqual(len(body), 2)
        self.assertEqual([item["id"] for item in body], [1, 2])
        for item in body:
            self.assertEqual(set(item), {"id", "title", "description"})
            self.assertTrue(item["title"].strip())
            self.assertTrue(item["description"].strip())

    def test_create_submission(self):
        before = datetime.now(timezone.utc)
        status, headers, body = request(self.base, "/api/submissions", submission())
        self.assertEqual(status, 201)
        self.assertNotEqual(uuid.UUID(body["id"]).int, 0)
        self.assertEqual(headers["Location"], "/api/submissions/" + body["id"])
        self.assertEqual(body["assignmentId"], 1)
        self.assertEqual(body["studentCode"], "DTC245200604")
        self.assertEqual(body["repositoryUrl"], REPOSITORY)
        self.assertEqual(body["status"], "received")
        received = datetime.fromisoformat(body["receivedAt"].replace("Z", "+00:00"))
        self.assertEqual(received.utcoffset().total_seconds(), 0)
        self.assertLessEqual(before, received)
        self.assertLessEqual(received, datetime.now(timezone.utc))

    def test_reject_unknown_assignment(self):
        for assignment in (0, -1, 999):
            with self.subTest(assignment=assignment):
                status, _, _ = request(self.base, "/api/submissions", submission(assignmentId=assignment))
                self.assertEqual(status, 400)

    def test_reject_blank_student(self):
        for student in ("", "  ", "\t\n", None):
            with self.subTest(student=student):
                status, _, _ = request(self.base, "/api/submissions", submission(studentCode=student))
                self.assertEqual(status, 400)

    def test_reject_invalid_repository(self):
        for url in ("", " ", None, "relative/repo", "github.com/vietdotest/repo", "ftp://example.com/repo", "file:///C:/repo", "https://", "http:repo"):
            with self.subTest(url=url):
                status, _, _ = request(self.base, "/api/submissions", submission(repositoryUrl=url))
                self.assertEqual(status, 400)

    def test_stored_submissions_roundtrip_concurrently(self):
        def roundtrip(index):
            status, headers, created = request(self.base, "/api/submissions",
                submission(assignmentId=1 + index % 2, repositoryUrl="http://example.com/repo/" + str(index)))
            self.assertEqual(status, 201)
            status, _, retrieved = request(self.base, headers["Location"])
            self.assertEqual(status, 200)
            self.assertEqual(retrieved, created)
            return created["id"]
        with ThreadPoolExecutor(max_workers=12) as executor:
            ids = list(executor.map(roundtrip, range(36)))
        self.assertEqual(len(set(ids)), 36)
        status, _, _ = request(self.base, "/api/submissions/00000000-0000-0000-0000-000000000000")
        self.assertEqual(status, 404)
        status, _, _ = request(self.base, "/api/submissions/not-a-guid")
        self.assertEqual(status, 404)

    def test_client_get_assignments_custom_base(self):
        result = run_client(self.base)
        self.assertEqual(result.returncode, 0)
        self.assertIn("HTTP GET /api/assignments -> 200", result.stdout)
        self.assertIn("1: Web API cơ bản", result.stdout)
        self.assertIn("2: HTTP Client", result.stdout)

    def test_client_creates_submission(self):
        result = run_client(self.base)
        self.assertEqual(result.returncode, 0)
        self.assertIn("HTTP POST /api/submissions -> 201", result.stdout)
        self.assertIn("MSSV: DTC245200604", result.stdout)
        self.assertIn("Kho mã: " + REPOSITORY, result.stdout)
        self.assertIn("Trạng thái: received", result.stdout)
        match = re.search(r"ID: ([0-9a-f-]{36})", result.stdout)
        self.assertIsNotNone(match)
        status, _, body = request(self.base, "/api/submissions/" + match.group(1))
        self.assertEqual(status, 200)
        self.assertEqual(body["studentCode"], "DTC245200604")
        self.assertEqual(body["repositoryUrl"], REPOSITORY)
        self.assertIn("UTC: ", result.stdout)

    def test_client_reads_back_created_resource(self):
        result = run_client(self.base + "/")
        self.assertEqual(result.returncode, 0)
        match = re.search(r"ID: ([0-9a-f-]{36})", result.stdout)
        self.assertIsNotNone(match)
        self.assertIn("HTTP GET /api/submissions/" + match.group(1) + " -> 200", result.stdout)
        self.assertIn("Đối chiếu dữ liệu: khớp", result.stdout)
        self.assertLessEqual(len(result.stdout.splitlines()), 20)

    def test_client_reports_invalid_base_url(self):
        for base in ("not-an-absolute-url", "ftp://example.com", ""):
            result = run_client(base)
            self.assertEqual(result.returncode, 1)
            self.assertIn("Lỗi: URL API phải là địa chỉ HTTP/HTTPS tuyệt đối.", result.stderr)
            self.assertNotIn("Unhandled exception", result.stderr)

    def test_client_reports_connection_error(self):
        result = run_client(f"http://127.0.0.1:{free_port()}")
        self.assertEqual(result.returncode, 1)
        self.assertIn("Lỗi HTTP:", result.stderr)
        self.assertNotIn("Unhandled exception", result.stderr)
        self.assertNotIn("Đối chiếu dữ liệu: khớp", result.stdout)

    def test_client_uses_default_base_url(self):
        with socket.socket() as probe:
            try:
                probe.bind(("127.0.0.1", 5000))
            except OSError:
                self.fail("Cổng 5000 đang được dùng; không dừng tiến trình không thuộc kiểm thử.")
        with running_api(5000) as base:
            result = run_client()
            self.assertEqual(result.returncode, 0)
            self.assertIn("API: http://localhost:5000/", result.stdout)
            self.assertIn("HTTP POST /api/submissions -> 201", result.stdout)
            self.assertIn("Đối chiếu dữ liệu: khớp", result.stdout)
            self.assertLessEqual(len(result.stdout.splitlines()), 20)

    def test_requests_file_runs_six_requests(self):
        path = ROOT / "requests.http"
        self.assertTrue(path.exists(), "Thiếu requests.http")
        text = path.read_text(encoding="utf-8")
        self.assertIn("# @name createSubmission", text)
        self.assertIn("{{createSubmission.response.body.$.id}}", text)
        blocks = re.split(r"^###.*$", text, flags=re.MULTILINE)
        requests = [block for block in blocks if re.search(r"^(GET|POST) ", block, re.MULTILINE)]
        self.assertEqual(len(requests), 6)
        created_id = None
        for block, expected in zip(requests, (200, 200, 201, 400, 200, 404)):
            method, endpoint = re.search(r"^(GET|POST) (\S+)", block, re.MULTILINE).groups()
            endpoint = endpoint.replace("{{baseUrl}}", "")
            if created_id:
                endpoint = endpoint.replace("{{createSubmission.response.body.$.id}}", created_id)
            data = json.loads(block.split("\n\n", 1)[1].strip()) if method == "POST" else None
            status, _, body = request(self.base, endpoint, data)
            self.assertEqual(status, expected)
            if expected == 201:
                created_id = body["id"]


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--label", default="latest")
    parser.add_argument("--test", action="append")
    options = parser.parse_args()
    EVIDENCE.mkdir(parents=True, exist_ok=True)
    output = io.StringIO()
    with contextlib.redirect_stdout(output), contextlib.redirect_stderr(output):
        build = subprocess.run(["dotnet", "build", str(ROOT / "EduSubmit.slnx"), "--nologo"],
                               cwd=ROOT, env=ENV, capture_output=True, text=True, encoding="utf-8")
        print("COMMAND: dotnet build EduSubmit.slnx --nologo")
        print(build.stdout, build.stderr)
        print(f"BUILD_EXIT={build.returncode}")
        if build.returncode == 0:
            names = [f"EduSubmitTests.{name}" for name in options.test] if options.test else ["EduSubmitTests"]
            suite = unittest.defaultTestLoader.loadTestsFromNames(names, sys.modules[__name__])
            result = unittest.TextTestRunner(stream=output, verbosity=2).run(suite)
            success = result.wasSuccessful()
        else:
            success = False
        print(f"RESULT={'PASS' if success else 'FAIL'}")
    text = output.getvalue()
    (EVIDENCE / f"{options.label}.txt").write_text(text, encoding="utf-8")
    sys.stdout.buffer.write(text.encode("utf-8"))
    sys.exit(0 if success else 1)
