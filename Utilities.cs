using HalojenBackups.Config;
using HalojenBackups.Destination;
using HalojenBackups.MessageOutput;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HalojenBackups {
	internal class Utilities {
		internal static FileInfo GetDestFile(FileInfo sourceFile, DestinationDrive destination) {
			string driveLetter = sourceFile.Directory.Root.ToString().Substring(0, 1);
			string restOfPath = sourceFile.FullName.Substring(sourceFile.Directory.Root.Name.Length);
			string destPath = Path.Combine(destination.RootPath, driveLetter, restOfPath);
			Message.Write(destPath);
			return new FileInfo(destPath);
		}
		internal static DirectoryInfo GetDestDir(DirectoryInfo sourcePath, DestinationDrive destination) {
			string driveLetter = sourcePath.Root.ToString().Substring(0, 1);
			string restOfPath = sourcePath.FullName.Substring(sourcePath.Root.Name.Length);
			string destPath = Path.Combine(destination.RootPath, driveLetter, restOfPath);
			return new DirectoryInfo(destPath);
		}
		internal static bool FilesAreEqual(FileInfo fileInfo1, FileInfo fileInfo2) {
			if (fileInfo1.LastWriteTimeUtc > fileInfo2.LastWriteTimeUtc) {
				return false;
			}
			// Compare file sizes before continuing. 
			if (CompareFileSizes(fileInfo1, fileInfo2)) {
				// If sizes are equal then compare bytes.
				return CompareFileHashes(fileInfo1, fileInfo2);
			}
			return false;
		}
		internal static bool CompareFileHashes(FileInfo fileInfo1, FileInfo fileInfo2) {
				MD5 hash = MD5.Create();

				// Declare byte arrays to store our file hashes
				byte[] fileHash1;
				byte[] fileHash2;

				// Open a System.IO.FileStream for each file.
				// Note: With the 'using' keyword the streams 
				// are closed automatically.
				using (FileStream
					fileStream1 = new FileStream(fileInfo1.FullName, FileMode.Open, FileAccess.Read),
					fileStream2 = new FileStream(fileInfo2.FullName, FileMode.Open, FileAccess.Read)
				) {
					// Compute file hashes
					fileHash1 = hash.ComputeHash(fileStream1);
					fileHash2 = hash.ComputeHash(fileStream2);
				}

				return BitConverter.ToString(fileHash1) == BitConverter.ToString(fileHash2);
		}
		private static bool CompareFileSizes(FileInfo fileInfo1, FileInfo fileInfo2) {
			bool fileSizeEqual = true;

			// Compare file sizes
			if (fileInfo1.Length != fileInfo2.Length) {
				// File sizes are not equal therefore files are not identical
				fileSizeEqual = false;
			}

			return fileSizeEqual;
		}
	}
}
