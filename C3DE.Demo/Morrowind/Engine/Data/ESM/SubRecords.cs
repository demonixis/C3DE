using System;

namespace TES3Unity.ESM
{
    public class BYDTSubRecord : SubRecord
    {
        public enum BodyPart
        {
            Head = 0,
            Hair = 1,
            Neck = 2,
            Chest = 3,
            Groin = 4,
            Hand = 5,
            Wrist = 6,
            Forearm = 7,
            Upperarm = 8,
            Foot = 9,
            Ankle = 10,
            Knee = 11,
            Upperleg = 12,
            Clavicle = 13,
            Tail = 14
        }

        public enum Flag
        {
            Female = 1, Playabe = 2
        }

        public enum BodyPartType
        {
            Skin = 0, Clothing = 1, Armor = 2
        }

        public byte part;
        public byte vampire;
        public byte flags;
        public byte partType;

        public override void DeserializeData(UnityBinaryReader reader, uint dataSize)
        {
            part = reader.ReadByte();
            vampire = reader.ReadByte();
            flags = reader.ReadByte();
            partType = reader.ReadByte();
        }
    }

    // Common sub-records.
    public class STRVSubRecord : SubRecord
    {
        public string value;

        public override void DeserializeData(UnityBinaryReader reader, uint dataSize)
        {
            value = reader.ReadPossiblyNullTerminatedASCIIString((int)header.dataSize);
        }
    }

    // variable size
    public class INTVSubRecord : SubRecord
    {
        public long value;

        public override void DeserializeData(UnityBinaryReader reader, uint dataSize)
        {
            switch (header.dataSize)
            {
                case 1:
                    value = reader.ReadByte();
                    break;
                case 2:
                    value = reader.ReadLEInt16();
                    break;
                case 4:
                    value = reader.ReadLEInt32();
                    break;
                case 8:
                    value = reader.ReadLEInt64();
                    break;
                default:
                    throw new NotImplementedException("Tried to read an INTV subrecord with an unsupported size (" + header.dataSize.ToString() + ").");
            }
        }
    }

    public class LONGSubRecord : SubRecord
    {
        public long Value { get; private set; }

        public override void DeserializeData(UnityBinaryReader reader, uint dataSize)
        {
            Value = reader.ReadLEInt64();
        }
    }

    public class INTVTwoI32SubRecord : SubRecord
    {
        public int value0, value1;

        public override void DeserializeData(UnityBinaryReader reader, uint dataSize)
        {
            value0 = reader.ReadLEInt32();
            value1 = reader.ReadLEInt32();
        }
    }

    public class INDXSubRecord : INTVSubRecord { }

    public class FLTVSubRecord : SubRecord
    {
        public float value;

        public override void DeserializeData(UnityBinaryReader reader, uint dataSize)
        {
            value = reader.ReadLESingle();
        }
    }

    public class ByteSubRecord : SubRecord
    {
        public byte value;

        public override void DeserializeData(UnityBinaryReader reader, uint dataSize)
        {
            value = reader.ReadByte();
        }
    }

    public class ByteArraySubRecord : SubRecord
    {
        public byte[] Value { get; private set; }

        public override void DeserializeData(UnityBinaryReader reader, uint dataSize)
        {
            Value = reader.ReadBytes((int)dataSize);
        }
    }

    public class Int32SubRecord : SubRecord
    {
        public int value;

        public override void DeserializeData(UnityBinaryReader reader, uint dataSize)
        {
            value = reader.ReadLEInt32();
        }
    }

    public class UInt32SubRecord : SubRecord
    {
        public uint value;

        public override void DeserializeData(UnityBinaryReader reader, uint dataSize)
        {
            value = reader.ReadLEUInt32();
        }
    }

    public class NAMESubRecord : STRVSubRecord { }
    public class FNAMSubRecord : STRVSubRecord { }
    public class SNAMSubRecord : STRVSubRecord { }
    public class ANAMSubRecord : STRVSubRecord { }
    public class ITEXSubRecord : STRVSubRecord { }
    public class ENAMSubRecord : STRVSubRecord { }
    public class BNAMSubRecord : STRVSubRecord { }
    public class CNAMSubRecord : STRVSubRecord { }
    public class SCRISubRecord : STRVSubRecord { }
    public class SCPTSubRecord : STRVSubRecord { }
    public class MODLSubRecord : STRVSubRecord { }
    public class TEXTSubRecord : STRVSubRecord { }

    public class INDXBNAMCNAMGroup
    {
        public INDXSubRecord INDX;
        public BNAMSubRecord BNAM;
        public CNAMSubRecord CNAM;
    }
}
