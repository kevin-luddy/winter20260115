using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using GenBOE.Common;
using GenBOE.Dtos;

namespace GenBOE.DataBridge.DTO
{
    public class FindReplaceDTODataMapper : IFindReplaceDTODataMapper
    {

        private const int MAX_BOE_DESC_LENGTH = 250000;
        private const int MAX_BOE_TITLE_LENGTH = 100;
        private const int MAX_SOURCES_OF_DATA_LENGTH = 2000;
        private const int MAX_TASK_DESC_LENGTH = 250000;
        private const int MAX_MOQ_TEXT_LENGTH = 250000;
        private const int MAX_TASK_TITLE_LENGTH = 100;

        private Logger _log = new Logger(typeof(FindReplaceDTODataMapper));

        // the Find Replace DTO data loader
        private IFindReplaceDTODataLoader _FindReplaceDataLoader;

        // the CLIN DTO Mapper
        private IClinDTODataLoader clinLoader;

        // the CLIN DTO Mapper
        private IWbsDTODataMapper _WBSDTODataMapper;

        // the BOE data Mapper
        private IInternalBoeDTODataMapper _BoeDataMapper;

        // BOE Task Element Mapper
        private IInternalBoeTaskElementDTODataMapper _BoeTaskElementMapper;

        // Material Task Element Loader
        private IMaterialDTODataLoader _MaterialLoader;

        // Travel Task Element Mapper
        private ITravelDTODataMapper _TravelMapper;

        // ODC Task Element Mapper
        private IOtherDirectCostDTODataMapper _ODCMapper;

        /// <summary>
        ///
        /// </summary>
        /// <param name="inFindReplaceDataLoader">Find Replace DTO data loader</param>
        /// <param name="inCacheDataLoader"> Cache Data loader</param>
        public FindReplaceDTODataMapper(
            IFindReplaceDTODataLoader inFindReplaceDataLoader,
            IWbsDTODataMapper inWBSDTODataMapper,
            IBoeDTODataMapper inBOEDataMapper,
            IBoeTaskElementDTODataMapper inBoeTaskElementMapper,
            IMaterialDTODataLoader inMaterialLoader,
            ITravelDTODataMapper inTravelMapper,
            IOtherDirectCostDTODataMapper inODCMapper,
            IClinDTODataLoader clinLoader
                )
        {
            _FindReplaceDataLoader = inFindReplaceDataLoader;
            _WBSDTODataMapper = inWBSDTODataMapper;
            _BoeDataMapper = inBOEDataMapper as IInternalBoeDTODataMapper;
            _BoeTaskElementMapper = inBoeTaskElementMapper as IInternalBoeTaskElementDTODataMapper;
            _MaterialLoader = inMaterialLoader;
            _TravelMapper = inTravelMapper;
            _ODCMapper = inODCMapper;
            this.clinLoader = clinLoader;
        }


        virtual public Collection<FindReplaceDTO> getFindReferences(FindReplaceDTO inFindParams, int inWorkspaceId)
        {
            if (inFindParams == null) throw new ArgumentNullException("inFindParams");

            Collection<FindReplaceDTO> allFoundDTOs = _FindReplaceDataLoader.getFindReferences(inFindParams, inWorkspaceId);

            foreach (FindReplaceDTO FindDTO in allFoundDTOs)
            {
                FindDTO.ReplaceText = inFindParams.ReplaceText;
                FindDTO.FindText = inFindParams.FindText;
                BoeDTO tempBOE = new BoeDTO();

                if (FindDTO.BOEId == -1) throw new ArgumentException("BOEID not found");

                tempBOE = _BoeDataMapper.GetBoeDataByBoeID(FindDTO.BOEId);

                if (tempBOE.WBSID != null)
                {
                    FindDTO.WBSInfo = _WBSDTODataMapper.GetWbsByWbsID((int)tempBOE.WBSID).WbsNumber + " - " + _WBSDTODataMapper.GetWbsByWbsID((int)tempBOE.WBSID).WbsTitle;
                }
                if (tempBOE.CLINID.HasValue)
                {
                    ClinDTO clin = this.clinLoader.GetById(tempBOE.CLINID.Value);

                    FindDTO.ClinInfo = clin.ClinNumber + " - " + clin.ClinTitle;
                }

                FindDTO.BOETitle = tempBOE.Title;

                int allowedLength = 0;

                if (FindDTO.FieldEnum == 1)
                {
                    FindDTO.FieldName = "BOE Description";
                    allowedLength = MAX_BOE_DESC_LENGTH;

                }
                else if (FindDTO.FieldEnum == 2)
                {
                    FindDTO.FieldName = "Sources of Data";
                    allowedLength = MAX_SOURCES_OF_DATA_LENGTH;

                }
                else if (FindDTO.FieldEnum == 3)
                {
                    FindDTO.FieldName = "Task Title";
                    allowedLength = MAX_TASK_DESC_LENGTH;
                }
                else if (FindDTO.FieldEnum == 4)
                {
                    FindDTO.FieldName = "Task Description";
                    allowedLength = MAX_MOQ_TEXT_LENGTH;

                }
                else if (FindDTO.FieldEnum == 5)
                {
                    FindDTO.FieldName = "MOQ Text";
                    allowedLength = MAX_TASK_TITLE_LENGTH;
                }
                else if (FindDTO.FieldEnum == 6)
                {
                    FindDTO.FieldName = "BOE Title";
                    allowedLength = MAX_BOE_TITLE_LENGTH;

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

                FindDTO.TextAfterReplace = Regex.Replace(FindDTO.FoundText, FindDTO.FindText, FindDTO.ReplaceText, RegexOptions.IgnoreCase);

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
        public Dictionary<int, Collection<FieldChanged>> saveReplace(Collection<FindReplaceDTO> inReplaceDTOs)
        {

            if (inReplaceDTOs == null) throw new ArgumentNullException("inReplaceDTOs");
            Dictionary<int, Collection<FieldChanged>> toReturn = new Dictionary<int, Collection<FieldChanged>>();
            Collection<FieldChanged> changed = new Collection<FieldChanged>();

            foreach (FindReplaceDTO replaceDTO in inReplaceDTOs)
            {
                if (replaceDTO.toSave == true)
                {
                    //Need this because taskElement mapper has no save for just one task element
                    Collection<BoeTaskElementDTO> taskElementCollectionToSave = new Collection<BoeTaskElementDTO>();
                    Collection<MaterialDTO> materialCollectionToSave = new Collection<MaterialDTO>();
                    Collection<TravelDTO> travelCollectionToSave = new Collection<TravelDTO>();
                    Collection<OtherDirectCostDTO> odcCollectionToSave = new Collection<OtherDirectCostDTO>();

                    if (replaceDTO.FieldName == "BOE Title")
                    {
                        BoeDTO boeToSave = _BoeDataMapper.GetBoeDataByBoeID(replaceDTO.BOEId);
                        string originalValue = boeToSave.Title;
                        boeToSave.Title = replaceDTO.TextAfterReplace;
                        boeToSave.Updateable = UpdateType.Upsert;
                        _BoeDataMapper.Save(boeToSave);

                        changed = _AddBOEToFieldChangedCollection(toReturn, changed, replaceDTO, boeToSave.Id, originalValue);
                    }
                    else if (replaceDTO.FieldName == "BOE Description")
                    {
                        BoeDTO boeToSave = _BoeDataMapper.GetBoeDataByBoeID(replaceDTO.BOEId);
                        string originalValue = boeToSave.Description;
                        boeToSave.Description = replaceDTO.TextAfterReplace;
                        boeToSave.Updateable = UpdateType.Upsert;
                        _BoeDataMapper.Save(boeToSave);

                        changed = _AddBOEToFieldChangedCollection(toReturn, changed, replaceDTO, boeToSave.Id, originalValue);
                    }
                    else if (replaceDTO.FieldName == "Sources of Data")
                    {
                        BoeDTO boeToSave = _BoeDataMapper.GetBoeDataByBoeID(replaceDTO.BOEId);
                        string originalValue = boeToSave.DataSource;
                        boeToSave.DataSource = replaceDTO.TextAfterReplace;
                        boeToSave.Updateable = UpdateType.Upsert;
                        _BoeDataMapper.Save(boeToSave);

                        changed = _AddBOEToFieldChangedCollection(toReturn, changed, replaceDTO, boeToSave.Id, originalValue);
                    }
                    else if (replaceDTO.FieldName == "Task Title")
                    {
                        switch (replaceDTO.FindReplaceTaskElementType)
                        {
                            case FindReplaceElementType.BOE:
                                BoeTaskElementDTO taskElementToSave = _BoeTaskElementMapper.GetBoeTaskElementByBoeTaskElementID(replaceDTO.TaskElementID);
                                string originalValue = taskElementToSave.TaskTitle;
                                taskElementToSave.TaskTitle = replaceDTO.TextAfterReplace;
                                taskElementToSave.Updateable = UpdateType.Upsert;
                                taskElementCollectionToSave.Add(taskElementToSave);
                                if (ConfigurationUtilities.GetAppSetting<bool>("TrackMOQEquation"))
                                    _log.Warn("Track MOQ Equation - FindReplaceDTODataMapper.saveReplace() Line 237 - Field Name is Task Title - Call Mapper SaveBoeTaskElements  Task Element:" + taskElementToSave + " MOQ Equation:" + taskElementToSave.MOQHoursEquation);
                                
                                _BoeTaskElementMapper.SaveBoeTaskElements(taskElementCollectionToSave);

                                changed = _AddTaskElementToFieldChangedCollection(toReturn, changed, replaceDTO, taskElementToSave.BoeID, originalValue);
                                break;
                            case FindReplaceElementType.Material:
                                MaterialDTO materialTaskElementToSave = _MaterialLoader.GetMaterialDTOByMaterialID(replaceDTO.TaskElementID);
                                originalValue = materialTaskElementToSave.TaskTitle;
                                materialTaskElementToSave.TaskTitle = replaceDTO.TextAfterReplace;
                                materialTaskElementToSave.Updateable = UpdateType.Upsert;
                                materialCollectionToSave.Add(materialTaskElementToSave);
                                _MaterialLoader.SaveMaterials(materialCollectionToSave);
                                changed = _AddTaskElementToFieldChangedCollection(toReturn, changed, replaceDTO, materialTaskElementToSave.BoeID, originalValue);
                                break;
                            case FindReplaceElementType.ODC:
                                OtherDirectCostDTO odcElementToSave = _ODCMapper.GetODCByODCID(replaceDTO.TaskElementID);
                                originalValue = odcElementToSave.TaskTitle;
                                odcElementToSave.TaskTitle = replaceDTO.TextAfterReplace;
                                odcElementToSave.Updateable = UpdateType.Upsert;
                                odcCollectionToSave.Add(odcElementToSave);
                                _ODCMapper.SaveODCs(odcCollectionToSave);
                                changed = _AddTaskElementToFieldChangedCollection(toReturn, changed, replaceDTO, odcElementToSave.BoeID, originalValue);
                                break;
                            case FindReplaceElementType.Travel:
                                TravelDTO travelElementToSave = _TravelMapper.GetTravelByTravelID(replaceDTO.TaskElementID);
                                originalValue = travelElementToSave.TaskTitle;
                                travelElementToSave.TaskTitle = replaceDTO.TextAfterReplace;
                                travelElementToSave.Updateable = UpdateType.Upsert;
                                travelCollectionToSave.Add(travelElementToSave);
                                _TravelMapper.SaveTravels(travelCollectionToSave);
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
                                BoeTaskElementDTO taskElementToSave = _BoeTaskElementMapper.GetBoeTaskElementByBoeTaskElementID(replaceDTO.TaskElementID);
                                taskElementToSave.Description = replaceDTO.TextAfterReplace;
                                string originalValue = taskElementToSave.Description;
                                taskElementToSave.Updateable = UpdateType.Upsert;
                                taskElementCollectionToSave.Add(taskElementToSave);
                                if (ConfigurationUtilities.GetAppSetting<bool>("TrackMOQEquation"))
                                    _log.Warn("Track MOQ Equation - FindReplaceDTODataMapper.saveReplace() Line 286 - Field Name is Task Description - Call Mapper SaveBoeTaskElements  Task Element:" + taskElementToSave + " MOQ Equation:" + taskElementToSave.MOQHoursEquation);
                                
                                _BoeTaskElementMapper.SaveBoeTaskElements(taskElementCollectionToSave);

                                changed = _AddTaskElementToFieldChangedCollection(toReturn, changed, replaceDTO, taskElementToSave.BoeID, originalValue);
                                break;
                            case FindReplaceElementType.Material:
                                MaterialDTO materialElementToSave = _MaterialLoader.GetMaterialDTOByMaterialID(replaceDTO.TaskElementID);
                                materialElementToSave.TaskDescription = replaceDTO.TextAfterReplace;
                                originalValue = materialElementToSave.TaskDescription;
                                materialElementToSave.Updateable = UpdateType.Upsert;
                                materialCollectionToSave.Add(materialElementToSave);
                                _MaterialLoader.SaveMaterials(materialCollectionToSave);
                                changed = _AddTaskElementToFieldChangedCollection(toReturn, changed, replaceDTO, materialElementToSave.BoeID, originalValue);
                                break;
                            case FindReplaceElementType.ODC:
                                OtherDirectCostDTO odcElementToSave = _ODCMapper.GetODCByODCID(replaceDTO.TaskElementID);
                                odcElementToSave.TaskDescription = replaceDTO.TextAfterReplace;
                                originalValue = odcElementToSave.TaskDescription;
                                odcElementToSave.Updateable = UpdateType.Upsert;
                                odcCollectionToSave.Add(odcElementToSave);
                                _ODCMapper.SaveODCs(odcCollectionToSave);

                                changed = _AddTaskElementToFieldChangedCollection(toReturn, changed, replaceDTO, odcElementToSave.BoeID, originalValue);
                                break;
                            case FindReplaceElementType.Travel:
                                TravelDTO travelElementToSave = _TravelMapper.GetTravelByTravelID(replaceDTO.TaskElementID);
                                travelElementToSave.Description = replaceDTO.TextAfterReplace;
                                originalValue = travelElementToSave.Description;
                                travelElementToSave.Updateable = UpdateType.Upsert;
                                travelCollectionToSave.Add(travelElementToSave);
                                _TravelMapper.SaveTravels(travelCollectionToSave);
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
                                BoeTaskElementDTO taskElementToSave = _BoeTaskElementMapper.GetBoeTaskElementByBoeTaskElementID(replaceDTO.TaskElementID);
                                string originalValue = taskElementToSave.MOQText;
                                changed.Add(new FieldChanged { Field = replaceDTO.FieldName, OldValue = taskElementToSave.MOQText, NewValue = replaceDTO.TextAfterReplace });
                                taskElementToSave.MOQText = replaceDTO.TextAfterReplace;
                                taskElementToSave.Updateable = UpdateType.Upsert;
                                taskElementCollectionToSave.Add(taskElementToSave);
                                if (ConfigurationUtilities.GetAppSetting<bool>("TrackMOQEquation"))
                                    _log.Warn("Track MOQ Equation - FindReplaceDTODataMapper.saveReplace() Line 337 - Field Name is MOQ Title - Call Mapper SaveBoeTaskElements  Task Element:" + taskElementToSave + " MOQ Equation:" + taskElementToSave.MOQHoursEquation);
                                
                                _BoeTaskElementMapper.SaveBoeTaskElements(taskElementCollectionToSave);

                                changed = _AddTaskElementToFieldChangedCollection(toReturn, changed, replaceDTO, taskElementToSave.BoeID, originalValue);
                                break;
                            case FindReplaceElementType.Material:
                                MaterialDTO materialElementToSave = _MaterialLoader.GetMaterialDTOByMaterialID(replaceDTO.TaskElementID);
                                originalValue = materialElementToSave.MoqText;
                                changed.Add(new FieldChanged { Field = replaceDTO.FieldName, OldValue = materialElementToSave.MoqText, NewValue = replaceDTO.TextAfterReplace });
                                materialElementToSave.MoqText = replaceDTO.TextAfterReplace;
                                materialElementToSave.Updateable = UpdateType.Upsert;
                                materialCollectionToSave.Add(materialElementToSave);
                                _MaterialLoader.SaveMaterials(materialCollectionToSave);

                                changed = _AddTaskElementToFieldChangedCollection(toReturn, changed, replaceDTO, materialElementToSave.BoeID, originalValue);
                                break;
                            case FindReplaceElementType.ODC:
                                OtherDirectCostDTO odcElementToSave = _ODCMapper.GetODCByODCID(replaceDTO.TaskElementID);
                                originalValue = odcElementToSave.MoqText;
                                changed.Add(new FieldChanged { Field = replaceDTO.FieldName, OldValue = odcElementToSave.MoqText, NewValue = replaceDTO.TextAfterReplace });
                                odcElementToSave.MoqText = replaceDTO.TextAfterReplace;
                                odcElementToSave.Updateable = UpdateType.Upsert;
                                odcCollectionToSave.Add(odcElementToSave);
                                _ODCMapper.SaveODCs(odcCollectionToSave);
                                changed = _AddTaskElementToFieldChangedCollection(toReturn, changed, replaceDTO, odcElementToSave.BoeID, originalValue);
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
