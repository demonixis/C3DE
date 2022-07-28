using C3DE;
using System;
using System.Collections.Generic;
using System.IO;
using TES3Unity.ESM.Records;


namespace TES3Unity.ESM
{
    public class ESMFile
    {
        private const int recordHeaderSizeInBytes = 16;

        public List<Record> Records { get; private set; }
        public Dictionary<Type, List<Record>> RecordsByType { get; private set; }
        public Dictionary<string, Record> ObjectsByIDString { get; private set; }
        public Dictionary<Vector2i, CELLRecord> ExteriorCELLRecordsByIndices { get; private set; }
        public Dictionary<Vector2i, LANDRecord> LANDRecordsByIndices { get; private set; }

        public ESMFile(string filePath)
        {
            Records = new List<Record>();
            RecordsByType = new Dictionary<Type, List<Record>>();
            ObjectsByIDString = new Dictionary<string, Record>();
            ExteriorCELLRecordsByIndices = new Dictionary<Vector2i, CELLRecord>();
            LANDRecordsByIndices = new Dictionary<Vector2i, LANDRecord>();

            ReadRecords(filePath);
        }

        public List<Record> GetRecordsOfType<T>() where T : Record
        {
            if (RecordsByType.TryGetValue(typeof(T), out List<Record> records))
            {
                return records;
            }

            return null;
        }

        public List<T> GetRecords<T>() where T : Record
        {
            var type = typeof(T);

            if (!RecordsByType.ContainsKey(type))
            {
                return null;
            }

            var list = new List<T>();
            var collection = RecordsByType[type];

            foreach (var item in collection)
            {
                list.Add((T)item);
            }

            return list;
        }

        public void ReadRecords(string filePath)
        {
            RecordHeader recordHeader = null;
            Record record = null;
            Type type = null;
            IIdRecord nameRecord = null;
            CELLRecord CELL = null;
            LANDRecord LAND = null;
            string recordName = null;

            var bytes = File.ReadAllBytes(filePath);

            using (var reader = new UnityBinaryReader(new MemoryStream(bytes)))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var recordStartStreamPosition = reader.BaseStream.Position;

                    recordHeader = new RecordHeader();
                    recordHeader.Deserialize(reader);

                    recordName = recordHeader.name;
                    record = CreateRecordOfType(recordName);

                    // Read or skip the record.
                    if (record != null)
                    {
                        record.header = recordHeader;

                        var recordDataStreamPosition = reader.BaseStream.Position;

                        if (record.NewFetchMethod)
                        {
                            record.DeserializeDataNew(reader);
                        }
                        else
                        {
                            record.DeserializeData(reader);
                        }

                        if (reader.BaseStream.Position != (recordDataStreamPosition + record.header.dataSize))
                        {
                            throw new FormatException("Failed reading " + recordName + " record at offset " + recordStartStreamPosition + " in " + filePath);
                        }

                        Records.Add(record);

                        type = record.GetType();
                        if (!RecordsByType.ContainsKey(type))
                        {
                            RecordsByType.Add(type, new List<Record>());
                        }

                        RecordsByType[type].Add(record);

                        nameRecord = record as IIdRecord;
                        if (nameRecord != null)
                        {
                            ObjectsByIDString.Add(nameRecord.Id, record);
                        }

                        CELL = record as CELLRecord;
                        if (CELL != null && !CELL.isInterior)
                        {
                            ExteriorCELLRecordsByIndices[CELL.gridCoords] = CELL;
                        }

                        LAND = record as LANDRecord;
                        if (LAND != null)
                        {
                            LANDRecordsByIndices[LAND.GridCoords] = LAND;
                        }
                    }
                    else
                    {
                        // Skip the record.
                        reader.BaseStream.Position += recordHeader.dataSize;
                    }
                }
            }
        }

        public static Record CreateRecordOfType(string recordName)
        {
            switch (recordName)
            {
                case "TES3": return new TES3Record();
                case "GMST": return new GMSTRecord();
                case "GLOB": return new GLOBRecord();
                case "SOUN": return new SOUNRecord();
                case "REGN": return new REGNRecord();
                case "LTEX": return new LTEXRecord();
                case "STAT": return new STATRecord();
                case "DOOR": return new DOORRecord();
                case "MISC": return new MISCRecord();
                case "WEAP": return new WEAPRecord();
                case "CONT": return new CONTRecord();
                case "LIGH": return new LIGHRecord();
                case "ARMO": return new ARMORecord();
                case "CLOT": return new CLOTRecord();
                case "REPA": return new REPARecord();
                case "ACTI": return new ACTIRecord();
                case "APPA": return new APPARecord();
                case "LOCK": return new LOCKRecord();
                case "PROB": return new PROBRecord();
                case "INGR": return new INGRRecord();
                case "BOOK": return new BOOKRecord();
                case "ALCH": return new ALCHRecord();
                case "CELL": return new CELLRecord();
                case "LAND": return new LANDRecord();
                case "CLAS": return new CLASRecord();
                case "FACT": return new FACTRecord();
                case "RACE": return new RACERecord();
                case "SKIL": return new SKILRecord();
                case "MGEF": return new MGEFRecord();
                case "SCPT": return new SCPTRecord();
                case "SPEL": return new SPELRecord();
                case "BODY": return new BODYRecord();
                case "ENCH": return new ENCHRecord();
                case "LEVI": return new LEVIRecord();
                case "LEVC": return new LEVCRecord();
                case "PGRD": return new PGRDRecord();
                case "SNDG": return new SNDGRecord();
                case "DIAL": return new DIALRecord();
                case "INFO": return new INFORecord();
                case "BSGN": return new BSGNRecord();
                case "CREA": return new CREARecord();
                case "NPC_": return new NPC_Record();
                default:
                    Debug.LogWarning("Unsupported ESM record type: " + recordName);
                    return null;
            }
        }
    }
}