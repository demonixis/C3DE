namespace TES3Unity.ESM.Records
{
    public sealed class FACTRecord : Record
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Rank { get; private set; }
        public FactionData Data { get; private set; }
        public string FactionName { get; private set; }
        public int Reaction { get; private set; }

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
            else if (subRecordName == "RNAM")
            {
                Rank = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "FADT")
            {
                var data = new FactionData();
                data.AttributeID1 = reader.ReadLEInt32();
                data.AttributeID2 = reader.ReadLEInt32();

                var rankData = new FactionRankData[10];
                for (var i = 0; i < rankData.Length; i++)
                {
                    rankData[i] = new FactionRankData
                    {
                        Attribute1 = reader.ReadLEInt32(),
                        Attribute2 = reader.ReadLEInt32(),
                        FirstSkill = reader.ReadLEInt32(),
                        SecondSkill = reader.ReadLEInt32(),
                        Faction = reader.ReadLEInt32()
                    };
                }

                data.Data = rankData;
                data.SkillID = reader.ReadInt32Array(6);
                data.Unknown1 = reader.ReadLEInt32();
                data.Flags = reader.ReadLEInt32();

                Data = data;
            }
            else if (subRecordName == "ANAM")
            {
                FactionName = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "INTV")
            {
                Reaction = (int)reader.ReadIntRecord(dataSize);
            }
            else
            {
                ReadMissingSubRecord(reader, subRecordName, dataSize);
            }
        }
    }
}
