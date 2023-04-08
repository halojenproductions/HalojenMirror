using HalojenBackups.Config;
using HalojenBackups.Destination;
using HalojenBackups.MessageOutput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalojenBackups.Operation {
	internal class Operation {
		private Source _source;
		private Destination.DestinationDrive _destination;
		private DirectoryInfo SourceDir { get; set; }
		private DirectoryInfo DestinationDir { get; set; }
		private Options Options { get; set; }
		public Operation(Source source, Destination.DestinationDrive destination, Options options) {
			_source = source;
			SourceDir = new DirectoryInfo(source.SourcePath);
			DestinationDir = new DirectoryInfo(Path.Combine(destination.RootPath, source.DestPath));
			Options = options;
			Message.Write($"Gonna do a thang with {DestinationDir}");
		}
		public void Go() {
			if (CopyDirectory(SourceDir, DestinationDir)) {
				Message.Write(new MessagePart($"Copy was successful, or so they say.") { FColour = ConsoleColor.Green });
				Delete();
			}

		}

		private bool CopyDirectory(DirectoryInfo sourceDir, DirectoryInfo destinationDir) {
			// Check if the source directory exists
			if (!sourceDir.Exists) {
				throw new DirectoryNotFoundException($"Source directory not found: {sourceDir.FullName}");
			}

			// Create the destination directory if it doesn't exist in the destination.
			if (!destinationDir.Exists) {
				destinationDir.Create();
			}

			// Cache directories before we start copying
			DirectoryInfo[] dirs = sourceDir.GetDirectories();

			// Get the files in the source directory and copy to the destination directory
			foreach (FileInfo file in sourceDir.GetFiles()) {
				FileInfo targetFile = new FileInfo(Path.Combine(destinationDir.FullName, file.Name));
				if (targetFile.Exists) {
					if (!Utilities.FilesAreEqual(file, targetFile)) {
						Message.Write($"{targetFile} already exists and is getting overwritten because the files aren't equal.");
						targetFile.IsReadOnly = false;
						file.CopyTo(targetFile.FullName, true);
						targetFile.LastWriteTimeUtc = file.LastWriteTimeUtc;
					} else {
						/*Message.Write(
							new List<MessagePart>() {
							new MessagePart($"Not overwriting "),
							new MessagePart($"{targetFile}"){FColour=ConsoleColor.Magenta},
							new MessagePart($" because {file.LastWriteTimeUtc} <= {targetFile.LastWriteTimeUtc}."),
							}
						);*/
					}
				} else {
					Message.Write($"Adding {targetFile}.");
					file.CopyTo(targetFile.FullName, true);
					targetFile.CreationTimeUtc = file.CreationTimeUtc;
					targetFile.LastWriteTimeUtc = file.LastWriteTimeUtc;
				}
			}

			// Recursively call this method to copy subdirectories.
			foreach (DirectoryInfo subDir in dirs) {
				DirectoryInfo newDestinationDir = new DirectoryInfo(Path.Combine(destinationDir.FullName, subDir.Name));
				CopyDirectory(subDir, newDestinationDir);
			}

			return true; // TODO: Check it worked. Consider MD5 hash.
		}

		private bool Delete() {


			return true;
		}
	}
}
