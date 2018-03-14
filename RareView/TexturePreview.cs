 /*
 * Created by SharpDevelop.
 * User: NCDyson
 * Date: 1/7/2018
 * Time: 3:25 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace RareView
{
	/// <summary>
	/// Description of TexturePreview.
	/// </summary>
	public static class TexturePreview
	{
		private const string vsSource = @"#version 330 core
layout(location=0) in vec3 Position;
layout(location=1) in vec2 TexCoord;

uniform mat4 Matrix;
uniform vec3 Size;

out vec2 fTexCoords;

void main()
{
	float vX = Position.x * Size.x;
	float vY = Position.y * Size.y;
	gl_Position = Matrix * vec4(vX * Size.z, vY * Size.z, 0.0, 1.0);
	fTexCoords = TexCoord;
}
";
		public const string fsSource = @"#version 330 core
uniform sampler2D Texture;
in vec2 fTexCoords;
out vec4 color;

void main()
{
	color = texture2D(Texture, fTexCoords);
}
";
		
		public static bool WasInit = false;
		[StructLayout(LayoutKind.Sequential)]
		public struct TextureVertex
		{
			public Vector3 Position;
			public Vector2 TexCoord;
		}
		
		public static int VertexCount = 4;
		public static int ProgramID = -1;
		public static int AttribPosition = -1;
		public static int AttribTexCoord = -1;
		public static int UniformMatrix = -1;
		public static int UniformTextureSize = -1;
		public static int UniformTexture = -1;
		
		public static int ArrayID = -1;
		public static int BufferID = -1;
		
		public static TextureVertex[] Vertices = null;
		
		public static bool Init()
		{
			if(WasInit) return true;
			
			Vertices = new TextureVertex[4];
			//Bottom Left
			Vertices[0] = new TextureVertex()
			{
				Position = new Vector3(-0.5f, -0.5f, 0.0f),
				TexCoord = new Vector2(0.0f, 0.0f)
			};
			//Top Left
			Vertices[1] = new TextureVertex()
			{
				Position = new Vector3(-0.5f, 0.5f, 0.0f),
				TexCoord = new Vector2(0.0f, 1.0f)
			};
			//Bottom Right
			Vertices[2] = new TextureVertex()
			{
				Position = new Vector3(0.5f, -0.5f, 0.0f),
				TexCoord = new Vector2(1.0f, 0.0f)
			};	
			//Top Right
			Vertices[3] = new TextureVertex()
			{
				Position = new Vector3(0.5f, 0.5f, 0.0f),
				TexCoord = new Vector2(1.0f, 1.0f)
			};
			
			ProgramID = Scene.LoadProgramFromStrings(vsSource, fsSource);
			if(ProgramID == -1)
			{
				Logger.LogError("Error loading Texture Preview Shader Program.\n");
				return false;
			}
			
			AttribPosition = GL.GetAttribLocation(ProgramID, "Position");
			AttribTexCoord = GL.GetAttribLocation(ProgramID, "TexCoord");
			UniformMatrix = GL.GetUniformLocation(ProgramID, "Matrix");
			UniformTextureSize = GL.GetUniformLocation(ProgramID, "Size");
			UniformTexture = GL.GetUniformLocation(ProgramID, "Texture");
			
			if(AttribPosition == -1 || AttribTexCoord == -1 || UniformMatrix == -1 || UniformTextureSize == -1 || UniformTexture == -1)
			{
				Logger.LogError("Error geting Texture Preview Shader Attribute/Uniform Locations\n");
				Logger.LogError(string.Format("\tPosition: {0}, TexCoord: {1}, Matrix: {2}, Size: {3}, Texture: {4}\n", AttribPosition, AttribTexCoord, UniformMatrix, UniformTextureSize, UniformTexture));
				return false;
			}
			
			ArrayID = GL.GenVertexArray();
			GL.BindVertexArray(ArrayID);
			
			BufferID = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID);
			
			Type vType = Vertices[0].GetType();
			int vSize = Marshal.SizeOf(vType);
			
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vSize * VertexCount), Vertices, BufferUsageHint.StaticDraw);
			
			GL.EnableVertexAttribArray(AttribPosition);
			GL.VertexAttribPointer(AttribPosition, 3, VertexAttribPointerType.Float, false, vSize, Marshal.OffsetOf(vType, "Position"));
			
			GL.EnableVertexAttribArray(AttribTexCoord);
			GL.VertexAttribPointer(AttribTexCoord, 2, VertexAttribPointerType.Float, false, vSize, Marshal.OffsetOf(vType, "TexCoord"));
			
			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			
			WasInit = true;
			return true;
		}
		
		public static void DeInit()
		{
			if(ProgramID != -1) GL.DeleteProgram(ProgramID);
			ProgramID = -1;
			
			if(ArrayID != -1) GL.DeleteVertexArray(ArrayID);
			ArrayID = -1;
			
			if(BufferID != -1) GL.DeleteBuffer(BufferID);
			BufferID = -1;
		}
		
		public static void Render(Matrix4 matrix, int textureID, float textureWidth, float textureHeight)
		{
			if(!WasInit || ProgramID == -1) return;
			
			GL.UseProgram(ProgramID);
			
			GL.BindVertexArray(ArrayID);
			
			GL.Disable(EnableCap.CullFace);
			
			GL.Uniform1(UniformTexture, 0);
			GL.Uniform3(UniformTextureSize, textureWidth, textureHeight, 1.0f);
			GL.UniformMatrix4(UniformMatrix, false, ref matrix);
			GL.BindTexture(TextureTarget.Texture2D, textureID);
			
			GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
			
			GL.BindVertexArray(0);
			GL.UseProgram(0);
			
			if(Scene.BackfaceCulling) GL.Enable(EnableCap.CullFace);
		}
		
		
	}
}
