"""Load the real selected 3B LoRA and record one deterministic loading example.
This is a loading check, not a new model comparison or teaching-effect evaluation.
GPU execution only inside Slurm.
"""
import hashlib,json,os,time
from pathlib import Path
import torch
from transformers import AutoModelForCausalLM,AutoTokenizer,BitsAndBytesConfig
from peft import PeftModel
P=Path(__file__).resolve().parents[1]
def main():
 assert os.environ.get('SLURM_JOB_ID') and torch.cuda.is_available(), 'Slurm GPU allocation required'
 torch.set_num_threads(8);torch.manual_seed(42);start=time.time();adapter=P/'outputs/run_001/best_adapter';base=P/'models/Qwen2.5-3B-Instruct'
 out=P/'prototype_delivery/results'/('adapter_load_job_'+os.environ['SLURM_JOB_ID']);out.mkdir(parents=True,exist_ok=False)
 sample=json.loads((P/'data/test.jsonl').open().readline());messages=sample['messages'][:-1]
 config={'purpose':'verify real trained artifact loading, not performance benchmark','job':os.environ['SLURM_JOB_ID'],'base_model_path':str(base),'adapter_path':str(adapter),'adapter_sha256':hashlib.sha256((adapter/'adapter_model.safetensors').read_bytes()).hexdigest(),'sample_id':sample['id'],'input_messages':messages,'quantization':'NF4 double quant BF16'}
 (out/'config.json').write_text(json.dumps(config,ensure_ascii=False,indent=2))
 tok=AutoTokenizer.from_pretrained(adapter,local_files_only=True);tok.pad_token=tok.eos_token
 q=BitsAndBytesConfig(load_in_4bit=True,bnb_4bit_quant_type='nf4',bnb_4bit_use_double_quant=True,bnb_4bit_compute_dtype=torch.bfloat16)
 model=AutoModelForCausalLM.from_pretrained(base,local_files_only=True,quantization_config=q,torch_dtype=torch.bfloat16,device_map={'':'cuda:0'},attn_implementation='sdpa')
 model=PeftModel.from_pretrained(model,adapter,is_trainable=False).eval()
 inp=tok(tok.apply_chat_template(messages,tokenize=False,add_generation_prompt=True),return_tensors='pt').to('cuda')
 with torch.inference_mode():seq=model.generate(**inp,max_new_tokens=384,do_sample=False,pad_token_id=tok.eos_token_id,use_cache=True)
 raw=tok.decode(seq[0,inp.input_ids.shape[1]:],skip_special_tokens=True)
 try:parsed=json.loads(raw)
 except ValueError:parsed=None
 result={'loaded':True,'raw':raw,'parsed':parsed,'wall_seconds':time.time()-start,'output_tokens':len(seq[0])-inp.input_ids.shape[1],'quality_note':'One held-out synthetic sample confirms loading only; prior independent challenge failures remain.'}
 (out/'result.json').write_text(json.dumps(result,ensure_ascii=False,indent=2));print('EXPLORATORY_ADAPTER_LOADED',result['output_tokens'],'tokens',flush=True)
if __name__=='__main__':main()
