using C3DE;
using System;
using System.Collections.Generic;


namespace TES3Unity.ESM
{
    public class RecordHeader
    {
        public string name; // 4 bytes
        public uint dataSize;
        public uint unknown0;
        public uint flags;

        public virtual void Deserialize(UnityBinaryReader reader)
        {
            name = reader.ReadASCIIString(4);
            dataSize = reader.ReadLEUInt32();
            unknown0 = reader.ReadLEUInt32();
            flags = reader.ReadLEUInt32();
        }
    }

    public class SubRecordHeader
    {
        public string name; // 4 bytes
        public uint dataSize;

        public virtual void Deserialize(UnityBinaryReader reader)
        {
            name = reader.ReadASCIIString(4);
            dataSize = reader.ReadLEUInt32();
        }
    }

    public abstract class SubRecord
    {
        public SubRecordHeader header;
        public abstract void DeserializeData(UnityBinaryReader reader, uint dataSize);
    }

    public abstract class Record
    {
        private static List<string> MissingRecordLogs = new List<string>();

        #region Deprecated
        public virtual bool NewFetchMethod { get; } = true;
        public RecordHeader header;
        public virtual SubRecord CreateUninitializedSubRecord(string subRecordName, uint dataSize) => null;

        public void DeserializeData(UnityBinaryReader reader)
        {
            var dataEndPos = reader.BaseStream.Position + header.dataSize;

            while (reader.BaseStream.Position < dataEndPos)
            {
                var subRecordStartStreamPosition = reader.BaseStream.Position;

                var subRecordHeader = new SubRecordHeader();
                subRecordHeader.Deserialize(reader);

                var subRecord = CreateUninitializedSubRecord(subRecordHeader.name, subRecordHeader.dataSize);

                // Read or skip the record.
                if (subRecord != null)
                {
                    subRecord.header = subRecordHeader;

                    var subRecordDataStreamPosition = reader.BaseStream.Position;
                    subRecord.DeserializeData(reader, subRecordHeader.dataSize);

                    if (reader.BaseStream.Position != (subRecordDataStreamPosition + subRecord.header.dataSize))
                    {
                        throw new FormatException("Failed reading " + subRecord.header.name + " subrecord at offset " + subRecordStartStreamPosition);
                    }
                }
                else
                {
                    reader.BaseStream.Position += subRecordHeader.dataSize;
                }
            }
        }

        #endregion

        #region New API to deserialize SubRecords

        public abstract void DeserializeSubRecord(UnityBinaryReader reader, string subRecordName, uint dataSize);

        public void ReadMissingSubRecord(UnityBinaryReader reader, string subRecordName, uint dataSize)
        {
            reader.BaseStream.Position += dataSize;

            var log = $"{header.name} have missing subRecord: {subRecordName}";

            if (!MissingRecordLogs.Contains(log))
            {
                MissingRecordLogs.Add(log);

                //if (TES3Engine.LogEnabled)
                {
                    Debug.Log(log);
                }
            }
        }

        public void DeserializeDataNew(UnityBinaryReader reader)
        {
            var dataEndPos = reader.BaseStream.Position + header.dataSize;

            while (reader.BaseStream.Position < dataEndPos)
            {
                var subRecordName = reader.ReadASCIIString(4);
                var dataSize = reader.ReadLEUInt32();

                DeserializeSubRecord(reader, subRecordName, dataSize);
            }
        }

        #endregion
    }
}