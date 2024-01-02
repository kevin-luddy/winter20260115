using System;

namespace IES.Standard
{
	public interface ILogger2
	{
		string Identifier { get; }

		/// <summary>
		/// Write an entry to our performance log
		/// </summary>
		/// <param name="inMessage">the message to log</param>
		/// <param name="inTimeTaken">milliseconds taken for method</param>
		/// <param name="traceStatement">If true, set severity to verbose.</param>
		void Performance(string inMessage, long inTimeTaken, bool traceStatement = false);

		bool DebugEnabled { get; }

		void Debug(string inMessage);

		void Debug(Exception exception, string inMessage = null);

		void Info(string inMessage);

		void Info(Exception exception, string inMessage = null);

		void Warn(string inMessage);

		void Warn(Exception exception, string inMessage = null);

		void Error(string inMessage);

		void Error(Exception exception, string inMessage = null);
	}

}
