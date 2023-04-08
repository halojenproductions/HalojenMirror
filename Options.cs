using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace HalojenBackups {
	internal class Options {
		[Option('s', "sources", HelpText = "The file listing all directories to be backed up.", Required = false, Default = "sources.txt")]
		public string Sources { get; set; }

		[Option('n', "dest-name", HelpText = "Prefix of the label of the destination drive.", Required = false, Default = "Backup-")]
		public string DestLabel { get; set; }

		[Option('d', "dest-dir", HelpText = "Directory to put backups in.", Required = false, Default = "Backup")]
		public string DestDir { get; set; }


		[Option('c', "dest-create", HelpText = "Whether to create the destination directory if it doesn't exist.", Required = false, Default = false)]
		public bool DestCreate { get; set; }


		[Option('w', "write", HelpText = "Whether to write to the destination. (Otherwise it's just a simulation).", Required = false, Default = true)]
		public bool Write { get; set; }

		[Option('r', "remove", HelpText = "Whether to remove files from the destination that are no longer in the source. (False if 'write' is false).", Required = false, Default = false)]
		public bool Remove { get; set; }


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
