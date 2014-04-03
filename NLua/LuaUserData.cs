namespace NLua
{
    public class LuaUserData : LuaBase
    {
        public LuaUserData(int reference, Lua interpreter)
        {
            _Reference = reference;
            _Interpreter = interpreter;
        }

        /// <summary>
        /// Indexer for string fields of the userdata
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public object this[string field]
        {
            get
            {
                return _Interpreter.GetObject(_Reference, field);
            }
            set
            {
                _Interpreter.SetObject(_Reference, field, value);
            }
        }

        /// <summary>
        ///
        /// <Indexer for numeric fields of the userdata/summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public object this[object field]
        {
            get
            {
                return _Interpreter.GetObject(_Reference, field);
            }
            set
            {
                _Interpreter.SetObject(_Reference, field, value);
            }
        }

        /// <summary>
        /// Calls the userdata and returns its return values inside an array
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object[] Call(params object[] args)
        {
            return _Interpreter.CallFunction(this, args);
        }

        public override string ToString()
        {
            return "userdata";
        }
    }
}