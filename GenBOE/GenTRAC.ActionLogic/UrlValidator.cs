// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    /// <summary>
    /// Class to help validate URLs
    /// </summary>
    public static class UrlValidator
    {
        /// <summary>
        /// Tests the string to make sure it's a valid link, it's absolute and within lmco.com
        /// </summary>
        /// <param name="linkTextToTest">Link to test</param>
        /// <returns>Valid or not</returns>
        public static bool IsAbsoluteLockheedUrl(string linkTextToTest)
        {
            if (string.IsNullOrEmpty(linkTextToTest))
            {
                return false;
            }

            bool result = false;

            Uri urlToCheck = null;

            if (Uri.TryCreate(linkTextToTest, UriKind.Absolute, out urlToCheck))
            {
                if (urlToCheck.Host.ToLower().EndsWith(".lmco.com"))
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Scrapes the target page, verifying that it exists
        /// </summary>
        /// <param name="linkToCheck">Link to check</param>
        /// <returns>Does link point to an actual page</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
        public static bool DoesPageExist(string linkToCheck)
        {
            bool result = false;

            // This sometimes fails after 30 min... putting it inside a thread so that it will fail after 30 seconds instead.
            Task task = Task.Factory.StartNew(() =>
            {
                try
                {
                    HttpWebRequest request = HttpWebRequest.Create(linkToCheck) as HttpWebRequest;
                    request.UseDefaultCredentials = true; // user user's credentials

                    request.GetResponse();

                    // If we got here, a response exists. It would have thrown an exception if it didn't
                    result = true;
                }
                catch (WebException ex)
                {
                    if (ex.Message.Contains("401") || ex.Message.Contains("403"))
                    {
                        // not authorized, but the link works, so we'll allow it
                        result = true;
                    }
                }
            });

            // only give the task 30 seconds to complete.
            task.Wait(30000);

            return result;
        }
    }
}
