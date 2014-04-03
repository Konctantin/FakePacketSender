using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public static class IntelliSienceManager
    {
        public static List<WowApi> IntelliSienceCollection { get; private set; }

        static IntelliSienceManager()
        {
            Reload();
        }

        public static void Reload()
        {
            IntelliSienceCollection = new List<WowApi>();

            if (File.Exists("YanittaApi.xml"))
            {
                using (var fstream = File.OpenRead("YanittaApi.xml"))
                {
                    var serialiser = new XmlSerializer(typeof(List<WowApi>));
                    var list = (List<WowApi>)serialiser.Deserialize(fstream);
                    IntelliSienceCollection.AddRange(list);
                }
            }

            if (File.Exists("WowApi.xml"))
            {
                using (var fstream = File.OpenRead("WowApi.xml"))
                {
                    var serialiser = new XmlSerializer(typeof(List<WowApi>));
                    var list = (List<WowApi>)serialiser.Deserialize(fstream);
                    IntelliSienceCollection.AddRange(list);
                }
            }
        }
    }
}