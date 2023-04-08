using ByteSizeLib;
using HalojenBackups.Config;
using HalojenBackups.MessageOutput;
using System.Security.AccessControl;

namespace HalojenBackups.Locations;
internal class DestinationLocation : LocationBase, ILocation {
	public string RootPath { get; private set; }
	public ByteSize FreeSpace {
		get {
			return ByteSize.FromBytes(LocationInfo?.AvailableFreeSpace ?? 0);
		}
	}
	public DestinationLocation(Options options) {
		FindDriveByLabel(options.DestLabel);
		SetRootPath(options);
	}

	public void SetRootPath(Options options) {
		if (LocationInfo?.Name is null) {
			throw new Exception($"Can't find output directory '{options.DestDir}' because the drive hasn't been found.");
		}

		string fullPath = System.IO.Path.Combine(LocationInfo.Name, options.DestDir);
		DirectoryInfo directoryInfo = new DirectoryInfo(fullPath);

		try {
			if (!directoryInfo.Exists && options.DestCreate) {
				directoryInfo.Create();
				Message.Write(
					new List<MessagePart> {
						new MessagePart($"Creating root directory "),
						new MessagePart($"{options.DestDir}"){FColour=ConsoleColor.Cyan},
						new MessagePart($".")
					}
				);
			}

			if (directoryInfo.Exists && CheckWritePermission(directoryInfo)) {
				RootPath = fullPath;
				Message.Write(
					new List<MessagePart> {
						new MessagePart($"Output root directory is set to "),
						new MessagePart($"{RootPath}"){FColour=ConsoleColor.Cyan},
						new MessagePart($".")
					}
				);
			} else {
				throw new Exception($"Directory '{options.DestDir}' doesn't exist (or can't be written to).");
			}
		} catch (Exception e) {
			throw; // Meh.
		}

	}

}
