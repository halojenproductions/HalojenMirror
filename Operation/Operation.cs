using ByteSizeLib;

using CommandLine.Text;

using HalojenBackups.Config;
using HalojenBackups.Destination;
using HalojenBackups.MessageOutput;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HalojenBackups.Operation {
	internal class Operation {
		private DirectoryInfo SourceDir { get; set; }
		private DirectoryInfo DestinationDir { get; set; }
		private Options Options { get; set; }
		private Stopwatch stopwatchOverall { get; set; } = new Stopwatch();
		private Stopwatch stopwatch1 { get; set; } = new Stopwatch();
		public Operation(Source source, Destination.DestinationDrive destination, Options options) {
			SourceDir = new DirectoryInfo(source.SourcePath);
			DestinationDir = new DirectoryInfo(Path.Combine(destination.RootPath, source.DestPath));
			Options = options;
			Message.Write($"{DateTime.Now.ToLongTimeString()} Doing the thing {SourceDir}");
		}
		public void Go() {
			stopwatchOverall.Start();
			var synced = SyncDirectory(SourceDir, DestinationDir);
			stopwatchOverall.Stop();

			var timePercent = (stopwatch1.Elapsed / stopwatchOverall.Elapsed) * 100;

			if (synced) {
				Message.Write(
					new MessagePart($"{DateTime.Now.ToLongTimeString()} "),
					new MessagePart($"{SourceDir} ") { FColour = ConsoleColor.Cyan },
					new MessagePart($"synchronisation complete. Hopefully it worked.") { FColour = ConsoleColor.Green }
				);
			} else {
				Message.Write(
					new MessagePart($"{DateTime.Now.ToLongTimeString()} "){ BColour = ConsoleColor.DarkRed },
					new MessagePart($"{SourceDir} ") { FColour = ConsoleColor.Cyan, BColour=ConsoleColor.DarkRed },
					new MessagePart($"synchronisation failed.") { FColour = ConsoleColor.White, BColour = ConsoleColor.DarkRed }
				);
			}
			Message.Write(new MessagePart($"Total time was {stopwatchOverall.Elapsed}. Time spent comparing files was {stopwatch1.Elapsed}, which was {timePercent}%.") { FColour = ConsoleColor.Blue, BColour = ConsoleColor.Black });
		}

		private bool SyncDirectory(DirectoryInfo sourceDir, DirectoryInfo destDir) {
			if (Options.Verbose) {
				Message.Write(
					new MessagePart($"Synching "),
					new MessagePart($"{destDir}") { FColour = ConsoleColor.Cyan }
				);
			}

			try {
				if (!sourceDir.Exists) {
					Message.Write(new MessagePart($"Source directory not found: {sourceDir.FullName}"));
					return false;
				}

				DirectoryInfo[] sourceSubDirs = sourceDir.GetDirectories();
				FileInfo[] sourceFiles = sourceDir.GetFiles();

				DirectoryInfo[] destSubDirs = new DirectoryInfo[] { };

				// Create the destination directory if it doesn't exist in the destination.
				if (destDir.Exists) {
					destSubDirs = destDir.GetDirectories();
				} else {
					AddDir(destDir);
				}

				// Get the files in the source directory and copy to the destination directory
				foreach (FileInfo sourceFile in sourceFiles) {
					FileInfo targetFile = new FileInfo(Path.Combine(destDir.FullName, sourceFile.Name));
					try {
						if (targetFile.Exists) {
							bool needsUpdating;
							// 0 = Only add new.
							// 1 = Compare timestamps.
							// 2 = Compare timestamps and contents.
							// 3 = Always copy.
							if (Options.Mode == 0) {
								needsUpdating = false; // Never overwrite.
							} else if (Options.Mode == 3) {
								needsUpdating = true; // Always overwrite.
							} else {
								needsUpdating = (sourceFile.LastWriteTimeUtc > targetFile.LastWriteTimeUtc);

								// Check that it's _actually_ changed and not just touched. I.e. eliminate false positives.
								// Unless it's small, in which case just copy anyway. That's probabably faster than comparing.
								// Not that I've bench tested this at all. I'm just wildly guessing.
								// I need to somehow make a graph of: time to calculate hashes vs. time to overwrite vs. file size.
								// There must be a crossover somewhere.
								// I guess the hash time is, realistically, just disk read time. 
								// And reading will be faster than writing at _any_ file size.
								// And reading is less taking on disk, right?
								// Ok so that settles it. I should always hash. I'll commeent out the size check.
								if (needsUpdating && Options.Mode == 2 /*&& ByteSize.FromBytes(sourceFile.Length).MegaBytes > 100*/) {
									stopwatch1.Start();
									needsUpdating = !Utilities.FilesAreEqual(sourceFile, targetFile);
									stopwatch1.Stop();
								}
							}


							if (needsUpdating) {
								UpdateFile(sourceFile, targetFile);
							} else {
								if (Options.Verbose) {
									Message.Write($"Leaving {targetFile}");
								}
							}
						} else {
							AddFile(sourceFile, targetFile);
						}
					} catch (Exception ex) {
						Message.Write(
							new MessagePart($"Couldn't copy ") { FColour = ConsoleColor.White, BColour = ConsoleColor.DarkRed },
							new MessagePart($"{sourceFile}") { FColour = ConsoleColor.Cyan, BColour = ConsoleColor.DarkRed, ForceNewline = true },
							new MessagePart($"{ex.Message}") { FColour = ConsoleColor.Red }
						);
					}
				}


				// Recursively call this method to copy subdirectories.
				foreach (DirectoryInfo subDir in sourceSubDirs) {
					DirectoryInfo newDestinationDir = new DirectoryInfo(Path.Combine(destDir.FullName, subDir.Name));
					// Let the mindfuck begin. Heeeere weee goooo...
					SyncDirectory(subDir, newDestinationDir);
					// Are you looking for: recursion https://www.google.com/search?q=recursion
				}

				// Deletions.
				// Doing this after recursing means that deletions are deferred until after copies/updates are completed for subdir.
				// I.e. it copies on its way in and deletes on its way back out.
				// ..Actually that could still have situations where a file that was moved will be deleted in the
				// destination before it's added to the new destination location. Hmmm.
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

			} catch (Exception ex) {
				Message.Write(
					new MessagePart($"Crashed when syncing "),
					new MessagePart($"{sourceDir}") { FColour = ConsoleColor.Red, ForceNewline = true },
					new MessagePart($"{ex.Message}") { FColour = ConsoleColor.Red }
				);
				return false;
			}
		}

		private void AddDir(DirectoryInfo destDir) {
			MessagePart messagePart = new MessagePart($"{destDir}") { FColour = ConsoleColor.Yellow };
			if (Options.Write) {
				Message.Write(
					new MessagePart($"Adding "),
					messagePart
				);
				destDir.Create();
			} else {
				Message.Write(
					new MessagePart($"Pretending to add "),
					messagePart
				);
			}
		}

		private void UpdateFile(FileInfo sourceFile, FileInfo targetFile) {
			MessagePart messagePart = new MessagePart($"{targetFile}") { FColour = ConsoleColor.Magenta };
			if (Options.Write) {
				Message.Write(
					new MessagePart($"Updating "),
					messagePart
				);
				targetFile.IsReadOnly = false;
				sourceFile.CopyTo(targetFile.FullName, true);
				targetFile.LastWriteTimeUtc = sourceFile.LastWriteTimeUtc;
			} else {
				Message.Write(
					new MessagePart($"Pretending to update "),
					messagePart
				);
			}
		}

		private void AddFile(FileInfo sourceFile, FileInfo targetFile) {
			MessagePart messagePart = new MessagePart($"{targetFile}") { FColour = ConsoleColor.Yellow };
			if (Options.Write) {
				if (Options.Verbose) {
					Message.Write(
						new MessagePart($"Adding "),
						messagePart
					);
				}
				sourceFile.CopyTo(targetFile.FullName, true);
				targetFile.CreationTimeUtc = sourceFile.CreationTimeUtc;
				targetFile.LastWriteTimeUtc = sourceFile.LastWriteTimeUtc;
			} else if (Options.Verbose) {
				Message.Write(
					new MessagePart($"Pretending to add "),
					messagePart
				);
			}
		}

		private void DeleteDirectory(DirectoryInfo directory) {
			MessagePart messagePart = new MessagePart($"{directory.FullName}") { FColour = ConsoleColor.Red };
			if (Options.Write && Options.Remove) {
				Message.Write(
					new MessagePart($"Deleting "),
					messagePart
				);
				foreach (var aoeu in directory.GetFileSystemInfos("*", SearchOption.AllDirectories)) {
					// Ensure no files are read-only because that would prevent the dir being deleted.
					aoeu.Attributes = FileAttributes.Normal;
				}
				directory.Delete(true);
			} else {
				Message.Write(
					new MessagePart($"Pretending to delete "),
					messagePart
				);
			}
		}

		private void DeleteFile(FileInfo file) {
			MessagePart messagePart = new MessagePart($"{file.FullName}") { FColour = ConsoleColor.Red };
			if (Options.Write && Options.Remove) {
				Message.Write(
					new MessagePart($"Deleting "),
					messagePart
				);
				// Ensure the file is not read-only before trying to delete it.
				file.Attributes = FileAttributes.Normal;
				file.Delete();
			} else {
				Message.Write(
					new MessagePart($"Pretending to delete "),
					messagePart
				);
			}
		}
	}
}
