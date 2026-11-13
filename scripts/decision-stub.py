"""Deterministic stand-in for a Decision (system-one) provider.

Conductor OSS calls this instead of a real provider when started with
CONDUCTOR_AI_DECISION_ENDPOINT pointing here (see docker-compose-oss.yaml), so
decision-backed SWITCH tasks run end to end with no credentials and no model.

Rule for a choice question: score each choice by how many of its words (the
key plus the description, matched on a 5-letter prefix) appear in `state`.
The highest score wins; ties go to the first choice. Only choice questions are
supported; any other question type gets HTTP 400, which fails the workflow.

Standard library only. Usage: python decision-stub.py [port]  (default 8090)
"""

import json
import re
import sys
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer

WORD = re.compile(r"[a-z0-9]+")


def stems(text):
    return {w[:5] for w in WORD.findall(str(text).lower()) if len(w) >= 4}


def choose(state, criteria):
    state_stems = stems(state)
    best, best_score = None, -1
    for key, description in criteria.items():
        score = len((stems(key) | stems(description)) & state_stems)
        if score > best_score:
            best, best_score = key, score
    return best


def answer(state, question):
    return {
        "type": "choice",
        "choice": choose(state, question.get("criteria") or {}),
        "confidence": 1.0,
    }


class Handler(BaseHTTPRequestHandler):
    # Conductor's HTTP client pools connections and never retries, so keep them
    # alive (HTTP/1.1) rather than closing after each HTTP/1.0 response.
    protocol_version = "HTTP/1.1"

    def do_POST(self):
        body = json.loads(self.rfile.read(int(self.headers.get("Content-Length", 0))) or b"{}")
        state = body.get("state")
        if not isinstance(state, str):
            state = json.dumps(state)
        questions = body.get("questions") or {}
        unsupported = [n for n, q in questions.items() if q.get("type") != "choice"]
        if unsupported:
            self.respond(400, {"error": f"decision-stub supports choice questions only: {unsupported}"})
            return
        reply = {
            "id": "decision-stub",
            "model": body.get("model", "stub"),
            "answers": {name: answer(state, q) for name, q in questions.items()},
            "usage": {"input_tokens": 0, "output_tokens": 0, "cost": 0},
        }
        self.respond(200, reply)

    def respond(self, status, payload):
        data = json.dumps(payload).encode()
        self.send_response(status)
        self.send_header("Content-Type", "application/json")
        self.send_header("Content-Length", str(len(data)))
        self.end_headers()
        self.wfile.write(data)

    def do_GET(self):  # health check
        self.send_response(200)
        self.send_header("Content-Length", "0")
        self.end_headers()

    def log_message(self, fmt, *args):
        sys.stderr.write("decision-stub: " + fmt % args + "\n")


if __name__ == "__main__":
    port = int(sys.argv[1]) if len(sys.argv) > 1 else 8090
    ThreadingHTTPServer(("0.0.0.0", port), Handler).serve_forever()
