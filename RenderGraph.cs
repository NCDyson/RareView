/*
 * Created by SharpDevelop.
 * User: NCDyson
 * Date: 1/6/2018
 * Time: 3:42 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
 //#define DUMP_RENDERGRAPHS
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.ComponentModel;



namespace RareView
{
	/// <summary>
	/// Description of RenderGraph.
	/// </summary>
	public class RenderGraph
	{	
		
		private const string vsSource = @"#version 330 core
layout(location=0) in vec3 Position;
layout(location=1) in vec2 TexCoords;

uniform mat4 Matrix;
uniform vec4 ColorOverride;
out vec2 fTexCoords;
out vec4 fColor;
void main()
{
	if(ColorOverride.w == -1.0)
	{
		gl_Position = Matrix * vec4(TexCoords.x * ColorOverride.x, TexCoords.y * ColorOverride.y, 0.25, 1.0);
	}
	else
	{
		gl_Position = Matrix * vec4(Position, 1.0);
	}
	fTexCoords = TexCoords;
	fColor = ColorOverride;
}
";
		
		private const string fsSource = @"#version 330 core
uniform sampler2D Texture;
in vec2 fTexCoords;
in vec4 fColor;
out vec4 color;

void main()
{
	if(fColor.w == 1.0)
	{
		color = vec4(fColor.x, fColor.y, fColor.z, 1.0);
	}
	else if(fColor.w == -1.0)
	{
		color = vec4(1.0, 1.0, 1.0, 1.0);
	}
	else
	{
		color = texture2D(Texture, fTexCoords);
	}
}
";
	
		[StructLayout(LayoutKind.Sequential)]
		public struct Vertex
		{
			public Vector3 Position;
			public Vector2 TexCoords;
		}
		
		
		public class VertexBatch
		{
			public Vertex[] Vertices = null;
			public int VertexCount = 0;
			public int VertexStride = 0;
			public int ArrayID = -1;
			public int BufferID = -1;
			public bool WasInit = false;
			public string VType = "";
			public int VDataOffset = 0;
			public int VGPUOffset = 0;
			public byte[] RawData = null;
			public BinaryReader bStream = null;
			
			public int vD_PositionOffset = 4;
			[Category("Position Options")]
			[DisplayName("Position Offset")]
			public int VD_PositionOffset {
				get {
					return vD_PositionOffset;
				}
				set {
					vD_PositionOffset = value;
					ParseVertices(true);
				}
			}

			public bool vD_FullPosition = false;
			[Category("Position Options")]
			[DisplayName("32bit")]
			[Description("True if Positions are 32bit float, or False if Positions are 16bit half float")]
			
			public bool VD_FullPosition {
				get {
					return vD_FullPosition;
				}
				set {
					vD_FullPosition = value;
					ParseVertices(true);
				}
			}

			public int vD_TexCoordOffset = 0;
			[Category("Texture Coordinate Options")]
			[DisplayName("Texture Coordinate Offset")]
			public int VD_TexCoordOffset {
				get {
					return vD_TexCoordOffset;
				}
				set {
					vD_TexCoordOffset = value;
					ParseVertices(true);
				}
			}

			public bool vD_FullTexCoord = false;
			[Category("Texture Coordinate Options")]
			[DisplayName("32bit")]
			[Description("True if Texture Coordinates are 32bit float, or False if texture coordinates are 16bit half float")]
			public bool VD_FullTexCoord {
				get {
					return vD_FullTexCoord;
				}
				set {
					vD_FullTexCoord = value;
					ParseVertices(true);
				}
			}
			
			public bool vD_FlipTexCoord = false;
			[Category("Texture Coordinate Options")]
			[DisplayName("Flip")]
			[Description("Flip along Y Axis")]
			public bool VD_FlipTexCoord {
				get {
					return vD_FlipTexCoord;
				}
				set {
					vD_FlipTexCoord = value;
					ParseVertices(true);
				}
			}
			
			public float vD_TexCoordScale = 4096.0f;
			[Category("Texture Coordinate Options")]
			[DisplayName("Scale")]
			public float VD_TexCoordScale {
				get {
					return vD_TexCoordScale;
				}
				set {
					vD_TexCoordScale = value;
					ParseVertices(true);
				}
			}
			
			public bool vD_HasTexCoords = true;
			[Category("Texture Coordinate Options")]
			[DisplayName("Has TexCoords")]
			public bool VD_HasTexCoords {
				get {
					return vD_HasTexCoords;
				}
				
				set {
					vD_HasTexCoords = value;
					ParseVertices(true);
				}
			}
			
			public VertexBatch()
			{
			}
			
			public Vector2 ReadUVHF(BinaryReader bStream)
			{
				float tmpU = IO.ReadBig16(bStream) / vD_TexCoordScale;
				float tmpV = IO.ReadBig16(bStream) / vD_TexCoordScale;
				
				if(vD_FlipTexCoord) return new Vector2(tmpU, 1.0f - tmpV);
				else return new Vector2(tmpU, tmpV);
			}
			
			public static Vector2 ReadUV(BinaryReader bStream)
			{
				float tmpU = IO.ReadBigF32(bStream);
				float tmpV = IO.ReadBigF32(bStream);
				return new Vector2(tmpU, 1.0f - tmpV);
			}
			
			public static Vector3 ReadPositionHF(BinaryReader bStream)
			{
				const float posScale = (32762.0f / 2);
				float tmpX = IO.ReadBig16(bStream) / posScale;
				float tmpY = IO.ReadBig16(bStream) / posScale;
				float tmpZ = IO.ReadBig16(bStream) / posScale;
				return new Vector3(tmpX, tmpY, tmpZ);
			}
			
			public static Vector3 ReadPosition(BinaryReader bStream)
			{
				float tmpX = IO.ReadBigF32(bStream);
				float tmpY = IO.ReadBigF32(bStream);
				float tmpZ = IO.ReadBigF32(bStream);
				return new Vector3(tmpX, tmpY, tmpZ);
			}
			
			public void ParseVertices(bool doUpdate = false)
			{
				for(int i = 0; i < VertexCount; i++)
				{
					var tmpV = new Vertex();
					int vOffs = i * VertexStride;
					bStream.BaseStream.Seek(vOffs + vD_PositionOffset, SeekOrigin.Begin);
					tmpV.Position = (vD_FullPosition) ? ReadPosition(bStream) : ReadPositionHF(bStream);
					bStream.BaseStream.Seek(vOffs + vD_TexCoordOffset, SeekOrigin.Begin);
					if(vD_HasTexCoords) tmpV.TexCoords = (vD_FullTexCoord) ? ReadUV(bStream) : ReadUVHF(bStream);
					else tmpV.TexCoords = Vector2.Zero;
					Vertices[i] = tmpV;
				}
				
				if(doUpdate) Update(true);
			}
			
			public bool Read(BinaryReader gStream, int vStride, int vBlockSize, int vBatchID)
			{
				if(RawData != null || bStream != null)
				{
					Logger.LogWarning(string.Format("Overwriting Raw Data for Vertex Batch {0}!\n", vBatchID));
				}
				
				RawData = gStream.ReadBytes(vBlockSize);
				var ms = new MemoryStream(RawData);
				bStream = new BinaryReader(ms);
				VertexStride = vStride;
				VertexCount = (vBlockSize / vStride);
				
				Vertices = new Vertex[VertexCount];
				ParseVertices();
				return true;
			}
			
			public bool Init()
			{	
				ArrayID = GL.GenVertexArray();
				BufferID = GL.GenBuffer();
				
				GL.BindVertexArray(ArrayID);
				GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID);
				
				Type VertexType = Vertices[0].GetType();
				int VertexSize = Marshal.SizeOf(VertexType);
				
				GL.BufferData(BufferTarget.ArrayBuffer, (VertexSize * VertexCount), Vertices, BufferUsageHint.StaticDraw);
				
				GL.EnableVertexAttribArray(AttribPosition);
				GL.VertexAttribPointer(AttribPosition, 3, VertexAttribPointerType.Float, false, VertexSize, Marshal.OffsetOf(VertexType, "Position"));
				GL.EnableVertexAttribArray(AttribTexCoords);
				GL.VertexAttribPointer(AttribTexCoords, 2, VertexAttribPointerType.Float, false, VertexSize, Marshal.OffsetOf(VertexType, "TexCoords"));
				
				GL.BindVertexArray(0);
				GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
				
				WasInit = true;
				return true;
			}
			
			public void DeInit()
			{
				if(ArrayID != -1) GL.DeleteVertexArray(ArrayID);
				ArrayID = -1;
				if(BufferID != -1) GL.DeleteBuffer(BufferID);
				BufferID = -1;
				WasInit = false;
			}
			
			
			
			public bool Bind()
			{
				if(!WasInit) return false;
				GL.BindVertexArray(ArrayID);
				return true;
			}
			
			public void Update(bool unbindAfter = false)
			{
				int vertexSize = Marshal.SizeOf(Vertices[0].GetType());
				
				GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID);
				GL.BufferData(BufferTarget.ArrayBuffer, (vertexSize * VertexCount), Vertices, BufferUsageHint.StaticDraw);
				
				if(unbindAfter) GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			}
			
		}
		
		public class TriStripBatch
		{
			public int[] TSIndices = null;
			public int TriStripCount = 0;
			public int ElementID = -1;
			public int VertexBatchID = -1;
			public bool WasInit = false;
			public int MaterialID = 0;
			
			public TriStripBatch()
			{	
			}
			
			public void Read(int tsCount, int vBatchID, BinaryReader gStream, bool fullPrecision = false)
			{
				VertexBatchID = vBatchID;
				TriStripCount = tsCount + 2;
				TSIndices = new int[TriStripCount];
				
				for(int i = 0; i < TriStripCount; i++)
				{
					if(fullPrecision) TSIndices[i] = IO.ReadBig32(gStream);
					else TSIndices[i] = IO.ReadBig16(gStream);
				}
			}
			
			public bool Init()
			{
				ElementID = GL.GenBuffer();
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementID);
				GL.BufferData(BufferTarget.ElementArrayBuffer, TriStripCount * Marshal.SizeOf(TSIndices[0].GetType()), TSIndices, BufferUsageHint.StaticDraw);
				
				WasInit = true;
				return true;
			}
			
			public bool Bind()
			{
				if(!WasInit) return false;
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementID);
				
				return true;
			}
			
			public void DeInit()
			{
				if(ElementID != -1) GL.DeleteBuffer(ElementID);
				ElementID = -1;
			}
			
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct Triangle
		{
			public int id1;
			public int id2;
			public int id3;
			
			public Triangle(int _id1, int _id2, int _id3)
			{
				id1 = _id1;
				id2 = _id2;
				id3 = _id3;
			}
		}
		
		public class TriangleBatch
		{
			public Triangle[] Triangles = null;
			public int TriangleCount = 0;
			public int ElementID = -1;
			public int VertexBatchID = -1;
			public bool WasInit = false;
			public int MaterialID = 0;
			
			public TriangleBatch()
			{
				
			}
			
			public void Read(int tsCount, int vBatchID, BinaryReader gStream, bool fullPrecision = false)
			{
				VertexBatchID = vBatchID;
				TriangleCount = tsCount + 2;
				Triangles = new Triangle[TriangleCount];
				
				int lastID2 = (fullPrecision) ? IO.ReadBig32(gStream) : IO.ReadBig16(gStream);
				int lastID1 = (fullPrecision) ? IO.ReadBig32(gStream) : IO.ReadBig16(gStream);
				
				int tCount = 0;
				for(int i = 2; i < TriangleCount; i++)
				{
					int currentID = (fullPrecision) ? IO.ReadBig32(gStream) : IO.ReadBig16(gStream);
					if(currentID != lastID1 && currentID != lastID2 && lastID1 != lastID2) 
					{
					
						if((i % 2) == 1)
						{
							Triangles[tCount] = new Triangle(currentID, lastID1, lastID2);
						}
						else
						{
							Triangles[tCount] = new Triangle(lastID2, lastID1, currentID);
						}
						
						tCount++;
					}
					
					lastID2 = lastID1;
					lastID1 = currentID;
				}
				
				TriangleCount = tCount + 1;
			}
			
			public bool Init()
			{
				ElementID = GL.GenBuffer();
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementID);
				GL.BufferData(BufferTarget.ElementArrayBuffer, TriangleCount * Marshal.SizeOf(Triangles[0].GetType()), Triangles, BufferUsageHint.StaticDraw);
				
				WasInit = true;
				return true;
			}
			
			public bool Bind()
			{
				if(!WasInit) return false;
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementID);
				
				return true;
			}
			
			public void DeInit()
			{
				if(ElementID != -1) GL.DeleteBuffer(ElementID);
				ElementID = -1;
			}
		}
		
		//or, shader_whatever
		public class Material
		{
			public int textureID = 0;

			public int TextureID {
				get {
					return textureID;
				}
				set {
					if(value < 0) textureID = -1;
					else textureID = value;
				}
			}

			public string MaterialName = "";
			
			public Material()
			{
			}
			
			public Material(string name)
			{
				MaterialName = name;
			}
		}
		
		public List<VertexBatch> VertexBatches = new List<VertexBatch>();
		public List<TriStripBatch> TriStripBatches = new List<TriStripBatch>();
		public List<TriangleBatch> TriangleBatches = new List<TriangleBatch>();
		public List<Material> MaterialList = new List<Material>();
		public Dictionary<int, int> VertexBatchMap = new Dictionary<int, int>();
		public Dictionary<string, int> MaterialMap = new Dictionary<string, int>();
		public int DataOffset = 0;
		public int GPUOffset = 0;
		
		public CaffFile ParentFile = null;
		public int ObjectID = 0;
		public static bool WasInit = false;
		public static int ProgramID = -1;
		public static int ProgramRefCount;
		public static int AttribPosition = -1;
		public static int AttribTexCoords = -1;
		public static int UniformMatrix = -1;
		public static int UniformTexture = -1;
		public static int UniformColor = -1;
		public static bool ProgramWasInit = false;
		
		public RenderGraph(CaffFile _parentFile, int _objectID)
		{
			ParentFile = _parentFile;
			ObjectID = _objectID;
		}
		
		public bool Read(BinaryReader DataSection, BinaryReader GPUSection, int dataSectionSize, int gpuSectionSize)
		{
			DataOffset = (int)DataSection.BaseStream.Position;
			GPUOffset = (int)GPUSection.BaseStream.Position;
			
			int rendergraphID = ParentFile.RenderGraphs.Count;
			
			Debug.WriteLine(string.Format("Reading RenderGraph {0}", rendergraphID));
			
			#if DUMP_RENDERGRAPHS
			string dumpPath = Path.GetDirectoryName(ParentFile.FileName);
			string baseName = Path.GetFileNameWithoutExtension(ParentFile.FileName);
			
			using(var ofs = new FileStream(Path.Combine(dumpPath, string.Format("{0}_{1}.rg", baseName, rendergraphID)), FileMode.OpenOrCreate))
			{
				using(var osw = new StreamWriter(ofs))
				{
					var oData = DataSection.ReadBytes(dataSectionSize);
					ofs.Write(oData, 0, dataSectionSize);
				}
			}
			#endif
			
			DataSection.BaseStream.Seek(DataOffset + 0x2c, SeekOrigin.Begin);
			int pttOffset = IO.ReadBig32(DataSection);
			DataSection.BaseStream.Seek(DataOffset + pttOffset, SeekOrigin.Begin);
			//Debug.WriteLine(string.Format("Reading PTT at 0x{0:X}", pttOffset));
			int ptCount = IO.ReadBig32(DataSection);
			DataSection.BaseStream.Seek(0xc, SeekOrigin.Current);
			
			int startIndex = 0;
			for(int i = 0; i < ptCount; i++)
			{
				int pt1 = IO.ReadBig32(DataSection);
				int pt2 = IO.ReadBig32(DataSection);
				int val = IO.ReadBig32(DataSection);
				
				string pts1 = IO.ReadStringFrom(DataSection, DataOffset + pt1);
				//Debug.WriteLine(pts1);
				if(pts1.ToLower() == "numtextures")
				{
					startIndex = i;
					break;
				}
			}
			
			for(int i = 0; i < ptCount - (startIndex + 1); i++)
			{
				int pt1 = IO.ReadBig32(DataSection);
				int pt2 = IO.ReadBig32(DataSection);
				int val = IO.ReadBig32(DataSection);
				//Debug.WriteLine(string.Format("Entry_{0} at 0x{1:X}:  {2:X}, {3:X}: 0x{4:X}",i,DataSection.BaseStream.Position - DataOffset, pt1, pt2, val));
				if(!CheckReadMesh(DataSection, GPUSection, pt1, val))
				{
					break;
				}
			}
			
			return true;
		}
		
		public bool CheckReadMesh(BinaryReader dStream, BinaryReader gStream, int sName, int dOffset)
		{
			var strName = IO.ReadStringFrom(dStream, DataOffset + sName);
			Debug.WriteLine(string.Format("\tShader Name: {0}", strName));
			if(!strName.StartsWith("shader", StringComparison.CurrentCulture)) return false;
			
			//Debug.WriteLine(string.Format("Reading RenderGraph subMesh at 0x{0:X}.", dOffset));
			long oldPos = dStream.BaseStream.Position;
			dStream.BaseStream.Seek(DataOffset + dOffset + 0x14, SeekOrigin.Begin);
			int tsDOffset = IO.ReadBig32(dStream);
			IO.ReadBig32(dStream);
			int vDOffset = IO.ReadBig32(dStream);
			dStream.BaseStream.Seek(DataOffset + dOffset + 0x50, SeekOrigin.Begin);
			int tsBatchCount = IO.ReadBig32(dStream);
			//first of all, check to make sure we haven't read this vertex batch first...
			//Debug.WriteLine(string.Format("Reading VertexBatch at 0x{0:X}...", vDOffset));
			if(!VertexBatchMap.ContainsKey(vDOffset))
			{
				//Hacky, but it works for now.
				if(vDOffset > (int)dStream.BaseStream.Length)
				{
					return false;
				}

				dStream.BaseStream.Seek(DataOffset + vDOffset + 0x30, SeekOrigin.Begin);
				int vDOffset2 = IO.ReadBig32(dStream);
				dStream.BaseStream.Seek(DataOffset + vDOffset2, SeekOrigin.Begin);
				
				var tmpBatch = new VertexBatch();
				tmpBatch.VDataOffset = vDOffset2;
				
				int tmpStride = IO.ReadBig32(dStream);
				IO.ReadBig32(dStream);
				IO.ReadBig32(dStream);
				int tmpGPUOffset = IO.ReadBig32(dStream);
				int tmpGPUBlockSize = IO.ReadBig32(dStream);
				string tmpVType = IO.ReadString(dStream);
				tmpBatch.VGPUOffset = tmpGPUOffset;
				gStream.BaseStream.Seek(GPUOffset + tmpGPUOffset, SeekOrigin.Begin);
				tmpBatch.Read(gStream, tmpStride, tmpGPUBlockSize, VertexBatches.Count);
				tmpBatch.VType = tmpVType;
				VertexBatches.Add(tmpBatch);
				VertexBatchMap[vDOffset] = VertexBatches.Count - 1;
			}

			//Now, read TriangleStrip Data...
			int vBatchID = VertexBatchMap[vDOffset];
			
			/*
			int nextBatchOffset = tsDOffset;
			//for(int i = 0; i < tsBatchCount; i++)
			while(nextBatchOffset != 0)
			{
				dStream.BaseStream.Seek(DataOffset + nextBatchOffset, SeekOrigin.Begin);
				
				int stallCheck = 0;
				while(true)
				{
					int tmpCheck = IO.ReadBig32(dStream);
					if(tmpCheck == tsDOffset) break;
					if(stallCheck > 0x80)
					{
						Logger.LogError(string.Format("Stalled looking for TriangleStrip descriptor at 0x{0:X}\n", tsDOffset));
						return true;
					}
					stallCheck += 1;
					nextBatchOffset = tmpCheck;
				}
				
				dStream.BaseStream.Seek(0x1c, SeekOrigin.Current);
				int gpuOffsetOffset = IO.ReadBig32(dStream);
				IO.ReadBig32(dStream);
				int gpuCountOffset = IO.ReadBig32(dStream);
				int indexSize = IO.ReadBig16(dStream);
				
				dStream.BaseStream.Seek(DataOffset + gpuCountOffset, SeekOrigin.Begin);
				int tsCount = IO.ReadBig32(dStream);
				dStream.BaseStream.Seek(DataOffset + gpuOffsetOffset, SeekOrigin.Begin);
				int tsOffset = IO.ReadBig32(dStream);
				bool fullIndex = (indexSize == 1);
				
				var tmpTBatch = new TriStripBatch();
				gStream.BaseStream.Seek(GPUOffset + tsOffset, SeekOrigin.Begin);
				tmpTBatch.Read(tsCount, vBatchID, gStream, fullIndex);
				
				dStream.BaseStream.Seek(DataOffset + sName, SeekOrigin.Begin);
				
				string tmpMaterialName = IO.ReadString(dStream);
				TriStripBatches.Add(tmpTBatch);
				
				if(!MaterialMap.ContainsKey(tmpMaterialName))
				{
					var tmpMaterial = new KameoMaterial(tmpMaterialName);
					MaterialList.Add(tmpMaterial);
					MaterialMap[tmpMaterialName] = MaterialList.Count - 1;
					tmpTBatch.MaterialID = MaterialList.Count - 1;
				}
				else
				{
					int materialID = MaterialMap[tmpMaterialName];
					tmpTBatch.MaterialID = materialID;
				}
			}
			*/
			
			while(true)
			{
				int nextTSDescriptorOffset = ReadTriangleStripBatch(dStream, gStream, tsDOffset, vBatchID, sName);
				
				if(nextTSDescriptorOffset == 0) break;
				tsDOffset = nextTSDescriptorOffset;
			}
			
			
			dStream.BaseStream.Seek(oldPos, SeekOrigin.Begin);
			return true;
		}
		
		public void ReadTriangleStripSubBatch(BinaryReader dStream, BinaryReader gStream, int tsDOffset, int vBatchID, int sNameOffset)
		{
			//The amount of recusion in these files is bullshit...
			dStream.BaseStream.Seek(DataOffset + tsDOffset + 0x14, SeekOrigin.Begin);
			int tso = IO.ReadBig32(dStream);
			int nextOffset = IO.ReadBig32(dStream);
			
			if(nextOffset != 0) ReadTriangleStripSubBatch(dStream, gStream, nextOffset, vBatchID, sNameOffset);
			
			if(tso != 0) dStream.BaseStream.Seek(DataOffset + tso + 0x3c, SeekOrigin.Begin);
			else dStream.BaseStream.Seek(DataOffset + tsDOffset + 0x3c, SeekOrigin.Begin);
			
			int tsOffsetPtr = IO.ReadBig32(dStream);
			int tsTypePtr = IO.ReadBig32(dStream);
			int tsCountPtr = IO.ReadBig32(dStream);
			bool tsFullIndex = (IO.ReadBig16(dStream) == 1);
			
			dStream.BaseStream.Seek(DataOffset + tsOffsetPtr, SeekOrigin.Begin);
			int tsGPUOffset = IO.ReadBig32(dStream);
			dStream.BaseStream.Seek(DataOffset + tsTypePtr, SeekOrigin.Begin);
			int tsGPUType = IO.ReadBig32(dStream);
			dStream.BaseStream.Seek(DataOffset + tsCountPtr, SeekOrigin.Begin);
			int tsGPUCount = IO.ReadBig32(dStream);
			
			//var tmpTBatch = new TriStripBatch();
			var tmpTBatch = new TriangleBatch();
			gStream.BaseStream.Seek(GPUOffset + tsGPUOffset, SeekOrigin.Begin);
			tmpTBatch.Read(tsGPUCount, vBatchID, gStream, tsFullIndex);
			
			dStream.BaseStream.Seek(DataOffset + sNameOffset, SeekOrigin.Begin);
			string tmpMaterialName = IO.ReadString(dStream);
			//TriStripBatches.Add(tmpTBatch);
			TriangleBatches.Add(tmpTBatch);
			if(!MaterialMap.ContainsKey(tmpMaterialName))
			{
				var tmpMaterial = new Material(tmpMaterialName);
				MaterialList.Add(tmpMaterial);
				MaterialMap[tmpMaterialName] = MaterialList.Count - 1;
				tmpTBatch.MaterialID = MaterialList.Count - 1;
			}
			else
			{
				tmpTBatch.MaterialID = MaterialMap[tmpMaterialName];
			}
		}
		
		public int ReadTriangleStripBatch(BinaryReader dStream, BinaryReader gStream, int tsDOffset, int vBatchID, int sNameOffset)
		{
			dStream.BaseStream.Seek(DataOffset + tsDOffset + 0x14, SeekOrigin.Begin);
			int tsDescriptorOffset = IO.ReadBig32(dStream);
			int nextTSDescriptorOffset = IO.ReadBig32(dStream);
			
			if(tsDescriptorOffset == 0)
			{
				ReadTriangleStripSubBatch(dStream, gStream, tsDOffset, vBatchID, sNameOffset);
			}
			else
			{
				ReadTriangleStripSubBatch(dStream, gStream, tsDescriptorOffset, vBatchID, sNameOffset);
			}
			
			return nextTSDescriptorOffset;
		}
		
		public void Render(Matrix4 mtx, int _objectID = -1)
		{
			if(ProgramID == -1 || !WasInit) return;
			GL.UseProgram(ProgramID);
			
			if(_objectID == -1)
			{
				//for(int i = 0; i < TriStripBatches.Count; i++)
				for(int i = 0; i < TriangleBatches.Count; i++)
				{
					RenderSubModel(mtx, i);
				}
				
				/*
 				for(int i = 0; i < VertexBatches.Count; i++)
				{
					RenderSubModelPoints(mtx, i);
				}
				*/
			}
			else
			{
				RenderSubModel(mtx, _objectID);
			}
			
			GL.UseProgram(0);
		}
		
		public void RenderSubModel(Matrix4 Mtx, int _objectID)
		{
			//var tmpTBatch = TriStripBatches[_objectID];
			var tmpTBatch = TriangleBatches[_objectID];
			var tmpVBatch = VertexBatches[tmpTBatch.VertexBatchID];
			
			tmpVBatch.Bind();
			tmpTBatch.Bind();
			
			Matrix4 finalMtx = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f) * Mtx;
			GL.UniformMatrix4(UniformMatrix, false , ref finalMtx);
			GL.Uniform1(UniformTexture, 0);
			GL.Uniform4(UniformColor, 0.0f, 0.0f, 0.0f, 0.0f);
			var tmpMaterial = GetMaterial(tmpTBatch.MaterialID);
			if(tmpMaterial != null && tmpMaterial.textureID != -1)
			{
				var tmpTexture = ParentFile.GetTexture(tmpMaterial.textureID);
				if(tmpTexture != null)
				{
					GL.BindTexture(TextureTarget.Texture2D, tmpTexture.GLID);
				}
				else GL.BindTexture(TextureTarget.Texture2D, DefaultTexture.GLID);
			}
			else GL.BindTexture(TextureTarget.Texture2D, DefaultTexture.GLID);
			
			GL.DrawElements(PrimitiveType.Triangles, tmpTBatch.TriangleCount * 3, DrawElementsType.UnsignedInt, 0);
			
			if(Scene.RenderWireframeOverlay)
			{
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
				
				GL.Uniform4(UniformColor, 1.0f, 1.0f, 1.0f, 1.0f);
				GL.DrawElements(PrimitiveType.Triangles, tmpTBatch.TriangleCount * 3, DrawElementsType.UnsignedInt, 0);
				
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			}
			
			if(Scene.RenderVertexPointOverlay)
			{
				GL.Uniform4(UniformColor, 1.0f, 1.0f, 1.0f, 1.0f);
				GL.DrawArrays(PrimitiveType.Points, 0, tmpVBatch.VertexCount); 
			}
			
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
			GL.BindVertexArray(0);
		}
		
		
		/*
		public void RenderSubModelPoints(Matrix4 Mtx, int _objectID)
		{
			var tmpVBatch = VertexBatches[_objectID];
			
			tmpVBatch.Bind();
			
			Matrix4 finalMtx = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f) * Mtx;
			GL.Uniform1(UniformTexture, 0);
			GL.Uniform4(UniformColor, 1.0f, 1.0f, 1.0f, 1.0f);
			GL.UniformMatrix4(UniformMatrix, false, ref finalMtx);
			
			GL.DrawArrays(PrimitiveType.Points, 0, tmpVBatch.VertexCount);
			
			GL.BindVertexArray(0);
		}
		*/
		
		public void RenderSubModelUVs(Matrix4 Mtx, int _objectID)
		{
			//var tmpTBatch = TriStripBatches[_objectID];
			var tmpTBatch = TriangleBatches[_objectID];
			var tmpVBatch = VertexBatches[tmpTBatch.VertexBatchID];
			
			tmpVBatch.Bind();
			tmpTBatch.Bind();
			
			Matrix4 finalMtx = Matrix4.CreateTranslation(-0.5f, -0.5f, 0.0f) * Mtx;
			GL.UniformMatrix4(UniformMatrix, false, ref finalMtx);
			var tmpMaterial = GetMaterial(tmpTBatch.MaterialID);
			
			GL.Uniform4(UniformColor, 1.0f, 1.0f, 1.0f, -1.0f);
			GL.Uniform1(UniformTexture, 0);
			
			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
			
			GL.DrawElements(PrimitiveType.Triangles, tmpTBatch.TriangleCount * 3, DrawElementsType.UnsignedInt, 0);
			
			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
			GL.BindVertexArray(0);
		}
		
		
		public void RenderVertexBatch(Matrix4 mtx, int vBatchID)
		{
			if(ProgramID == -1 || !WasInit) return;
			GL.UseProgram(ProgramID);
			
			//for(int i = 0; i < TriStripBatches.Count; i++)
			for(int i = 0; i < TriangleBatches.Count; i++)
			{
				//var tmpTBatch = TriStripBatches[i];
				var tmpTBatch = TriangleBatches[i];
				if(tmpTBatch.VertexBatchID == vBatchID)
				{
					RenderSubModel(mtx, i);
				}
			}
			//RenderSubModelPoints(mtx, vBatchID);
			
			
			GL.UseProgram(0);
		}
		
		public void RendeVertexBatchUVs(Matrix4 mtx, int vBatchID)
		{
			if(ProgramID == -1 || !WasInit) return;
			GL.UseProgram(ProgramID);
			
			//for(int i = 0; i < TriStripBatches.Count; i++)
			for(int i = 0; i < TriangleBatches.Count; i++)
			{
				//var tmpTBatch = TriStripBatches[i];
				var tmpTBatch = TriangleBatches[i];
				if(tmpTBatch.VertexBatchID == vBatchID)
				{
					RenderSubModelUVs(mtx, i);
				}
			}
			
			GL.UseProgram(0);
		}
		
		public void RenderTriangleStripUVs(Matrix4 mtx, int objectID)
		{
			if(ProgramID == -1 || !WasInit) return;
			GL.UseProgram(ProgramID);
			
			if(objectID < TriangleBatches.Count)
			{
				RenderSubModelUVs(mtx, objectID);
			}
			
			GL.UseProgram(0);
		}
		
		public void RenderMaterial(Matrix4 mtx, int materialID)
		{
			if(ProgramID == -1 || !WasInit) return;
			GL.UseProgram(ProgramID);
			
			//for(int i = 0; i < TriStripBatches.Count; i++)
			for(int i = 0; i < TriangleBatches.Count; i++)
			{
				//var tmpTBatch = TriStripBatches[i];
				var tmpTBatch = TriangleBatches[i];
				if(tmpTBatch.MaterialID == materialID) RenderSubModel(mtx, i);
			}
			
			GL.UseProgram(0);
		}
		
		public void RenderMaterialUVs(Matrix4 mtx, int materialID)
		{
			if(ProgramID == -1 || !WasInit) return;
			GL.UseProgram(ProgramID);
			
			for(int i = 0; i < TriangleBatches.Count; i++)
			{
				var tmpTBatch = TriangleBatches[i];
				if(tmpTBatch.MaterialID == materialID) RenderSubModelUVs(mtx, i);
			}
			
			GL.UseProgram(0);
		}
		
		public void Init()
		{
			if(!InitShaders()) return;
			
			foreach(var tmpVBatch in VertexBatches)
			{
				tmpVBatch.Init();
			}
			
			//foreach(var tmpTBatch in TriStripBatches)
			foreach(var tmpTBatch in TriangleBatches)
			{
				tmpTBatch.Init();
			}
			WasInit = true;
		}
		
		public void DeInit()
		{
			
			DeInitShaders();
			
			foreach(var tmpVBatch in VertexBatches)
			{
				tmpVBatch.DeInit();
			}
			
			//foreach(var tmpTBatch in TriStripBatches)
			foreach(var tmpTBatch in TriangleBatches)
			{
				tmpTBatch.DeInit();
			}
		}
		
		public static bool InitShaders()
			{
				if(ProgramID != -1 && ProgramWasInit)
				{
					ProgramRefCount += 1;
					return true;
				}
				ProgramID = Scene.LoadProgramFromStrings(vsSource, fsSource);
				if(ProgramID == -1)
				{	
					Logger.LogError("Error loading Rendergraph Shader.\n");
					return false;
				}
				
				AttribPosition = GL.GetAttribLocation(ProgramID, "Position");
				AttribTexCoords = GL.GetAttribLocation(ProgramID, "TexCoords");
				UniformMatrix = GL.GetUniformLocation(ProgramID, "Matrix");
				UniformTexture = GL.GetUniformLocation(ProgramID, "Texture");
				UniformColor = GL.GetUniformLocation(ProgramID, "ColorOverride");
				if(AttribPosition == -1 || AttribTexCoords == -1 || UniformMatrix == -1 || UniformTexture == -1 || UniformColor == -1)
				{
					Logger.LogError("Error Getting RenderGraph Program Attrib/Uniform Locations\n");
					Logger.LogError(string.Format("\tPosition: {0}, TexCoords: {1}, Matrix: {2}, Color: {3}, Texture: {4}\n", AttribPosition, AttribTexCoords, UniformMatrix, UniformColor, UniformTexture));
					return false;
				}
				
				ProgramWasInit = true;
				ProgramRefCount += 1;
				return true;
			}
			
			public static void DeInitShaders()
			{
				ProgramRefCount -= 1;
				if(ProgramRefCount <= 0)
				{
					if(ProgramID != -1) GL.DeleteProgram(ProgramID);
					ProgramID = -1;
					ProgramWasInit = false;
				}
			}
			
			public TreeNode ToNode(int rgID)
			{
				string descStr = GetDescription(rgID);
				var retVal = new TreeNode(descStr);
				int i = 0;
				var TriStripNode = new TreeNode("Triangle Strips");
				var VertexBatchNode = new TreeNode("Vertex Batches");
				var MaterialNode = new TreeNode("Materials");
				retVal.Nodes.Add(TriStripNode);
				retVal.Nodes.Add(VertexBatchNode);
				retVal.Nodes.Add(MaterialNode);
				
				//foreach(var tmpTBatch in TriStripBatches)
				foreach(var tmpTBatch in TriangleBatches)
				{
					var tmpVBatch = VertexBatches[tmpTBatch.VertexBatchID];
					var tmpMaterial = MaterialList[tmpTBatch.MaterialID];
					var tmpTNode = new TreeNode(string.Format("Triangle Batch {0}, {1} Triangles, Material: {2}, VertexBatch: {3}", i, tmpTBatch.TriangleCount, tmpMaterial.MaterialName, tmpTBatch.VertexBatchID));
					tmpTNode.Tag = new TreeNodeTag(ParentFile, ObjectID, TreeNodeTag.OBJECT_TYPE_RENDERGRAPH_TRIANGLE_STRIP, i, TreeNodeTag.NodeType.SubNode);
					i++;
					TriStripNode.Nodes.Add(tmpTNode);
				}
				
				i = 0;
				foreach(var tmpVBatch in VertexBatches)
				{
					var tmpVNode = new TreeNode(string.Format("Vertex Batch {0} (Type: {1}) {2} Vertices, .data: 0x{3:X}, .gpu: 0x{4:X}", i, tmpVBatch.VType, tmpVBatch.VertexCount, tmpVBatch.VDataOffset, tmpVBatch.VGPUOffset));
					tmpVNode.Tag = new TreeNodeTag(ParentFile, ObjectID, TreeNodeTag.OBJECT_TYPE_RENDERGRAPH_VERTEX_BATCH, i, TreeNodeTag.NodeType.SubNode);
					i++;
					VertexBatchNode.Nodes.Add(tmpVNode);
				}
				
				i=0;
				foreach(var tmpMat in MaterialList)
				{
					var tmpMNode = new TreeNode(string.Format("Material {0}: '{1}'", i, tmpMat.MaterialName));
					tmpMNode.Tag = new TreeNodeTag(ParentFile, ObjectID, TreeNodeTag.OBJECT_TYPE_RENDERGRAPH_MATERIAL, i, TreeNodeTag.NodeType.SubNode);
					i++;
					MaterialNode.Nodes.Add(tmpMNode);
				}
				
				return retVal;
			}
			
			public string GetDescription(int rgID)
			{
				return string.Format("RenderGraph_{0}: {1} Vertex Batches, {2} Triangle Batches", rgID, VertexBatches.Count, TriangleBatches.Count);
			}
			
			public VertexBatch GetVertexBatch(int id)
			{
				if(id > (VertexBatches.Count - 1)) return null;
				return VertexBatches[id];
			}
			
			public TriangleBatch GetTriangleBatch(int id)
			{
				if(id > (TriangleBatches.Count - 1)) return null;
				return TriangleBatches[id];
			}
			
			public Material GetMaterial(int id)
			{
				if(id > (MaterialList.Count - 1)) return null;
				return MaterialList[id];
			}
			
			public void SetDefaultUVScale(float val)
			{
				foreach(var tmpVBatch in VertexBatches)
				{
					tmpVBatch.VD_TexCoordScale = val;
				}
			}
			
			public int DumpToObj(StreamWriter fs, int vOffset, int renderGraphID, bool splitSubMeshes)
			{
				int totalVertexCount = vOffset;
				
				if(!splitSubMeshes)
				{
					fs.WriteLine(string.Format("g rg_{0}", renderGraphID));
				}
				
				int VBID = 0;
				foreach(var tmpVB in VertexBatches)
				{
					foreach(var tmpV in tmpVB.Vertices)
					{
						fs.WriteLine(string.Format("v\t{0}\t{1}\t{2}", tmpV.Position.X, tmpV.Position.Y, tmpV.Position.Z));
						fs.WriteLine(string.Format("vt\t{0}\t{1}", tmpV.TexCoords.X, 1.0 - tmpV.TexCoords.Y));
					}
				
					int submeshID = 0;
					//foreach(var tmpTS in TriStripBatches)
					foreach(var tmpTB in TriangleBatches)
					{
						if(tmpTB.VertexBatchID == VBID)
						{
							if(splitSubMeshes)
							{
								fs.WriteLine(string.Format("g rg_{0}_{1}", renderGraphID, submeshID));
							}
							var tmpMat = GetMaterial(tmpTB.MaterialID);
							
							fs.WriteLine(string.Format("usemtl texturefile_{0}", tmpMat.textureID));
							
							/*
							int tCount = 0;
							for(int i = 2; i < tmpTS.TriStripCount; i++)
							{
								//tmpTS.TSIndices[i];
								int ts1 = tmpTS.TSIndices[i-2] + totalVertexCount;
								int ts2 = tmpTS.TSIndices[i-1] + totalVertexCount;
								int ts3 = tmpTS.TSIndices[i] + totalVertexCount;
								if((tCount % 2) == 1)
								{
									ts1 = tmpTS.TSIndices[i] + totalVertexCount;
									ts2 = tmpTS.TSIndices[i - 1] + totalVertexCount;
									ts3 = tmpTS.TSIndices[i - 2] + totalVertexCount;
								}
								fs.WriteLine(string.Format("f\t{0}/{0}\t{1}/{1}\t{2}/{2}", ts1, ts2, ts3));
								tCount++;
							}
							*/
							
							for(int i = 0; i < tmpTB.TriangleCount; i++)
							{
								var tmpT = tmpTB.Triangles[i];
								fs.WriteLine(string.Format("f\t{0}/{0}\t{1}/{1}\t{2}/{2}", tmpT.id1 + totalVertexCount, tmpT.id2 + totalVertexCount, tmpT.id3 + totalVertexCount));
							}
							
							submeshID++;
						}
					}
					VBID++;
					totalVertexCount += tmpVB.VertexCount;
				}
				return totalVertexCount;
			}
	}
}
