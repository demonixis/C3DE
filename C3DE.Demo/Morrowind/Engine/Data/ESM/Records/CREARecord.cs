namespace TES3Unity.ESM.Records
{
    public enum Flags
    {
        Biped = 0x0001,
        Respawn = 0x0002,
        WeaponAndShield = 0x0004,
        None = 0x0008,
        Swims = 0x0010,
        Flies = 0x0020,
        Walks = 0x0040,
        DefaultFlags = 0x0048,
        Essential = 0x0080,
        SkeletonBlood = 0x0400,
        MetalBlood = 0x0800
    }

    public sealed class CREARecord : Record, IIdRecord, IModelRecord
    {
        public string Id { get; private set; }
        public string Model { get; private set; }
        public string Name { get; private set; }
        public CreatureData Data { get; private set; }
        public int Flags { get; private set; }
        public string Script { get; private set; }
        public float Scale { get; set; }

        public override void DeserializeSubRecord(UnityBinaryReader reader, string subRecordName, uint dataSize)
        {
            if (subRecordName == "NAME")
            {
                Id = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "MODL")
            {
                Model = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "FNAM")
            {
                Name = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "NPDT")
            {
                Data = new CreatureData
                {
                    Type = reader.ReadLEInt32(),
                    Level = reader.ReadLEInt32(),
                    Strength = reader.ReadLEInt32(),
                    Intelligence = reader.ReadLEInt32(),
                    Willpower = reader.ReadLEInt32(),
                    Agility = reader.ReadLEInt32(),
                    Speed = reader.ReadLEInt32(),
                    Endurance = reader.ReadLEInt32(),
                    Personality = reader.ReadLEInt32(),
                    Luck = reader.ReadLEInt32(),
                    Health = reader.ReadLEInt32(),
                    SpellPts = reader.ReadLEInt32(),
                    Fatigue = reader.ReadLEInt32(),
                    Soul = reader.ReadLEInt32(),
                    Combat = reader.ReadLEInt32(),
                    Magic = reader.ReadLEInt32(),
                    Stealth = reader.ReadLEInt32(),
                    AttackMin1 = reader.ReadLEInt32(),
                    AttackMax1 = reader.ReadLEInt32(),
                    AttackMin2 = reader.ReadLEInt32(),
                    AttackMax2 = reader.ReadLEInt32(),
                    AttackMin3 = reader.ReadLEInt32(),
                    AttackMax3 = reader.ReadLEInt32(),
                    Gold = reader.ReadLEInt32()
                };
            }
            else if (subRecordName == "FLAG")
            {
                Flags = (int)reader.ReadIntRecord(dataSize);
            }
            else if (subRecordName == "SCRI")
            {
                Script = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "XSCL")
            {
                Scale = reader.ReadLESingle();
            }
            else
            {
                ReadMissingSubRecord(reader, subRecordName, dataSize);
            }
        }
    }
}
