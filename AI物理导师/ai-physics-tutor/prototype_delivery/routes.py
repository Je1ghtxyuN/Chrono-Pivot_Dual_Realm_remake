"""Same-model routes; public mode only, no evaluator fields."""
import json,time
from pipeline import VerifiedTutor,safe_request,evidence_text,RENDERER
from retrieval import dialogue_query
from formatting import generate_teaching,STATUS
DIRECT_SYSTEM=RENDERER.replace('你只输出JSON {"reply":"学生看到的简短引导","evidence_ids":["已给的卡ID"]}。','你只输出JSON字段status、value、unit、reply、evidence_ids。')+'''
你同时核对物理原理、玩家状态、适用条件和证据，不需要另一个后台规划器。status仅answerable/insufficient/inconsistent/invalid_approximation/unsupported；能针对已有问题给引导则answerable；单位误区不是近似失效。缺任务/观测无法具体诊断则insufficient并询问记录，不猜领域。value必须null、unit必须空字符串，引用只选实际支持陈述的给定ID，无关资料不用引用。先明确认可学生已改正的理解，再回应最新问题，不能一直重复旧误区。'''
class ModeTutor(VerifiedTutor):
 def respond(self,row,seed=42):
  self.thinking=row.get('mode') in ('solve','concept')
  return super().respond(row,seed)
def direct_response(backend,index,row,seed):
 if row.get('mode')!='teach':raise ValueError('Direct route is teaching only')
 start=time.perf_counter();query=dialogue_query(row);t=time.perf_counter();evidence=index.search(query,k=3);retrieval_seconds=time.perf_counter()-t
 chat=[{'role':'system','content':DIRECT_SYSTEM},{'role':'user','content':json.dumps({**safe_request(row),'knowledge':evidence_text(evidence)},ensure_ascii=False)}]
 rec=generate_teaching(backend,chat,seed,[c['id']for c in evidence],True);a=rec['parsed'];ids={c['id']for c in evidence}
 valid=isinstance(a,dict) and a.get('status') in STATUS and a.get('value','missing') is None and a.get('unit')=='' and isinstance(a.get('reply'),str) and bool(a['reply'].strip()) and isinstance(a.get('evidence_ids'),list) and all(isinstance(c,str) and c in ids for c in a['evidence_ids'])
 return {'parsed':a if valid else None,'valid':valid,'direct_input':chat,'direct_output':rec,'evidence':evidence,'retrieval_query':query,'retrieval_seconds':retrieval_seconds,'response_seconds':time.perf_counter()-start,'truncated':rec['truncated'],'planner_fallback':False}
