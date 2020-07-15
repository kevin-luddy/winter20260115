// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Validation
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BOECommentsResponsesValidatorTest
    {
        [TestMethod]
        public void AllBOEAuthorCommentsResponses()
        {           
            var boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();

            int boeID = 1;
            
            ICollection<BOECommentDTO> boeComments = new Collection<BOECommentDTO>();
            boeComments.Add(new BOECommentDTO(){ Id = 1, BoeID = 1, FieldID = 3 });
            boeComments.Add(new BOECommentDTO() { Id = 2, BoeID = 1, FieldID = 4, BOEResponseToCommentID = 1 });
            boeComments.Add(new BOECommentDTO() { Id = 3, BoeID = 1, FieldID = 3 });
            boeComments.Add(new BOECommentDTO() { Id = 4, BoeID = 1, FieldID = 4, BOEResponseToCommentID = 3 });
            boeComments.Add(new BOECommentDTO() { Id = 5, BoeID = 1, FieldID = 3 });

            boeCommentDTODataLoader.Setup(x => x.GetByBoeId(boeID)).Returns(boeComments);

            var sut = new BOECommentsResponsesValidator(boeCommentDTODataLoader.Object);
            bool validationResponse = sut.AllBOEAuthorCommentsResponses(boeID);

            Assert.IsTrue(validationResponse.Equals(false));
            boeComments.Add(new BOECommentDTO() { Id = 6, BoeID = 1, FieldID = 4, BOEResponseToCommentID = 5 });
            
            boeCommentDTODataLoader.Setup(x => x.GetByIds(new List<int>() { boeID })).Returns(boeComments);

            var sut2 = new BOECommentsResponsesValidator(boeCommentDTODataLoader.Object);
            bool validationResponse2 = sut2.AllBOEAuthorCommentsResponses(boeID);
            validationResponse = sut2.AllBOEAuthorCommentsResponses(boeID);

            Assert.IsTrue(validationResponse2.Equals(true));
        }
    }
}
