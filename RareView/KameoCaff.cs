/*
 * Created by SharpDevelop.
 * User: NCDyson
 * Date: 1/14/2018
 * Time: 6:22 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;

namespace RareView
{
	/// <summary>
	/// Description of KameoCaff.
	/// </summary>
	public class KameoCaff : CaffFile
	{
		public class Caff_Toc_Entry
		{
			public int ID;
			public int Offset;
			public int Size;
			public byte SectionID;
			public byte Unk4;
		}
		
		public struct TocEntry_Section
		{
			public int Offset;
			public int Size;
			public int Unk4;
		}
		
		public class Caff_SubObject
		{
			public TocEntry_Section[] Sections = null;
			
			public Caff_SubObject()
			{
				Sections = new TocEntry_Section[3];
			}
			
			public Caff_SubObject(int sectionCount)
			{
				Sections = new TocEntry_Section[sectionCount];
			}
			
			public void SetSection(int sectionID, int offset, int size, int unk4)
			{
				if(sectionID < Sections.Length)
				{
					Sections[sectionID] = new TocEntry_Section()
					{
						Offset = offset,
						Size = size,
						Unk4 = unk4
					};
				}
			}
			
			public TocEntry_Section GetSectionInfo(int sectionID)
			{
				if(sectionID < Sections.Length)
				{
					return Sections[sectionID];
				}
				return new TocEntry_Section()
				{
					Offset = -1,
					Size = -1,
					Unk4 = -1
				};
			}
		}
		
		public Caff_SubObject[] SubObjects = null;
		
		public KameoCaff()
		{
		}
		
		public override void ReadContents(MainForm.UpdateStatusDelegate UpdateDelegate = null)
		{
			var DataSection = GetSectionData(0);
			var GPUSection = GetSectionData(1);
			DataSection.BaseStream.Seek(Header.TocOffset + 4, System.IO.SeekOrigin.Begin);
			
			SubObjects = new Caff_SubObject[Header.FileCount];
			for(int i = 0; i < Header.TocEntryCount; i++)
			{
				if(UpdateDelegate != null)
				{
					float percent = (100.0f / Header.TocEntryCount) * i;
					UpdateDelegate((int)percent, "Reading Caf TOC...");
				}
				int tmpEntryID = IO.ReadBig32(DataSection) - 1;
				Caff_SubObject tmpSubObj = SubObjects[tmpEntryID];
				if(tmpSubObj == null)
				{
					tmpSubObj = new Caff_SubObject();
					SubObjects[tmpEntryID] = tmpSubObj;
				}
				
				int tmpOffset = IO.ReadBig32(DataSection);
				int tmpSize = IO.ReadBig32(DataSection);
				int tmpSectionID = IO.ReadByte(DataSection);
				int tmpUnk4 = IO.ReadByte(DataSection);
				tmpSubObj.SetSection(tmpSectionID - 1, tmpOffset, tmpSize, tmpUnk4);
			}
			
			#if VERBOSE_LOGGING
			using(var fs = new FileStream(FileName + ".txt", FileMode.OpenOrCreate))
			{
				using(var ss = new StreamWriter(fs))
				{
					int subObjID = 0;
					foreach(var tmpSubObj in SubObjects)
					{
						var tmpDataSection = tmpSubObj.GetSectionInfo(0);
						var tmpGPUSection = tmpSubObj.GetSectionInfo(1);
						var tmpStreamSection = tmpSubObj.GetSectionInfo(2);
						ss.WriteLine(string.Format("SubObject {0}", subObjID));
						if(tmpDataSection.Offset != -1) ss.WriteLine(string.Format("\t.data: Offset: 0x{0:X}, Size: 0x{1:X}", tmpDataSection.Offset, tmpDataSection.Size));
						if(tmpGPUSection.Offset != -1) ss.WriteLine(string.Format("\t.gpu: Offset: 0x{0:X}, Size: 0x{1:X}", tmpGPUSection.Offset, tmpGPUSection.Size));
						if(tmpStreamSection.Offset != -1) ss.WriteLine(string.Format("\t.stream: Offset: 0x{0:X}, Size: 0x{1:X}", tmpStreamSection.Offset, tmpStreamSection.Size));
						subObjID++;	
					}
				}
			}
			#endif
			
			//var tmpXPR = new XPRFile();
			string baseDir = System.IO.Path.GetDirectoryName(FileName);
			string baseFileName = System.IO.Path.GetFileNameWithoutExtension(FileName);
			
			int tmpSubObjID = 0;
			foreach(var tmpSubObj in SubObjects)
			{
				if(UpdateDelegate != null)
				{
					float percent = (100.0f / SubObjects.Length) * tmpSubObjID;
					UpdateDelegate((int)percent, "Reading Contents...");
				}
				var tmpDataSectionInfo = tmpSubObj.GetSectionInfo(0);
				var tmpGPUSectionInfo = tmpSubObj.GetSectionInfo(1);
				if(tmpDataSectionInfo.Offset != -1)
				{
					DataSection.BaseStream.Seek(tmpDataSectionInfo.Offset, SeekOrigin.Begin);
					var tmpTypeStr = IO.ReadString(DataSection);
					if(tmpTypeStr == "texture")
					{
						var tmpTexture = new Texture();
						DataSection.BaseStream.Seek(tmpDataSectionInfo.Offset, SeekOrigin.Begin);
						if(tmpGPUSectionInfo.Offset != -1) GPUSection.BaseStream.Seek(tmpGPUSectionInfo.Offset, SeekOrigin.Begin);
						else
						{
							Logger.LogError(string.Format("Texture Block at 0x{0:X} has no GPU Section Data.\n", tmpDataSectionInfo.Offset));
							continue;
						}
						
						if(tmpTexture.Read(DataSection, GPUSection))
						{
							
							Textures.Add(tmpTexture);
							var tmpNode = tmpTexture.ToNode(Textures.Count - 1);
							tmpNode.Tag = new TreeNodeTag(this, Textures.Count - 1, TreeNodeTag.OBJECT_TYPE_TEXTURE, 0, TreeNodeTag.NodeType.Main);
							TreeViewNode.Nodes.Add(tmpNode);
						}
					}
					else if(tmpTypeStr == "rendergraph")
					{
						var tmpRenderGraph = new RenderGraph(this, RenderGraphs.Count);
						DataSection.BaseStream.Seek(tmpDataSectionInfo.Offset, SeekOrigin.Begin);
						if(tmpGPUSectionInfo.Offset != -1) GPUSection.BaseStream.Seek(tmpGPUSectionInfo.Offset, SeekOrigin.Begin);
						else
						{
							Logger.LogError(string.Format("RenderGraph Block at 0x{0:X} has no GPU Section.\n", tmpDataSectionInfo.Offset));
							continue;
						}
						
						if(tmpRenderGraph.Read(DataSection, GPUSection, tmpDataSectionInfo.Size, tmpGPUSectionInfo.Size))
						{
							RenderGraphs.Add(tmpRenderGraph);
							var tmpNode = tmpRenderGraph.ToNode(RenderGraphs.Count - 1);
							tmpNode.Tag = new TreeNodeTag(this, RenderGraphs.Count - 1, TreeNodeTag.OBJECT_TYPE_RENDERGRAPH, 0, TreeNodeTag.NodeType.Main);
							TreeViewNode.Nodes.Add(tmpNode);
						}
					}
					
				}
				tmpSubObjID++;
			}
			
			//tmpXPR.Write(System.IO.Path.Combine(baseDir, string.Format("{0}.xpr", baseFileName)));
			DataSection.Close();
			GPUSection.Close();
		}
		

		

	}
}
