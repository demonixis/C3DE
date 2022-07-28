namespace TES3Unity.ESM.Records
{
    public struct TESVector3
    {
        public float X;
        public float Y;
        public float Z;
    }

    public struct LandVertexHeightFieldData
    {
        public float ReferenceHeight;
        public sbyte[] HeightOffsets;
    }

    public class LANDRecord : Record
    {
        public Vector2i GridCoords { get; private set; }
        public long Data { get; private set; }
        public TESVector3[] VertexNormalData { get; private set; }
        public LandVertexHeightFieldData VertexHeightFieldData { get; private set; }
        public byte[] HeightLODData { get; private set; }
        public TESVector3[] VertexColorData { get; private set; }
        public ushort[] VertexIndiceData { get; private set; }

        public override void DeserializeSubRecord(UnityBinaryReader reader, string subRecordName, uint dataSize)
        {
            if (subRecordName == "INTV")
            {
                GridCoords = new Vector2i
                {
                    X = reader.ReadLEInt32(),
                    Y = reader.ReadLEInt32()
                };
            }
            else if (subRecordName == "DATA_")
            {
                Data = reader.ReadIntRecord(dataSize);
            }
            else if (subRecordName == "VNML_")
            {
                var vertexCount = dataSize / 3;
                VertexNormalData = new TESVector3[vertexCount];

                for (var i = 0; i < vertexCount; i++)
                {
                    VertexNormalData[i] = new TESVector3
                    {
                        X = reader.ReadByte(),
                        Y = reader.ReadByte(),
                        Z = reader.ReadByte()
                    };
                }
            }
            else if (subRecordName == "VHGT")
            {
                var referenceHeight = reader.ReadLESingle();
                var heightOffsetCount = dataSize - 4 - 2 - 1;
                var heightOffsets = new sbyte[heightOffsetCount];

                for (int i = 0; i < heightOffsetCount; i++)
                {
                    heightOffsets[i] = reader.ReadSByte();
                }

                VertexHeightFieldData = new LandVertexHeightFieldData
                {
                    HeightOffsets = heightOffsets,
                    ReferenceHeight = referenceHeight
                };

                // unknown
                reader.ReadLEInt16();
                reader.ReadSByte();
            }
            else if (subRecordName == "WNAM_")
            {
                var heightCount = header.dataSize;
                HeightLODData = new byte[heightCount];

                for (var i = 0; i < heightCount; i++)
                {
                    HeightLODData[i] = reader.ReadByte();
                }
            }
            else if (subRecordName == "VCLR_")
            {
                var vertexCount = dataSize / 3;
                VertexColorData = new TESVector3[vertexCount];

                for (var i = 0; i < vertexCount; i++)
                {
                    VertexColorData[i] = new TESVector3
                    {
                        X = reader.ReadByte(),
                        Y = reader.ReadByte(),
                        Z = reader.ReadByte()
                    };
                }
            }
            else if (subRecordName == "VTEX")
            {
                var indexCount = dataSize / 2;
                VertexIndiceData = new ushort[indexCount];

                for (var i = 0; i < indexCount; i++)
                {
                    VertexIndiceData[i] = reader.ReadLEUInt16();
                }
            }
            else
            {
                ReadMissingSubRecord(reader, subRecordName, dataSize);
            }
        }
    }
}
