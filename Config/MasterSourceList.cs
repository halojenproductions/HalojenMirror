using ByteSizeLib;
using HalojenBackups.MessageOutput;
using System.Drawing;
using System.Text;

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
		public List<Source> Sources { get; set; } = new();
		public ByteSize TotalSize {
			get {
				return ByteSize.FromBytes(Sources.Sum(s => s.Size.Bytes));
			}
		}
		public void Analyse() {
			StringBuilder sb = new StringBuilder();
			List<MessagePart> message = new List<MessagePart>();

			Message.Write($"Checking free space."); // TODO: This is dumb, it assumes there's nothing already backed up. Oppsies.

			foreach (Source source in Sources) {
				DirectoryInfo directoryInfo = new DirectoryInfo(source.SourcePath);
				source.Size = ByteSize.FromBytes(DirSize(directoryInfo));
				string driveLetter = directoryInfo.Root.ToString().Substring(0, 1);
				string restOfPath = directoryInfo.FullName.Substring(directoryInfo.Root.Name.Length);
				source.DestPath = Path.Combine(driveLetter, restOfPath);
				message.Add(new MessagePart(source.SourcePath) { FColour = ConsoleColor.Green });
				message.Add(new MessagePart($" {source.Size:#.000}") { FColour = ConsoleColor.Cyan, ForceNewline = true });
			}

			Message.Write(message);
		}
		private static long DirSize(DirectoryInfo d) {
			long size = 0;
			// Add file sizes.
			FileInfo[] fis = d.GetFiles();
			foreach (FileInfo fi in fis) {
				size += fi.Length;
			}
			// Add subdirectory sizes.
			DirectoryInfo[] dis = d.GetDirectories();
			foreach (DirectoryInfo di in dis) {
				size += DirSize(di);
			}
			return size;
		}
	}
	internal class Source {
		public string SourcePath { get; set; }
		public string DestPath { get; set; }
		public ByteSize Size { get; set; }
		public int Status { get; set; } = 0;
		public int Progress { get; set; }
		public Source(string path) {
			SourcePath = path;
		}
	}
}
