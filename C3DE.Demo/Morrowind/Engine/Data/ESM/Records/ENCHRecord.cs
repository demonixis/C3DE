namespace TES3Unity.ESM.Records
{
    public enum EnchantType
    {
        CastOne = 0,
        CastStrikes = 1,
        CastWhenUsed = 2,
        ConstantEffect = 3
    }

    public enum EnchantRangeType
    {
        Self = 0,
        Touch = 1,
        Target = 2
    }

    public class ENCHRecord : Record
    {
        public string Id { get; private set; }
        public EnchantData Data { get; private set; }
        public SingleEnchantData SingleData { get; private set; }

        public override void DeserializeSubRecord(UnityBinaryReader reader, string subRecordName, uint dataSize)
        {
            if (subRecordName == "NAME")
            {
                Id = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "ENDT")
            {
                Data = new EnchantData
                {
                    Type = (EnchantType)reader.ReadLEInt32(),
                    EnchantCost = reader.ReadLEInt32(),
                    Charge = reader.ReadLEInt32(),
                    AutoCalc = reader.ReadLEInt32(),
                };
            }
            else if (subRecordName == "ENAM")
            {
                SingleData = new SingleEnchantData
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
            else
            {
                ReadMissingSubRecord(reader, subRecordName, dataSize);
            }
        }
    }
}
