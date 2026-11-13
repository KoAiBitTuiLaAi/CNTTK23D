"""Integration tests: stdlib only; real dotnet build, API and console client.

Run from catalogapi: python -m unittest discover -s tests -v
Set CATALOG_EVIDENCE_DIR to keep build/server/client logs outside the repo.
Every test owns its server, uses a fresh port and stops it in finally/cleanup.
"""
import contextlib
import json
import os
from pathlib import Path
import socket
import subprocess
import tempfile
import time
import unittest
from urllib.error import HTTPError, URLError
from concurrent.futures import ThreadPoolExecutor
from datetime import datetime, timezone
from urllib.request import Request, urlopen

ROOT = Path(__file__).resolve().parents[1]
LOGS = Path(os.environ.get("CATALOG_EVIDENCE_DIR", str(Path(tempfile.gettempdir()) / "catalog-evidence")))
API = ROOT / "src/Catalog.Api/bin/Debug/net10.0/Catalog.Api.dll"
CLIENT = ROOT / "src/Catalog.Client/bin/Debug/net10.0/Catalog.Client.dll"


def fresh_port():
    with socket.socket() as sock:
        sock.bind(("127.0.0.1", 0))
        return sock.getsockname()[1]


def request(base, path="/api/products", method="GET", payload=None, content_type="application/json"):
    data = None if payload is None else json.dumps(payload).encode("utf-8")
    req = Request(base + path, data=data, method=method)
    if data is not None:
        req.add_header("Content-Type", content_type)
    try:
        response = urlopen(req, timeout=5)
    except HTTPError as error:
        response = error
    with response:
        body = response.read()
        return response.status, dict(response.headers), body


def decode(response):
    return json.loads(response[2])


@contextlib.contextmanager
def server(label, port=None):
    port = port or fresh_port()
    base = f"http://127.0.0.1:{port}"
    LOGS.mkdir(parents=True, exist_ok=True)
    with (LOGS / f"{label}-server.log").open("w", encoding="utf-8") as output:
        process = subprocess.Popen(["dotnet", str(API), "--urls", base], cwd=ROOT,
                                   stdout=output, stderr=subprocess.STDOUT)
        try:
            deadline = time.monotonic() + 15
            while time.monotonic() < deadline:
                if process.poll() is not None:
                    raise RuntimeError(f"API exited: {LOGS / (label + '-server.log')}")
                try:
                    if request(base, "/")[0] == 200:
                        break
                except (URLError, OSError):
                    time.sleep(0.05)
            else:
                raise TimeoutError("API did not become ready")
            yield base
        finally:
            process.terminate()
            try:
                process.wait(timeout=5)
            except subprocess.TimeoutExpired:
                process.kill()
                process.wait(timeout=5)
            with (LOGS / "server-lifecycle.jsonl").open("a", encoding="utf-8") as lifecycle:
                lifecycle.write(json.dumps({"label": label, "pid": process.pid, "port": port,
                                            "returncode": process.returncode, "stopped": process.poll() is not None}) + "\n")


def client(base=None, label="client"):
    command = ["dotnet", str(CLIENT)]
    if base is not None:
        command.append(base)
    result = subprocess.run(command, cwd=ROOT, capture_output=True, text=True,
                            encoding="utf-8", errors="replace", timeout=20)
    LOGS.mkdir(parents=True, exist_ok=True)
    (LOGS / f"{label}.log").write_text(
        f"COMMAND: {command}\nEXIT: {result.returncode}\n{result.stdout}{result.stderr}", encoding="utf-8")
    return result


class CatalogTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        LOGS.mkdir(parents=True, exist_ok=True)
        build = subprocess.run(["dotnet", "build", "catalogapi.slnx", "--nologo"], cwd=ROOT,
                               capture_output=True, text=True, encoding="utf-8", errors="replace", timeout=90)
        (LOGS / "build.log").write_text(build.stdout + build.stderr, encoding="utf-8")
        if build.returncode:
            raise RuntimeError(build.stdout + build.stderr)

    def test_original_eight_http_cases(self):
        with server(self._testMethodName) as base:
            root = request(base, "/")
            self.assertEqual(200, root[0])
            self.assertEqual({"service": "Catalog.Api", "status": "running"}, decode(root))
            self.assertEqual(3, len(decode(request(base))))
            self.assertEqual(1, decode(request(base, "/api/products/1"))["id"])
            missing = request(base, "/api/products/999")
            self.assertEqual(404, missing[0])
            self.assertEqual("PRODUCT_NOT_FOUND", decode(missing)["code"])
            unrouted = request(base, "/api/products/abc")
            self.assertEqual((404, b""), (unrouted[0], unrouted[2]))
            created = request(base, method="POST", payload={"name": "Lap trinh gRPC", "description": "Tai lieu bai 5", "price": 80000})
            self.assertEqual(201, created[0])
            self.assertEqual("/api/products/4", created[1]["Location"])
            self.assertEqual(decode(created), decode(request(base, created[1]["Location"])))
            invalid = request(base, method="POST", payload={"name": "San pham loi", "price": -1})
            self.assertEqual(400, invalid[0])
            self.assertEqual("INVALID_PRODUCT", decode(invalid)["code"])
            unsupported = request(base, method="POST", payload={"name": "a", "price": 1}, content_type="text/plain")
            self.assertEqual(415, unsupported[0])
            not_allowed = request(base, "/api/products/1", "PUT")
            self.assertEqual(405, not_allowed[0])
            self.assertEqual({"GET", "DELETE"}, {part.strip() for part in not_allowed[1]["Allow"].split(",")})

    def test_concurrent_creates_and_reads(self):
        with server(self._testMethodName) as base:
            def create(index):
                response = request(base, method="POST", payload={"name": f"Concurrent {index}", "price": 1})
                self.assertEqual(201, response[0])
                self.assertEqual(200, request(base)[0])
                return decode(response)["id"]
            with ThreadPoolExecutor(max_workers=8) as pool:
                identifiers = list(pool.map(create, range(24)))
            self.assertEqual(list(range(4, 28)), sorted(identifiers))
            self.assertEqual(27, len(decode(request(base))))

    def test_trim_and_nullable_description(self):
        with server(self._testMethodName) as base:
            for description in (None, "  text  "):
                response = request(base, method="POST", payload={"name": "  Trim me  ", "description": description, "price": 1})
                self.assertEqual(201, response[0])
                self.assertEqual("Trim me", decode(response)["name"])
                self.assertEqual("" if description is None else "text", decode(response)["description"])

    def test_four_experiments(self):
        predictions = LOGS / "experiment-predictions.jsonl"
        results = []

        def predict(number, expectation):
            with predictions.open("a", encoding="utf-8") as output:
                output.write(json.dumps({"experiment": number, "before_utc": datetime.now(timezone.utc).isoformat(),
                                         "prediction": expectation}, ensure_ascii=False) + "\n")

        predict(1, "Client twice on one server: IDs 4 then 5; list count 5; both exit 0.")
        port = fresh_port()
        with server("experiment-1", port) as base:
            first = client(base, "experiment-1-first")
            second = client(base, "experiment-1-second")
            self.assertEqual((0, 0), (first.returncode, second.returncode))
            self.assertIn("Da tao ID: 4", first.stdout)
            self.assertIn("Da tao ID: 5", second.stdout)
            self.assertEqual(5, len(decode(request(base))))
            results.append({"experiment": 1, "ids": [4, 5], "list_count": 5, "exit_codes": [first.returncode, second.returncode]})
            predict(2, "After stopping server: connection failure, no HTTP response, Client exit 2, no crash.")
        stopped = client(base, "experiment-2-stopped")
        self.assertEqual(2, stopped.returncode)
        self.assertIn("Khong ket noi duoc API", stopped.stderr)
        results.append({"experiment": 2, "exit_code": stopped.returncode, "http_response": False})

        predict(3, "Restart same address: list back to 3 seeds; GET previously created ID 4 => 404 PRODUCT_NOT_FOUND.")
        with server("experiment-3-restarted", port) as base:
            products = decode(request(base))
            lost = request(base, "/api/products/4")
            self.assertEqual([1, 2, 3], [item["id"] for item in products])
            self.assertEqual(404, lost[0])
            self.assertEqual("PRODUCT_NOT_FOUND", decode(lost)["code"])
            results.append({"experiment": 3, "list_count": len(products), "lost_id_status": lost[0]})

            predict(4, "Server running but Client points to unused wrong port: connection failure, exit 2; correct server still 200.")
            wrong = client(f"http://127.0.0.1:{fresh_port()}", "experiment-4-wrong-port")
            self.assertEqual(2, wrong.returncode)
            self.assertIn("Khong ket noi duoc API", wrong.stderr)
            self.assertEqual(200, request(base, "/")[0])
            results.append({"experiment": 4, "exit_code": wrong.returncode, "correct_server_status": 200})
        (LOGS / "experiment-results.json").write_text(json.dumps(results, indent=2), encoding="utf-8")

    def test_name_length_validation_order_and_boundary(self):
        with server(self._testMethodName) as base:
            invalids = [(None, 1), ("", 1), ("   ", 1), ("x" * 101, -1), ("x" * 101, 0)]
            for name, price in invalids:
                response = request(base, method="POST", payload={"name": name, "price": price})
                self.assertEqual(400, response[0])
                self.assertEqual("INVALID_PRODUCT", decode(response)["code"])
            for name in ("x" * 101, " " + "x" * 100):
                response = request(base, method="POST", payload={"name": name, "price": 1})
                self.assertEqual(400, response[0])
                self.assertEqual("NAME_TOO_LONG", decode(response)["code"])
            self.assertEqual(3, len(decode(request(base))))
            response = request(base, method="POST", payload={"name": "x" * 100, "price": 1})
            self.assertEqual(201, response[0])
            self.assertEqual(4, decode(response)["id"])

    def test_delete_then_read_and_delete_missing(self):
        with server(self._testMethodName) as base:
            created = request(base, method="POST", payload={"name": "Delete me", "price": 1})
            self.assertEqual(201, created[0])
            path = f"/api/products/{decode(created)['id']}"
            deleted = request(base, path, "DELETE")
            self.assertEqual(204, deleted[0])
            self.assertEqual(b"", deleted[2])
            for method in ("GET", "DELETE"):
                missing = request(base, path, method)
                self.assertEqual(404, missing[0])
                self.assertEqual("PRODUCT_NOT_FOUND", decode(missing)["code"])
            self.assertEqual(3, len(decode(request(base))))
            self.assertEqual(404, request(base, "/api/products/abc", "DELETE")[0])

    def test_delete_all_then_create(self):
        with server(self._testMethodName) as base:
            for identifier in (1, 2, 3):
                self.assertEqual(204, request(base, f"/api/products/{identifier}", "DELETE")[0])
            self.assertEqual([], decode(request(base)))
            created = request(base, method="POST", payload={"name": "After empty", "price": 1})
            self.assertEqual(201, created[0])
            self.assertEqual(4, decode(created)["id"])

    def test_keyword_case_insensitive(self):
        with server(self._testMethodName) as base:
            for keyword in ("REST", "rest", "ReSt"):
                response = request(base, f"/api/products?keyword={keyword}")
                self.assertEqual(200, response[0])
                self.assertEqual([2], [item["id"] for item in decode(response)])
            self.assertEqual([], decode(request(base, "/api/products?keyword=no-match")))
            self.assertEqual(3, len(decode(request(base, "/api/products?keyword="))))

    def test_client_http_errors_are_safe(self):
        source = ROOT / "src/Catalog.Api/Program.cs"
        original = source.read_bytes()
        cases = [
            ("empty", 'Results.StatusCode(404)', 404, "Body rong"),
            ("text", 'Results.Text("unsupported", statusCode: 415)', 415, "unsupported"),
            ("json", 'Results.NotFound(new ApiError("TEST_ERROR", "Loi thu nghiem"))', 404, "TEST_ERROR"),
            ("malformed", 'Results.Text("{broken", "application/json", statusCode: 400)', 400, "{broken"),
            ("success-empty", 'Results.StatusCode(200)', 200, "Body JSON rong"),
            ("success-malformed", 'Results.Text("{broken", "application/json", statusCode: 200)', 200, "Loi Client"),
        ]
        try:
            for label, response, status, expected in cases:
                with self.subTest(label=label):
                    text = original.decode("utf-8-sig")
                    self.assertEqual(1, text.count("Results.Ok(matches.ToArray())"))
                    text = text.replace("Results.Ok(matches.ToArray())", response)
                    source.write_text(text, encoding="utf-8")
                    self.setUpClass()
                    with server(f"http-error-{label}") as base:
                        result = client(base, f"http-error-{label}")
                        self.assertEqual(1, result.returncode, result.stdout + result.stderr)
                        self.assertIn(f"[1] GET /api/products: {status}", result.stdout)
                        self.assertIn(expected, result.stdout + result.stderr)
                        self.assertNotIn("Unhandled exception", result.stderr)
        finally:
            source.write_bytes(original)
            self.setUpClass()

    def test_client_invalid_address(self):
        result = client("not a valid URL", self._testMethodName)
        self.assertEqual(1, result.returncode, result.stdout + result.stderr)
        self.assertIn("Loi Client", result.stderr)
        self.assertNotIn("Unhandled exception", result.stderr)

    def test_client_network_failure(self):
        result = client(f"http://127.0.0.1:{fresh_port()}", self._testMethodName)
        self.assertEqual(2, result.returncode, result.stdout + result.stderr)
        self.assertIn("Khong ket noi duoc API", result.stderr)
        self.assertNotIn("Unhandled exception", result.stderr)

    def test_client_four_steps(self):
        with server(self._testMethodName) as base:
            result = client(base, self._testMethodName)
            self.assertEqual(0, result.returncode, result.stdout + result.stderr)
            output = result.stdout
            markers = ["[1] GET /api/products: 200", "[2] POST /api/products: 201",
                       "[3] GET /api/products/4: 200", "[4] POST /api/products: 400"]
            for marker in markers:
                self.assertIn(marker, output)
            positions = [output.index(marker) for marker in markers]
            self.assertEqual(sorted(positions), positions)
            self.assertIn("INVALID_PRODUCT", output)
            products = decode(request(base))
            self.assertEqual(4, len(products))
            self.assertEqual({"id": 4, "name": "Nhap mon REST API", "description": "Sach tham khao", "price": 120000}, products[-1])


if __name__ == "__main__":
    unittest.main(verbosity=2)
