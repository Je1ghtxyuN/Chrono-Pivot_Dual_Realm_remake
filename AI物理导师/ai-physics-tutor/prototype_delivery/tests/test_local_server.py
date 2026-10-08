"""Real HTTP behavior; real GPU generation is checked separately."""
import importlib.util
import json
import sys
import tempfile
import threading
import unittest
import urllib.error
import urllib.request
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parents[1]))
local_server = None
if importlib.util.find_spec('local_server'):
    import local_server

class FailingBackend:
    def generate(self, *args, **kwargs):
        raise RuntimeError('GPU generation failed')

class LocalServerTests(unittest.TestCase):
    def setUp(self):
        self.assertIsNotNone(local_server, 'Local 3B HTTP backend is not implemented')
        self.temp = tempfile.TemporaryDirectory()
        self.server = local_server.create_server(FailingBackend(), {}, Path(self.temp.name), port=0)
        self.worker = threading.Thread(target=self.server.serve_forever, daemon=True)
        self.worker.start()
        self.url = 'http://127.0.0.1:' + str(self.server.server_port)

    def tearDown(self):
        if hasattr(self, 'server'):
            self.server.shutdown()
            self.server.server_close()
            self.worker.join()
            self.temp.cleanup()

    def post(self, body, route='/api/tutor'):
        try:
            response = urllib.request.urlopen(urllib.request.Request(self.url + route, body, {'Content-Type':'application/json'}), timeout=3)
        except urllib.error.HTTPError as error:
            response = error
        with response:
            return response.code, json.loads(response.read())

    def test_invalid_input_is_rejected_before_generation(self):
        for body in (b'not-json', b'{}', b'{"question":"x","hint_level":true}'):
            code, reply = self.post(body)
            self.assertEqual(code, 400)
            self.assertIn('error', reply)

    def test_busy_model_does_not_start_another_generation(self):
        self.server.model_lock.acquire()
        try:
            code, reply = self.post(b'{"question":"x","mode":"teach"}')
            self.assertEqual(code, 429)
            self.assertIn('error', reply)
        finally:
            self.server.model_lock.release()

    def test_generation_failure_is_not_reported_as_a_model_answer(self):
        code, reply = self.post(b'{"question":"x","mode":"teach"}')
        self.assertEqual(code, 500)
        self.assertNotIn('answer', reply)
        code, _ = self.post(b'{"question":"x","mode":"teach"}')
        self.assertEqual(code, 500, 'Error must release the model lock')
        self.assertIn('GPU generation failed', (Path(self.temp.name)/'requests.jsonl').read_text(encoding='utf-8'))

    def test_unknown_endpoint_does_not_generate(self):
        code, _ = self.post(b'{"question":"x"}', '/unknown')
        self.assertEqual(code, 404)

if __name__ == '__main__':
    unittest.main()
