// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;
    using System.Collections.Generic;
    using System.Transactions;
    using IES.Common.Exceptions;

    /// <summary>
    /// Data Loader base class
    /// </summary>
    public abstract class DataLoader<TDtoType> : ReadOnlyDataLoader<TDtoType>, IDataLoader<TDtoType>
        where TDtoType : IUpdateableDTO
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        protected DataLoader()
        {
            this.Log = new Logger(typeof(DataLoader<TDtoType>));
        }

        /// <summary>
        /// Verifies that the upserted ID is correct.
        /// </summary>
        /// <param name="originalId">Original Item ID.</param>
        /// <param name="resultId">Resulting Item ID.</param>
        /// <returns>Indicates whether validation passed.</returns>
        private bool VerifyIdAfterUpsert(int? originalId, int? resultId)
        {
            bool verificationPassed = true;

            // if we were inserting (ID was null or ID was < 0), we need to make sure that the returned id was higher than 0
            // if we were updating (ID not null and ID > 0), we need to make sure that the returned id is the same as original
            if ((originalId == null || originalId < 0)
                && (resultId == null || resultId < 0))
            {
                this.Log.Error(Constants.ERR_INSERT_FAILED_DUE_TO_ID);
                verificationPassed = false;
            }
            else if ((originalId != null && originalId > 0)
                && (resultId == null || resultId.Value != originalId.Value))
            {
                this.Log.Error(Constants.ERR_UPDATE_FAILED_DUE_TO_ID);
                verificationPassed = false;
            }

            return verificationPassed;
        }

        /// <summary>
        /// Saves the specified Dto into the database, either deleting or upserting it.
        /// </summary>
        /// <param name="dtoToSave">Dto to save.</param>
        /// <returns>Wbs Id for the saved item, or null if save failed.</returns>
        public virtual int? Save(TDtoType dtoToSave)
        {
            if (dtoToSave == null)
            {
                throw new ArgumentNullException(nameof(dtoToSave));
            }

            this.ValidateTransactionScope();

            int? result = null;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                if (dtoToSave.Updateable == UpdateType.Deleted)
                {
                    result = this.Delete(dtoToSave);
                }
                else if (dtoToSave.Updateable == UpdateType.Upsert)
                {
                    int originalId = dtoToSave.Id;
                    result = this.Upsert(dtoToSave);

                    if (!this.VerifyIdAfterUpsert(originalId, result))
                    {
                        throw new GeneralAppException(Constants.ERR_INSERT_FAILED_DUE_TO_ID);
                    }
                }
                else
                {
                    throw new ArgumentException("The dto did not specify the Updateable type.");
                }
            }

            return result;
        }

        /// <summary>
        /// Save method to process collection of dtos to save
        /// </summary>
        /// <param name="dtosToSave">collection of Dtos</param>
        /// <returns>Dictionary of saved object ids</returns>
        public virtual Dictionary<int, int> Save(ICollection<TDtoType> dtosToSave)
        {
            if (dtosToSave == null)
            {
                throw new ArgumentNullException(nameof(dtosToSave));
            }

            Dictionary<int, int> toReturn = new Dictionary<int, int>();

            foreach (TDtoType dto in dtosToSave)
            {
                int originalId = dto.Id;
                int? temp = this.Save(dto);

                if (temp.HasValue && dto.Updateable == UpdateType.Upsert)
                {
                    toReturn.Add(originalId, temp.Value);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Validates whether or not we're inside a transaction
        /// </summary>
        protected void ValidateTransactionScope()
        {
            if (Transaction.Current == null)
            {
                // Temporarily commenting out the exception and logging so genBOE Production does not have breaking changes.
                // We will comb the logs and find these to fix in future sprints
                // throw new GeneralAppException("A transaction was not supplied with the save, please create a bug report");
                this.Log.Error("A transaction was not supplied with the save, please create a bug report." + Environment.NewLine + Environment.StackTrace);
            }
        }

        /// <summary>
        /// Delete method to be overridden by the derived class.
        /// </summary>
        /// <param name="dtoToDelete">Dto that will be deleted.</param>
        /// <returns>Id of the deleted object</returns>
        protected abstract int? Delete(TDtoType dtoToDelete);

        /// <summary>
        /// Upsert method to be overridden by the derived class
        /// </summary>
        /// <param name="dtoToUpsert">Dto to upsert.</param>
        /// <returns>Int representing the id of the upserted item.</returns>
        protected abstract int? Upsert(TDtoType dtoToUpsert);
    }
}