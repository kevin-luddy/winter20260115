// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
	using GenBOE.ActionLogic.IO.Import;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using IES.Common;
	using System;
	using System.Collections.ObjectModel;

	public class ImportWbsResultsModelView
	{
		public ImportWbsResultsModelView()
		{
			ClinID = -1;
			WbsID = -1;
			ClinIDs = new Collection<int>();

		}

		/// <summary>
		/// Creates a new WBS DTO to upsert from the data in this ModelView
		/// </summary>
		/// <returns>A DTO with data from this ModelView</returns>
		public WbsDTO GetWBSDTO()
		{
			if (ImportType != (int)WbsImportResult.CreateWbs && ImportType != (int)WbsImportResult.UpdateWbs)
			{
				throw new ArgumentException("ImportType must be 'CreateWbs' or 'UpdateWbs' to convert this ModelView to a WBS DTO");
			}

			return new WbsDTO()
			{
				Id = WbsID,
				WbsNumber = WbsNumber,
				WbsTitle = WbsTitle,
				ClinIDs = ClinIDs,
				Updateable = (ImportType == (int)WbsImportResult.CreateWbs || ImportType == (int)WbsImportResult.UpdateWbs) ? UpdateType.Upsert : UpdateType.None
			};
		}

		/// <summary>
		/// Takes an existing WBS DTO and updates its non-ID properties based on the data in this ModelView
		/// </summary>
		/// <param name="oldWbs">The old WBS whose ID will be retained</param>
		/// <returns>A DTO with non-ID data from this ModelView</returns>
		public WbsDTO GetWBSDTO(WbsDTO oldWbs)
		{
			if (oldWbs == null)
			{
				throw new ArgumentNullException(nameof(oldWbs));
			}

			if (ImportType != (int)WbsImportResult.CreateWbs && ImportType != (int)WbsImportResult.UpdateWbs)
			{
				throw new ArgumentException("ImportType must be 'CreateWbs' or 'UpdateWbs' to convert this ModelView to a WBS DTO");
			}

			oldWbs.WbsNumber = WbsNumber;
			oldWbs.WbsTitle = WbsTitle;
			oldWbs.ClinIDs = ClinIDs;
			oldWbs.Updateable = (ImportType == (int)WbsImportResult.CreateWbs || ImportType == (int)WbsImportResult.UpdateWbs) ? UpdateType.Upsert : UpdateType.None;

			return oldWbs;
		}

		/// <summary>
		/// Creates a new BOE DTO to upsert from the data in this ModelView
		/// </summary>
		/// <returns>A DTO with data from this ModelView</returns>
		public BoeDTO GetBOEDTO()
		{
			if (ImportType != (int)WbsImportResult.CreateBoe)
			{
				throw new ArgumentException("ImportType must be CreateBoe to convert this ModelView to a BOE DTO");
			}

			return new BoeDTO()
			{
				WBSID = WbsID,
				CLINID = ClinID > 0 ? (int?)ClinID : null,
				Updateable = (ImportType == (int)WbsImportResult.CreateBoe) ? UpdateType.Upsert : UpdateType.None
			};
		}

		public int WbsID { get; set; }
		public string WbsNumber { get; set; }
		public string WbsTitle { get; set; }
		public int ClinID { get; set; }
		public string ClinNumber { get; set; }
		public string ClinTitle { get; set; }

		// This needs to stay an "int" and not a "ImportResult" in order to translate correctly during model binding.
		public int ImportType { get; set; }

		public Collection<int> ClinIDs { get; set; }
	}
}
