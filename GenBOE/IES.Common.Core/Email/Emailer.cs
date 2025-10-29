// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Email
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Linq;
	using System.Net.Mail;
	using System.Text.RegularExpressions;
	using IES.Common.Core;
	using IES.Common.Core.Configuration;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Models;
	using IES.Common.Core.Utilities;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// The Emailer class will send an email to the recipient and replace any tokens in the subject and body as required.
	/// </summary>
	public class Emailer : IEmailer
	{
		/// <summary>
		/// Protected Logger property
		/// </summary>
		protected ILogger Log { get; }

		/// <summary>
		/// The email address reg ex string.
		/// </summary>
		private const string EMAIL_ADDRESS_REG_EX = @"^([a-zA-Z0-9_\-\.\(\)]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

		/// <summary>
		/// The regex for an email address.
		/// </summary>
		private static readonly Regex RegexEmailAddress = new(EMAIL_ADDRESS_REG_EX, RegexOptions.None, CommonConstants.REGEX_TIMEOUT);

		/// <summary>
		/// Public ctor
		/// </summary>
		/// <param name="logger">logger</param>
		public Emailer(ILogger<Emailer> logger)
		{
			Log = logger;
		}

		/// <summary>
		/// Sends email messages based on parameters and uses Host
		/// 
		/// Validation Note:
		///   - The method will validate that the # of tokens in the subject/body are equal to the number of tokens sent in 
		///         (i.e. "Hi, {0}, I'm a message" should have 1 value in the *ReplaceTokens array)
		/// 
		/// Special Logic:
		///   - If the web.config contains a true value for "EmailsToCurrentlyLoggedInUser", 
		///     this method will attempt to locate the currently logged in users' email.
		///     
		/// General Note:
		///   Check your spam folder as it appears messages will go there by default!
		/// </summary>
		/// <param name="inEmailTypeToSend">The email to send</param>
		/// <param name="inRecipient">recipient(s) of the email (use comma between names if multiple)</param>
		/// <param name="inCClist">CC list for the email</param>
		/// <param name="inSubjectReplaceTokens">tokens in the subject email to replace, empty if none to replace</param>
		/// <param name="inBodyReplaceTokens">tokens in the body of the email to replace, empty if none to replace</param>
		/// <param name="inAttachment">attachment to add, null if none to attach</param>
		/// <param name="inUserData">User data for the current user</param>
		/// <param name="extraLoggingInfo">Extra info to include in logs</param>
		/// <returns>true if email sent successfully, false if it failed</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public bool SendEmail(EmailContent inEmailTypeToSend, string inRecipient, ICollection<UserData> inCClist, string[] inSubjectReplaceTokens, string[] inBodyReplaceTokens, Attachment inAttachment, UserData inUserData, string extraLoggingInfo = null)
		{
			if (inSubjectReplaceTokens == null)
			{
				throw new ArgumentNullException(nameof(inSubjectReplaceTokens));
			}

			if (inBodyReplaceTokens == null)
			{
				throw new ArgumentNullException(nameof(inBodyReplaceTokens));
			}

			if (inUserData == null)
			{
				throw new ArgumentNullException(nameof(inUserData));
			}

			bool emailSent = false;
			string originalIncomingRecipient = inRecipient;

			if (!ConfigurationUtilities.GetAppSetting("DisableAllEmails", false))
			{
				// Append space to the start of extra logging info if it doesn't contain one
				if (!string.IsNullOrEmpty(extraLoggingInfo) && !char.IsWhiteSpace(extraLoggingInfo, 0))
				{
					extraLoggingInfo = extraLoggingInfo.Insert(0, " ");
				}

				// if passed in as null just make sure it's an empty array so we can manage easier
				if (inCClist == null)
				{
					inCClist = new Collection<UserData>();
				}

				// Replace any semicolons used to separate multiple addresses with commas
				// Needed for use with MailAddressCollection.Add
				inRecipient = inRecipient?.Replace(';', ',') ?? string.Empty;

				Log.LogDebug("EMAIL - Entering sendemail for " + inEmailTypeToSend.ToString() + " to recipient [" + inRecipient + "]" + extraLoggingInfo);

				// validate the number of tokens to replace (i.e. number of {0}, {1}, ...) is the same as the 
				// tokens we have been given to plug in (i.e. elements in subject and body)

				int count = inEmailTypeToSend.Subject.Count(f => f == '{');
				if (!inSubjectReplaceTokens.Length.Equals(count))
				{
					string error = string.Format("Number of replaceable tokens in subject [{0}] did not match the actual number of tokens for replacing [{1}].{2}", count, inSubjectReplaceTokens.Length, extraLoggingInfo);
					Log.LogError(error);
					throw new ArgumentException(
						error,
						nameof(inSubjectReplaceTokens));
				}

				count = inEmailTypeToSend.Body.Count(f => f == '{');
				if (!inBodyReplaceTokens.Length.Equals(count))
				{
					string error = string.Format("Number of replaceable tokens in body [{0}] did not match the actual number of tokens for replacing [{1}].{2}", count, inBodyReplaceTokens.Length, extraLoggingInfo);
					Log.LogError(error);
					throw new ArgumentException(
						error,
						nameof(inBodyReplaceTokens));
				}

				// check the overrides for the recipient, this is useful in testing or other scenarios
				string originalRecipient = string.Empty;
				string fromAddress = CommonUtilities.HelpdeskEmailAddress();

				bool emailCurrentUser = ConfigurationUtilities.GetAppSetting("EmailsToCurrentlyLoggedInUser", false);
				if (emailCurrentUser)
				{
					// Remove the space after the comma in the Recipient list.
					string orgRecipient = inRecipient != null ? inRecipient.Replace(", ", ",") : string.Empty;

					string[] orgValidatedRecipients = orgRecipient.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
						.Where(x => RegexEmailAddress.IsMatch(x)).ToArray();
					orgRecipient = string.Join(",", orgValidatedRecipients);

					originalRecipient = "EMAIL ORIGINALLY SENT TO : " + orgRecipient + "<BR/><BR/>";
					if (inCClist.Any())
					{
						originalRecipient += "With a CC List of " + string.Join(",", inCClist.Select(x => x.Email)) + "<BR/><BR/>";
					}

					inRecipient = inUserData.Email;
					inCClist = new Collection<UserData>(new UserData[] { new UserData { Email = inRecipient } }); // override CC users .. we're in override mode

					Log.LogDebug("EMAIL - Overriding original email recipient to the currently logged in user [" + inRecipient + "]" + extraLoggingInfo);
				}
				else
				{
					// remove current user from email and cc list
					if (!string.IsNullOrEmpty(inUserData.Email))
					{
						inRecipient = inRecipient?.Replace(inUserData.Email, string.Empty);
					}

					if (inCClist != null)
					{
						ICollection<UserData> removeEmails = inCClist.Where(c => c.Email == inUserData.Email).ToList();
						if (removeEmails.Any())
						{
							foreach (UserData removeEmail in removeEmails)
							{
								inCClist.Remove(removeEmail);
							}
						}
					}
				}

				// Remove invalid entries from the cc list.  An empty list is handled below so an empty ValidatedRecipients array is okay here
				List<string> ccStringList = new();
				foreach (UserData cc in inCClist)
				{
					if (cc == null || string.IsNullOrEmpty(cc.Email))
					{
						Log.LogError(string.Format("Found an invalid cc email address for email, removing from list.  Recipient is [{0}], cc is [{1}], cc ntid is [{2}]. Email subject is {3}.  Email body is {4}.{5}",
							inRecipient,
							cc == null ? "cc null" : cc.Email,
							cc == null ? "no ntid (cc null)" : cc.Ntid,
							inEmailTypeToSend.Subject,
							inEmailTypeToSend.Body,
							extraLoggingInfo));
					}
					else
					{
						ccStringList.Add(cc.Email);
					}
				}

				// validate recipients and cc list
				string[] validatedCCList = ccStringList.Where(x => RegexEmailAddress.IsMatch(x)).ToArray();
				string ccsToEmail = string.Join(",", validatedCCList);

				string originalParameterRecipient = inRecipient;

				// Remove the spaces after the comma in the Recipient list.
				inRecipient = inRecipient != null ? inRecipient.Replace(", ", ",") : string.Empty;

				string[] validatedRecipients = inRecipient.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
					.Where(x => RegexEmailAddress.IsMatch(x)).ToArray();
				inRecipient = string.Join(",", validatedRecipients);

				// form the message itself now that we have the proper recipient and subject/body have been verified as being valid
				using (MailMessage message = new())
				{
					Log.LogDebug("EMAIL - Getting ready to send email to " + inRecipient + extraLoggingInfo);

					message.Subject = !inSubjectReplaceTokens.Any()
						? inEmailTypeToSend.Subject
						: string.Format(inEmailTypeToSend.Subject, inSubjectReplaceTokens);

					message.Body = originalRecipient + (!inBodyReplaceTokens.Any()
										? inEmailTypeToSend.Body
										: string.Format(inEmailTypeToSend.Body, inBodyReplaceTokens));
					message.IsBodyHtml = true;

					if (inAttachment != null)
					{
						message.Attachments.Add(inAttachment);
					}

					// wrap the body in a div to apply general styles
					message.Body = "<div style=\"font-family: calibri;\">" + message.Body + "</div>";

					#region Generate alternate view to support embedded images

					EmbeddedImageViewInfo viewInfo = GenerateAlternateViewWithEmbeddedImages(message.Body);

					// the message body is "replaced" with the alternate view
					string originalBody = message.Body;
					message.Body = null;
					message.AlternateViews.Add(viewInfo.View);

					#endregion

					if ((string.IsNullOrWhiteSpace(inRecipient) ||
						string.IsNullOrWhiteSpace(inRecipient.Trim().Replace(",", string.Empty))) && string.IsNullOrWhiteSpace(ccsToEmail))
					{
						// there are no valid recipients for the email ... this is a problem.  log an error message and abort
						// the send since it's not going to anyone right now
						Log.LogWarning(string.Format(
							"Unable to send email as there are no recipients, recipients are [{0}]. Email subject is {1}.  Email body is {2}.{3}",
							inRecipient, message.Subject, originalBody, extraLoggingInfo));
						// returning true if there was originally a recipient
						return !string.IsNullOrWhiteSpace(originalIncomingRecipient);
					}

					Log.LogDebug("EMAIL - SmtpSend begin" + extraLoggingInfo);
					emailSent = TrySendEmail(inRecipient, extraLoggingInfo, fromAddress, ccsToEmail, originalParameterRecipient, message, viewInfo, 1);
				}
			}
			else
			{
				Log.LogInformation(string.Format(
					"Email is disabled by configuration setting, recipients are [{0}]. Email subject is [{1}].  Email body is [{2}].{3}",
					inRecipient, inEmailTypeToSend.Subject, inEmailTypeToSend.Body, extraLoggingInfo));
			}

			return emailSent;
		}

		/// <summary>
		/// Tries to send an email up to 3 times.
		/// </summary>
		/// <param name="inRecipient">The in recipient.</param>
		/// <param name="extraLoggingInfo">The extra logging information.</param>
		/// <param name="fromAddress">From address.</param>
		/// <param name="ccsToEmail">The CCS to email.</param>
		/// <param name="originalParameterRecipient">The original parameter recipient.</param>
		/// <param name="message">The message.</param>
		/// <param name="viewInfo">The view information.</param>
		/// <param name="tryNumber">The nth time we are trying to send the email.</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private bool TrySendEmail(string inRecipient, string extraLoggingInfo, string fromAddress, string ccsToEmail, string originalParameterRecipient, MailMessage message, EmbeddedImageViewInfo viewInfo, int tryNumber)
		{
			bool emailSent = false;
			bool dispose = false;

			if (string.IsNullOrWhiteSpace(inRecipient))
			{
				// move the cc list to the recipient list
				inRecipient = ccsToEmail;
				ccsToEmail = string.Empty;
			}

			try
			{
				if (tryNumber > 3)
				{
					// we have tried 3 times already, log the error
					Log.LogError(
							string.Format(
								"There was an error sending email (with 2 retries), recipients are [{0}] (original recipient was [{4}]), cc list is [{3}]. Email subject is {1}.  Email body is {2}. {5}",
								inRecipient, message.Subject, message.Body.ToXmlString(), ccsToEmail,
								originalParameterRecipient, extraLoggingInfo));
					dispose = true;
				}
				else
				{
					try
					{
						// put the From and To addresses in exception try b/c they can generate exceptions if they are invalid (and not slipped through the logic above)
						// and the exceptions need to be caught.
						message.From = new MailAddress(fromAddress);

						// clear the To addresses so recipients aren't re-added on multiple attempts
						message.To.Clear();
						message.To.Add(inRecipient);

						// only add cc if there's a valid email address
						if (!string.IsNullOrWhiteSpace(ccsToEmail))
						{
							message.CC.Add(ccsToEmail);
						}

						using (SmtpClient smtp = new(ConfigurationUtilities.GetAppSetting("EmailServer")))
						{
							smtp.EnableSsl = true;
							smtp.Send(message);
						}

						emailSent = true;
						dispose = true;
					}
					catch (SmtpFailedRecipientsException e)
					{
						// The email failed to send to some of the recipients, but still sent, so log the error but mark as sent
						foreach (SmtpFailedRecipientException ex in e.InnerExceptions)
						{
							Log.LogError(ex,
							string.Format(
								"There was an error sending email to recipient {0}. SmtpStatus is {1}.",
								ex.FailedRecipient, ex.StatusCode.GetDescription()));
						}

						emailSent = true;
						dispose = true;
					}
					catch (SmtpFailedRecipientException e)
					{
						// The email failed to send to one of the recipients, but still sent, so log the error but mark as sent
						Log.LogError(e,
							string.Format(
								"There was an error sending email to recipient {0}. SmtpStatus is {1}.",
								e.FailedRecipient, e.StatusCode.GetDescription()));

						emailSent = true;
						dispose = true;
					}
					catch (Exception e)
					{
						// handling this because not handling it happens on an anonymous thread which can crash the app server.

						// We can continue here since sending an email is not critical to the operation
						// This is necessary when running from a development boxes where email is blocked by McAfee
						Log.LogError(e,
							string.Format(
								"There was an error sending email, try #{0}",
								tryNumber));

						// retry the email
						Thread.Sleep(TimeSpan.FromSeconds(60.0));
						return TrySendEmail(inRecipient, extraLoggingInfo, fromAddress, ccsToEmail, originalParameterRecipient, message, viewInfo, ++tryNumber);
					}
				}
			}
			finally
			{
				if (dispose)
				{
					#region Clean up alternate view resources

					// dispose the alternate view ...
					viewInfo.View.Dispose();

					// ... and close the streams used to embed images in the alternate view
					if (viewInfo.ImageStreams != null)
					{
						foreach (Stream s in viewInfo.ImageStreams)
						{
							s.Close();
						}
					}

					#endregion

					Log.LogDebug("EMAIL - SmtpSend end" + extraLoggingInfo);
				}
			}

			return emailSent;
		}

		/// <summary>
		/// Given the "intended" HTML body for a new email message, parse the HTML to locate and strip out images, then
		/// generate an alternate view for the message that contains those images as embedded content.
		/// </summary>
		/// <param name="htmlBody">HTML</param>
		/// <returns>Alternate view, with images embedded in the message body.  Note: View should be disposed after email is sent.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private EmbeddedImageViewInfo GenerateAlternateViewWithEmbeddedImages(string htmlBody)
		{
			ICollection<EmailImageInfo> images = new Collection<EmailImageInfo>();

			string alternateViewBody = GenImageUtilities.SeparateImagesForEmail(htmlBody, images);

			AlternateView htmlView = AlternateView.CreateAlternateViewFromString(alternateViewBody, null, "text/html");

			ICollection<Stream> imageStreams = new List<Stream>();

			// create linked image resources
			foreach (EmailImageInfo image in images)
			{
				// imageStream must remain open until email has been sent
				Stream imageStream = GenImageUtilities.ConvertBase64StringToStream(image);
				imageStreams.Add(imageStream);

				LinkedResource imageResource = new(imageStream, image.MimeType)
				{
					ContentId = image.EmailContentId
				};
				htmlView.LinkedResources.Add(imageResource);
			}

			return new EmbeddedImageViewInfo
			{
				View = htmlView,
				ImageStreams = imageStreams
			};
		}
	}

	public static class Emails
	{
		#region PTM Emails

		/// <summary>
		/// Email content sent for the approver emails.
		/// </summary>
		public static readonly EmailContent APPROVER_EMAIL = new()
		{
			Subject = "PTM: PROPOSAL APPROVAL ACTION REQUIRED",
			Body = "You have a proposal approval action due for {0} - {1}. Please use the link below to complete your approval. Please complete your action as soon as possible. " +
				   "To access the proposal and provide any inputs needed in order to submit, navigate to the " +
				   "<a href=\"{2}\">Proposal Approval tab</a>."
		};

		/// <summary>
		/// Email content sent for the Lead Alert when approvers do not approve.
		/// </summary>
		public static readonly EmailContent LEAD_ALERT_APPROVERS_EMAIL = new()
		{
			Subject = "PTM: PROPOSAL APPROVAL ACTION REQUIRED",
			Body = "Warning: All approvers have not approved after 3 emails have been sent for {0} - {1}. " +
				   "To access the proposal, navigate to the " +
				   "<a href=\"{2}\">Proposal Approval tab</a>."
		};

		/// <summary>
		/// Email content sent for the Lead Alert when LOB Estimating Lead/Manager does not approve.
		/// </summary>
		public static readonly EmailContent LEAD_ALERT_LOB_NOT_APPROVED_EMAIL = new()
		{
			Subject = "PTM: PROPOSAL APPROVAL ACTION REQUIRED",
			Body = "Warning: LOB Estimating Lead/Manager has not approved after 3 emails have been sent for {0} - {1}. " +
				   "To access the proposal, navigate to the " +
				   "<a href=\"{2}\">Proposal Approval tab</a>."
		};

		/// <summary>
		/// Email content sent for the Lead Alert when LOB Estimating Lead/Manager approves.
		/// </summary>
		public static readonly EmailContent LEAD_ALERT_LOB_APPROVED_EMAIL = new()
		{
			Subject = "PTM: PROPOSAL APPROVAL ALERT",
			Body = "Alert: LOB Estimating Lead/Manager has approved the proposal for {0} - {1}. " +
				   "The proposal is now locked. To access the checklist, navigate to the " +
				   "<a href=\"{2}\">Proposal Checklist tab</a>."
		};

		/// <summary>
		/// Email content sent when a Forecast Proposal is within X days of the Anticipated Delivery Date.
		/// </summary>
		public static readonly EmailContent FORECAST_ALERT_EMAIL = new()
		{
			Subject = "PTM: FORECASTED PROPOSAL ALERT",
			Body = "Alert: Forecasted Proposal {0} - {1} is due soon. " +
				   "To update the Proposal, navigate to the " +
				   "<a href=\"{2}\">Proposal Details tab</a>."
		};

		/// <summary>
		/// Email content sent for monthly Certification Timeline reminder email
		/// </summary>
		public static readonly EmailContent CERTIFICATION_TIMELINE_EMAIL = new()
		{
			Subject = "PTM: Certification Timeline ALERT for {0} - {1} (Action Required)",
			Body = "PTM records indicate that for proposal tracking number {0} ({1}), the Certification Timeline data has not been completed.  " +
				"If your proposal has been certified, please complete the Certification Timeline section in PTM as soon as possible.<br/><br/>" +
				"Reminders to the Contracts, Estimating, and Supply Chain team:<br/><br/>" +
				"If the proposal has not been negotiated and is scheduled to start negotiations within the next few weeks, Contracts should initiate completion of the Pre-Negotiation checklist at this time.<br/><br/>" +
				"It is incumbent upon the proposal team to ensure we are providing timely disclosures to the customer through Contracts and are tracking them in our Contracts disclosure log for traceability and documentation purposes.<br/><br/>" +
				"This e-mail will be sent monthly as a reminder, until the certification is complete.  Thank You"
		};

		/// <summary>
		/// Email content sent to lead estimator when user approves
		/// </summary>
		public static readonly EmailContent APPROVAL_EMAIL = new()
		{
			Subject = "PTM: APPROVAL STATUS PROPOSAL {0} {1}",
			Body = "{0} has approved the subject proposal as {1}. " +
				   " <a href=\"{2}\">Proposal Details tab</a>."
		};

		/// <summary>
		/// Email content sent to the Estimating Data Distribution List after RDM Publishes a new version.
		/// </summary>
		public static readonly EmailContent PUBLISH_EMAIL = new()
		{
			Subject = "PPR&D REVISION {0} Published {1}",
			Body = "The PPR&D has been revised and is available in RDM.<br/><br/>" +
					"Program Contracts and Estimating are expected to review the revised PPR&D in full and provide the appropriate disclosures for your CCOPD proposal(s) to your Contracting Officers. Disclosable information may include data beyond rates and factors such as CAS, system status and Proposal Disclosures. Please note that disclosure artifacts are to be kept in the Contracts file and tracked in the proposal disclosure log for proper records retention and audit purposes.<br/><br/>" +
					"For Contract POCs, the <a href=\"" + CommonUtilities.PPRDDisclosureLogURL() + "\">PPR&D Disclosure Log</a> can be used to create the necessary disclosures for CCoPD covered proposals.  The log will be updated to include the specifics of this revision shortly.<br/><br/>" +
					"{0}<br/>" +
						"<a href=\"{1}\">RDM Home</a><br/>" +
						"<a href=\"{2}\">Export Current Published PPR&D</a><br/>" +
						"<a href=\"{3}\">Revision Comparison</a><br/>"
		};

		/// <summary>
		/// Email content sent to the distribution list after RDM successfully deploys a revision to the classified environment.
		/// </summary>
		public static readonly EmailContent CLASSIFIED_DEPLOYMENT_SUCCESS_EMAIL = new()
		{
			Subject = "PPR&D REVISION {0} deployment succeeded",
			Body = "The PPR&D REVISION {0} deployment succeeded.<br/>" +
					"{1}<br/>"
		};

		/// <summary>
		/// Email content sent to the distribution list after RDM fails to deploy a revision to the classified environment.
		/// </summary>
		public static readonly EmailContent CLASSIFIED_DEPLOYMENT_FAILED_EMAIL = new()
		{
			Subject = "PPR&D revision deployment failed",
			Body = "PPR&D revision deployment failed.<br/>" +
					"{0}<br/>"
		};

		/// <summary>
		/// Email content sent to LOB Estimating Manager/Delegate when a user selects a pricing tool other than ProPricer and/or a boe tool other than genBoe
		/// </summary>
		public static readonly EmailContent NONPREFERRED_PRICING_ESTIMATING_TOOL_EMAIL = new()
		{
			Subject = "PTM: NON-PREFERRED PRICING/ESTIMATING TOOL SELECTED FOR PROPOSAL",
			Body = "{0} has selected a tool other than ProPricer and/or genBOE to price/estimate their {1} {2} proposal. "
			+ "If you disagree you should contact the estimator immediately, and request that they correct their tool selection in PTM. "
			+ "If you agree, no further action is necessary. Upon proposal workflow approval you will be prompted to provide justification for use of the tool(s) selected."
		};

		/// <summary>
		/// Email content sent to LOB Estimating Manager/Delegate when a user adjusts a previously non-preferred a pricing and/or a boe tool to ProPricer and genBoe
		/// </summary>
		public static readonly EmailContent PREFERRED_PRICING_ESTIMATING_TOOL_EMAIL = new()
		{
			Subject = "PTM: PRICING/ESTIMATING TOOL ADJUSTED FOR PROPOSAL",
			Body = "{0} has adjusted his/her tool selection for {1} {2}. Both tools are now set to our preferred tool(s). "
			+ "Justification for utilization of a non-preferred tool is no longer necessary upon proposal workflow approval."
		};

		/// <summary>
		/// Email content to 
		/// </summary>
		public static readonly EmailContent MISSING_OPTIONAL_DOCUMENT_REMINDER_EMAIL = new()
		{
			Subject = "PTM: DOCUMENT UPLOAD REMINDER",
			Body = "Reminder: At the time of your approval, as Lead{0} Estimator of PTM record {1} - {2}, you had not uploaded the Documented "
				+ "Approval to Submit (and DOA if applicable). This is a reminder to upload the document today or as soon as it becomes available.<br/>"
				+ "To upload the document, navigate to the <a href=\"{3}\">Attachments tab</a>."
		};

		/// <summary>
		/// Email sent when Proposal status is set to Lost
		/// </summary>
		public static readonly EmailContent STATUS_LOST_SET = new()
		{
			Subject = "{0} Notification", // [PTM Entry Number]
			Body = "{0} was not awarded. {1}" // [PTM Entry Number], [Link to specific PTM entry]
		};

		/// <summary>
		/// Email sent when Proposal status is set to No Bid
		/// </summary>
		public static readonly EmailContent STATUS_NO_BID_SET = new()
		{
			Subject = "{0} Notification", // [PTM Entry Number]
			Body = "{0} was a No Bid. {1}" // [PTM Entry Number], [Link to specific PTM entry.]
		};

		/// <summary>
		/// Email sent to remind the contracts leads to enter the Mod Executed Date
		/// </summary>
		public static readonly EmailContent MISSING_MOD_CERTIFICATION_DATE = new()
		{
			Subject = "{0} Notification", // [PTM Entry Number]
			Body = "{0} requires the Mod Executed Date, which is necessary to complete the entry. Please visit the Contracts tab and enter the date. Your prompt response is appreciated, thank you. {1}" // [PTM Entry Number], [Link to specific PTM entry.]
		};

		/// <summary>
		/// Email sent when Proposal status is set to Completed
		/// </summary>
		public static readonly EmailContent STATUS_COMPLETED_SET = new()
		{
			Subject = "{0} Notification", // [PTM Entry Number]
			Body = "{0} was completed and placed on contract. {1}" // [PTM Entry Number], [Link to specific PTM entry.]
		};

		#endregion
	}

	public struct EmailContent
	{
		public string Subject { get; set; }

		public string Body { get; set; }

		public override bool Equals(object obj)
		{
			bool equals = false;
			if (obj is not null and EmailContent)
			{
				EmailContent other = (EmailContent)obj;
				if (other.Subject == Subject &&
					other.Body == Body)
				{
					equals = true;
				}
			}

			return equals;
		}

		public override int GetHashCode()
		{
			return Subject.GetHashCode() | Body.GetHashCode();
		}

		public static bool operator ==(EmailContent first, EmailContent second)
		{
			return first.Equals(second);
		}

		public static bool operator !=(EmailContent first, EmailContent second)
		{
			return !first.Equals(second);
		}
	}

	public class EmbeddedImageViewInfo
	{
		/// <summary>
		/// Alternate view
		/// </summary>
		public AlternateView View { get; set; }

		/// <summary>
		/// Collection of (open) image streams (that need to be closed once the email is sent)
		/// </summary>
		public ICollection<Stream> ImageStreams { get; set; }
	}
}
