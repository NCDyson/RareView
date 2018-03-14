/*
 * Created by SharpDevelop.
 * User: NCDyson
 * Date: 1/14/2018
 * Time: 5:07 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace RareView
{
	/// <summary>
	/// Description of DefaultTexture.
	/// </summary>
	public static class DefaultTexture
	{
		public static byte[] TextureData = null;
		public static int Width = 0;
		public static int Height = 0;
		public static int GLID = -1;
		
		public static bool Init()
		{
			using(var fs = new FileStream("data/textures/UV_Grid_Sm.tga", FileMode.Open))
			{
				using(var bs = new BinaryReader(fs))
				{
					bs.ReadUInt16();
					if(bs.ReadByte() != 0x2)
					{
						Logger.LogError("Error loading UV_Grid_Sm.tga, unsupported TGA Type.\n");
						Logger.LogError("\tExpected 32 or 24bit uncompressed texture.");
						return false;
					}
					
					bs.BaseStream.Seek(0xc, SeekOrigin.Begin);
					Width = bs.ReadUInt16();
					Height = bs.ReadUInt16();
					bs.BaseStream.Seek(0x10, SeekOrigin.Begin);
					byte bitDepth = bs.ReadByte();
					if(bitDepth == 0x18)
					{
						TextureData = bs.ReadBytes(Width * Height * 3);
					}
					else if(bitDepth == 0x20)
					{
						TextureData = bs.ReadBytes(Width * Height * 4);
					}
					
					GLID = GL.GenTexture();
					GL.BindTexture(TextureTarget.Texture2D, GLID);
					
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.Repeat);
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.Repeat);
				
					if(bitDepth == 0x18) GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, Width, Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, TextureData);
					else GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, TextureData);
					
					GL.BindTexture(TextureTarget.Texture2D, 0);
				}
			}
			
			return true;
		}
		
		public static bool DeInit()
		{
			if(GLID != -1) GL.DeleteTexture(GLID);
			GLID = -1;
			
			return true;
		}
	}
}
