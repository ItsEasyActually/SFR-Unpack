using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAModel;

namespace RidersUnpack
{
	class Unpack
	{
		public class outFile
		{
			public byte[] fileByte;
			public string fileName;

			public outFile(byte[] data, string name)
			{
				fileByte = data;
				fileName = name;
			}
		}

		public static void ReadFile(byte[] file, List<outFile> output, List<string> logger)
		{
			List<int> fileAddr = new List<int>();
			int fileType = ByteConverter.ToInt32(file, 0);

			if (fileType == 1885426516)	//"paST"
			{
				int fileCount = ByteConverter.ToInt32(file, 4);

				Console.WriteLine("Total Files: {0}", fileCount);
				//Append addresses to an array for ease of use.
				for (int p = 0; p < fileCount; p++)
				{
					int addr = ByteConverter.ToInt32(file, 8 + 4 * p);
					//Check to see if address is null or already present in the array before adding.
					if (addr != 0 && !fileAddr.Contains(addr))
						fileAddr.Add(addr);
				}


				// Loop through array to dump files.
				for (int f = 0; f < fileAddr.Count; f++)
				{
					int typeCheck = ByteConverter.ToInt32(file, fileAddr[f]);

					// If check for the 'type' of file.
					if (typeCheck == 1313163590)
					{
						// Reads NEIF/Sega NN Library files.
						ReadAndSaveNNLibFile(file, fileAddr[f], output, f);
					}
					else
					{
						// Reads DDS Texture array.
						ReadAndSaveDDSFiles(file, fileAddr[f], output);
					}
				}
			}

			if (fileType == 1885430635)	//"pack"
			{
				if (file.Length > 32)
				{
					int firstTotal = ByteConverter.ToInt16(file, 4);
					int idxEnd;

					// Hacky method for bypassing the garbage in the headers for the basic pack files.
					if (ByteConverter.ToInt32(file, (firstTotal*2) + 8) == 0)
						idxEnd = 8 + (firstTotal * 2) + 6;
					else
						idxEnd = 8 + (firstTotal * 2) + 4;

					Console.WriteLine("Index End: {0}", idxEnd.ToString());
					int shortCheck = ByteConverter.ToInt16(file, idxEnd);
					int sc = 0;
					
					while (shortCheck != 0)
					{
						shortCheck = ByteConverter.ToInt16(file, idxEnd + (2 * sc));
						sc++;
					}

					int ptrStart;
					if (idxEnd == 16 && (ByteConverter.ToInt16(file, idxEnd) == 0))
						ptrStart = idxEnd;
					else
						ptrStart = idxEnd + (2 * sc) - 2;

					Console.WriteLine("Pointer array starts at {0}", ptrStart.ToString());
					// Scan pointer array and save valid pointers to array.
					int curPtr = 0;
					int i = 0;
					while (curPtr != file.Length)
					{
						curPtr = ByteConverter.ToInt32(file, ptrStart + (4 * i));
						if (curPtr != 0)
							fileAddr.Add(curPtr);
						i++;
					}

					// Process all file addresses stored in the file address list.
					for (int f = 0; f < fileAddr.Count - 1; f++)
					{
						int fileSize = fileAddr[f + 1] - fileAddr[f];
						byte[] newfile = new byte[fileSize];

						int typeCheck = ByteConverter.ToInt32(file, fileAddr[f]);

						// Switch check for type of file found.
						switch (typeCheck)
						{
							case 1313163590: // NEIF Types/NN Lib Files
								ReadAndSaveNNLibFile(file, fileAddr[f], output, f);
								break;
							case 1479751216: // X360 Files
								Console.WriteLine("X360 file located: {0}", fileAddr[f].ToString());
								GenericOutput(file, newfile, fileAddr[f], output, f, ".x360");
								break;
							case 1179602516: //Font Data, two types. 
								Console.WriteLine("Font Data located: {0}", fileAddr[f].ToString());
								if (ByteConverter.ToInt32(file, fileAddr[f] + 4) == 1145132102)
									GenericOutput(file, newfile, fileAddr[f], output, f, ".datf");
								else if (ByteConverter.ToInt32(file, fileAddr[f] + 4) == 1398033474)
									GenericOutput(file, newfile, fileAddr[f], output, f, ".stlb");
								break;
							case 1179075072: // FGB Files
								Console.WriteLine("FGB File located: {0}", fileAddr[f].ToString());
								GenericOutput(file, newfile, fileAddr[f], output, f, ".fgb");
								break;
							default: // Defaults to DDS or Logger. 
								if (ByteConverter.ToInt16(file, fileAddr[f]) != 0 && ByteConverter.ToInt16(file, fileAddr[f] + 2) == 1 || ByteConverter.ToInt16(file, fileAddr[f]) != 0 && ByteConverter.ToInt16(file, fileAddr[f] + 2) == 0)
									ReadAndSaveDDSFiles(file, fileAddr[f], output);
								else
								{
									// Output for logger in DEBUG builds only.
									logger.Add("0x" + fileAddr[f].ToString("X") + ": No extractable data found via main extraction method");
								}
								break;
						}
					}
				}
			}

			// Some files have NN Lib related files in a secondary array.
			// Checking for that would be difficult without more knowledge.
			// This bypasses that secondary array and simply scans for matching byte patterns.
			// It checks for duplicates, so files will only be output once.
			Console.WriteLine("Checking for missed NN Lib files.");
			byte[] neifPattern = new byte[] { 0x4E, 0x45, 0x49, 0x46 };
			List<int> neifAddr = SearchBytePattern(neifPattern, file);

			for (int n = 0; n < neifAddr.Count; n++)
			{
				if (!fileAddr.Contains(neifAddr[n]))
					ReadAndSaveNNLibFile(file, neifAddr[n], output, n);
			}
		}

		public static void GenericOutput(byte[] src, byte[] dst, int addr, List<outFile> output, int idx, string ext)
		{
			// Copy supplied data to destination bytes.
			Array.Copy(src, addr, dst, 0, dst.Length);

			output.Add(new outFile(dst, idx.ToString("D3") + ext));
		}

		public static void ReadAndSaveNNLibFile(byte[] file, int addr, List<outFile> output, int idx)
		{
			// Print that NEIF Chunk was found. (NN xEnon Info)
			Console.WriteLine("NEIF Chunk Located: {0}", addr.ToString("X"));

			// Print size of file pre-NON0 Chunk
			int objSize = ByteConverter.ToInt32(file, (addr + 20));
			Console.WriteLine("objSize = {0}", objSize.ToString());

			// Set NOF0 Chunk address. (NN Offsets)
			int nof0Loc = addr + objSize;
			Console.WriteLine("NOF0 at {0}", nof0Loc.ToString("X"));
			
			// NFN0 Chunk address. (NN Filename)
			int nfn0Loc = BitConverter.ToInt32(file, nof0Loc + 4) + nof0Loc + 8;
			Console.WriteLine("NFN0 Chunk at {0}", nfn0Loc.ToString("X"));

			// Read NFN0 Chunk for filename.
			int namesize = 0;
			int NFN0Size = BitConverter.ToInt32(file, nfn0Loc + 4) - 8;
			for (int s = 0; s < NFN0Size; s++)
			{
				if (file[nfn0Loc + 16 + s] != 0)
					namesize++;
				else
					break;
			}
			byte[] namechunk = new byte[namesize];
			Array.Copy(file, nfn0Loc + 16, namechunk, 0, namesize);
			string fileName = System.Text.Encoding.ASCII.GetString(namechunk);

			// Print filename to console.
			Console.WriteLine("Filename found: {0}", fileName);

			// Set full filesize from NEIF Chunk to end of NEND Chunk.
			int fileSize = (nfn0Loc + NFN0Size + 16) - addr;
			Console.WriteLine("Filesize: {0}", fileSize.ToString("X"));

			// Copy bytes to a byte array.
			byte[] newfile = new byte[fileSize];
			Array.Copy(file, addr, newfile, 0, newfile.Length);

			// Add new file byte array to output for writing.
			output.Add(new outFile(newfile, (idx.ToString("D3") + "_" + fileName)));
		}

		public static void ReadAndSaveDDSFiles(byte[] file, int addr, List<outFile> output)
		{
			// Define Lists to use.
			List<int> texOff = new List<int>();
			List<int> texSize = new List<int>();

			// Print where reading DDS Data chunk begins.
			Console.WriteLine("Reading textures at {0}", addr);
			int texTotal = ByteConverter.ToInt16(file, addr);	// Texture Total
			int texPtrArr = addr + 4;							// Texture Pointer Array Start Address
			int texSizeArr = addr + 4 + (4 * texTotal);			// Texture Size Array Start Address
			int texNameStart = addr + 4 + (8 * texTotal);		// Texture Name Array Start Address
			int texNameEnd = ByteConverter.ToInt32(file, texPtrArr) + addr;	// Sets where Texture Name Array ends.
			int texNameSize = texNameEnd - texNameStart;		// Gets byte size of the Texture Names Array.

			// Get Pointers and Filesizes for each DDS File.
			for (int tp = 0; tp < texTotal; tp++)
			{
				int ptr_ddsFile = ByteConverter.ToInt32(file, texPtrArr + 4 * tp) + addr;
				Console.WriteLine("Pointer to Texture: {0}", ptr_ddsFile);
				texOff.Add(ptr_ddsFile);
				int siz_ddFile = ByteConverter.ToInt32(file, texSizeArr + 4 * tp);
				Console.WriteLine("Texture Filesize: {0}", siz_ddFile);
				texSize.Add(siz_ddFile);
			}

			// Process names.
			// Copies Names Array as bytes into a new byte array.
			byte[] texNameChunk = new byte[texNameSize];
			Array.Copy(file, texNameStart, texNameChunk, 0, texNameChunk.Length);	

			// Trims away any excess from the filenames in the byte array.
			string texName = System.Text.Encoding.ASCII.GetString(texNameChunk).Trim(new char[] { (char)0, (char)0x11, (char)0x5F });

			// Splits the byte texture names array into proper strings.
			string[] splitTexNames = texName.Split(new char[] { (char)0 });

			// Save DDS file as byte array to output for writing.
			for (int t = 0; t < texOff.Count; t++)
			{
				byte[] ddsFile = new byte[texSize[t]];
				Array.Copy(file, texOff[t], ddsFile, 0, ddsFile.Length);
				output.Add(new outFile(ddsFile, (GetSafeFilename(splitTexNames[t]) + ".dds")));
			}
		}

		public static void WriteFiles(List<outFile> outFiles, string folder, string filename, List<string> logger)
		{
			// Set path for file output.
			string path = Path.Combine(folder, ("out_" + filename));

			// If there is output, process output.
			if (outFiles.Count != 0)
			{
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);

				// For files in output, get the filename and write bytes to new file.
				for (int o = 0; o < outFiles.Count; o++)
				{
					string outFilename = Path.Combine(path, outFiles[o].fileName);
					Console.WriteLine("Writing file {0}", outFiles[o].fileName);
					File.WriteAllBytes(outFilename, outFiles[o].fileByte);
				}
			}
#if DEBUG
			//Logger data. DEBUG only.
			if (logger.Count != 0)
			{
				Console.WriteLine("{0}: Possible non-extracted files located. Log created.", filename);
				TextWriter log = new StreamWriter(Path.Combine(folder, filename + ".log"));
				foreach (string line in logger)
					log.WriteLine(line);

				log.Close();
			}
#endif
		}

		// Not all filenames in these files are OS safe.
		// This returns a safe filename for the OS.
		public static string GetSafeFilename(string filename) { return string.Join("_", filename.Split(Path.GetInvalidFileNameChars())); }

		// Searches for a specific byte pattern.
		static public List<int> SearchBytePattern(byte[] pattern, byte[] bytes)
		{
			List<int> positions = new List<int>();
			int patternLength = pattern.Length;
			int totalLength = bytes.Length;
			byte firstMatchByte = pattern[0];
			for (int i = 0; i < totalLength; i++)
			{
				if (firstMatchByte == bytes[i] && totalLength - i >= patternLength)
				{
					byte[] match = new byte[patternLength];
					Array.Copy(bytes, i, match, 0, patternLength);
					if (match.SequenceEqual<byte>(pattern))
					{
						positions.Add(i);
						i += patternLength - 1;
					}
				}
			}
			return positions;
		}
	}
}
