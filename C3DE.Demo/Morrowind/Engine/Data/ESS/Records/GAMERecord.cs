using TES3Unity.ESM;

namespace TES3Unity.ESS.Records
{
    public sealed class GAMERecord : Record
    {
        public string CellName { get; private set; }

        public override void DeserializeSubRecord(UnityBinaryReader reader, string subRecordName, uint dataSize)
        {
            if (subRecordName == "GMDT")
            {
                CellName = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else
            {
                ReadMissingSubRecord(reader, subRecordName, dataSize);
            }
        }
    }
}
