/*
 * Created by SharpDevelop.
 * User: NCDyson
 * Date: 1/5/2018
 * Time: 4:50 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
//#define CAFF_SECTIONS_IN_MEMORY
using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zlib;
using System.Windows.Forms;

namespace RareView
{
	/// <summary>
	/// Description of CaffFile.
	/// </summary>
	public class CaffFile
	{
		
		
		public enum SectionType
		{
			Stream = 0x2,
			Data = 0x5,
			GPU = 0xc
		}
		
		public const int SECTION_TYPE_DATA = 0x5;
		public const int SECTION_TYPE_STREAM = 0x2;
		public const int SECTION_TYPE_GPU = 0xc;
		
		public struct CaffHeader
		{
			public string HeaderString;
			public string VersionString;
			public int Unk1;
			public int FileCount;
			public int TocEntryCount;
			public int Unk4;
			public int Unk5;
			public int Unk6;
			public int Unk7;
			public int Unk8;
			public int TocOffset;
			public int SectionStartOffset;
			public int Unk11;
			public int SectionCount;
			public int Unk13;
			public int Unk14;
		}
		
		public struct CaffSection
		{
			public string Name;
			public SectionType Type;
			public int Flags;
			public int DecompressedSize;
			public int Unk5;
			public int Unk6;
			public int Unk7;
			public int Unk8;
			public int CompressedSize;
			public int Offset;
		}
		
		public CaffHeader Header = new CaffHeader();
		public CaffSection[] Sections = null;
		public string FileName = "";
		BinaryReader bStream = null;
		public TreeNode TreeViewNode = null;
		
		public List<Texture> Textures = new List<Texture>();
		public List<RenderGraph> RenderGraphs = new List<RenderGraph>();
		
		public CaffFile()
		{
			
		}
		
		public bool Read(string _fileName)
		{
			FileName = _fileName;
			TreeViewNode = new TreeNode(Path.GetFileName(FileName));
			TreeViewNode.Tag = new TreeNodeTag(this, 0, 0, 0, TreeNodeTag.NodeType.File);
			FileStream fs = new FileStream(_fileName, FileMode.Open);
			bStream = new BinaryReader(fs);
			Header.HeaderString = IO.ReadStringF(bStream, 0x14);
			if(Header.HeaderString != "CAFF28.01.05.0031")
			{
				Logger.LogError("Error reading Caff File, Header section mismatch.\n");
				return false;
			}
			
			Header.Unk1 = IO.ReadBig32(bStream); //0x14
			Header.FileCount = IO.ReadBig32(bStream); //0x18
			Header.TocEntryCount = IO.ReadBig32(bStream); //0x1c
			
			Header.Unk4 = IO.ReadBig32(bStream);	//0x20
			Header.Unk5 = IO.ReadBig32(bStream);
			Header.Unk6 = IO.ReadBig32(bStream);
			Header.Unk7 = IO.ReadBig32(bStream);
			
			Header.Unk8 = IO.ReadBig32(bStream);	//0x30
			Header.TocOffset = IO.ReadBig32(bStream);
			Header.SectionStartOffset = IO.ReadBig32(bStream);
			Header.Unk11 = IO.ReadByte(bStream);
			Header.SectionCount = IO.ReadByte(bStream);
			Header.Unk13 = IO.ReadByte(bStream);
			Header.Unk14 = IO.ReadByte(bStream);
			
			Sections = new CaffSection[Header.SectionCount];
			int sectionOffset = Header.SectionStartOffset;
			for(int i = 0; i < Header.SectionCount; i++)
			{
				CaffSection tmpSection = new CaffSection();
				tmpSection.Name = IO.ReadString(bStream);
				IO.Align(bStream, 4);
				int tmpSectionType = IO.ReadBig32(bStream);
				
				switch (tmpSectionType)
				{
					case SECTION_TYPE_DATA:
					{
						tmpSection.Type = SectionType.Data;
						break;
					}
					case SECTION_TYPE_GPU:
					{
						tmpSection.Type = SectionType.GPU;
						break;
					}
					case SECTION_TYPE_STREAM:
					{
						tmpSection.Type = SectionType.Stream;
						break;
					}
					default:
					{
							Logger.LogError(string.Format("Error reading Caff File, Unknown caff section type {0:X} at 0x{1:X}", tmpSectionType, (int)bStream.BaseStream.Position));
							return false;
					}
				}
				
				tmpSection.Flags = IO.ReadBig32(bStream);
				tmpSection.DecompressedSize = IO.ReadBig32(bStream);
				tmpSection.Unk5 = IO.ReadBig32(bStream);
				tmpSection.Unk6 = IO.ReadBig32(bStream);
				tmpSection.Unk7 = IO.ReadBig32(bStream);
				tmpSection.Unk8 = IO.ReadBig32(bStream);
				tmpSection.CompressedSize = IO.ReadBig32(bStream);
				tmpSection.Offset = sectionOffset;
				Sections[i] = tmpSection;
				//If section isn't comrpessed, then CompressedSize and DecompressedSize will match anyways.
				sectionOffset += tmpSection.CompressedSize;
			}
			
			ReadContents();
			
			return true;
		}
		
		public BinaryReader GetSectionData(int sectionID)
		{
			if(sectionID < Header.SectionCount)
			{
				CaffSection tmpSection = Sections[sectionID];
				bStream.BaseStream.Seek(tmpSection.Offset, SeekOrigin.Begin);
				if(tmpSection.CompressedSize != tmpSection.DecompressedSize)
				{

					#if CAFF_SECTIONS_IN_MEMORY
						ZlibStream zs = new ZlibStream(bStream.BaseStream, CompressionMode.Decompress, CompressionLevel.BestCompression);
						byte[] decompressedData = new byte[tmpSection.DecompressedSize];
						MemoryStream ms = new MemoryStream(decompressedData);
						zs.CopyTo(ms);
						return new BinaryReader(ms);
					#else
					var fs = new FileStream(FileName + tmpSection.Name, FileMode.OpenOrCreate);
					var zs = new ZlibStream(bStream.BaseStream, CompressionMode.Decompress, CompressionLevel.BestCompression);
					zs.CopyTo(fs);
					var bs = new BinaryReader(fs);
					return bs;
					#endif
				}
				else
				{
					#if CAFF_SECTIONS_IN_MEMORY
						byte[] decompressedData = new byte[tmpSection.DecompressedSize];
						bStream.Read(decompressedData, 0, tmpSection.DecompressedSize);
						MemoryStream ms = new MemoryStream(decompressedData);
						return new BinaryReader(ms);
					#else
					var fs = new FileStream(FileName + tmpSection.Name, FileMode.OpenOrCreate);
					var buffer = new byte[0x800];
					int bytesRead = 0;
					while(bytesRead < tmpSection.DecompressedSize)
					{
						var toRead = Math.Min(tmpSection.DecompressedSize - bytesRead, 0x800);
						var readNow = bStream.Read(buffer, 0, toRead);
						if(readNow == 0) break;
						fs.Write(buffer, 0, readNow);
						bytesRead += readNow;
					}
					fs.Seek(0, SeekOrigin.Begin);
					return new BinaryReader(fs);
					#endif
				}
			}
			return null;
		}

		public virtual void ReadContents()
		{
		}
		
		
		public void Close()
		{
			if(bStream != null)
			{
				bStream.Close();
				bStream = null;
			}
		}
		
		public bool Init()
		{
			
			foreach(var tmpTexture in Textures)
			{
				tmpTexture.Init();
			}

			
			foreach(var tmpRenderGraph in RenderGraphs)
			{
				tmpRenderGraph.Init();
			}
			
			return  true;
		}
		
		public bool DeInit()
		{
			foreach(var tmpTexture in Textures)
			{
				tmpTexture.DeInit();
			}
			
			foreach(var tmpRenderGraph in RenderGraphs)
			{
				tmpRenderGraph.DeInit();
			}
			
			return true;
		}
		
		public Texture GetTexture(int id)
		{
			if(id < Textures.Count)
			{
				return Textures[id];
			}
			
			return null;
		}
		
		public RenderGraph GetRenderGraph(int id)
		{
			if(id < RenderGraphs.Count)
			{
				return RenderGraphs[id];
			}
			
			return null;
		}
		
		public void DumpToOBJ(string outputPath, bool splitSubModels = true)
		{
			string caffFileName = Path.GetFileNameWithoutExtension(FileName);
			string outputFileName = Path.Combine(outputPath, caffFileName);
			Logger.LogInfo(string.Format("Dumping {0} to {1}.obj...", FileName, outputFileName));
			
			//dump OBJ
			int rgID = 0;
			using(System.IO.StreamWriter fs = new StreamWriter(outputFileName + ".obj", false))
			{
				fs.WriteLine(string.Format("mtllib {0}.mtl", caffFileName));
				int totalVertCount = 1;
				foreach(var tmpRG in RenderGraphs)
				{
					totalVertCount = tmpRG.DumpToObj(fs, totalVertCount, rgID, splitSubModels);
					rgID++;
				}
			}
			
			//make mtllib
			using(System.IO.StreamWriter fs = new StreamWriter(outputFileName + ".mtl", false))
			{
				for(int i = 0; i < Textures.Count; i++)
				{
					var tmpTexture = Textures[i];
					tmpTexture.DumpToMtl(fs, Path.GetDirectoryName(outputFileName), i);
				}
				
			}
			
			Logger.LogInfo("Done.");
			
		}
		
		public void RecropTexture(int textureID)
		{
			if(textureID < Textures.Count)
			{
				var tmpTexture = Textures[textureID];
				if(tmpTexture != null)
				{
					var tmpGSection = GetSectionData(1);
					tmpTexture.ReCrop(tmpGSection);
					tmpGSection.Close();
				}
			}
		}
		
		public void DumpTextureToXpr(int textureID)
		{
			if(textureID < Textures.Count)
			{
				var tmpTexture = Textures[textureID];
				if(tmpTexture != null)
				{
					string baseDir = System.IO.Path.GetDirectoryName(FileName);
					string baseFileName = System.IO.Path.GetFileNameWithoutExtension(FileName);
					string outputName = string.Format("{0}_{1}.xpr", baseFileName, textureID);
					
					var gStream = GetSectionData(1);
					tmpTexture.DumpToXpr(gStream, System.IO.Path.Combine(baseDir, outputName));
					gStream.Close();
				}
			}
		}
		
	}
}
