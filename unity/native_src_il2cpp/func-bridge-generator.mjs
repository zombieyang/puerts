import { default as t, ENDIF, FOR, IF } from "./tte.mjs";


console.log(gen('stsPs'));

function parseSignature(signature, i) {
    if (signature[i] == "s" && signature[i + 1] == "_") {
        // struct
    }
    if (signature[i] == "P") {
        // isbyref
        const [s, ii] = parseSignature(signature, i + 1);
        s.isByRef = true;
        return [s, ii];
    }
    if (signature[i] == 'i' && signature[i + 1] == '1') return [{type: 'sbyte'}, 2]
    if (signature[i] == 'i' && signature[i + 1] == '2') return [{type: 'short'}, 2]
    if (signature[i] == 'i' && signature[i + 1] == '4') return [{type: 'int'}, 2]
    if (signature[i] == 'i' && signature[i + 1] == '8') return [{type: 'long'}, 2]
    if (signature[i] == 'u' && signature[i + 1] == '1') return [{type: 'byte'}, 2]
    if (signature[i] == 'u' && signature[i + 1] == '2') return [{type: 'ushort'}, 2]
    if (signature[i] == 'u' && signature[i + 1] == '4') return [{type: 'uint'}, 2]
    if (signature[i] == 'u' && signature[i + 1] == '8') return [{type: 'ulong'}, 2]
    if (signature[i] == 'r' && signature[i + 1] == '4') return [{type: 'float'}, 2]
    if (signature[i] == 'r' && signature[i + 1] == '8') return [{type: 'double'}, 2]
    if (signature[i] == 'c') return [{type: 'char'}, 1]
    if (signature[i] == 'b') return [{type: 'bool'}, 1]
    if (signature[i] == 's') return [{type: 'string'}, 1]
    if (signature[i] == 'v') return [{type: 'void'}, 1]
    if (signature[i] == 'p') return [{type: 'IntPtr'}, 1]
    if (signature[i] == 'd') return [{type: 'DateTime'}, 1]
    if (signature[i] == 'o') return [{type: 'RefType'}, 1]
    throw new Error('invalid signature' + signature.substr(i));
}
function CSToJSCode(S, paramName) {
    switch (S.type) {
        case 'string':
            return `CSAnyToJsValue(Isolate, Context, (void *)${paramName})`;
        case 'int':
        case 'uint':
            return `v8::Integer::New(Isolate, ${paramName})`
        default:
            throw new Error('invalid signature for CSToJSCode:' + JSON.stringify(S));
    }
}
function JSToCSCode(S, paramName) {
    switch (S.type) {
        case 'string':
            return `v8::String::Utf8Value ${paramName}s(Isolate, Info[0]);  ${paramName} = CStringToCSharpString(*${paramName}s);`;
        case 'int':
        case 'uint':
        default:
            throw new Error('invalid signature for JSToCSCode:' + JSON.stringify(S));
    }
}
function CppTypeName(S) {
    switch (S.type) {
        case 'string':
            return `void*`;
        case 'int':
        case 'uint':
            return 'int32_t';
        case 'float':
        case 'double':
        case 'void':
            return S.type;
        default:
            throw new Error('invalid signature for CppTypeName:' + JSON.stringify(S));
    }
}

function gen(signature) {
    let i = 0;
    let returnS = null;
    let needThis = false;
    let paramS = [];
    
    {
        const [s, ii] = parseSignature(signature, i);
        i += ii;
        if (signature[i] == 't') {
            needThis = true;
            i++;
        }
        returnS = s;
    }
    while (i < signature.length) {
        const [s, ii] = parseSignature(signature, i);
        i += ii;
        paramS.push(s);
    }

const ret = t`
static bool w_${signature}(void* method, MethodPointer methodPointer, const v8::FunctionCallbackInfo<v8::Value>& Info, bool checkArgument, void** typeInfos) {
    v8::Isolate* Isolate = Info.GetIsolate();
    v8::Local<v8::Context> Context = Isolate->GetCurrentContext();
    
    if (checkArgument)
    {
        // TODO
    }
    ${IF(needThis)}
    auto This = puerts::DataTransfer::GetPointerFast<void>(Info.Holder());
    ${ENDIF()}
    ${FOR(paramS, (S, i)=> {
        if (!S.isByRef) {
            t`
    ${CppTypeName(S)} p${i};
    ${JSToCSCode(S, `p${i}`)}
            `
        } else {
            t`
    ${CppTypeName(S)} p${i};
    v8::Local<v8::Object> Outer${i};
    if (Info[${i}]->IsObject())
    {
        Outer${i} = Info[${i}]->ToObject(Context).ToLocalChecked();
        auto Realvalue = Outer${i}->Get(Context, ${i}).ToLocalChecked();
        ${JSToCSCode(S, `p${i}`)}
    }
            `
        }
    })}
    // invoke
    typedef ${CppTypeName(returnS)} (*NativeFuncPtr)(void* ___this, ${paramS.map((S, i)=> `${CppTypeName(S)} p${i}, `).join('')}const void* method);
    ${returnS.type != 'void' ? 'auto ret =' : ""}((NativeFuncPtr)methodPointer)(${needThis ? 'This, ' : ''}${paramS.map((S, i) => `p${i}, `).join('')}method);
    ${FOR(paramS, (S, index) => {
        if (!S.isByRef) return;
        t`
    if (!Outer${index}.IsEmpty())
    {
        Outer${index}->Set(Context, 0, ${CSToJSCode(S, `p${index}`)});
    }`
    })}
    ${IF(returnS.type != 'void')}
    if (ret)
    {
        Info.GetReturnValue().Set(${CSToJSCode(returnS, `ret`)});
    }
    ${ENDIF()}
    return true;
}
    `

    return ret;
}