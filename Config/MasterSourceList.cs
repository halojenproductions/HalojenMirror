namespace HalojenBackups.Config {
	internal enum Status {
		Valid,
		Copying,
		Copied,
		Checking,
		Checked,
		Deleting,
		Deleted

	}
	internal class MasterSourceList {
		public List<Source> Sources { get; set; }
	}
	internal class Source {
		public string Path { get; set; }
		public long Size { get; set; }
		public int Status { get; set; } = 0;
		public int Progress { get; set; }
		public Source(string path) {
			Path = path;
		}
	}
}
