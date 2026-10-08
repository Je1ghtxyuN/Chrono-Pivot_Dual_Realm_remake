import unittest,sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parents[1]))
from api_contract import validate_request,public_response
class ApiTests(unittest.TestCase):
 def test_limits_and_types(self):
  for row in ({'question':'','hint_level':1},{'question':'x','hint_level':True},{'question':'x','hint_level':4},{'question':'x'*4001},{'question':'x','history':'secret'},{'question':'x','mode':'change_system'}):
   with self.assertRaises(ValueError):validate_request(row)
 def test_actual_dialogue_history_preserved(self):
  r=validate_request({'question':'我画了受力图','hint_level':2,'history':[{'role':'assistant','content':'请先指定对象'},{'role':'user','content':'我研究盒子'}]})
  self.assertEqual(r['history'][0]['content'],'请先指定对象')
 def test_no_hidden_backend_answer_leaks_to_teach_api(self):
  private={'parsed':{'status':'answerable','value':None,'unit':'','reply':'先观察','evidence_ids':[]},'valid':True,'verified_plan':{'value':876543},'planner_output':{'raw':'秘密推导'},'response_seconds':1.5,'evidence':[]}
  out=public_response(private,'teach')
  self.assertNotIn('verified_plan',out);self.assertNotIn('planner_output',out);self.assertIsNone(out['answer']['value'])
 def test_invalid_model_does_not_become_a_rule_success(self):
  out=public_response({'valid':False,'parsed':None,'response_seconds':1.5,'evidence':[]},'teach')
  self.assertFalse(out['ok']);self.assertIsNone(out['answer'])
if __name__=='__main__':unittest.main()
