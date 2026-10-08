"""Finite, bounded arithmetic only; no eval, filesystem, variables or code execution."""
import ast
import math
import operator
BIN={ast.Add:operator.add,ast.Sub:operator.sub,ast.Mult:operator.mul,ast.Div:operator.truediv,ast.Pow:operator.pow}
FUN={n:getattr(math,n) for n in ('sqrt','sin','cos','tan','asin','acos','atan','log','log10','exp')}
FUN['abs']=abs
for name in ('sin','cos','tan'):
 FUN[name+'_rad']=getattr(math,name)
 FUN[name+'_deg']=lambda x,n=name:getattr(math,n)(math.radians(x))
for name in ('asin','acos','atan'):
 FUN[name+'_rad']=getattr(math,name)
 FUN[name+'_deg']=lambda x,n=name:math.degrees(getattr(math,n)(x))

def calculate(expression):
 if not isinstance(expression,str) or not 1<=len(expression)<=512:raise ValueError('Expression length')
 try:
  tree=ast.parse(expression,mode='eval')
  if sum(1 for _ in ast.walk(tree))>100:raise ValueError('Expression complexity')
  def visit(n):
   if isinstance(n,ast.Constant) and type(n.value) in (int,float):value=n.value
   elif isinstance(n,ast.Name) and n.id in ('pi','e'):value=getattr(math,n.id)
   elif isinstance(n,ast.UnaryOp) and isinstance(n.op,(ast.UAdd,ast.USub)):value=visit(n.operand)*(1 if isinstance(n.op,ast.UAdd) else -1)
   elif isinstance(n,ast.BinOp) and type(n.op) in BIN:
    a,b=visit(n.left),visit(n.right)
    if isinstance(n.op,ast.Pow) and abs(b)>100:raise ValueError('Exponent limit')
    value=BIN[type(n.op)](a,b)
   elif isinstance(n,ast.Call) and isinstance(n.func,ast.Name) and n.func.id in FUN and len(n.args)==1 and not n.keywords:value=FUN[n.func.id](visit(n.args[0]))
   else:raise ValueError('Unsupported expression')
   if type(value) not in (int,float) or not math.isfinite(value) or abs(value)>1e100:raise ValueError('Nonfinite or magnitude limit')
   return float(value)
  return visit(tree.body)
 except (SyntaxError,TypeError,ArithmeticError,RecursionError) as exc:raise ValueError('Invalid arithmetic') from exc
