namespace HalojenBackups.Locations {
	internal interface ILocation {
		public DriveInfo? LocationInfo { get;  }
		public string? Path { get; set; }

		//public bool FindDriveByLabel(string driveLabel);
		//public bool FindDriveByLetter(string driveLetter);
	}
}