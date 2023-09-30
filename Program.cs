using System.IO;
using CommandLine;
using System.Data;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using HalojenBackups;
using System.Diagnostics;
using HalojenBackups.MessageOutput;
using HalojenBackups.Config;
using ByteSizeLib;
using HalojenBackups.Destination;
using HalojenBackups.Operation;

Parser parser = new Parser(with => { with.EnableDashDash = true; with.HelpWriter = Console.Out; });
ParserResult<Options> config = parser.ParseArguments<Options>(args);
MasterSourceList masterSourceList = new();

config.WithParsed(Main);
void Main(Options options) {

	try {
		// Display parsed cmd arguments.
		Message.Write(Parser.Default.FormatCommandLine(options));


		//	Get destination location.
		DestinationDrive destination = new DestinationDrive(options);

		//	Import config file.
		ConfigFile.Import(options, masterSourceList);


		//	Analyse source tree.
		masterSourceList.Analyse();
		Message.Write(new List<MessagePart>() {
			new MessagePart($"Total source size ") ,
			new MessagePart($"{masterSourceList.TotalSize:#.000}"){FColour=ConsoleColor.Cyan},
		});

		//	Analyse destination tree.
		Message.Write(new List<MessagePart>() {
			new MessagePart($"Free space on destination drive "),
			new MessagePart($"{destination.FreeSpace:#.000}"){FColour=ConsoleColor.Cyan},
		});

		//	Validate space.
		if (destination.FreeSpace < masterSourceList.TotalSize) {
			// Nooooope, can't do this. It assumes that all files a new. Duh.
			//throw new Exception($"Not enough space to perform backup unless you choose delete-first.");
		}


		//	For each operation in config.
		foreach (var aoeu in masterSourceList.Sources) {
			var op = new Operation(aoeu, destination, options);
			//		Get source path.
			//		Do copies.
			op.Go();
			//		Validate success.
			//		Do deletes.
		}
		//	Final report.





	} catch (Exception ex) {
		Message.Write(
			new MessagePart() {
				FColour = ConsoleColor.Red,
				Text = ex.ToString()
			}
		);
	}
	Thread.Sleep(1000); // Let the console finish outputting.
}
