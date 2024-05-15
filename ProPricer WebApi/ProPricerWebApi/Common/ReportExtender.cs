/*
	Copyright 2016-2024 Lockheed Martin Corporation.

	This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
	commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
	by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
	and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi.Common
{
	using EBS.ProPricer.Model;
	using EBS.ProPricer.Reports;

	/// <summary>
	/// Extension class for Pro Pricer Reports
	/// </summary>
	public class ReportExtender : IReportEx
	{
		private readonly Proposal proposal;

		public ReportExtender(Proposal proposal)
		{
			this.proposal = proposal;
		}


		public void AskToContinue(string message)
		{

		}

		public void OnCurrentTableNotUsed(string message)
		{

		}

		public Project RequestCurrentProject()
		{
			return this.proposal.ParentProject;
		}

		public Proposal RequestCurrentProposal()
		{
			return this.proposal;
		}
	}
}
