using System;
using System.Diagnostics.CodeAnalysis;

namespace GenBOE.DataBridge.Core.ModelView
{
	/// <summary>
	/// This class will return the different proposal state types
	/// </summary>
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class ProposalStatusTypeModelView
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public ProposalStatusTypeModelView()
		{
			ProposalStateID = 0;
			ProposalStateType = string.Empty;
		}

		/// <summary>
		/// The ID of the workspace state
		/// </summary>
		public int ProposalStateID { get; set; }

		/// <summary>
		/// the name of the workspace state
		/// </summary>
		public string ProposalStateType { get; set; }
	}
}
