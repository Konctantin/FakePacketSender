using System;
using KopiLua;

namespace NLua
{
    /// <summary>
    /// Class used for generating delegates that get a function from the Lua stack as a delegate of a specific type.
    /// </summary>
    internal class DelegateGenerator
    {
        private ObjectTranslator translator;
        private Type delegateType;

        public DelegateGenerator(ObjectTranslator objectTranslator, Type type)
        {
            translator = objectTranslator;
            delegateType = type;
        }

        public object ExtractGenerated(LuaState luaState, int stackPos)
        {
            return CodeGeneration.Instance.GetDelegate(delegateType, translator.GetFunction(luaState, stackPos));
        }
    }
}