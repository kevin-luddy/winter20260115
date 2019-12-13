// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.DataBridge.DTO;
    using GenBOE.DataBridge.Reference;
    using GenBOE.Dtos;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class InUseDataLoaderTest : MOQLoaderObject
    {
        /// <summary>
        /// Create a loader for testing
        /// </summary>
        /// <returns>Test loader</returns>
        private OtherDirectCostDTODataLoader CreateTestLoader()
        {
            return new OtherDirectCostDTODataLoader();
        }

        [TestMethod]
        public void L_GetInUse()
        {
            var sut = new InUseDataLoader();

            bool inUse = sut.GetInUse(IES.Common.InUseDataType.WorkspaceResources, this.Resource.Id, this.Workspace.ResourceListID); // use the workspace.resourceListID instead of this.ResourceListID

            Assert.IsTrue(inUse == false, "moq resource isn't in use");


        }

        [TestMethod]
        public void L_GetSystemResourceIDsInUse()
        {
            var sut = new InUseDataLoader();

            HashSet<int> resourceIDs = sut.GetSystemResourceIDsInUse();

            Assert.IsTrue(resourceIDs.Count > 0, "no in use system resources found");

        }
    }
}
