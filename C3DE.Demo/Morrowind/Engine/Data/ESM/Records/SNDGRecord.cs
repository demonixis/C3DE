namespace TES3Unity.ESM.Records
{
    public enum SoundTypeData
    {
        LeftFoot = 0,
        RightFoot = 1,
        SwimLeft = 2,
        SwimRight = 3,
        Moan = 4,
        Roar = 5,
        Scream = 6,
        Land = 7
    }

    public sealed class SNDGRecord : Record
    {
        public string Id { get; private set; }
        public SoundTypeData SoundType { get; private set; }
        public string Sound { get; private set; }
        public string Creature { get; private set; }

        public override void DeserializeSubRecord(UnityBinaryReader reader, string subRecordName, uint dataSize)
        {
            if (subRecordName == "NAME")
            {
                Id = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "DATA")
            {
                SoundType = (SoundTypeData)reader.ReadLEInt32();
            }
            else if (subRecordName == "SNAM")
            {
                Sound = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "CNAM")
            {
                Creature = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else
            {
                ReadMissingSubRecord(reader, subRecordName, dataSize);
            }
        }
    }
}
