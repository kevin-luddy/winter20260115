using System.Runtime.Serialization;

namespace IES.Common.Core.Exceptions
{
	[Serializable]
	public class TransactionFailureException : Exception
	{
		protected TransactionFailureException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public TransactionFailureException(String message, Exception innerException)
			: base(message, innerException)
		{
		}

		public TransactionFailureException(String message)
			: base(message)
		{
		}

		public TransactionFailureException()
			: base()
		{
		}
	}
}