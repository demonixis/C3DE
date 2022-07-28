namespace TES3Unity.ESM.Records
{
    public sealed class LEVIRecord : Record
    {
        public string Id { get; private set; }
        public int Data { get; private set; }
        public byte Chance { get; private set; }
        public int NumberOfItems { get; private set; }
        public string Item { get; private set; }
        public int PCLevel { get; private set; }

        public override void DeserializeSubRecord(UnityBinaryReader reader, string subRecordName, uint dataSize)
        {
            if (subRecordName == "NAME")
            {
                Id = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "DATA")
            {
                Data = (int)reader.ReadIntRecord(dataSize);
            }
            else if (subRecordName == "NNAM")
            {
                Chance = reader.ReadByte();
            }
            else if (subRecordName == "INDX")
            {
                NumberOfItems = (int)reader.ReadIntRecord(dataSize);
            }
            else if (subRecordName == "INAM")
            {
                Item = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "INTV")
            {
                PCLevel = (int)reader.ReadIntRecord(dataSize);
            }
            else
            {
                ReadMissingSubRecord(reader, subRecordName, dataSize);
            }
        }
    }
}
