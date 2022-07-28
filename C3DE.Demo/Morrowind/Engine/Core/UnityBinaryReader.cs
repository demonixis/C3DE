using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Text;

/// <summary>
/// An improved BinaryReader for Unity.
/// </summary>
public class UnityBinaryReader : IDisposable
{
    private BinaryReader _reader;
    // A buffer for read bytes the size of a decimal variable. Created to minimize allocations. 
    private byte[] _readBuffer = new byte[16];
    private byte[] _emptyArray = new byte[0];

    public Stream BaseStream => _reader.BaseStream;

    public UnityBinaryReader(Stream input)
    {
        _reader = new BinaryReader(input, Encoding.UTF8);
    }

    void IDisposable.Dispose()
    {
        Close();
    }

    ~UnityBinaryReader()
    {
        Close();
    }

    public void Close()
    {
        if (_reader != null)
        {
            _reader.Close();
            _reader = null;
        }
    }

    public byte ReadByte()
    {
        return _reader.ReadByte();
    }

    public sbyte ReadSByte()
    {
        return _reader.ReadSByte();
    }

    public void Read(byte[] buffer, int index, int count)
    {
        _reader.Read(buffer, index, count);
    }

    public byte[] ReadBytes(int count)
    {
        return _reader.ReadBytes(count);
    }

    public byte[] ReadRestOfBytes()
    {
        var remainingByteCount = _reader.BaseStream.Length - _reader.BaseStream.Position;
        return _reader.ReadBytes((int)remainingByteCount);
    }

    public void ReadRestOfBytes(byte[] buffer, int startIndex)
    {
        var remainingByteCount = _reader.BaseStream.Length - _reader.BaseStream.Position;
        _reader.Read(buffer, startIndex, (int)remainingByteCount);
    }

    public string ReadASCIIString(int length)
    {
        return Encoding.ASCII.GetString(_reader.ReadBytes(length));
    }

    public string ReadUnicodeString(int length)
    {
        return Encoding.Unicode.GetString(_reader.ReadBytes(length));
    }

    public string ReadUTF8String(int length)
    {
        return Encoding.UTF8.GetString(_reader.ReadBytes(length));
    }

    public string ReadPossiblyNullTerminatedASCIIString(int lengthIncludingPossibleNullTerminator)
    {
        var bytes = _reader.ReadBytes(lengthIncludingPossibleNullTerminator);
        var count = bytes.Length;

        for (var i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] == 0)
            {
                count = i;
                break;
            }
        }

        // Ignore the null terminator.
        return Encoding.Default.GetString(bytes, 0, count);
    }

    #region Little Endian

    public bool ReadLEBool32()
    {
        return ReadLEUInt32() != 0;
    }

    public ushort ReadLEUInt16()
    {
        _reader.Read(_readBuffer, 0, 2);

        return (ushort)((_readBuffer[1] << 8) | _readBuffer[0]);
    }
    public uint ReadLEUInt32()
    {
        _reader.Read(_readBuffer, 0, 4);

        return ((uint)_readBuffer[3] << 24) | ((uint)_readBuffer[2] << 16) | ((uint)_readBuffer[1] << 8) | _readBuffer[0];
    }
    public ulong ReadLEUInt64()
    {
        _reader.Read(_readBuffer, 0, 8);

        return ((ulong)_readBuffer[7] << 56) | ((ulong)_readBuffer[6] << 48) | ((ulong)_readBuffer[5] << 40) | ((ulong)_readBuffer[4] << 32) | ((ulong)_readBuffer[3] << 24) | ((ulong)_readBuffer[2] << 16) | ((ulong)_readBuffer[1] << 8) | _readBuffer[0];
    }

    public short ReadLEInt16()
    {
        _reader.Read(_readBuffer, 0, 2);

        return BitConverter.ToInt16(_readBuffer, 0);
    }

    public int ReadLEInt32()
    {
        _reader.Read(_readBuffer, 0, 4);

        return BitConverter.ToInt32(_readBuffer, 0);
    }

    public long ReadLEInt64()
    {
        _reader.Read(_readBuffer, 0, 8);

        return BitConverter.ToInt32(_readBuffer, 0);
    }

    public float ReadLESingle()
    {
        _reader.Read(_readBuffer, 0, 4);

        return BitConverter.ToSingle(_readBuffer, 0);
    }

    public double ReadLEDouble()
    {
        _reader.Read(_readBuffer, 0, 8);

        return BitConverter.ToDouble(_readBuffer, 0);
    }

    public byte[] ReadLELength32PrefixedBytes()
    {
        var length = ReadLEInt32();

        if (length <= 0)
        {
            return _emptyArray;
        }

        var count = length;
        return _reader.ReadBytes(count);
    }

    public string ReadLELength32PrefixedASCIIString()
    {
        return Encoding.ASCII.GetString(ReadLELength32PrefixedBytes());
    }

    public Vector2 ReadLEVector2()
    {
        var x = ReadLESingle();
        var y = ReadLESingle();

        return new Vector2(x, y);
    }

    public Vector3 ReadLEVector3()
    {
        var x = ReadLESingle();
        var y = ReadLESingle();
        var z = ReadLESingle();

        return new Vector3(x, y, z);
    }

    public Vector4 ReadLEVector4()
    {
        var x = ReadLESingle();
        var y = ReadLESingle();
        var z = ReadLESingle();
        var w = ReadLESingle();

        return new Vector4(x, y, z, w);
    }

    /// <summary>
    /// Reads a column-major 3x3 matrix but returns a functionally equivalent 4x4 matrix.
    /// </summary>
    public Matrix ReadLEColumnMajorMatrix3x3()
    {
        var matrix = new Matrix();

        for (int columnIndex = 0; columnIndex < 4; columnIndex++)
        {
            for (int rowIndex = 0; rowIndex < 4; rowIndex++)
            {
                // If we're in the 3x3 part of the matrix, read values. Otherwise, use the identity matrix.
                if ((rowIndex <= 2) && (columnIndex <= 2))
                {
                    matrix[rowIndex, columnIndex] = ReadLESingle();
                }
                else
                {
                    matrix[rowIndex, columnIndex] = (rowIndex == columnIndex) ? 1 : 0;
                }
            }
        }

        return matrix;
    }

    /// <summary>
    /// Reads a row-major 3x3 matrix but returns a functionally equivalent 4x4 matrix.
    /// </summary>
    public Matrix ReadLERowMajorMatrix3x3()
    {
        var matrix = new Matrix();

        for (int rowIndex = 0; rowIndex < 4; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < 4; columnIndex++)
            {
                // If we're in the 3x3 part of the matrix, read values. Otherwise, use the identity matrix.
                if ((rowIndex <= 2) && (columnIndex <= 2))
                {
                    matrix[rowIndex, columnIndex] = ReadLESingle();
                }
                else
                {
                    matrix[rowIndex, columnIndex] = (rowIndex == columnIndex) ? 1 : 0;
                }
            }
        }

        return matrix;
    }

    public Matrix ReadLEColumnMajorMatrix4x4()
    {
        var matrix = new Matrix();

        for (int columnIndex = 0; columnIndex < 4; columnIndex++)
        {
            for (int rowIndex = 0; rowIndex < 4; rowIndex++)
            {
                matrix[rowIndex, columnIndex] = ReadLESingle();
            }
        }

        return matrix;
    }

    public Matrix ReadLERowMajorMatrix4x4()
    {
        var matrix = new Matrix();

        for (int rowIndex = 0; rowIndex < 4; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < 4; columnIndex++)
            {
                matrix[rowIndex, columnIndex] = ReadLESingle();
            }
        }

        return matrix;
    }

    public Quaternion ReadLEQuaternionWFirst()
    {
        float w = ReadLESingle();
        float x = ReadLESingle();
        float y = ReadLESingle();
        float z = ReadLESingle();

        return new Quaternion(x, y, z, w);
    }

    public Quaternion ReadLEQuaternionWLast()
    {
        float x = ReadLESingle();
        float y = ReadLESingle();
        float z = ReadLESingle();
        float w = ReadLESingle();

        return new Quaternion(x, y, z, w);
    }

    #endregion

    #region Helpers

    public long ReadIntRecord(uint dataSize)
    {
        if (dataSize == 1)
        {
            return ReadByte();
        }
        else if (dataSize == 2)
        {
            return ReadLEInt16();
        }
        else if (dataSize == 4)
        {
            return ReadLEInt32();
        }
        else if (dataSize == 8)
        {
            return ReadLEInt64();
        }

        _reader.BaseStream.Position += dataSize;

        return 0;
    }

    public float[] ReadDoubleArray(int size)
    {
        var array = new float[size];
        for (var i = 0; i < 4; i++)
        {
            array[i] = ReadLESingle();
        }
        return array;
    }

    public int[] ReadInt32Array(int size)
    {
        var array = new int[size];
        for (var i = 0; i < size; i++)
        {
            array[i] = ReadLEInt32();
        }
        return array;
    }

    public float[] ReadFloatArray(int size)
    {
        var array = new float[size];
        for (var i = 0; i < size; i++)
        {
            array[i] = ReadLESingle();
        }
        return array;
    }

    public string ReadStringFromChar(int size)
    {
        var bytes = ReadBytes(size);

        return Encoding.ASCII.GetString(bytes, 0, bytes.Length);

        var array = new char[size];

        for (var i = 0; i < size; i++)
        {
            array[i] = Convert.ToChar(bytes[i]);
        }

        return Convert.ToString(array);
        //return TES3Unity.Convert.CharToString(array);
    }

    public string ReadStringFromByte(int size)
    {
        var bytes = ReadBytes(size);
        var str = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
        return Convert.ToString(str);
        //return TES3Unity.Convert.RemoveNullChar(str);
    }

    #endregion
}