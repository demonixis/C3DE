namespace TES3Unity.ESM.Records
{
    public enum TESHeaderType
    {
        ESP = 0, ESM = 1, ESS = 2
    }

    public sealed class TES3Record : Record
    {
        public float Version;
        public TESHeaderType Type;
        public string Company;
        public string Description;
        public uint RecordCount;
        public string Master;
        public long PreviousMasterSize;

        public override void DeserializeSubRecord(UnityBinaryReader reader, string subRecordName, uint dataSize)
        {
            if (subRecordName == "HEDR")
            {
                Version = reader.ReadLESingle();
                Type = (TESHeaderType)reader.ReadLEUInt32();
                Company = reader.ReadASCIIString(32);
                Description = reader.ReadASCIIString(256);
                RecordCount = reader.ReadLEUInt32();
            }
            else if (subRecordName == "MAST")
            {
                Master = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "DATA")
            {
                PreviousMasterSize = reader.ReadLEInt64();
            }
            else
            {
                ReadMissingSubRecord(reader, subRecordName, dataSize);
            }
        }
    }
}
