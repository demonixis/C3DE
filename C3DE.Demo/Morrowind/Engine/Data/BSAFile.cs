using System;
using System.Collections.Generic;
using System.IO;

namespace TES3Unity
{
    public struct FileNameHash
    {
        public uint value1;
        public uint value2;

        public override int GetHashCode()
        {
            return unchecked((int)(value1 ^ value2));
        }
    }

    public class FileMetadata
    {
        public uint size;
        public uint offsetInDataSection;
        public string path;
        public FileNameHash pathHash;
    }

    public class BSAFile : IDisposable
    {
        private UnityBinaryReader _reader;
        private long _hashTablePosition;
        private long _fileDataSectionPostion;

        public byte[] version; // 4 bytes
        public FileMetadata[] fileMetadatas;

        public Dictionary<FileNameHash, FileMetadata> fileMetadataHashTable;

        public VirtualFileSystem.Directory rootDir;

        public bool isAtEOF
        {
            get
            {
                return _reader.BaseStream.Position >= _reader.BaseStream.Length;
            }
        }

        public BSAFile(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            _reader = new UnityBinaryReader(new MemoryStream(bytes));
            ReadMetadata();
        }

        void IDisposable.Dispose()
        {
            Close();
        }

        ~BSAFile()
        {
            Close();
        }

        public void Close()
        {
            _reader?.Close();
            _reader = null;
        }

        /// <summary>
        /// Determines whether the BSA archive contains a file.
        /// </summary>
        public bool ContainsFile(string filePath)
        {
            return fileMetadataHashTable.ContainsKey(HashFilePath(filePath));
        }

        /// <summary>
        /// Loads an archived file's data.
        /// </summary>
        public byte[] LoadFileData(string filePath)
        {
            var hash = HashFilePath(filePath);
            FileMetadata metadata;

            if (fileMetadataHashTable.TryGetValue(hash, out metadata))
            {
                return LoadFileData(metadata);
            }
            else
            {
                throw new FileNotFoundException("Could not find file \"" + filePath + "\" in a BSA file.");
            }
        }

        /// <summary>
        /// Loads an archived file's data.
        /// </summary>
        public byte[] LoadFileData(FileMetadata fileMetadata)
        {
            _reader.BaseStream.Position = _fileDataSectionPostion + fileMetadata.offsetInDataSection;

            return _reader.ReadBytes((int)fileMetadata.size);
        }

        private void ReadMetadata()
        {
            // Read the header.
            version = _reader.ReadBytes(4);
            uint hashTableOffsetFromEndOfHeader = _reader.ReadLEUInt32(); // minus header size (12 bytes)
            uint fileCount = _reader.ReadLEUInt32();

            // Calculate some useful values.
            var headerSize = _reader.BaseStream.Position;
            _hashTablePosition = headerSize + hashTableOffsetFromEndOfHeader;
            _fileDataSectionPostion = _hashTablePosition + (8 * fileCount);

            // Create file metadatas.
            fileMetadatas = new FileMetadata[fileCount];

            for (int i = 0; i < fileCount; i++)
            {
                fileMetadatas[i] = new FileMetadata();
            }

            // Read file sizes/offsets.
            for (int i = 0; i < fileCount; i++)
            {
                fileMetadatas[i].size = _reader.ReadLEUInt32();
                fileMetadatas[i].offsetInDataSection = _reader.ReadLEUInt32();
            }

            // Read filename offsets.
            var filenameOffsets = new uint[fileCount]; // relative offset in filenames section

            for (int i = 0; i < fileCount; i++)
            {
                filenameOffsets[i] = _reader.ReadLEUInt32();
            }

            // Read filenames.
            var filenamesSectionStartPos = _reader.BaseStream.Position;
            var filenameBuffer = new List<byte>(64);

            for (int i = 0; i < fileCount; i++)
            {
                _reader.BaseStream.Position = filenamesSectionStartPos + filenameOffsets[i];

                filenameBuffer.Clear();
                byte curCharAsByte;

                while ((curCharAsByte = _reader.ReadByte()) != 0)
                {
                    filenameBuffer.Add(curCharAsByte);
                }

                fileMetadatas[i].path = System.Text.Encoding.ASCII.GetString(filenameBuffer.ToArray());
            }

            // Read filename hashes.
            _reader.BaseStream.Position = _hashTablePosition;

            for (int i = 0; i < fileCount; i++)
            {
                fileMetadatas[i].pathHash.value1 = _reader.ReadLEUInt32();
                fileMetadatas[i].pathHash.value2 = _reader.ReadLEUInt32();
            }

            // Create the file metadata hash table.
            fileMetadataHashTable = new Dictionary<FileNameHash, FileMetadata>();

            for (int i = 0; i < fileCount; i++)
            {
                fileMetadataHashTable[fileMetadatas[i].pathHash] = fileMetadatas[i];
            }

            // Create a virtual directory tree.
            rootDir = new VirtualFileSystem.Directory();

            foreach (var fileMetadata in fileMetadatas)
            {
                rootDir.CreateDescendantFile(fileMetadata.path);
            }

            // Skip to the file data section.
            _reader.BaseStream.Position = _fileDataSectionPostion;
        }

        private FileNameHash HashFilePath(string filePath)
        {
            filePath = filePath.Replace('/', '\\');
            filePath = filePath.ToLower();

            FileNameHash hash = new FileNameHash();

            uint len = (uint)filePath.Length;
            uint l = (len >> 1);
            int off, i;
            uint sum, temp, n;

            sum = 0;
            off = 0;

            for (i = 0; i < l; i++)
            {
                sum ^= (uint)(filePath[i]) << (off & 0x1F);
                off += 8;
            }

            hash.value1 = sum;

            sum = 0;
            off = 0;

            for (; i < len; i++)
            {
                temp = (uint)(filePath[i]) << (off & 0x1F);
                sum ^= temp;
                n = temp & 0x1F;
                sum = (sum << (32 - (int)n)) | (sum >> (int)n);
                off += 8;
            }

            hash.value2 = sum;

            return hash;
        }
    }
}