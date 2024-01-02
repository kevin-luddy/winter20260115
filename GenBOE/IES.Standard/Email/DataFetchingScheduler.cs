// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace IES.Standard
{
    /// <summary>
    /// This class will asynchronously schedule threads to retrieve data from the DB and load it into cache.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DataFetchingScheduler : IDataFetchingScheduler
    {
        private readonly ILogger _log;
        
        // Delegates for methods that return data
        private delegate object DataDelegate();
        private delegate object DataDelegateBySingleID(int ID);
        private delegate object DataDelegateByIDCollection(Collection<int> IDs);

        // Delegates for methods that return void
        private delegate void VoidDelegate();
        private delegate void VoidDelegateBySingleID(int ID);
        private delegate void VoidDelegateByIDCollection(Collection<int> IDs);

        public DataFetchingScheduler(ILogger logger)
        {
            this._log = logger;
        }

        /// <summary>
        /// This will thread a specific email
        /// </summary>
        /// <param name="inEmail">email delegate to send</param>
        /// <param name="inEmailParameters">email to send parameters</param>
        public void FetchEmails(Delegate inEmail, object[] inEmailParameters)
        {
            _FetchData(inEmail, inEmailParameters);
        }
        
        /// <summary>
        /// Queues the method onto the threadpool for execution when a thread is free.
        /// </summary>
        /// <param name="inLoaderMethod">Delegate method</param>
        /// <param name="inLoadMethodParams">Params for the delegate method</param>
        private void _FetchData(Delegate inLoaderMethod, object[] inLoadMethodParams)
        {
            // GRK - 9/8/2011
            // This needs to be unsafe or else the context is destroyed if the parent controller action returns first, 
            // causing errors in the logger when trying to retrieve the current user.  For more information see Jim or Geoff.

            // Queue it up!
            ThreadPool.UnsafeQueueUserWorkItem(
                delegate
                {
                    this.InvokeThread(inLoaderMethod, inLoadMethodParams);
                },
                new object());
        }
        /// <summary>
        /// Method that Invokes the LoaderMethod so we can wrap a try catch around the call incase something fails, this is so we can capture the exception and log it out. This will also stop
        /// IIS from failing and restarting
        /// </summary>
        /// <param name="inLoaderMethod">Delegate method</param>
        /// <param name="inLoadMethodParams">Params for the delegate method</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
       
        private void InvokeThread(Delegate inLoaderMethod, object[] inLoadMethodParams)
        {
            try
            {
                // Invoke the delegate with its params.
                inLoaderMethod.DynamicInvoke(inLoadMethodParams);
            }
            catch (Exception ex)
            {
                //Logging the information for now.
                _log.LogError(ex, "Exception was caught during threading for email.");
            }
        }
    }

}
