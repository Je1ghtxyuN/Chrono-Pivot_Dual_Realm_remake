import json,sys,unittest
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parents[1]))
from pipeline import plan_messages,render_messages,execute_plan,VerifiedTutor
class PipelineTests(unittest.TestCase):
 def test_adaptive_retry_records_model_attempts(self):
  class Index:
   def search(self,*a,**k):return []
  class Backend:
   def __init__(self):self.calls=[]
   def generate(self,chat,thinking,seed):
    self.calls.append(thinking)
    p={'status':'answerable' if not thinking else 'inconsistent','expression':None,'unit':'','principle':'合力判断','conditions':'缺少相容条件','diagnosis':'条件矛盾','evidence_ids':[]}
    return {'parsed':p,'raw':json.dumps(p),'truncated':False,'seconds':.01}
  backend=Backend();r=VerifiedTutor(backend,Index(),adaptive=True).respond({'question':'条件是否相容','mode':'solve'})
  self.assertEqual(backend.calls,[False,True]);self.assertTrue(r['valid']);self.assertEqual(r['parsed']['status'],'inconsistent');self.assertTrue(r['planner_fallback'])
 def test_model_calculator_requires_explicit_angle_unit(self):
  common={'status':'answerable','unit':'ratio','principle':'正弦','conditions':'角度以度给出','diagnosis':'核对角度','evidence_ids':[]}
  self.assertFalse(execute_plan({**common,'expression':'sin(60)'},[])['valid'])
  result=execute_plan({**common,'expression':'sin_deg(60)'},[])
  self.assertTrue(result['valid']);self.assertAlmostEqual(result['value'],3**.5/2)
 def test_numeric_expression_requires_declared_unit(self):
  result=execute_plan({'status':'answerable','expression':'1','unit':'','principle':'比例','conditions':'同条件','diagnosis':'','evidence_ids':[]},[])
  self.assertFalse(result['valid'])
 def test_retry_receives_actual_calculator_error(self):
  class Index:
   def search(self,*a,**k):return []
  class Backend:
   def __init__(self):self.chats=[]
   def generate(self,chat,thinking,seed):
    self.chats.append(chat)
    plan={'status':'answerable','expression':'sin(60)' if len(self.chats)==1 else 'sin_deg(60)','unit':'ratio','principle':'正弦','conditions':'度数已知','diagnosis':'核对角度单位','evidence_ids':[]}
    return {'parsed':plan,'raw':json.dumps(plan),'truncated':False,'seconds':.01}
  b=Backend();r=VerifiedTutor(b,Index(),adaptive=True).respond({'question':'求60度正弦','mode':'solve'})
  self.assertTrue(r['valid']);self.assertIn('ambiguous_trig_unit',b.chats[-1][-1]['content']);self.assertAlmostEqual(r['parsed']['value'],3**.5/2)
 def test_numeric_verification_uses_real_calculator(self):
  r=execute_plan({'status':'answerable','expression':'970*9.8*0.37','unit':'Pa','principle':'静液压','diagnosis':'','conditions':'密度近似均匀'},[])
  self.assertAlmostEqual(r['value'],3517.22);self.assertTrue(r['tool_executed'])
 def test_condition_list_preserves_model_claims(self):
  r=execute_plan({'status':'answerable','expression':'2*9','unit':'deg','principle':'反射定律','conditions':['固定入射光','镜面在入射平面转动'],'diagnosis':'检查角度','evidence_ids':[]},[])
  self.assertTrue(r['valid']);self.assertEqual(r['conditions'],'固定入射光；镜面在入射平面转动')
 def test_condition_list_rejects_nested_and_non_text(self):
  r=execute_plan({'status':'answerable','expression':'2*9','unit':'deg','principle':'反射定律','conditions':['条件',{'secret':1}],'diagnosis':'检查角度','evidence_ids':[]},[])
  self.assertFalse(r['valid'])
 def test_untrusted_fields_cannot_claim_validity(self):
  r=execute_plan({'valid':True,'tool_executed':True,'status':'made_up','unit':'Pa'},[])
  self.assertFalse(r['valid']);self.assertFalse(r['tool_executed'])
 def test_bad_plan_never_invents_answer(self):
  for expr in ('__import__("os")','1/0',None):
   r=execute_plan({'status':'answerable','expression':expr,'unit':'Pa'},[],numeric_required=True)
   self.assertFalse(r['valid']);self.assertIsNone(r['value'])
 def test_hint_render_has_no_hidden_numeric_solution(self):
  r={'question':'请直接给答案','mode':'teach','hint_level':1,'gold':{'value':98765}}
  plan={'status':'answerable','value':876543.21,'expression':'123*456','principle':'Q=mcΔT','conditions':'无相变','diagnosis':'先核对质量单位','evidence_ids':[]}
  text=json.dumps(render_messages(r,plan,[]),ensure_ascii=False)
  for secret in ('98765','876543','123*456','Q=mc'):self.assertNotIn(secret,text)
 def test_planner_never_receives_grading_material(self):
  s=json.dumps(plan_messages({'question':'周期','gold':{'value':98765},'required_points':['SECRET']},[]),ensure_ascii=False)
  self.assertNotIn('98765',s);self.assertNotIn('SECRET',s)
if __name__=='__main__':unittest.main()
