"""BM25 candidate retriever: natural text only, no topic labels or answer key."""
from collections import Counter
import json
import math
from pathlib import Path
import re

def tokens(text):
    text=text.lower()
    result=re.findall(r'[a-z]+(?:\d+)?',text)
    for fragment in re.findall(r'[\u4e00-\u9fff]+',text):
        result.extend(fragment[i:i+2] for i in range(len(fragment)-1))
    return result

class KnowledgeIndex:
    def __init__(self,path):
        self.cards=json.loads(Path(path).read_text())
        self.by_id={c['id']:c for c in self.cards}
        if len(self.by_id)!=len(self.cards):raise ValueError('Duplicate knowledge identifiers')
        self.docs=[Counter(tokens(c['title']+' '+c['text']+' '+c['misconceptions']+' '+' '.join(c['aliases']))) for c in self.cards]
        self.lengths=[sum(d.values()) for d in self.docs];self.avg=sum(self.lengths)/max(1,len(self.docs))
        self.df=Counter(t for d in self.docs for t in d)

    def search(self,query,k=3):
        if not isinstance(query,str):raise TypeError('query must be natural-language text')
        if not 1<=k<=100:raise ValueError('k must be1..100')
        terms=set(tokens(query));scores=[];n=len(self.cards)
        for i,d in enumerate(self.docs):
            score=0.0
            for t in terms:
                freq=d.get(t,0)
                if not freq:continue
                idf=math.log(1+(n-self.df[t]+.5)/(self.df[t]+.5))
                score+=idf*freq*2.2/(freq+1.2*(.25+.75*self.lengths[i]/self.avg))
            if score>0:scores.append((score,i))
        # A zero-overlap request gets no evidence; threshold calibration uses dev only.
        return [{'score':score,**self.cards[i]} for score,i in sorted(scores,key=lambda x:(-x[0],self.cards[x[1]]['id']))[:k]]


def dialogue_query(row):
    """Natural text only: bounded task anchor and recent real user context.

    Assistant text is not retrieval evidence. A new UI session supplies empty
    history. No guessed topic reset, evaluator labels, or source IDs are used.
    """
    question = row.get('question', '')
    if not isinstance(question, str):
        raise TypeError('question must be text')
    history = row.get('history', [])
    user_turns = [m['content'] for m in history if isinstance(m, dict)
                  and m.get('role') == 'user' and isinstance(m.get('content'), str)]
    contextual = ([user_turns[0]] + user_turns[-2:]) if user_turns else []
    # Stable deduplication avoids overweighting a repeated task anchor.
    parts = list(dict.fromkeys(t[:2000] for t in contextual if t != question))
    parts.append(question[:4000])
    parts.append(json.dumps(row.get('student_state', {}), ensure_ascii=False)[:4000])
    return ' '.join(parts)[:16000]
