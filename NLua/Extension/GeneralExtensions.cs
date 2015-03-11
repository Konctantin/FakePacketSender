namespace NLua.Extensions
{
    /// <summary>
    /// Some random extension stuff.
    /// </summary>
    internal static class CheckNull
    {
        /// <summary>
        /// Determines whether the specified obj is null.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>
        /// 	<c>true</c> if the specified obj is null; otherwise, <c>false</c>.
        /// </returns>
        ///
        public static bool IsNull(object obj)
        {
            return (obj == null);
        }
    }
}