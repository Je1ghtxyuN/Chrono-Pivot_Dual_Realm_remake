"""Necessary HTTP smoke cases, not a new model benchmark or teaching acceptance."""
import json,time,urllib.request,urllib.error
from pathlib import Path
P=Path(__file__).parent
out=P/'evidence';out.mkdir(exist_ok=True)
records=[]
def call(label,row):
 body=json.dumps(row,ensure_ascii=False).encode(); start=time.perf_counter()
 try:
  with urllib.request.urlopen(urllib.request.Request('http://127.0.0.1:18080/api/tutor',body,{'Content-Type':'application/json'}),timeout=120) as r:code=r.status;raw=r.read().decode()
 except urllib.error.HTTPError as e:code=e.code;raw=e.read().decode()
 response=json.loads(raw)
 rec={'case':label,'request':row,'http_status':code,'response':response,'wall_seconds':time.perf_counter()-start,'automatic_check':'interface and known demo quantity only; not expert review'}
 records.append(rec);(out/'http_smoke.jsonl').write_text(''.join(json.dumps(x,ensure_ascii=False)+'\n' for x in records))
 print(label,code,response.get('answer'),flush=True)
 return response
base=json.loads((out/'request_level1.json').read_text());ans=json.loads((out/'response_level1.json').read_text())
history=[{'role':'user','content':base['question']},{'role':'assistant','content':ans['answer']['reply']}]
for level in (2,3):
 row={'question':'我理解了力和质量不能相加。请继续给第'+str(level)+'级提示。','mode':'teach','hint_level':level,'student_state':{'observation':'质量2 kg，合力6 N。我已纠正相加的想法。'},'history':history}
 response=call('newton_level_'+str(level),row)
 if response.get('ok'):history+= [{'role':'user','content':row['question']},{'role':'assistant','content':response['answer']['reply']}]
call('numerical_tool',{'question':'物体质量2 kg，水平方向合力6 N，在惯性参考系求其加速度，单位m/s²。','mode':'solve','hint_level':3,'student_state':{},'history':[]})
call('concept_condition',{'question':'匀速圆周运动中，速率不变为什么仍然有加速度？请解释适用条件。','mode':'concept','hint_level':2,'student_state':{},'history':[]})
call('missing_information',{'question':'实验失败了，我应该怎么办？','mode':'teach','hint_level':1,'student_state':{},'history':[]})
call('invalid_request',{'question':'检查接口','hint_level':4})
summary={'purpose':'small prototype end-to-end check only','cases':len(records),'successful_answer_count':sum(x['response'].get('ok',False) for x in records),'input_rejection_400':records[-1]['http_status']==400,'unity_editor_executed':False,'records':'http_smoke.jsonl'}
(out/'http_smoke_summary.json').write_text(json.dumps(summary,ensure_ascii=False,indent=2))
