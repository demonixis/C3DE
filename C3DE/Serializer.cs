using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;

namespace C3DE
{
    public sealed class Serializer
    {
        private static Type[] SerializablesTypes = null;

        public static void Serialize(string name, object obj)
        {
            if (SerializablesTypes == null)
                AddTypes(typeof(Engine));

            var dataContractSerializer = new DataContractSerializer(obj.GetType(), SerializablesTypes);
            var xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;
            xmlSettings.IndentChars = "\t";

            using (var xmlWriter = XmlWriter.Create(name, xmlSettings))
            {
                dataContractSerializer.WriteObject(xmlWriter, obj);
                xmlWriter.Close();
            }
        }

        public static object Deserialize(string name, Type type)
        {
            if (SerializablesTypes == null)
                AddTypes(typeof(Engine));

            object result = null;

            var fileStream = new FileStream(name, FileMode.Open);
            var reader = XmlDictionaryReader.CreateTextReader(fileStream, new XmlDictionaryReaderQuotas());
            var serializer = new DataContractSerializer(type, SerializablesTypes);

            result = serializer.ReadObject(reader, true);
            reader.Close();
            fileStream.Close();

            return result;
        }

        public static void AddTypes(Type type)
        {
            AddAssembly(Assembly.GetAssembly(type));
        }

        public static void AddAssembly(Assembly assembly)
        {
            var types = assembly.GetTypes();
            var list = new List<Type>(types.Length);

            foreach (var type in types)
            {
                if (Attribute.IsDefined(type, typeof(DataContractAttribute)) || Attribute.IsDefined(type, typeof(CollectionDataContractAttribute)))
                {
                    if ((SerializablesTypes != null && Array.IndexOf(SerializablesTypes, type) == -1) || SerializablesTypes == null)
                        list.Add(type);
                }
            }

            if (SerializablesTypes != null)
            {
                var aTypes = list.ToArray();
                var sSize = SerializablesTypes.Length;
                var tSize = aTypes.Length;
                Array.Resize<Type>(ref SerializablesTypes, sSize + tSize);
                Array.Copy(aTypes, 0, SerializablesTypes, sSize, tSize);
            }
            else
                SerializablesTypes = list.ToArray();
        }
    }
}
