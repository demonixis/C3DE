namespace TES3Unity.ESM.Records
{
    public sealed class GLOBRecord : Record, IIdRecord
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public float FloatValue { get; private set; }

        public override void DeserializeSubRecord(UnityBinaryReader reader, string subRecordName, uint dataSize)
        {
            if (subRecordName == "NAME")
            {
                Id = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "FNAM")
            {
                Name = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "FLTV")
            {
                FloatValue = reader.ReadLESingle();
            }
            else
            {
                ReadMissingSubRecord(reader, subRecordName, dataSize);
            }
        }
    }
}
