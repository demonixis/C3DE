using TES3Unity.ESM;

namespace TES3Unity.ESS.Records
{
    public struct GameDataInfo
    {
        public float Health;
        public float MaxHealth;
        public float Time;
        public float Month;
        public float Day;
        public float Year;
        public string Cell;
        public float DayPassed;
        public string CharacterName;
    }

    public sealed class TES3Record : Record
    {
        public GameDataInfo GameData { get; private set; }
        public string CellName => Convert.RemoveNullChar(GameData.Cell);

        public override void DeserializeSubRecord(UnityBinaryReader reader, string subRecordName, uint dataSize)
        {
            if (subRecordName == "GMDT")
            {
                GameData = new GameDataInfo
                {
                    Health = reader.ReadLESingle(),
                    MaxHealth = reader.ReadLESingle(),
                    Time = reader.ReadLESingle(),
                    Month = reader.ReadLESingle(),
                    Day = reader.ReadLESingle(),
                    Year = reader.ReadLESingle(),
                    Cell = reader.ReadPossiblyNullTerminatedASCIIString(64),
                    DayPassed = reader.ReadLESingle(),
                    CharacterName = reader.ReadPossiblyNullTerminatedASCIIString(32)
                };
            }
            else
            {
                ReadMissingSubRecord(reader, subRecordName, dataSize);
            }
        }
    }
}
