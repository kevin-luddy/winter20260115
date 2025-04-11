using System;
using System.Diagnostics.CodeAnalysis;

namespace GenBOE.DataBridge.Core.ModelView
{
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class ProposalClassTypeModelView
	{
		public ProposalClassTypeModelView()
		{
			ProposalClassID = 0;
			ProposalClass = string.Empty;
		}

		public int ProposalClassID { get; set; }
		public string ProposalClass { get; set; }
	}
}
