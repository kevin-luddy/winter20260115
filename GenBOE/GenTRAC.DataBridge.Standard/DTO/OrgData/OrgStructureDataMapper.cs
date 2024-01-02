// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using IES.Standard;
    using IES.Standard.PickList;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// OrgStructureDataLoader
	/// </summary>
	public class OrgStructureDataMapper : IOrgStructureDataMapper
    {
        // Getter and Setters
        #region GetterSetters

        /// <summary>
        /// Logger
        /// </summary>
        protected ILogger Log { get; private set; }

        /// <summary>
        /// ILineOfBusinessDataLoader
        /// </summary>
        protected IPickListLoader LineOfBusinessLoader { get; set; }

        /// <summary>
        /// IProgramAreaDataLoader
        /// </summary>
        protected IPickListLoader ProgramAreaLoader { get; set; }

        #endregion GetterSetters

        /// <summary>
        /// OrgStructureDataMapper
        /// </summary>
        /// <param name="inLineOfBusinessDataLoader">LineOfBusiness DataLoader</param>
        /// <param name="inProgramAreaDataLoader">Program Area DataLoader</param>
        public OrgStructureDataMapper(IPickListLoader inLineOfBusinessDataLoader,
                                      IPickListLoader inProgramAreaDataLoader,
									  ILogger<OrgStructureDataMapper> logger)
		{
			this.Log = logger;
			this.LineOfBusinessLoader = inLineOfBusinessDataLoader;
            this.ProgramAreaLoader = inProgramAreaDataLoader;
        }

        // Line of Business Routines
        #region LineOfBusiness Routines

        /// <summary>
        ///  Get All Lines of Business
        /// </summary>
        /// <returns>Collection of LineOfBusiness</returns>
        virtual public ICollection<PickListDto> GetAllLinesOfBusiness()
        {
            ICollection<PickListDto> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("OrgStructureDataLoader.GetAllLineOfBusiness", this.Log))
            {
                toReturn = this.LineOfBusinessLoader.GetPickListValues();
            }

            return toReturn;
        }

        /// <summary>
        /// Get by Id
        /// </summary>
        /// <param name="id">theLine of Business Id</param>
        /// <returns>LineOfBusinessDTO with data</returns>
        virtual public PickListDto GetLineOfBusinessById(int id)
        {
            return this.LineOfBusinessLoader.GetById(id);
        }

        /// <summary>
        /// Get Lines of Business by IDs
        /// </summary>
        /// <param name="ids">Collection of Lines of Business</param>
        /// <returns>Collection of Line of Business Dtos</returns>
        virtual public ICollection<PickListDto> GetLineOfBusinessById(ICollection<int> ids)
        {
            return this.LineOfBusinessLoader.GetByIds(ids);
        }

        #endregion LineOfBusiness Routines

        // Program Area Routines
        #region ProgramArea Routines

        /// <summary>
        ///  Get All ProgramArea
        /// </summary>
        /// <returns>Collection of ProgramAreas</returns>
        virtual public ICollection<PickListDto> GetAllProgramAreas()
        {
            ICollection<PickListDto> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("OrgStructureDataLoader.GetAllProgramAreas", this.Log))
            {
                toReturn = this.ProgramAreaLoader.GetPickListValues();
            }

            return toReturn;
        }

        /// <summary>
        /// Get by Id
        /// </summary>
        /// <param name="id">the Program AreaId</param>
        /// <returns>The ProgramAreaDTO</returns>
        virtual public PickListDto GetProgramAreaById(int id)
        {
            return this.ProgramAreaLoader.GetById(id);
        }

        /// <summary>
        /// Get Program Areas by IDs
        /// </summary>
        /// <param name="ids">Collection of ProgramAreas</param>
        /// <returns>Collection of ProgramArea Dtos</returns>
        virtual public ICollection<PickListDto> GetProgramAreaById(ICollection<int> ids)
        {
            return this.ProgramAreaLoader.GetByIds(ids);
        }

        /// <summary>
        /// Return available Program Areas as HTML select items for selected line of business
        /// </summary>
        /// <param name="lineOfBusinessId">Line of Business to filter</param>
        /// <param name="programAreaId">An optional Program Area Id</param>
        /// <param name="includeDefaultSelect">Should the "Select Program Area" option be included</param>
        /// <returns>Program Areas</returns>
        public string GetProgramAreaHtmlOptionsForLineOfBusiness(int? lineOfBusinessId, int? programAreaId = null, bool includeDefaultSelect = true)
        {
            StringBuilder selectList = new StringBuilder();

            if (includeDefaultSelect)
            {
                selectList.Append("<option value=\"\">Select Program Area</option>");
            }

            ICollection<PickListDto> programAreas = this.GetAllProgramAreas().Where(x => (((!lineOfBusinessId.HasValue || x.ParentIds.Contains(lineOfBusinessId.Value)) && x.IsActive) || programAreaId.HasValue && x.Id == programAreaId)).OrderBy(x => x.Text).ToList();

            foreach (PickListDto programArea in programAreas)
            {
                selectList.Append(string.Format("<option value=\"{0}\">{1}</option>", programArea.Id, programArea.Text));
            }
            
            return selectList.ToString();
        }

        /// <summary>
        /// Create the dynamic Program Area help text for active and inactive Program Areas
        /// </summary>
        /// <returns>String to display as help text for the Program Area field</returns>
        virtual public string GetProgramAreaDynamicHelpText()
        {
            ICollection<PickListDto> linesOfBusiness = this.GetAllLinesOfBusiness();
            ICollection<PickListDto> programAreas = this.GetAllProgramAreas();

            ICollection<PickListDto> activeProgramAreas = (from x in programAreas
                                                         where x.IsActive
                                                         select x).OrderBy(x => x.Text).ToList();

            ICollection<PickListDto> inactiveProgramAreas = (from x in programAreas
                                                           where !x.IsActive
                                                           select x).OrderBy(x => x.Text).ToList();

            ICollection<string> activeProgramAreaStrings = (from x in activeProgramAreas
                                                    select string.Format("{0} ({1})", x.Text,
                                                    linesOfBusiness.FirstOrDefault(y => x.ParentIds != null && x.ParentIds.Count == 1 && y.Id == x.ParentIds.First()).Text)).ToList();

            ICollection<string> inactiveProgramAreaStrings = (from x in inactiveProgramAreas
                                                      select string.Format("{0} ({1})", x.Text,
                                                      linesOfBusiness.FirstOrDefault(y => x.ParentIds != null && x.ParentIds.Count == 1 && y.Id == x.ParentIds.First()).Text)).ToList();

            string toReturn = string.Format("<b><u>Active Program Areas:</u></b><br />{0}<br /><br /><b><u>Inactive Program Areas:</u></b><br />{1}",
                string.Join("<br />", activeProgramAreaStrings), string.Join("<br />", inactiveProgramAreaStrings));

            return toReturn;
        }

        #endregion ProgramArea Routines
    }
}
