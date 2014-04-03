#if !SILVERLIGHT

#endif

namespace NLua
{
    public enum LuaTypes : int
    {
        None            = -1,
        Nil             = 0,
        Boolean         = 1,
        LightUserdata   = 2,
        Number          = 3,
        String          = 4,
        Table           = 5,
        Function        = 6,
        UserData        = 7,
        Thread          = 8,
    }
}