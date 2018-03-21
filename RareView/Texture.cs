/*
 * Created by SharpDevelop.
 * User: NCDyson
 * Date: 1/6/2018
 * Time: 6:50 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;


namespace RareView
{
	/// <summary>
	/// Description of Texture.
	/// </summary>
	public class Texture
	{
		public static string TEXTURE_VERSION_STRING = "04.05.05.0032";
		public const int TEXTURE_FORMAT_L8 = 2;
		public const int TEXTURE_FORMAT_A8R8G8B8 = 0x86;
		public const int TEXTURE_FORMAT_DXT1 = 0x52;
		public const int TEXTURE_FORMAT_DXT3 = 0x53;
		public const int TEXTURE_FORMAT_DXT5 = 0x54;
		public const int TEXTURE_FORMAT_DXN = 0x71;
		public const int TEXTURE_FORMAT_A8L8 = 0x4a;
		public const int TEXTURE_FORMAT_X4R4G4B4 = 0x4f;
		public const int TEXTURE_FORMAT_R5G6B5 = 0x44;
		
		
		public int Offset = 0;
		public int ABSOffset = 0;
		public int Width = 0;
		public int Height = 0;
		public int TextureFormat = 0;
		public int TextureType = 0;
		public int MipLevelCount = 0;
		public int TextureArgs = 0;
		
		public byte[] TextureData = null;
		public int GLID = -1;
		
		public Texture()
		{
			
		}
		
		public static int GetTextureDataSize(int _width, int _height, int _textureFormat)
		{
			int textureDataSize = 0;
			switch(_textureFormat)
			{
				case TEXTURE_FORMAT_A8L8:
				{
					textureDataSize = (_width * _height) * 2;
					break;
				}
				case TEXTURE_FORMAT_L8:
				{
					textureDataSize = (_width * _height);
					break;
				}
				case TEXTURE_FORMAT_DXT1:
				{
					textureDataSize = (_width * _height / 2);		
					break;
				}
				case TEXTURE_FORMAT_DXT3:
				case TEXTURE_FORMAT_DXT5:
				case TEXTURE_FORMAT_DXN:
				{
					textureDataSize = (_width * _height);
					break;
				}
				case TEXTURE_FORMAT_A8R8G8B8:
				{
					textureDataSize = (_width * _height) * 4;
					break;
				}
				case TEXTURE_FORMAT_X4R4G4B4:
				{
					textureDataSize = (_width * 2) * _height;
					break;
				}
				case TEXTURE_FORMAT_R5G6B5:
				{
					textureDataSize = (_width * 2) * _height;
					break;
				}
				default:
				{
					Logger.LogError(string.Format("Texture Format {0:X} is currently unsupported.\n", _textureFormat));
					return 0;
				}
			}
			
			return textureDataSize;
		}
		
		public bool Read(BinaryReader dSection, BinaryReader gSection)
		{
			int texOffset = (int)dSection.BaseStream.Position;
			string isTexture = IO.ReadString(dSection);
			if(isTexture != "texture")
			{
				Logger.LogError(string.Format("Error reading Texture Block at 0x{0:X}, Header String Mismatch.\n", texOffset));
				return false;
			}
			
			string versionStr = IO.ReadString(dSection);
			if(versionStr != TEXTURE_VERSION_STRING)
			{
				Logger.LogError(string.Format("Error reading Texture Block at 0x{0:X}, Version String Mismatch.\n", texOffset));
				return false;
			}
			
			IO.Align(dSection, 4);
			TextureFormat = IO.ReadBig32(dSection) & 0xFF;
			TextureType = IO.ReadBig32(dSection);
			dSection.BaseStream.Seek(0x4, SeekOrigin.Current);
			Width = IO.ReadBig16(dSection);
			Height = IO.ReadBig16(dSection);
			Offset = IO.ReadBig32(dSection);
			MipLevelCount = IO.ReadByte(dSection);
			
			TextureArgs = IO.ReadBig32(dSection);
			
			int textureDataSize = GetTextureDataSize(Width, Height, TextureFormat);
			
			gSection.BaseStream.Seek(Offset, SeekOrigin.Current);
			ABSOffset = (int)gSection.BaseStream.Position;
			TextureData = gSection.ReadBytes(textureDataSize);
			
			return true;
		}
		
		
		
		public bool ReCrop(BinaryReader gSection)
		{
			int nextWidth = Width * 2;
			int nextHeight = Height * 2;
			
			int textureDataSize = GetTextureDataSize(nextWidth, nextHeight, TextureFormat);
			
			//Now, real quick, check to make sure we have enough data to recrop this.
			gSection.BaseStream.Seek(0, SeekOrigin.End);
			long streamSize = gSection.BaseStream.Position;
			gSection.BaseStream.Seek(ABSOffset, SeekOrigin.Begin);
			long availableBytes = streamSize - ABSOffset;
			if(availableBytes < textureDataSize)
			{
				Logger.LogError("Cannot recrop texture, not enough data available.\n");
				return true;
			}
			
			
			
			var recropData = gSection.ReadBytes(textureDataSize);
			recropData = ConvertToLinearTexture(recropData, nextWidth, nextHeight, TextureFormat);
			recropData = DecodeTexture(recropData, TextureFormat, nextWidth, nextHeight);
			
			for(int y = 0; y < Height; y++)
			{
				for(int x = 0; x < Width; x++)
				{
					int inputOffset = ((y * nextWidth) + x) * 4;
					int outputOffset = ((y * Width) + x) * 4;
					TextureData[outputOffset] = recropData[inputOffset];
					TextureData[outputOffset + 1] = recropData[inputOffset + 1];
					TextureData[outputOffset + 2] = recropData[inputOffset + 2];
					TextureData[outputOffset + 3] = recropData[inputOffset + 3];
				}
			}
			
			if(GLID == -1) return true;
			GL.BindTexture(TextureTarget.Texture2D, GLID);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, TextureData);
			GL.BindTexture(TextureTarget.Texture2D, 0);
			
			return true;
		}
		
		
		public bool Init()
		{
			if(TextureData == null) return true;
			
			if(GLID == -1) GLID = GL.GenTexture();
			
			TextureData = ConvertToLinearTexture(TextureData, Width, Height, TextureFormat);
			
			GL.BindTexture(TextureTarget.Texture2D, GLID);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.Repeat);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.Repeat);
		
			TextureData = DecodeTexture(TextureData, TextureFormat, Width, Height);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, TextureData);
			
			
			GL.BindTexture(TextureTarget.Texture2D, 0);
			return true;
		}
		
		public bool DeInit()
		{
			if(GLID != -1) GL.DeleteTexture(GLID);
			GLID = -1;
			return true;
		}
		
		public TreeNode ToNode(int textureID)
		{
			string TextureTypeStr = GetTextureDescription();
			var retNode = new TreeNode(string.Format("Texturefile_{0} {1} at {2:X}", textureID, TextureTypeStr, Offset));
			
			return retNode;
		}
		
		public string GetTextureTypeStr()
		{
			string tmpTextureStr = "";
			switch (TextureFormat)
			{
				case TEXTURE_FORMAT_A8L8:
				{
					tmpTextureStr = "A8L8";
					break;
				}
				case TEXTURE_FORMAT_A8R8G8B8:
				{
					tmpTextureStr = "A8R8G8B8";
					break;
				}
				case TEXTURE_FORMAT_DXN:
				{
					tmpTextureStr = "DXN";
					break;
				}
				case TEXTURE_FORMAT_DXT1:
				{
					tmpTextureStr = "DXT1";
					break;
				}
				case TEXTURE_FORMAT_DXT3:
				{
					tmpTextureStr = "DXT3";
					break;
				}
				case TEXTURE_FORMAT_DXT5:
				{
					tmpTextureStr = "DXT5";
					break;
				}
				case TEXTURE_FORMAT_L8:
				{
					tmpTextureStr = "L8";
					break;
				}
				case TEXTURE_FORMAT_X4R4G4B4:
				{
					tmpTextureStr = "X4R4G4B4";
					break;
				}
				case TEXTURE_FORMAT_R5G6B5:
				{
					tmpTextureStr = "R5G6B5";
					break;
				}
				default:
				{
					tmpTextureStr = string.Format("Unknown({0:X})", TextureFormat);
					break;
				}
			}
			return tmpTextureStr;
		}
		
		
		public string GetTextureDescription()
		{
			string tmpTextureStr = GetTextureTypeStr();
			
			tmpTextureStr += string.Format(" ({0}x{1})", Width, Height);
			
			/*
			if(TextureType != 0)
			{
				tmpTextureStr += string.Format(" Unknown Type {0:X}, {1:X}", TextureFormat, TextureArgs);
			}
			*/
			
			return tmpTextureStr;
		}
		
		private static byte[] ConvertToLinearTexture(byte[] data, int _width, int _height, int _textureFormat)
		{
			byte[] destData = new byte[data.Length];

            int blockSize;
            int texelPitch;

            switch (_textureFormat)
            {
           	case TEXTURE_FORMAT_A8L8:
            		blockSize = 1;
            		texelPitch = 2;
            		break;
            	case TEXTURE_FORMAT_L8:
            		blockSize = 1;
            		texelPitch = 1;
            		break;
                case TEXTURE_FORMAT_DXT1:
                    blockSize = 4;
                    texelPitch = 8;
                    break;
                case TEXTURE_FORMAT_DXT3:
                case TEXTURE_FORMAT_DXT5:
                case TEXTURE_FORMAT_DXN:
                    blockSize = 4;
                    texelPitch = 16;
                    break;
               case TEXTURE_FORMAT_A8R8G8B8:
                	blockSize = 1;
                	texelPitch = 4;
                    break;
               case TEXTURE_FORMAT_X4R4G4B4:
                    blockSize = 1;
                    texelPitch = 2;
                    break;
                   case TEXTURE_FORMAT_R5G6B5:
                    blockSize = 1;
                    texelPitch = 2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Bad texture type!");
            }

            int blockWidth = _width / blockSize;
            int blockHeight = _height / blockSize;

            for (int j = 0; j < blockHeight; j++)
            {
                for (int i = 0; i < blockWidth; i++)
                {
                    int blockOffset = j * blockWidth + i;

                    int x = XGAddress2DTiledX(blockOffset, blockWidth, texelPitch);
                    int y = XGAddress2DTiledY(blockOffset, blockWidth, texelPitch);

                    int srcOffset = j * blockWidth * texelPitch + i * texelPitch;
                    int destOffset = y * blockWidth * texelPitch + x * texelPitch;
        			//TODO: ConvertToLinearTexture apparently breaks on on textures with a height of 64...
        			if(destOffset >= destData.Length) continue;
                    Array.Copy(data, srcOffset, destData, destOffset, texelPitch);
                }
            }

            return destData;
		}
		
        private static int XGAddress2DTiledX(int Offset, int Width, int TexelPitch)
        {
            int AlignedWidth = (Width + 31) & ~31;

            int LogBpp = (TexelPitch >> 2) + ((TexelPitch >> 1) >> (TexelPitch >> 2));
            int OffsetB = Offset << LogBpp;
            int OffsetT = ((OffsetB & ~4095) >> 3) + ((OffsetB & 1792) >> 2) + (OffsetB & 63);
            int OffsetM = OffsetT >> (7 + LogBpp);

            int MacroX = ((OffsetM % (AlignedWidth >> 5)) << 2);
            int Tile = ((((OffsetT >> (5 + LogBpp)) & 2) + (OffsetB >> 6)) & 3);
            int Macro = (MacroX + Tile) << 3;
            int Micro = ((((OffsetT >> 1) & ~15) + (OffsetT & 15)) & ((TexelPitch << 3) - 1)) >> LogBpp;

            return Macro + Micro;
        }

        private static int XGAddress2DTiledY(int Offset, int Width, int TexelPitch)
        {
            int AlignedWidth = (Width + 31) & ~31;

            int LogBpp = (TexelPitch >> 2) + ((TexelPitch >> 1) >> (TexelPitch >> 2));
            int OffsetB = Offset << LogBpp;
            int OffsetT = ((OffsetB & ~4095) >> 3) + ((OffsetB & 1792) >> 2) + (OffsetB & 63);
            int OffsetM = OffsetT >> (7 + LogBpp);

            int MacroY = ((OffsetM / (AlignedWidth >> 5)) << 2);
            int Tile = ((OffsetT >> (6 + LogBpp)) & 1) + (((OffsetB & 2048) >> 10));
            int Macro = (MacroY + Tile) << 3;
            int Micro = ((((OffsetT & (((TexelPitch << 6) - 1) & ~31)) + ((OffsetT & 15) << 1)) >> (3 + LogBpp)) & ~1);

            return Macro + Micro + ((OffsetT & 16) >> 4);
        }
        
        
        public static byte[] DecodeTexture(byte[] _textureData, int _textureFormat, int _width, int _height)
        {
        	switch(_textureFormat)
        	{
        		case TEXTURE_FORMAT_DXT1:
    			{
    				return DecodeDXT1(_textureData, _width, _height);
    			}
        		case TEXTURE_FORMAT_DXT3:
    			{
    				return DecodeDXT3(_textureData, _width, _height);
    			}
        		case TEXTURE_FORMAT_DXT5:
    			{
    				return DecodeDXT5(_textureData, _width, _height);
    			}
        		case TEXTURE_FORMAT_DXN:
    			{
    				return DecodeDXN(_textureData, _width, _height);
    			}
        		case TEXTURE_FORMAT_A8L8:
    			{
    				return DecodeA8L8(_textureData, _width, _height);
    			}
    			case TEXTURE_FORMAT_A8R8G8B8:
    			{
    				return DecodeA8R8G8B8(_textureData, _width, _height);
    			}
        		case TEXTURE_FORMAT_L8:
    			{
    				return DecodeL8(_textureData, _width, _height);
    			}
        		case TEXTURE_FORMAT_X4R4G4B4:
    			{
    				return DecodeX4R4G4B4(_textureData, _width, _height);
    			}
        		case TEXTURE_FORMAT_R5G6B5:
    			{
    				return DecodeR5G6B5(_textureData, _width, _height);
    			}
        		default:
    			{
    				return _textureData;
    			}
        	}
        }
        
		public static byte[] DecodeX4R4G4B4(byte[] data, int width, int height)
		{
			var pixData = new byte[(width * height) * 4];			
			for(int i = 0; i < (width * height); i++)
			{
				int srcOffset = i * 2;
				int destOffset = i * 4;
				byte b = (byte)(data[srcOffset] & 0xF);
				byte x = (byte)((data[srcOffset] >> 4) & 0xf);
				byte r = (byte)(data[srcOffset + 1] & 0xF);
				byte g = (byte)((data[srcOffset + 1] >> 4) & 0xf);
				pixData[destOffset + 0] = (byte)((r << 4 | r) & 0xFF);
				pixData[destOffset + 1] = (byte)((g << 4 | g) & 0xFF);
				pixData[destOffset + 2] = (byte)((b << 4 | b) & 0xFF);
				pixData[destOffset + 3] = 0xff;
			}
			
			return pixData;
		}

        
        public static byte[] DecodeDXT1(byte[] data, int width, int height)
		{
			byte[] pixData = new byte[width * height * 4];
            int xBlocks = width / 4;
            int yBlocks = height / 4;
            for (int y = 0; y < yBlocks; y++)
            {
                for (int x = 0; x < xBlocks; x++)
                {
                    int blockDataStart = ((y * xBlocks) + x) * 8;

                    uint color0 = ((uint)data[blockDataStart + 0] << 8) + data[blockDataStart + 1];
                    uint color1 = ((uint)data[blockDataStart + 2] << 8) + data[blockDataStart + 3];

                    uint code = BitConverter.ToUInt32(data, blockDataStart + 4);

                    ushort r0 = 0, g0 = 0, b0 = 0, r1 = 0, g1 = 0, b1 = 0;
                    r0 = (ushort)(8 * (color0 & 31));
                    g0 = (ushort)(4 * ((color0 >> 5) & 63));
                    b0 = (ushort)(8 * ((color0 >> 11) & 31));

                    r1 = (ushort)(8 * (color1 & 31));
                    g1 = (ushort)(4 * ((color1 >> 5) & 63));
                    b1 = (ushort)(8 * ((color1 >> 11) & 31));

                    for (int k = 0; k < 4; k++)
                    {
                        int j = k ^ 1;

                        for (int i = 0; i < 4; i++)
                        {
                            int pixDataStart = (width * (y * 4 + j) * 4) + ((x * 4 + i) * 4);
                            uint codeDec = code & 0x3;

                            switch (codeDec)
                            {
                                case 0:
                                    pixData[pixDataStart + 0] = (byte)r0;
                                    pixData[pixDataStart + 1] = (byte)g0;
                                    pixData[pixDataStart + 2] = (byte)b0;
                                    pixData[pixDataStart + 3] = 255;
                                    break;
                                case 1:
                                    pixData[pixDataStart + 0] = (byte)r1;
                                    pixData[pixDataStart + 1] = (byte)g1;
                                    pixData[pixDataStart + 2] = (byte)b1;
                                    pixData[pixDataStart + 3] = 255;
                                    break;
                                case 2:
                                    pixData[pixDataStart + 3] = 255;
                                    if (color0 > color1)
                                    {
                                        pixData[pixDataStart + 0] = (byte)((2 * r0 + r1) / 3);
                                        pixData[pixDataStart + 1] = (byte)((2 * g0 + g1) / 3);
                                        pixData[pixDataStart + 2] = (byte)((2 * b0 + b1) / 3);
                                    }
                                    else
                                    {
                                        pixData[pixDataStart + 0] = (byte)((r0 + r1) / 2);
                                        pixData[pixDataStart + 1] = (byte)((g0 + g1) / 2);
                                        pixData[pixDataStart + 2] = (byte)((b0 + b1) / 2);
                                    }
                                    break;
                                case 3:
                                    if (color0 > color1)
                                    {
                                        pixData[pixDataStart + 0] = (byte)((r0 + 2 * r1) / 3);
                                        pixData[pixDataStart + 1] = (byte)((g0 + 2 * g1) / 3);
                                        pixData[pixDataStart + 2] = (byte)((b0 + 2 * b1) / 3);
                                        pixData[pixDataStart + 3] = 255;
                                    }
                                    else
                                    {
                                        pixData[pixDataStart + 0] = 0;
                                        pixData[pixDataStart + 1] = 0;
                                        pixData[pixDataStart + 2] = 0;
                                        pixData[pixDataStart + 3] = 0;
                                    }
                                    break;
                            }

                            code >>= 2;
                        }
                    }


                }
            }
            return pixData;
		}
	
		public static byte[] DecodeDXT3(byte[] data, int width, int height)
		{
            byte[] pixData = new byte[width * height * 4];
            int xBlocks = width / 4;
            int yBlocks = height / 4;
            for (int y = 0; y < yBlocks; y++)
            {
                for (int x = 0; x < xBlocks; x++)
                {
                    int blockDataStart = ((y * xBlocks) + x) * 16;
                    ushort[] alphaData = new ushort[4];

                    alphaData[0] = (ushort)((data[blockDataStart + 0] << 8) + data[blockDataStart + 1]);
                    alphaData[1] = (ushort)((data[blockDataStart + 2] << 8) + data[blockDataStart + 3]);
                    alphaData[2] = (ushort)((data[blockDataStart + 4] << 8) + data[blockDataStart + 5]);
                    alphaData[3] = (ushort)((data[blockDataStart + 6] << 8) + data[blockDataStart + 7]);
                    
                    byte[,] alpha = new byte[4, 4];
                    for (int j = 0; j < 4; j++)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            alpha[i, j] = (byte)((alphaData[j] & 0xF) * 16);
                            alphaData[j] >>= 4;
                        }
                    }

                    ushort color0 = (ushort)((data[blockDataStart + 8] << 8) + data[blockDataStart + 9]);
                    ushort color1 = (ushort)((data[blockDataStart + 10] << 8) + data[blockDataStart + 11]);

                    uint code = BitConverter.ToUInt32(data, blockDataStart + 8 + 4);

                    ushort r0 = 0, g0 = 0, b0 = 0, r1 = 0, g1 = 0, b1 = 0;
                    r0 = (ushort)(8 * (color0 & 31));
                    g0 = (ushort)(4 * ((color0 >> 5) & 63));
                    b0 = (ushort)(8 * ((color0 >> 11) & 31));

                    r1 = (ushort)(8 * (color1 & 31));
                    g1 = (ushort)(4 * ((color1 >> 5) & 63));
                    b1 = (ushort)(8 * ((color1 >> 11) & 31));

                    for (int k = 0; k < 4; k++)
                    {
                        int j = k ^ 1;
                        
                        for (int i = 0; i < 4; i++)
                        {
                            int pixDataStart = (width * (y * 4 + j) * 4) + ((x * 4 + i) * 4);
                            uint codeDec = code & 0x3;

                            pixData[pixDataStart + 3] = alpha[i, j];

                            switch (codeDec)
                            {
                                case 0:
                                    pixData[pixDataStart + 0] = (byte)r0;
                                    pixData[pixDataStart + 1] = (byte)g0;
                                    pixData[pixDataStart + 2] = (byte)b0;
                                    break;
                                case 1:
                                    pixData[pixDataStart + 0] = (byte)r1;
                                    pixData[pixDataStart + 1] = (byte)g1;
                                    pixData[pixDataStart + 2] = (byte)b1;
                                    break;
                                case 2:
                                    if (color0 > color1)
                                    {
                                        pixData[pixDataStart + 0] = (byte)((2 * r0 + r1) / 3);
                                        pixData[pixDataStart + 1] = (byte)((2 * g0 + g1) / 3);
                                        pixData[pixDataStart + 2] = (byte)((2 * b0 + b1) / 3);
                                    }
                                    else
                                    {
                                        pixData[pixDataStart + 0] = (byte)((r0 + r1) / 2);
                                        pixData[pixDataStart + 1] = (byte)((g0 + g1) / 2);
                                        pixData[pixDataStart + 2] = (byte)((b0 + b1) / 2);
                                    }
                                    break;
                                case 3:
                                    if (color0 > color1)
                                    {
                                        pixData[pixDataStart + 0] = (byte)((r0 + 2 * r1) / 3);
                                        pixData[pixDataStart + 1] = (byte)((g0 + 2 * g1) / 3);
                                        pixData[pixDataStart + 2] = (byte)((b0 + 2 * b1) / 3);
                                    }
                                    else
                                    {
                                        pixData[pixDataStart + 0] = 0;
                                        pixData[pixDataStart + 1] = 0;
                                        pixData[pixDataStart + 2] = 0;
                                    }
                                    break;
                            }

                            code >>= 2;
                        }
                    }


                }
            }
            return pixData;
		}
		
		public static ulong ReadDXNBlockBits(byte[] data, int blockStart)
		{
			ulong blockBits = 0;
		
			blockBits |= data[blockStart + 6];
			blockBits <<= 8;
			blockBits |= data[blockStart + 7];
			blockBits <<= 8;
			blockBits |= data[blockStart + 4];
			blockBits <<= 8;
			blockBits |= data[blockStart + 5];
			blockBits <<= 8;
			blockBits |= data[blockStart + 2];
			blockBits <<= 8;
			blockBits |= data[blockStart + 3];

			return blockBits;
		}
		
		public static byte[] DecodeDXN(byte[] data, int width, int height)
		{
			byte[] pixData = new byte[width * height * 4];
			
			int xBlocks = width / 4;
			int yBlocks = height / 4;
			for(int y = 0; y < yBlocks; y++)
			{
				for(int x = 0; x < xBlocks; x++)
				{
					int blockStart = ((y * xBlocks) + x) * 16;
					byte[] red = new byte[8];
					red[1] = data[blockStart];
					red[0] = data[blockStart + 1];
					if(red[0] > red[1])
					{
						red[2] = (byte)((6 * red[0] + 1 * red[1]) / 7);
						red[3] = (byte)((5 * red[0] + 2 * red[1]) / 7);
						red[4] = (byte)((4 * red[0] + 3 * red[1]) / 7);
						red[5] = (byte)((3 * red[0] + 4 * red[1]) / 7);
						red[6] = (byte)((2 * red[0] + 5 * red[1]) / 7);
						red[7] = (byte)((1 * red[0] + 6 * red[1]) / 7);
					}
					else
					{
						red[2] = (byte)((4 * red[0] + 1 * red[1]) / 5);
						red[3] = (byte)((3 * red[0] + 2 * red[1]) / 5);
						red[4] = (byte)((2 * red[0] + 3 * red[1]) / 5);
						red[5] = (byte)((1 * red[0] + 4 * red[1]) / 5);
						red[6] = 0;
						red[7] = 0xff;
					}
					
					ulong blockBits = 0;
					blockBits = ReadDXNBlockBits(data, blockStart);
					
					byte[] redIndices = new byte[16];
					for(int i = 0; i < 16; i++)
					{
						redIndices[i] = (byte)((blockBits >> (3 * i)) & 0x7);
					}
					
					blockStart += 8;
					
					byte[] green = new byte[8];
					green[1] = data[blockStart];
					green[0] = data[blockStart + 1];
					
					if(green[0] > green[1])
					{
						green[2] = (byte)((6 * green[0] + 1 * green[1]) / 7);
						green[3] = (byte)((5 * green[0] + 2 * green[1]) / 7);
						green[4] = (byte)((4 * green[0] + 3 * green[1]) / 7);
						green[5] = (byte)((3 * green[0] + 4 * green[1]) / 7);
						green[6] = (byte)((2 * green[0] + 5 * green[1]) / 7);
						green[7] = (byte)((1 * green[0] + 6 * green[1]) / 7);
					}
					else
					{
						green[2] = (byte)((4 * green[0] + 1 * green[1]) / 5);
						green[3] = (byte)((3 * green[0] + 2 * green[1]) / 5);
						green[4] = (byte)((2 * green[0] + 3 * green[1]) / 5);
						green[5] = (byte)((1 * green[0] + 4 * green[1]) / 5);
						green[6] = 0;
						green[7] = 0xff;
					}
					
					blockBits = 0;
					blockBits = ReadDXNBlockBits(data, blockStart);
					
					byte[] greenIndices = new byte[16];
					for(int i = 0; i < 16; i++)
					{
						greenIndices[i] = (byte)((blockBits >> (i * 3)) & 0x7);
					}
					
					
					for(int pY = 0; pY < 4; pY++)
					{
						int j = pY;// ^ 1;
						for(int pX = 0; pX < 4; pX++)
						{
                            int pixDataStart = (width * (y * 4 + j) * 4) + ((x * 4 + pX) * 4);
                            int colID = pY * 4 + pX;
                            byte colRed = red[redIndices[colID]];
                            byte colBlue = green[greenIndices[colID]];
                            pixData[pixDataStart] = 0xff;
                            pixData[pixDataStart + 1] = colBlue;
                            pixData[pixDataStart + 2] = colRed;
                            pixData[pixDataStart + 3] = 0xff;
						}
					}
				}
			}
			
			return pixData;
			
		}
		
		public static byte[] DecodeDXT5(byte[] data, int width, int height)
		{
			byte[] pixData = new byte[width * height * 4];
            int xBlocks = width / 4;
            int yBlocks = height / 4;
            for (int y = 0; y < yBlocks; y++)
            {
                for (int x = 0; x < xBlocks; x++)
                {
                    int blockDataStart = ((y * xBlocks) + x) * 16;
                    uint[] alphas = new uint[8];
                    ulong alphaMask = 0;

                    alphas[0] = data[blockDataStart + 1];
                    alphas[1] = data[blockDataStart + 0];

                    alphaMask |= data[blockDataStart + 6];
                    alphaMask <<= 8;
                    alphaMask |= data[blockDataStart + 7];
                    alphaMask <<= 8;
                    alphaMask |= data[blockDataStart + 4];
                    alphaMask <<= 8;
                    alphaMask |= data[blockDataStart + 5];
                    alphaMask <<= 8;
                    alphaMask |= data[blockDataStart + 2];
                    alphaMask <<= 8;
                    alphaMask |= data[blockDataStart + 3];


                    // 8-alpha or 6-alpha block
                    if (alphas[0] > alphas[1])
                    {
                        // 8-alpha block: derive the other 6
                        // Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
                        alphas[2] = (byte)((6 * alphas[0] + 1 * alphas[1] + 3) / 7);    // bit code 010
                        alphas[3] = (byte)((5 * alphas[0] + 2 * alphas[1] + 3) / 7);    // bit code 011
                        alphas[4] = (byte)((4 * alphas[0] + 3 * alphas[1] + 3) / 7);    // bit code 100
                        alphas[5] = (byte)((3 * alphas[0] + 4 * alphas[1] + 3) / 7);    // bit code 101
                        alphas[6] = (byte)((2 * alphas[0] + 5 * alphas[1] + 3) / 7);    // bit code 110
                        alphas[7] = (byte)((1 * alphas[0] + 6 * alphas[1] + 3) / 7);    // bit code 111
                    }
                    else
                    {
                        // 6-alpha block.
                        // Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
                        alphas[2] = (byte)((4 * alphas[0] + 1 * alphas[1] + 2) / 5);    // Bit code 010
                        alphas[3] = (byte)((3 * alphas[0] + 2 * alphas[1] + 2) / 5);    // Bit code 011
                        alphas[4] = (byte)((2 * alphas[0] + 3 * alphas[1] + 2) / 5);    // Bit code 100
                        alphas[5] = (byte)((1 * alphas[0] + 4 * alphas[1] + 2) / 5);    // Bit code 101
                        alphas[6] = 0x00;                                               // Bit code 110
                        alphas[7] = 0xFF;                                               // Bit code 111
                    }

                    byte[,] alpha = new byte[4, 4];

                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            alpha[j, i] = (byte)alphas[alphaMask & 7];
                            alphaMask >>= 3;
                        }
                    }

                    ushort color0 = (ushort)((data[blockDataStart + 8] << 8) + data[blockDataStart + 9]);
                    ushort color1 = (ushort)((data[blockDataStart + 10] << 8) + data[blockDataStart + 11]);

                    uint code = BitConverter.ToUInt32(data, blockDataStart + 8 + 4);

                    ushort r0 = 0, g0 = 0, b0 = 0, r1 = 0, g1 = 0, b1 = 0;
                    r0 = (ushort)(8 * (color0 & 31));
                    g0 = (ushort)(4 * ((color0 >> 5) & 63));
                    b0 = (ushort)(8 * ((color0 >> 11) & 31));

                    r1 = (ushort)(8 * (color1 & 31));
                    g1 = (ushort)(4 * ((color1 >> 5) & 63));
                    b1 = (ushort)(8 * ((color1 >> 11) & 31));

                    for (int k = 0; k < 4; k++)
                    {
                        int j = k ^ 1;

                        for (int i = 0; i < 4; i++)
                        {
                            int pixDataStart = (width * (y * 4 + j) * 4) + ((x * 4 + i) * 4);
                            uint codeDec = code & 0x3;

                            pixData[pixDataStart + 3] = alpha[i, j];

                            switch (codeDec)
                            {
                                case 0:
                                    pixData[pixDataStart + 0] = (byte)r0;
                                    pixData[pixDataStart + 1] = (byte)g0;
                                    pixData[pixDataStart + 2] = (byte)b0;
                                    break;
                                case 1:
                                    pixData[pixDataStart + 0] = (byte)r1;
                                    pixData[pixDataStart + 1] = (byte)g1;
                                    pixData[pixDataStart + 2] = (byte)b1;
                                    break;
                                case 2:
                                    if (color0 > color1)
                                    {
                                        pixData[pixDataStart + 0] = (byte)((2 * r0 + r1) / 3);
                                        pixData[pixDataStart + 1] = (byte)((2 * g0 + g1) / 3);
                                        pixData[pixDataStart + 2] = (byte)((2 * b0 + b1) / 3);
                                    }
                                    else
                                    {
                                        pixData[pixDataStart + 0] = (byte)((r0 + r1) / 2);
                                        pixData[pixDataStart + 1] = (byte)((g0 + g1) / 2);
                                        pixData[pixDataStart + 2] = (byte)((b0 + b1) / 2);
                                    }
                                    break;
                                case 3:
                                    if (color0 > color1)
                                    {
                                        pixData[pixDataStart + 0] = (byte)((r0 + 2 * r1) / 3);
                                        pixData[pixDataStart + 1] = (byte)((g0 + 2 * g1) / 3);
                                        pixData[pixDataStart + 2] = (byte)((b0 + 2 * b1) / 3);
                                    }
                                    else
                                    {
                                        pixData[pixDataStart + 0] = 0;
                                        pixData[pixDataStart + 1] = 0;
                                        pixData[pixDataStart + 2] = 0;
                                    }
                                    break;
                            }

                            code >>= 2;
                        }
                    }


                }
            }
            return pixData;
		}
		
				public static byte[] DecodeL8(byte[] data, int width, int height)
		{
			var pixData = new byte[(width * height) * 4];
			for(int i = 0; i < (width * height); i++)
			{
				int destOffset = i * 4;
				pixData[destOffset] = data[i];
				pixData[destOffset + 1] = data[i];
				pixData[destOffset + 2] = data[i];
				pixData[destOffset + 3] = 0xff;
			}
			
			return pixData;
		}
		
		public static byte[] DecodeA8L8(byte[] data, int width, int height)
		{
			var pixData = new byte[(width * height) * 4];
			for(int i = 0; i < (width * height); i++)
			{
				int srcOffset = i * 2;
				int destOffset = i * 4;
				pixData[destOffset] = data[srcOffset + 1];
				pixData[destOffset + 1] = data[srcOffset + 1];
				pixData[destOffset + 2] = data[srcOffset + 1];
				pixData[destOffset + 3] = data[srcOffset];
			}
			
			return pixData;
		}
		
		public static byte[] DecodeA8R8G8B8(byte[] data, int width, int height)
		{
			byte[] pixData = new byte[(width * height) * 4];
			
			for(int i = 0; i < (width * height); i++)
			{
				int offset = i * 4;
				pixData[offset] = data[offset+3];
				pixData[offset+1] = data[offset+2];
				pixData[offset+2] = data[offset+1];
				pixData[offset+3] = data[offset];
			}
			
			return pixData;
		}
		
		public static byte[] DecodeR5G6B5(byte[] data, int width, int height)
		{
			var tmpS = new byte[2];
			var pixData = new byte[(width * height) * 4];
			for(int i = 0; i < (width * height); i++)
			{
				int srcOffset = i * 2;
				int destOffst = i * 4;
				
				tmpS[0] = data[srcOffset + 1];
				tmpS[1] = data[srcOffset];
				short color = BitConverter.ToInt16(tmpS, 0);
				
				byte b5 = (byte)((color & 0xf800) >> 11);
				byte g6 = (byte)((color & 0x07e0) >> 5);
				byte r5 = (byte)((color & 0x1f));
					
				pixData[destOffst] = (byte)((r5 << 3) | (r5 >> 2));
				pixData[destOffst + 1] = (byte)((g6 << 2) | (g6 >> 4));
				pixData[destOffst + 2] = (byte)((b5 << 3) | (b5 >> 2));
				pixData[destOffst + 3] = 0xff;
			}
			
			return pixData;
		}
		
		public void DumpToMtl(StreamWriter fs, string outputPath, int textureID)
		{
			string textureName = string.Format("texturefile_{0}", textureID);
			
			fs.WriteLine(string.Format("newmtl {0}", textureName));
			fs.WriteLine("Ka 1.0 1.0 1.0");
			fs.WriteLine("Kd 1.0 1.0 1.0");
			fs.WriteLine("Ks 0.0 0.0 0.0");
			
			fs.WriteLine(string.Format("map_Kd {0}.png", textureName));
			
			//Now, convert to bitmap and save
			Bitmap oB = new Bitmap(Width, Height);
			for(int y = 0; y < Height; y++)
			{
				for(int x = 0; x < Width; x++)
				{
					int indiceOffset = (((y * Width) + x) * 4);
					oB.SetPixel(x, y, Color.FromArgb(TextureData[indiceOffset + 3], TextureData[indiceOffset + 2], TextureData[indiceOffset + 1], TextureData[indiceOffset]));
				}
			}
			oB.Save(Path.Combine(outputPath, textureName + ".png"));
		}
		
		public static void GetTextureAspect(int width, int height, out float texW, out float texH)
		{
			if(width < height)
			{
				texW = (float)width / (float)height;
				texH = 1.0f;
			}
			else
			{
				texW = 1.0f;
				texH = (float) height / (float)width;
			}
		}
		
		public void DumpToXpr(BinaryReader gStream, string outputFileName)
		{
			gStream.BaseStream.Seek(ABSOffset, SeekOrigin.Begin);
			var texDataSize = GetTextureDataSize(Width, Height, TextureFormat);
			var texData = gStream.ReadBytes(texDataSize);
			var tmpXPR = new XPRFile();
			string baseName = System.IO.Path.GetFileNameWithoutExtension(outputFileName);
			tmpXPR.AddEntry(baseName, texData, Width, Height, TextureFormat);
			tmpXPR.Write(outputFileName);
		}
		
	}
}
