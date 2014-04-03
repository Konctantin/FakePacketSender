using ICSharpCode.AvalonEdit.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

namespace ICSharpCode.AvalonEdit.Highlighting
{
    /// <summary>
    /// Manages a list of syntax highlighting definitions.
    /// </summary>
    /// <remarks>
    /// All memers on this class (including instance members) are thread-safe.
    /// </remarks>
    public class HighlightingManager : IHighlightingDefinitionReferenceResolver
    {
        private sealed class DelayLoadedHighlightingDefinition : IHighlightingDefinition
        {
            private readonly object lockObj = new object();
            private readonly string name;
            private Func<IHighlightingDefinition> lazyLoadingFunction;
            private IHighlightingDefinition definition;
            private Exception storedException;

            public DelayLoadedHighlightingDefinition(string name, Func<IHighlightingDefinition> lazyLoadingFunction)
            {
                this.name = name;
                this.lazyLoadingFunction = lazyLoadingFunction;
            }

            public string Name
            {
                get
                {
                    if (name != null)
                        return name;
                    else
                        return GetDefinition().Name;
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
                                                             Justification = "The exception will be rethrown")]
            private IHighlightingDefinition GetDefinition()
            {
                Func<IHighlightingDefinition> func;
                lock (lockObj)
                {
                    if (this.definition != null)
                        return this.definition;
                    func = this.lazyLoadingFunction;
                }
                Exception exception = null;
                IHighlightingDefinition def = null;
                try
                {
                    using (var busyLock = BusyManager.Enter(this))
                    {
                        if (!busyLock.Success)
                            throw new InvalidOperationException("Tried to create delay-loaded highlighting definition recursively. Make sure the are no cyclic references between the highlighting definitions.");
                        def = func();
                    }
                    if (def == null)
                        throw new InvalidOperationException("Function for delay-loading highlighting definition returned null");
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                lock (lockObj)
                {
                    this.lazyLoadingFunction = null;
                    if (this.definition == null && this.storedException == null)
                    {
                        this.definition = def;
                        this.storedException = exception;
                    }
                    if (this.storedException != null)
                        throw new HighlightingDefinitionInvalidException("Error delay-loading highlighting definition", this.storedException);
                    return this.definition;
                }
            }

            public HighlightingRuleSet MainRuleSet
            {
                get
                {
                    return GetDefinition().MainRuleSet;
                }
            }

            public HighlightingRuleSet GetNamedRuleSet(string name)
            {
                return GetDefinition().GetNamedRuleSet(name);
            }

            public HighlightingColor GetNamedColor(string name)
            {
                return GetDefinition().GetNamedColor(name);
            }

            public IEnumerable<HighlightingColor> NamedHighlightingColors
            {
                get
                {
                    return GetDefinition().NamedHighlightingColors;
                }
            }

            public override string ToString()
            {
                return this.Name;
            }
        }

        private readonly object lockObj = new object();
        private Dictionary<string, IHighlightingDefinition> highlightingsByName = new Dictionary<string, IHighlightingDefinition>();
        private Dictionary<string, IHighlightingDefinition> highlightingsByExtension = new Dictionary<string, IHighlightingDefinition>(StringComparer.OrdinalIgnoreCase);
        private List<IHighlightingDefinition> allHighlightings = new List<IHighlightingDefinition>();

        /// <summary>
        /// Gets a highlighting definition by name.
        /// Returns null if the definition is not found.
        /// </summary>
        public IHighlightingDefinition GetDefinition(string name)
        {
            lock (lockObj)
            {
                IHighlightingDefinition rh;
                if (highlightingsByName.TryGetValue(name, out rh))
                    return rh;
                else
                    return null;
            }
        }

        /// <summary>
        /// Gets a copy of all highlightings.
        /// </summary>
        public ReadOnlyCollection<IHighlightingDefinition> HighlightingDefinitions
        {
            get
            {
                lock (lockObj)
                {
                    return Array.AsReadOnly(allHighlightings.ToArray());
                }
            }
        }

        /// <summary>
        /// Gets a highlighting definition by extension.
        /// Returns null if the definition is not found.
        /// </summary>
        public IHighlightingDefinition GetDefinitionByExtension(string extension)
        {
            lock (lockObj)
            {
                IHighlightingDefinition rh;
                if (highlightingsByExtension.TryGetValue(extension, out rh))
                    return rh;
                else
                    return null;
            }
        }

        /// <summary>
        /// Registers a highlighting definition.
        /// </summary>
        /// <param name="name">The name to register the definition with.</param>
        /// <param name="extensions">The file extensions to register the definition for.</param>
        /// <param name="highlighting">The highlighting definition.</param>
        public void RegisterHighlighting(string name, string[] extensions, IHighlightingDefinition highlighting)
        {
            if (highlighting == null)
                throw new ArgumentNullException("highlighting");

            lock (lockObj)
            {
                allHighlightings.Add(highlighting);
                if (name != null)
                {
                    highlightingsByName[name] = highlighting;
                }
                if (extensions != null)
                {
                    foreach (string ext in extensions)
                    {
                        highlightingsByExtension[ext] = highlighting;
                    }
                }
            }
        }

        /// <summary>
        /// Registers a highlighting definition.
        /// </summary>
        /// <param name="name">The name to register the definition with.</param>
        /// <param name="extensions">The file extensions to register the definition for.</param>
        /// <param name="lazyLoadedHighlighting">A function that loads the highlighting definition.</param>
        public void RegisterHighlighting(string name, string[] extensions, Func<IHighlightingDefinition> lazyLoadedHighlighting)
        {
            if (lazyLoadedHighlighting == null)
                throw new ArgumentNullException("lazyLoadedHighlighting");
            RegisterHighlighting(name, extensions, new DelayLoadedHighlightingDefinition(name, lazyLoadedHighlighting));
        }

        /// <summary>
        /// Gets the default HighlightingManager instance.
        /// The default HighlightingManager comes with built-in highlightings.
        /// </summary>
        public static HighlightingManager Instance
        {
            get
            {
                return DefaultHighlightingManager.Instance;
            }
        }

        internal sealed class DefaultHighlightingManager : HighlightingManager
        {
            public new static readonly DefaultHighlightingManager Instance = new DefaultHighlightingManager();

            public DefaultHighlightingManager()
            {
                Resources.RegisterBuiltInHighlightings(this);
            }

            // Registering a built-in highlighting
            internal void RegisterHighlighting(string name, string[] extensions, string resourceName)
            {
                try
                {
#if DEBUG
                    // don't use lazy-loading in debug builds, show errors immediately
                    Xshd.XshdSyntaxDefinition xshd;
                    using (Stream s = Resources.OpenStream(resourceName))
                    {
                        using (XmlTextReader reader = new XmlTextReader(s))
                        {
                            xshd = Xshd.HighlightingLoader.LoadXshd(reader, false);
                        }
                    }

                    RegisterHighlighting(name, extensions, Xshd.HighlightingLoader.Load(xshd, this));
#else
					RegisterHighlighting(name, extensions, LoadHighlighting(resourceName));
#endif
                }
                catch (HighlightingDefinitionInvalidException ex)
                {
                    throw new InvalidOperationException("The built-in highlighting '" + name + "' is invalid.", ex);
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
                                                             Justification = "LoadHighlighting is used only in release builds")]
            private Func<IHighlightingDefinition> LoadHighlighting(string resourceName)
            {
                Func<IHighlightingDefinition> func = delegate
                {
                    Xshd.XshdSyntaxDefinition xshd;
                    using (Stream s = Resources.OpenStream(resourceName))
                    {
                        using (XmlTextReader reader = new XmlTextReader(s))
                        {
                            // in release builds, skip validating the built-in highlightings
                            xshd = Xshd.HighlightingLoader.LoadXshd(reader, true);
                        }
                    }
                    return Xshd.HighlightingLoader.Load(xshd, this);
                };
                return func;
            }
        }
    }
}