// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IES.Common.Core;
	using IES.Common.Core.Loaders;
	using IES.Common.Core.Utilities;
	using IES.DataBridge.ModelViews;
    using IES.Models;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// Cobra Years Loader
	/// </summary>
    public class CobraYearsLoader : DataLoader<CobraYearGridModelView>, ICobraYearsLoader
    {
		/// <summary>
		/// default ctor
		/// </summary>
		/// <param name="logger">logger</param>
		public CobraYearsLoader(ILogger<CobraYearsLoader> logger) : base(logger)
		{ }

		#region Retrieves

		/// <summary>
		/// Get All Cobra Year Configuration Data
		/// </summary>
		/// <returns>All Cobra Year Data</returns>
		public ICollection<CobraYearGridModelView> GetAll()
        {
            ICollection<CobraYearGridModelView> result;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (IESEntities context = new IESEntities())
                {
                    result = context.CobraFiscalYearLUs.Select(e =>
                        new CobraYearGridModelView()
                        {
                            Id = e.ID,
                            CobraDate = e.FiscalYearStartDate,
                            UpdateDate = e.UpdateDate,
                            Year = e.Year
                        }).OrderBy(x => x.Year).ToList();
                }
            }

            // Add 12 hours so that people in other timezones see the same date
            Parallel.ForEach(result, r => r.CobraDate = r.CobraDate.AddHours(12));

            return result;
        }

        /// <summary>
        /// GetCobraExportRows
        /// </summary>
        /// <param name="id">Revision Id</param>
        /// <param name="startYear">Start Year</param>
        /// <returns>All COBRA rates for Start Year and beyond.</returns>
        public ICollection<CobraExportRowModelView> GetCobraExportRows(int id, int startYear)
        {
            ICollection<CobraExportRowModelView> cobraRates;
            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (IESEntities context = new IESEntities())
                {
                    // navigation property not available for RateCodeYears so joins created in query below
                    cobraRates = (from ry in context.RateCodeYears
                              join r in context.RateCodes on ry.RateCodeID equals r.ID
                              join c in context.CobraCode1LU on r.CobraCode1ID equals c.ID
                              join cy in context.CobraFiscalYearLUs on ry.Year equals cy.Year
                              where
                              (r.RevisionID == id && !string.IsNullOrEmpty(r.CobraRateSet) && !string.IsNullOrEmpty(c.Description) && ry.Year >= startYear)
                              orderby r.CobraRateSet, r.RateCode1, cy.Year
                              select new CobraExportRowModelView()
                              {
                                  RateSet = r.CobraRateSet,
                                  Code1 = c.Description,
                                  RateCode = r.RateCode1,
                                  Description = r.Description,
                                  Date = cy.FiscalYearStartDate,
                                  Value = ry.Rate
                              }).ToCollection();
                }
            }

            return cobraRates;
        }

        /// <summary>
        /// Gets all objects based on the IDs that were passed in
        /// </summary>
        /// <param name="ids">IDs</param>
        /// <returns>
        /// Corresponding Data
        /// </returns>
        public override ICollection<CobraYearGridModelView> GetByIds(ICollection<int> ids)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Commits
        /// <summary>
        /// Upsert
        /// </summary>
        /// <param name="dtoToUpsert">Dto that is upserted</param>
        /// <returns>Id of the dto after the modification</returns>
        protected override int? Upsert(CobraYearGridModelView dtoToUpsert)
        {
            if (dtoToUpsert == null)
            {
                throw new ArgumentNullException(nameof(dtoToUpsert));
            }

            int? result;

            using (IESEntities iesEntities = new IESEntities())
            {
                result = iesEntities.upsertCobraFiscalYear(dtoToUpsert.Id, dtoToUpsert.UpdateDate, dtoToUpsert.Year, dtoToUpsert.CobraDate).First();
            }

            return result;
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="dtoToDelete">Dto that is deleted</param>
        /// <returns>Id of the deleted dto</returns>
        protected override int? Delete(CobraYearGridModelView dtoToDelete)
        {
            if (dtoToDelete == null)
            {
                throw new ArgumentNullException(nameof(dtoToDelete));
            }

            int? toReturn;

            using (IESEntities iesEntities = new IESEntities())
            {
                toReturn = iesEntities.deleteCobraFiscalYear(dtoToDelete.Id, dtoToDelete.UpdateDate);
            }

            return toReturn;
        }

        #endregion
    }
} 