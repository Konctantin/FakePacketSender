using System.Collections.Generic;
using KopiLua;

namespace NLua
{
    internal class ObjectTranslatorPool
    {
        private static volatile ObjectTranslatorPool instance = new ObjectTranslatorPool();
        private Dictionary<LuaState, ObjectTranslator> translators = new Dictionary<LuaState, ObjectTranslator>();

        public static ObjectTranslatorPool Instance
        {
            get
            {
                return instance;
            }
        }

        public ObjectTranslatorPool()
        {
        }

        public void Add(LuaState luaState, ObjectTranslator translator)
        {
            translators.Add(luaState, translator);
        }

        public ObjectTranslator Find(LuaState luaState)
        {
            if (!translators.ContainsKey(luaState))
                return null;

            return translators[luaState];
        }

        public void Remove(LuaState luaState)
        {
            if (!translators.ContainsKey(luaState))
                return;

            translators.Remove(luaState);
        }
    }
}