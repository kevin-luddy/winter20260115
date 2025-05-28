// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using GenBOE.ActionLogic.ModelView.Clin;
	using IES.Common.PickList;

	public class ManageCLINGridModelView 
	{
		public ManageCLINGridModelView()
			: base()
		{
			ClinResults = new Collection<ManageCLINModelView>();
		}

		public Collection<ManageCLINModelView> ClinResults { get; set; }
		public bool HideContractType { get; internal set; }

		public ICollection<PickListDto> ContractTypeList { get; set; }
	}
}
