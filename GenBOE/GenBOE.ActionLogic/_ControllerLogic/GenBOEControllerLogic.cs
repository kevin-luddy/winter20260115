// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.Exceptions;

    /// <summary>
    /// Controller logic class for the base controller
    /// </summary>
    public class GenBOEControllerLogic : IGenBOEControllerLogic
    {
        private Logger _log = new Logger(typeof(GenBOEControllerLogic));

        /// <summary>
        /// Validates the RTE Answers
        /// </summary>
        /// <param name="answers">The answers to validate.</param>
        /// <param name="sources">The sources for RTE Templates.</param>
        /// <param name="rteSizeLimit">The RTE Size limit for the workspace if overridden.</param>
        /// <returns>Validation warnings.</returns>
        public ICollection<ValidationMessage> ValidateRteAnswers(ICollection<RTECustomTemplateQuestionAnswerModelView> answers, ICollection<RteCustomTemplateSourceModelView> sources, int? rteSizeLimit)
        {
            List<ValidationMessage> allValidationMessages = new List<ValidationMessage>();

            if (answers != null && answers.Any())
            {
                foreach (RTECustomTemplateQuestionAnswerModelView rteTemplateAnswer in answers)
                {
                    ICollection<ValidationMessage> scrubMessages = this.ScrubRichTextPropertiesForSave(rteTemplateAnswer);
                    if (scrubMessages.Any())
                    {
                        allValidationMessages.AddRange(scrubMessages);
                    }

                    RteCustomTemplateSourceModelView source = sources.First(s => s.SourceId == rteTemplateAnswer.SourceId);
                    if (string.IsNullOrEmpty(rteTemplateAnswer.AnswerText) && rteTemplateAnswer.Required)
                    {
                        allValidationMessages.Add(new ValidationMessage(source.Description, string.Format("Text is missing from required Custom RTE Template for {0}.", source.Description)));
                    }

                    if (rteSizeLimit.HasValue && !string.IsNullOrEmpty(rteTemplateAnswer.AnswerText) && rteTemplateAnswer.SourceId > 0 && rteSizeLimit < GenBOEUtilities.ConvertHtmlToText(rteTemplateAnswer.AnswerText).Length)
                    {
                        allValidationMessages.Add(new ValidationMessage(source.Description, string.Format("The maximum length of a Custom RTE Template text for {0} is {1} characters.", source.Description, rteSizeLimit.Value)));
                    }
                }
            }

            return allValidationMessages;
        }

		/// <summary>
		/// Validate the Skill Mix Table for any errors
		/// </summary>
		/// <param name="skillMixTable">The Skill Mix table, as a model view</param>
		/// <returns>A collection of any validation errors/messages</returns>
		public ICollection<ValidationMessage> ValidateSkillMixTable(ICollection<SkillMixModelView> skillMixTable)
		{
			List<ValidationMessage> validationMessages = new List<ValidationMessage>();

			if (skillMixTable != null && skillMixTable.Any())
			{
				foreach (SkillMixModelView skillMixRow in skillMixTable)
				{
					if (string.IsNullOrEmpty(skillMixRow.Rationale))
					{
						validationMessages.Add(new ValidationMessage(skillMixRow.ResourceNew, string.Format("Rationale is missing for {0}.", skillMixRow.ResourceNew)));
					}
					else
					{
						if (skillMixRow.Rationale.Length > 255)
						{
							validationMessages.Add(new ValidationMessage(skillMixRow.ResourceNew, string.Format("The maximum length of the Rationale field for {0} is {1} characters.", skillMixRow.ResourceNew, 255)));
						}
					}
				}
			}

			return validationMessages;
		}

        /// <summary>
        /// For any properties marked as rich-text, remove styling and/or markup that are known to cause problems
        /// for the third-party HTML conversion utility.
        /// </summary>
        /// <param name="obj">Object to be saved</param>
        /// <returns>List of validation messages, if any</returns>
        /// <seealso cref="IES.Common.RichTextAttribute"/>
        public ICollection<ValidationMessage> ScrubRichTextPropertiesForSave(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            List<ValidationMessage> allValidationMessages = new List<ValidationMessage>();

            bool skipRichTextScrub = ConfigurationUtilities.GetAppSetting<bool>("SkipRichTextScrub", false);

            if (!skipRichTextScrub)
            {
                PropertyInfo[] properties = obj.GetType().GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    RichTextAttribute[] richTextAttrs = (RichTextAttribute[])property.GetCustomAttributes(typeof(RichTextAttribute), false);
                    if (richTextAttrs.Any())  // if the property is marked as having rich-text
                    {
                        // get the current value of this property
                        string html = property.GetValue(obj) as string;

                        if (!string.IsNullOrEmpty(html))
                        {
                            // remove the "unsaveable" parts of the markup
                            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();

                            string scrubbed = this.DoAnExtraScrubbingForRTEInput(html);
                            scrubbed = GenBOEUtilities.ScrubRichTextForSave(scrubbed, validationMessages);

                            if (validationMessages.Any())
                            {
                                DisplayAttribute displayAttribute = property.GetCustomAttribute(typeof(DisplayAttribute)) as DisplayAttribute;
                                string displayedPropertyName = (displayAttribute == null || string.IsNullOrEmpty(displayAttribute.Name)) ? property.Name : displayAttribute.Name;

                                foreach (ValidationMessage msg in validationMessages)
                                {
                                    // prepend the property name to each validation message
                                    msg.ValidationIssue = string.Format("{0}: {1}", displayedPropertyName, msg.ValidationIssue);
                                    msg.FieldName = property.Name;
                                }
                                allValidationMessages.AddRange(validationMessages);
                            }

                            // update the property value
                            property.SetValue(obj, scrubbed);
                        }
                    }
                }
            }

            return allValidationMessages;
        }

        /// <summary>
        /// Scrub rich-text markup to remove unwanted items and convert image tag src-attribute route values to base-64.
        /// Note that pasted images (that are not already stored) must be added to the editor in Base-64 format so they
        /// can be converted to binary and stored when the rich-text is saved.
        /// </summary>
        /// <param name="html">Rich-text markup</param>
        /// <param name="validationMessages">Repository for validation messages</param>
        /// <returns>Scrubbed rich-text</returns>
        public string ScrubRichTextForPaste(string html, ICollection<ValidationMessage> validationMessages)
        {
            string cleanedText = this.DoAnExtraScrubbingForRTEInput(html);

            return HtmlAgilityPackUtilities.ScrubRichTextForSave(cleanedText, this.ConvertImageSrcAttribute, validationMessages);
        }

        /// <summary>
        /// Convert a URL-based img-tag src attribute value to a base-64-based attribute value.
        /// </summary>
        /// <param name="src">Original src attribute value</param>
        /// <returns>Base-64 converted src attribute value</returns>
        private string ConvertImageSrcAttribute(string src)
        {
            string resultingSrc = src;

            Uri uri;

            // skip for blob, only do this for base64 encoded images
            if (!src.StartsWith("blob"))
            {
                if (src.StartsWith("data:"))
                {
                    resultingSrc = src.Replace(' ', '+');  // replace spaces with plus-sign to ensure valid base-64 syntax
                }
                else if (Uri.TryCreate(src, UriKind.RelativeOrAbsolute, out uri))
                {
                    try
                    {
                        // retrieve the URL-referenced image directly
                        byte[] imageBytes = GenImageUtilities.DownloadImage(uri, ConfigurationUtilities.GetAppSetting("LMIProxy"));

                        string base64String = GenImageUtilities.ConvertImageToBase64String(imageBytes);
                        string mimeType = GenImageUtilities.GetImageMIMETypeFromExtension(uri.AbsolutePath);

                        // e.g. "data:image/jpeg;base64,<<base64encodeddata>>"
                        resultingSrc = String.Format("data:{0};base64,{1}", mimeType, base64String);
                    }
                    catch (WebException webx)
                    {
                        this._log.Warn(string.Format("Could not retrieve image at \"{0}\".", src));

                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("Message = {0}", webx.Message);
                        sb.AppendFormat(", Status = {0}", webx.Status.ToString());
                        if (webx.Data != null)
                        {
                            foreach (DictionaryEntry entry in webx.Data)
                            {
                                sb.AppendFormat(", {0} = {1}", entry.Key.ToString(), entry.Value.ToString());
                            }
                        }

                        string msg = sb.ToString();

                        this._log.Error(webx, msg);
                    }
                }
            }

            return resultingSrc;
        }

        /// <summary>
        /// Does extra scrubbing in case HTML garbage made it through. it removes scripts and comments (stuff <script> ... </script> and <!-- ... -->
        /// </summary>
        /// <param name="originalScrubbedText">Original text to be cleaned</param>
        /// <returns>Cleaned text</returns>
        public string DoAnExtraScrubbingForRTEInput(string originalScrubbedText)
        {
            string result = originalScrubbedText;

            result = this.RemoveTextBasedOnStartAndEndString(result, "<script", "</script>");
            result = this.RemoveTextBasedOnStartAndEndString(result, "<!--", "-->");

            return result;
        }

        /// <summary>
        /// Removes text between starting and ending strings
        /// </summary>
        /// <param name="originalScrubbedText">original text to be cleaned</param>
        /// <param name="startingString">Starting string</param>
        /// <param name="endingString">Ending string</param>
        /// <returns>Cleaned string</returns>
        private string RemoveTextBasedOnStartAndEndString(string originalScrubbedText, string startingString, string endingString)
        {
            string result = originalScrubbedText;

            while (true)
            {
                int startIndex = result.ToUpper().IndexOf(startingString.ToUpper());

                if (startIndex == -1)
                {
                    break;
                }

                int endIndex = result.ToUpper().IndexOf(endingString.ToUpper(), startIndex);

                if (endIndex == -1)
                {
                    break;
                }

                result = result.Remove(startIndex, endIndex - startIndex + endingString.Length);
            }

            return result;
        }

        /// <summary>
        /// Determines if either the Escalation Rates or Fees/Cost Rates are out of date.
        /// </summary>
        /// <param name="workspaceId">The id of the workspace to check.</param>
        /// <returns></returns>
        public virtual bool AreZoneTravelRatesOutOfDate(int workspaceId)
        {
            return false;
        }

        /// <summary>
        /// Determines if the Offload Rates are out of date.
        /// </summary>
        /// <param name="workspaceId">The id of the workspace to check.</param>
        /// <returns></returns>
        public virtual bool AreOffloadRatesOutOfDate(int workspaceId)
        {
            return false;
        }
    }
}