namespace NLua
{
    /// <summary>
    /// Enumeration of basic lua globals.
    /// </summary>
    public enum LuaEnums : int
    {
        /// <summary>
        /// Option for multiple returns in `lua_pcall' and `lua_call'
        /// </summary>
        MultiRet = -1,

        /// <summary>
        /// Everything is OK.
        /// </summary>
        Ok = 0,

        /// <summary>
        /// Thread status, Ok or Yield
        /// </summary>
        Yield = 1,

        /// <summary>
        /// A Runtime error.
        /// </summary>
        ErrorRun = 2,

        /// <summary>
        /// A syntax error.
        /// </summary>
        ErrorSyntax = 3,

        /// <summary>
        /// A memory allocation error. For such errors, Lua does not call the error handler function.
        /// </summary>
        ErrorMemory = 4,

        /// <summary>
        /// An error in the error handling function.
        /// </summary>
        ErrorError = 5,

        /// <summary>
        /// An extra error for file load errors when using luaL_loadfile.
        /// </summary>
        ErrorFile = 6
    }
}