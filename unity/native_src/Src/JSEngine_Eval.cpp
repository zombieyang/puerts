#include "JSEngine.h"
#if WITH_QUICKJS
#include "quickjs.h"
#endif
namespace puerts {
#if !WITH_QUICKJS
    v8::MaybeLocal<v8::Module> ResolveModule(
        v8::Local<v8::Context> Context,
        v8::Local<v8::String> Specifier,
        v8::Local<v8::Module> Referrer
    )
    {
        v8::Isolate* Isolate = Context->GetIsolate();
        JSEngine* JsEngine = FV8Utils::IsolateData<JSEngine>(Isolate);
        
        v8::String::Utf8Value Specifier_utf8(Isolate, Specifier);
        std::string Specifier_std(*Specifier_utf8, Specifier_utf8.length());
    #if !WITH_QUICKJS
        auto Iter = JsEngine->ModuleCacheMap.find(Specifier_std);
        if (Iter != JsEngine->ModuleCacheMap.end())//create and link
        {
            return v8::Local<v8::Module>::New(Isolate, Iter->second);
        }
    #endif 
        size_t Length = 0;
        const char* Code = JsEngine->ModuleResolver(Specifier_std.c_str(), JsEngine->Idx, Length);
        if (Code == nullptr) 
        {
            return v8::MaybeLocal<v8::Module>();
        }

        v8::ScriptOrigin Origin(Specifier,
                            v8::Integer::New(Isolate, 0),                      // line offset
                            v8::Integer::New(Isolate, 0),                    // column offset
                            v8::True(Isolate),                    // is cross origin
                            v8::Local<v8::Integer>(),                 // script id
                            v8::Local<v8::Value>(),                   // source map URL
                            v8::False(Isolate),                   // is opaque (?)
                            v8::False(Isolate),                   // is WASM
                            v8::True(Isolate),                    // is ES Module
                            v8::PrimitiveArray::New(Isolate, 10));
        v8::TryCatch TryCatch(Isolate);

        v8::Local<v8::Module> Module;
        v8::ScriptCompiler::CompileOptions options;
        
        v8::ScriptCompiler::Source Source(FV8Utils::V8String(Isolate, Code), Origin);

        if (!v8::ScriptCompiler::CompileModule(Isolate, &Source, v8::ScriptCompiler::kNoCompileOptions)
                .ToLocal(&Module)) 
        {
            JsEngine->LastExceptionInfo = FV8Utils::ExceptionToString(Isolate, TryCatch);
            return v8::MaybeLocal<v8::Module>();
        }

    #if !WITH_QUICKJS
        JsEngine->ModuleCacheMap[Specifier_std] = v8::UniquePersistent<v8::Module>(Isolate, Module);
    #endif 

        return Module;
    }
#else 

// copy from pixui
#if !defined(MAKEFOURCC)
#define MAKEFOURCC(ch0, ch1, ch2, ch3)                        \
	((unsigned int) (unsigned char) (ch0) | ((unsigned int) (unsigned char) (ch1) << 8) | \
	 ((unsigned int) (unsigned char) (ch2) << 16) | ((unsigned int) (unsigned char) (ch3) << 24))
#endif
#define TAG_PIX1  (MAKEFOURCC('P', 'I', 'X', '1'))
#define TAG_PIX2  (MAKEFOURCC('P', 'I', 'X', '2'))

    bool JS_IsCompiled(const void *buf) {
        unsigned int tag = *(unsigned  int*) buf;
        return tag == TAG_PIX1 || tag == TAG_PIX2;
    }

    int pxJSCompiledModuleInitFunc(JSContext *ctx, JSModuleDef *m)
    {
        return 0;
    }
// end copy from pixui

    JSModuleDef* js_module_loader(JSContext* ctx, const char *name, void *opaque) {
        JSRuntime *rt = JS_GetRuntime(ctx);
        v8::Isolate* Isolate = (v8::Isolate*)JS_GetRuntimeOpaque(rt);
        JSEngine* JsEngine = FV8Utils::IsolateData<JSEngine>(Isolate);

        std::string name_std(name);

        auto Iter = JsEngine->ModuleCacheMap.find(name_std);
        if (Iter != JsEngine->ModuleCacheMap.end())//create and link
        {
            return Iter->second;
        }

        size_t Length = 0;
        const char* Code = JsEngine->ModuleResolver(name_std.c_str(), JsEngine->Idx, Length);
        JSValue func_val;
        JSModuleDef* module_ = nullptr;

		if (JS_IsCompiled(Code))
		{
			JSValue obj = JS_LoadBuffer(ctx, name, (uint8_t*)Code, Length);

			uint32_t tag = JS_VALUE_GET_TAG(obj);
			if (tag == JS_TAG_OBJECT && JS_IsFunction(ctx, obj))
			{
				JSValue ret = func_val = JS_Call(ctx, obj, JS_Undefined(), 0, nullptr);
				JS_FreeValue(ctx, obj);
				if (!JS_IsException(ret))
				{
					JS_FreeValue(ctx, ret);
                    module_ = JS_NewCModule(ctx, name, pxJSCompiledModuleInitFunc);
				}
			}
			else if (tag == JS_TAG_MODULE)
			{
				func_val = obj;
			}
			else if (tag == JS_TAG_EXCEPTION)
			{
				func_val = JS_EXCEPTION;
			} 
            else 
            {
                // printf("unknown tag %d\n", tag);
            }

		} else {
            func_val = JS_Eval(ctx, Code, strlen(Code), name, JS_EVAL_TYPE_MODULE | JS_EVAL_FLAG_COMPILE_ONLY);
        }

		if (JS_IsException(func_val))
		{
            // it will be handled in outest place
            // Isolate->handleException();
			return nullptr;
		}

        if (module_ == nullptr) {
            module_ = (JSModuleDef *) JS_VALUE_GET_PTR(func_val);
            JS_FreeValue(ctx, func_val);
        }

        JsEngine->ModuleCacheMap[name_std] = module_;

        return module_;
    }
#endif

    bool JSEngine::ExecuteModule(const char* Path) 
    {
        if (ModuleResolver == nullptr) 
        {
            LastExceptionInfo = "ModuleResolver is not registered";
            return false;
        }
        v8::Isolate* Isolate = MainIsolate;
        v8::Isolate::Scope IsolateScope(Isolate);
        v8::HandleScope HandleScope(Isolate);
        v8::Local<v8::Context> Context = ResultInfo.Context.Get(Isolate);
        v8::Context::Scope ContextScope(Context);
        v8::TryCatch TryCatch(Isolate);
        
#if !WITH_QUICKJS
        v8::ScriptOrigin Origin(FV8Utils::V8String(Isolate, ""),
                            v8::Integer::New(Isolate, 0),                      // line offset
                            v8::Integer::New(Isolate, 0),                    // column offset
                            v8::True(Isolate),                    // is cross origin
                            v8::Local<v8::Integer>(),                 // script id
                            v8::Local<v8::Value>(),                   // source map URL
                            v8::False(Isolate),                   // is opaque (?)
                            v8::False(Isolate),                   // is WASM
                            v8::True(Isolate),                    // is ES Module
                            v8::PrimitiveArray::New(Isolate, 10));
        
        v8::ScriptCompiler::Source Source(FV8Utils::V8String(Isolate, ""), Origin);
        v8::Local<v8::Module> EntryModule = v8::ScriptCompiler::CompileModule(Isolate, &Source, v8::ScriptCompiler::kNoCompileOptions)
                .ToLocalChecked();
                
        v8::MaybeLocal<v8::Module> Module = ResolveModule(Context, FV8Utils::V8String(Isolate, Path), EntryModule);

        if (Module.IsEmpty())
        {
            return false;
        }

        v8::Local<v8::Module> ModuleChecked = Module.ToLocalChecked();
        v8::Maybe<bool> ret = ModuleChecked->InstantiateModule(Context, ResolveModule);
        if (!ret.ToChecked()) 
        {
            LastExceptionInfo = FV8Utils::ExceptionToString(Isolate, TryCatch);
            return false;
        }
        v8::MaybeLocal<v8::Value> evalRet = ModuleChecked->Evaluate(Context);
        if (evalRet.IsEmpty()) 
        {
            LastExceptionInfo = FV8Utils::ExceptionToString(Isolate, TryCatch);
            return false;
        }
        else
        {
            ResultInfo.Result.Reset(Isolate, evalRet.ToLocalChecked());
        }
        return true;
#else
        JS_SetModuleLoaderFunc(MainIsolate->runtime_, NULL, js_module_loader, NULL);
        JSContext* ctx = ResultInfo.Context.Get(MainIsolate)->context_;

        JSEngine* JsEngine = FV8Utils::IsolateData<JSEngine>(MainIsolate);
        size_t Length = 0;
        const char* EntryCode = JsEngine->ModuleResolver(Path, JsEngine->Idx, Length);
        JSValue evalRet;
        
		if (JS_IsCompiled(EntryCode))
		{
            evalRet = JS_EvalBuffer(ctx, Path, (uint8_t*)EntryCode, Length);

		} else {
            evalRet = JS_Eval(ctx, EntryCode, strlen(EntryCode), Path, JS_EVAL_TYPE_MODULE);
        }

        v8::Value* val = nullptr;
        if (JS_IsException(evalRet)) {
            MainIsolate->handleException();
            LastExceptionInfo = FV8Utils::ExceptionToString(Isolate, TryCatch);
            return false;

        } else {
            //脚本执行的返回值由HandleScope接管，这可能有需要GC的对象
            val = MainIsolate->Alloc<v8::Value>();
            val->value_ = evalRet;
            ResultInfo.Result.Reset(MainIsolate, v8::Local<v8::Value>(val));
            return true;
            
        }
#endif
    }
    
    bool JSEngine::Eval(const char *Code, const char* Path)
    {
        v8::Isolate* Isolate = MainIsolate;
        v8::Isolate::Scope IsolateScope(Isolate);
        v8::HandleScope HandleScope(Isolate);
        v8::Local<v8::Context> Context = ResultInfo.Context.Get(Isolate);
        v8::Context::Scope ContextScope(Context);

        v8::Local<v8::String> Url = FV8Utils::V8String(Isolate, Path == nullptr ? "" : Path);
        v8::Local<v8::String> Source = FV8Utils::V8String(Isolate, Code);
        v8::ScriptOrigin Origin(Url);
        v8::TryCatch TryCatch(Isolate);

        auto CompiledScript = v8::Script::Compile(Context, Source, &Origin);
        if (CompiledScript.IsEmpty())
        {
            LastExceptionInfo = FV8Utils::ExceptionToString(Isolate, TryCatch);
            return false;
        }
        auto maybeValue = CompiledScript.ToLocalChecked()->Run(Context);//error info output
        if (TryCatch.HasCaught())
        {
            LastExceptionInfo = FV8Utils::ExceptionToString(Isolate, TryCatch);
            return false;
        }

        if (!maybeValue.IsEmpty())
        {
            ResultInfo.Result.Reset(Isolate, maybeValue.ToLocalChecked());
        }

        return true;
    }
}
