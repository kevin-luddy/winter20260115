// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Common
{
    using System;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Mvc;
    using IES.Common;
    using IES.Common.Exceptions;

    /// <summary>
    /// General utilities
    /// </summary>
    public class SiteMasterUtilities
    {
        /// <summary>
        /// Logger
        /// </summary>
        private static IES.Common.Logger log = new IES.Common.Logger(typeof(SiteMasterUtilities));

        /// <summary>
        /// Create static Regex object for Index.
        /// </summary>
        private static Regex regexIndex = new Regex(@"(\[\d+\])(?!.*(\[\d+\]))");

        /// <summary>
        /// Create static Regex object for Element.
        /// </summary>
        private static Regex regexElement = new Regex(@"[^\.]+(\[\d+\])");

        /// <summary>
        /// Determines if the current action matches the expected action
        /// </summary>
        /// <param name="context">Controller context</param>
        /// <param name="controllerNameToMatch">Name of the controller</param>
        /// <returns>True, if match; false if not</returns>
        public static bool CurrentControllerMatches(ControllerContext context, string controllerNameToMatch)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (controllerNameToMatch == null)
            {
                throw new ArgumentNullException(nameof(controllerNameToMatch));
            }

            return context.RouteData.Values["controller"].ToString().IsEquivalentTo(controllerNameToMatch);
        }

        /// <summary>
        /// Creates a string containing all of the errors in the Model State
        /// </summary>
        /// <param name="modelStateDictionary">The ModelState being checked</param>
        /// <returns>A string of errors</returns>
        public string CreateValidationErrorResponse(ModelStateDictionary modelStateDictionary)
        {
            if (modelStateDictionary == null)
            {
                throw new ArgumentNullException(nameof(modelStateDictionary));
            }

            StringBuilder errors = new StringBuilder();
            foreach (ModelState state in modelStateDictionary.Values)
            {
                foreach (ModelError error in state.Errors)
                {
                    if (!string.IsNullOrEmpty(error.ErrorMessage))
                    {
                        errors.AppendLine(error.ErrorMessage);
                    }
                    else
                    {
                        errors.AppendLine("An exception occurred during model creation.  Please check your data, and try again");
                        log.Error(error.Exception, "An exception occurred during model creation.");
                    }
                }
            }

            return errors.ToString();
        }

        /// <summary>
        /// Creates a string containing all of the errors in the Model State
        /// </summary>
        /// <param name="modelStateDictionary">The ModelState being checked</param>
        /// <returns>A string of errors</returns>
        public static Collection<ValidationMessage> CreateModelStateValidationErrorList(ModelStateDictionary modelStateDictionary)
        {
            if (modelStateDictionary == null)
            {
                throw new ArgumentNullException(nameof(modelStateDictionary));
            }

			Collection<ValidationMessage> errors = new Collection<ValidationMessage>();

            foreach (System.Collections.Generic.KeyValuePair<string, ModelState> state in modelStateDictionary)
            {
                foreach (ModelError error in state.Value.Errors)
                {
                    ValidationMessage message = null;

                    if (!string.IsNullOrEmpty(error.ErrorMessage))
                    {
                        message = new ValidationMessage(state.Key, error.ErrorMessage);
                    }
                    else
                    {
                        message = new ValidationMessage(state.Key, "Field was not valid.");
                    }

                    Match indexMatch = regexIndex.Match(state.Key);

                    if (indexMatch.Success)
                    {
                        message.RowIndex = Convert.ToInt32(indexMatch.Value.TrimStart('[').TrimEnd(']'));

                        Match elementMatch = regexElement.Match(state.Key);
                        if (elementMatch.Success)
                        {
                            message.IndexedItem = elementMatch.Value.Split('[')[0];
                        }
                    }

                    errors.Add(message);
                }
            }

            // remove duplicate messages.
            errors = new Collection<ValidationMessage>(errors.GroupBy(x => x.ValidationIssue).Select(x => x.First()).ToArray());

            return errors;
        }

        /// <summary>
        /// This function will take a collection of user Ids and converts them
        /// to a list the view can read
        /// </summary>
        /// <param name="collection">Data collection</param>
        /// <returns>JS string representation of collection data</returns>
        public static string ConvertCollectionToJavascriptString(Collection<int> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (collection.Any())
            {
                StringBuilder input = new StringBuilder();
                foreach (int x in collection)
                {
                    input.Append("'");
                    input.Append(x.ToString().Trim());
                    input.Append("'");
                    input.Append(",");
                }

                // Trim the trailing comma
                input = input.Remove(input.Length - 1, 1);

                return input.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// This method will set the Proposal in use in the current http context for use by Views
        /// </summary>
        /// <param name="proposalId">Id of the Proposal</param>
        public void SetProposalContext(int proposalId)
        {
            if (HttpContext.Current.Items[WebConstants.RequestKey.PROPOSAL_ID] == null ||
                !proposalId.Equals(HttpContext.Current.Items[WebConstants.RequestKey.PROPOSAL_ID]))
            {
                HttpContext.Current.Items[WebConstants.RequestKey.PROPOSAL_ID] = proposalId;
            }
        }

        /// <summary>
        /// Returns true/false indicating whether the piwik should be disabled. This is used for classified installations.
        /// </summary>
        /// <returns>Bool whether the links should be shut off or not</returns>
        public static bool DisablePiwik()
        {
            bool result = false;

            if (!string.IsNullOrEmpty(IES.Common.ConfigurationUtilities.GetAppSetting("DisablePiwik"))
                && IES.Common.ConfigurationUtilities.GetAppSetting("DisablePiwik").ToLower().Equals("true"))
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Gets the Piwik URL from web.config
        /// </summary>
        /// <returns>Piwik Url</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
        public static string PiwikUrl()
        {
            return IES.Common.ConfigurationUtilities.GetAppSetting("PiwikURL");
        }

        /// <summary>
        /// Returns PiwikId from the web.config; if it doesn't exist -> returns 0
        /// </summary>
        /// <returns>Piwik Id for the site</returns>
        public static int PiwikId()
        {
            int result;

            if (!int.TryParse(IES.Common.ConfigurationUtilities.GetAppSetting("PiwikId"), out result))
            {
                result = 0;
            }

            return result;
        }

        /// <summary>
        /// Read-only property that caches PSA allowed file types from web.config
        /// </summary>
        public static string AllowedFileTypes
        {
            get
            {
                return allowedFileTypes ?? (allowedFileTypes = ConfigurationManager.AppSettings["PSAAllowedFileTypes"]);
            }
        }

        /// <summary>
        /// Read-only property that caches PSA allowed file types from web.config
        /// </summary>
        private static string allowedFileTypes;

        /// <summary>
        /// Read-only property that caches PSA max file size
        /// </summary>
        public static int? MaxFileSize
        {
            get
            {
                return maxFileSize ?? (maxFileSize = int.Parse(ConfigurationManager.AppSettings["PSAMaxFileSize"]));
            }
        }

        /// <summary>
        /// Read-only property that caches PSA max file size
        /// </summary>
        private static int? maxFileSize;

        /// <summary>
        /// Read-only property that caches PSA max file count.
        /// </summary>
        public static int? MaxOtherFileCount
        {
            get
            {
                return maxOtherFileCount ?? (maxOtherFileCount = int.Parse(ConfigurationManager.AppSettings["PSAMaxOtherFileCount"]));
            }
        }

        /// <summary>
        /// Read-only property that caches PSA max other file count.
        /// </summary>
        private static int? maxOtherFileCount;

        /// <summary>
        /// Read-only property that caches the Base Url where the checklist instruction files are located (from web.config)
        /// </summary>
        public static string BaseUrlForInstructionLocation
        {
            get
            {
                return ConfigurationManager.AppSettings["BaseUrlForInstructionLocation"] ?? string.Empty;
            }
        }

        /// <summary>
        /// Are we in a classified environment?
        /// </summary>
        public static bool IsClassEnvironment
        {
            get
            {   
                return bool.TrueString.ToLower() == ConfigurationManager.AppSettings["IsClassEnvironment"].ToLower();
            }
        }

		/// <summary>
		/// Is EPP Integration Enabled?
		/// </summary>
		public static bool IsEPPIntegrationEnabled
		{
			get => bool.TrueString.ToLower() == ConfigurationManager.AppSettings["EnableEppIntegration"].ToLower();
		}
    }
}