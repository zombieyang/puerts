namespace Puerts.ThirdParty 
{
    public class CommonJS
    {
        public static void InjectSupportForCJS(Puerts.JsEnv env)
        {
            if (env.ExecuteModule<bool>("puer-commonjs/require-existed.mjs", "default")) return;
            env.ExecuteModule("puer-commonjs/load.mjs");
            env.ExecuteModule("puer-commonjs/modular.mjs");
        }        
    }
}