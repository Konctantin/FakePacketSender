using System;

namespace NLua.Method
{
    /// <summary>
    /// Parameter information
    /// </summary>
    internal struct MethodArgs
    {
        /// <summary>
        /// Position of parameter
        /// </summary>
        public int index;

        /// <summary>
        /// Type-conversion function
        /// </summary>
        public ExtractValue extractValue;

        public bool isParamsArray;
        public Type paramsArrayType;
    }
}