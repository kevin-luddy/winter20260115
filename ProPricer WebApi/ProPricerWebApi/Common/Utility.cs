/*
	Copyright 2016-2020 Lockheed Martin Corporation.

	This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
	commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
	by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
	and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi.Common
{
	using ACV.Shared;
	using APTSPropricerApi.Connection;
	using APTSPropricerApi.DTOs;
	using EBS.Core;
	using EBS.ProPricer.Data;
	using EBS.ProPricer.Model;
	using EBS.ProPricer.Model.General;
	using EBS.ProPricer.Reports;
	using EBS.ProPricer.Reports.Engine.Processing;
	using EBS.ProPricer.Reports.Export;

	/// <summary>
	/// Utility Class for Common Methods used in multiple Controllers.
	/// </summary>
	public static class Utility
	{
		/// <summary>
		/// Generate TempFile for Export to utilize
		/// </summary>
		/// <param name="batchReport">Batch Report</param>
		/// <param name="proposal">Proposal</param>
		/// <param name="exportType">Export Type</param>
		/// <returns>Temp File</returns>
		public static string GenerateTempFile(BatchReport batchReport, Proposal proposal, ExportType exportType)
		{
			batchReport.Open();

			string tempFile = Path.GetRandomFileName();
			BatchReportContextManager mgr = new(proposal);
			BatchReportRuntimeContext ctx = new(batchReport, mgr);

			if (batchReport.ReportType == EBS.ProPricer.Data.ReportType.Regular)
			{
				ctx.Options.ExportType = exportType;
				ctx.Options.Folder = Constants.TEMP_DIRECTORY;
				ctx.Options.FileName = Path.GetFileNameWithoutExtension(tempFile);
				ctx.Options.Destination = ReportDestination.File;
				ctx.Options.OutputMode = OutputMode.Combined;
				ctx.ProcessAll = true;

				BatchReportGenerator generator = new(ctx);
				ctx.Generator = generator;
				generator.Process();
				ctx.RuntimeReports.ForEach(x =>
				{
					x.OutputOptions.Destination = ReportDestination.File;
					x.OutputOptions.ExportDestinationType = exportType;
					x.OutputOptions.ExportPath = Directory.GetParent(tempFile).FullName;
					x.OutputOptions.ExportFileName = Path.GetFileName(tempFile);
					x.OutputOptions.OpenExportedDocument = false;

					ReportGenerator generator = new(x);
					generator.ProcessReport();
				});
			}
			else
			{
				ctx.Options.ExportType = exportType;
				ctx.Options.Folder = Constants.TEMP_DIRECTORY;
				ctx.Options.FileName = Path.GetFileNameWithoutExtension(tempFile);
				ctx.Options.Destination = ReportDestination.File;
				ctx.Options.OutputMode = OutputMode.Combined;
				ReportOption<bool> option = ctx.Options.FindOption<ReportOption<bool>>(BatchReportOptions.OpenExportedDocumentKey);
				option.SetValue(false);
				ctx.ProcessAll = true;

				BatchReportGenerator generator = new(ctx);
				ctx.Generator = generator;
				generator.Process();
			}

			tempFile = Path.Combine(ctx.Options.Folder, ctx.Options.FileName + GetExportExtension(ctx.Options.ExportType));

			return tempFile;
		}

		/// <summary>
		/// Get File extension for Report (Default is Excel => .xlsx)
		/// </summary>
		/// <param name="exportType">Enum of File type</param>
		/// <returns>Complete extension of File (i.e .xlsx)</returns>
		public static string GetExportExtension(ExportType exportType)
		{
			// Defaulting to Excel
			string extension = ".xlsx";

			if (exportType == ExportType.Pdf)
			{
				extension = ".pdf";
			} 
			else if (exportType == ExportType.Word)
			{
				extension = ".docx";
			}

			return extension;
		}

		/// <summary>
		/// Get All Proposals
		/// </summary>
		/// <param name="ppc">The ProPricer Connection</param>
		/// <param name="logger">The Logger</param>
		/// <returns>List of all proposals</returns>
		public static List<ProposalFolderInfo> GetAllProposals(IProPricerConnection ppc, ILogger logger)
		{
			List<ProposalFolderInfo> tree = new();

			try
			{
				if (ppc.Workspace != null)
				{
					List<ProposalFolderInfo> allFolders = ppc.Workspace.GlobalLibrary.Folders.GetItems(FolderCategory.Proposal).Select(x => new ProposalFolderInfo(x)).OrderBy(f => f.Name).ToList();

					List<ProposalFolderInfo> allProposals = ppc.Workspace.Proposals.Items().Select(x => new ProposalFolderInfo(x)).OrderBy(f => f.Name).ToList();

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
				logger.LogError(ex, "GetAllProposals");
				throw new Exception("Operation failed.");
			}

			return tree;
		}

		/// <summary>
		/// Gets the Pool Manager instances.
		/// </summary>
		/// <returns>Returns a collection of the pool manager instances.</returns>
		public static ICollection<PoolInstanceDto> GetAllPoolInstances(PoolManagerList poolManagerList, ILogger logger)
		{
			List<PoolInstanceDto> instances = new();

			try
			{
				bool isUsingBackup = ConfigurationServiceWeb.Configuration.GetValue<bool>("UseProPricerBackup");
				foreach (PoolManager poolManager in poolManagerList.Instances)
				{
					instances.Add(new PoolInstanceDto
					{
						Id = poolManager.InstanceId,
						IsBackup = isUsingBackup,
						FriendlyName = poolManager.FriendlyName
					});
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Get Pool Instances");
				throw new Exception("Operation failed.");
			}

			return instances;
		}

		/// <summary>
		/// Returns the general proposal data for a given proposal
		/// </summary>
		/// <param name="poolManagerList">The pool manager list</param>
		/// <param name="logger">The logger</param>
		/// <param name="instanceId">The instance identifier.</param>
		/// <param name="id">The EntityId of the proposal in the form of a GUID. Ex: 58b0d1c8-b06d-11e3-83f5-b499bae158c0</param>
		/// <returns>
		/// Returns the general proposal data for a given proposal
		/// </returns>
		public static ProposalDto GetProposal(PoolManagerList poolManagerList, ILogger logger, int instanceId, string id)
		{
			ProposalDto pDto = new();
			using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
			{
				Proposal pr;
				// GUID or Name|Version?
				if (id.Contains('|'))
				{
					// Name
					string[] parts = id.Split('|');
					pr = ppc.Workspace.Proposals.Find(parts[0], parts[1]).Value();
				}
				else
				{
					// GUID
					EntityId pEntityId = new(new Guid(id));
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
						SummaryFieldDefinitionsDto sfdDto = new()
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
							logger.LogError(ex, "Property: TitleTable");
						}

						try
						{
							sfdDto.Validate = sfd.Validate;
						}
						catch // (Exception ex)
						{
							// Dusan - this seems to be failing quite a bit, and clogging up logs. Leaving it in here in case that property serves a purpose, but not going to keep logging it
							// logger.LogError(ex, "Property: Validate");
						}

						sfdDto.Required = sfd.Required;
					}

					pr.Close();
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Error retrieving Proposal");
					throw new Exception("Operation failed.");
				}
			}

			return pDto;
		}

		/// <summary>
		/// Returns the tasks for a given proposal
		/// </summary>
		/// <param name="poolManagerList">The pool manager list</param>
		/// <param name="logger">The logger</param>
		/// <param name="instanceId">The instance identifier.</param>
		/// <param name="id">The EntityId of the proposal in the form of a GUID. Ex: 58b0d1c8-b06d-11e3-83f5-b499bae158c0</param>
		/// <returns>
		/// Returns the tasks for a given proposal
		/// </returns>
		public static ICollection<TaskDto> GetTasksForProposal(PoolManagerList poolManagerList, ILogger logger, int instanceId, string id)
		{
			bool getall = false;
			if (id.EndsWith("Direct"))
			{
				id = id[..^6];
			}
			else
			{
				getall = true;
			}

			Proposal pr = null;
			string whichvar = "proposal id";
			List<TaskDto> tasks = new();
			using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
			{
				try
				{
					// GUID or Name|Version?
					if (id.Contains('|'))
					{
						// Name
						string[] parts = id.Split('|');
						pr = ppc.Workspace.Proposals.Find(parts[0], parts[1]).Value();
					}
					else
					{
						// GUID
						EntityId pEntityId = new(new Guid(id));
						pr = ppc.Workspace.Proposals.Find(pEntityId).Value();
					}

					pr.Open();
					///////////////////////////////////
					foreach (EBS.ProPricer.Model.Task t in pr.Tasks.Items())
					{
						whichvar = "tasks";
						t.Open();
						TaskDto tdto = new()
						{
							Id = t.Id.ToString(),
							Name = t.Name,
							Description = t.Description,
							StartDate = t.StartDate.ToString(),
							EndDate = t.EndDate.ToString(),
							ActualFee = t.ActualFee.ToString(),
							Quantity = t.Quantity
						};

						whichvar = "resource assignment";
						List<ResourceAssignmentDto> resourceAssignments = new();

						if (t.ResourceAssignments != null && t.ResourceAssignments.Count > 0)
						{
							foreach (IResourceAssignment r in t.ResourceAssignments.Items())
							{
								r.Open();
								if (r.Source.Type.ToString() == "Direct" || getall)
								{
									//System.Diagnostics.Debug.WriteLine("Resource: " + r.Info.Description);

									//CostInfo cInfo = r.GetCost();
									//System.Diagnostics.Debug.WriteLine("DirectCost: " + cInfo.DirectCost);

									//BurdenCostElementCollection bcec = cInfo.BurdenElements;
									//foreach (var bce in bcec)
									//{
									//    System.Diagnostics.Debug.WriteLine(bce.Name);
									//}

									ResourceAssignmentDto rdto = new();

									List<SpreadDto> spread = new();
									rdto.SpreadCurve = r.Spread.Curve != null ? r.Spread.Curve.Name : string.Empty;
									rdto.Amount = r.Spread.Amount.ToString();
									rdto.StartDate = r.Spread.StartDate != null ? r.Spread.StartDate.ToString() : string.Empty;
									rdto.EndDate = r.Spread.EndDate != null ? r.Spread.EndDate.ToString() : string.Empty;
									rdto.Id = r.Info.Resource.Id.ToString();
									rdto.Name = r.Info.Resource.Name;
									rdto.InfoDescription = r.Info.Resource.Description;
									rdto.SourceType = r.Source.Type.ToString();

									List<ResourceFieldsDto> rsfdto = new();
									if (r.Info.ResourceFields != null)
									{
										foreach (KeyValuePair<IResourceFieldDefinition, IResourceFieldStandardValue> item in r.Info.ResourceFields)
										{
											ResourceFieldsDto rfdto = new()
											{
												Key = item.Key != null ? item.Key.Name : string.Empty,
												Value = item.Value != null ? item.Value.Value.ToString() : string.Empty
											};
											rsfdto.Add(rfdto);
										}
									}

									rdto.ResourceFields = rsfdto;

									if (rdto.SourceType == "Group")
									{
										rdto.SourceType = "CER";
									}

									// COST for v9.2+
									CostInfo c = r.GetCost();
									if (c != null)
									{
										rdto.DirectCost = c.DirectCost.ToString();
										// Look in BurdenElements to find the Price element (Linq)
										IEnumerable<IBurdenCostElement> price =
											from ele in c.BurdenElements
											where ele.Name.Equals("Price")
											select ele;

										if (price != null && price.Any())
										{
											rdto.Price = c.BurdenCost(price.ElementAt(0).Position).ToString();
										}

										List<BurdenCostDto> burdensDto = new();
										foreach (IBurdenCostElement el in c.BurdenElements)
										{
											BurdenCostDto burdens = new()
											{
												Name = el.Name
											};
											int pos = el.Position;
											burdens.Value = c.BurdenCost(pos).ToString();
											burdensDto.Add(burdens);
										}

										rdto.BurdenCost = burdensDto;
									}
									else
									{
										rdto.BurdenCost = new List<BurdenCostDto>();
									}

									r.GetCost();

									if (r.Spread.Distribution.Any())
									{
										spread.AddRange(r.Spread.Distribution.Select(s => new SpreadDto
										{
											Year = s.Key.Year,
											Month = s.Key.Month,
											Value = s.Value.ToString()
										}));
									}

									rdto.Spread = spread;
									resourceAssignments.Add(rdto);
								} //direct

								r.Close();
							}
						}

						tdto.ResourceAssignments = resourceAssignments;

						whichvar = "material assignment";
						// material assignment
						List<MaterialAssignmentDto> materialAssignments = new();
						foreach (MaterialAssignment ma in t.MaterialAssignments.Items())
						{
							ma.Open();

							MaterialAssignmentDto madto = new()
							{
								MaterialName = ma.MaterialName, //mat id 
								Description = ma.Description,
								Type = ma.Type.ToString(),
								PartName = ma.Name, //Assembly/Part
								PartDescription = ma.MaterialDescription,
								MakeBuy = ma.MakeBuy.ToString(),
								UnitQty = ma.UnitQty.ToString(),
								ShipQty = ma.ShipQty.ToString(),
								TotalMfgStartQty = ma.TotalMfgStartQty.ToString(),
								UnitCost = ma.UnitCost.ToString(),
								TotalCost = ma.TotalCost.ToString()
							};
							//  madto.spreadCurve = (ma.Spread.Curve != null) ? ma.Spread.Curve.Name : string.Empty;
							//  madto.startDate = (ma.Spread.StartDate != null) ? ma.Spread.StartDate.ToString() : string.Empty;
							//  madto.endDate = (ma.Spread.EndDate != null) ? ma.Spread.EndDate.ToString() : string.Empty;

							whichvar = "material resource assignment";

							if (ma.ResourceAssignmentInfo != null)
							{
								// if (r.Source.Type.ToString() == "Direct")
								// {

								ResourceAssignmentDto rdto = new();

								List<SpreadDto> matspread = new();

								rdto.InfoDescription = ma.ResourceAssignmentInfo.Resource.Description;
								rdto.Name = ma.ResourceAssignmentInfo.Resource.Name;

								rdto.SpreadCurve = ma.Spread.Curve != null ? ma.Spread.Curve.Name : string.Empty;
								rdto.StartDate = ma.Spread.StartDate != null ? ma.Spread.StartDate.ToString() : string.Empty;
								rdto.EndDate = ma.Spread.EndDate != null ? ma.Spread.EndDate.ToString() : string.Empty;
								rdto.Amount = ma.Spread.Amount != null ? ma.Spread.Amount.ToString() : "0";

								List<ResourceFieldsDto> rsfdto = new();
								if (ma.ResourceAssignmentInfo.ResourceFields != null)
								{
									foreach (KeyValuePair<IResourceFieldDefinition, IResourceFieldStandardValue> item in ma.ResourceAssignmentInfo.ResourceFields)
									{
										ResourceFieldsDto rfdto = new()
										{
											Key = item.Key != null ? item.Key.Name : string.Empty,
											Value = item.Value != null ? item.Value.Value.ToString() : string.Empty
										};
										rsfdto.Add(rfdto);
									}
								}

								rdto.ResourceFields = rsfdto;

								if (ma.Spread != null)
								{
									foreach (KeyValuePair<TimeFrame, EBS.Number> s in ma.Spread.Distribution)
									{
										SpreadDto sdto = new()
										{
											Year = s.Key.Year,
											Month = s.Key.Month,
											Value = s.Value.ToString()
										};
										matspread.Add(sdto);
									}
								}

								rdto.Spread = matspread;
								madto.ResourceAssignment = rdto;
							}

							whichvar = "material associated costs";

							List<AssociatedCostsDto> asclistdto = new();
							if (ma.AssociatedCosts != null && ma.AssociatedCosts.Count > 0)
							{
								List<SpreadDto> ascspread = new();
								foreach (MaterialAssignmentAssociatedCost asc in ma.AssociatedCosts.Items())
								{
									AssociatedCostsDto ascdto = new()
									{
										Id = asc.Id.ToString(),
										Name = asc.Name,

										SpreadCurve = asc.Spread.Curve != null ? asc.Spread.Curve.Name : string.Empty,
										StartDate = asc.Spread.StartDate != null ? asc.Spread.StartDate.ToString() : string.Empty,
										EndDate = asc.Spread.EndDate != null ? asc.Spread.EndDate.ToString() : string.Empty,
										TotalAmount = asc.Spread.Amount ?? 0,
										Amount = asc.Amount,
										LinkQty = asc.LinkQty,
										LinkSpread = asc.LinkSpread
									};

									ResourcesDto ascresdto = new()
									{
										Name = asc.ResourceAssignmentInfo.Resource.Name,
										Description = asc.ResourceAssignmentInfo.Resource.Description,
										Rclass = asc.ResourceAssignmentInfo.Resource.ResourceClass.Name,
										Type = asc.ResourceAssignmentInfo.Resource.Type.ToString()
									};
									ascdto.Resource = ascresdto;

									List<ResourceFieldsDto> rsfdto = new();
									if (asc.ResourceAssignmentInfo.ResourceFields != null)
									{
										foreach (KeyValuePair<IResourceFieldDefinition, IResourceFieldStandardValue> item in asc.ResourceAssignmentInfo.ResourceFields)
										{
											ResourceFieldsDto rfdto = new()
											{
												Key = item.Key != null ? item.Key.Name : string.Empty,
												Value = item.Value != null ? item.Value.Value.ToString() : string.Empty
											};
											rsfdto.Add(rfdto);
										}
									}

									ascdto.ResourceFields = rsfdto;

									if (asc.Spread != null)
									{
										foreach (KeyValuePair<TimeFrame, EBS.Number> s in asc.Spread.ActualDistribution)
										{
											SpreadDto sdto = new()
											{
												Year = s.Key.Year,
												Month = s.Key.Month,
												Value = s.Value.ToString()
											};
											ascspread.Add(sdto);
										}
									}

									ascdto.Spread = ascspread;
									asclistdto.Add(ascdto);
								}
							}

							madto.AssociatedCosts = asclistdto;
							materialAssignments.Add(madto);
							ma.Close();
						}

						tdto.MaterialAssignments = materialAssignments;

						whichvar = "summary fields";
						List<SummaryFieldsDto> summaryFields = new();
						foreach (KeyValuePair<SummaryFieldDefinition, SummaryFieldValue?> sf in t.SummaryFields)
						{
							SummaryFieldsDto sfdto = new();
							System.Diagnostics.Debug.WriteLine("sf.Key: " + sf.Key.Name);
							System.Diagnostics.Debug.WriteLine("sf.Value: " + sf.Value);
							sfdto.Key = sf.Key.Name;
							sfdto.Value = sf.Value.ToString();
							summaryFields.Add(sfdto);
						}

						tdto.SummaryFields = summaryFields;

						whichvar = "travel";
						// travel
						List<TravelsDto> trvlDto = new();
						foreach (TravelAssignment trv in t.Travels.Items())
						{
							TravelsDto trvl = new()
							{
								Id = trv.Id.ToString(),
								Name = trv.Name,
								Description = trv.Description,
								Destination = trv.DestinationName,
								DestinationDescription = trv.DestinationDescription,
								Comments = trv.Comments,
								People = trv.People,
								Days = trv.Days.ToString(),
								Trips = trv.Trips,
								TripCost = trv.TripCost.ToString(),
								TotalCost = trv.TotalCost.ToString()
							};

							ResourceAssignmentDto resassign = new();
							if (trv.ResourceAssignmentInfo.Resource != null)
							{
								resassign.Name = trv.ResourceAssignmentInfo.Resource.Name;
								resassign.InfoDescription = trv.ResourceAssignmentInfo.Resource.Description;
							}

							List<ResourceFieldsDto> rsfdto = new();
							if (trv.ResourceAssignmentInfo.ResourceFields != null)
							{
								foreach (KeyValuePair<IResourceFieldDefinition, IResourceFieldStandardValue> item in trv.ResourceAssignmentInfo.ResourceFields)
								{
									ResourceFieldsDto rfdto = new()
									{
										Key = item.Key != null ? item.Key.Name : string.Empty,
										Value = item.Value != null ? item.Value.Value.ToString() : string.Empty
									};
									rsfdto.Add(rfdto);
								}
							}

							resassign.ResourceFields = rsfdto;

							resassign.SpreadCurve = trv.Spread.Curve != null ? trv.Spread.Curve.Name : string.Empty;
							resassign.StartDate = trv.Spread.StartDate != null ? trv.Spread.StartDate.ToString() : string.Empty;
							resassign.EndDate = trv.Spread.EndDate != null ? trv.Spread.EndDate.ToString() : string.Empty;
							resassign.Amount = trv.Spread.Amount != null ? trv.Spread.Amount.ToString() : "0";

							List<SpreadDto> trvspread = new();
							if (trv.Spread != null)
							{
								foreach (KeyValuePair<TimeFrame, EBS.Number> s in trv.Spread.Distribution)
								{
									SpreadDto sdto = new()
									{
										Year = s.Key.Year,
										Month = s.Key.Month,
										Value = s.Value.ToString()
									};
									trvspread.Add(sdto);
								}
							}

							resassign.Spread = trvspread;

							trvl.ResourceAssignment = resassign;

							List<TravelExpenseDto> trexdto = new();
							if (trv.Expenses != null)
							{
								foreach (TravelAssignment.Expense item in trv.Expenses.Items())
								{
									TravelExpenseDto trdto = new()
									{
										Name = item.Definition.Name,
										Qty = item.Quantity.ToString(),
										Rate = item.Rate.ToString(),
										Cost = item.Cost.ToString()
									};
									trexdto.Add(trdto);
								}
							}

							trvl.Expenses = trexdto;

							trvlDto.Add(trvl);
						}

						tdto.Travels = trvlDto;

						t.Close();

						tasks.Add(tdto);
					}
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Error with " + whichvar);
					tasks[tasks.Count].Id = "Error with " + whichvar + " - " + ex.Message;
				}

				pr.Close();
			}

			return tasks;
		}
	}
}
