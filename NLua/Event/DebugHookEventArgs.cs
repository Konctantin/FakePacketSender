using System;
using KopiLua;

namespace NLua.Event
{
    /// <summary>
    /// Event args for hook callback event
    /// </summary>
    /// <author>Reinhard Ostermeier</author>
    public class DebugHookEventArgs : EventArgs
    {
        private readonly LuaDebug luaDebug;

        public DebugHookEventArgs(LuaDebug luaDebug)
        {
            this.luaDebug = luaDebug;
        }

        public LuaDebug LuaDebug
        {
            get { return luaDebug; }
        }
    }
}