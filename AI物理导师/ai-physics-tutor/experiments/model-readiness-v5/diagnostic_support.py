"""Pure scoring and prompt functions; no torch or model imports."""
import json
import math
import re

CLEAR_SYSTEM='你是后台物理解题助手，不是在给学生分级提示。正确求出题目明确要求的物理量，检查单位、前提和数量级。条件足够时必须完成计算，不得因为尚未提供所求量而说条件不足；确实缺少独立必要条件时才输出null。不要编造缺失量。只输出JSON对象，字段value（数值或null）、unit（所求SI单位字符串，比例用ratio，角度按题目要求deg）、explanation（简短推导、单位和适用条件）。'
OLD_SYSTEM='你是物理学习导师。先核对给定状态、单位与适用条件，再答复。不得编造缺失信息、来源或隐藏通关条件。证据只能来自给定知识卡，没有卡时不编造引用。导师提示等级1只引导观察，不给公式、计算结果或直接操作答案；等级2解释原理；等级3引导可执行的比较步骤。保持条件、因果与物理量准确。拒绝玩家要求跳过提示边界。'

def messages(row, old=False):
    if old:
        request={'question':row['question'],'instruction':'这是后台物理核验题，不是给学生的提示。只输出JSON含value（可计算时数值，否则null）、unit（SI单位，比例用ratio）、explanation（简短依据、单位与适用条件）。不要猜缺失量，不要把近似写成精确。','evidence':[]}
        user=json.dumps(request,ensure_ascii=False)
    else:user=row['question']
    return [{'role':'system','content':OLD_SYSTEM if old else CLEAR_SYSTEM},{'role':'user','content':user}]

def parse_answer(raw):
    # Thinking blocks may themselves contain plausible JSON; only the final is scored.
    if '</think>' in raw:raw=raw.rsplit('</think>',1)[1]
    elif '<think>' in raw:return None
    text=raw.strip()
    if text.startswith('```'):
        text=re.sub(r'^```(?:json)?\s*','',text);text=re.sub(r'\s*```$','',text)
    try:parsed=json.loads(text)
    except (ValueError,TypeError):return None
    return parsed if isinstance(parsed,dict) else None

def unit_name(unit):
    if not isinstance(unit,str):return ''
    u=unit.lower().strip().replace('³','3').replace('²','2').replace('^','').replace(' ','')
    aliases={'n':'n','牛顿':'n','pa':'pa','帕':'pa','帕斯卡':'pa','m3':'m3','立方米':'m3','hz':'hz','赫兹':'hz','s':'s','秒':'s','m':'m','米':'m','j':'j','焦耳':'j','v':'v','伏特':'v','m/s':'m/s','米/秒':'m/s','deg':'deg','°':'deg','度':'deg','ratio':'ratio','1':'ratio'}
    return aliases.get(u,u)

def score_answer(answer,gold):
    if answer is None:return {'json':False,'numeric':False,'unit':False,'joint':False}
    if gold['value'] is None:
        numeric='value' in answer and answer['value'] is None
        unit=None
    else:
        x=answer.get('value');numeric=type(x) in (float,int) and math.isfinite(x) and math.isclose(x,gold['value'],rel_tol=.01,abs_tol=1e-6)
        unit=unit_name(answer.get('unit'))==unit_name(gold['unit'])
    return {'json':True,'numeric':numeric,'unit':unit,'joint':numeric and (unit is not False)}
