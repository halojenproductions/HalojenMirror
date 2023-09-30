using HalojenBackups.MessageOutput;
using System.Text;

namespace HalojenBackups.Config;
internal static class ConfigFile {
	public static void Import(Options config, MasterSourceList masterSourceList) {
		FileInfo sourceFile = new FileInfo(config.Sources);
		if (!sourceFile.Exists) {
			throw new Exception($"Source list file {sourceFile} was not found.");
		}

		using (var fileStream = File.OpenRead(config.Sources)) {
			using (var streamReader = new StreamReader(fileStream)) {
				String line;
				while ((line = streamReader.ReadLine()) != null) {
					if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#")) {
						masterSourceList.Sources.Add(
							new Source(line.Trim().Trim('\"'))
						);
					}
				}
			}
		}

		Message.Write(
			new List<MessagePart> {
				new MessagePart($"Successfully imported "),
				new MessagePart($"{masterSourceList.Sources.Count} "){FColour=ConsoleColor.Cyan},
				new MessagePart($"{Pluralise("path",masterSourceList.Sources.Count)} from source list file "),
				new MessagePart($"{sourceFile}"){FColour=ConsoleColor.Cyan},
				new MessagePart($".")
			}
		);

		string Pluralise(string word, int count) {
			if (count == 1) {
				return word;
			} else {
				return word + "s";
			}
		}
	}
}