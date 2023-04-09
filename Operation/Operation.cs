using HalojenBackups.Config;
using HalojenBackups.Destination;
using HalojenBackups.MessageOutput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		private Stopwatch stopwatchOverall { get; set; } = new Stopwatch();
		private Stopwatch stopwatch1 { get; set; } = new Stopwatch();
		public Operation(Source source, Destination.DestinationDrive destination, Options options) {
			_source = source;
			SourceDir = new DirectoryInfo(source.SourcePath);
			DestinationDir = new DirectoryInfo(Path.Combine(destination.RootPath, source.DestPath));
			Options = options;
			Message.Write($"{DateTime.Now.ToLongTimeString()} Gonna do a thang with {DestinationDir}");
		}
		public void Go() {
			stopwatchOverall.Start();
			if (CopyDirectory(SourceDir, DestinationDir)) {
				stopwatchOverall.Stop();
				var aoeu = (stopwatch1.Elapsed / stopwatchOverall.Elapsed) * 100;

				Message.Write(new MessagePart($"{DateTime.Now.ToLongTimeString()} Copy was successful, or so they say.") { FColour = ConsoleColor.Green });
				Message.Write(new MessagePart($"Total time was {stopwatchOverall.Elapsed}. Time spent comparing files was {stopwatch1.Elapsed}, which was {aoeu}%.") { FColour = ConsoleColor.Blue,BColour=ConsoleColor.Black });
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
					stopwatch1.Start();
					bool filesAreEqual = Utilities.FilesAreEqual(file, targetFile);
					stopwatch1.Stop();
					if (!filesAreEqual) {
						Message.Write(
							new List<MessagePart>() {
								new MessagePart($"Updating "),
								new MessagePart($"{targetFile}"){FColour=ConsoleColor.Magenta},
								new MessagePart($"."),
							}
						);
						targetFile.IsReadOnly = false;
						file.CopyTo(targetFile.FullName, true);
						targetFile.LastWriteTimeUtc = file.LastWriteTimeUtc;
					} else {
						Message.Write(
							new List<MessagePart>() {
							new MessagePart($"Leaving "),
							new MessagePart($"{targetFile}"){FColour=ConsoleColor.Green},
							new MessagePart($"."),
							}
						);
					}
				} else {
					Message.Write(
						new List<MessagePart>() {
							new MessagePart($"Adding "),
							new MessagePart($"{targetFile}"){FColour=ConsoleColor.Yellow},
							new MessagePart($"."),
						}
					);
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
