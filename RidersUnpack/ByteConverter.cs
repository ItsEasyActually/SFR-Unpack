﻿using System;
using System.Diagnostics;

//Code is originally taken from the SAModel Class in the SA Tools.
//Thanks to all the members of x-hax for this.

namespace SAModel
{
	[DebuggerNonUserCode]
	public static class ByteConverter
	{
		public static bool BigEndian { get; set; }
		public static bool Reverse { get; set; }

		public static byte[] GetBytes(ushort value)
		{
			byte[] y = BitConverter.GetBytes(value);
			SwapEndian(y);
			return y;
		}

		public static byte[] GetBytes(short value)
		{
			byte[] y = BitConverter.GetBytes(value);
			SwapEndian(y);
			return y;
		}

		public static byte[] GetBytes(uint value)
		{
			byte[] y = BitConverter.GetBytes(value);
			SwapEndian(y);
			return y;
		}

		public static byte[] GetBytes(int value)
		{
			byte[] y = BitConverter.GetBytes(value);
			SwapEndian(y);
			return y;
		}

		public static byte[] GetBytes(ulong value)
		{
			byte[] y = BitConverter.GetBytes(value);
			SwapEndian(y);
			return y;
		}

		public static byte[] GetBytes(long value)
		{
			byte[] y = BitConverter.GetBytes(value);
			SwapEndian(y);
			return y;
		}

		public static byte[] GetBytes(float value)
		{
			byte[] y = BitConverter.GetBytes(value);
			SwapEndian(y);
			return y;
		}

		public static byte[] GetBytes(double value)
		{
			byte[] y = BitConverter.GetBytes(value);
			SwapEndian(y);
			return y;
		}

		public static ushort ToUInt16(byte[] value, int startIndex)
		{
			byte[] y = new byte[2];
			Array.Copy(value, startIndex, y, 0, 2);
			SwapEndian(y);
			return BitConverter.ToUInt16(y, 0);
		}

		public static short ToInt16(byte[] value, int startIndex)
		{
			byte[] y = new byte[2];
			Array.Copy(value, startIndex, y, 0, 2);
			SwapEndian(y);
			return BitConverter.ToInt16(y, 0);
		}

		public static uint ToUInt32(byte[] value, int startIndex)
		{
			byte[] y = new byte[4];
			Array.Copy(value, startIndex, y, 0, 4);
			SwapEndian(y);
			return BitConverter.ToUInt32(y, 0);
		}

		public static int ToInt32(byte[] value, int startIndex)
		{
			byte[] y = new byte[4];
			Array.Copy(value, startIndex, y, 0, 4);
			SwapEndian(y);
			return BitConverter.ToInt32(y, 0);
		}

		public static ulong ToUInt64(byte[] value, int startIndex)
		{
			byte[] y = new byte[8];
			Array.Copy(value, startIndex, y, 0, 8);
			SwapEndian(y);
			return BitConverter.ToUInt64(y, 0);
		}

		public static long ToInt64(byte[] value, int startIndex)
		{
			byte[] y = new byte[8];
			Array.Copy(value, startIndex, y, 0, 8);
			SwapEndian(y);
			return BitConverter.ToInt64(y, 0);
		}

		public static float ToSingle(byte[] value, int startIndex)
		{
			byte[] y = new byte[4];
			Array.Copy(value, startIndex, y, 0, 4);
			SwapEndian(y);
			float res = BitConverter.ToSingle(y, 0);
			if (res == -0.0f) res = 0.0f;
			return res;
		}

		public static double ToDouble(byte[] value, int startIndex)
		{
			byte[] y = new byte[8];
			Array.Copy(value, startIndex, y, 0, 8);
			SwapEndian(y);
			return BitConverter.ToDouble(y, 0);
		}

		private static void SwapEndian(byte[] value)
		{
			if (!BigEndian & !BitConverter.IsLittleEndian)
				Array.Reverse(value);
			if (BigEndian & BitConverter.IsLittleEndian)
				Array.Reverse(value);
		}
	}
}