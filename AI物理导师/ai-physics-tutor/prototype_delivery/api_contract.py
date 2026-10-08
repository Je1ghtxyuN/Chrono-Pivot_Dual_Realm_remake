"""Request/response contract with bounded inputs and no implicit rule fallback."""
def validate_request(data):
 if not isinstance(data,dict):raise ValueError('请求必须是JSON对象')
 question=data.get('question');mode=data.get('mode','teach');level=data.get('hint_level',1)
 if not isinstance(question,str) or not 1<=len(question.strip())<=4000:raise ValueError('问题需为1至4000字符')
 if mode not in ('solve','concept','teach'):raise ValueError('mode需为solve/concept/teach')
 if type(level)!=int or not 1<=level<=3:raise ValueError('提示等级需为整数1至3')
 state=data.get('student_state',{});history=data.get('history',[])
 if not isinstance(state,dict) or len(str(state))>8000:raise ValueError('学生状态格式或长度错误')
 if not isinstance(history,list) or len(history)>20:raise ValueError('对话历史格式或长度错误')
 for item in history:
  if not isinstance(item,dict) or item.get('role') not in ('user','assistant') or not isinstance(item.get('content'),str) or len(item['content'])>4000:raise ValueError('历史需为user/assistant文本列表')
 if sum(len(item['content']) for item in history)>10000:raise ValueError('对话历史太长，请开启新对话')
 return {'question':question.strip(),'mode':mode,'hint_level':level,'student_state':state,'history':history}
def public_response(private,mode):
 evidence=[{k:c[k] for k in ('id','title','source_url','source_section','source_revision','attribution','review_status','text','misconceptions','license') if k in c} for c in private.get('evidence',[])]
 answer=private.get('parsed') if private.get('valid') else None
 if answer and mode=='teach':answer={**answer,'value':None,'unit':''}
 return {'ok':answer is not None,'answer':answer,'evidence':evidence,'response_seconds':private.get('response_seconds'),'error':None if answer is not None else '模型未返回通过结构核验的结果；本次回答失败，请保留日志。','review_status':'自动核验与模型辅助审查，尚非专家审核','scope':'入门物理的有限知识范围'}
