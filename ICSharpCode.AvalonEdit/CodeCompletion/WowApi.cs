using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    [Serializable]
    public class ApiElement
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        public ApiElement()
        {
            Name = "";
            Description = "";
        }
    }

    [Serializable]
    public class WowApi : ICompletionData
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("RawData")]
        public string RawData { get; set; }

        [XmlAttribute("Priority")]
        public double Priority { get; set; }

        [XmlAttribute("ImageType")]
        public ImageType ImageType { get; set; }

        [XmlElement("Signature")]
        public string Signature { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlElement("ArgumentList")]
        public List<ApiElement> ArgList { get; set; }

        [XmlElement("ReturnList")]
        public List<ApiElement> RetList { get; set; }

        public ImageSource Image
        {
            get
            {
                var path = string.Format(@"pack://application:,,,/ICSharpCode.AvalonEdit;component/Images/{0}.png", this.ImageType);
                return new BitmapImage(new Uri(path));
            }
        }

        public string Content
        {
            get
            {
                if (string.IsNullOrWhiteSpace(RawData))
                    return Name;

                return string.Format("{0} = {1}", Name, RawData);
            }
        }

        public WowApi()
        {
            ArgList = new List<ApiElement>();
            RetList = new List<ApiElement>();
        }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Name);
        }

        public override string ToString()
        {
            var content = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(Signature))
            {
                content.AppendLine(Signature);
                content.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(Description))
            {
                content.AppendLine(Description);
            }

            if (ArgList.Count > 0)
            {
                content.AppendLine();
                content.AppendLine("Arguments:");
                foreach (var par in ArgList)
                {
                    content.AppendFormat("    {0} - {1}", par.Name, par.Description).AppendLine();
                }
            }

            if (RetList.Count > 0)
            {
                content.AppendLine();
                content.AppendLine("Returns:");
                foreach (var par in RetList)
                {
                    content.AppendFormat("    {0} - {1}", par.Name, par.Description).AppendLine();
                }
            }

            return content.ToString().Trim();
        }
    }

    public enum ImageType
    {
        Delegate,
        EnumValue,
        Event,
        ExtensionMethod,
        Field,
        Method,
        Oterator,
        Struct,
    }
}