namespace NLua
{
    public class LuaIndexes
    {
        private static int registryIndex = 0;

        public static int Registry
        {
            get
            {
                if (registryIndex != 0)
                    return registryIndex;

                registryIndex = KopiLua.Lua.LuaNetRegistryIndex();

                return registryIndex;
            }
        }
    }
}