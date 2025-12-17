// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Transactions;
    using System.Web.Mvc;
    using GenBOE.ActionLogic.BLL;
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.Metrics;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.Common.Interfaces;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using GenBOE.Web.Common;
    using GenBOE.Web.ModelView;
    using IES.Common;
    using IES.Common.Exceptions;

    public class FindReplaceController : GenBOEController
    {
        private Logger _log = new Logger(typeof(FindReplaceController));

        // the Find Replace DTO data loader
        private IFindReplaceDTODataLoader findReplaceDataLoader;

        // BOE Task Element Mediator
        private IBoeTaskElementMediator taskElementMediator;

        // Travel Task Element Loader
        private ITravelDTODataLoader travelLoader;

        private BOEStateMachine _BOEStateMachine = null;
        private BoeMediator _BoeMediator = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inSecurityAccess">Access to the security APIs</param>
        public FindReplaceController(ISecurityAccess inSecurityAccess,
            CommonDataMapper inCommonDataMapper,
            IFindReplaceDTODataLoader inFindReplaceDTOLoader,
            SiteMasterUtilities inSiteMasterUtilities,
            BOEStateMachine inBOEStateMachine,
            BoeMediator inBoeMediator,
            SystemMetrics inSystemMetrics,
            IFullObjectFactory factory,
            IBoeTaskElementMediator inTaskElementMediator,
            ITravelDTODataLoader inTravelLoader,
            IUserDTODataLoader userLoader,
            IPermissionsDTODataLoader permissionLoader,
            IGenBOEControllerLogic inControllerLogic)
            : base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, userLoader, permissionLoader, inControllerLogic)
        {
            this.findReplaceDataLoader = inFindReplaceDTOLoader;
            _BOEStateMachine = inBOEStateMachine;
            _BoeMediator = inBoeMediator;
            this.taskElementMediator = inTaskElementMediator;
            this.travelLoader = inTravelLoader;
        }

        /// <summary>
        /// This function calls the FindReplace view
        /// </summary>
        /// <param name="workspace">the workspace the FindReplace is associated with</param>
        /// <returns>returns the FindReplace view </returns>
        [HttpGet]
		public ViewResult Index(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_FIND_REPLACE, SecurityPage.FindReplace, SecurityAuthorization.Read, ws, null);
            
            // Perform Action
            ViewResult toReturn = GetMasterView(WebConstants.VIEW_FIND_REPLACE, workspace);

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_FIND_REPLACE, sw);
            return toReturn;
        }

        #region Display

        [ChildActionOnly, HttpGet]
		public ViewResult DisplayFindReplace(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_FIND_REPLACE, SecurityPage.FindReplace, SecurityAuthorization.Read, ws, null);

            ViewData["WorkspaceID"] = ws.Id;

            // Perform Action
            ViewResult toReturn = View(WebConstants.VIEW_FIND_REPLACE_FORM);

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_DISPLAY_FIND_REPLACE, sw);
            return toReturn;
        }

        #endregion Display

        #region Actions

		[HttpPost]
        public ViewResult PageFindResults(string workspace, FindReplaceResultsModelView findReplaceResults)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_PAGE_FIND_REPLACE_RESULTS, SecurityPage.FindReplace, SecurityAuthorization.Read, ws, null);

            if (findReplaceResults == null)
            {
                throw new ArgumentNullException(nameof(findReplaceResults));
            }

            findReplaceResults.FindResults = new Collection<GenBOE.Web.ModelView.FindReplaceResult>();

            for (int i = findReplaceResults.StartArrayIndex; i <= findReplaceResults.EndArrayIndex; i++)
            {
                findReplaceResults.FindResults.Add(findReplaceResults.PagedIndexes[i]);
            }

            ViewResult toReturn = View(WebConstants.VIEW_FIND_REPLACE_RESULTS, findReplaceResults);

            FinalizeAction(_log, WebConstants.ACTION_PAGE_FIND_REPLACE_RESULTS, sw);
            return toReturn;
        }

		/// <summary>
		/// Performs a search for find/replace text
		/// </summary>
		/// <param name="workspace"></param>
		/// <param name="findParams"></param>
		/// <returns></returns>
		[HttpPost]
		public ViewResult FindAllforReplace(string workspace, FindReplaceModelView findParams)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_FIND_REPLACE_RESULTS, SecurityPage.FindReplace, SecurityAuthorization.Read, ws, null);

            if (findParams == null)
            {
                throw new ArgumentNullException(nameof(findParams));
            }

            ViewResult toReturn = null;

            /** Valid Model Check */
            if (ModelState.IsValid)
            {
                FindReplaceDTO findDTO = new FindReplaceDTO()
                {
                    FindText = findParams.FindText,
                    ReplaceText = findParams.ReplaceText,
                    WorkspaceID = ws.Id
                };

                Collection<FindReplaceDTO> results = this.getFindReferences(findDTO, ws.Id);

                FindReplaceResultsModelView modelView = new FindReplaceResultsModelView(results);
                modelView.WorkspaceID = ws.Id;
                modelView.WorkspaceName = workspace;


                // Return the partial view
                toReturn = View(WebConstants.VIEW_FIND_REPLACE_RESULTS, modelView);
            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_FIND_REPLACE_RESULTS, sw);
            return toReturn;
        }

		[HttpPost]
		public virtual JsonResult SaveReplacedValues(string workspace, FindReplaceResultsModelView ReplacedTextModelViews)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_REPLACED_VALUES, SecurityPage.FindReplace, SecurityAuthorization.ReadUpdate, ws, null);

            if (ReplacedTextModelViews == null)
            {
                throw new ArgumentNullException(nameof(ReplacedTextModelViews));
            }

            Collection<FindReplaceDTO> findReplaceDTOsToSave = new Collection<FindReplaceDTO>();

            foreach (GenBOE.Web.ModelView.FindReplaceResult replaceResult in ReplacedTextModelViews.FindResults)
            {
                FindReplaceDTO tempReplaceDTO = new FindReplaceDTO();

                tempReplaceDTO.BOEId = replaceResult.BOEId;
                tempReplaceDTO.TaskElementID = replaceResult.TaskElementID;
                tempReplaceDTO.WorkspaceID = replaceResult.WorkspaceID;
                tempReplaceDTO.FieldName = replaceResult.FieldName;
                tempReplaceDTO.TextAfterReplace = replaceResult.TextAfterReplace;
                tempReplaceDTO.toSave = replaceResult.toSave;
                tempReplaceDTO.FindReplaceTaskElementType = replaceResult.FindReplaceTaskElementType;
                findReplaceDTOsToSave.Add(tempReplaceDTO);
            }

            if (findReplaceDTOsToSave != null)
            {
                Dictionary<int, Collection<FieldChanged>> fieldsChangedByBoeId = new Dictionary<int, Collection<FieldChanged>>();

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    fieldsChangedByBoeId = this.saveReplace(findReplaceDTOsToSave);

                    ICollection<FullBoe> boes = this.Factory.CreateFullBoes(findReplaceDTOsToSave.Select(x => x.BOEId).ToCollection());

                    foreach (KeyValuePair<int, Collection<FieldChanged>> boeChanged in fieldsChangedByBoeId)
                    {
                        FullBoe boeAdjusted = boes.First(x => x.Id == boeChanged.Key);

                        if (fieldsChangedByBoeId.ContainsKey(boeAdjusted.Id))
                        {
                            if (boeAdjusted.State == BOEState.AwaitingApproval || boeAdjusted.State == BOEState.Approved || boeAdjusted.State == BOEState.DraftLocked)
                            {
                                // for AwaitingApproval and Approved, move back to draft and then send email if save worked
                                // move the BOE back to DRAFT
                                BOEState newBOEState = BOEState.Draft;
                                boeAdjusted.State = newBOEState;
                                boeAdjusted.Updateable = UpdateType.Upsert;

                                string errorMessage;
                                if (_BOEStateMachine.PerformStateTransitionValidation(this.Factory.CreateFullBoe(boeAdjusted), ws, boeAdjusted.State, newBOEState, out errorMessage))
                                {
                                    _BoeMediator.MediatedSave(this.Factory.CreateFullWorkspace(boeAdjusted.WorkspaceID), boeAdjusted);

                                    // save of BOE worked .. perform transition steps and send email
                                    _BOEStateMachine.PerformStateTransitionAction(this.Factory.CreateFullBoe(boeAdjusted), ws, boeAdjusted.State, newBOEState);
                                }
                                else
                                {
                                    // we can't move the BOE back to DRAFT for some reason ... abort
                                    // pull this error message from the state machine itself
                                    throw new ValidationException(errorMessage);
                                }
                            }
                        }
                        else
                        {
                            // log a warning that we changed fields in a BOE but were unable to find the changed fields struct 
                            // that should correspond to the changed fields
                            _log.Warn("Unable to locate BOE Id " + boeAdjusted.Id + " (description = " + boeAdjusted.Description + ") " +
                                      "in the list of changes for the find/replace.");
                        }
                    } // end foreach

                    scope.Complete();

                } // end transaction

                // transaction worked, send emails
            }

            JsonResult toReturn = Json(new { Status = true });

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_SAVE_REPLACED_VALUES, sw);
            return toReturn;
        }
        #endregion Actions

        private Collection<FindReplaceDTO> getFindReferences(FindReplaceDTO inFindParams, int inWorkspaceId)
        {
            if (inFindParams == null)
            {
                throw new ArgumentNullException(nameof(inFindParams));
            }

            Collection<FindReplaceDTO> allFoundDTOs = findReplaceDataLoader.getFindReferences(inFindParams, inWorkspaceId);

            ICollection<FullBoe> boes = this.Factory.CreateFullBoes(allFoundDTOs.Select(x => x.BOEId).ToCollection());

            foreach (FindReplaceDTO FindDTO in allFoundDTOs)
            {
                if (FindDTO.BOEId == -1)
                {
                    throw new ArgumentException("BOEID not found");
                }

                FindDTO.ReplaceText = inFindParams.ReplaceText;
                FindDTO.FindText = inFindParams.FindText;

                FullBoe tempBOE = boes.First(x => x.Id == FindDTO.BOEId);

                if (tempBOE.WBSID != null)
                {
                    FindDTO.WBSInfo = IES.Common.Utilities.FormatNumberTitleString(tempBOE.Wbs.WbsNumber, tempBOE.Wbs.WbsTitle, " - ");
                }

                if (tempBOE.CLINID.HasValue)
                {
                    FullClin clin = this.Factory.CreateFullClin(tempBOE.CLINID.Value);
                    FindDTO.ClinInfo = IES.Common.Utilities.FormatNumberTitleString(clin.ClinNumber, clin.ClinTitle, " - ");
                }

                FindDTO.BOETitle = tempBOE.Title;

                int allowedLength = 0;

                if (FindDTO.FieldEnum == 1)
                {
                    FindDTO.FieldName = "BOE Description";
                    allowedLength = ValidationConstants.MAX_BOE_DESC_LENGTH;
                }
                else if (FindDTO.FieldEnum == 2)
                {
                    FindDTO.FieldName = "Sources of Data";
                    allowedLength = ValidationConstants.MAX_SOURCES_OF_DATA_LENGTH;
                }
                else if (FindDTO.FieldEnum == 3)
                {
                    FindDTO.FieldName = "Task Title";
                    allowedLength = ValidationConstants.MAX_TASK_TITLE_LENGTH;
                }
                else if (FindDTO.FieldEnum == 4)
                {
                    FindDTO.FieldName = "Task Description";
                    allowedLength = ValidationConstants.MAX_TASK_DESC_LENGTH;
                }
                else if (FindDTO.FieldEnum == 5)
                {
                    FindDTO.FieldName = "MOQ Text";
                    allowedLength = ValidationConstants.MAX_MOQ_TEXT_LENGTH;
                }
                else if (FindDTO.FieldEnum == 6)
                {
                    FindDTO.FieldName = "BOE Title";
                    allowedLength = ValidationConstants.MAX_BOE_TITLE_LENGTH;
                }
                else
                {
                    throw new ArgumentException("Field Name " + FindDTO.FieldName + " not found.");
                }

                FindDTO.CenteredText = getCenteredString(FindDTO.FoundText, FindDTO.FindText);
                FindDTO.TextBeforeFind = string.Empty;
                FindDTO.TextAfterFind = string.Empty;
                if (!FindDTO.CenteredText.StartsWith(FindDTO.FindText, StringComparison.CurrentCultureIgnoreCase))
                {
                    FindDTO.TextBeforeFind = FindDTO.CenteredText.Substring(0, FindDTO.CenteredText.ToLower().IndexOf(FindDTO.FindText.ToLower()));
                }

                if (!FindDTO.CenteredText.EndsWith(FindDTO.FindText, StringComparison.CurrentCultureIgnoreCase))
                {
                    FindDTO.TextAfterFind = FindDTO.CenteredText.Substring(FindDTO.CenteredText.ToLower().IndexOf(FindDTO.FindText.ToLower()) + FindDTO.FindText.Length, FindDTO.CenteredText.Length - (FindDTO.CenteredText.ToLower().IndexOf(FindDTO.FindText.ToLower()) + FindDTO.FindText.Length));
                }

                FindDTO.TextAfterReplace = Regex.Replace(FindDTO.FoundText, FindDTO.FindText, FindDTO.ReplaceText, RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);

                FindDTO.tooLong = false;
                if (FindDTO.TextAfterReplace.Length > allowedLength)
                {
                    FindDTO.tooLong = true;
                }

                // the found text should really be the string that the findText matched but in the case the user entered
                FindDTO.FoundText = FindDTO.CenteredText.Substring(FindDTO.TextBeforeFind.Length, FindDTO.FindText.Length);

            }

            return allFoundDTOs;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inReplaceDTOs"></param>
        /// <returns>dictionary of boe id's to the fields that were changed for that BOE</returns>
        private Dictionary<int, Collection<FieldChanged>> saveReplace(Collection<FindReplaceDTO> inReplaceDTOs)
        {
            if (inReplaceDTOs == null)
            {
                throw new ArgumentNullException(nameof(inReplaceDTOs));
            }
            Dictionary<int, Collection<FieldChanged>> toReturn = new Dictionary<int, Collection<FieldChanged>>();
            Collection<FieldChanged> changed = new Collection<FieldChanged>();

            foreach (FindReplaceDTO replaceDTO in inReplaceDTOs)
            {
                if (replaceDTO.toSave == true)
                {
                    //Need this because taskElement loader has no save for just one task element
                    Collection<BoeTaskElementDTO> taskElementCollectionToSave = new Collection<BoeTaskElementDTO>();
                    Collection<TravelDTO> travelCollectionToSave = new Collection<TravelDTO>();

                    if (replaceDTO.FieldName == "BOE Title")
                    {
                        FullBoe boeToSave = this.Factory.CreateFullBoe(replaceDTO.BOEId);
                        string originalValue = boeToSave.Title;
                        boeToSave.Title = replaceDTO.TextAfterReplace;
                        boeToSave.Updateable = UpdateType.Upsert;
                        FullWorkspace fullWorkspace = this.Factory.CreateFullWorkspace(boeToSave.WorkspaceID);
                        this._BoeMediator.MediatedSave(fullWorkspace, boeToSave);

                        changed = _AddBOEToFieldChangedCollection(toReturn, changed, replaceDTO, boeToSave.Id, originalValue);
                    }
                    else if (replaceDTO.FieldName == "BOE Description")
                    {
                        FullBoe boeToSave = this.Factory.CreateFullBoe(replaceDTO.BOEId);
                        string originalValue = boeToSave.Description;
                        boeToSave.Description = replaceDTO.TextAfterReplace;
                        boeToSave.Updateable = UpdateType.Upsert;
                        FullWorkspace fullWorkspace = this.Factory.CreateFullWorkspace(boeToSave.WorkspaceID);
                        this._BoeMediator.MediatedSave(fullWorkspace, boeToSave);

                        changed = _AddBOEToFieldChangedCollection(toReturn, changed, replaceDTO, boeToSave.Id, originalValue);
                    }
                    else if (replaceDTO.FieldName == "Sources of Data")
                    {
                        FullBoe boeToSave = this.Factory.CreateFullBoe(replaceDTO.BOEId);
                        string originalValue = boeToSave.DataSource;
                        boeToSave.DataSource = replaceDTO.TextAfterReplace;
                        boeToSave.Updateable = UpdateType.Upsert;
                        FullWorkspace fullWorkspace = this.Factory.CreateFullWorkspace(boeToSave.WorkspaceID);
                        this._BoeMediator.MediatedSave(fullWorkspace, boeToSave);

                        changed = _AddBOEToFieldChangedCollection(toReturn, changed, replaceDTO, boeToSave.Id, originalValue);
                    }
                    else if (replaceDTO.FieldName == "Task Title")
                    {
                        switch (replaceDTO.FindReplaceTaskElementType)
                        {
                            case FindReplaceElementType.BOE:
                                FullBoe boeToSave = this.Factory.CreateFullBoe(replaceDTO.BOEId);
                                FullWorkspace fullWorkspace = this.Factory.CreateFullWorkspace(boeToSave.WorkspaceID);
                                BoeTaskElementDTO taskElementToSave = this.Factory.CreateTaskElement(replaceDTO.TaskElementID, fullWorkspace.DecimalPrecision, fullWorkspace.CostDecimalPrecision);
                                string originalValue = taskElementToSave.TaskTitle;
                                taskElementToSave.TaskTitle = replaceDTO.TextAfterReplace;
                                taskElementToSave.Updateable = UpdateType.Upsert;
                                taskElementCollectionToSave.Add(taskElementToSave);

                                FullBoe boe = this.Factory.CreateFullBoe(taskElementToSave.BoeID);
                                taskElementMediator.MediatedSaveTaskElements(taskElementCollectionToSave, boe.Workspace);

                                changed = _AddTaskElementToFieldChangedCollection(toReturn, changed, replaceDTO, taskElementToSave.BoeID, originalValue);
                                break;
                            case FindReplaceElementType.Travel:
                                TravelDTO travelElementToSave = this.Factory.CreateTravel(replaceDTO.TaskElementID);
                                originalValue = travelElementToSave.TaskTitle;
                                travelElementToSave.TaskTitle = replaceDTO.TextAfterReplace;
                                travelElementToSave.Updateable = UpdateType.Upsert;
                                travelCollectionToSave.Add(travelElementToSave);
                                travelLoader.SaveTravels(travelCollectionToSave);
                                changed = _AddTaskElementToFieldChangedCollection(toReturn, changed, replaceDTO, travelElementToSave.BoeID, originalValue);
                                break;
                            default:
                                break;
                        }

                    }
                    else if (replaceDTO.FieldName == "Task Description")
                    {
                        switch (replaceDTO.FindReplaceTaskElementType)
                        {
                            case FindReplaceElementType.BOE:
                                FullBoe boeToSave = this.Factory.CreateFullBoe(replaceDTO.BOEId);
                                FullWorkspace fullWorkspace = this.Factory.CreateFullWorkspace(boeToSave.WorkspaceID);
                                BoeTaskElementDTO taskElementToSave = this.Factory.CreateTaskElement(replaceDTO.TaskElementID, fullWorkspace.DecimalPrecision, fullWorkspace.CostDecimalPrecision);
                                taskElementToSave.Description = replaceDTO.TextAfterReplace;
                                string originalValue = taskElementToSave.Description;
                                taskElementToSave.Updateable = UpdateType.Upsert;
                                taskElementCollectionToSave.Add(taskElementToSave);

                                FullBoe boe = this.Factory.CreateFullBoe(taskElementToSave.BoeID);
                                taskElementMediator.MediatedSaveTaskElements(taskElementCollectionToSave, boe.Workspace);

                                changed = _AddTaskElementToFieldChangedCollection(toReturn, changed, replaceDTO, taskElementToSave.BoeID, originalValue);
                                break;
                            case FindReplaceElementType.Travel:
                                TravelDTO travelElementToSave = this.Factory.CreateTravel(replaceDTO.TaskElementID);
                                travelElementToSave.Description = replaceDTO.TextAfterReplace;
                                originalValue = travelElementToSave.Description;
                                travelElementToSave.Updateable = UpdateType.Upsert;
                                travelCollectionToSave.Add(travelElementToSave);
                                travelLoader.SaveTravels(travelCollectionToSave);
                                changed = _AddTaskElementToFieldChangedCollection(toReturn, changed, replaceDTO, travelElementToSave.BoeID, originalValue);
                                break;
                            default:
                                break;
                        }

                    }
                    else if (replaceDTO.FieldName == "MOQ Text")
                    {
                        switch (replaceDTO.FindReplaceTaskElementType)
                        {
                            case FindReplaceElementType.BOE:
                                FullBoe boeToSave = this.Factory.CreateFullBoe(replaceDTO.BOEId);
                                FullWorkspace fullWorkspace = this.Factory.CreateFullWorkspace(boeToSave.WorkspaceID);
                                BoeTaskElementDTO taskElementToSave = this.Factory.CreateTaskElement(replaceDTO.TaskElementID, fullWorkspace.DecimalPrecision, fullWorkspace.CostDecimalPrecision);
                                string originalValue = taskElementToSave.MOQText;                                
                                taskElementToSave.MOQText = replaceDTO.TextAfterReplace;
                                taskElementToSave.Updateable = UpdateType.Upsert;
                                taskElementCollectionToSave.Add(taskElementToSave);

                                FullBoe boe = this.Factory.CreateFullBoe(taskElementToSave.BoeID);
                                taskElementMediator.MediatedSaveTaskElements(taskElementCollectionToSave, boe.Workspace);

                                changed = _AddTaskElementToFieldChangedCollection(toReturn, changed, replaceDTO, taskElementToSave.BoeID, originalValue);
                                break;
                            case FindReplaceElementType.Travel: // no Travel MOQ Text
                            default:
                                break;
                        }

                    }
                    else
                    {
                        throw new ArgumentException("Field Name " + replaceDTO.FieldName + " not found.");
                    }

                }
            }

            return toReturn;
        }

        private static Collection<FieldChanged> _AddTaskElementToFieldChangedCollection(Dictionary<int, Collection<FieldChanged>> toReturn, Collection<FieldChanged> changed, FindReplaceDTO replaceDTO, int boeId, string originalValue)
        {
            if (toReturn.ContainsKey(boeId))
            {
                changed = toReturn[boeId];
            }
            else
            {
                changed = new Collection<FieldChanged>();
                toReturn.Add(boeId, changed);
            }

            changed.Add(new FieldChanged { Field = replaceDTO.FieldName, OldValue = originalValue, NewValue = replaceDTO.TextAfterReplace });
            return changed;
        }

        private static Collection<FieldChanged> _AddBOEToFieldChangedCollection(Dictionary<int, Collection<FieldChanged>> toReturn, Collection<FieldChanged> changed, FindReplaceDTO replaceDTO, int boeId, string originalValue)
        {
            if (toReturn.ContainsKey(boeId))
            {
                changed = toReturn[boeId];
            }
            else
            {
                changed = new Collection<FieldChanged>();
                toReturn.Add(boeId, changed);
            }
            changed.Add(new FieldChanged { Field = replaceDTO.FieldName, OldValue = originalValue, NewValue = replaceDTO.TextAfterReplace });
            return changed;
        }

        /// <summary>
        /// Takes in a string and key with the key being a set of text in the string.
        /// </summary>
        /// <param name="inText, inFind"></param>
        /// <returns>A string centered around the inFind key found</returns>
        private string getCenteredString(string inText, string inFind)
        {

            if (inText.Length > 200)
            {
                int firstOccurrence = inText.ToLower().IndexOf(inFind.ToLower());
                int endOfFirstOccurrence = firstOccurrence + inFind.Length;
                string firstHalf = null;
                string secondHalf = null;
                if (endOfFirstOccurrence - 100 > 0)
                {
                    firstHalf = inText.Substring(endOfFirstOccurrence - 100, 100);
                }
                else
                {
                    firstHalf = inText.Substring(0, endOfFirstOccurrence);

                }

                if (inText.Length - endOfFirstOccurrence > 100)
                {
                    secondHalf = inText.Substring(endOfFirstOccurrence, 100);
                }
                else
                {
                    secondHalf = inText.Substring(endOfFirstOccurrence, inText.Length - endOfFirstOccurrence);
                }

                return firstHalf + secondHalf;

            }
            else
            {
                return inText;
            }
        }
    }
}
