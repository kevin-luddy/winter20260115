// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IES.Common;
    using System.Collections.ObjectModel;
    using GenBOE.Models;
    using GenBOE.Dtos;

    public class FindReplaceDTODataLoader : IFindReplaceDTODataLoader
    {

        // default constructor
        public FindReplaceDTODataLoader() { }

        #region Retrieves


        [DbQuery]
        public Collection<FindReplaceDTO> getFindReferences(FindReplaceDTO inFindParams, int inWorkspaceId)
        {
            if (inFindParams == null)
            {
                throw new ArgumentNullException(nameof(inFindParams));
            }

            Collection<FindReplaceDTO> toReturn = new Collection<FindReplaceDTO>();
            List<int> ProcessedBOEIDs = new List<int>();

            string searchWithPadding = "\"*" + inFindParams.FindText + "*\"";

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                gbe.Database.CommandTimeout = 180;

                List<getBOEForFindReplace_Result> spResults = (from x in gbe.getBOEForFindReplace(searchWithPadding, inWorkspaceId)
                                 select x).ToList();

                foreach (var resultRow in spResults)
                {
                    if (resultRow.BOEDescriptionOccurrences.HasValue && resultRow.BOEDescriptionOccurrences > 0
                        && !ProcessedBOEIDs.Contains(resultRow.BOEID.Value))
                    {
                        FindReplaceDTO textFound = new FindReplaceDTO();
                        textFound.WorkspaceID = inWorkspaceId;
                        textFound.BOEId = resultRow.BOEID.HasValue ? resultRow.BOEID.Value : -1;
                        textFound.TaskElementID = resultRow.TaskElementID.HasValue ? resultRow.TaskElementID.Value : -1;
                        textFound.FindReplaceTaskElementType = GetFindReplaceEnum(resultRow.TaskElementType); 
                        textFound.FieldEnum = 1;
                        textFound.Occurences = resultRow.BOEDescriptionOccurrences.HasValue ? resultRow.BOEDescriptionOccurrences.Value : -1;
                        textFound.FoundText = resultRow.BOEDescription;
                        toReturn.Add(textFound);
                    }

                    if (resultRow.DataSourceOccurrences.HasValue && resultRow.DataSourceOccurrences > 0
                        && !ProcessedBOEIDs.Contains(resultRow.BOEID.Value))
                    {
                        FindReplaceDTO textFound = new FindReplaceDTO();
                        textFound.WorkspaceID = inWorkspaceId;
                        textFound.BOEId = resultRow.BOEID.HasValue ? resultRow.BOEID.Value : -1;
                        textFound.TaskElementID = resultRow.TaskElementID.HasValue ? resultRow.TaskElementID.Value : -1;
                        textFound.FindReplaceTaskElementType = GetFindReplaceEnum(resultRow.TaskElementType); 
                        textFound.FieldEnum = 2;
                        textFound.Occurences = resultRow.DataSourceOccurrences.HasValue ? resultRow.DataSourceOccurrences.Value : -1;
                        textFound.FoundText = resultRow.DataSource;
                        toReturn.Add(textFound);
                    }

                    if (resultRow.TaskTitleOccurrences.HasValue && resultRow.TaskTitleOccurrences > 0)
                    {
                        FindReplaceDTO textFound = new FindReplaceDTO();
                        textFound.WorkspaceID = inWorkspaceId;
                        textFound.BOEId = resultRow.BOEID.HasValue ? resultRow.BOEID.Value : -1;
                        textFound.TaskElementID = resultRow.TaskElementID.HasValue ? resultRow.TaskElementID.Value : -1;
                        textFound.FindReplaceTaskElementType = GetFindReplaceEnum(resultRow.TaskElementType); 
                        textFound.FieldEnum = 3;
                        textFound.Occurences = resultRow.TaskTitleOccurrences.HasValue ? resultRow.TaskTitleOccurrences.Value : -1;
                        textFound.FoundText = resultRow.TaskTitle;
                        toReturn.Add(textFound);
                    }

                    if (resultRow.TaskDescriptionOccurrences.HasValue && resultRow.TaskDescriptionOccurrences > 0)
                    {
                        FindReplaceDTO textFound = new FindReplaceDTO();
                        textFound.WorkspaceID = inWorkspaceId;
                        textFound.BOEId = resultRow.BOEID.HasValue ? resultRow.BOEID.Value : -1;
                        textFound.TaskElementID = resultRow.TaskElementID.HasValue ? resultRow.TaskElementID.Value : -1;
                        textFound.FindReplaceTaskElementType = GetFindReplaceEnum(resultRow.TaskElementType); 
                        textFound.FieldEnum = 4;
                        textFound.Occurences = resultRow.TaskDescriptionOccurrences.HasValue ? resultRow.TaskDescriptionOccurrences.Value : -1;
                        textFound.FoundText = resultRow.TaskDescription;
                        toReturn.Add(textFound);
                    }

                    if (resultRow.MOQTextOccurrences.HasValue && resultRow.MOQTextOccurrences > 0)
                    {
                        FindReplaceDTO textFound = new FindReplaceDTO();
                        textFound.WorkspaceID = inWorkspaceId;
                        textFound.BOEId = resultRow.BOEID.HasValue ? resultRow.BOEID.Value : -1;
                        textFound.TaskElementID = resultRow.TaskElementID.HasValue ? resultRow.TaskElementID.Value : -1;
                        textFound.FindReplaceTaskElementType = GetFindReplaceEnum(resultRow.TaskElementType); 
                        textFound.FieldEnum = 5;
                        textFound.Occurences = resultRow.MOQTextOccurrences.HasValue ? resultRow.MOQTextOccurrences.Value : -1;
                        textFound.FoundText = resultRow.MOQText;
                        toReturn.Add(textFound);
                    }

                    if (resultRow.BOETitleOccurrences.HasValue && resultRow.BOETitleOccurrences > 0
                        && !ProcessedBOEIDs.Contains(resultRow.BOEID.Value))
                    {
                        FindReplaceDTO textFound = new FindReplaceDTO();
                        textFound.WorkspaceID = inWorkspaceId;
                        textFound.BOEId = resultRow.BOEID.HasValue ? resultRow.BOEID.Value : -1;
                        textFound.TaskElementID = resultRow.TaskElementID.HasValue ? resultRow.TaskElementID.Value : -1;
                        textFound.FindReplaceTaskElementType = GetFindReplaceEnum(resultRow.TaskElementType);
                        textFound.FieldEnum = 6;
                        textFound.Occurences = resultRow.BOETitleOccurrences.HasValue ? resultRow.BOETitleOccurrences.Value : -1;
                        textFound.FoundText = resultRow.BOETitle;
                        toReturn.Add(textFound);
                    }

                    if (!ProcessedBOEIDs.Contains(resultRow.BOEID.Value))
                    {
                        ProcessedBOEIDs.Add(resultRow.BOEID.Value);
                    }
                }
            }

            return toReturn;
        }

        private FindReplaceElementType GetFindReplaceEnum(string inFindReplaceFromDB)
        {
            FindReplaceElementType toReturn = FindReplaceElementType.BOE;

            switch (inFindReplaceFromDB)
            {
                case "BOE":
                    toReturn = FindReplaceElementType.BOE;
                    break;
                case "Travel":
                    toReturn = FindReplaceElementType.Travel;
                    break;
                default:
                    break;
            }
            return toReturn;
        }
        #endregion Retrieves
    }
}
