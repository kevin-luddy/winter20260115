// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.LoadGeeps
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenTRAC.Models;
    using IES.Common;

    public class DataMartEmployeeDTOLoader : BulkDataLoader<DataMartEmployeeDTO, DataMartEmployee>
    {
        /// <summary>
        /// Provides the metadata to support bulk save processing for DataMart Employees
        /// </summary>
        /// <exception cref=NotImplementedException>Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
        /// <returns>Meta data required for bulk save processing</returns>
        public override BulkSaveMetaData CreateBulkSaveMetaData()
        {
            BulkSaveMetaData metaData = new BulkSaveMetaData(Constants.PTM_DB_CONTEXT_NAME);

            metaData.BulkInsertStoredProcedureName = "insertDataMartEmployees";

            // only insert is performed using the employee entity.
            metaData.BulkUpdateStoredProcedureName = null;
            metaData.BulkDeleteStoredProcedureName = null;

            metaData.BulkInsertStoredProcedureReturnsUpdateDate = false;
            metaData.BulkUpdateStoredProcedureReturnsUpdateDate = false;

            metaData.DBTableTypeName = "TT_DatamartEmployee";
            metaData.StoredProcedureTableTypeParameterName = "@Employees";

            metaData.EntityPropertiesToMapToDataTable = new Collection<string>()
            {
                "empl_ser_no", "empl_first_nm", "empl_last_nm", "nt_domain_nm", "nt_account_nm", "rpt_to_ser_no"
            };

            return metaData;
        }

        public override ICollection<DataMartEmployeeDTO> GetByIds(ICollection<int> ids)
        {
            throw new NotImplementedException();
        }

        protected override DataMartEmployee ConvertDtoToEntity(DataMartEmployeeDTO dtoToConvert)
        {
            return dtoToConvert;
        }

        protected override int? Delete(DataMartEmployeeDTO dtoToDelete)
        {
            throw new NotImplementedException();
        }

        protected override int? Upsert(DataMartEmployeeDTO dtoToUpsert)
        {
            throw new NotImplementedException();
        }
    }
}
