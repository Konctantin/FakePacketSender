using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Markup;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ICSharpCode.AvalonEdit")]
[assembly: AssemblyDescription("WPF-based extensible text editor")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: CLSCompliant(true)]
[assembly: InternalsVisibleTo("ICSharpCode.AvalonEdit.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100F1844BC8CBDC3779B0E5970A30D800668414128135F5D6CD274E726F7C84FBDBF74AD1AD0D9FBA9C0A6CC64C11D0F6A9EDBBE7B32B6F19D8F734E1C130814D40DF54FF9D063CE29BF7AF86B46A69F0E2B910991B52A2AE443648E199A09547E74663CBE1E72E89365034FF53B6A3CE281415CBE7E2DFB5E40E54667F35DC04CA")]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]

[assembly: XmlnsPrefix("http://icsharpcode.net/sharpdevelop/avalonedit", "avalonedit")]
[assembly: XmlnsDefinition("http://icsharpcode.net/sharpdevelop/avalonedit", "ICSharpCode.AvalonEdit")]
[assembly: XmlnsDefinition("http://icsharpcode.net/sharpdevelop/avalonedit", "ICSharpCode.AvalonEdit.Editing")]
[assembly: XmlnsDefinition("http://icsharpcode.net/sharpdevelop/avalonedit", "ICSharpCode.AvalonEdit.Rendering")]
[assembly: XmlnsDefinition("http://icsharpcode.net/sharpdevelop/avalonedit", "ICSharpCode.AvalonEdit.Highlighting")]
[assembly: XmlnsDefinition("http://icsharpcode.net/sharpdevelop/avalonedit", "ICSharpCode.AvalonEdit.Search")]

[assembly: XmlnsPrefix("http://icsharpcode.net/sharpdevelop/notifyicon", "notify")]
[assembly: XmlnsDefinition("http://icsharpcode.net/sharpdevelop/notifyicon", "Microsoft.Windows.Controls")]