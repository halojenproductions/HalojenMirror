using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalojenBackups.MessageOutput;

namespace HalojenBackups.Locations;
internal class OutputLocation : LocationBase, ILocation {
	public string RootPath { get; private set; }
    public OutputLocation(Options options)
    {
		FindDriveByLabel(options.DestLabel);
		SetRootPath(options);
	}

	public void SetRootPath(Options options) {
		if (LocationInfo?.Name is null) {
			throw new Exception($"Can't find output directory '{options.DestDir}' because the drive hasn't been found.");
		}

		string fullPath = System.IO.Path.Combine(LocationInfo.Name, options.DestDir);
		DirectoryInfo directoryInfo = new DirectoryInfo(fullPath);

		try {
			if (!directoryInfo.Exists && options.DestCreate) {
				directoryInfo.Create();
				Message.Write(
					new List<MessagePart> {
						new MessagePart($"Creating root directory "),
						new MessagePart($"{options.DestDir}"){FColour=ConsoleColor.Cyan},
						new MessagePart($".")
					}
				);
			}

			if (directoryInfo.Exists) {
				RootPath = fullPath;
				Message.Write(
					new List<MessagePart> {
						new MessagePart($"Output root directory is set to "),
						new MessagePart($"{RootPath}"){FColour=ConsoleColor.Cyan},
						new MessagePart($".")
					}
				);
			} else {
				throw new Exception($"Directory '{options.DestDir}' doesn't exist.");
			}
		} catch (Exception e) {
			throw; // Meh.
		}
	}

}
