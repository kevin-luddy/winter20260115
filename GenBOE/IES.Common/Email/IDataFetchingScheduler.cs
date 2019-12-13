// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;

    public interface IDataFetchingScheduler
    {
        /// <summary>
        /// This will thread a specific email
        /// </summary>
        /// <param name="inEmail">The email.</param>
        /// <param name="inEmailParameters">The email parameters.</param>
        void FetchEmails(Delegate inEmail, object[] inEmailParameters);
    }
}
