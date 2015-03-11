using System;

#if !SILVERLIGHT

using System.Runtime.Serialization;

#endif

namespace NLua.Exceptions
{
    /// <summary>
    /// Exceptions thrown by the Lua runtime
    /// </summary>
#if !SILVERLIGHT

    [Serializable]
#endif
    public class LuaException : Exception
    {
        public LuaException()
        {
        }

        public LuaException(string message)
            : base(message)
        {
        }

        public LuaException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

#if !SILVERLIGHT

        protected LuaException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

#endif
    }
}