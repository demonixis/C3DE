namespace TES3Unity.ESM.Records
{
    public sealed class SOUNRecord : Record, IIdRecord
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public byte Volume { get; private set; }
        public byte MinRange { get; private set; }
        public byte MaxRange { get; private set; }

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
            else if (subRecordName == "DATA")
            {
                Volume = reader.ReadByte();
                MinRange = reader.ReadByte();
                MaxRange = reader.ReadByte();
            }
            else
            {
                ReadMissingSubRecord(reader, subRecordName, dataSize);
            }
        }
    }
}
