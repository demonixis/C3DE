using System.Collections.Generic;

namespace TES3Unity.ESM.Records
{
    public enum NPCFlags
    {
        Female = 0x0001,
        Essential = 0x0002,
        Respawn = 0x0004,
        None = 0x0008,
        Autocalc = 0x0010,
        BloodSkel = 0x0400,
        BloodMetal = 0x0800
    }

    public enum NPC_DataFlagsType
    {
        Weapon = 0x00001,
        Armor = 0x00002,
        Clothing = 0x00004,
        Books = 0x00008,
        Ingrediant = 0x00010,
        Picks = 0x00020,
        Probes = 0x00040,
        Lights = 0x00080,
        Apparatus = 0x00100,
        Repair = 0x00200,
        Misc = 0x00400,
        Spells = 0x00800,
        MagicItems = 0x01000,
        Potions = 0x02000,
        Training = 0x04000,
        Spellmaking = 0x08000,
        Enchanting = 0x10000,
        RepairItem = 0x20000
    }

    public enum NPC_AIDataFlags
    {
        Weapon = 0x00001,
        Armor = 0x00002,
        Clothing = 0x00004,
        Books = 0x00008,
        Ingrediant = 0x00010,
        Picks = 0x00020,
        Probes = 0x00040,
        Lights = 0x00080,
        Apparatus = 0x00100,
        Repair = 0x00200,
        Misc = 0x00400,
        Spells = 0x00800,
        MagicItems = 0x01000,
        Potions = 0x02000,
        Training = 0x04000,
        Spellmaking = 0x08000,
        Enchanting = 0x10000,
        RepairItem = 0x20000
    }

    public sealed class NPC_Record : Record, IIdRecord, IModelRecord
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Model { get; private set; }
        public string Race { get; private set; }
        public string Faction { get; private set; }
        public string HeadModel { get; private set; }
        public string Class { get; private set; }
        public string HairModel { get; private set; }
        public NPC_Data Data { get; private set; }
        public int Flags { get; private set; }
        public List<NPCOData> Items { get; private set; }
        public List<string> Spells { get; private set; }
        public NPC_AIData AIData { get; private set; }
        public NPC_AIW AIW { get; private set; }
        public NPC_AITravel AITravel { get; private set; }
        public NPC_AIFollow AIFollow { get; private set; }
        public NPC_AIFollow AIEscort { get; private set; }
        public string CellEscortFollow { get; private set; }
        public NPC_AIActivate AIActivate { get; private set; }
        public NPC_CellTravelDestination CellTravelDestination { get; private set; }
        public string PreviousCellDestination { get; private set; }
        public float Scale { get; private set; } = 1.0f;

        public bool IsFemale => Utils.ContainsBitFlags((uint)Flags, (uint)NPCFlags.Female);

        public NPC_Record()
        {
            Items = new List<NPCOData>();
            Spells = new List<string>();
        }

        public static NPC_Record CreateRaw(string name, string race, string faction, string className, string head, string hair, int flags, List<NPCOData> items)
        {
            return new NPC_Record
            {
                Id = name,
                Name = name,
                Race = race,
                Faction = faction,
                Class = className,
                HeadModel = head,
                HairModel = hair,
                Flags = flags,
                Items = items
            };
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
            else if (subRecordName == "MODL")
            {
                Model = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "RNAM")
            {
                Race = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "ANAM")
            {
                Faction = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "BNAM")
            {
                HeadModel = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "CNAM")
            {
                Class = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "KNAM")
            {
                HairModel = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "NPDT")
            {
                if (dataSize == 52)
                {
                    Data = new NPC_Data
                    {
                        Level = reader.ReadLEInt16(),
                        Strength = reader.ReadByte(),
                        Intelligence = reader.ReadByte(),
                        Willpower = reader.ReadByte(),
                        Agility = reader.ReadByte(),
                        Speed = reader.ReadByte(),
                        Endurance = reader.ReadByte(),
                        Personality = reader.ReadByte(),
                        Luck = reader.ReadByte(),
                        Skills = reader.ReadBytes(27),
                        Reputation = reader.ReadByte(),
                        Health = reader.ReadLEInt16(),
                        SpellPts = reader.ReadLEInt16(),
                        Fatigue = reader.ReadLEInt16(),
                        Disposition = reader.ReadByte(),
                        FactionID = reader.ReadByte(),
                        Rank = reader.ReadByte(),
                        Unknown1 = reader.ReadByte(),
                        Gold = reader.ReadLEInt32()
                    };
                }
                else if (dataSize == 12)
                {
                    Data = new NPC_Data
                    {
                        Level = reader.ReadLEInt16(),
                        Disposition = reader.ReadByte(),
                        FactionID = reader.ReadByte(),
                        Rank = reader.ReadByte(),
                        Unknown1 = reader.ReadByte(),
                        unknown2 = reader.ReadByte(),
                        unknown3 = reader.ReadByte(),
                        Gold = reader.ReadLEInt32()
                    };
                }
                else
                {
                    ReadMissingSubRecord(reader, subRecordName, dataSize);
                }
            }
            else if (subRecordName == "FLAG")
            {
                Flags = (int)reader.ReadIntRecord(dataSize);
            }
            else if (subRecordName == "NPCO")
            {
                Items.Add(new NPCOData
                {
                    Count = reader.ReadLEUInt32(),
                    Name = reader.ReadPossiblyNullTerminatedASCIIString(32)
                });
            }
            else if (subRecordName == "NPCS")
            {
                Spells.Add(reader.ReadStringFromChar(32));
            }
            else if (subRecordName == "AIDT")
            {
                AIData = new NPC_AIData
                {
                    Hello = reader.ReadByte(),
                    Unknown1 = reader.ReadByte(),
                    Fight = reader.ReadByte(),
                    Flee = reader.ReadByte(),
                    Alarm = reader.ReadByte(),
                    Unknown2 = reader.ReadByte(),
                    Unknown3 = reader.ReadByte(),
                    Unknown4 = reader.ReadByte(),
                    Flags = reader.ReadLEInt32()
                };
            }
            else if (subRecordName == "AI_W")
            {
                AIW = new NPC_AIW
                {
                    Distance = reader.ReadLEInt16(),
                    Duration = reader.ReadLEInt16(),
                    TimeOfDay = reader.ReadByte(),
                    Idle = reader.ReadBytes(8),
                    Unknow = reader.ReadByte()
                };
            }
            else if (subRecordName == "AI_T")
            {
                AITravel = new NPC_AITravel
                {
                    X = reader.ReadLESingle(),
                    Y = reader.ReadLESingle(),
                    Z = reader.ReadLESingle(),
                    Unknown = reader.ReadLEInt32()
                };
            }
            else if (subRecordName == "AI_F")
            {
                AIFollow = new NPC_AIFollow
                {
                    X = reader.ReadLESingle(),
                    Y = reader.ReadLESingle(),
                    Z = reader.ReadLESingle(),
                    Duration = reader.ReadLEInt16(),
                    Id = reader.ReadStringFromChar(32),
                    Unknown = reader.ReadLEInt16()
                };
            }
            else if (subRecordName == "AI_E")
            {
                AIEscort = new NPC_AIFollow
                {
                    X = reader.ReadLESingle(),
                    Y = reader.ReadLESingle(),
                    Z = reader.ReadLESingle(),
                    Duration = reader.ReadLEInt16(),
                    Id = reader.ReadStringFromChar(32),
                    Unknown = reader.ReadLEInt16()
                };
            }
            else if (subRecordName == "CNDT")
            {
                CellEscortFollow = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "AI_A")
            {
                AIActivate = new NPC_AIActivate
                {
                    Name = reader.ReadStringFromChar(32),
                    Unknown = reader.ReadByte()
                };
            }
            else if (subRecordName == "DODT")
            {
                CellTravelDestination = new NPC_CellTravelDestination
                {
                    X = reader.ReadLESingle(),
                    Y = reader.ReadLESingle(),
                    Z = reader.ReadLESingle(),
                    RotationX = reader.ReadLESingle(),
                    RotationY = reader.ReadLESingle(),
                    RotationZ = reader.ReadLESingle()
                };
            }
            else if (subRecordName == "DNAM")
            {
                PreviousCellDestination = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "XSCL")
            {
                Scale = reader.ReadLESingle();
            }
            else
            {
                ReadMissingSubRecord(reader, subRecordName, dataSize);
            }
        }
    }
}
