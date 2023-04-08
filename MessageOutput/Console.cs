using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalojenBackups.MessageOutput {
	public static class Message {
		// https://stackoverflow.com/a/3670628
		private static BlockingCollection<List<MessagePart>> m_Queue = new BlockingCollection<List<MessagePart>>(); // Could make this an array of messages for more fastness.

		static Message() {
			//Console.BackgroundColor = ConsoleColor.Black; // Powershell blue is yucky.
			var thread = new Thread(() => {
				while (true) {
					List<MessagePart> messageParts = m_Queue.Take(); // Blocks until there is an item to take.

					ConsoleColor originalForeColour = Console.ForegroundColor;
					ConsoleColor originalBackColour = Console.BackgroundColor;

					for (int i = 0; i < messageParts.Count; i++) {
						MessagePart messagePart = messageParts[i];

						SetForeColour(messagePart.FColour);
						SetBackColour(messagePart.BColour);

						if (i == messageParts.Count - 1 || messagePart.ForceNewline) {
							Console.WriteLine(messagePart.Text);
						} else {
							Console.Write(messagePart.Text);
						}
					}

					// Reset colours in case something else writes directly to the console.
					SetForeColour(originalForeColour);
					SetBackColour(originalBackColour);
				}
			});

			thread.IsBackground = true;
			thread.Start();
		}
		private static void SetForeColour(ConsoleColor? colour) {
			if (colour != null && colour != Console.ForegroundColor) {
				Console.ForegroundColor = colour.Value;
			}
		}
		private static void SetBackColour(ConsoleColor? colour) {
			if (colour != null && colour != Console.BackgroundColor) {
				Console.BackgroundColor = colour.Value;
			}
		}

		public static void Write(List<MessagePart> messages) {
			m_Queue.Add(messages);
		}
		public static void Write(MessagePart message) {
			Write(new List<MessagePart>() { message });
		}
		public static void Write(string message) {
			Write(new MessagePart(message));
		}
	}
}
