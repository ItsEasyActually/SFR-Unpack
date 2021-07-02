using System;
using System.Collections.Generic;
using System.IO;
using SAModel;

namespace RidersUnpack
{
	class Program
	{
		static void Main(string[] args)
		{
			ByteConverter.BigEndian = true;
			byte[] file = File.ReadAllBytes(args[0]);
			
			string folder = Path.GetDirectoryName(args[0]);
			List<Unpack.outFile> output = new List<Unpack.outFile>();
			List<string> loggerInfo = new List<string>();
			Unpack.ReadFile(file, output, loggerInfo);
			Unpack.WriteFiles(output, folder, Path.GetFileName(args[0]), loggerInfo);
		}
	}
}
