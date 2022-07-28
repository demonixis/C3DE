namespace TES3Unity.ESM.Records
{
    public enum SpellType
    {
        Spell = 0,
        Ability = 1,
        Blight = 2,
        Disease = 3,
        Curse = 4,
        Power = 5
    }

    public enum SpellFlags
    {
        AutoCalc = 0x0001,
        PCStart = 0x0002,
        AlwaysSucceeds = 0x0004
    }

    public sealed class SPELRecord : Record
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public SpellType Type { get; private set; }
        public int SpellCost { get; private set; }
        public int Flags { get; private set; }
        public string Data { get; private set; }

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
            else if (subRecordName == "SPDT")
            {
                Type = (SpellType)reader.ReadLEInt32();
                SpellCost = reader.ReadLEInt32();
                Flags = reader.ReadLEInt32();
            }
            else if (subRecordName == "ENAM")
            {
                Data = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else
            {
                ReadMissingSubRecord(reader, subRecordName, dataSize);
            }
        }
    }
}
