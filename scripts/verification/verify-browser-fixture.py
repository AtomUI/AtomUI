#!/usr/bin/env python3
"""Execute the published registration fixture in Chrome and verify its runtime assertion."""
import argparse
import functools
from html.parser import HTMLParser
import http.server
import json
import os
from pathlib import Path
import platform
import secrets
import shutil
import signal
import subprocess
import sys
import tempfile
import threading
import time

MARKER = "PRODUCT_PASS browser"
REQUIRED_PACKAGES = {"AtomUI.Controls.Common", "AtomUI.Desktop.Controls", "Fixture.ThirdParty"}
OBSERVER = """<script>(() => {
    let previous = '';
    const report = (error = '') => {
        const result = document.querySelector('pre#result');
        if (!error && (!result || !(result.textContent.includes('PRODUCT_PASS browser') ||
            result.textContent.includes('PRODUCT_FAIL') || result.textContent.includes('ERROR ') ||
            result.dataset.done === 'error'))) return;
        const payload = JSON.stringify({dom: document.documentElement.outerHTML, error,
            userAgent: navigator.userAgent});
        if (payload === previous) return;
        previous = payload;
        fetch(__ENDPOINT__, {method: 'POST', headers: {'Content-Type': 'application/json'},
            body: payload}).catch(() => {});
    };
    new MutationObserver(() => report()).observe(document.documentElement,
        {subtree: true, childList: true, characterData: true, attributes: true});
    addEventListener('error', event => report(String(event.message || 'Browser error')));
    addEventListener('unhandledrejection', event => report(String(event.reason)));
    report();
})();</script>"""


class ResultParser(HTMLParser):
    def __init__(self):
        super().__init__(convert_charrefs=True)
        self.parts = []
        self.inside = False
        self.count = 0
        self.failed = False

    def handle_starttag(self, tag, attrs):
        attributes = dict(attrs)
        if tag == "pre" and attributes.get("id") == "result":
            self.count += 1
            self.inside = True
            self.failed |= attributes.get("data-done") == "error"

    def handle_endtag(self, tag):
        if tag == "pre":
            self.inside = False

    def handle_data(self, data):
        if self.inside:
            self.parts.append(data)


def verify_dom(dom):
    result = ResultParser()
    result.feed(dom)
    lines = [line.strip() for line in "".join(result.parts).splitlines()]
    if result.count != 1 or result.failed:
        raise ValueError("The fixture result element is missing, duplicated, or reports an error.")
    if any("PRODUCT_FAIL" in line or line.startswith("ERROR ") for line in lines):
        raise ValueError("The published fixture reported a runtime failure.")
    if lines.count(MARKER) != 1:
        raise ValueError("The page loaded without the required actual fixture runtime assertion.")
    return lines


def verify_receipt(path):
    receipt = json.loads(path.read_text(encoding="utf-8"))
    if not isinstance(receipt, dict) or receipt.get("status") != "output-verified" or type(receipt.get("abi")) is not int or receipt.get("abi") != 1:
        raise ValueError("The TypeMap receipt is not an output-verified ABI 1 receipt.")
    accessors = receipt.get("accessors")
    if not isinstance(accessors, list):
        raise ValueError("The TypeMap receipt has no verified accessor list.")
    packages = {entry.get("packageId") for entry in accessors if isinstance(entry, dict)}
    if not REQUIRED_PACKAGES.issubset(packages):
        raise ValueError("The fixture's Common, Desktop, and third-party registrations were not all linked.")
    return receipt


def chrome_path(explicit):
    if explicit:
        candidate = Path(explicit).expanduser().resolve()
        if not candidate.is_file():
            raise ValueError("The specified Chrome executable does not exist.")
        return str(candidate)
    for name in ("google-chrome", "google-chrome-stable", "chromium", "chromium-browser"):
        if found := shutil.which(name):
            return found
    candidate = Path("/Applications/Google Chrome.app/Contents/MacOS/Google Chrome")
    if candidate.is_file():
        return str(candidate)
    raise ValueError("A Chrome/Chromium executable is required; pass --chrome.")


def stop_chrome(process):
    if os.name == "nt":
        subprocess.run(["taskkill", "/PID", str(process.pid), "/T", "/F"], capture_output=True, check=False)
    else:
        try:
            os.killpg(process.pid, signal.SIGTERM)
        except ProcessLookupError:
            pass
    try:
        process.wait(timeout=5)
    except subprocess.TimeoutExpired:
        process.kill()
    finally:
        if os.name != "nt":
            try:
                os.killpg(process.pid, signal.SIGKILL)
            except ProcessLookupError:
                pass
        process.wait(timeout=5)


def run_chrome(command, timeout, completed, trace):
    with trace.open("w", encoding="utf-8") as log:
        process = subprocess.Popen(command, stdout=log, stderr=subprocess.STDOUT,
                                   start_new_session=os.name != "nt")
        try:
            deadline = time.monotonic() + timeout
            while not completed.wait(0.1):
                if process.poll() is not None:
                    raise RuntimeError("Chrome exited before the actual fixture assertion.")
                if time.monotonic() >= deadline:
                    raise RuntimeError("The actual fixture did not finish within the bounded timeout.")
            if process.poll() not in (None, 0):
                raise RuntimeError(f"Chrome exited with code {process.returncode}.")
        finally:
            stop_chrome(process)
    return process.returncode


def execute(args, report):
    if not (args.root / "index.html").is_file():
        raise ValueError("--root must be the published fixture's wwwroot containing index.html.")
    receipt = verify_receipt(args.receipt)
    (args.output / "receipt.json").write_text(json.dumps(receipt, indent=2), encoding="utf-8")
    executable = chrome_path(args.chrome)
    completed = threading.Event()
    state = {}
    gate = threading.Lock()
    endpoint = "/__fixture_result_" + secrets.token_urlsafe(18)
    observer = OBSERVER.replace("__ENDPOINT__", json.dumps(endpoint))

    class Handler(http.server.SimpleHTTPRequestHandler):
        def do_GET(self):
            if self.path in ("/", "/index.html"):
                page = (args.root / "index.html").read_text(encoding="utf-8")
                # Observe the real DOM; never manufacture a product assertion. Waiting for
                # completion avoids dump-dom/virtual time ending before async WASM startup.
                page = page.replace("</body>", observer + "</body>") if "</body>" in page else page + observer
                data = page.encode("utf-8")
                self.send_response(200)
                self.send_header("Content-Type", "text/html; charset=utf-8")
                self.send_header("Content-Length", str(len(data)))
                self.end_headers()
                self.wfile.write(data)
            else:
                super().do_GET()

        def do_POST(self):
            if self.path != endpoint or self.headers.get("Origin") != f"http://127.0.0.1:{self.server.server_port}":
                self.send_error(404)
                return
            try:
                length = int(self.headers.get("Content-Length", "0"))
                if not 0 < length <= 2 * 1024 * 1024:
                    raise ValueError("Invalid result length.")
                payload = json.loads(self.rfile.read(length))
                if not isinstance(payload, dict) or not all(isinstance(payload.get(key), str) for key in ("dom", "error", "userAgent")):
                    raise ValueError("Invalid browser result.")
            except (ValueError, UnicodeError):
                self.send_error(400)
                return
            with gate:
                state["dom"] = payload["dom"]
                state["userAgent"] = payload["userAgent"]
                if payload["error"]:
                    state["error"] = payload["error"]
            self.send_response(204)
            self.end_headers()
            completed.set()

        def end_headers(self):
            self.send_header("Cross-Origin-Opener-Policy", "same-origin")
            self.send_header("Cross-Origin-Embedder-Policy", "require-corp")
            self.send_header("Cache-Control", "no-store")
            super().end_headers()

        def log_message(self, *_):
            pass

    handler = functools.partial(Handler, directory=str(args.root.resolve()))
    server = http.server.ThreadingHTTPServer(("127.0.0.1", 0), handler)
    worker = threading.Thread(target=server.serve_forever, daemon=True)
    worker.start()
    try:
        with tempfile.TemporaryDirectory(prefix="atomui-browser-profile-") as profile:
            url = f"http://127.0.0.1:{server.server_port}/"
            command = [executable, "--headless", "--no-first-run", "--no-default-browser-check",
                       "--disable-background-networking", "--use-gl=angle", "--use-angle=swiftshader",
                       "--enable-unsafe-swiftshader", f"--user-data-dir={profile}", url]
            if sys.platform.startswith("linux"):
                command[1:1] = ["--no-sandbox", "--disable-dev-shm-usage"]
            code = run_chrome(command, args.timeout, completed, args.output / "chrome.log")
            with gate:
                result = dict(state)
            (args.output / "dom.html").write_text(result.get("dom", ""), encoding="utf-8")
            report.update({"chrome_exit": code, "controlled_shutdown": True,
                           "renderer": "SwiftShader", "url": url, "completion": "actual DOM assertion",
                           "user_agent": result.get("userAgent")})
            if result.get("error"):
                raise RuntimeError("The browser reported a runtime error: " + result["error"])
            report["assertions"] = verify_dom(result.get("dom", ""))
    finally:
        server.shutdown()
        server.server_close()
        worker.join(timeout=5)
    report["status"] = "passed"


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, required=True)
    parser.add_argument("--receipt", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--mode", choices=("interpreted", "aot"), required=True)
    parser.add_argument("--chrome")
    parser.add_argument("--timeout", type=int, default=180)
    args = parser.parse_args(argv)
    if not 1 <= args.timeout <= 600:
        parser.error("--timeout must be between 1 and 600 seconds")
    args.output.mkdir(parents=True, exist_ok=False)
    report = {"status": "failed", "mode": args.mode, "source_revision": os.environ.get("GITHUB_SHA"),
              "root": str(args.root.resolve()), "runtime_only": True,
              "host": {"system": platform.system(), "release": platform.release(), "architecture": platform.machine()}}
    started = time.monotonic()
    try:
        execute(args, report)
    except (OSError, ValueError, RuntimeError) as error:
        report["error"] = str(error)
        print(f"BROWSER_FIXTURE_FAIL: {error}", file=sys.stderr)
    finally:
        report["seconds"] = round(time.monotonic() - started, 3)
        (args.output / "result.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    if report["status"] != "passed":
        return 1
    print(f"BROWSER_FIXTURE_PASS {args.mode}: {MARKER}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
