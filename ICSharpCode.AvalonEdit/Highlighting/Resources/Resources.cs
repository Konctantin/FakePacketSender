using ICSharpCode.AvalonEdit.Folding;
using System.IO;

namespace ICSharpCode.AvalonEdit.Highlighting
{
    public static class Resources
    {
        public static AbstractFoldingStrategy LuaFoldringStrategy
        {
            get { return new RegexFoldingStrategy(); }
        }

        public static IHighlightingDefinition LUA
        {
            get { return HighlightingManager.Instance.GetDefinitionByExtension(".lua"); }
        }

        private static readonly string Prefix = typeof(Resources).FullName + ".";

        public static Stream OpenStream(string name)
        {
            var stream = typeof(Resources).Assembly.GetManifestResourceStream(Prefix + name);
            if (stream == null)
                throw new FileNotFoundException("The resource file '" + name + "' was not found.");
            return stream;
        }

        internal static void RegisterBuiltInHighlightings(HighlightingManager.DefaultHighlightingManager hlm)
        {
            hlm.RegisterHighlighting("LUA", new[] { ".lua" }, "LUA.xshd");
        }
    }
}