import sys,unittest
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parents[1]))
from formatting import teaching_schema,generate_teaching
class StructuredTeachingTests(unittest.TestCase):
 def test_actual_retrieved_id_enum_not_answer_injection(self):
  s=teaching_schema(['cardA','cardA','cardB'],True)
  self.assertEqual(s['properties']['evidence_ids']['items']['enum'],['cardA','cardB'])
  self.assertEqual(s['properties']['value']['type'],'null')
  self.assertEqual(s['properties']['unit']['enum'],[''])
  self.assertFalse(s['additionalProperties'])
 def test_unknown_task_can_produce_no_citations(self):
  s=teaching_schema([],False)
  self.assertEqual(s['properties']['evidence_ids']['maxItems'],0)
  self.assertNotIn('value',s['properties'])
 def test_real_structured_method_is_called_with_unchanged_messages(self):
  class Backend:
   def generate_structured(self,messages,seed,schema):self.args=messages,seed,schema;return {'raw':'test'}
   def generate(self,*args):raise AssertionError('Unconstrained generation should not be used')
  b=Backend();m=[{'role':'user','content':'自然问题'}];generate_teaching(b,m,321,['cardA'])
  self.assertEqual(b.args[0],m);self.assertEqual(b.args[1],321)
  self.assertEqual(b.args[2]['properties']['evidence_ids']['items']['enum'],['cardA'])
if __name__=='__main__':unittest.main()
