using System;

namespace NLua.Method
{
    /// <summary>
    /// Static helper methods for Lua tables acting as CLR objects.
    /// </summary>
    public class LuaClassHelper
    {
        /// <summary>
        /// Gets the function called name from the provided table, returning null if it does not exist
        /// </summary>
        /// <param name="luaTable"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static LuaFunction GetTableFunction(LuaTable luaTable, string name)
        {
            if (luaTable == null)
                return null;

            object funcObj = luaTable.RawGet(name);

            if (funcObj is LuaFunction)
                return (LuaFunction)funcObj;
            else
                return null;
        }

        /// <summary>
        /// Calls the provided function with the provided parameters
        /// </summary>
        /// <param name="function"></param>
        /// <param name="args"></param>
        /// <param name="returnTypes"></param>
        /// <param name="inArgs"></param>
        /// <param name="outArgs"></param>
        /// <returns></returns>
        public static object CallFunction(LuaFunction function, object[] args, Type[] returnTypes, object[] inArgs, int[] outArgs)
        {
            // args is the return array of arguments, inArgs is the actual array
            // of arguments passed to the function (with in parameters only), outArgs
            // has the positions of out parameters
            object returnValue;
            int iRefArgs;
            object[] returnValues = function.Call(inArgs, returnTypes);

            if (returnTypes[0] == typeof(void))
            {
                returnValue = null;
                iRefArgs = 0;
            }
            else
            {
                returnValue = returnValues[0];
                iRefArgs = 1;
            }

            for (int i = 0; i < outArgs.Length; i++)
            {
                args[outArgs[i]] = returnValues[iRefArgs];
                iRefArgs++;
            }

            return returnValue;
        }
    }
}