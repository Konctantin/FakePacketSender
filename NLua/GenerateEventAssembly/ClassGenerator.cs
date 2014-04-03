using System;
using KopiLua;

namespace NLua
{
    /// <summary>
    /// Class used for generating delegates that get a table from the Lua stack as a an object of a specific type.
    /// </summary>
    internal class ClassGenerator
    {
        private ObjectTranslator translator;
        private Type klass;

        public ClassGenerator(ObjectTranslator objTranslator, Type typeClass)
        {
            translator = objTranslator;
            klass = typeClass;
        }

        public object ExtractGenerated(LuaState luaState, int stackPos)
        {
            return CodeGeneration.Instance.GetClassInstance(klass, translator.GetTable(luaState, stackPos));
        }
    }
}