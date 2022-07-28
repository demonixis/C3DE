namespace TES3Unity.ESM.Records
{
    public sealed class ALCHRecord : Record, IIdRecord, IModelRecord
    {
        public string Id { get; private set; }
        public string Model { get; private set; }
        public string Name { get; private set; }
        public AlchemyData Data { get; private set; }
        public SingleEnchantData Enchantment { get; private set; }
        public string Icon { get; private set; }
        public string Script { get; private set; }

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
            else if (subRecordName == "ALDT")
            {
                Data = new AlchemyData
                {
                    Weight = reader.ReadLESingle(),
                    Value = reader.ReadLEInt32(),
                    AutoCalc = reader.ReadLEInt32()
                };
            }
            else if (subRecordName == "ENAM")
            {
                Enchantment = new SingleEnchantData
                {
                    EffectID = reader.ReadLEInt16(),
                    SkillID = reader.ReadByte(),
                    AttributeID = reader.ReadByte(),
                    RangeType = (EnchantRangeType)reader.ReadLEInt32(),
                    Area = reader.ReadLEInt32(),
                    Duration = reader.ReadLEInt32(),
                    MagMin = reader.ReadLEInt32(),
                    MagMax = reader.ReadLEInt32()
                };
            }
            else if (subRecordName == "TEXT")
            {
                Icon = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "SCRI")
            {
                Script = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else
            {
                ReadMissingSubRecord(reader, subRecordName, dataSize);
            }
        }
    }
}
