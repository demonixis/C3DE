using TES3Unity.ESM.Records;

namespace TES3Unity.ESM
{
    public struct AlchemyData
    {
        public float Weight;
        public int Value;
        public int AutoCalc;
    }

    public struct AppaData
    {
        public int Type;
        public float Quality;
        public float Weight;
        public int Value;
    }

    public struct ArmorData
    {
        public ArmorType Type;
        public float Weight;
        public int Value;
        public int Health;
        public int EnchantPts;
        public int Armour;
    }

    public struct ArmorBodyPartGroup
    {
        public BodyPartIndex Index;
        public string MalePartName;
        public string FemalePartName;

        public void SetMalePart(string part) => MalePartName = part;
        public void SetFemalePart(string part) => FemalePartName = part;
    }

    public struct BookData
    {
        public float Weight;
        public int Value;
        public int Scroll;
        public int SkillID;
        public int EnchantPts;
    }

    public struct CreatureData
    {
        public int Type;
        public int Level;
        public int Strength;
        public int Intelligence;
        public int Willpower;
        public int Agility;
        public int Speed;
        public int Endurance;
        public int Personality;
        public int Luck;
        public int Health;
        public int SpellPts;
        public int Fatigue;
        public int Soul;
        public int Combat;
        public int Magic;
        public int Stealth;
        public int AttackMin1;
        public int AttackMax1;
        public int AttackMin2;
        public int AttackMax2;
        public int AttackMin3;
        public int AttackMax3;
        public int Gold;
    }

    public struct NPCOData
    {
        public uint Count;
        public string Name;
    }

    public struct ClassData
    {
        public int AttributeID1;
        public int AttributeID2;
        public int Specialization;
        public int MinorID1;
        public int MajorID1;
        public int MinorID2;
        public int MajorID2;
        public int MinorID3;
        public int MajorID3;
        public int MinorID4;
        public int MajorID4;
        public int MinorID5;
        public int MajorID5;
        public int Flags;
        public int AutoCalcFlags;
    }

    public struct ClothData
    {
        public int Type;
        public float Weight;
        public short Value;
        public short EnchantPts;
    }

    public struct EnchantData
    {
        public EnchantType Type;
        public int EnchantCost;
        public int Charge;
        public int AutoCalc;
    }

    public struct FactionData
    {
        public int AttributeID1;
        public int AttributeID2;
        public FactionRankData[] Data;
        public int[] SkillID;
        public int Unknown1;
        public int Flags;
    }

    public struct FactionRankData
    {
        public int Attribute1;
        public int Attribute2;
        public int FirstSkill;
        public int SecondSkill;
        public int Faction;
    }

    public struct SingleEnchantData
    {
        public short EffectID;
        public byte SkillID;
        public byte AttributeID;
        public EnchantRangeType RangeType;
        public int Area;
        public int Duration;
        public int MagMin;
        public int MagMax;
    }

    public struct IngrediantData
    {
        public float Weight;
        public int Value;
        public int[] EffectID;
        public int[] SkillID;
        public int[] AttributeID;
    }

    public struct LightData
    {
        public float Weight;
        public int Value;
        public int Time;
        public int Radius;
        public byte Red;
        public byte Green;
        public byte Blue;
        public byte NullByte;
        public int Flags;
    }

    public struct LockData
    {
        public float Weight;
        public int Value;
        public float Quality;
        public int Uses;
    }

    public struct MiscData
    {
        public float Weight;
        public uint Value;
        public uint Unknown;
    }

    public struct ProbData
    {
        public float Weight;
        public int Value;
        public float Quality;
        public int Uses;
    }

    public struct WeatherData
    {
        public byte Clear;
        public byte Cloudy;
        public byte Foggy;
        public byte Overcast;
        public byte Rain;
        public byte Thunder;
        public byte Ash;
        public byte Blight;
    }

    public struct MapColorData
    {
        public byte Red;
        public byte Green;
        public byte Blue;
        public byte NullByte;
    }

    public struct MagicEffectData
    {
        public MagicSpellSchool SpellSchool;
        public float BaseCost;
        public int Flags;
        public int Red;
        public int Blue;
        public int Green;
        public float SpeedX;
        public float SizeX;
        public float SizeCap;
    }

    public struct NPC_AIData
    {
        public byte Hello;
        public byte Unknown1;
        public byte Fight;
        public byte Flee;
        public byte Alarm;
        public byte Unknown2;
        public byte Unknown3;
        public byte Unknown4;
        public int Flags;
    }

    public struct NPC_Data
    {
        public short Level;
        public byte Strength;
        public byte Intelligence;
        public byte Willpower;
        public byte Agility;
        public byte Speed;
        public byte Endurance;
        public byte Personality;
        public byte Luck;
        public byte[] Skills;
        public byte Reputation;
        public short Health;
        public short SpellPts;
        public short Fatigue;
        public byte Disposition;
        public byte FactionID;
        public byte Rank;
        public byte Unknown1;
        public int Gold;
        // 12 byte version
        public byte unknown2;
        public byte unknown3;
    }

    public struct NPC_AIW
    {
        public short Distance;
        public short Duration;
        public short TimeOfDay;
        public byte[] Idle;
        public byte Unknow;
    }

    public struct NPC_AITravel
    {
        public float X;
        public float Y;
        public float Z;
        public int Unknown;
    }

    public struct NPC_AIFollow
    {
        public float X;
        public float Y;
        public float Z;
        public short Duration;
        public string Id;
        public short Unknown;
    }

    public struct NPC_AIActivate
    {
        public string Name;
        public byte Unknown;
    }

    public struct NPC_TravelDestination
    {
        public float X;
        public float Y;
        public float Z;
        public float RotationX;
        public float RotationY;
        public float RotationZ;
    }

    public struct NPC_CellTravelDestination
    {
        public float X;
        public float Y;
        public float Z;
        public float RotationX;
        public float RotationY;
        public float RotationZ;
    }

    public struct SoundRecordData
    {
        public string Sound;
        public byte Chance;
    }

    public struct RepaData
    {
        public float Weight;
        public int Value;
        public int Uses;
        public float Quality;
    }

    public struct ScriptHeader
    {
        public string Name;
        public uint NumShorts;
        public uint NumLongs;
        public uint NumFloats;
        public uint ScriptDataSize;
        public uint LocalVarSize;
    }

    public struct SkillData
    {
        public long Attribute;
        public long Specification;
        public float[] UseValue;
    }

    public struct WeaponData
    {
        public float Weight;
        public int Value;
        public short Type;
        public short Health;
        public float Speed;
        public float Reach;
        public short EnchantPts;
        public byte ChopMin;
        public byte ChopMax;
        public byte SlashMin;
        public byte SlashMax;
        public byte ThrustMin;
        public byte ThrustMax;
        public int Flags;
    }
}
