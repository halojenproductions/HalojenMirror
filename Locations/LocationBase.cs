using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalojenBackups.MessageOutput;

namespace HalojenBackups.Locations;
internal abstract class LocationBase : ILocation {
	public DriveInfo? LocationInfo { get; private set; }
	public string? Path { get; set; }


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

	private void ReportFound(string driveLabel,string driveLetter) {
		var messageBlock = new List<MessagePart>();
		messageBlock.Add(new MessagePart($"Found drive "));
		messageBlock.Add(new MessagePart($"{driveLabel}") { FColour = ConsoleColor.Cyan });
		messageBlock.Add(new MessagePart($" ("));
		messageBlock.Add(new MessagePart($"{driveLetter}") { FColour = ConsoleColor.Cyan });
		messageBlock.Add(new MessagePart($")."));
		Message.Write(messageBlock);
	}
}
