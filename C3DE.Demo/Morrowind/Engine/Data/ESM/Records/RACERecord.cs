using System;

namespace TES3Unity.ESM.Records
{
    [Serializable]
    public enum RaceType
    {
        Breton = 0,
        Imperial,
        Nord,
        Redguard,
        High_Elf,
        Wood_Elf,
        Dark_Elf,
        Orc,
        Argonian,
        Khajiit
    }

    public sealed class RACERecord : Record
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string NPCs { get; private set; }
        public string Description { get; private set; }

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
            else if (subRecordName == "NPCS")
            {
                NPCs = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "DESC")
            {
                Description = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else
            {
                ReadMissingSubRecord(reader, subRecordName, dataSize);
            }
        }
    }
}
