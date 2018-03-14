﻿/*
 * Created by SharpDevelop.
 * User: NCDyson
 * Date: 1/6/2018
 * Time: 5:17 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace RareView
{
	/// <summary>
	/// Description of FloorGird.
	/// </summary>
	public static class FloorGrid
	{
		public static string vsSource = @"#version 330 core
layout(location=0) in vec3 Position;

uniform mat4 Matrix;
uniform vec4 Color;
out vec4 FragmentBaseColor;

void main(){
	gl_Position = Matrix * vec4(Position, 1.0);
	FragmentBaseColor = Color;
}
";

		public static string fsSource = @"#version 330 core
in vec4 FragmentBaseColor;
out vec4 color;

void main(){
	color = FragmentBaseColor;
}
";
		public static bool WasInit = false;
		public static Vector3[] Vertices;
		public static int VertexCount = 0;
		public static int ProgramID = -1;
		public static int AttribPosition = -1;
		public static int UniformMatrix = -1;
		public static int UniformColor = -1;
		public static int ArrayID = -1;
		public static int BufferID = -1;
		
		public static int GridSize = 0;
		public static float GridSpacing = 1.0f;
		
		public static bool Init()
		{
			if(WasInit) return true;
			
			InitGridVertices(16);
			
			ProgramID = Scene.LoadProgramFromStrings(vsSource, fsSource);
			if(ProgramID == -1)
			{
				return false;
			}
			
			AttribPosition = GL.GetAttribLocation(ProgramID, "Position");
			UniformMatrix = GL.GetUniformLocation(ProgramID, "Matrix");
			UniformColor = GL.GetUniformLocation(ProgramID, "Color");
			
			if(AttribPosition == -1 || UniformMatrix == -1 || UniformColor == -1)
			{
				Logger.LogError("Error getting Grid Shader Attributes/Uniform Locations:");
				Logger.LogError(string.Format("\tPosition: {0}, Matrix: {1}, Color: {2}", AttribPosition, UniformMatrix, UniformColor));
				return false;
			}
			
			ArrayID = GL.GenVertexArray();
			GL.BindVertexArray(ArrayID);
			
			BufferID = GL.GenBuffer();
			UpdateGridVerts(false);
			
			Type vertexType = Vertices[0].GetType();
			int vertexSize = Marshal.SizeOf(vertexType);
			
			GL.EnableVertexAttribArray(AttribPosition);
			GL.VertexAttribPointer(AttribPosition, 3, VertexAttribPointerType.Float, false, vertexSize, IntPtr.Zero);
			
			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			
			WasInit = true;
			return true;
		}
		
		public static void InitGridVertices(int _gridSize)
		{
			int gridLines = (_gridSize * 2) + 1;
			GridSize = _gridSize;
			VertexCount = gridLines * 4;
			Vertices = new Vector3[VertexCount];
			
			float startX = -GridSize;
			float startY = -GridSize;
			
			int vCount = 0;
			//Do Horizontal Lines
			for(int y = 0; y < gridLines; y++)
			{
				float vertY = startY + y;
				Vertices[vCount] = new Vector3(startX, 0.0f, vertY);
				Vertices[vCount + 1] = new Vector3(-startX, 0.0f, vertY);
				vCount += 2;
			}
			
			//Do Vertical Lines
			for(int x = 0; x < gridLines; x++)
			{
				float vertX = startX + x;
				Vertices[vCount] = new Vector3(vertX, 0.0f, startY);
				Vertices[vCount + 1] = new Vector3(vertX, 0.0f, -startY);
				vCount += 2;
			}
		}
		
		private static void UpdateGridVerts(bool unbindAfter = true)
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID);
			Type vertexType = Vertices[0].GetType();
			int vertexSize = Marshal.SizeOf(vertexType);
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexSize * VertexCount), Vertices, BufferUsageHint.StaticDraw);
			
			if(unbindAfter) GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}
		
		public static void DeInit()
		{
			if(ProgramID != -1) GL.DeleteProgram(ProgramID);
			ProgramID = -1;
			if(ArrayID != -1) GL.DeleteVertexArray(ArrayID);
			ArrayID = -1;
			if(BufferID != -1) GL.DeleteBuffer(BufferID);
			BufferID = -1;
			
			WasInit = false;
		}
		
		public static void Render(Matrix4 matrix)
		{
			if(!WasInit || ProgramID == -1) return;
			
			GL.UseProgram(ProgramID);
			
			GL.BindVertexArray(ArrayID);
			
			GL.Uniform4(UniformColor, 0.5f, 0.5f, 0.5f, 1.0f);
			GL.UniformMatrix4(UniformMatrix, false, ref matrix);
			
			GL.DrawArrays(PrimitiveType.Lines, 0, VertexCount);
			
			GL.BindVertexArray(0);
			GL.UseProgram(0);
		}
	}
}
