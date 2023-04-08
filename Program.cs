using System.IO;
using CommandLine;
using System.Data;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using HalojenBackups;
using HalojenBackups.Locations;
using System.Diagnostics;
using HalojenBackups.MessageOutput;
using HalojenBackups.Config;
using ByteSizeLib;

Parser parser = new Parser(with => { with.EnableDashDash = true; with.HelpWriter = Console.Out; });
ParserResult<Options> config = parser.ParseArguments<Options>(args);
MasterSourceList masterSourceList = new();

config.WithParsed(Main);
void Main(Options options) {

	try {
		//	Get destination location.
		DestinationLocation destination = new DestinationLocation(options);

		//	Import config file.
		ConfigFile.Import(options, masterSourceList);


		//	Analyse source tree.
		masterSourceList.Analyse();
		Message.Write(new List<MessagePart>() {
			new MessagePart($"Total size to back up ") ,
			new MessagePart($"{masterSourceList.TotalSize:#.000}"){FColour=ConsoleColor.Cyan},
		});

		//	Analyse destination tree.
		Message.Write(new List<MessagePart>() {
			new MessagePart($"Free space on destination drive "),
			new MessagePart($"{destination.FreeSpace:#.000}"){FColour=ConsoleColor.Cyan},
		});

		//	Validate space.
		if (destination.FreeSpace < masterSourceList.TotalSize) {
			throw new Exception($"Not enough space to perform backup unless you choose delete-first.");
		}


		//	For each operation in config.
		//		Get source path.
		//		Do copies.
		//		Validate success.
		//		Do deletes.

		//	Final report.





	} catch (Exception ex) {
		Message.Write(
			new MessagePart() {
				FColour = ConsoleColor.Red,
				Text = ex.ToString()
			}
		);
	}
	Thread.Sleep(10); // Let the console finish outputting.
}
