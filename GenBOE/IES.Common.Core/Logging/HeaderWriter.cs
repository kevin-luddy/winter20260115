// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Logging
{
	using System;
	using System.IO;
	using System.Text;
	using Serilog.Debugging;
	using Serilog.Sinks.File;

	/// <inheritdoc />
	/// <summary>
	/// Writes a header at the start of every log file
	/// </summary>
	public class HeaderWriter : FileLifecycleHooks
	{
		// Same as the default StreamWriter buffer size
		private const int DEFAULT_BUFFER_SIZE = 1024;

		// Factory method to generate the file header
		private readonly Func<string> headerFactory;

		// Whether to always write the header, even for non-empty files
		private readonly bool alwaysWriteHeader;

		public HeaderWriter(string header, bool alwaysWriteHeader = false)
		{
			headerFactory = () => header;
			this.alwaysWriteHeader = alwaysWriteHeader;
		}

		public HeaderWriter(Func<string> headerFactory, bool alwaysWriteHeader = false)
		{
			this.headerFactory = headerFactory;
			this.alwaysWriteHeader = alwaysWriteHeader;
		}

		public override Stream OnFileOpened(Stream underlyingStream, Encoding encoding)
		{
			try
			{
				if (!alwaysWriteHeader && underlyingStream.Length != 0)
				{
					SelfLog.WriteLine($"File header will not be written, as the stream already contains {underlyingStream.Length} bytes of content");
					return base.OnFileOpened(underlyingStream, encoding);
				}
			}
			catch (NotSupportedException)
			{
				// Not all streams support reading the length - in this case, we always write the header,
				// otherwise we'd *never* write it!
			}

			using (StreamWriter writer = new(underlyingStream, encoding, DEFAULT_BUFFER_SIZE, true))
			{
				string header = headerFactory();

				writer.WriteLine(header);
				writer.Flush();
				underlyingStream.Flush();
			}

			return base.OnFileOpened(underlyingStream, encoding);
		}
	}
}