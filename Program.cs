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

Parser parser = new Parser(with => { with.EnableDashDash = true; with.HelpWriter = Console.Out; });
ParserResult<Options> config = parser.ParseArguments<Options>(args);
List<Source> masterSourceList = new();

config.WithParsed(Main);
void Main(Options options) {

	try {
		//	Get destination location.
		DestinationLocation destination = new DestinationLocation(options);

		//	Import config file.
		ConfigFile.Import(options, masterSourceList);


		//	Analyse source tree.

		//	Analyse destination tree.

		//	Validate space.

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
