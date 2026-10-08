"""Local CUDA NF4 inference for the selected Qwen2.5-3B physics adapter."""
import json
import time

class LocalBackend:
    def __init__(self, model_path, adapter_path, max_input_tokens=4096, max_new_tokens=512):
        import torch
        import bitsandbytes as bnb
        import psutil
        from peft import PeftModel
        from transformers import AutoModelForCausalLM, AutoTokenizer, BitsAndBytesConfig
        if not torch.cuda.is_available():
            raise RuntimeError('本机 CUDA 不可用，不能进行 GPU 四位量化测试')
        torch.set_num_threads(4)
        self.torch = torch
        self.process = psutil.Process()
        self.max_input_tokens = max_input_tokens
        self.max_new_tokens = max_new_tokens
        dtype = torch.bfloat16 if torch.cuda.is_bf16_supported() else torch.float16
        self.tokenizer = AutoTokenizer.from_pretrained(adapter_path, local_files_only=True)
        self.tokenizer.pad_token = self.tokenizer.eos_token
        quant = BitsAndBytesConfig(load_in_4bit=True, bnb_4bit_quant_type='nf4',
            bnb_4bit_use_double_quant=True, bnb_4bit_compute_dtype=dtype)
        base = AutoModelForCausalLM.from_pretrained(model_path, local_files_only=True,
            quantization_config=quant, torch_dtype=dtype, device_map={'': 'cuda:0'},
            attn_implementation='sdpa', low_cpu_mem_usage=True)
        if not getattr(base, 'is_loaded_in_4bit', False):
            raise RuntimeError('基础模型未实际以四位量化加载')
        self.quantized_layers = sum(isinstance(layer, bnb.nn.Linear4bit) for layer in base.modules())
        if not self.quantized_layers:
            raise RuntimeError('模型中未找到实际的四位量化层')
        self.model = PeftModel.from_pretrained(base, adapter_path, is_trainable=False).eval()
        self.model_path = str(model_path)
        self.adapter_path = str(adapter_path)
        self.dtype = str(dtype)
        print('MODEL_LOADED_NF4', json.dumps(self.info(), ensure_ascii=False), flush=True)

    def info(self):
        torch = self.torch
        return {'model_path': self.model_path, 'adapter_path': self.adapter_path,
            'quantization': 'NF4 4-bit double quant', 'quantized_layers': self.quantized_layers,
            'compute_dtype': self.dtype, 'gpu': torch.cuda.get_device_name(0),
            'max_input_tokens': self.max_input_tokens, 'max_new_tokens': self.max_new_tokens,
            'model_bytes': self.model.get_memory_footprint(),
            'gpu_allocated_bytes': torch.cuda.memory_allocated(),
            'gpu_reserved_bytes': torch.cuda.memory_reserved(),
            'process_rss_bytes': self.process.memory_info().rss}

    def generate(self, messages, thinking, seed, schema=None):
        # Qwen2.5 uses its normal chat template; there is no thinking-mode switch.
        torch = self.torch
        prompt = self.tokenizer.apply_chat_template(messages, tokenize=False, add_generation_prompt=True)
        inputs = self.tokenizer(prompt, return_tensors='pt')
        count = inputs.input_ids.shape[1]
        if count > self.max_input_tokens:
            raise ValueError('输入超过本机 '+str(self.max_input_tokens)+' token 上限，请缩短问题或开启新对话')
        inputs = inputs.to('cuda:0')
        extra = {}
        if schema is not None:
            from lmformatenforcer import JsonSchemaParser, CharacterLevelParserConfig
            from lmformatenforcer.integrations.transformers import (
                build_token_enforcer_tokenizer_data, build_transformers_prefix_allowed_tokens_fn)
            if not hasattr(self, '_schema_data'):
                self._schema_data = build_token_enforcer_tokenizer_data(self.tokenizer)
            parser = JsonSchemaParser(schema, CharacterLevelParserConfig(alphabet=self._schema_data.tokenizer_alphabet))
            extra['prefix_allowed_tokens_fn'] = build_transformers_prefix_allowed_tokens_fn(self._schema_data, parser)
        torch.manual_seed(seed)
        torch.cuda.reset_peak_memory_stats()
        torch.cuda.synchronize()
        start = time.perf_counter()
        with torch.inference_mode():
            sequences = self.model.generate(**inputs, max_new_tokens=self.max_new_tokens,
                do_sample=False, temperature=None, top_p=None, top_k=None,
                pad_token_id=self.tokenizer.eos_token_id, **extra)
        torch.cuda.synchronize()
        seconds = time.perf_counter()-start
        output = sequences[0, count:]
        raw = self.tokenizer.decode(output, skip_special_tokens=True).strip()
        try:
            parsed = json.loads(raw)
        except ValueError:
            parsed = None
        return {'raw': raw, 'parsed': parsed if isinstance(parsed, dict) else None,
            'seconds': seconds, 'input_tokens': count, 'output_tokens': len(output),
            'tokens_per_second': len(output)/seconds,
            'truncated': len(output) >= self.max_new_tokens,
            'gpu_peak_allocated_bytes': torch.cuda.max_memory_allocated(),
            'gpu_peak_reserved_bytes': torch.cuda.max_memory_reserved(),
            'process_rss_bytes': self.process.memory_info().rss,
            'format_enforcement': 'JSON schema' if schema is not None else None}

    def generate_structured(self, messages, seed, schema):
        return self.generate(messages, False, seed, schema)
