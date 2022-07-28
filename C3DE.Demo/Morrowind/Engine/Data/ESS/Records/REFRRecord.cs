using TES3Unity.ESM;

namespace TES3Unity.ESS.Records
{
    public class REFRRecord : Record
    {
        public float[] Position { get; private set; }
        public float[] Rotation { get; private set; }

        public override void DeserializeSubRecord(UnityBinaryReader reader, string subRecordName, uint dataSize)
        {
            if (subRecordName == "DATA")
            {
                Position = new float[]
                {
                    reader.ReadLESingle(),
                    reader.ReadLESingle(),
                    reader.ReadLESingle()
                };

                Rotation = new float[]
                {
                    reader.ReadLESingle(),
                    reader.ReadLESingle(),
                    reader.ReadLESingle()
                };
            }
            else
            {
                ReadMissingSubRecord(reader, subRecordName, dataSize);
            }
        }
    }
}
