namespace NLua.Config
{
    public static class Consts
    {
        public const string NLuaDescription = "Bridge between the Lua runtime and the CLR";
#if DEBUG
		public const string NLuaConfiguration = "Debug";
#else
        public const string NLuaConfiguration = "Release";
#endif
        public const string NLuaCompany = "NLua.org";
        public const string NLuaProduct = "NLua";
        public const string NLuaCopyright = "Copyright 2003-2013 Vinicius Jarina , Fabio Mascarenhas, Kevin Hesterm and Megax";
        public const string NLuaTrademark = "MIT license";
        public const string NLuaVersion = "1.3.0";
        public const string NLuaFileVersion = "1.3.0";
    }
}