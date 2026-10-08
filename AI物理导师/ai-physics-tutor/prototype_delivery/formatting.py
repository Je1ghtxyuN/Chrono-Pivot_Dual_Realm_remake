"""Generation schemas restrict structure, never supply physics answers."""
STATUS=['answerable','insufficient','inconsistent','invalid_approximation','unsupported']
def teaching_schema(ids,direct=False):
 ids=list(dict.fromkeys(ids))
 citation={'type':'array','items':{'type':'string'},'maxItems':len(ids)}
 if ids:citation['items']['enum']=ids
 props={'reply':{'type':'string','minLength':1},'evidence_ids':citation}
 if direct:props.update(status={'type':'string','enum':STATUS},value={'type':'null'},unit={'type':'string','enum':['']})
 return {'type':'object','properties':props,'required':list(props),'additionalProperties':False}
def generate_teaching(backend,messages,seed,ids,direct=False):
 schema=teaching_schema(ids,direct)
 # The real backend implements generate_structured; test doubles may use generate.
 if hasattr(backend,'generate_structured'):
  return backend.generate_structured(messages,seed,schema)
 return backend.generate(messages,False,seed)
