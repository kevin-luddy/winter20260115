// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Transactions;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CustomFieldValueDTODataLoaderTest
    {
        /// <summary>
        /// Override the TestContext for reference in the TestInitialize method.
        /// </summary>
        public TestContext TestContext { get; set; }

        [ClassInitialize]
        /// Since this function is defined as ClassInitialize, it will be the first function
        /// called in this entire class. Therefore, the workspace used in all of these test methods
        /// will be the same
        public static void GetCurrentBOE(TestContext testContext)
        {
            Initialize();
        }

        private static void Initialize()
        {
            GlobalTestCaseSetup.ResetGlobalWorkspaceID();
            GlobalTestCaseSetup.CreateBOE(GlobalTestCaseSetup.GlobalWorkspaceID);
            GlobalTestCaseSetup.CreateGlobalTaskElementID();
            GlobalTestCaseSetup.CreateGlobalBOELaborTypeID();
        }

        [TestInitialize]
        public void CreateCustomFieldData()
        {
            GlobalTestCaseSetup.CreateCustomField(GlobalTestCaseSetup.GlobalWorkspaceID);

            // Do not create a pre-existing CustomFieldValue for the Alphabetical Order tests.
            switch (TestContext.TestName)
            {
                case "GetCustomFieldValueDTOsByCustomFieldID_AlphabeticalOrder":
                    break;
                case "GetCustomFieldValueDTOsByCustomFieldIds_AlphabeticalOrder":
                    break;
                default:
                    GlobalTestCaseSetup.CreateCustomFieldValue();
                    break;
            }
        }

        [TestMethod]
        public void GetByIds()
        {
            var sut = new CustomFieldValueDTODataLoader();

            ICollection<CustomFieldValueDTO> customFieldValueDTOs = sut.GetByIds(new Collection<int> { GlobalTestCaseSetup.GlobalCustomFieldValueID });

            Assert.IsTrue(customFieldValueDTOs.Count > 0);
        }

        [TestMethod]
        public void GetCustomFieldValueDtosByCustomFieldIds()
        {
            var sut = new CustomFieldValueDTODataLoader();

            ICollection<CustomFieldValueDTO> customFieldValueDTOs = sut.GetCustomFieldValueDTOsByCustomFieldIds(new Collection<int> { GlobalTestCaseSetup.GlobalCustomFieldID });

            Assert.IsTrue(customFieldValueDTOs.Count > 0);
        }

        [TestMethod]
        public void GetCustomFieldValueIDsContainerIDsByResourceIDs()
        {
            //set up CustomFieldValueDTODataLoader
            var sut = new CustomFieldValueDTODataLoader();
            
            // Clear all existing LaborTypeCustomFieldValueXrefs that may be left over from other tests.
            GlobalTestCaseSetup.DeleteAllLaborTypeCustomFieldValueXref();

            //test that when a resource has no custom field values, no mapping is returned
            //create a collection to hold the labor type IDs and add the first ID
            Collection<int> laborTypeIDs = new Collection<int>();
            laborTypeIDs.Add(GlobalTestCaseSetup.GlobalBOELaborTypeID);
            //no xref set up, so should return empty
            Dictionary<int, ICollection<KeyValuePair<int, int>>> customFieldValueIdMapping = sut.GetCustomFieldValueIDsContainerIDsByResourceIDs(laborTypeIDs);
            //confirm there is no mapping
            Assert.IsTrue(!customFieldValueIdMapping.Any(), "customFieldValueIdMapping is not empty");

            //test that when a resource has a custom field value, one mapping is returned
            //create the custom field value and xref
            GlobalTestCaseSetup.CreateLaborTypeCustomFieldValueXref();
            //now with an xref, a mapping should be returned
            customFieldValueIdMapping = sut.GetCustomFieldValueIDsContainerIDsByResourceIDs(laborTypeIDs);
            //confirm that GetCustomFieldValueIDsContainerIDsByResourceIDs returns one mapping
            Assert.IsTrue(customFieldValueIdMapping.Count() == 1, "customFieldValueIdMapping is empty");

            //test that when a resource has mulitple custom field values, one mapping is returned
            //create additional custom field value and xref
            GlobalTestCaseSetup.ResetGlobalCustomFieldValueID();
            GlobalTestCaseSetup.CreateLaborTypeCustomFieldValueXref();
            //with one labor type with multiple custom field values, one mapping should still be returned
            customFieldValueIdMapping = sut.GetCustomFieldValueIDsContainerIDsByResourceIDs(laborTypeIDs);
            //confirm that GetCustomFieldValueIDsContainerIDsByResourceIDs returns one mapping
            Assert.IsTrue(customFieldValueIdMapping.Count() == 1, "customFieldValueIdMapping is empty");

        }

        [TestMethod]
        public void GetCustomFieldValueDTO()
        {
            var sut = new CustomFieldValueDTODataLoader();

            CustomFieldValueDTO customFieldValueDTOdata = sut.GetById(GlobalTestCaseSetup.GlobalCustomFieldValueID);

            Assert.IsTrue(customFieldValueDTOdata != null, "customFieldValueDTOdata is equal to null");
        }


        [TestMethod]
        public void SaveCustomFieldValues()
        {
            var sut = new CustomFieldValueDTODataLoader();

            CustomFieldValueDTO customFieldValueDTO = new CustomFieldValueDTO
            {
                CustomFieldID = GlobalTestCaseSetup.GlobalCustomFieldID,
                CustomFieldValueDescription = "PA",
                CustomFieldValueID = -1,
                CustomFieldValueInUseFlag = false,
                CustomFieldValueName = "1",
                Updateable = UpdateType.Upsert,
                UpdateDate = DateTime.Now
            };

            Collection<CustomFieldValueDTO> customFieldValueDTOs = new Collection<CustomFieldValueDTO> { customFieldValueDTO };

            ICollection<CustomFieldValueDTO> customFieldValuesBeforeSave = sut.GetCustomFieldValueDTOsByCustomFieldID(GlobalTestCaseSetup.GlobalCustomFieldID);

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(customFieldValueDTOs);
                scope.Complete();
            }

            ICollection<CustomFieldValueDTO> customFieldValuesAfterSave = sut.GetCustomFieldValueDTOsByCustomFieldID(GlobalTestCaseSetup.GlobalCustomFieldID);

            Assert.IsTrue(customFieldValuesAfterSave.Count > customFieldValuesBeforeSave.Count, "Custom Field Values were not saved");

            ICollection<CustomFieldValueDTO> customFieldValuesToDelete = customFieldValuesAfterSave.Where(cfvA => !customFieldValuesBeforeSave.Any(cfvB => cfvB.CustomFieldValueID == cfvA.CustomFieldValueID)).ToList();
            CustomFieldValueDTO customFieldValueDTOAfterSave = sut.GetById(customFieldValuesToDelete.FirstOrDefault().CustomFieldValueID);
            customFieldValueDTOAfterSave.Updateable = UpdateType.Deleted;

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(customFieldValueDTOAfterSave);
                scope.Complete();
            }

            ICollection<CustomFieldValueDTO> customFieldValueIDsAfterDelete = sut.GetCustomFieldValueDTOsByCustomFieldID(GlobalTestCaseSetup.GlobalCustomFieldID);

            Assert.IsTrue(customFieldValueIDsAfterDelete.Count < customFieldValuesAfterSave.Count, "Did not delete custom field value");
        }

        /// <summary>
        /// Test updating CF Value when it is in use
        /// </summary>
        [TestMethod]
        public void SaveCustomFieldValues_InUse()
        {
            var sut = new CustomFieldValueDTODataLoader();

            CustomFieldValueDTO customFieldValueDTO = new CustomFieldValueDTO();

            // Get a Custom Field Valut DTO for testing
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                customFieldValueDTO = (from c in gbe.CustomFieldValues
                                        where c.CustomFieldValueInUseFlag == true
                                        select new CustomFieldValueDTO
                                        {
                                            Id = c.CustomFieldValueID,
                                            CustomFieldValueID = c.CustomFieldValueID,
                                            CustomFieldValueName = c.CustomFieldValueName,
                                            CustomFieldValueDescription = c.CustomFieldValueDescription,
                                            CustomFieldID = c.CustomFieldID,
                                            CustomFieldValueInUseFlag = c.CustomFieldValueInUseFlag,
                                            UpdateDate = c.UpdateDT
                                        }).First();
            }
            
            // Store original name and description for testing and to restore later
            string originalName = customFieldValueDTO.CustomFieldValueName;
            string originalDescription = customFieldValueDTO.CustomFieldValueDescription;

            // Change the Name and Description and set to Upsert
            customFieldValueDTO.CustomFieldValueName = "TestName";
            customFieldValueDTO.CustomFieldValueDescription = "TestDescription";
            customFieldValueDTO.Updateable = UpdateType.Upsert;

            // Save the changes
            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(new Collection<CustomFieldValueDTO> { customFieldValueDTO });
                scope.Complete();
            }

            // Get the updated Custom Field Value
            CustomFieldValueDTO cfvAfterUpdate = sut.GetById(customFieldValueDTO.CustomFieldValueID);

            // Assert that the Name did NOT change and that the Description did change
            Assert.AreEqual(originalName, cfvAfterUpdate.CustomFieldValueName);
            Assert.AreNotEqual(customFieldValueDTO.CustomFieldValueName, cfvAfterUpdate.CustomFieldValueName);
            Assert.AreNotEqual(originalDescription, cfvAfterUpdate.CustomFieldValueDescription);
            Assert.AreEqual(customFieldValueDTO.CustomFieldValueDescription, cfvAfterUpdate.CustomFieldValueDescription);

            // Restore the original description (name is the same, so no need to restore)
            cfvAfterUpdate.CustomFieldValueDescription = originalDescription;
            cfvAfterUpdate.Updateable = UpdateType.Upsert;

            // Save the restored Custom Field Value
            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(new Collection<CustomFieldValueDTO> { cfvAfterUpdate });
                scope.Complete();
            }

            // Get the Custom Field Value again after restore
            CustomFieldValueDTO cfvAfterReset = sut.GetById(cfvAfterUpdate.CustomFieldValueID);

            // Assert that it was restored properly
            Assert.AreEqual(originalName, cfvAfterReset.CustomFieldValueName);
            Assert.AreEqual(originalDescription, cfvAfterReset.CustomFieldValueDescription);
        }

        [TestMethod]
        public void GetCustomFieldValueDTOsByCustomFieldID_AlphabeticalOrder()
        {
            var sut = new CustomFieldValueDTODataLoader();

            // Initialize the Custom Field Values to be saved.
            CustomFieldValueDTO customFieldValue_Red = new CustomFieldValueDTO { CustomFieldValueName = "1", CustomFieldValueDescription = "Red", CustomFieldID = GlobalTestCaseSetup.GlobalCustomFieldID, CustomFieldValueID = -1, Updateable = UpdateType.Upsert, UpdateDate = DateTime.Now, CustomFieldValueInUseFlag = false };
            CustomFieldValueDTO customFieldValue_Blue = new CustomFieldValueDTO { CustomFieldValueName = "2", CustomFieldValueDescription = "Blue", CustomFieldID = GlobalTestCaseSetup.GlobalCustomFieldID, CustomFieldValueID = -1, Updateable = UpdateType.Upsert, UpdateDate = DateTime.Now, CustomFieldValueInUseFlag = false };
            CustomFieldValueDTO customFieldValue_Yellow = new CustomFieldValueDTO { CustomFieldValueName = "3", CustomFieldValueDescription = "Yellow", CustomFieldID = GlobalTestCaseSetup.GlobalCustomFieldID, CustomFieldValueID = -1, Updateable = UpdateType.Upsert, UpdateDate = DateTime.Now, CustomFieldValueInUseFlag = false };
            CustomFieldValueDTO customFieldValue_Green = new CustomFieldValueDTO { CustomFieldValueName = "4", CustomFieldValueDescription = "Green", CustomFieldID = GlobalTestCaseSetup.GlobalCustomFieldID, CustomFieldValueID = -1, Updateable = UpdateType.Upsert, UpdateDate = DateTime.Now, CustomFieldValueInUseFlag = false };

            // Create an array of the values to loop through.
            CustomFieldValueDTO[] customFieldValuesToCreate = { customFieldValue_Red, customFieldValue_Blue, customFieldValue_Yellow, customFieldValue_Green };

            // Initialize an empty collection to be used for saving and the alphabetical order flag.
            Collection<CustomFieldValueDTO> customFieldValueDTOs = new Collection<CustomFieldValueDTO>();
            bool alphabeticalOrder = true;

            ICollection<CustomFieldValueDTO> customFieldValuesBeforeSave = sut.GetCustomFieldValueDTOsByCustomFieldID(GlobalTestCaseSetup.GlobalCustomFieldID);

            // Save each individual Custom Field Value.  Saving the entire collection at once leads to errors with the DTO.
            for (int i = 0; i < customFieldValuesToCreate.Count(); i++)
            {
                // Check for a pre-existing alphabetical order, but don't try to access an index outside the bounds of the array.
                if (alphabeticalOrder && i < customFieldValuesToCreate.Count() - 1)
                {
                    if (string.Compare(customFieldValuesToCreate[i].CustomFieldValueDescription, customFieldValuesToCreate[i + 1].CustomFieldValueDescription) > 0)
                    {
                        alphabeticalOrder = false;
                    }
                }

                // Clear the collection and add the new value to save, then save it.
                customFieldValueDTOs.Clear();
                customFieldValueDTOs.Add(customFieldValuesToCreate[i]);

                using (TransactionScope scope = new TransactionScope())
                {
                    sut.Save(customFieldValueDTOs);
                    scope.Complete();
                }
            }

            // Ensure that the initial CustomFieldValues were not in alphabetical order so that the test result will be conclusive.
            Assert.IsFalse(alphabeticalOrder, "Initial CustomFieldValues were already in alphabetical order, so the test result will be inconclusive.");

            // Get the values that were saved, then ensure the correct number of items were created.
            ICollection<CustomFieldValueDTO> customFieldValuesAfterSave = sut.GetCustomFieldValueDTOsByCustomFieldID(GlobalTestCaseSetup.GlobalCustomFieldID);
            Assert.AreEqual(customFieldValuesBeforeSave.Count() + customFieldValuesToCreate.Count(), customFieldValuesAfterSave.Count);

            // Ensure default alphabetical order by CustomFieldValueDescription of the saved items.
            for (int i = 0; i < customFieldValuesAfterSave.Count - 1; i++)
            {
                Assert.IsTrue(string.Compare(customFieldValuesAfterSave.ElementAt(i).CustomFieldValueDescription, customFieldValuesAfterSave.ElementAt(i + 1).CustomFieldValueDescription) < 1, "CustomFieldValues are not sorted alphabetically by CustomFieldValueDescription.");
            }

            // Clean up the items we added to reset the test.
            ICollection<CustomFieldValueDTO> customFieldValuesToDelete = customFieldValuesAfterSave.Where(cfvA => !customFieldValuesBeforeSave.Any(cfvB => cfvB.CustomFieldValueID == cfvA.CustomFieldValueID)).ToList();
            foreach (CustomFieldValueDTO customFieldValueToDelete in customFieldValuesToDelete)
            {
                customFieldValueToDelete.Updateable = UpdateType.Deleted;
            }

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(customFieldValuesToDelete);
                scope.Complete();
            }
        }

        [TestMethod]
        public void GetCustomFieldValueDTOsByCustomFieldIds_AlphabeticalOrder()
        {
            var sut = new CustomFieldValueDTODataLoader();

            // Initialize the Custom Field Values to be saved for Custom Field 1.
            CustomFieldValueDTO customFieldValue1_Red = new CustomFieldValueDTO { CustomFieldValueName = "1", CustomFieldValueDescription = "Red", CustomFieldID = GlobalTestCaseSetup.GlobalCustomFieldID, CustomFieldValueID = -1, Updateable = UpdateType.Upsert, UpdateDate = DateTime.Now, CustomFieldValueInUseFlag = false };
            CustomFieldValueDTO customFieldValue1_Blue = new CustomFieldValueDTO { CustomFieldValueName = "2", CustomFieldValueDescription = "Blue", CustomFieldID = GlobalTestCaseSetup.GlobalCustomFieldID, CustomFieldValueID = -1, Updateable = UpdateType.Upsert, UpdateDate = DateTime.Now, CustomFieldValueInUseFlag = false };
            CustomFieldValueDTO customFieldValue1_Yellow = new CustomFieldValueDTO { CustomFieldValueName = "3", CustomFieldValueDescription = "Yellow", CustomFieldID = GlobalTestCaseSetup.GlobalCustomFieldID, CustomFieldValueID = -1, Updateable = UpdateType.Upsert, UpdateDate = DateTime.Now, CustomFieldValueInUseFlag = false };
            CustomFieldValueDTO customFieldValue1_Green = new CustomFieldValueDTO { CustomFieldValueName = "4", CustomFieldValueDescription = "Green", CustomFieldID = GlobalTestCaseSetup.GlobalCustomFieldID, CustomFieldValueID = -1, Updateable = UpdateType.Upsert, UpdateDate = DateTime.Now, CustomFieldValueInUseFlag = false };

            // Create a second CustomFieldID for the second set of values.
            int customFieldID2 = GlobalTestCaseSetup.CreateCustomField(GlobalTestCaseSetup.GlobalWorkspaceID);

            // Initialize the Custom Field Values to be saved for Custom Field 2.
            CustomFieldValueDTO customFieldValue2_Red = new CustomFieldValueDTO { CustomFieldValueName = "1", CustomFieldValueDescription = "Red", CustomFieldID = customFieldID2, CustomFieldValueID = -1, Updateable = UpdateType.Upsert, UpdateDate = DateTime.Now, CustomFieldValueInUseFlag = false };
            CustomFieldValueDTO customFieldValue2_Blue = new CustomFieldValueDTO { CustomFieldValueName = "2", CustomFieldValueDescription = "Blue", CustomFieldID = customFieldID2, CustomFieldValueID = -1, Updateable = UpdateType.Upsert, UpdateDate = DateTime.Now, CustomFieldValueInUseFlag = false };
            CustomFieldValueDTO customFieldValue2_Yellow = new CustomFieldValueDTO { CustomFieldValueName = "3", CustomFieldValueDescription = "Yellow", CustomFieldID = customFieldID2, CustomFieldValueID = -1, Updateable = UpdateType.Upsert, UpdateDate = DateTime.Now, CustomFieldValueInUseFlag = false };
            CustomFieldValueDTO customFieldValue2_Green = new CustomFieldValueDTO { CustomFieldValueName = "4", CustomFieldValueDescription = "Green", CustomFieldID = customFieldID2, CustomFieldValueID = -1, Updateable = UpdateType.Upsert, UpdateDate = DateTime.Now, CustomFieldValueInUseFlag = false };

            // Create an array of the values to loop through.
            CustomFieldValueDTO[] customFieldValuesToCreate = { customFieldValue1_Red, customFieldValue1_Blue, customFieldValue1_Yellow, customFieldValue1_Green, customFieldValue2_Red, customFieldValue2_Blue, customFieldValue2_Yellow, customFieldValue2_Green };

            // Initialize an empty collection to be used for saving and the alphabetical order flag.
            Collection<CustomFieldValueDTO> customFieldValueDTOs = new Collection<CustomFieldValueDTO>();
            bool alphabeticalOrder = true;

            ICollection<CustomFieldValueDTO> customFieldValuesBeforeSave = sut.GetCustomFieldValueDTOsByCustomFieldID(GlobalTestCaseSetup.GlobalCustomFieldID);

            // Save each individual Custom Field Value.  Saving the entire collection at once leads to errors with the DTO.
            for (int i = 0; i < customFieldValuesToCreate.Count(); i++)
            {
                // Check for a pre-existing alphabetical order, but don't try to access an index outside the bounds of the array.
                if (alphabeticalOrder && i < customFieldValuesToCreate.Count() - 1)
                {
                    if (string.Compare(customFieldValuesToCreate[i].CustomFieldValueDescription, customFieldValuesToCreate[i + 1].CustomFieldValueDescription) > 0)
                    {
                        alphabeticalOrder = false;
                    }
                }

                // Clear the collection and add the new value to save, then save it.
                customFieldValueDTOs.Clear();
                customFieldValueDTOs.Add(customFieldValuesToCreate[i]);

                using (TransactionScope scope = new TransactionScope())
                {
                    sut.Save(customFieldValueDTOs);
                    scope.Complete();
                }
            }

            // Ensure that the initial CustomFieldValues were not in alphabetical order so that the test result will be conclusive.
            Assert.IsFalse(alphabeticalOrder, "Initial CustomFieldValues were already in alphabetical order, so the test result will be inconclusive.");

            // Get the values that were saved (for BOTH Custom Fields), then ensure the correct number of items were created.
            ICollection<CustomFieldValueDTO> customFieldValuesAfterSave = sut.GetCustomFieldValueDTOsByCustomFieldIds(new Collection<int>() { GlobalTestCaseSetup.GlobalCustomFieldID, customFieldID2 });
            Assert.AreEqual(customFieldValuesBeforeSave.Count() + customFieldValuesToCreate.Count(), customFieldValuesAfterSave.Count);

            // Ensure default alphabetical order by CustomFieldValueDescription of the saved items.
            for (int i = 0; i < customFieldValuesAfterSave.Count - 1; i++)
            {
                Assert.IsTrue(string.Compare(customFieldValuesAfterSave.ElementAt(i).CustomFieldValueDescription, customFieldValuesAfterSave.ElementAt(i + 1).CustomFieldValueDescription) < 1, "CustomFieldValues are not sorted alphabetically by CustomFieldValueDescription.");
            }

            // Clean up the items we added to reset the test.
            ICollection<CustomFieldValueDTO> customFieldValuesToDelete = customFieldValuesAfterSave.Where(cfvA => !customFieldValuesBeforeSave.Any(cfvB => cfvB.CustomFieldValueID == cfvA.CustomFieldValueID)).ToList();
            foreach (CustomFieldValueDTO customFieldValueToDelete in customFieldValuesToDelete)
            {
                customFieldValueToDelete.Updateable = UpdateType.Deleted;
            }

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(customFieldValuesToDelete);
                scope.Complete();
            }
        }
    }
}
