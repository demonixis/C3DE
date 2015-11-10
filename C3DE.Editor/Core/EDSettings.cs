using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace C3DE.Editor.Core
{
    [DataContract]
    public class EDSettings
    {
        private static EDSettings _instance = null;

        public static EDSettings current
        {
            get
            {
                if (_instance == null)
                    _instance = new EDSettings();

                return _instance;
            }
        }

        private const string Filepath = "";
        private const string Filename = "Settings.xml";

        [DataMember]
        public bool AllowLockCursor = false;

        [DataMember]
        public string LastProjectPath = "";

        public static void Save()
        {
            var dataContractSerializer = new DataContractSerializer(current.GetType());
            var xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;
            xmlSettings.IndentChars = "\t";

            using (var xmlWriter = XmlWriter.Create(Path.Combine(Filepath, Filename), xmlSettings))
            {
                dataContractSerializer.WriteObject(xmlWriter, current);
                xmlWriter.Close();
            }
        }

        public static void Load()
        {
            var fileStream = new FileStream(Path.Combine(Filepath, Filename), FileMode.Open);
            var reader = XmlDictionaryReader.CreateTextReader(fileStream, new XmlDictionaryReaderQuotas());
            var serializer = new DataContractSerializer(current.GetType());

            var result = serializer.ReadObject(reader, true);
            reader.Close();
            fileStream.Close();

            _instance = (EDSettings)result;
        }
    }
}
