using ByteSizeLib;
using HalojenBackups.Config;
using HalojenBackups.MessageOutput;
using System.Security.AccessControl;
using System.Security.Principal;

namespace HalojenBackups.Destination;
internal class DestinationDrive {
	public DriveInfo? LocationInfo { get; private set; }
	public string? Path { get; set; }
	public string RootPath { get; private set; }
	public ByteSize FreeSpace {
		get {
			return ByteSize.FromBytes(LocationInfo?.AvailableFreeSpace ?? 0);
		}
	}
	public DestinationDrive(Options options) {
		FindDriveByLabel(options.DestLabel);
		SetRootPath(options);
	}

	public void FindDriveByLabel(string driveLabel) {
		IList<DriveInfo> drives = DriveInfo.GetDrives()
			.Where(d => d.DriveType == DriveType.Fixed)
			.Where(d => d.IsReady)
			.Where(d => d.VolumeLabel.StartsWith(driveLabel))
			.ToList();

		if (drives.Count != 1) {
			throw new Exception("Drive not found.");
		}

		LocationInfo = drives.First();
		ReportFound(LocationInfo.VolumeLabel, LocationInfo.Name);
	}

	public void FindDriveByLetter(string driveLetter) {
		IList<DriveInfo> drives = DriveInfo.GetDrives()
			.Where(d => d.DriveType == DriveType.Fixed)
			.Where(d => d.IsReady)
			.Where(d => d.Name == driveLetter)
			.ToList();

		if (drives.Count != 1) {
			throw new Exception("Drive not found.");
		}

		LocationInfo = drives.First();
		ReportFound(LocationInfo.VolumeLabel, LocationInfo.Name);
	}

	protected bool CheckWritePermission(DirectoryInfo directoryInfo) {
		try {
			FileSystemRights AccessRight = FileSystemRights.Write;

			AuthorizationRuleCollection rules = directoryInfo.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
			WindowsIdentity identity = WindowsIdentity.GetCurrent();

			foreach (FileSystemAccessRule rule in rules) {
				if (identity.Groups.Contains(rule.IdentityReference)) {
					if ((AccessRight & rule.FileSystemRights) == AccessRight) {
						if (rule.AccessControlType == AccessControlType.Allow)
							return true;
					}
				}
			}
		} catch (Exception e) {
			throw;
		}
		return false;
	}

	protected void ReportFound(string driveLabel, string driveLetter) {
		var messageBlock = new List<MessagePart>();
		messageBlock.Add(new MessagePart($"Found drive "));
		messageBlock.Add(new MessagePart($"{driveLabel}") { FColour = ConsoleColor.Cyan });
		messageBlock.Add(new MessagePart($" ("));
		messageBlock.Add(new MessagePart($"{driveLetter}") { FColour = ConsoleColor.Cyan });
		messageBlock.Add(new MessagePart($")."));
		Message.Write(messageBlock);
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
