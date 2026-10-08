"""Real HTTP text smoke test; no canned answers and no fixture backend."""
import json
import re
import time
import urllib.request
from pathlib import Path

P = Path(__file__).resolve().parents[1]
URL = 'http://127.0.0.1:18080'
with urllib.request.urlopen(URL+'/health', timeout=10) as response:
    health = json.load(response)
assert health['ready'] and health['quantization']=='NF4 4-bit double quant'
assert health['quantized_layers'] > 0
assert 'best_adapter' in health['adapter_path']
records = []
history = []
cases = [
    ('buoyancy_hint_1', '为什么船装入配重后会下沉一点？', 1,
        {'task':'观察装载前后浮船的吃水深度','observation':'船仍漂浮，装入配重后吃水加深，水面高度基本不变'}),
    ('buoyancy_followup_hint_2', '我发现船排开的水更多了，这与浮力和重力有什么关系？', 2,
        {'task':'解释浮船的平衡状态','observation':'船仍漂浮且已静止，装载后吃水更深'}),
    ('newton_hint_3', '质量2 kg，水平合力6 N，我应该怎样检查加速度的计算？', 3,
        {'task':'根据合力和质量验证牛顿第二定律','observation':'惯性参考系，质量2 kg，水平合力6 N'})
]
for label, question, level, state in cases:
    if label=='newton_hint_3':
        history=[]
    row = {'question':question,'mode':'teach','hint_level':level,'student_state':state,'history':list(history)}
    started = time.perf_counter()
    request = urllib.request.Request(URL+'/api/tutor', json.dumps(row,ensure_ascii=False).encode('utf-8'), {'Content-Type':'application/json'})
    with urllib.request.urlopen(request, timeout=120) as response:
        status = response.status
        body = json.load(response)
    answer = body.get('answer')
    assert status==200 and body.get('ok'), body
    assert isinstance(answer,dict) and answer.get('reply','').strip(),body
    assert re.search('[\u4e00-\u9fff]',answer['reply']),answer
    assert answer.get('value') is None and answer.get('unit')=='',answer
    assert all(citation in {c['id'] for c in body['evidence']} for citation in answer['evidence_ids']),answer
    assert body['components']['adapter_used'] and body['components']['quantization']=='NF4 4-bit double quant'
    elapsed=time.perf_counter()-started
    records.append({'case':label,'request':row,'http_status':status,'response':body,'elapsed_seconds':elapsed})
    history += [{'role':'user','content':question},{'role':'assistant','content':answer['reply']}]
    print('TEXT_CASE',label,round(elapsed,2),'seconds:',answer['reply'],flush=True)
result={'health':health,'cases':records,'checks':'Chinese replies, actual adapter+NF4, HTTP 200, teach answer bounds and valid source IDs; not expert grading'}
(P/'results/local_text_check.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
print('PASS_REAL_TEXT',len(records),'cases',flush=True)
