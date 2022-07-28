namespace TES3Unity.ESM.Records
{
    public sealed class GMSTRecord : Record, IIdRecord
    {
        public string Id { get; private set; }
        public string StringValue { get; private set; }
        public int IntValue { get; private set; }
        public float FloatValue { get; private set; }

        public override void DeserializeSubRecord(UnityBinaryReader reader, string subRecordName, uint dataSize)
        {
            if (subRecordName == "NAME")
            {
                Id = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "STRV")
            {
                StringValue = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "INTV")
            {
                IntValue = (int)reader.ReadIntRecord(dataSize);
            }
            else if (subRecordName == "FLTV")
            {
                FloatValue = reader.ReadLESingle();
            }
        }
    }
}
