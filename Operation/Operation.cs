using HalojenBackups.Config;
using HalojenBackups.Destination;
using HalojenBackups.MessageOutput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
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
			Message.Write($"{DateTime.Now.ToLongTimeString()} Doing the thing {SourceDir}");
		}
		public void Go() {
			stopwatchOverall.Start();
			if (SyncDirectory(SourceDir, DestinationDir)) {
				stopwatchOverall.Stop();
				var aoeu = (stopwatch1.Elapsed / stopwatchOverall.Elapsed) * 100;

				Message.Write(new MessagePart($"{DateTime.Now.ToLongTimeString()} {SourceDir} synchronisation complete. Hopefully it worked.") { FColour = ConsoleColor.Green });
				Message.Write(new MessagePart($"Total time was {stopwatchOverall.Elapsed}. Time spent comparing files was {stopwatch1.Elapsed}, which was {aoeu}%.") { FColour = ConsoleColor.Blue, BColour = ConsoleColor.Black });
			}

		}

		private bool SyncDirectory(DirectoryInfo sourceDir, DirectoryInfo destDir) {
			if (Options.Verbose) {
				Message.Write(
					new List<MessagePart>() {
						new MessagePart($"Synching "),
						new MessagePart($"{destDir}"){FColour=ConsoleColor.Cyan}
					}
				);
			}

			if (!sourceDir.Exists) {
				throw new DirectoryNotFoundException($"Source directory not found: {sourceDir.FullName}");
			}

			DirectoryInfo[] sourceSubDirs = sourceDir.GetDirectories();
			FileInfo[] sourceFiles = sourceDir.GetFiles();

			DirectoryInfo[] destSubDirs = new DirectoryInfo[] { };

			// Create the destination directory if it doesn't exist in the destination.
			if (destDir.Exists) {
				destSubDirs = destDir.GetDirectories();
			} else {
				Message.Write(
					new List<MessagePart>() {
						new MessagePart($"Adding "),
						new MessagePart($"{destDir}"){FColour=ConsoleColor.Yellow}
					}
				);
				destDir.Create();
			}

			// Get the files in the source directory and copy to the destination directory
			foreach (FileInfo sourceFile in sourceFiles) {
				FileInfo targetFile = new FileInfo(Path.Combine(destDir.FullName, sourceFile.Name));
				try {
					if (targetFile.Exists) {
						stopwatch1.Start();
						bool filesAreEqual = Utilities.FilesAreEqual(sourceFile, targetFile);
						stopwatch1.Stop();
						if (!filesAreEqual) {
							Message.Write(
								new List<MessagePart>() {
								new MessagePart($"Updating "),
								new MessagePart($"{targetFile}") { FColour = ConsoleColor.Magenta}
								}
							);
							targetFile.IsReadOnly = false;
							sourceFile.CopyTo(targetFile.FullName, true);
							targetFile.LastWriteTimeUtc = sourceFile.LastWriteTimeUtc;
						} else {
							if (Options.Verbose) {
								Message.Write(
									new List<MessagePart>() {
									new MessagePart($"Leaving {targetFile}")
									}
								);
							}
						}
					} else {
						if (Options.Verbose) {
							Message.Write(
								new List<MessagePart>() {
								new MessagePart($"Adding "),
								new MessagePart($"{targetFile}"){FColour=ConsoleColor.Yellow}
								}
							);
						}
						sourceFile.CopyTo(targetFile.FullName, true);
						targetFile.CreationTimeUtc = sourceFile.CreationTimeUtc;
						targetFile.LastWriteTimeUtc = sourceFile.LastWriteTimeUtc;
					}
				} catch (Exception ex) {
					Message.Write(
						new List<MessagePart>() {
							new MessagePart($"Couldn't copy "),
							new MessagePart($"{sourceFile}"){FColour=ConsoleColor.Red,ForceNewline=true},
							new MessagePart($"{ex.Message}"){FColour=ConsoleColor.Red}
						}
					);
				}
			}


			// Recursively call this method to copy subdirectories.
			foreach (DirectoryInfo subDir in sourceSubDirs) {
				DirectoryInfo newDestinationDir = new DirectoryInfo(Path.Combine(destDir.FullName, subDir.Name));
				// Lead the brainfuck begin. Heeeere we goooo...
				SyncDirectory(subDir, newDestinationDir);
			}

			// Deletions.
			// Doing this after recursing means that deletions are deferred until after copies/updates are completed for subdir.
			// I.e. it copies on its way in and deletes on its way back out.
			foreach (
				DirectoryInfo destSubDir in destSubDirs
				.Where(dsb =>
					!sourceSubDirs.Select(ssb => ssb.Name).Contains(dsb.Name)
				)
			) {
				DeleteDirectory(destSubDir);
			}

			FileInfo[] destFiles = destDir.GetFiles();
			foreach (
				FileInfo destFile in destFiles
				.Where(df =>
					!sourceFiles.Select(sf => sf.Name).Contains(df.Name)
				)
			) {
				DeleteFile(destFile);
			}

			return true; // TODO: Check it worked. Consider MD5 hash.
		}

		private static bool DeleteDirectory(DirectoryInfo directory) {
			Message.Write(
				new List<MessagePart>() {
					new MessagePart($"Deleting "),
					new MessagePart($"{directory.FullName}"){FColour=ConsoleColor.Red},
					new MessagePart($"."),
				}
			);
			foreach (var aoeu in directory.GetFileSystemInfos("*", SearchOption.AllDirectories)) {
				// Ensure no files are read-only because that would prevent the dir being deleted.
				aoeu.Attributes = FileAttributes.Normal;
			}
			directory.Delete(true);
			return true;
		}

		private static bool DeleteFile(FileInfo file) {
			Message.Write(
				new List<MessagePart>() {
					new MessagePart($"Deleting "),
					new MessagePart($"{file.FullName}"){FColour=ConsoleColor.Red},
					new MessagePart($"."),
				}
			);
			// Ensure the file is not read-only before trying to delete it.
			file.Attributes = FileAttributes.Normal;
			file.Delete();
			return true;
		}
	}
}
