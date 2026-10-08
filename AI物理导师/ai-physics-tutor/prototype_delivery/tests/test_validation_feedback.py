import json,sys,unittest
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parents[1]))
import pipeline
class FeedbackTests(unittest.TestCase):
 def test_variable_error_explained_without_inventing_binding(self):
  f=pipeline.validation_feedback({'error':'invalid_expression','tool_error':'Unsupported expression','expression':'1/(1/f-1/u)'})
  self.assertEqual(f['undefined_symbols'],['f','u'])
  self.assertIn('数字字面量',f['instruction'])
  self.assertNotIn('sin_deg(30)',json.dumps(f));self.assertNotIn('0.2',json.dumps(f))
 def test_inconsistent_status_not_forced_to_numeric_answer(self):
  f=pipeline.validation_feedback({'error':'incompatible_expression_status','status':'inconsistent','expression':'0.2*30'})
  self.assertIn('null',f['instruction']);self.assertIn('中间',f['instruction'])
  self.assertEqual(f['actual_validator_error'],'incompatible_expression_status')
 def test_renderer_can_check_actual_evidence_content(self):
  card={'id':'cardA','title':'力','text':'零合力不代表速度为零。','misconceptions':'速度与加速度不同','source_url':'https://example.org/book','source_section':'1','attribution':'test'}
  m=pipeline.render_messages({'question':'该怎么核对','mode':'teach','hint_level':2},{'status':'answerable','conditions':'惯性系','diagnosis':'核对状态','principle':'合力','value':987654,'expression':'999*999'},[card])
  text=json.dumps(m,ensure_ascii=False)
  self.assertIn(card['text'],text);self.assertNotIn('987654',text);self.assertNotIn('999*999',text)
if __name__=='__main__':unittest.main()
