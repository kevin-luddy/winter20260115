// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.DAL.Loader
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Transactions;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using IES.Common.PickList;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test class for the pick lists
    /// </summary>
    [TestClass]
    public class PickListLoadersTests
    {
        /// <summary>
        /// Test the Pick List Loaders
        /// </summary>
        [TestMethod]
        public void L_LoaderTests()
        {
            this.L_TestLoader(new ProposalClassLULoader());
            this.L_TestLoader(new ProposalTypeLULoader());
            this.L_TestLoader(new TypeOfRequestLULoader());
        }

        /// <summary>
        /// Tests a single Pick List Loader
        /// </summary>
        /// <param name="loader">Loader that will be tested</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "IOUOIEeflkdsafi")]
        private void L_TestLoader(IPickListLoader loader)
        {
            // Insert
            PickListDto newRecord = new PickListDto() { Text = "THIS IS A TEST.. DUSAN IOUOIEeflkdsafi*($&#(", IsActive = true, Updateable = UpdateType.Upsert };
            using (TransactionScope scope = new TransactionScope())
            {
                loader.Save(newRecord);
                scope.Complete();
            }

            ICollection<PickListDto> data = loader.GetPickListValues();

            // Insert passes, the item is inserted
            Assert.IsTrue(data.Any(x => x.Text == newRecord.Text && x.IsActive == newRecord.IsActive && x.IsReadOnly == false));

            // Update - not in use
            newRecord = data.First(x => x.Text == newRecord.Text && x.IsActive == newRecord.IsActive);
                newRecord.Text = "TESTING UPDATE THIS IS A TEST.. IOUOIEeflkdsafi";
                newRecord.Updateable = UpdateType.Upsert;
            using (TransactionScope scope = new TransactionScope())
            {
                loader.Save(newRecord);
                scope.Complete();
            }

            data = loader.GetPickListValues();

            // Update passes, the item is updated
            Assert.IsTrue(data.Any(x => x.Text == newRecord.Text && x.IsActive == newRecord.IsActive && x.IsReadOnly == false));

            // Delete - not in use
            newRecord = data.First(x => x.Text == newRecord.Text && x.IsActive == newRecord.IsActive);
                newRecord.Updateable = UpdateType.Deleted;
            using (TransactionScope scope = new TransactionScope())
            {
                loader.Save(newRecord);
                scope.Complete();
            }

            data = loader.GetPickListValues();

            // Deletion passes, the item is removed
            Assert.IsFalse(data.Any(x => x.Text == newRecord.Text && x.IsActive == newRecord.IsActive));

            // In Use
            if (data.Any(x => x.InUse))
            {
                // Upsert
                newRecord = data.First(x => x.InUse);
                    newRecord.Text = "TESTING UPDATE THIS IS A TEST.. IOUOIEeflkdsafi";
                    newRecord.Updateable = UpdateType.Upsert;
                using (TransactionScope scope = new TransactionScope())
                {
                    loader.Save(newRecord);
                    scope.Complete();
                }

                data = loader.GetPickListValues();

                // Upsert fails, there's no such item
                Assert.IsFalse(data.Any(x => x.Text == newRecord.Text && x.IsActive == newRecord.IsActive));

                // Delete
                newRecord = data.First(x => x.InUse);
                    newRecord.Updateable = UpdateType.Deleted;
                using (TransactionScope scope = new TransactionScope())
                {
                    loader.Save(newRecord);
                    scope.Complete();
                }

                data = loader.GetPickListValues();

                // Delete fails, item should exist
                Assert.IsTrue(data.Any(x => x.Text == newRecord.Text && x.IsActive == newRecord.IsActive));
            }
        }
    }
}