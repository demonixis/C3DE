using System.Collections.Generic;

namespace TES3Unity.ESM.Records
{
    public sealed class REGNRecord : Record, IIdRecord
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public WeatherData Data { get; private set; }
        public string SleepCreature { get; private set; }
        public MapColorData MapColor { get; private set; }
        public List<SoundRecordData> Sounds { get; private set; }

        public REGNRecord()
        {
            Sounds = new List<SoundRecordData>();
        }

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
            else if (subRecordName == "WEAT")
            {
                Data = new WeatherData
                {
                    Clear = reader.ReadByte(),
                    Cloudy = reader.ReadByte(),
                    Foggy = reader.ReadByte(),
                    Overcast = reader.ReadByte(),
                    Rain = reader.ReadByte(),
                    Thunder = reader.ReadByte(),
                    Ash = reader.ReadByte(),
                    Blight = reader.ReadByte()
                };

                // v1.3 ESM files add 2 bytes to WEAT subrecords.
                if (dataSize == 10)
                {
                    reader.ReadByte();
                    reader.ReadByte();
                }
            }
            else if (subRecordName == "BNAM")
            {
                SleepCreature = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "CNAM")
            {
                MapColor = new MapColorData
                {
                    Red = reader.ReadByte(),
                    Green = reader.ReadByte(),
                    Blue = reader.ReadByte(),
                    NullByte = reader.ReadByte()
                };
            }
            else if (subRecordName == "SNAM")
            {
                var data = new SoundRecordData
                {
                    Sound = reader.ReadStringFromByte(32),
                    Chance = reader.ReadByte()
                };

                Sounds.Add(data);
            }
            else
            {
                ReadMissingSubRecord(reader, subRecordName, dataSize);
            }
        }
    }
}
