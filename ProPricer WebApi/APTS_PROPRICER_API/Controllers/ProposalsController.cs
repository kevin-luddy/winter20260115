/*
	Copyright 2016-2020 Lockheed Martin Corporation.

	This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
	commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
	by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
	and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Web.Http;
	using APTSPropricerApi.Connection;
	using APTSPropricerApi.DTOs;
	using EBS.Core;
	using EBS.ProPricer.Data;
	using EBS.ProPricer.Model;

	/// <summary>
	/// Methods to read and update proposals general data 
	/// </summary>
	public class ProposalsController : ProPricerController
	{
		///// <summary>
		///// Looks up the list of current proposals in ProPricer
		///// </summary>
		///// <param name="instanceId">The instance identifier.</param>
		///// <returns>
		///// An array of basic proposal information
		///// </returns>
		//public IEnumerable<ProposalDto> Get(int instanceId)
		//{
		//    IEnumerable<ProposalDto> proposalsResult = null;
		//    using(IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
		//    { 
		//        if (ppc.Workspace != null)
		//        {
		//            try
		//            {
		//                proposalsResult =
		//                    from prop in ppc.Workspace.Proposals.Cast<Proposal>()
		//                    select new ProposalDto
		//                    {
		//                        Id = prop.Id.ToString(),
		//                        Name = prop.Name,
		//                        Version = prop.Version,
		//                        CreatorName = prop.Creator == null ? string.Empty : prop.Creator.Name,
		//                        Description = prop.Description,
		//                        ParentFolderName = prop.ParentFolder == null ? string.Empty : prop.ParentFolder.Name
		//                    };
		//            }
		//            catch (Exception ex)
		//            {
		//                this.Logger.Error(ex);
		//            }
		//        }
		//    }

		//    return proposalsResult;
		//}

		// GET api/proposals/id
		/// <summary>
		/// Returns the general proposal data for a given proposal
		/// </summary>
		/// <param name="instanceId">The instance identifier.</param>
		/// <param name="id">The EntityId of the proposal in the form of a GUID. Ex: 58b0d1c8-b06d-11e3-83f5-b499bae158c0</param>
		/// <returns>
		/// Returns the general proposal data for a given proposal
		/// </returns>
		public ProposalDto Get(int instanceId, string id)
		{
			ProposalDto pDto = new ProposalDto();
			Proposal pr = null;

			using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
			{
				// GUID or Name|Version?
				if (id.Contains("|"))
				{
					// Name
					string[] parts = id.Split('|');
					pr = ppc.Workspace.Proposals.Find(parts[0], parts[1]).Value();
				}
				else
				{
					// GUID
					EntityId pEntityId = new EntityId(new Guid(id));
					pr = ppc.Workspace.Proposals.Find(pEntityId).Value();
				}

				try
				{
					pr.Open();
					pDto.CreatorName = pr.Creator == null ? string.Empty : pr.Creator.Name;
					pDto.Id = pr.Id.ToString();
					pDto.Number = pr.Number;
					pDto.Name = pr.Name;
					pDto.Version = pr.Version;
					pDto.Description = pr.Description;
					//System.Diagnostics.Debug.WriteLine(pr.Notes.Text);
					pDto.Title = pr.Title;
					pDto.Manager = pr.Manager;
					pDto.BusinessUnit = pr.BusinessUnit;
					pDto.Rfq = pr.RFQ;
					pDto.FiscalYearStartMonthOffset = pr.FiscalYearStartMonthOffset;
					pDto.TaskIdLabel = pr.TaskIdLabel;
					pDto.RptFooter = pr.RptFooter;
					pDto.StartDate = pr.StartDate.ToString();
					pDto.EndDate = pr.EndDate.ToString();
					pDto.NumberMonths = ((pr.EndDate.Value.Year - pr.StartDate.Value.Year) * 12) + pr.EndDate.Month - pr.StartDate.Month + 1;
					pDto.DueDate = pr.DueDate.ToString();
					pDto.ResourceDecimalPercision = pr.ResourceDecimals;
					pDto.AwardProbability = pr.AwardProbability.ToString();
					pDto.TargetPrice = pr.TargetPrice.ToString();
					pDto.GlobalProfitFactor = pr.GlobalProfitFactor.HasValue ? pr.GlobalProfitFactor.Value.ToString() : "0";
					pDto.DirectRateTable = pr.DirectRateTable != null ? pr.DirectRateTable.Name : string.Empty;
					pDto.BurdenRateTable = pr.BurdenRateTable != null ? pr.BurdenRateTable.Name : string.Empty;
					pDto.TravelRateTable = pr.TravelRateTable != null ? pr.TravelRateTable.Name : string.Empty;
					pDto.FactorRateTable = pr.FactorRateTable != null ? pr.FactorRateTable.Name : string.Empty;
					pr.Notes.Open();
					pDto.Notes = pr.Notes.Text;
					pr.Notes.Close();

					foreach (SummaryFieldDefinition sfd in pr.SummaryFieldDefinitions.Items())
					{
						SummaryFieldDefinitionsDto sfdDto = new SummaryFieldDefinitionsDto
						{
							Name = sfd.Name,
							DataType = sfd.DataType.ToString(),
							MaxLength = sfd.MaxLength,
							SortType = sfd.SortType.ToString()
						};
						try
						{
							sfdDto.TitleTable = sfd.TitleTable != null ? sfd.TitleTable.Name : string.Empty;
						}
						catch (Exception ex)
						{
							this.Logger.Error(ex, "Property: TitleTable");
						}

						try
						{
							sfdDto.Validate = sfd.Validate;
						}
						catch // (Exception ex)
						{
							// Dusan - this seems to be failing quite a bit, and clogging up logs. Leaving it in here in case that property serves a purpose, but not going to keep logging it
							// this.Logger.Error(ex, "Property: Validate");
						}

						sfdDto.Required = sfd.Required;
					}

					pr.Close();
				}
				catch (Exception ex)
				{
					this.Logger.Error(ex);
					throw new Exception("Operation failed.");
				}
			}

			return pDto;
		}

		// POST api/proposals
		/// <summary>
		/// Add a new proposal to ProPricer
		/// </summary>
		/// <param name="instanceId">The instance identifier.</param>
		/// <param name="newProp">Proposal object that must contain the following:
		/// "id" - The entity id of the proposal to be copied from.
		/// Ex: 58b0d1c8-b06d-11e3-83f5-b499bae158c0
		/// "name" - The name of the new proposal.
		/// "version" - The version number of the new proposal.
		/// "description" - The description of the new proposal.
		/// 'folder" - The folder where the new proposal will be stored.</param>
		/// <returns>
		/// Returns the id of the newly created proposal if successful. If not, returns an error message.
		/// </returns>
		public ReturnDto Post(int instanceId, [FromBody] ProposalDto newProp)
		{
			if (newProp?.Name == null || newProp.Name.Trim() == string.Empty)
			{
				ReturnDto retdto = new ReturnDto
				{
					Retcode = "500",
					Retmsg = "New Proposal name cannot be blank"
				};
				return retdto;
			}

			if (newProp.Version.Trim() == string.Empty)
			{
				newProp.Version = "0";
			}

			string whichvar = "notes";
			using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
			{
				try
				{
					// Get notes from our template to copy 
					// Templates are in different folders now 
					//   EBS.ProPricer.Data.EntityId pEntityId = new EBS.ProPricer.Data.EntityId(new Guid("58b0d1c8-b06d-11e3-83f5-b499bae158c0"));
					//   Proposal pr = ppc.workspace.Proposals.Find(pEntityId).Value;
					//   pr.Open();
					//   pr.Notes.Open();
					//   string notes = pr.Notes.Text;
					//   pr.Notes.Close();
					//   pr.Close();

					/**************************************************************************************************************
					 * Create the proposal and set the proposal into the folder that is specified by the user.
					 * No folder path indicates no folder insertion.
					 * *************************************************************************************************************/
					whichvar = "AddNew commands";
					Proposal proposal = ppc.Workspace.Proposals.AddNew();
					whichvar = "Open and edit commands";
					//lock the proposal for modification.
					proposal.Open();
					proposal.BeginEdit();

					/**********************************************************************************************
					 * Set the General Proposal Data parameters these are required fields within propricer
					 * 
					 * Observations to consider:
					 *  -the API will default the Proposal version to 0, if not provided.
					 *  -Various Try / Catch blocks are necessary to ensure that issues are caught and displayed 
					 *  in the appropriate manner, due to the complexity of the API, it's properties, and the lack of
					 *  documentation.
					 * *******************************************************************************************/
					whichvar = "new name";
					proposal.Name = newProp.Name;
					whichvar = "new number";
					proposal.Number = newProp.Number;
					whichvar = "new version";
					proposal.Version = newProp.Version;
					whichvar = "new description";
					proposal.Description = newProp.Description;
					whichvar = "new manager name";
					proposal.Manager = newProp.Manager;
					whichvar = "new business unit";
					proposal.BusinessUnit = newProp.BusinessUnit;
					whichvar = "new rfq";
					proposal.RFQ = newProp.Rfq;
					whichvar = "new notes";
					proposal.Notes.Open();
					proposal.Notes.BeginEdit();
					proposal.Notes.Text = newProp.Notes;
					proposal.Notes.EndEdit();
					proposal.Notes.Close();

					whichvar = "new folder";
					Folder folder = ppc.Workspace.GlobalLibrary.Folders.Find(newProp.ParentFolderName, FolderCategory.Proposal, null).Value();
					proposal.ParentFolder = folder;

					whichvar = "new start date";
					proposal.StartDate = TimeFrame.FromMonth(int.Parse(newProp.StartDate.Substring(0, 4)), int.Parse(newProp.StartDate.Substring(5, 2)));
					whichvar = "new end date";
					proposal.EndDate = TimeFrame.FromMonth(int.Parse(newProp.EndDate.Substring(0, 4)), int.Parse(newProp.EndDate.Substring(5, 2)));
					whichvar = "new due date";
					proposal.DueDate = TimeFrame.FromDay(int.Parse(newProp.DueDate.Substring(0, 4)), int.Parse(newProp.DueDate.Substring(5, 2)), int.Parse(newProp.DueDate.Substring(8, 2)));

					whichvar = "profit fee";
					if (newProp.GlobalProfitFactor != null)
					{
						proposal.GlobalProfitFactor = double.Parse(newProp.GlobalProfitFactor);
					}

					string directRate = newProp.DirectRateTable; // "Direct 04_10_14B";
					string burdenRate = newProp.BurdenRateTable; // "Indirect Common 04_10_14";
					string factorRate = newProp.FactorRateTable; //  "CER Factors 04_10_14";
					string travelRate = newProp.TravelRateTable; //  "140818_Travel";

					whichvar = "new direct rate table";
					proposal.DirectRateTable = ppc.Workspace.GlobalLibrary.ResourceRateTables.Find(directRate).Value();
					whichvar = "new burden rate table";
					proposal.BurdenRateTable = ppc.Workspace.GlobalLibrary.BurdenRateTables.Find(burdenRate).Value();
					whichvar = "new factor rate table";
					proposal.FactorRateTable = ppc.Workspace.GlobalLibrary.FactorRateTables.Find(factorRate).Value();
					whichvar = "new travel rate table";
					proposal.TravelRateTable = ppc.Workspace.GlobalLibrary.TravelRateTables.Find(travelRate).Value();

					whichvar = "end edit and close";
					//End edits and release the record
					proposal.EndEdit();
					//Close the proposal
					proposal.Close();

					ReturnDto retdto = new ReturnDto
					{
						Retcode = "200",
						Retmsg = proposal.Id.ToString()
					};
					return retdto;
				}
				//exception block to catch any broken rules relating to the API business logic
				catch (EBS.ProPricer.Model.General.BOBrokenRulesException ex)
				{
					this.Logger.Error(ex, "Error with " + whichvar + " - " + ex.BrokenRules[0]);
					System.Diagnostics.Debug.WriteLine("Error with " + whichvar + " - " + ex.BrokenRules[0]);
					//  var message = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
					//  message.Content = new System.Net.Http.StringContent("Error with " + whichvar + " - " + ex.BrokenRules[0]);
					if (ppc.Workspace.Proposals.IsOpened())
					{
						ppc.Workspace.Proposals.Close();
					}

					if (ppc.Workspace.IsOpened())
					{
						ppc.Workspace.Close();
					}
					//   Workspace ppcwork = ppc.workspace;
					//   EBS.Core.DisposeHelper.Dispose(ref ppcwork);

					//  throw new HttpResponseException(message);

					ReturnDto retdto = new ReturnDto
					{
						Retcode = "500",
						Retmsg = "Broken rules with " + whichvar + " - " + ex.BrokenRules[0]
					};
					return retdto;
				}
				//exception block to catch any other issues with the data
				catch (Exception ex)
				{
					this.Logger.Error(ex, "Error with " + whichvar);
					System.Diagnostics.Debug.WriteLine("Error with " + whichvar + " - " + ex.Message);

					if (ppc.Workspace.Proposals.IsOpened())
					{
						ppc.Workspace.Proposals.Close();
					}

					if (ppc.Workspace.IsOpened())
					{
						ppc.Workspace.Close();
					}
					//  Workspace ppcwork = ppc.workspace;
					//  EBS.Core.DisposeHelper.Dispose(ref ppcwork);
					//  throw new HttpResponseException(message);

					ReturnDto retdto = new ReturnDto
					{
						Retcode = "500",
						Retmsg = "Error with " + whichvar + " - " + ex.Message
					};
					return retdto;
				}
			}
		}

		// PUT api/proposals
		/// <summary>
		/// Update an existing proposal in ProPricer.
		/// </summary>
		/// <param name="instanceId">The instance identifier.</param>
		/// <param name="changeProp">Proposal object that must contain the following:
		/// "id" - The entity id of the proposal to be changed.
		/// Ex: 58b0d1c8-b06d-11e3-83f5-b499bae158c0
		/// Any other parameters will contain the data to be changed for that field.
		/// Any fields that are not to be changed should be null or match the current data.</param>
		/// <returns>
		/// Returns 200 if successful. If not, returns an error message.
		/// </returns>
		public ReturnDto Put(int instanceId, [FromBody] ProposalDto changeProp)
		{
			if (changeProp == null)
			{
				ReturnDto retdto = new ReturnDto
				{
					Retcode = "500",
					Retmsg = "No data was entered to change"
				};
				return retdto;
			}

			string whichvar = "Id";
			Proposal pr = null;
			using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
			{
				try
				{
					EntityId pEntityId = new EntityId(new Guid(changeProp.Id));
					pr = ppc.Workspace.Proposals.Find(pEntityId).Value();
					if (pr.Locked)
					{
						ReturnDto ret = new ReturnDto
						{
							Retcode = "500",
							Retmsg = "This proposal is locked in PROPRICER. It must be unlocked or renamed before send-to-pricing can be done in APTS"
						};
						return ret;
					}

					//lock the proposal for modification.
					whichvar = "Open and edit commands";
					pr.Open();
					pr.BeginEdit();

					whichvar = "new name";
					if (changeProp.Name != null && changeProp.Name.Trim() != string.Empty)
					{
						pr.Name = changeProp.Name;
					}

					whichvar = "new version";
					if (changeProp.Version != null)
					{
						pr.Version = changeProp.Version;
					}

					whichvar = "new number";
					if (changeProp.Number != null)
					{
						pr.Number = changeProp.Number;
					}

					whichvar = "new description";
					if (changeProp.Version != null)
					{
						pr.Description = changeProp.Description;
					}

					whichvar = "new manager";
					// cannot set here due to the state of the object
					if (changeProp.Manager != null)
					{
						pr.Manager = changeProp.Manager;
					}

					whichvar = "new start date";
					if (changeProp.StartDate != null)
					{
						pr.StartDate = TimeFrame.FromMonth(int.Parse(changeProp.StartDate.Substring(0, 4)), int.Parse(changeProp.StartDate.Substring(5, 2)));
					}

					whichvar = "new end date";
					if (changeProp.EndDate != null)
					{
						pr.EndDate = TimeFrame.FromMonth(int.Parse(changeProp.EndDate.Substring(0, 4)), int.Parse(changeProp.EndDate.Substring(5, 2)));
					}

					whichvar = "new due date";
					if (changeProp.DueDate != null)
					{
						pr.DueDate = TimeFrame.FromDay(int.Parse(changeProp.DueDate.Substring(0, 4)), int.Parse(changeProp.DueDate.Substring(5, 2)), int.Parse(changeProp.DueDate.Substring(8, 2)));
					}

					whichvar = "new business unit";
					if (changeProp.BusinessUnit != null)
					{
						pr.BusinessUnit = changeProp.BusinessUnit;
					}

					whichvar = "new rfq";
					if (changeProp.Rfq != null)
					{
						pr.RFQ = changeProp.Rfq;
					}

					whichvar = "new fiscal year start month";
					if (changeProp.FiscalYearStartMonthOffset > 0 &&
						changeProp.FiscalYearStartMonthOffset < 13)
					{
						pr.FiscalStartMonth = Convert.ToByte(changeProp.FiscalYearStartMonthOffset);
					}

					whichvar = "new decimal precision";
					if (changeProp.ResourceDecimalPercision != null)
					{
						pr.ResourceDecimals = changeProp.ResourceDecimalPercision.Value;
					}

					whichvar = "new target price";
					if (changeProp.TargetPrice != null)
					{
						pr.TargetPrice = double.Parse(changeProp.TargetPrice);
					}

					whichvar = "new award probability";
					if (changeProp.AwardProbability != null)
					{
						pr.AwardProbability = double.Parse(changeProp.AwardProbability);
					}

					whichvar = "profit fee";
					if (changeProp.GlobalProfitFactor != null)
					{
						pr.GlobalProfitFactor = double.Parse(changeProp.GlobalProfitFactor);
					}

					string directRate = changeProp.DirectRateTable; // "Direct 04_10_14B";
					string burdenRate = changeProp.BurdenRateTable; // "Indirect Common 04_10_14";
					string factorRate = changeProp.FactorRateTable; //  "CER Factors 04_10_14";
					string travelRate = changeProp.TravelRateTable; //  "140818_Travel";

					whichvar = "new direct rate table";
					if (changeProp.DirectRateTable != null)
					{
						pr.DirectRateTable = ppc.Workspace.GlobalLibrary.ResourceRateTables.Find(directRate).Value();
					}

					whichvar = "new burden rate table";
					if (changeProp.BurdenRateTable != null)
					{
						pr.BurdenRateTable = ppc.Workspace.GlobalLibrary.BurdenRateTables.Find(burdenRate).Value();
					}

					whichvar = "new factor rate table";
					if (changeProp.FactorRateTable != null)
					{
						pr.FactorRateTable = ppc.Workspace.GlobalLibrary.FactorRateTables.Find(factorRate).Value();
					}

					whichvar = "new travel rate table";
					if (changeProp.TravelRateTable != null)
					{
						pr.TravelRateTable = ppc.Workspace.GlobalLibrary.TravelRateTables.Find(travelRate).Value();
					}

					whichvar = "new notes";
					if (changeProp.Notes != null)
					{
						pr.Notes.Open();
						pr.Notes.BeginEdit();
						pr.Notes.Text = changeProp.Notes;
						pr.Notes.EndEdit();
						pr.Notes.Close();
					}

					whichvar = "end edit and close";
					//End edits and release the record
					pr.EndEdit();
					pr.Close();
					//Close the proposal

					ReturnDto retdto = new ReturnDto
					{
						Retcode = "200",
						Retmsg = "Successful"
					};
					return retdto;
				}
				//exception block to catch any broken rules relating to the API business logic
				catch (EBS.ProPricer.Model.General.BOBrokenRulesException ex)
				{
					this.Logger.Error(ex, "Error with " + whichvar + " - " + ex.BrokenRules[0]);
					System.Diagnostics.Debug.WriteLine("Error with " + whichvar + " - " + ex.BrokenRules[0]);
					pr.CancelEdit();
					if (pr.IsOpened())
					{
						pr.Close();
					}

					//   var message = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
					//   message.Content = new System.Net.Http.StringContent("Error with " + whichvar + " - " + ex.BrokenRules[0]);

					//   throw new HttpResponseException(message);

					ReturnDto retdto = new ReturnDto
					{
						Retcode = "500",
						Retmsg = "Broken rules with " + whichvar + " - " + ex.BrokenRules[0]
					};
					return retdto;
				}
				//exception block to catch any other issues with the data
				catch (Exception ex)
				{
					this.Logger.Error(ex, "Error with " + whichvar);
					System.Diagnostics.Debug.WriteLine("Error with " + whichvar + " - " + ex.Message);
					pr.CancelEdit();
					if (pr.IsOpened())
					{
						pr.Close();
					}

					//    var message = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
					//    message.Content = new System.Net.Http.StringContent("Error with " + whichvar + " - " + ex.Message);

					//    throw new HttpResponseException(message);

					ReturnDto retdto = new ReturnDto
					{
						Retcode = "500",
						Retmsg = "Error with " + whichvar + " - " + ex.Message
					};
					return retdto;
				}
			}
		}

		// DELETE api/proposals/5
		/// <summary>
		/// Delete a proposal from ProPricer
		/// </summary>
		/// <param name="instanceId">The instance identifier.</param>
		/// <param name="id">The EntityId of the proposal in the form of a GUID. Ex: 58b0d1c8-b06d-11e3-83f5-b499bae158c0</param>
		/// <returns></returns>
		[HttpDelete]
		public HttpResponseMessage Delete(int instanceId, string id)
		{
			using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
			{
				EntityId pEntityId = new EntityId(new Guid(id));
				Optional<Proposal> res = ppc.Workspace.Proposals.Find(pEntityId);
				if (res.HasValue)
				{
					Proposal pr = ppc.Workspace.Proposals.Find(pEntityId).Value();
					if (pr.Locked)
					{
						string message = "This proposal is locked in PROPRICER. It must be unlocked or renamed before send-to-pricing can be done in APTS";
						HttpError err = new HttpError(message);
						return this.Request.CreateResponse(HttpStatusCode.Forbidden, err);
					}

					try
					{
						pr.Delete();
					}
					catch (Exception ex)
					{
						this.Logger.Error(ex);
						System.Diagnostics.Debug.WriteLine(ex.Message);
					}
				}
			}
			return this.Request.CreateResponse(HttpStatusCode.OK);
		}

		/// <summary>
		/// Looks up the list of Proposals in ProPricer that the user is able to access
		/// </summary>
		/// <param name="instanceId">ProPricer Instance Id</param>
		/// <returns>An array of basic proposal information</returns>
		public ICollection<ProposalFolderInfo> Get(int instanceId)
		{
			ICollection<ProposalFolderInfo> result = new List<ProposalFolderInfo>();

			using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
			{
				if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.RMS)
				{
					// RMS allows access to all proposals
					result = this.GetAllProposals(ppc);
				}
				else
				{
					// SSC uses NTID to control access
					result = this.GetSpecificUsersProposals(ppc, Thread.CurrentPrincipal.Identity.Name);
				}

				return result;
			}
		}

		#region Private Helpers

		/// <summary>
		/// Gets all proposals.
		/// </summary>
		/// <param name="ppc">ProPricer Connection</param>
		/// <returns>All Proposals in the system</returns>
		internal List<ProposalFolderInfo> GetAllProposals(IProPricerConnection ppc)
		{
			List<ProposalFolderInfo> tree = new List<ProposalFolderInfo>();

			try
			{
				if (ppc.Workspace != null)
				{
					List<ProposalFolderInfo> allFolders = ppc.Workspace.GlobalLibrary.Folders.GetItems(FolderCategory.Proposal).Select(x => new ProposalFolderInfo(x)).OrderBy(f => f.Name).ToList();

					List<ProposalFolderInfo> allProposals = ppc.Workspace.Proposals.Items().Cast<Proposal>().Select(x => new ProposalFolderInfo(x)).OrderBy(f => f.Name).ToList();

					allFolders.AddRange(allProposals);

					Dictionary<EntityId, ProposalFolderInfo> proposalFolderById = allFolders.ToDictionary(x => x.EntityId);

					foreach (ProposalFolderInfo folder in allFolders)
					{
						bool parentFound = false;
						if (folder.ParentEntityId.HasValue)
						{
							if (proposalFolderById.TryGetValue(folder.ParentEntityId.Value, out ProposalFolderInfo parentFolder))
							{
								parentFound = true;
								parentFolder.ChildElements.Add(folder);
							}
						}

						if (!parentFound)
						{
							tree.Add(folder);
						}
					};
				}
			}
			catch (Exception ex)
			{
				this.Logger.Error(ex, "GetAllProposals");
				throw new Exception("Operation failed.");
			}

			return tree;
		}

		/// <summary>
		/// Gets proposals for the specific user.
		/// </summary>
		/// <param name="ppc">ProPricer Connection</param>
		/// <param name="ntid">The ntid of the user.</param>
		/// <returns>A tree structure of all folders and proposals, to which the user has access</returns>
		private ICollection<ProposalFolderInfo> GetSpecificUsersProposals(IProPricerConnection ppc, string ntid)
		{
			List<ProposalFolderInfo> tree = new List<ProposalFolderInfo>();

			if (string.IsNullOrWhiteSpace(ntid))
			{
				ntid = System.Security.Principal.WindowsIdentity.GetCurrent()?.Name;
			}

			try
			{
				if (ppc.Workspace != null)
				{
					ppc.Workspace.Users.Open();
					User user = (User)ppc.Workspace.Users.Find(ntid);

					// if system admin, display all proposals
					if (user.Role.ProposalAccess == ProposalAccessLevel.Full)
					{
						tree = this.GetAllProposals(ppc);
					}
					else
					{
						// we don't want to have to flatten the tree to search its contents, so we will just keep track on the fly.. ugly, I know :(
						List<ProposalFolderInfo> allItemsFlat = new List<ProposalFolderInfo>();

						user.ProposalPermissionInfo.Open();
						// In ProPricer lingo.. AllowView means that you are allowed to edit... (facepalm)
						foreach (ProposalPermissionInfo permission in user.ProposalPermissionInfo.GetItems().Where(x => x.AllowView))
						{
							this.BuildTreeBottomUp(tree, allItemsFlat, new ProposalFolderInfo(permission.Proposal), permission.Proposal.ParentFolder);
						}
						user.ProposalPermissionInfo.Close();

						IEnumerable<Proposal> proposals = ppc.Workspace?.Proposals?.Items();
						if (proposals != null)
						{
							// This piece is necessary because the above misses a small subset of proposals that are owned by the user... (facepalm)
							foreach (Proposal proposal in proposals.Cast<Proposal>().Where(x => x.ActualOwner?.LoginName?.ToLower() == ntid.ToLower()))
							{
								this.BuildTreeBottomUp(tree, allItemsFlat, new ProposalFolderInfo(proposal), proposal.ParentFolder);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.Logger.Error(ex, "GetSpecificUsersProposals");
				throw new Exception("Operation failed.");
			}
			finally
			{
				if (ppc.Workspace.Users.IsOpened()) { ppc.Workspace.Users.Close(); }
			}

			return tree;
		}

		/// <summary>
		/// Builds the tree bottom up, starting from the proposal. This method merges
		/// </summary>
		/// <param name="tree">The tree.</param>
		/// <param name="allItemsFlat">Flattened tree.</param>
		/// <param name="child">The child.</param>
		/// <param name="folder">The folder.</param>
		private void BuildTreeBottomUp(ICollection<ProposalFolderInfo> tree, List<ProposalFolderInfo> allItemsFlat, ProposalFolderInfo child, Folder folder)
		{
			// if tree contains proposal -> EXIT
			if (child.IsProposal && allItemsFlat.Any(x => x.EntityId == child.EntityId))
			{
				return;
			}

			// add child to the flatTree, so we know it's a part of our tree
			allItemsFlat.Add(child);

			// if we reached top of the folder tree -> add child to the tree, EXIT
			if (folder == null)
			{
				tree.Add(child); 
				return;
			}

			// if tree already contains folder -> add child to the folder, EXIT
			ProposalFolderInfo matchingFolder = allItemsFlat.FirstOrDefault(x => x.EntityId == folder.Id);
			if (matchingFolder != null)
			{
				matchingFolder.ChildElements.Add(child);
				return;
			}

			// the folder is valid, but not a part of a tree yet -> recurse w/ parent folder
			this.BuildTreeBottomUp(tree, allItemsFlat, new ProposalFolderInfo(folder, child), folder.ParentFolder);
		}

		#endregion
	}
}