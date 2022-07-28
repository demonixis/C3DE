namespace TES3Unity.ESM.Records
{
    public enum BodyPart
    {
        Head = 0,
        Hair = 1,
        Neck = 2,
        Chest = 3,
        Groin = 4,
        Hand = 5,
        Wrist = 6,
        Forearm = 7,
        Upperarm = 8,
        Foot = 9,
        Ankle = 10,
        Knee = 11,
        Upperleg = 12,
        Clavicle = 13,
        Tail = 14
    }

    public enum BodyFlags
    {
        Female = 1, Playabe = 2
    }

    public enum BodyPartType
    {
        Skin = 0, Clothing = 1, Armor = 2
    }

    public sealed class BODYRecord : Record, IIdRecord, IModelRecord
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Model { get; private set; }
        public BodyPart Part { get; private set; }
        public byte Vampire { get; private set; }
        public byte Flags { get; private set; }
        public BodyPartType PartType { get; private set; }

        public override void DeserializeSubRecord(UnityBinaryReader reader, string subRecordName, uint dataSize)
        {
            if (subRecordName == "NAME")
            {
                Id = reader.ReadASCIIString((int)dataSize);
            }
            else if (subRecordName == "FNAM")
            {
                Name = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "BYDT")
            {
                Part = (BodyPart)reader.ReadByte();
                Vampire = reader.ReadByte();
                Flags = reader.ReadByte();
                PartType = (BodyPartType)reader.ReadByte();
            }
            else if (subRecordName == "MODL")
            {
                Model = reader.ReadASCIIString((int)dataSize);
            }
            else
            {
                ReadMissingSubRecord(reader, subRecordName, dataSize);
            }
        }
    }
}
