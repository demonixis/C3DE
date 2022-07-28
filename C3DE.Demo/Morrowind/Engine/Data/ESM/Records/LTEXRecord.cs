namespace TES3Unity.ESM.Records
{
    public sealed class LTEXRecord : Record, IIdRecord
    {
        public string Id { get; private set; }
        public long IntValue { get; private set; }
        public string Texture { get; private set; }

        public override void DeserializeSubRecord(UnityBinaryReader reader, string subRecordName, uint dataSize)
        {
            if (subRecordName == "NAME")
            {
                Id = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "INTV")
            {
                IntValue = reader.ReadIntRecord(dataSize);
            }
            else if (subRecordName == "DATA")
            {
                Texture = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else
            {
                ReadMissingSubRecord(reader, subRecordName, dataSize);
            }
        }
    }
}
