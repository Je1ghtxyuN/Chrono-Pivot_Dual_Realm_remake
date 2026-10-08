import json,sys,unittest
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parents[1]))
from retrieval import KnowledgeIndex
import retrieval
class DialogueRetrievalTests(unittest.TestCase):
 def test_short_followup_keeps_actual_user_task(self):
  row={'question':'向上设为正，下一步呢？','student_state':{},'history':[{'role':'user','content':'电梯中人站在秤上，向上加速，想比较支持力与重力。'},{'role':'assistant','content':'观察运动状态。'}]}
  query=retrieval.dialogue_query(row)
  index=KnowledgeIndex(Path(__file__).parents[1]/'data/knowledge.json')
  self.assertIn('weight_normal', [x['id'] for x in index.search(query)])
  self.assertIn('电梯',query)
 def test_short_followup_keeps_lens_not_generated_domain(self):
  row={'question':'负号不是长度为负吧？','history':[{'role':'user','content':'薄透镜放大率算出了负数，请解释像的朝向。'},{'role':'assistant','content':'气体 温度 压强 温度 压强 气体 气体'}]}
  q=retrieval.dialogue_query(row)
  self.assertNotIn('气体',q)
  index=KnowledgeIndex(Path(__file__).parents[1]/'data/knowledge.json')
  self.assertIn('lens',[x['id'] for x in index.search(q)])
 def test_new_session_and_evaluator_fields_excluded(self):
  row={'question':'弹簧的形变量要以什么单位代入？','history':[],'student_state':{'observed':'长度单位厘米'},'gold':{'value':123987},'topic':'SECRET_TOPIC','source_id':'SECRET_SOURCE'}
  q=retrieval.dialogue_query(row)
  for x in ('123987','SECRET_TOPIC','SECRET_SOURCE'):self.assertNotIn(x,q)
  self.assertIn('厘米',q)
 def test_query_is_bounded(self):
  row={'question':'Q'*4000,'history':[{'role':'user','content':'A'*4000}for _ in range(20)],'student_state':{'long':'B'*8000}}
  self.assertLessEqual(len(retrieval.dialogue_query(row)),16000)
if __name__=='__main__':unittest.main()
