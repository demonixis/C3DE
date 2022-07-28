namespace TES3Unity.ESM.Records
{
    public sealed class SCPTRecord : Record
    {
        public ScriptHeader Header { get; private set; }
        public string LocalVariables { get; private set; }
        public byte[] Binary { get; private set; }
        public string Text { get; private set; }

        public override void DeserializeSubRecord(UnityBinaryReader reader, string subRecordName, uint dataSize)
        {
            if (subRecordName == "SCHD")
            {
                Header = new ScriptHeader
                {
                    Name = reader.ReadStringFromChar(32),
                    NumShorts = reader.ReadLEUInt32(),
                    NumLongs = reader.ReadLEUInt32(),
                    NumFloats = reader.ReadLEUInt32(),
                    ScriptDataSize = reader.ReadLEUInt32(),
                    LocalVarSize = reader.ReadLEUInt32()
                };
            }
            else if (subRecordName == "SCVR")
            {
                LocalVariables = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
            else if (subRecordName == "SCDT")
            {
                Binary = reader.ReadBytes((int)dataSize);
            }
            else if (subRecordName == "SCTX")
            {
                Text = reader.ReadPossiblyNullTerminatedASCIIString((int)dataSize);
            }
        }
    }
}
