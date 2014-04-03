using System;

namespace KopiLua
{
    using lu_byte = System.Byte;
    using ptrdiff_t = System.Int32;
    using StkId = Lua.LuaTypeValue;

    using TValue = Lua.LuaTypeValue;
    /*
        ** `per thread' state
        */

    public class LuaState : Lua.GCObject
    {
        public lu_byte status;
        /// <summary>
        /// first free slot in the stack 
        /// </summary>
        public StkId top;
        /// <summary>
        /// base of current function
        /// </summary>
        public StkId base_;
        public Lua.GlobalState l_G;
        /// <summary>
        /// call info for current function
        /// </summary>
        public Lua.CallInfo ci;
        /// <summary>
        /// `savedpc' of current function
        /// </summary>
        public InstructionPtr savedpc = new InstructionPtr();
        public StkId stack_last;  /* last free slot in the stack */
        public StkId[] stack;  /* stack base */
        public Lua.CallInfo end_ci;  /* points after end of ci array*/
        public Lua.CallInfo[] base_ci;  /* array of CallInfo's */
        public int stacksize;
        public int size_ci;  /* size of array `base_ci' */

        [CLSCompliant(false)]
        public ushort nCcalls;  /* number of nested C calls */

        [CLSCompliant(false)]
        public ushort baseCcalls;  /* nested C calls when resuming coroutine */

        public lu_byte hookmask;
        public lu_byte allowhook;
        public int basehookcount;
        public int hookcount;
        public LuaHook hook;
        public TValue l_gt = new Lua.LuaTypeValue();  /* table of globals */
        public TValue env = new Lua.LuaTypeValue();  /* temporary place for environments */
        public Lua.GCObject openupval;  /* list of open upvalues in this stack */
        public Lua.GCObject gclist;
        public Lua.LuaLongJmp errorJmp;  /* current error recover point */
        public ptrdiff_t errfunc;  /* current error handling function (stack index) */
    }
}