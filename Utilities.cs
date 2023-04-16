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
		public static readonly List<string> reliableFileTypes = new List<string>() { "jpg", "jpeg", "mp4", "avi", "mp3", "exe" };
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
		internal static bool FilesAreEqual(FileInfo sourceFileInfo, FileInfo destFileInfo) {
			if (sourceFileInfo.LastWriteTimeUtc > destFileInfo.LastWriteTimeUtc) {
				return false;
			}
			// Compare file sizes before continuing. 
			if (FileSizesAreEqual(sourceFileInfo, destFileInfo)) {
				// If sizes are equal then compare bytes.
				if (reliableFileTypes.Contains(sourceFileInfo.Extension.ToLower().Replace(".", String.Empty))) {
					// Ignore file types which are unlikely to have the same length if the contents have changed.
					return true;
				}
				return HashesAreEqual(sourceFileInfo, destFileInfo);
			}
			return false;
		}
		internal static bool HashesAreEqual(FileInfo sourceFileInfo, FileInfo destFileInfo) {
			var task = Task.Run(async () => await FileHash.FileHash.GetHash(sourceFileInfo));
			var task2 = Task.Run(async () => await FileHash.FileHash.GetHash(destFileInfo));

			return BitConverter.ToString(task.Result) == BitConverter.ToString(task2.Result);
		}

		private static bool FileSizesAreEqual(FileInfo sourceFileInfo, FileInfo destFileInfo) {
			return sourceFileInfo.Length == destFileInfo.Length;
		}
	}
}
