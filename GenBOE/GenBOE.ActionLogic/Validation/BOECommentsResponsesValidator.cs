// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Validation
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;

    public class BOECommentsResponsesValidator
    {
        IBOECommentDTODataLoader _BOECommentDataLoader;

        public BOECommentsResponsesValidator(IBOECommentDTODataLoader inBOECommentDataLoader)
        {
            this._BOECommentDataLoader = inBOECommentDataLoader;
        }

        public virtual bool AllBOEAuthorCommentsResponses(int inBoeID)
        {
            // Get all comments for the current BOE
            ICollection<BOECommentDTO> allComments = this._BOECommentDataLoader.GetByBoeId(inBoeID);

            // Get all comments that have responses
            Collection<int> boeCommentsWithResponses = (from c in allComments
                                                    where c.FieldID == (int)FieldType.AuthorResponse
                                                    select c.BOEResponseToCommentID.Value).ToCollection();
            // get all comments
            Collection<int> boeComments = (from c in allComments
                                           where c.FieldID == (int)FieldType.Comment
                                           select c.Id).ToCollection();

			//determine which comments do not have responses
			IEnumerable<int> boeCommentsWithoutResponses = boeComments.Where(c => !boeCommentsWithResponses.Any(c2 => c == c2));
            
            //if any are found, fail validation
            return !boeCommentsWithoutResponses.Any();
        }
    }
}
