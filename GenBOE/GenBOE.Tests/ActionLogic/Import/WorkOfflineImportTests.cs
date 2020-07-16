// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Import
{
    using System;
    using System.Web.Script.Serialization;
    using GenBOE.ActionLogic.IO.Import;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WorkOfflineImportTests
    {
        // [TestMethod]
        // [Ignore] // ToDo: This needs to be either fixed or removed???
        public void WorkofflineImport_SerializationTest()
        {
            //Arrange
            WorkofflineImport importObject = CreateNewOfflineImportObject();
            JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            string serializedOfflineImport = serializer.Serialize(importObject);
            JavaScriptSerializer deserializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            //string serializedOfflineImport = JsonConvert.SerializeObject(importObject);


            //Act
            WorkofflineImport deserializedImportData =
                deserializer.Deserialize<WorkofflineImport>(serializedOfflineImport);
            //WorkofflineImport deserializedImportData =
            //    JsonConvert.DeserializeObject<WorkofflineImport>(serializedOfflineImport);


            //Assert
            //BoeDTO - WorkofflineImportedBoe Data
            Assert.AreEqual(importObject.ImportedBoes[0].StartDate,
                deserializedImportData.ImportedBoes[0].StartDate);
            Assert.AreEqual(importObject.ImportedBoes[0].EndDate,
                deserializedImportData.ImportedBoes[0].EndDate);
            Assert.AreEqual(importObject.ImportedBoes[0].DataSource,
                deserializedImportData.ImportedBoes[0].DataSource);
            Assert.AreEqual(importObject.ImportedBoes[0].Description,
                deserializedImportData.ImportedBoes[0].Description);
            Assert.AreEqual(importObject.ImportedBoes[0].Id,
                deserializedImportData.ImportedBoes[0].Id);
            Assert.AreEqual(importObject.ImportedBoes[0].Title,
                deserializedImportData.ImportedBoes[0].Title);

            //BoeTaskElementDTO - WorkofflineImportedTaskElement Data
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].StartDate,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].StartDate);
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].EndDate,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].EndDate);
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].Description,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].Description);
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].MOQHoursEquation,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].MOQHoursEquation);
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].MOQText,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].MOQText);
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].MOQType,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].MOQType);
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].BoeID,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].BoeID);
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].Id,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].Id);
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].TaskElementType,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].TaskElementType);
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].BOETaskID,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].BOETaskID);
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].TaskTitle,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].TaskTitle);

            //BOELaborType - WorkofflineImportedResourceType Data
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].Id,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].Id);
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].BoeID,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].BoeID);
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].StartDateValue,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].StartDateValue);
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].EndDateValue,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].EndDateValue);
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].PercentSpread,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].PercentSpread);
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].ValueSpread,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].ValueSpread);
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].ResourceID,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].ResourceID);
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].PerformingOrgID,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].PerformingOrgID);
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].SpreadCurveID,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].SpreadCurveID);

            //BoeLaborSpread - WorkofflineImportedResourceSpread Data
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].ImportedResourceSpreads[0].BoeID,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].ImportedResourceSpreads[0].BoeID);
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].ImportedResourceSpreads[0].Id,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].ImportedResourceSpreads[0].Id);
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].ImportedResourceSpreads[0].LaborSpreadDate,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].ImportedResourceSpreads[0].LaborSpreadDate);
            Assert.AreEqual(importObject.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].ImportedResourceSpreads[0].LaborSpreadValue,
                deserializedImportData.ImportedBoes[0].ImportedTaskElements[0].ImportedResourceTypes[0].ImportedResourceSpreads[0].LaborSpreadValue);
        }


        private WorkofflineImport CreateNewOfflineImportObject()
        {
            int uniqueIdCounter = -1;
            WorkofflineImport toReturn = new WorkofflineImport();
            toReturn.ImportTypes.Add(WorkofflineImportResult.None);

            //BOE Updates
            WorkofflineImportedBoe updatedBoe = new WorkofflineImportedBoe();
            updatedBoe.ImportTypes.Add(BoeImportResult.UpdateBoe);
            updatedBoe.Id = 32613;
            updatedBoe.Title = "new test - Imported Title - 2";
            updatedBoe.Description = "new test - Text exists in genBOE.  Character limitations or existing rich text formatting restrict the export of this data.  Do not edit or remove this placeholder.  Edits made to this placeholder will replace data within genBOE.";
            updatedBoe.DataSource = "new test - Text exists in genBOE.  Character limitations or existing rich text formatting restrict the export of this data.  Do not edit or remove this placeholder.  Edits made to this placeholder will replace data within genBOE.";
            updatedBoe.StartDate = DateTime.Parse("04/15/2013");
            updatedBoe.EndDate = DateTime.Parse("08/15/2014");

            //Task Element update
            WorkofflineImportedTaskElement updatedTaskElement = new WorkofflineImportedTaskElement();
            updatedTaskElement.Id = uniqueIdCounter--;
            updatedTaskElement.BoeID = 32613;
            updatedTaskElement.TaskElementType = TaskElementType.Labor;
            updatedTaskElement.BOETaskID = uniqueIdCounter.ToString() + " new test - TaskID1";
            updatedTaskElement.TaskTitle = uniqueIdCounter.ToString() + " new test - Task Title #1";
            updatedTaskElement.StartDate = DateTime.Parse("04/15/2013");
            updatedTaskElement.EndDate = DateTime.Parse("08/15/2014");
            updatedTaskElement.MOQHoursEquation = uniqueIdCounter.ToString() + "*200*24";
            updatedTaskElement.MOQType = MOQType.Factor;
            updatedTaskElement.MOQText = uniqueIdCounter.ToString() + " new test - Text exists in genBOE.  Character limitations or existing rich text formatting restrict the export of this data.  Do not edit or remove this placeholder.  Edits made to this placeholder will replace data within genBOE.";


            WorkofflineImportedResourceType updatedResourceType = new WorkofflineImportedResourceType();
            updatedResourceType.Id = uniqueIdCounter--;
            updatedResourceType.BoeID = 32613;
            updatedResourceType.StartDateValue = DateTime.Parse("04/15/2013");  //these fields set via *DateValue property
            updatedResourceType.EndDateValue = DateTime.Parse("08/15/2014");
            updatedResourceType.PercentSpread = 0.0m;
            updatedResourceType.ValueSpread = null;
            updatedResourceType.HourSpreadLocked = true;
            updatedResourceType.PercentSpreadLocked = false;

            //these are FK ID's to other tables, how does this work?
            updatedResourceType.ResourceID = 8185;
            updatedResourceType.PerformingOrgID = 939;
            updatedResourceType.SpreadCurveID = SpreadCurves.SpreadCurve1;


            WorkofflineImportedResourceSpread aprResourceSpread = new WorkofflineImportedResourceSpread();
            aprResourceSpread.BoeID = 32613;
            aprResourceSpread.Id = uniqueIdCounter--;
            aprResourceSpread.LaborSpreadDate = DateTime.Parse("04/15/2013");
            aprResourceSpread.LaborSpreadValue = 0.0m;
            aprResourceSpread.ImportTypes.Add(LaborSpreadImportResult.UpdateSpread);

            WorkofflineImportedResourceSpread mayResourceSpread = new WorkofflineImportedResourceSpread();
            mayResourceSpread.BoeID = 32613;
            mayResourceSpread.Id = uniqueIdCounter--;
            mayResourceSpread.LaborSpreadDate = DateTime.Parse("05/15/2013");
            mayResourceSpread.LaborSpreadValue = 5000m;
            mayResourceSpread.ImportTypes.Add(LaborSpreadImportResult.UpdateSpread);

            WorkofflineImportedResourceSpread junResourceSpread = new WorkofflineImportedResourceSpread();
            junResourceSpread.BoeID = 32613;
            junResourceSpread.Id = uniqueIdCounter--;
            junResourceSpread.LaborSpreadDate = DateTime.Parse("06/15/2013");
            junResourceSpread.LaborSpreadValue = 1234m;
            junResourceSpread.ImportTypes.Add(LaborSpreadImportResult.UpdateSpread);

            WorkofflineImportedResourceSpread julResourceSpread = new WorkofflineImportedResourceSpread();
            julResourceSpread.BoeID = 32613;
            julResourceSpread.Id = uniqueIdCounter--;
            julResourceSpread.LaborSpreadDate = DateTime.Parse("07/15/2013");
            julResourceSpread.LaborSpreadValue = 0.0m;
            julResourceSpread.ImportTypes.Add(LaborSpreadImportResult.UpdateSpread);

            WorkofflineImportedResourceSpread augResourceSpread = new WorkofflineImportedResourceSpread();
            augResourceSpread.BoeID = 32613;
            augResourceSpread.Id = uniqueIdCounter--;
            augResourceSpread.LaborSpreadDate = DateTime.Parse("08/15/2013");
            augResourceSpread.LaborSpreadValue = 0.0m;
            julResourceSpread.ImportTypes.Add(LaborSpreadImportResult.UpdateSpread);


            //build collections
            updatedResourceType.ImportTypes.Add(LaborTypeImportResult.UpdateLaborType);
            updatedResourceType.ImportedResourceSpreads.Add(aprResourceSpread);
            updatedResourceType.ImportedResourceSpreads.Add(mayResourceSpread);
            updatedResourceType.ImportedResourceSpreads.Add(junResourceSpread);
            updatedResourceType.ImportedResourceSpreads.Add(julResourceSpread);
            updatedResourceType.ImportedResourceSpreads.Add(augResourceSpread);

            updatedTaskElement.ImportTypes.Add(TaskElementImportResult.UpdateTaskElement);
            updatedTaskElement.ImportedResourceTypes.Add(updatedResourceType);

            updatedBoe.ImportTypes.Add(BoeImportResult.UpdateBoe);
            updatedBoe.ImportedTaskElements.Add(updatedTaskElement);

            toReturn.ImportedBoes.Add(updatedBoe);


            return toReturn;
        }
    }
}
