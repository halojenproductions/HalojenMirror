namespace HalojenBackups.MessageOutput {
	public class MessagePart {
		public ConsoleColor? FColour { get; set; }
		public ConsoleColor? BColour { get; set; }
		public string Text { get; set; }

		/// <summary>
		/// Forces this message part to writeline instead of write.
		/// </summary>
		/// <remarks>
		/// The last part in a message will always get a new line regardless of what ForceNewline is set to.
		/// </remarks>
		public bool ForceNewline { get; set; } = false;
		public MessagePart() { }
		public MessagePart(string message) {
			Text = message;
		}
	}
}
