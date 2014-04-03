using System.Collections;
using KopiLua;

namespace NLua
{
    /// <summary>
    /// Wrapper class for Lua tables
    /// </summary>
    public class LuaTable : LuaBase
    {
        public LuaTable(int reference, Lua interpreter)
        {
            _Reference = reference;
            _Interpreter = interpreter;
        }

        /// <summary>
        /// Indexer for string fields of the table
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
        /// Indexer for numeric fields of the table
        /// </summary>
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

        public System.Collections.IDictionaryEnumerator GetEnumerator()
        {
            return _Interpreter.GetTableDict(this).GetEnumerator();
        }

        public ICollection Keys
        {
            get { return _Interpreter.GetTableDict(this).Keys; }
        }

        public ICollection Values
        {
            get { return _Interpreter.GetTableDict(this).Values; }
        }

        /// <summary>
        /// Gets an string fields of a table ignoring its metatable, if it exists
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        internal object RawGet(string field)
        {
            return _Interpreter.RawGetObject(_Reference, field);
        }

        /// <summary>
        /// Pushes this table into the Lua stack
        /// </summary>
        /// <param name="luaState"></param>
        internal void Push(LuaState luaState)
        {
            LuaLib.LuaGetRef(luaState, _Reference);
        }

        public override string ToString()
        {
            return "table";
        }
    }
}