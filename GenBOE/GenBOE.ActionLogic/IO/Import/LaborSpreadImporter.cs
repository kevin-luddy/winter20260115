// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;

    public enum LaborSpreadImportResult
    {
        UpdateSpread = 0,
        ChangeTypeToDiscrete = 1,
        NoSpreadDate = 2,
        InvalidSpreadDateFormat = 3,
        InvalidSpreadValue = 4,
        SpreadDateOutsideOfLaborTypeDateRange = 5,
        MissingResources = 6,
        MissingStartDate = 7,
        MissingEndDate = 8,
        StartDateChanged = 9,
        EndDateChanged = 10,
        ResourceHourValueTooLarge = 11,
        ResourceCostValueTooLarge = 12,
        HoursSpreadRangeInvalid = 13,
        CostSpreadRangeInvalid = 14,
        DiscreteWhereCurveExpected = 15,
        DiscreteEntriesPastValidDate = 16,
        CreateSpread = 17,
        DecimalPrecisionViolation = 18,
        CostDecimalPrecisionViolation = 19
    }

    [ExcludeFromCodeCoverage]
    public class ImportedLaborSpread : ResourceSpreadDto
    {
        public ImportedLaborSpread()
        {
            this.Updateable = UpdateType.Upsert;
            this.ImportTypes = new Collection<LaborSpreadImportResult>();
        }

        public ImportedLaborSpread(
            LaborSpreadImportResult inImportResult)
            : this()
        {
            this.ImportTypes.Add(inImportResult);
        }

        public ImportedLaborSpread(
            ResourceTypeDto inBOELaborType,
            IResourceDTODataLoader inResourceDTODataLoader,
            IPerformingOrgDTODataLoader perfOrgLoader,
            LaborSpreadImportResult inImportResult)
            : this(inImportResult)
        {
            if (inResourceDTODataLoader == null)
            {
                throw new ArgumentNullException(nameof(inResourceDTODataLoader));
            }

            if (perfOrgLoader == null)
            {
                throw new ArgumentNullException(nameof(perfOrgLoader));
            }

            if (inBOELaborType != null)
            {
                this.LaborTypeIDForImport = inBOELaborType.Id;
                this.BoeID = inBOELaborType.BoeID;
            }
        }

        public ImportedLaborSpread(
            ResourceSpreadDto inBoeLaborSpread,
            ResourceTypeDto inBOELaborType,
            IResourceDTODataLoader inResourceDTODataLoader,
            IPerformingOrgDTODataLoader perfOrgLoader,
            LaborSpreadImportResult inImportResult)
            : this(inBOELaborType, inResourceDTODataLoader, perfOrgLoader, inImportResult)
        {
            if (inBoeLaborSpread != null)
            {
                this.UpdateDate = inBoeLaborSpread.UpdateDate;
                this.Id = inBoeLaborSpread.Id;
                this.LaborSpreadDate = inBoeLaborSpread.LaborSpreadDate.Normalize();
                this.LaborSpreadValue = inBoeLaborSpread.LaborSpreadValue;
            }
        }

        public Collection<LaborSpreadImportResult> ImportTypes { get; set; }
        
        /// <summary>
        /// Labor Type id that is used exclusively for import. This name must be difference from parents 'LaborTypeId' or the default binder complains.
        /// </summary>
        public int? LaborTypeIDForImport { get; set; }
        
        public string LaborSpreadDateFormatted
        {
            get
            {
                return this.LaborSpreadDate.ToString("MM/yyyy");
            }
        }
    }
}