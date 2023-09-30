using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandLine;
using CommandLine.Text;

namespace HalojenBackups {
	internal class Options {
		[Option('s', "sources", Required = false, Default = "sources.txt",
			HelpText = "The file listing all directories to be backed up.")]
		public string Sources { get; set; }

		[Option('n', "dest-name", Required = false, Default = "Backup-",
			HelpText = "Prefix of the label of the destination drive.")]
		public string DestLabel { get; set; }

		[Option('d', "dest-dir", Required = false, Default = "Backup",
			HelpText = "Directory to put backups in.")]
		public string DestDir { get; set; }


		[Option('c', "dest-create", Required = false, Default = false,
			HelpText = "Whether to create the destination directory if it doesn't exist.")]
		public bool DestCreate { get; set; }


		[Option('w', "write", Required = false, Default = true,
			HelpText = "Whether to write to the destination. (Otherwise it's just a simulation).")]
		public bool Write { get; set; }

		[Option('r', "remove", Required = false, Default = false,
			HelpText = "Whether to remove files from the destination that are no longer in the source. (False if 'write' is false).")]
		public bool Remove { get; set; }

		[Option('m', "mode", Required = false, Default = 2,
			HelpText = "0 = Never overwrite. 1 = Compare timestamps. 2 = Compare timestamps and contents. 3 = Always overwrite.")]
		public int Mode {
			get {
				return _mode;
			}
			set {
				_mode = value switch {
					< 0 => 1,
					> 3 => 3,
					_ => value
				};
			}
		}
		private int _mode;


		[Option('v', "verbose", Required = false, Default = false,
					HelpText = "Log everything.")]
		public bool Verbose { get; set; }


		/*
		 * [Usage(ApplicationAlias = "yourapp")]
		public static IEnumerable<Example> Examples {
			get {
				return new List<Example>() {
					new Example("Convert file to a trendy format", new Options { File = "file.csv" })
				};
			}
		}
		*/
	}
}
