namespace TES3Unity.ESM.Records
{
    public class INFORecord : Record
    {
        public override void DeserializeSubRecord(UnityBinaryReader reader, string subRecordName, uint dataSize)
        {
            ReadMissingSubRecord(reader, subRecordName, dataSize);
        }
    }
}
