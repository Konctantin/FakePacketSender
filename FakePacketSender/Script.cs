using ICSharpCode.AvalonEdit.Document;
using System.Xml;
using System.Xml.Serialization;

namespace FakePacketSender
{
    public class Script
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlIgnore]
        public TextDocument Lua { get; set; } = new TextDocument();

        [XmlElement("Lua")]
        public XmlCDataSection _lua
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Lua.Text))
                    return null;
                return new XmlDocument().CreateCDataSection("\n" + Lua.Text + "\n");
            }
            set => Lua = new TextDocument(value?.Value?.Trim() ?? "");
        }
    }
}
