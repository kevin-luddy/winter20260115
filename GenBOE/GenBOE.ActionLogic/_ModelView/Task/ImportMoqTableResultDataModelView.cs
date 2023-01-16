// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{

	using System.Collections.Generic;
	using System.Linq;
	using IES.Common;

	/// <summary>
	/// Model View returned after Import MOQ Table has been validated (and possibly had SAP Actuals calculated)
	/// </summary>
	public class ImportMoqTableResultDataModelView
	{
		/// <summary>
		/// The Total Result of the MOQ Table Import
		/// </summary>
		public ICollection<ImportMoqTableResultsModelView> Result { get; set; } = new List<ImportMoqTableResultsModelView>();

		/// <summary>
		/// The Data that will be saved during Import
		/// </summary>
		/// <returns></returns>
		public ICollection<ImportMoqTableResultsModelView> DataToSave()
		{ 
			return Result.Where(x => x.ImportType == (int)MoqTableImportType.CreateMoqTable).ToList(); 
		} 

		/// <summary>
		/// Gets whether an Exception has been thrown
		/// </summary>
		public bool ErrorsOccurred { get; set; } = false;
	}
}
