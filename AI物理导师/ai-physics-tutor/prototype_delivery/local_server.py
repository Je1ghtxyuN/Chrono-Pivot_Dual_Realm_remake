"""Loopback-only HTTP tutor using the real local 3B model and physics LoRA."""
import argparse
import json
import threading
import time
import traceback
from pathlib import Path
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from api_contract import validate_request, public_response
from retrieval import KnowledgeIndex
from routes import ModeTutor, direct_response

P = Path(__file__).resolve().parent

class Handler(BaseHTTPRequestHandler):
    def send_json(self, status, data):
        body = json.dumps(data, ensure_ascii=False).encode('utf-8')
        self.send_response(status)
        self.send_header('Content-Type', 'application/json; charset=utf-8')
        self.send_header('Content-Length', str(len(body)))
        self.end_headers()
        self.wfile.write(body)

    def do_GET(self):
        if self.path == '/health':
            return self.send_json(200, {'ready': True, **self.server.model_info})
        self.send_json(404, {'error': 'unknown endpoint'})

    def do_POST(self):
        if self.path != '/api/tutor':
            return self.send_json(404, {'error': 'unknown endpoint'})
        try:
            length = int(self.headers.get('Content-Length', '0'))
            if not 1 <= length <= 64000:
                raise ValueError('invalid request length')
            row = validate_request(json.loads(self.rfile.read(length)))
        except (ValueError, TypeError):
            return self.send_json(400, {'error': '请求格式错误或超出输入限制'})
        if not self.server.model_lock.acquire(blocking=False):
            return self.send_json(429, {'error': '导师正在处理上一请求，请稍后再试'})
        record = {'timestamp_unix': time.time(), 'request': row}
        try:
            private = (direct_response(self.server.backend, self.server.index, row, 42)
                if row['mode'] == 'teach' else self.server.tutor.respond(row, seed=42))
            public = public_response(private, row['mode'])
            public['components'] = {'model': 'Qwen2.5-3B-Instruct + physics LoRA',
                'quantization': 'NF4 4-bit double quant', 'adapter_used': True}
            record.update(private_model_result=private, public_response=public)
            self.send_json(200 if public['ok'] else 502, public)
        except ValueError as error:
            record['error'] = traceback.format_exc()
            self.send_json(400, {'error': str(error)})
        except Exception:
            record['error'] = traceback.format_exc()
            self.send_json(500, {'error': '模型运行失败，详见本机服务日志'})
        finally:
            with self.server.log_path.open('a', encoding='utf-8') as stream:
                stream.write(json.dumps(record, ensure_ascii=False)+'\n')
            self.server.model_lock.release()


def create_server(backend, config, log_dir, port=18080):
    log_dir = Path(log_dir)
    log_dir.mkdir(parents=True, exist_ok=True)
    server = ThreadingHTTPServer(('127.0.0.1', port), Handler)
    server.backend = backend
    server.index = KnowledgeIndex(P/'data/knowledge.json')
    server.tutor = ModeTutor(backend, server.index, False, True)
    server.model_lock = threading.Lock()
    server.model_info = backend.info() if hasattr(backend, 'info') else {}
    server.log_path = log_dir/'requests.jsonl'
    return server


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--config', default=str(P/'config.json'))
    parser.add_argument('--port', type=int, default=18080)
    args = parser.parse_args()
    config = json.loads(Path(args.config).read_text(encoding='utf-8'))
    root = P.parent
    model_path = Path(config['model_path'])
    adapter_path = root/config['training_artifact']
    from local_backend import LocalBackend
    backend = LocalBackend(model_path, adapter_path,
        config.get('max_input_tokens', 4096), config.get('max_new_tokens', 512))
    log_dir = P/'results'/('local_4bit_'+time.strftime('%Y%m%d_%H%M%S'))
    server = create_server(backend, config, log_dir, args.port)
    (log_dir/'startup.json').write_text(json.dumps({'config': config, **backend.info()},
        ensure_ascii=False, indent=2), encoding='utf-8')
    print('READY http://127.0.0.1:'+str(args.port)+'/api/tutor LOG_DIR '+str(log_dir), flush=True)
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        pass
    finally:
        server.server_close()

if __name__ == '__main__':
    main()
