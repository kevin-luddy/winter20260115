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
    using System.Linq;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OtherDirectCostDTODataLoaderTest
    {
        /// <summary>
        /// Create a loader for testing
        /// </summary>
        /// <returns>Test loader</returns>
        private OtherDirectCostDTODataLoader CreateTestLoader()
        {
            return new OtherDirectCostDTODataLoader();
        }

        /// <summary>
        /// Tests that RTE loading works as expected
        /// </summary>
        [TestMethod]
        public void TestingRteLoadChanges()
        {
            OtherDirectCostDTODataLoader loader = new OtherDirectCostDTODataLoader();

            int id = -1;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                id = gbe.ODCTaskElements.First(x => x.ODCTaskDescription.Length > 0).ODCTaskElementID;
            }

            // A first test:
            // Check the RTE data being correctly loaded/not loaded..
            OtherDirectCostDTO nonRteLoadedBoe = loader.GetByIds(new List<int>() { id }).First();

            // make sure RTE data was not loaded by default
            Assert.IsFalse(nonRteLoadedBoe.WasDescriptionSet);
            Assert.IsFalse(nonRteLoadedBoe.WasMoqTextSet);

            // make sure Description is handled correctly
            nonRteLoadedBoe.TaskDescription = "blah blah";
            Assert.IsTrue(nonRteLoadedBoe.WasDescriptionSet);
            Assert.IsFalse(nonRteLoadedBoe.WasMoqTextSet);

            // load RTE data, make sure it loads correctly, leaving description alone
            loader.LoadRTEFields(new List<OtherDirectCostDTO>() { nonRteLoadedBoe });
            Assert.IsTrue(nonRteLoadedBoe.WasDescriptionSet);
            Assert.IsTrue(nonRteLoadedBoe.TaskDescription == "blah blah");
            Assert.IsTrue(nonRteLoadedBoe.WasMoqTextSet);


            // A second test:
            // do a manual, non RTE load first, then load RTE data after, then do a full RTE load, and compare the two, to make sure it's all the same
            nonRteLoadedBoe = loader.GetByIds(new List<int>() { id }).First();
            Assert.IsFalse(nonRteLoadedBoe.WasDescriptionSet);
            loader.LoadRTEFields(new List<OtherDirectCostDTO>() { nonRteLoadedBoe });
            Assert.IsTrue(nonRteLoadedBoe.WasDescriptionSet);
            Assert.IsTrue(nonRteLoadedBoe.WasMoqTextSet);

            OtherDirectCostDTO rteLoadedBoe = loader.GetByIds(new List<int>() { id }, true).First();
            Assert.IsTrue(rteLoadedBoe.WasDescriptionSet);
            Assert.IsTrue(nonRteLoadedBoe.WasMoqTextSet);

            this.VerifyDtos(nonRteLoadedBoe, rteLoadedBoe);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification="Leaving this here for future test support")]
        private void VerifyCollections(ICollection<OtherDirectCostDTO> collection1, ICollection<OtherDirectCostDTO> collection2)
        {
            Assert.AreEqual(collection1.Count, collection2.Count);
            for (int i = 0; i < collection1.Count; i++)
            {
                this.VerifyDtos(collection1.ElementAt(i), collection2.ElementAt(i));
            }
        }

        private void VerifyDtos(OtherDirectCostDTO dto1, OtherDirectCostDTO dto2)
        {
            Assert.AreEqual(dto1.BoeID, dto2.BoeID);
            Assert.AreEqual(dto1.BOETaskElementOrder, dto2.BOETaskElementOrder);
            Assert.AreEqual(dto1.BOETaskID, dto2.BOETaskID); 
            Assert.AreEqual(dto1.EndDate, dto2.EndDate);
            Assert.AreEqual(dto1.Id, dto2.Id);
            Assert.AreEqual(dto1.StartDate, dto2.StartDate);
            Assert.AreEqual(dto1.TaskID, dto2.TaskID);
            Assert.AreEqual(dto1.TaskTitle, dto2.TaskTitle);
            Assert.AreEqual(dto1.UpdateDate, dto2.UpdateDate);
            Assert.AreEqual(dto1.WasDescriptionSet, dto2.WasDescriptionSet);
            Assert.AreEqual(dto1.WasMoqTextSet, dto2.WasMoqTextSet);
            
            if (dto1.WasDescriptionSet && dto2.WasDescriptionSet)
            {
                Assert.AreEqual(dto1.TaskDescription, dto2.TaskDescription);
            }

            if (dto1.WasMoqTextSet && dto2.WasMoqTextSet)
            {
                Assert.AreEqual(dto1.MoqText, dto2.MoqText);
            }

            Assert.AreEqual(dto1.ODCTypes.Count, dto2.ODCTypes.Count);
            for (int i = 0; i < dto1.ODCTypes.Count; i++)
            {
                Assert.AreEqual(dto1.ODCTypes.ElementAt(i).BoeID, dto2.ODCTypes.ElementAt(i).BoeID);
                Assert.AreEqual(dto1.ODCTypes.ElementAt(i).Cost, dto2.ODCTypes.ElementAt(i).Cost);
                Assert.AreEqual(dto1.ODCTypes.ElementAt(i).EndDate, dto2.ODCTypes.ElementAt(i).EndDate);
                Assert.AreEqual(dto1.ODCTypes.ElementAt(i).Id, dto2.ODCTypes.ElementAt(i).Id);
                Assert.AreEqual(dto1.ODCTypes.ElementAt(i).ODCTypeID, dto2.ODCTypes.ElementAt(i).ODCTypeID);
                Assert.AreEqual(dto1.ODCTypes.ElementAt(i).PerformingOrgID, dto2.ODCTypes.ElementAt(i).PerformingOrgID);
                Assert.AreEqual(dto1.ODCTypes.ElementAt(i).ResourceID, dto2.ODCTypes.ElementAt(i).ResourceID);
                Assert.AreEqual(dto1.ODCTypes.ElementAt(i).SpreadCurve, dto2.ODCTypes.ElementAt(i).SpreadCurve);
                Assert.AreEqual(dto1.ODCTypes.ElementAt(i).StartDate, dto2.ODCTypes.ElementAt(i).StartDate);
                Assert.AreEqual(dto1.ODCTypes.ElementAt(i).UpdateDate, dto2.ODCTypes.ElementAt(i).UpdateDate);

                Assert.AreEqual(dto1.ODCTypes.ElementAt(i).ODCSpreads.Count, dto1.ODCTypes.ElementAt(i).ODCSpreads.Count);
                for (int j = 0; j < dto1.ODCTypes.ElementAt(i).ODCSpreads.Count; j++)
                {
                    Assert.AreEqual(dto1.ODCTypes.ElementAt(i).ODCSpreads.ElementAt(j).BoeID, 
                                    dto2.ODCTypes.ElementAt(i).ODCSpreads.ElementAt(j).BoeID);
                    Assert.AreEqual(dto1.ODCTypes.ElementAt(i).ODCSpreads.ElementAt(j).CostSpreadValue, 
                                    dto2.ODCTypes.ElementAt(i).ODCSpreads.ElementAt(j).CostSpreadValue);
                    Assert.AreEqual(dto1.ODCTypes.ElementAt(i).ODCSpreads.ElementAt(j).Id, 
                                    dto2.ODCTypes.ElementAt(i).ODCSpreads.ElementAt(j).Id);
                    Assert.AreEqual(dto1.ODCTypes.ElementAt(i).ODCSpreads.ElementAt(j).ODCSpreadDate, 
                                    dto2.ODCTypes.ElementAt(i).ODCSpreads.ElementAt(j).ODCSpreadDate);
                    Assert.AreEqual(dto1.ODCTypes.ElementAt(i).ODCSpreads.ElementAt(j).ODCSpreadID, 
                                    dto2.ODCTypes.ElementAt(i).ODCSpreads.ElementAt(j).ODCSpreadID);
                    Assert.AreEqual(dto1.ODCTypes.ElementAt(i).ODCSpreads.ElementAt(j).UpdateDate, 
                                    dto2.ODCTypes.ElementAt(i).ODCSpreads.ElementAt(j).UpdateDate);
                }
            }

        }
    }
}
