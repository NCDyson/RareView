/*
 * Created by SharpDevelop.
 * User: NCDyson
 * Date: 1/5/2018
 * Time: 5:00 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;

namespace RareView
{
	/// <summary>
	/// Description of IO.
	/// </summary>
	public static class IO
	{
		public static ulong ReadBigU64(BinaryReader bStream)
		{
			var buffer = bStream.ReadBytes(8);
			Array.Reverse(buffer);
			return BitConverter.ToUInt64(buffer, 0);
		}
		
		public static long ReadBig64(BinaryReader bStream)
		{
			var buffer = bStream.ReadBytes(8);
			Array.Reverse(buffer);
			return BitConverter.ToInt64(buffer, 0);
		}
		
		public static uint ReadBigU32(BinaryReader bStream)
		{
			var buffer = bStream.ReadBytes(4);
			Array.Reverse(buffer);
			return BitConverter.ToUInt32(buffer, 0);
		}
		
		public static int ReadBig32(BinaryReader bStream)
		{
			var buffer = bStream.ReadBytes(4);
			Array.Reverse(buffer);
			return BitConverter.ToInt32(buffer, 0);
		}
		
		public static ushort ReadBigU16(BinaryReader bStream)
		{
			var buffer = bStream.ReadBytes(2);
			Array.Reverse(buffer);
			return BitConverter.ToUInt16(buffer, 0);
		}
		
		public static short ReadBig16(BinaryReader bStream)
		{
			var buffer = bStream.ReadBytes(2);
			Array.Reverse(buffer);
			return BitConverter.ToInt16(buffer, 0);
		}
		
		public static byte ReadByte(BinaryReader bStream)
		{
			return bStream.ReadByte();
		}
		
		public static sbyte ReadSByte(BinaryReader bStream)
		{
			return bStream.ReadSByte();
		}
		
		public static string ReadString(BinaryReader bStream)
		{
			string newString = "";
			while(true)
			{
				char temp = (char)bStream.ReadByte();
				if(temp != '\0') newString += temp;
				else break;
			}
			return newString;
		}
		
		public static string ReadStringFrom(BinaryReader bStream, int offset)
		{
			long oldPos = bStream.BaseStream.Position;
			bStream.BaseStream.Seek(offset, SeekOrigin.Begin);
			string tmpStr = ReadString(bStream);
			bStream.BaseStream.Seek(oldPos, SeekOrigin.Begin);
			return tmpStr;
		}
		
		public static string ReadStringF(BinaryReader bStream, int size)
		{
			string newString = "";
			int readLength = 0;
			for(int i = 0; i < size; i++)
			{
				char tempChar = (char)bStream.ReadByte();
				readLength++;
				if(tempChar != '\0')
				{
					newString += tempChar;
				}
				else
				{
					break;
				}
			}
			
			int padd = (size - readLength);
			bStream.BaseStream.Seek(padd, SeekOrigin.Current);
			return newString;
		}
		
		public static float ReadBigF32(BinaryReader bStream)
		{
			var buffer = bStream.ReadBytes(4);
			Array.Reverse(buffer);
			return BitConverter.ToSingle(buffer, 0);
		}
		
		public static double ReadBigF64(BinaryReader bStream)
		{
			var buffer = bStream.ReadBytes(8);
			Array.Reverse(buffer);
			return BitConverter.ToDouble(buffer, 0);
		}
		
		public static void Align(BinaryReader bStream, int size)
		{
			int padTo = (int)(bStream.BaseStream.Position % size);
			if(padTo > 0)
			{
				bStream.BaseStream.Seek(size - padTo, SeekOrigin.Current);
			}
		}
	}
}
