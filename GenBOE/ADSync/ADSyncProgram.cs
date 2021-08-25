// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using GenTRAC.DataBridge.DTO;
using IES.Common;
using IES.Common.classes;

namespace GenBOE.ADSync
{
    class ADSyncProgram
    {
        static readonly Logger logger = new Logger(typeof(ADSyncProgram));

        static void Main()
        {
            SyncBoeUsers();
            if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
            {
                SyncPtmUsers();
            }
        }

        private static void SyncPtmUsers()
        {
            try
            {
                ActiveDirectoryUtilities adUtils = new ActiveDirectoryUtilities();
                MemoryCache cache = new MemoryCache();
                UserMapper userMapper = new UserMapper(new UserLoader(adUtils), new NonCacheDataLoader(), new SecurityInformation(adUtils, cache), adUtils, cache);
                GenTRAC.ActionLogic.Synchronization.ActiveDirectorySynchronization adSync = new GenTRAC.ActionLogic.Synchronization.ActiveDirectorySynchronization(
                    adUtils, userMapper, new GenTRAC.ActionLogic.Mediator.UserMediator(userMapper));
                logger.Info("Beginning PTM Active Directory Sync.");

                Stopwatch timespent = new Stopwatch();
                timespent.Start();

                // Perform Work
                List<GenTRAC.DataBridge.DTO.UserDTO> updatedUsers = new List<UserDTO>(adSync.SyncUpdateUsers());
                timespent.Stop();

                StringBuilder toWrite = new StringBuilder();
                // Write to the stringbuilder once we know the transaction worked
                if (updatedUsers.Any())
                {
                    toWrite.AppendLine("The following " + updatedUsers.Count + " PTM users were updated:");

                    foreach (UserDTO updatedUser in updatedUsers)
                    {
                        toWrite.AppendLine("<br/>");
                        toWrite.AppendLine(updatedUser.Ntid);
                    }
                }
                else
                {
                    toWrite.AppendLine("No PTM users were updated.");
                }

                Console.WriteLine("Finished PTM Active Directory Sync.  Took " + timespent.Elapsed.TotalSeconds + " seconds.  Messages are as follows : " + toWrite.ToString());
                logger.Info("Finished PTM Active Directory Sync.  Took " + timespent.Elapsed.TotalSeconds + " seconds.  Messages are as follows : " + toWrite.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error syncing PTM Users.");
                logger.Error(ex, "Error syncing for PTM");
                throw;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static void SyncBoeUsers()
        {
            try
            {
                ActiveDirectoryUtilities adUtils = new ActiveDirectoryUtilities();
                GenBOE.ActionLogic.Synchronization.ActiveDirectorySynchronization adSync = new GenBOE.ActionLogic.Synchronization.ActiveDirectorySynchronization(
                    adUtils, new GenBOE.DataBridge.DTO.UserDTODataLoader(new SecurityInformation(adUtils, new MemoryCache()), adUtils));

                logger.Info("Beginning GenBOE Active Directory Sync for company: " + SystemConfiguration.Instance().CompanyMode.GetDescription());

                Stopwatch timespent = new Stopwatch();
                timespent.Start();
                
                // Perform Work
                List<GenBOE.Dtos.UserDTO> updatedUsers = new List<GenBOE.Dtos.UserDTO>(adSync.SyncUpdateUsers());
                timespent.Stop();

                StringBuilder toWrite = new StringBuilder();       
                // Write to the stringbuilder once we know the transaction worked
                if (updatedUsers.Any())
                {
                    toWrite.AppendLine("The following " + updatedUsers.Count + " BOE users were updated:");

                    foreach (Dtos.UserDTO updatedUser in updatedUsers)
                    {
                        toWrite.AppendLine("<br/>");
                        toWrite.AppendLine(updatedUser.NTID);
                    }
                }
                else
                {
                    toWrite.AppendLine("No BOE users were updated.");
                }

                Console.WriteLine("Finished GenBOE Active Directory Sync.  Took " + timespent.Elapsed.TotalSeconds + " seconds.  Messages are as follows : " + toWrite.ToString());
                logger.Info("Finished GenBOE Active Directory Sync.  Took " + timespent.Elapsed.TotalSeconds + " seconds.  Messages are as follows : " + toWrite.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error syncing for BOE for company mode: " + SystemConfiguration.Instance().CompanyMode.GetDescription());
                Console.WriteLine(ex.ToString());
                logger.Error(ex, "Error syncing for BOE for company mode: " + SystemConfiguration.Instance().CompanyMode.GetDescription());
            }
        }
    }
}
