"""Local transport fixture for PhysicsTutorPlayCheck; never serves model answers."""
import json
import sys
import threading
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'AI物理导师/ai-physics-tutor/prototype_delivery'
if not SOURCE.is_dir():
    SOURCE = ROOT.parent / 'AI物理导师/ai-physics-tutor/prototype_delivery'
sys.path.insert(0, str(SOURCE))
from api_contract import validate_request

calls = []
class Handler(BaseHTTPRequestHandler):
    def do_POST(self):
        try:
            assert self.path == '/api/tutor'
            row = validate_request(json.loads(self.rfile.read(int(self.headers['Content-Length']))))
            assert 'task' in row['student_state'] and 'observation' in row['student_state']
            assert row['student_state']['task'].strip()
            assert '读数' in row['student_state']['observation']
            assert len(row['history']) <= 20
            if len(calls) == 0:
                assert row['question'] == '为什么船装入配重后下沉？'
                assert row['history'] == []
            elif len(calls) == 1:
                assert row['history'][0]['content'] == '为什么船装入配重后下沉？'
                assert row['history'][1]['role'] == 'assistant'
            elif len(calls) == 2:
                assert row['history'] == []
            if row['question'] == '历史边界 10':
                assert len(row['history']) == 20
            if row['question'] == '长回答 3':
                assert sum(len(m['content']) for m in row['history']) <= 10000
                assert len(row['history']) < 20
                assert all(len(m['content']) <= 4000 for m in row['history'])
            calls.append(row)
            replies = ['测试回答：浮力', '第二次测试回答', '', '恢复测试回答']
            if row['question'] == '触发错误':
                status, body = 429, {'error': '测试服务忙'}
            else:
                if len(calls) <= 4:
                    reply = replies[len(calls)-1]
                    if len(calls) == 1:
                        reply += '\n' + ('这段仅用于验证长回答滚动显示，不代表模型推理。\n' * 15)
                else:
                    reply = '历史测试回答' if row['question'].startswith('历史边界') else '长回答测试：' + '测' * 6000
                status, body = 200, {'ok': True, 'answer': {'status': 'hint', 'reply': reply, 'value': None, 'unit': '', 'evidence_ids': ['test-card']}, 'evidence': [{'id': 'test-card', 'title': 'OpenStax 浮力', 'source_url': 'https://openstax.org/', 'source_section': '11.7', 'source_revision': 'test', 'review_status': 'test fixture', 'text': 'test', 'misconceptions': '', 'attribution': 'OpenStax', 'license': 'CC BY 4.0'}], 'response_seconds': 0.1, 'error': None, 'review_status': 'test fixture', 'scope': 'transport test only'}
        except Exception as ex:
            status, body = 400, {'error': str(ex) or 'contract assertion failed'}
            print('FIXTURE_FAIL', repr(ex), flush=True)
        raw = json.dumps(body, ensure_ascii=False).encode('utf-8')
        self.send_response(status)
        self.send_header('Content-Type', 'application/json; charset=utf-8')
        self.send_header('Content-Length', str(len(raw)))
        self.end_headers()
        self.wfile.write(raw)
        Path(__file__).with_name('tutor-fixture-calls.json').write_text(json.dumps(calls, ensure_ascii=False, indent=2), encoding='utf-8')
        print('FIXTURE_HTTP', status, 'calls', len(calls), flush=True)
        if len(calls) == 19:
            threading.Thread(target=self.server.shutdown, daemon=True).start()

if __name__ == '__main__':
    print('Transport test fixture 127.0.0.1:18081', flush=True)
    ThreadingHTTPServer(('127.0.0.1', 18081), Handler).serve_forever()
