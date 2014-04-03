using System;

namespace NLua.Event
{
    /// <summary>
    /// Event masks for lua hook callback
    /// </summary>
    /// <remarks>
    /// Do not change any of the values because they must match the lua values
    /// </remarks>
    /// <author>Reinhard Ostermeier</author>
    [Flags]
    public enum EventMasks
    {
        LUA_MASKCALL = (1 << EventCodes.LUA_HOOKCALL),
        LUA_MASKRET = (1 << EventCodes.LUA_HOOKRET),
        LUA_MASKLINE = (1 << EventCodes.LUA_HOOKLINE),
        LUA_MASKCOUNT = (1 << EventCodes.LUA_HOOKCOUNT),
        LUA_MASKALL = Int32.MaxValue
    }
}