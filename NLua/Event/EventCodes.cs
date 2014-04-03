namespace NLua.Event
{
    /// <summary>
    /// Event codes for lua hook function
    /// </summary>
    /// <remarks>
    /// Do not change any of the values because they must match the lua values
    /// </remarks>
    /// <author>Reinhard Ostermeier</author>
    public enum EventCodes
    {
        LUA_HOOKCALL = 0,
        LUA_HOOKRET = 1,
        LUA_HOOKLINE = 2,
        LUA_HOOKCOUNT = 3,
        LUA_HOOKTAILRET = 4
    }
}