/*
 * Created by SharpDevelop.
 * User: NCDyson
 * Date: 2/1/2018
 * Time: 5:18 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
using System.Diagnostics;

namespace RareView
{
	/// <summary>
	/// Description of XPRFile.
	/// </summary>
	public class XPRFile
	{
		public struct XPREntry
		{
			public string FileName;
			public byte[] Data;
			public int Width;
			public int Height;
			public int Format;
		}
		
		public List<XPREntry> Entries = new List<XPREntry>();
		
		public XPRFile()
		{
		}
		
		public void AddEntry(string _FileName, byte[] _Data, int _Width, int _Height, int _Format)
		{
			if(_Data == null) return;
			var tmpEntry = new XPREntry()
			{
				FileName = _FileName,
				Data = _Data,
				Width = _Width,
				Height = _Height,
				Format = _Format
			};
			Entries.Add(tmpEntry);
		}
		
		public static void Write32(int what, BinaryWriter outF)
		{
			var tmpBytes = BitConverter.GetBytes(what);
			Array.Reverse(tmpBytes);
			outF.Write(tmpBytes);
		}
		
		public static void Write16(short what, BinaryWriter outF)
		{
			var tmpBytes = BitConverter.GetBytes(what);
			Array.Reverse(tmpBytes);
			outF.Write(tmpBytes);
		}
		
		public static void WriteString(string what, BinaryWriter outF)
		{
			var strBytes = Encoding.ASCII.GetBytes(what);
			outF.Write(strBytes);
		}
		
		public static int GetTexelPitch(int format)
		{
			//return 0x2001;
			switch(format)
			{
				case 0x2:
					return 0x1400;
				case 0x4a:
					return 0x400;
				case 0x44:
					return 0x1414;
				case 0x52:
				case 0x53:
				case 0x54:
				case 0x71:
					return 0xd10;
				case 0x86:
					return 0xc14;
				default:
					throw new ArgumentOutOfRangeException("format", string.Format("Unknown Texture Type {0:X}", format));
			}
		}
		
		public static int GetSize(int width, int height)
		{
			return (((height - 1) << 13) | (width - 1));
		}
		
		public static int GetTiledWidth(int width)
		{
			int tileDepth = 0;
			for(int i = 0; i < 384; i++)
			{
				int tileLevel = i * 32;
				if(width > tileLevel)
				{
					tileDepth = tileLevel + 32;
				}
				else
				{
					break;
				}
			}
			
			return ((tileDepth << 1) | 0x8000);
		}
		
		public static int GetNextSector(int offset)
		{
			if((offset % 0x800) != 0)
			{
				return ((offset / 0x800) + 1) * 0x800;
			}
			
			return offset;
		}
		
		public static void PadToNextSector(int currentOffset, BinaryWriter outF)
		{
			int nextSectorOffset = GetNextSector(currentOffset);
			int padLength = (nextSectorOffset - currentOffset);
			for(int i = 0; i < padLength; i++)
			{
				outF.Write((byte)0);
			}
		}
		
		public void Write(string fileName)
		{
			int entryHeaderSize = 0x34;
			
			using(var fs = new FileStream(fileName, FileMode.OpenOrCreate))
			{
				using(var outF = new BinaryWriter(fs))
				{
					int nameTableLen = 0;
					int TotalDataSize = 0;
					foreach(var tmpE in Entries)
					{
						nameTableLen += (tmpE.FileName.Length + 1);
						TotalDataSize += (tmpE.Data.Length);
					}
					
					if((nameTableLen % 4) != 0)
					{
						nameTableLen = ((nameTableLen / 4) + 1) * 4;
					}
					
					int NameTableStartOffset = (Entries.Count * 0x10) + 0x8;
					int EntryStartOffset = NameTableStartOffset + nameTableLen;
					int DataStartOffset = EntryStartOffset + (Entries.Count * entryHeaderSize);
					
					if(DataStartOffset < 0x800) DataStartOffset = 0x800;
					
					
					outF.Write((int)0x32525058);
					Write32(DataStartOffset, outF);
					Write32(TotalDataSize + 0x80c, outF);
					Write32(Entries.Count, outF);
					
					int nameOffset = NameTableStartOffset;
					for(int i = 0; i < Entries.Count; i++)
					{
						var tmpE = Entries[i];
						outF.Write((int)0x44325854);
						int entryOffset = EntryStartOffset + (i * entryHeaderSize);
						Write32(entryOffset, outF);
						Write32(0x34, outF);
						Write32(nameOffset, outF);
						nameOffset += (tmpE.FileName.Length + 1);
					}
					
					Write32(0, outF);
					
					for(int i = 0; i < Entries.Count; i++)
					{
						var tmpE = Entries[i];
						WriteString(tmpE.FileName, outF);
						outF.Write((byte)0);
					}
					
					int padLen = (EntryStartOffset + 12) - (int)outF.BaseStream.Position;
					for(int i = 0; i < padLen; i++)
					{
						outF.Write((byte)0);
					}
					
					int TextureOffset = 0;
					for(int i = 0; i < Entries.Count; i++)
					{
						var tmpE = Entries[i];
						Write32(3, outF);
						Write32(1, outF);
						Write32(0, outF);
						Write32(0, outF);
						
						Write32(0, outF);
						Write16(-1, outF);
						Write16(0, outF);
						Write16(-1, outF);
						Write32(GetTiledWidth(tmpE.Width), outF);
						Write16((short)2, outF);
						TextureOffset = GetNextSector(TextureOffset);
						Write32(TextureOffset | tmpE.Format, outF);
						Write32(GetSize(tmpE.Width, tmpE.Height), outF);
						Write32(GetTexelPitch(tmpE.Format), outF);
						Write32(0, outF);
						Write32(0xa00, outF);
						
						TextureOffset += tmpE.Data.Length;
					}
					
					int TextureDelta = (int)outF.BaseStream.Position;
					if((TextureDelta - 0xc) < DataStartOffset)
					{
						int textureDataStartPad = (DataStartOffset - (TextureDelta - 0xc));
						for(int i = 0; i < textureDataStartPad; i++)
						{
							outF.Write((byte)0);
						}
					}
					
					for(int i = 0; i < Entries.Count; i++)
					{
						var tmpE = Entries[i];
						int currentOffset = (int)outF.BaseStream.Position - (0xc + DataStartOffset);
						PadToNextSector(currentOffset, outF);
						Debug.WriteLine(string.Format("Writing Texture {0} at 0x{1:X}", i, outF.BaseStream.Position - 0x80c));
						outF.Write(tmpE.Data);
					}
				}
			}
		}
	}
}
