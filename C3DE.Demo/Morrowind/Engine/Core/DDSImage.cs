/*
 * 
 *  DDSImage.cs - DDS Texture File Reading (Uncompressed, DXT1/2/3/4/5, V8U8) and Writing (Uncompressed Only)
 *  
 *  By Shendare (Jon D. Jackson)
 * 
 *  Rebuilt from Microsoft DDS documentation with the help of the DDSImage.cs reading class from
 *  Lorenzo Consolaro, under the MIT License.  https://code.google.com/p/kprojects/ 
 * 
 *  Portions of this code not covered by another author's or entity's copyright are released under
 *  the Creative Commons Zero (CC0) public domain license.
 *  
 *  To the extent possible under law, Shendare (Jon D. Jackson) has waived all copyright and
 *  related or neighboring rights to this DDSImage class. This work is published from: The United States. 
 *  
 *  You may copy, modify, and distribute the work, even for commercial purposes, without asking permission.
 * 
 *  For more information, read the CC0 summary and full legal text here:
 *  
 *  https://creativecommons.org/publicdomain/zero/1.0/
 * 
 */

using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace C3DE.Morrowind.Engine.Core
{
    public class DDSImage
    {
        #region Constants and Bitflags

        private const uint MAGIC_NUMBER = 0x20534444;

        private const uint DDPF_ALPHAPIXELS = 0x00000001;
        private const uint DDPF_ALPHA = 0x00000002; // Alpha channel only. Deprecated.
        private const uint DDPF_FOURCC = 0x00000004;
        private const uint DDPF_RGB = 0x00000040;
        private const uint DDPF_YUV = 0x00000200;
        private const uint DDPF_LUMINANCE = 0x00020000;

        private const int DDSD_CAPS = 0x00000001;
        private const int DDSD_HEIGHT = 0x00000002;
        private const int DDSD_WIDTH = 0x00000004;
        private const int DDSD_PITCH = 0x00000008;
        private const int DDSD_PIXELFORMAT = 0x00001000;
        private const int DDSD_MIPMAPCOUNT = 0x00020000;
        private const int DDSD_LINEARSIZE = 0x00080000;
        private const int DDSD_DEPTH = 0x00800000;

        private const int DDSCAPS_COMPLEX = 0x00000008;
        private const int DDSCAPS_TEXTURE = 0x00001000;
        private const int DDSCAPS_MIPMAP = 0x00400000;

        private const int DDSCAPS2_CUBEMAP = 0x00000200;
        private const int DDSCAPS2_CUBEMAP_POSITIVEX = 0x00000400;
        private const int DDSCAPS2_CUBEMAP_NEGATIVEX = 0x00000800;
        private const int DDSCAPS2_CUBEMAP_POSITIVEY = 0x00001000;
        private const int DDSCAPS2_CUBEMAP_NEGATIVEY = 0x00002000;
        private const int DDSCAPS2_CUBEMAP_POSITIVEZ = 0x00004000;
        private const int DDSCAPS2_CUBEMAP_NEGATIVEZ = 0x00008000;
        private const int DDSCAPS2_VOLUME = 0x00200000;

        private const uint FOURCC_DXT1 = 0x31545844;
        private const uint FOURCC_DXT2 = 0x32545844;
        private const uint FOURCC_DXT3 = 0x33545844;
        private const uint FOURCC_DXT4 = 0x34545844;
        private const uint FOURCC_DXT5 = 0x35545844;
        private const uint FOURCC_DX10 = 0x30315844;
        private const uint FOURCC_V8U8 = 0X38553856; // Only used internally

        #endregion

        public enum CompressionMode
        {
            Unknown = 0,
            DXT1 = 1,
            DXT2 = 2,
            DXT3 = 3,
            DXT4 = 4,
            DXT5 = 5,
            V8U8 = 7,
            DX10 = 10,
            A1R5G5B5 = 15,
            RGB15 = 15,
            R5G6B5 = 16,
            RGB16 = 16,
            RGB24 = 24,
            RGB32 = 32
        }

        public DDS_HEADER Header;

#pragma warning disable 0649
        public DDS_HEADER_DXT10 Header10;
#pragma warning restore 0649

        public DDS_PIXELFORMAT PixelFormat;

        public Bitmap[] Images;

        public int MipMapCount;

        public CompressionMode Format;
        public string FormatName
        {
            get
            {
                switch (Format)
                {
                    case CompressionMode.A1R5G5B5:
                        return "ARGB16";
                    case CompressionMode.R5G6B5:
                        return "RGB16";
                    default:
                        return Format.ToString();
                }
            }
        }

        public DDSImage() { }

        public static Texture2D LoadTexture2D(Stream inputStream)
        {
            var img = Load(inputStream);
            return GetTexture2DFromBitmap(img.Images[0]);
        }

        public static Texture2D GetTexture2DFromBitmap(Bitmap bitmap)
        {
            var tex = new Texture2D(Application.GraphicsDevice, bitmap.Width, bitmap.Height, false, SurfaceFormat.Color);
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var bufferSize = data.Height * data.Stride;
            var bytes = new byte[bufferSize];

            // copy bitmap data into buffer
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

            // copy our buffer to the texture
            tex.SetData(bytes);

            // unlock the bitmap data
            bitmap.UnlockBits(data);

            return tex;
        }

        public static DDSImage Load(Stream Source)
        {
            DDSImage _dds = new DDSImage();

            using (BinaryReader _data = new BinaryReader(Source))
            {
                if (_data.ReadInt32() != MAGIC_NUMBER)
                {
                    throw new FileFormatException("DDSImage.Load() requires a .dds texture file stream");
                }

                _dds.Format = CompressionMode.Unknown;

                _dds.Header.dwSize = _data.ReadInt32();
                _dds.Header.dwFlags = _data.ReadInt32();
                _dds.Header.dwHeight = _data.ReadInt32();
                _dds.Header.dwWidth = _data.ReadInt32();
                _dds.Header.dwPitchOrLinearSize = _data.ReadInt32();
                _dds.Header.dwDepth = _data.ReadInt32();
                _dds.Header.dwMipMapCount = _data.ReadInt32();

                // Unused Reserved1 Fields
                _data.ReadBytes(11 * sizeof(int));

                // Image Pixel Format
                _dds.PixelFormat.dwSize = _data.ReadUInt32();
                _dds.PixelFormat.dwFlags = _data.ReadUInt32();
                _dds.PixelFormat.dwFourCC = _data.ReadUInt32();
                _dds.PixelFormat.dwRGBBitCount = _data.ReadUInt32();
                _dds.PixelFormat.dwRBitMask = _data.ReadUInt32();
                _dds.PixelFormat.dwGBitMask = _data.ReadUInt32();
                _dds.PixelFormat.dwBBitMask = _data.ReadUInt32();
                _dds.PixelFormat.dwABitMask = _data.ReadUInt32();

                _dds.Header.dwCaps = _data.ReadInt32();
                _dds.Header.dwCaps2 = _data.ReadInt32();
                _dds.Header.dwCaps3 = _data.ReadInt32();
                _dds.Header.dwCaps4 = _data.ReadInt32();
                _dds.Header.dwReserved2 = _data.ReadInt32();

                if ((_dds.PixelFormat.dwFlags & DDPF_FOURCC) != 0)
                {
                    switch (_dds.PixelFormat.dwFourCC)
                    {
                        case FOURCC_DX10:
                            _dds.Format = CompressionMode.DX10;
                            throw new FileFormatException("DX10 textures not supported at this time.");
                        case FOURCC_DXT1:
                            _dds.Format = CompressionMode.DXT1;
                            break;
                        case FOURCC_DXT2:
                            _dds.Format = CompressionMode.DXT2;
                            break;
                        case FOURCC_DXT3:
                            _dds.Format = CompressionMode.DXT3;
                            break;
                        case FOURCC_DXT4:
                            _dds.Format = CompressionMode.DXT4;
                            break;
                        case FOURCC_DXT5:
                            _dds.Format = CompressionMode.DXT5;
                            break;
                        default:
                            switch (_dds.PixelFormat.dwFourCC)
                            {
                                default:
                                    break;
                            }
                            throw new FileFormatException("Unsupported compression format");
                    }
                }

                if ((_dds.PixelFormat.dwFlags & DDPF_FOURCC) == 0)
                {
                    // Uncompressed. How many BPP?

                    bool _supportedBpp = false;

                    switch (_dds.PixelFormat.dwRGBBitCount)
                    {
                        case 16:
                            if (_dds.PixelFormat.dwABitMask == 0)
                            {
                                _dds.Format = CompressionMode.R5G6B5;
                            }
                            else
                            {
                                _dds.Format = CompressionMode.A1R5G5B5;
                            }
                            _supportedBpp = true;
                            break;
                        case 24:
                            _dds.Format = CompressionMode.RGB24;
                            _supportedBpp = true;
                            break;
                        case 32:
                            _dds.Format = CompressionMode.RGB32;
                            _supportedBpp = true;
                            break;
                    }

                    if (!_supportedBpp)
                    {
                        throw new Exception("Only 16, 24, and 32-bit pixel formats are supported for uncompressed textures.");
                    }
                }

                _dds.MipMapCount = 1;
                if ((_dds.Header.dwFlags & DDSD_MIPMAPCOUNT) != 0)
                {
                    _dds.MipMapCount = _dds.Header.dwMipMapCount == 0 ? 1 : _dds.Header.dwMipMapCount;
                }

                _dds.Images = new Bitmap[_dds.MipMapCount];

                int _imageSize;
                int _w = _dds.Header.dwWidth < 0 ? -_dds.Header.dwWidth : _dds.Header.dwWidth;
                int _h = _dds.Header.dwHeight < 0 ? -_dds.Header.dwHeight : _dds.Header.dwHeight;

                // DDS Documentation recommends ignoring the dwLinearOrPitchSize value and calculating on your own.
                if ((_dds.PixelFormat.dwFlags & DDPF_RGB) != 0)
                {
                    // Linear Size

                    _imageSize = _w * _h * ((int)_dds.PixelFormat.dwRGBBitCount + 7) >> 3;
                }
                else
                {
                    // Compressed

                    _imageSize = (_w + 3 >> 2) * (_h + 3 >> 2);

                    switch (_dds.PixelFormat.dwFourCC)
                    {
                        case FOURCC_DXT1:
                            _imageSize <<= 3; // 64 bits color per block
                            break;
                        case FOURCC_DXT2:
                        case FOURCC_DXT3:
                            _imageSize <<= 4; // 64 bits alpha + 64 bits color per block
                            break;
                        case FOURCC_DXT4:
                        case FOURCC_DXT5:
                            _imageSize <<= 4; // 64 bits alpha + 64 bits color per block
                            break;
                    }
                }

                byte[] _imageBits;

                for (int _level = 0; _level < _dds.MipMapCount; _level++)
                {
                    try
                    {
                        _imageBits = _data.ReadBytes(_imageSize >> (_level << 1));

                        int _w2 = _w >> _level;
                        int _h2 = _h >> _level;

                        uint _compressionMode = _dds.PixelFormat.dwFourCC;

                        if ((_dds.PixelFormat.dwFlags & DDPF_RGB) != 0)
                        {
                            _compressionMode = (uint)_dds.Format;
                        }
                        else if ((_dds.PixelFormat.dwFlags & DDPF_FOURCC) == 0 &&
                                  _dds.PixelFormat.dwRGBBitCount == 16 &&
                                  _dds.PixelFormat.dwRBitMask == 0x00FF &&
                                  _dds.PixelFormat.dwGBitMask == 0xFF00 &&
                                  _dds.PixelFormat.dwBBitMask == 0x0000 &&
                                  _dds.PixelFormat.dwABitMask == 0x0000)
                        {
                            _dds.Format = CompressionMode.V8U8;
                            _compressionMode = FOURCC_V8U8;
                        }

                        _dds.Images[_level] = Decompress.Image(_imageBits, _w2, _h2, _compressionMode);
                    }
                    catch
                    {
                        // Unexpected end of file. Perhaps mipmaps weren't fully written to file.
                        // We'll at least provide them with what we've extracted so far.

                        _dds.MipMapCount = _level;

                        if (_level == 0)
                        {
                            _dds.Images = null;

                            throw new FileFormatException("Unable to read pixel data.");
                        }
                        else
                        {
                            Array.Resize(ref _dds.Images, _level);
                        }
                    }
                }
            }

            return _dds;
        }

        private class Decompress
        {
            public static Bitmap Image(byte[] Data, int W, int H, uint CompressionMode)
            {
                Bitmap _img = new Bitmap(W < 4 ? 4 : W, H < 4 ? 4 : H);

                switch (CompressionMode)
                {
                    case 15:
                    case 16:
                    case 24:
                    case 32:
                        return Linear(Data, W, H, CompressionMode);
                }

                // https://msdn.microsoft.com/en-us/library/bb147243%28v=vs.85%29.aspx

                // Gain direct access to the surface's bits
                BitmapData _bits = _img.LockBits(new Rectangle(0, 0, _img.Width, _img.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                IntPtr _bitPtr = _bits.Scan0;

                // Convert byte[] data into 16-bit ushorts per Microsoft design/documentation
                ushort[] _bpp16 = new ushort[Data.Length >> 1];
                Buffer.BlockCopy(Data, 0, _bpp16, 0, Data.Length);

                // Checking for negative stride per documentation just to be safe, but I don't think bottom-up format is supported with DXT1.
                // Converting from bytes to ushorts for _bpp16
                int _stride = (_bits.Stride < 0 ? -_bits.Stride : _bits.Stride) >> 2;

                // Our actual pixel data as it is decompressed
                int[] _pixels = new int[_stride * _bits.Height];

                // Decompress the blocks
                switch (CompressionMode)
                {
                    case FOURCC_DXT1:
                        DXT1(_bpp16, _pixels, W, H, _stride);
                        break;

                    case FOURCC_DXT2:
                    case FOURCC_DXT3:
                        DXT3(_bpp16, _pixels, W, H, _stride);
                        break;

                    case FOURCC_DXT4:
                    case FOURCC_DXT5:
                        DXT5(_bpp16, _pixels, W, H, _stride);
                        break;

                    case FOURCC_V8U8:
                        V8U8(_bpp16, _pixels, W, H, _stride);
                        break;

                    default:
                        _pixels = null;
                        break;
                }

                // Copy our decompressed bits back into the surface
                if (_pixels != null)
                {
                    Marshal.Copy(_pixels, 0, _bitPtr, _stride * _bits.Height);
                }

                // We're done!
                _img.UnlockBits(_bits);

                if (_pixels == null)
                {
                    throw new FileFormatException(string.Format("DDS compression Mode '{0}{0}{0}{0}' not supported.",
                        (char)(CompressionMode & 0xFF),
                        (char)(CompressionMode >> 8 & 0xFF),
                        (char)(CompressionMode >> 16 & 0xFF),
                        (char)(CompressionMode >> 24 & 0xFF)));
                }

                return _img;
            }

            private static void DXT1(ushort[] Data, int[] Pixels, int W, int H, int Stride)
            {
                uint[] _color = new uint[4];
                int _pos = 0;
                int _stride2 = Stride - 4;

                for (int _y = 0; _y < H; _y += 4)
                {
                    for (int _x = 0; _x < W; _x += 4)
                    {
                        ushort _c1 = Data[_pos++];
                        ushort _c2 = Data[_pos++];

                        bool _isAlpha = _c1 < _c2;

                        uint _r1 = (byte)(_c1 >> 11 & 0x1F);
                        uint _g1 = (byte)((_c1 & 0x07E0) >> 5);
                        uint _b1 = (byte)(_c1 & 0x001F);

                        uint _r2 = (byte)(_c2 >> 11 & 0x1F);
                        uint _g2 = (byte)((_c2 & 0x07E0) >> 5);
                        uint _b2 = (byte)(_c2 & 0x001F);

                        _r1 = (_r1 << 3) + (_r1 >> 2);
                        _g1 = (_g1 << 2) + (_g1 >> 4);
                        _b1 = (_b1 << 3) + (_b1 >> 2);

                        _r2 = (_r2 << 3) + (_r2 >> 2);
                        _g2 = (_g2 << 2) + (_g2 >> 4);
                        _b2 = (_b2 << 3) + (_b2 >> 2);

                        uint _a = unchecked((uint)(0xFF << 24));

                        if (_isAlpha)
                        {
                            _color[0] = _a | _r1 << 16 | _g1 << 8 | _b1;
                            _color[1] = _a | _r2 << 16 | _g2 << 8 | _b2;
                            _color[2] = _a | _r1 + _r2 >> 1 << 16 | _g1 + _g2 >> 1 << 8 | _b1 + _b2 >> 1;
                            _color[3] = 0x00000000; // Transparent pixel
                        }
                        else
                        {
                            _color[0] = _a | _r1 << 16 | _g1 << 8 | _b1;
                            _color[1] = _a | _r2 << 16 | _g2 << 8 | _b2;
                            _color[2] = _a | (_r2 * 3 + _r1 * 6) / 9 << 16 | (_g2 * 3 + _g1 * 6) / 9 << 8 | (_b2 * 3 + _b1 * 6) / 9;
                            _color[3] = _a | (_r1 * 3 + _r2 * 6) / 9 << 16 | (_g1 * 3 + _g2 * 6) / 9 << 8 | (_b1 * 3 + _b2 * 6) / 9;
                        }

                        int _pixel = _y * Stride + _x;

                        ushort _code = Data[_pos++];

                        Pixels[_pixel++] = unchecked((int)_color[_code & 0x03]);
                        Pixels[_pixel++] = unchecked((int)_color[_code >> 2 & 0x03]);
                        Pixels[_pixel++] = unchecked((int)_color[_code >> 4 & 0x03]);
                        Pixels[_pixel++] = unchecked((int)_color[_code >> 6 & 0x03]);
                        _pixel += _stride2;

                        Pixels[_pixel++] = unchecked((int)_color[_code >> 8 & 0x03]);
                        Pixels[_pixel++] = unchecked((int)_color[_code >> 10 & 0x03]);
                        Pixels[_pixel++] = unchecked((int)_color[_code >> 12 & 0x03]);
                        Pixels[_pixel++] = unchecked((int)_color[_code >> 14 & 0x03]);
                        _pixel += _stride2;

                        _code = Data[_pos++];

                        Pixels[_pixel++] = unchecked((int)_color[_code & 0x03]);
                        Pixels[_pixel++] = unchecked((int)_color[_code >> 2 & 0x03]);
                        Pixels[_pixel++] = unchecked((int)_color[_code >> 4 & 0x03]);
                        Pixels[_pixel++] = unchecked((int)_color[_code >> 6 & 0x03]);
                        _pixel += _stride2;

                        Pixels[_pixel++] = unchecked((int)_color[_code >> 8 & 0x03]);
                        Pixels[_pixel++] = unchecked((int)_color[_code >> 10 & 0x03]);
                        Pixels[_pixel++] = unchecked((int)_color[_code >> 12 & 0x03]);
                        Pixels[_pixel++] = unchecked((int)_color[_code >> 14 & 0x03]);
                    }
                }
            }

            private static void DXT3(ushort[] Data, int[] Pixels, int W, int H, int Stride)
            {
                ushort[] _alpha = new ushort[4];
                int _pos = 0;
                int _stride2 = Stride - 4;

                for (int _y = 0; _y < H; _y += 4)
                {
                    for (int _x = 0; _x < W; _x += 4)
                    {
                        for (int _i = 0; _i < 4; _i++)
                        {
                            _alpha[_i] = Data[_pos++];
                        }

                        int _pixel = _y * Stride + _x;

                        for (int _i = 0; _i < 4; _i++)
                        {
                            Pixels[_pixel++] = (_alpha[_i] >> 0 & 0x000F) * 255 / 15 << 24;
                            Pixels[_pixel++] = (_alpha[_i] >> 4 & 0x000F) * 255 / 15 << 24;
                            Pixels[_pixel++] = (_alpha[_i] >> 8 & 0x000F) * 255 / 15 << 24;
                            Pixels[_pixel++] = (_alpha[_i] >> 12 & 0x000F) * 255 / 15 << 24;

                            _pixel += _stride2;
                        }

                        uint[] _color = new uint[4];

                        ushort _c1 = Data[_pos++];
                        ushort _c2 = Data[_pos++];

                        uint _r1 = (byte)(_c1 >> 11 & 0x1F);
                        uint _g1 = (byte)((_c1 & 0x07E0) >> 5);
                        uint _b1 = (byte)(_c1 & 0x001F);

                        uint _r2 = (byte)(_c2 >> 11 & 0x1F);
                        uint _g2 = (byte)((_c2 & 0x07E0) >> 5);
                        uint _b2 = (byte)(_c2 & 0x001F);

                        _r1 = (_r1 << 3) + (_r1 >> 2);
                        _g1 = (_g1 << 2) + (_g1 >> 4);
                        _b1 = (_b1 << 3) + (_b1 >> 2);

                        _r2 = (_r2 << 3) + (_r2 >> 2);
                        _g2 = (_g2 << 2) + (_g2 >> 4);
                        _b2 = (_b2 << 3) + (_b2 >> 2);

                        _color[0] = _r1 << 16 | _g1 << 8 | _b1;
                        _color[1] = _r2 << 16 | _g2 << 8 | _b2;
                        _color[2] = (_r2 * 3 + _r1 * 6) / 9 << 16 | (_g2 * 3 + _g1 * 6) / 9 << 8 | (_b2 * 3 + _b1 * 6) / 9;
                        _color[3] = (_r1 * 3 + _r2 * 6) / 9 << 16 | (_g1 * 3 + _g2 * 6) / 9 << 8 | (_b1 * 3 + _b2 * 6) / 9;

                        _pixel = _y * Stride + _x;

                        ushort _code = Data[_pos++];

                        Pixels[_pixel++] |= unchecked((int)_color[_code & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 2 & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 4 & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 6 & 0x03]);
                        _pixel += _stride2;

                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 8 & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 10 & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 12 & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 14 & 0x03]);
                        _pixel += _stride2;

                        _code = Data[_pos++];

                        Pixels[_pixel++] |= unchecked((int)_color[_code & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 2 & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 4 & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 6 & 0x03]);
                        _pixel += _stride2;

                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 8 & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 10 & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 12 & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 14 & 0x03]);
                    }
                }
            }

            private static void DXT5(ushort[] Data, int[] Pixels, int W, int H, int Stride)
            {
                uint[] _color = new uint[4];
                ushort[] _alpha = new ushort[8];
                ushort[] _alphaBits = new ushort[3];
                ushort _alphaCode;
                int _pos = 0;
                int _stride2 = Stride - 4;
                ushort _code;

                for (int _y = 0; _y < H; _y += 4)
                {
                    for (int _x = 0; _x < W; _x += 4)
                    {
                        _alpha[0] = (ushort)(Data[_pos] & 0xFF);
                        _alpha[1] = (ushort)(Data[_pos++] >> 8);

                        if (_alpha[0] > _alpha[1])
                        {
                            // 8 alpha block
                            _alpha[2] = (ushort)((6 * _alpha[0] + 1 * _alpha[1] + 3) / 7);
                            _alpha[3] = (ushort)((5 * _alpha[0] + 2 * _alpha[1] + 3) / 7);
                            _alpha[4] = (ushort)((4 * _alpha[0] + 3 * _alpha[1] + 3) / 7);
                            _alpha[5] = (ushort)((3 * _alpha[0] + 4 * _alpha[1] + 3) / 7);
                            _alpha[6] = (ushort)((2 * _alpha[0] + 5 * _alpha[1] + 3) / 7);
                            _alpha[7] = (ushort)((1 * _alpha[0] + 6 * _alpha[1] + 3) / 7);
                        }
                        else
                        {
                            // 6 alpha block
                            _alpha[2] = (ushort)((4 * _alpha[0] + 1 * _alpha[1] + 2) / 5);
                            _alpha[3] = (ushort)((3 * _alpha[0] + 2 * _alpha[1] + 2) / 5);
                            _alpha[4] = (ushort)((2 * _alpha[0] + 3 * _alpha[1] + 2) / 5);
                            _alpha[5] = (ushort)((1 * _alpha[0] + 4 * _alpha[1] + 2) / 5);
                            _alpha[6] = 0;
                            _alpha[7] = 0xFF;
                        }

                        for (int _i = 0; _i < 3; _i++)
                        {
                            _alphaBits[_i] = Data[_pos++];
                        }

                        int _pixel = _y * Stride + _x;

                        _alphaCode = _alphaBits[0];
                        Pixels[_pixel++] = _alpha[_alphaCode & 0x0007] << 24; _alphaCode >>= 3;
                        Pixels[_pixel++] = _alpha[_alphaCode & 0x0007] << 24; _alphaCode >>= 3;
                        Pixels[_pixel++] = _alpha[_alphaCode & 0x0007] << 24; _alphaCode >>= 3;
                        Pixels[_pixel++] = _alpha[_alphaCode & 0x0007] << 24;
                        _pixel += _stride2;

                        _alphaCode = (ushort)(_alphaBits[0] >> 12 | _alphaBits[1] << 4);
                        Pixels[_pixel++] = _alpha[_alphaCode & 0x0007] << 24; _alphaCode >>= 3;
                        Pixels[_pixel++] = _alpha[_alphaCode & 0x0007] << 24; _alphaCode >>= 3;
                        Pixels[_pixel++] = _alpha[_alphaCode & 0x0007] << 24; _alphaCode >>= 3;
                        Pixels[_pixel++] = _alpha[_alphaCode & 0x0007] << 24;
                        _pixel += _stride2;

                        _alphaCode = (ushort)(_alphaBits[1] >> 8 | _alphaBits[2] << 8);
                        Pixels[_pixel++] = _alpha[_alphaCode & 0x0007] << 24; _alphaCode >>= 3;
                        Pixels[_pixel++] = _alpha[_alphaCode & 0x0007] << 24; _alphaCode >>= 3;
                        Pixels[_pixel++] = _alpha[_alphaCode & 0x0007] << 24; _alphaCode >>= 3;
                        Pixels[_pixel++] = _alpha[_alphaCode & 0x0007] << 24;
                        _pixel += _stride2;

                        _alphaCode = (ushort)(_alphaBits[2] >> 4);
                        Pixels[_pixel++] = _alpha[_alphaCode & 0x0007] << 24; _alphaCode >>= 3;
                        Pixels[_pixel++] = _alpha[_alphaCode & 0x0007] << 24; _alphaCode >>= 3;
                        Pixels[_pixel++] = _alpha[_alphaCode & 0x0007] << 24; _alphaCode >>= 3;
                        Pixels[_pixel++] = _alpha[_alphaCode & 0x0007] << 24;

                        ushort _c1 = Data[_pos++];
                        ushort _c2 = Data[_pos++];

                        uint _r1 = (byte)(_c1 >> 11 & 0x1F);
                        uint _g1 = (byte)((_c1 & 0x07E0) >> 5);
                        uint _b1 = (byte)(_c1 & 0x001F);

                        uint _r2 = (byte)(_c2 >> 11 & 0x1F);
                        uint _g2 = (byte)((_c2 & 0x07E0) >> 5);
                        uint _b2 = (byte)(_c2 & 0x001F);

                        _r1 = (_r1 << 3) + (_r1 >> 2);
                        _g1 = (_g1 << 2) + (_g1 >> 4);
                        _b1 = (_b1 << 3) + (_b1 >> 2);

                        _r2 = (_r2 << 3) + (_r2 >> 2);
                        _g2 = (_g2 << 2) + (_g2 >> 4);
                        _b2 = (_b2 << 3) + (_b2 >> 2);

                        _color[0] = _r1 << 16 | _g1 << 8 | _b1;
                        _color[1] = _r2 << 16 | _g2 << 8 | _b2;
                        _color[2] = (_r2 * 1 + _r1 * 2) / 3 << 16 | (_g2 * 1 + _g1 * 2) / 3 << 8 | (_b2 * 1 + _b1 * 2) / 3;
                        _color[3] = (_r1 * 1 + _r2 * 2) / 3 << 16 | (_g1 * 1 + _g2 * 2) / 3 << 8 | (_b1 * 1 + _b2 * 2) / 3;

                        _pixel = _y * Stride + _x;

                        _code = Data[_pos++];

                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 0 & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 2 & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 4 & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 6 & 0x03]);
                        _pixel += _stride2;

                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 8 & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 10 & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 12 & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 14 & 0x03]);
                        _pixel += _stride2;

                        _code = Data[_pos++];

                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 0 & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 2 & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 4 & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 6 & 0x03]);
                        _pixel += _stride2;

                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 8 & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 10 & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 12 & 0x03]);
                        Pixels[_pixel++] |= unchecked((int)_color[_code >> 14 & 0x03]);
                    }
                }
            }

            private static void V8U8(ushort[] Data, int[] Pixels, int W, int H, int Stride)
            {
                int _pos = 0;

                for (int _y = 0; _y < H; _y++)
                {
                    int _pixel = _y * Stride;

                    for (int _x = 0; _x < W; _x++)
                    {
                        Pixels[_pixel++] = unchecked((int)(Data[_pos++] ^ 0xFFFFFFFF));
                    }
                }
            }

            private static Bitmap Linear(byte[] Data, int W, int H, uint bpp)
            {
                Bitmap _img = new Bitmap(W, H);

                int _a, _r, _g, _b, _c, _pos;

                BitmapData _bits = _img.LockBits(new Rectangle(0, 0, _img.Width, _img.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                IntPtr _bitPtr = _bits.Scan0;

                int _stride = (_bits.Stride < 0 ? -_bits.Stride : _bits.Stride) >> 2;
                int[] _pixels = new int[_stride * _bits.Height];

                switch (bpp)
                {
                    case 15: // CompressionMode.A1R5G5B5:
                        _pos = 0;
                        for (int _y = 0; _y < H; _y++)
                        {
                            int _xy = _y * (_bits.Stride >> 2);

                            for (int _x = 0; _x < W; _x++)
                            {
                                _c = Data[_pos++];
                                _c |= Data[_pos++] << 8;

                                _a = (_c & 0x8000) == 0 ? 0 : 0xFF;
                                _r = ((_c & 0x7C00) >> 10) * 255 / 31;
                                _g = ((_c & 0x03E0) >> 5) * 255 / 31;
                                _b = (_c & 0x001F) * 255 / 31;

                                _pixels[_xy++] = _a << 24 | _r << 16 | _g << 8 | _b;
                            }
                        }
                        break;
                    case 16: // CompressionMode.R5G6B5:
                        _pos = 0;
                        _a = 0xFF << 24;

                        for (int _y = 0; _y < H; _y++)
                        {
                            int _xy = _y * (_bits.Stride >> 2);

                            for (int _x = 0; _x < W; _x++)
                            {
                                _c = Data[_pos++];
                                _c |= Data[_pos++] << 8;

                                _r = ((_c & 0xF800) >> 11) * 255 / 31;
                                _g = ((_c & 0x07E0) >> 5) * 255 / 63;
                                _b = (_c & 0x001F) * 255 / 31;

                                _pixels[_xy++] = _a | _r << 16 | _g << 8 | _b;
                            }
                        }
                        break;
                    case 24: // R8G8B8
                        _a = 0xFF << 24;

                        using (BinaryReader _reader = new BinaryReader(new MemoryStream(Data)))
                        {
                            _reader.BaseStream.Seek(0, SeekOrigin.Begin);

                            for (int _y = 0; _y < _img.Height; _y++)
                            {
                                int _xy = (_bits.Stride < 0 ? _img.Height - _y : _y) * _stride;

                                for (int _x = 0; _x < _img.Width; _x++)
                                {
                                    _b = _reader.ReadByte();
                                    _g = _reader.ReadByte();
                                    _r = _reader.ReadByte();

                                    _pixels[_xy++] = _a | _r << 16 | _g << 8 | _b;
                                }
                            }
                        }
                        break;
                    case 32: // A8R8G8B8
                        int[] _bpp32 = new int[Data.Length >> 2];
                        Buffer.BlockCopy(Data, 0, _bpp32, 0, Data.Length);

                        if (_stride == _img.Width && _bits.Stride > 0)
                        {
                            // Cohesive block of pixel data. No need to go row by row.

                            Array.Copy(_bpp32, _pixels, _pixels.Length);
                        }
                        else
                        {
                            for (int _y = 0; _y < _img.Height; _y++)
                            {
                                // if Stride < 0, image is stored from the bottom up, so we have to invert our _y
                                int _xy1 = (_bits.Stride < 0 ? _img.Height - _y : _y) * _stride;
                                int _xy2 = _y * W;

                                Array.Copy(_bpp32, _xy2, _pixels, _xy1, _stride);
                            }
                        }
                        break;
                }

                Marshal.Copy(_pixels, 0, _bitPtr, _stride * _bits.Height);

                _img.UnlockBits(_bits);

                return _img;
            }
        }        
    }

    public struct DDS_PIXELFORMAT
    {
        public uint dwSize;
        public uint dwFlags;
        public uint dwFourCC;
        public uint dwRGBBitCount;
        public uint dwRBitMask;
        public uint dwGBitMask;
        public uint dwBBitMask;
        public uint dwABitMask;
    }

    public struct DDS_HEADER
    {
        public int dwSize;
        public int dwFlags;
        public int dwHeight;
        public int dwWidth;
        public int dwPitchOrLinearSize;
        public int dwDepth;
        public int dwMipMapCount;
        public int[] dwReserved1;
        public int dwCaps;
        public int dwCaps2;
        public int dwCaps3;
        public int dwCaps4;
        public int dwReserved2;
    }

    #region DX10 - Not currently implemented.

    public struct DDS_HEADER_DXT10
    {
        public DXGI_FORMAT dxgiFormat;
        public D3D10_RESOURCE_DIMENSION resourceDimension;
        public uint miscFlag;
        public uint arraySize;
        public uint reserved;
    }

    public enum DXGI_FORMAT : uint
    {
        DXGI_FORMAT_UNKNOWN = 0,
        DXGI_FORMAT_R32G32B32A32_TYPELESS = 1,
        DXGI_FORMAT_R32G32B32A32_FLOAT = 2,
        DXGI_FORMAT_R32G32B32A32_UINT = 3,
        DXGI_FORMAT_R32G32B32A32_SINT = 4,
        DXGI_FORMAT_R32G32B32_TYPELESS = 5,
        DXGI_FORMAT_R32G32B32_FLOAT = 6,
        DXGI_FORMAT_R32G32B32_UINT = 7,
        DXGI_FORMAT_R32G32B32_SINT = 8,
        DXGI_FORMAT_R16G16B16A16_TYPELESS = 9,
        DXGI_FORMAT_R16G16B16A16_FLOAT = 10,
        DXGI_FORMAT_R16G16B16A16_UNORM = 11,
        DXGI_FORMAT_R16G16B16A16_UINT = 12,
        DXGI_FORMAT_R16G16B16A16_SNORM = 13,
        DXGI_FORMAT_R16G16B16A16_SINT = 14,
        DXGI_FORMAT_R32G32_TYPELESS = 15,
        DXGI_FORMAT_R32G32_FLOAT = 16,
        DXGI_FORMAT_R32G32_UINT = 17,
        DXGI_FORMAT_R32G32_SINT = 18,
        DXGI_FORMAT_R32G8X24_TYPELESS = 19,
        DXGI_FORMAT_D32_FLOAT_S8X24_UINT = 20,
        DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS = 21,
        DXGI_FORMAT_X32_TYPELESS_G8X24_UINT = 22,
        DXGI_FORMAT_R10G10B10A2_TYPELESS = 23,
        DXGI_FORMAT_R10G10B10A2_UNORM = 24,
        DXGI_FORMAT_R10G10B10A2_UINT = 25,
        DXGI_FORMAT_R11G11B10_FLOAT = 26,
        DXGI_FORMAT_R8G8B8A8_TYPELESS = 27,
        DXGI_FORMAT_R8G8B8A8_UNORM = 28,
        DXGI_FORMAT_R8G8B8A8_UNORM_SRGB = 29,
        DXGI_FORMAT_R8G8B8A8_UINT = 30,
        DXGI_FORMAT_R8G8B8A8_SNORM = 31,
        DXGI_FORMAT_R8G8B8A8_SINT = 32,
        DXGI_FORMAT_R16G16_TYPELESS = 33,
        DXGI_FORMAT_R16G16_FLOAT = 34,
        DXGI_FORMAT_R16G16_UNORM = 35,
        DXGI_FORMAT_R16G16_UINT = 36,
        DXGI_FORMAT_R16G16_SNORM = 37,
        DXGI_FORMAT_R16G16_SINT = 38,
        DXGI_FORMAT_R32_TYPELESS = 39,
        DXGI_FORMAT_D32_FLOAT = 40,
        DXGI_FORMAT_R32_FLOAT = 41,
        DXGI_FORMAT_R32_UINT = 42,
        DXGI_FORMAT_R32_SINT = 43,
        DXGI_FORMAT_R24G8_TYPELESS = 44,
        DXGI_FORMAT_D24_UNORM_S8_UINT = 45,
        DXGI_FORMAT_R24_UNORM_X8_TYPELESS = 46,
        DXGI_FORMAT_X24_TYPELESS_G8_UINT = 47,
        DXGI_FORMAT_R8G8_TYPELESS = 48,
        DXGI_FORMAT_R8G8_UNORM = 49,
        DXGI_FORMAT_R8G8_UINT = 50,
        DXGI_FORMAT_R8G8_SNORM = 51,
        DXGI_FORMAT_R8G8_SINT = 52,
        DXGI_FORMAT_R16_TYPELESS = 53,
        DXGI_FORMAT_R16_FLOAT = 54,
        DXGI_FORMAT_D16_UNORM = 55,
        DXGI_FORMAT_R16_UNORM = 56,
        DXGI_FORMAT_R16_UINT = 57,
        DXGI_FORMAT_R16_SNORM = 58,
        DXGI_FORMAT_R16_SINT = 59,
        DXGI_FORMAT_R8_TYPELESS = 60,
        DXGI_FORMAT_R8_UNORM = 61,
        DXGI_FORMAT_R8_UINT = 62,
        DXGI_FORMAT_R8_SNORM = 63,
        DXGI_FORMAT_R8_SINT = 64,
        DXGI_FORMAT_A8_UNORM = 65,
        DXGI_FORMAT_R1_UNORM = 66,
        DXGI_FORMAT_R9G9B9E5_SHAREDEXP = 67,
        DXGI_FORMAT_R8G8_B8G8_UNORM = 68,
        DXGI_FORMAT_G8R8_G8B8_UNORM = 69,
        DXGI_FORMAT_BC1_TYPELESS = 70,
        DXGI_FORMAT_BC1_UNORM = 71,
        DXGI_FORMAT_BC1_UNORM_SRGB = 72,
        DXGI_FORMAT_BC2_TYPELESS = 73,
        DXGI_FORMAT_BC2_UNORM = 74,
        DXGI_FORMAT_BC2_UNORM_SRGB = 75,
        DXGI_FORMAT_BC3_TYPELESS = 76,
        DXGI_FORMAT_BC3_UNORM = 77,
        DXGI_FORMAT_BC3_UNORM_SRGB = 78,
        DXGI_FORMAT_BC4_TYPELESS = 79,
        DXGI_FORMAT_BC4_UNORM = 80,
        DXGI_FORMAT_BC4_SNORM = 81,
        DXGI_FORMAT_BC5_TYPELESS = 82,
        DXGI_FORMAT_BC5_UNORM = 83,
        DXGI_FORMAT_BC5_SNORM = 84,
        DXGI_FORMAT_B5G6R5_UNORM = 85,
        DXGI_FORMAT_B5G5R5A1_UNORM = 86,
        DXGI_FORMAT_B8G8R8A8_UNORM = 87,
        DXGI_FORMAT_B8G8R8X8_UNORM = 88,
        DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM = 89,
        DXGI_FORMAT_B8G8R8A8_TYPELESS = 90,
        DXGI_FORMAT_B8G8R8A8_UNORM_SRGB = 91,
        DXGI_FORMAT_B8G8R8X8_TYPELESS = 92,
        DXGI_FORMAT_B8G8R8X8_UNORM_SRGB = 93,
        DXGI_FORMAT_BC6H_TYPELESS = 94,
        DXGI_FORMAT_BC6H_UF16 = 95,
        DXGI_FORMAT_BC6H_SF16 = 96,
        DXGI_FORMAT_BC7_TYPELESS = 97,
        DXGI_FORMAT_BC7_UNORM = 98,
        DXGI_FORMAT_BC7_UNORM_SRGB = 99,
        DXGI_FORMAT_AYUV = 100,
        DXGI_FORMAT_Y410 = 101,
        DXGI_FORMAT_Y416 = 102,
        DXGI_FORMAT_NV12 = 103,
        DXGI_FORMAT_P010 = 104,
        DXGI_FORMAT_P016 = 105,
        DXGI_FORMAT_420_OPAQUE = 106,
        DXGI_FORMAT_YUY2 = 107,
        DXGI_FORMAT_Y210 = 108,
        DXGI_FORMAT_Y216 = 109,
        DXGI_FORMAT_NV11 = 110,
        DXGI_FORMAT_AI44 = 111,
        DXGI_FORMAT_IA44 = 112,
        DXGI_FORMAT_P8 = 113,
        DXGI_FORMAT_A8P8 = 114,
        DXGI_FORMAT_B4G4R4A4_UNORM = 115,
        DXGI_FORMAT_FORCE_UINT = 0xffffffff
    }

    public enum D3D10_RESOURCE_DIMENSION
    {
        D3D10_RESOURCE_DIMENSION_UNKNOWN = 0,
        D3D10_RESOURCE_DIMENSION_BUFFER = 1,
        D3D10_RESOURCE_DIMENSION_TEXTURE1D = 2,
        D3D10_RESOURCE_DIMENSION_TEXTURE2D = 3,
        D3D10_RESOURCE_DIMENSION_TEXTURE3D = 4
    }

    #endregion
}