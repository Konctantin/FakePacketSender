namespace NLua.Method
{
    /// <summary>
    /// Base wrapper class for Lua function event handlers.
    /// Subclasses that do actual event handling are created at runtime.
    /// </summary>
    public class LuaEventHandler
    {
        public LuaFunction handler = null;

        /// <summary>
        /// CP: Fix provided by Ben Bryant for delegates with one param
        /// link: http://luaforge.net/forum/message.php?msg_id=9318
        /// </summary>
        /// <param name="args"></param>
        public void HandleEvent(object[] args)
        {
            handler.Call(args);
        }
    }
}