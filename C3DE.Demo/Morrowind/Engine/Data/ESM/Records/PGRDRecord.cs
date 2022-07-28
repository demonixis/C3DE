namespace TES3Unity.ESM.Records
{
    public sealed class PGRDRecord : Record
    {
        public string Id { get; private set; }
        public byte[] PathGrid { get; private set; }
        public byte[] PGRP { get; private set; }
        public byte[] PGRC { get; private set; }

        public override void DeserializeSubRecord(UnityBinaryReader reader, string subRecordName, uint dataSize)
        {
            if (subRecordName == "NAME")
            {
                Id = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "PGRP")
            {
                PGRP = reader.ReadBytes((int)dataSize);
            }
            else if (subRecordName == "PGRC")
            {
                PGRC = reader.ReadBytes((int)dataSize);
            }
            else if (subRecordName == "DATA")
            {
                PathGrid = reader.ReadBytes((int)dataSize);
            }
            else
            {
                ReadMissingSubRecord(reader, subRecordName, dataSize);
            }
        }
    }
}
