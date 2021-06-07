// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestClass]
    public class ExportReportsTest : MOQObject
    {
        Mock<IRetriever> retriever = new Mock<IRetriever>();
        Mock<ICommonDataMapper> commonDataMapper = new Mock<ICommonDataMapper>();
        Mock<IPermissionsDTODataLoader> permissionsDataLoader = new Mock<IPermissionsDTODataLoader>();
        Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
        Mock<ISystemSettingDTODataLoader> systemSettingLoader = new Mock<ISystemSettingDTODataLoader>();

        public static ProPricerDTO SetUpProPricerDTO(int workspaceId, bool allCustomFields = false, bool useEP = false, bool includeCustomFields = true)
        {
            //set up a ProPricer Export
            ProPricerDTO proPricerExport = new ProPricerDTO();
            string guidstring = "MOCK" + Guid.NewGuid().ToString();
            proPricerExport.ExportID = 1;
            proPricerExport.FormatName = guidstring;
            proPricerExport.WorkspaceID = workspaceId;
            proPricerExport.Scope = IES.Common.ProPricerScope.Workspace;

            ProPricerTasks ppTask1 = new ProPricerTasks();
            ppTask1.ListOrder = 0;
            ppTask1.Task = ProPricerField_Task.BOEStartDate;

            ProPricerTasks ppTask2 = new ProPricerTasks();
            ppTask2.ListOrder = 1;
            ppTask2.Task = ProPricerField_Task.BLANK;

            ProPricerTasks ppTask3 = new ProPricerTasks();
            ppTask3.ListOrder = 2;
            ppTask3.Task = ProPricerField_Task.TOTAL;

            ProPricerTasks ppTask4 = new ProPricerTasks();
            ppTask4.ListOrder = 3;
            ppTask4.Task = ProPricerField_Task.CLINNumber;

            ProPricerTasks ppTask5 = new ProPricerTasks();
            ppTask5.ListOrder = 4;
            ppTask5.Task = ProPricerField_Task.BOEEndDate;

            ProPricerTasks ppTask6 = new ProPricerTasks();
            ppTask6.ListOrder = 5;
            ppTask6.Task = ProPricerField_Task.ProPricerTaskID;

            ProPricerTasks customTask = new ProPricerTasks();
            customTask.CustomFieldID = 1;
            customTask.ListOrder = 6;
            customTask.Selection = ProPricerCustomFieldSelection.CustomFieldID;

            ProPricerTasks ppTask7 = new ProPricerTasks();
            ppTask7.ListOrder = includeCustomFields ? 7 : 6;
            ppTask7.Task = ProPricerField_Task.TaskTitle;

            ProPricerTasks ppTask8 = new ProPricerTasks();
            ppTask8.ListOrder = includeCustomFields ? 8 : 7;
            ppTask8.Task = ProPricerField_Task.CLINTitle;

            ProPricerTasks ppTask9 = new ProPricerTasks();
            ppTask9.ListOrder = includeCustomFields ? 9 : 8;
            ppTask9.Task = ProPricerField_Task.WBSTitle;

            ProPricerTasks ppTask10 = new ProPricerTasks();
            ppTask10.ListOrder = includeCustomFields ? 10 : 9;
            ppTask10.Task = ProPricerField_Task.WBSNumber;

            //task level      
            ProPricerTasks ppTask11 = new ProPricerTasks();
            ppTask11.ListOrder = 11;
            ppTask11.CustomFieldID = 1;
            ppTask11.Selection = ProPricerCustomFieldSelection.CustomFieldDescription;

            //resource level
            ProPricerTasks ppTask12 = new ProPricerTasks();
            ppTask12.CustomFieldID = 2;
            ppTask12.ListOrder = 12;
            ppTask12.Selection = ProPricerCustomFieldSelection.CustomFieldDescription;

            ProPricerTasks ppTask13 = new ProPricerTasks();
            ppTask13.CustomFieldID = 2;
            ppTask13.ListOrder = 13;
            ppTask13.Selection = ProPricerCustomFieldSelection.CustomFieldID;

            ProPricerTasks ppTask14 = new ProPricerTasks();
            ppTask14.ListOrder = includeCustomFields ? 14 : 10;
            ppTask14.Task = ProPricerField_Task.ResourceID;

            ProPricerTasks ppTask15 = new ProPricerTasks();
            ppTask15.ListOrder = includeCustomFields ? 15 : 11;
            ppTask15.Task = ProPricerField_Task.PerformingOrg;

            ProPricerTasks ppTask16 = new ProPricerTasks();
            ppTask16.ListOrder = includeCustomFields ? 16 : 12;
            ppTask16.Task = ProPricerField_Task.MOQType;

            ProPricerTasks ppTask17 = new ProPricerTasks();
            ppTask17.ListOrder = includeCustomFields ? 17 : 13;
            ppTask17.Task = ProPricerField_Task.GenBOEBOEID;

            ProPricerTasks ppTask18 = new ProPricerTasks();
            ppTask18.ListOrder = includeCustomFields ? 18 : 14;
            ppTask18.Task = ProPricerField_Task.GenBOETaskID;

            ProPricerTasks ppTask19 = new ProPricerTasks();
            ppTask19.ListOrder = includeCustomFields ? 19 : 15;
            ppTask19.Task = ProPricerField_Task.GenBOEResourceID;

            ProPricerTasks ppTask20 = new ProPricerTasks();
            ppTask20.ListOrder = includeCustomFields ? 20 : 16;
            ppTask20.Task = ProPricerField_Task.BOETitle;

            ProPricerTasks ppTask21 = new ProPricerTasks();
            ppTask21.ListOrder = includeCustomFields ? 21 : 17;
            ppTask21.Task = ProPricerField_Task.TaskID;

            Collection<ProPricerTasks> tasks = includeCustomFields ? new Collection<ProPricerTasks> { ppTask1, ppTask2, ppTask3, ppTask4, ppTask5, ppTask6, customTask, ppTask7, ppTask8, ppTask9, ppTask10, ppTask11, ppTask12, ppTask13,
                ppTask14, ppTask15, ppTask16, ppTask17, ppTask18, ppTask19, ppTask20, ppTask21 } :
                new Collection<ProPricerTasks> { ppTask1, ppTask2, ppTask3, ppTask4, ppTask5, ppTask6, ppTask7, ppTask8, ppTask9, ppTask10,
                ppTask14, ppTask15, ppTask16, ppTask17, ppTask18, ppTask19, ppTask20, ppTask21 };

            if (allCustomFields)
            {
                //boe level
                ProPricerTasks ppTask22 = new ProPricerTasks();
                ppTask22.CustomFieldID = 3;
                ppTask22.ListOrder = 22;
                ppTask22.Selection = ProPricerCustomFieldSelection.CustomFieldDescription;

                ProPricerTasks ppTask23 = new ProPricerTasks();
                ppTask23.CustomFieldID = 3;
                ppTask23.ListOrder = 23;
                ppTask23.Selection = ProPricerCustomFieldSelection.CustomFieldID;

                tasks.Add(ppTask22);
                tasks.Add(ppTask23);
            }

            proPricerExport.ProPricerTasks = tasks;

            ProPricerResources ppResource = new ProPricerResources();
            ppResource.ListOrder = 0;
            ppResource.Resource = ProPricerField_Resources.IMSCode;

            ProPricerResources ppResource2 = new ProPricerResources();
            ppResource2.ListOrder = 1;
            ppResource2.Resource = ProPricerField_Resources.ResourceID;

            ProPricerResources ppResource3 = new ProPricerResources();
            ppResource3.ListOrder = 2;
            ppResource3.Resource = ProPricerField_Resources.ProPricerTaskID;

            ProPricerResources ppResource4 = new ProPricerResources();
            ppResource4.ListOrder = 3;
            ppResource4.Resource = ProPricerField_Resources.WBSNumber;

            // resource level
            ProPricerResources customResource = new ProPricerResources();
            customResource.CustomFieldID = 2;
            customResource.ListOrder = 4;
            customResource.Selection = ProPricerCustomFieldSelection.CustomFieldDescription;

            ProPricerResources customResource2 = new ProPricerResources();
            customResource2.CustomFieldID = 2;
            customResource2.ListOrder = 5;
            customResource2.Selection = ProPricerCustomFieldSelection.CustomFieldID;

            ProPricerResources ppResource6 = new ProPricerResources();
            ppResource6.ListOrder = includeCustomFields ? 6 : 4;
            ppResource6.Resource = ProPricerField_Resources.BLANK;

            ProPricerResources ppResource7 = new ProPricerResources();
            ppResource7.ListOrder = includeCustomFields ? 7 : 5;
            ppResource7.Resource = ProPricerField_Resources.DISCRETE;

            ProPricerResources ppResource8 = new ProPricerResources();
            ppResource8.ListOrder = includeCustomFields ? 8 : 6;
            ppResource8.Resource = ProPricerField_Resources.CLINNumber;

            ProPricerResources ppResource9 = new ProPricerResources();
            ppResource9.ListOrder = includeCustomFields ? 9 : 7;
            ppResource9.Resource = ProPricerField_Resources.CLINTitle;

            ProPricerResources ppResource10 = new ProPricerResources();
            ppResource10.ListOrder = includeCustomFields ? 10 : 8;
            ppResource10.Resource = ProPricerField_Resources.StartDate;

            ProPricerResources ppResource11 = new ProPricerResources();
            ppResource11.ListOrder = includeCustomFields ? 11 : 9;
            ppResource11.Resource = ProPricerField_Resources.WBSTitle;

            ProPricerResources ppResource12 = new ProPricerResources();
            ppResource12.ListOrder = includeCustomFields ? 12 : 10;
            ppResource12.Resource = ProPricerField_Resources.GenBOEBOEID;

            ProPricerResources ppResource13 = new ProPricerResources();
            ppResource13.ListOrder = includeCustomFields ? 13 : 11;
            ppResource13.Resource = ProPricerField_Resources.GenBOETaskID;

            ProPricerResources ppResource14 = new ProPricerResources();
            ppResource14.ListOrder = includeCustomFields ? 14 : 12;
            ppResource14.Resource = ProPricerField_Resources.GenBOEResourceID;

            ProPricerResources ppResource15 = new ProPricerResources();
            ppResource15.ListOrder = includeCustomFields ? 15 : 13;
            ppResource15.Resource = ProPricerField_Resources.BOETitle;

            ProPricerResources ppResource16 = new ProPricerResources();
            ppResource16.ListOrder = includeCustomFields ? 16 : 14;
            ppResource16.Resource = ProPricerField_Resources.PerformingOrg;

            ProPricerResources ppResource17 = new ProPricerResources();
            ppResource17.ListOrder = includeCustomFields ? 17 : 15;
            ppResource17.Resource = ProPricerField_Resources.TaskTitle;

            ProPricerResources ppResource18 = new ProPricerResources();
            ppResource18.ListOrder = includeCustomFields ? 18 : 16;
            ppResource18.Resource = ProPricerField_Resources.TaskID;



            Collection<ProPricerResources> PPresources = includeCustomFields ? new Collection<ProPricerResources> { ppResource, ppResource2, ppResource3, ppResource4, customResource, customResource2, ppResource6, ppResource7, ppResource8,
                ppResource9, ppResource10, ppResource11, ppResource12, ppResource13, ppResource14, ppResource15, ppResource16, ppResource17, ppResource18 } :
                new Collection<ProPricerResources> { ppResource, ppResource2, ppResource3, ppResource4, ppResource6, ppResource7, ppResource8,
                ppResource9, ppResource10, ppResource11, ppResource12, ppResource13, ppResource14, ppResource15, ppResource16, ppResource17, ppResource18 };

            if (useEP)
            {
                // set using ep config option to true 
                System.Configuration.ConfigurationManager.AppSettings["ShowEquivalentPersonsOption"] = "true";
                ProPricerResources ppResource19 = new ProPricerResources();
                ppResource19.ListOrder = 19;
                ppResource19.Resource = ProPricerField_Resources.EP;
                PPresources.Add(ppResource19);
            }

            if (allCustomFields)
            {
                // task level
                ProPricerResources ppResource20 = new ProPricerResources();
                ppResource20.CustomFieldID = 1;
                ppResource20.ListOrder = 20;
                ppResource20.Selection = ProPricerCustomFieldSelection.CustomFieldDescription;

                ProPricerResources ppResource21 = new ProPricerResources();
                ppResource20.CustomFieldID = 1;
                ppResource20.ListOrder = 21;
                ppResource20.Selection = ProPricerCustomFieldSelection.CustomFieldID;

                // boe level
                ProPricerResources ppResource22 = new ProPricerResources();
                ppResource21.CustomFieldID = 3;
                ppResource21.ListOrder = 22;
                ppResource21.Selection = ProPricerCustomFieldSelection.CustomFieldDescription;

                ProPricerResources ppResource23 = new ProPricerResources();
                ppResource22.CustomFieldID = 3;
                ppResource22.ListOrder = 23;
                ppResource22.Selection = ProPricerCustomFieldSelection.CustomFieldID;

                PPresources.Add(ppResource20);
                PPresources.Add(ppResource21);
                PPresources.Add(ppResource22);
                PPresources.Add(ppResource23);
            }

            proPricerExport.ProPricerResources = PPresources;

            return proPricerExport;
        }


        [TestMethod, System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public void BL_ExportProPricer_EP()
        {
            Collection<BoeDTO> toReturn = new Collection<BoeDTO>();
            toReturn.Add(this.Boe1);

            CustomFieldDTO customField1 = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.TaskDisplay };
            CustomFieldDTO customField2 = new CustomFieldDTO { Id = 2, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay };

            CustomFieldValueDTO cv = new CustomFieldValueDTO();
            cv.CustomFieldID = customField1.Id;
            cv.CustomFieldValueID = 1;
            cv.CustomFieldValueName = "P<br/><BR /><bR/>E";
            cv.CustomFieldValueDescription = "an exercise<BR /> class";

            CustomFieldValueDTO cv2 = new CustomFieldValueDTO();
            cv2.CustomFieldID = customField2.Id;
            cv2.CustomFieldValueID = 2;
            cv2.CustomFieldValueName = "History";
            cv2.CustomFieldValueDescription = "where you<br /><BR/><BR><br><br / > learn past events";

            DateTime LaborstartDate = Convert.ToDateTime("11/01/2010");
            DateTime LaborendDate = Convert.ToDateTime("01/01/2011");

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsDataLoader.Object);

            FullWorkspace workspace = new FullWorkspace(this.Workspace);

            workspace.IsUsingEquivalentPerson = true;

            var beforeTest = System.Configuration.ConfigurationManager.AppSettings["ShowEquivalentPersonsOption"];
            System.Configuration.ConfigurationManager.AppSettings["ShowEquivalentPersonsOption"] = "true";
            FullObjectHelper.RefreshEPForTests();

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullBoe>() { new FullBoe(this.Boe1), new FullBoe(this.Boe2) });
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullWbs>() { new FullWbs(this.Wbs) });
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullClin>() { new FullClin(this.Clin1) });
            this.retriever.Setup(x => x.GetClinById(this.Boe1.CLINID.Value)).Returns(this.Clin1);
            this.retriever.Setup(x => x.GetWbsById(this.Wbs.Id)).Returns(this.Wbs);
            this.retriever.Setup(x => x.GetMoqTypeSelectionsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<MoqTypeSelection>());
            workspace.UsingTemplateBOE = false;

            // setup resources dto mapper
            Collection<ResourceDTO> resources = new Collection<ResourceDTO>();
            resources.Add(this.Resource);

            this.retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new List<CustomFieldValueDTO>() { cv, cv2 });
            this.retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>())).Returns(new List<PerformingOrgDTO>() { this.Perforg });
            this.retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(resources);
            this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(resources);
            this.retriever.Setup(x => x.GetFullWorkspaceById(workspace.Id)).Returns(workspace);
            this.factory.Setup(x => x.CreateFullWorkspace(It.IsAny<int>())).Returns(workspace);
            this.factory.Setup(x => x.CreateFullBoes(It.IsAny<ICollection<int>>())).Returns(new List<FullBoe>() { new FullBoe(this.Boe1) });
            foreach (FullBoe boe in workspace.Boes)
            {
                boe.Title = "test boe";
            }

            BoeTaskElementDTO task1 = new BoeTaskElementDTO
            {
                BoeID = this.Boe1.Id,
                Id = 1,
                BOETaskID = "123",
                TaskTitle = "MOCK TASK1",
                Description = "MOCK TASK1",
                StartDate = this.Boe1.StartDate,
                EndDate = this.Boe1.EndDate,
                IMS_ID = "500",
                MOQType = MOQType.Comparison,
                CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { new CustomFieldValueContainer { CustomFieldValueID = cv.CustomFieldValueID } },
                taskElementLabors = new Collection<ResourceTypeDto>{new ResourceTypeDto{Id=1, SpreadType = IES.Common.SpreadType.Cost, StartDateValue=LaborstartDate, EndDateValue=LaborendDate, ResourceID=this.Resource.Id, PerformingOrgID=this.Perforg.Id,SpreadCurveID = SpreadCurves.DiscreteHours,
                            LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=1, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=200 },
                            //discrete values of 0 are not stored in the db so replicate that here by removing a month
                            new ResourceSpreadDto{Id=3, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=200 }}
                        }}
            };
            //Two resources for this task to test multiple rows for tasks with multiple resources
            BoeTaskElementDTO task2 = new BoeTaskElementDTO
            {
                BoeID = this.Boe2.Id,
                Id = 2,
                BOETaskID = "1234",
                TaskTitle = "MOCK TASK2",
                Description = "MOCK TASK2",
                StartDate = this.Boe1.StartDate,
                EndDate = this.Boe1.EndDate,
                MOQType = MOQType.Factor,
                taskElementLabors = new Collection<ResourceTypeDto>{new ResourceTypeDto{Id=2, SpreadType = IES.Common.SpreadType.Hours, StartDateValue=LaborstartDate, EndDateValue=LaborendDate, ResourceID=this.Resource.Id, PerformingOrgID=this.Perforg.Id,  SpreadCurveID = SpreadCurves.DiscreteHours, CustomFieldValueContainers=new Collection<CustomFieldValueContainer>{new CustomFieldValueContainer{CustomFieldValueID=cv2.CustomFieldValueID}},
                            LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=4, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=150 },
                            new ResourceSpreadDto{Id=5, LaborSpreadDate=Convert.ToDateTime("12/01/2010"), LaborSpreadValue=150 },
                            new ResourceSpreadDto{Id=6, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=150 }}
                            },new ResourceTypeDto{Id=3, SpreadType = IES.Common.SpreadType.Hours, StartDateValue=LaborstartDate, EndDateValue=LaborendDate, ResourceID=this.Resource.Id, PerformingOrgID=this.Perforg.Id,  SpreadCurveID = SpreadCurves.DiscreteHours, CustomFieldValueContainers=new Collection<CustomFieldValueContainer>{new CustomFieldValueContainer{CustomFieldValueID=cv2.CustomFieldValueID}},
                            LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=4, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=50 },
                            new ResourceSpreadDto{Id=5, LaborSpreadDate=Convert.ToDateTime("12/01/2010"), LaborSpreadValue=150 },
                            new ResourceSpreadDto{Id=6, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=250 }}
                            }}
            };

            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { task1, task2 });
            this.retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, false)).Returns(new Collection<TravelDTO> { });
            this.retriever.Setup(x => x.GetMaterialsByBoeIds(It.IsAny<Collection<int>>(), false)).Returns(new Collection<MaterialDTO> { });
            this.retriever.Setup(x => x.GetOdcCollectionByBoeIds(It.IsAny<List<int>>(), false)).Returns(new Collection<OtherDirectCostDTO> { });

            retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<CustomFieldDTO> { customField1, customField2 });

            var TripCalculate = new Mock<TravelTripCostCalculation>();
            var rmsTripCalculate = new Mock<RMSZoneTravelRatesFeesDataLoader>();
            rmsTripCalculate.Setup(x => x.getAllFeesAndCostsByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSTravelNonzoneFeesAndCostsDTO>());
            rmsTripCalculate.Setup(x => x.getAllEscalationRatesByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSEscalationRatesDTO>());

            ProPricerExporter expReport = new ProPricerExporter(TripCalculate.Object, rmsTripCalculate.Object, this.retriever.Object, this.commonDataMapper.Object);

            ProPricerDTO proPricerExport = SetUpProPricerDTO(this.Workspace.Id, false, true);

            PpDataReadyForExport result = expReport.ExportProPricer(proPricerExport, workspace);
            Collection<string> TaskData = result.TaskData.ToCollection();
            Collection<string> ResourceCost = result.ResourceData.ToCollection();

            Assert.IsTrue(TaskData != null, "Task Data is not null");
            Assert.IsTrue(ResourceCost != null, "Resource Cost data is not null");

            //  there is no other way to verify the results other than hard coding string values
            Assert.AreEqual("112005,,TOTAL,1.1,012006,LIDN000001,\"PE\",\"MOCK TASK1\",\"mock clin1 title\",\"mock wbs title\",2.2,\"an exercise class\",,,\"BBBBBBB\",\"KristinePO\",\"Comparison\",20,1,1,\"test boe\",\"123\",", TaskData[0], "The first task did not match this value");
            Assert.AreEqual("112005,,TOTAL,,012006,LIDN000002,,\"MOCK TASK2\",\"\",\"mock wbs title\",2.2,,\"where you learn past events\",\"History\",\"BBBBBBB\",\"KristinePO\",\"Factor\",21,2,2,\"test boe\",\"1234\",", TaskData[1], "The second task (first resource) did not match this value");
            Assert.AreEqual("112005,,TOTAL,,012006,LIDN000003,,\"MOCK TASK2\",\"\",\"mock wbs title\",2.2,,\"where you learn past events\",\"History\",\"BBBBBBB\",\"KristinePO\",\"Factor\",21,2,3,\"test boe\",\"1234\",", TaskData[2], "The second task (second resource) did not match this value");

            Assert.AreEqual("500,\"BBBBBBB\",LIDN000001,2.2,,,,D,1.1,\"mock clin1 title\",112010,\"mock wbs title\",20,1,1,\"test boe\",\"KristinePO\",\"MOCK TASK1\",\"123\",,200.00,0,200.00,", ResourceCost[0], "The first resource did not match this value");
            Assert.AreEqual(",\"BBBBBBB\",LIDN000002,2.2,\"where you learn past events\",\"History\",,D,,\"\",112010,\"mock wbs title\",21,2,2,\"test boe\",\"KristinePO\",\"MOCK TASK2\",\"1234\",\"E\",150,150,150,", ResourceCost[1], "The second resource did not match this value");
            Assert.AreEqual(",\"BBBBBBB\",LIDN000003,2.2,\"where you learn past events\",\"History\",,D,,\"\",112010,\"mock wbs title\",21,2,3,\"test boe\",\"KristinePO\",\"MOCK TASK2\",\"1234\",\"E\",50,150,250,", ResourceCost[2], "The third resource did not match this value");

            System.Configuration.ConfigurationManager.AppSettings["ShowEquivalentPersonsOption"] = beforeTest;
            FullObjectHelper.RefreshEPForTests();
        }

        [TestMethod, System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public void BL_ExportProPricer_LaborElementsOnly()
        {
            Collection<BoeDTO> toReturn = new Collection<BoeDTO>();
            toReturn.Add(this.Boe1);

            CustomFieldDTO customField1 = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.TaskDisplay };
            CustomFieldDTO customField2 = new CustomFieldDTO { Id = 2, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay };

            CustomFieldValueDTO cv = new CustomFieldValueDTO();
            cv.CustomFieldID = customField1.Id;
            cv.CustomFieldValueID = 1;
            cv.CustomFieldValueName = "P<br/><BR /><bR/>E";
            cv.CustomFieldValueDescription = "an exercise<BR /> class";

            CustomFieldValueDTO cv2 = new CustomFieldValueDTO();
            cv2.CustomFieldID = customField2.Id;
            cv2.CustomFieldValueID = 2;
            cv2.CustomFieldValueName = "History";
            cv2.CustomFieldValueDescription = "where you<br /><BR/><BR><br><br / > learn past events";

            DateTime LaborstartDate = Convert.ToDateTime("11/01/2010");
            DateTime LaborendDate = Convert.ToDateTime("01/01/2011");

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsDataLoader.Object);
             
            FullWorkspace workspace = new FullWorkspace(this.Workspace);

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullBoe>() { new FullBoe(this.Boe1), new FullBoe(this.Boe2) });
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullWbs>() { new FullWbs(this.Wbs) });
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullClin>() { new FullClin(this.Clin1) });
            this.retriever.Setup(x => x.GetClinById(this.Boe1.CLINID.Value)).Returns(this.Clin1);
            this.retriever.Setup(x => x.GetWbsById(this.Wbs.Id)).Returns(this.Wbs);
            this.retriever.Setup(x => x.GetMoqTypeSelectionsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<MoqTypeSelection>());
            workspace.UsingTemplateBOE = false;

            // setup resources dto mapper
            Collection<ResourceDTO> resources = new Collection<ResourceDTO>();
            resources.Add(this.Resource);

            this.retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new List<CustomFieldValueDTO>() { cv, cv2 });
            this.retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>())).Returns(new List<PerformingOrgDTO>() { this.Perforg });
            this.retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(resources);
            this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(resources);
            this.retriever.Setup(x => x.GetFullWorkspaceById(workspace.Id)).Returns(workspace);
            this.factory.Setup(x => x.CreateFullWorkspace(It.IsAny<int>())).Returns(workspace);
            this.factory.Setup(x => x.CreateFullBoes(It.IsAny<ICollection<int>>())).Returns(new List<FullBoe>() { new FullBoe(this.Boe1) });
            foreach (FullBoe boe in workspace.Boes)
            {
                boe.Title = "test boe";
            }

            BoeTaskElementDTO task1 = new BoeTaskElementDTO
            {
                BoeID = this.Boe1.Id,
                Id = 1,
                BOETaskID = "123",
                TaskTitle = "MOCK TASK1",
                Description = "MOCK TASK1",
                StartDate = this.Boe1.StartDate,
                EndDate = this.Boe1.EndDate,
                IMS_ID = "500",
                MOQType = MOQType.Comparison,
                CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { new CustomFieldValueContainer { CustomFieldValueID = cv.CustomFieldValueID } },
                taskElementLabors = new Collection<ResourceTypeDto>{new ResourceTypeDto{Id=1, SpreadType = IES.Common.SpreadType.Hours, StartDateValue=LaborstartDate, EndDateValue=LaborendDate, ResourceID=this.Resource.Id, PerformingOrgID=this.Perforg.Id,SpreadCurveID = SpreadCurves.DiscreteHours,
                            LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=1, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=200 },
                            //discrete values of 0 are not stored in the db so replicate that here by removing a month
                            new ResourceSpreadDto{Id=3, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=200 }}
                        }}
            };
            //Two resources for this task to test multiple rows for tasks with multiple resources
            BoeTaskElementDTO task2 = new BoeTaskElementDTO
            {
                BoeID = this.Boe2.Id,
                Id = 2,
                BOETaskID = "1234",
                TaskTitle = "MOCK TASK2",
                Description = "MOCK TASK2",
                StartDate = this.Boe1.StartDate,
                EndDate = this.Boe1.EndDate,
                MOQType = MOQType.Factor,
                taskElementLabors = new Collection<ResourceTypeDto>{new ResourceTypeDto{Id=2, SpreadType = IES.Common.SpreadType.Hours, StartDateValue=LaborstartDate, EndDateValue=LaborendDate, ResourceID=this.Resource.Id, PerformingOrgID=this.Perforg.Id,  SpreadCurveID = SpreadCurves.DiscreteHours, CustomFieldValueContainers=new Collection<CustomFieldValueContainer>{new CustomFieldValueContainer{CustomFieldValueID=cv2.CustomFieldValueID}},
                            LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=4, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=150 },
                            new ResourceSpreadDto{Id=5, LaborSpreadDate=Convert.ToDateTime("12/01/2010"), LaborSpreadValue=150 },
                            new ResourceSpreadDto{Id=6, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=150 }}
                            },new ResourceTypeDto{Id=3, SpreadType = IES.Common.SpreadType.Hours, StartDateValue=LaborstartDate, EndDateValue=LaborendDate, ResourceID=this.Resource.Id, PerformingOrgID=this.Perforg.Id,  SpreadCurveID = SpreadCurves.DiscreteHours, CustomFieldValueContainers=new Collection<CustomFieldValueContainer>{new CustomFieldValueContainer{CustomFieldValueID=cv2.CustomFieldValueID}},
                            LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=4, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=50 },
                            new ResourceSpreadDto{Id=5, LaborSpreadDate=Convert.ToDateTime("12/01/2010"), LaborSpreadValue=150 },
                            new ResourceSpreadDto{Id=6, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=250 }}
                            }}
            };

            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { task1, task2 });
            this.retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, false)).Returns(new Collection<TravelDTO> { });
            this.retriever.Setup(x => x.GetMaterialsByBoeIds(It.IsAny<Collection<int>>(), false)).Returns(new Collection<MaterialDTO> { });
            this.retriever.Setup(x => x.GetOdcCollectionByBoeIds(It.IsAny<List<int>>(), false)).Returns(new Collection<OtherDirectCostDTO> { });

            retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<CustomFieldDTO> { customField1, customField2 });

            var TripCalculate = new Mock<TravelTripCostCalculation>();
            var rmsTripCalculate = new Mock<RMSZoneTravelRatesFeesDataLoader>();
            rmsTripCalculate.Setup(x => x.getAllFeesAndCostsByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSTravelNonzoneFeesAndCostsDTO>());
            rmsTripCalculate.Setup(x => x.getAllEscalationRatesByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSEscalationRatesDTO>());

            ProPricerExporter expReport = new ProPricerExporter(TripCalculate.Object, rmsTripCalculate.Object, this.retriever.Object, this.commonDataMapper.Object);

            ProPricerDTO proPricerExport = SetUpProPricerDTO(this.Workspace.Id);

            PpDataReadyForExport result = expReport.ExportProPricer(proPricerExport, workspace);
            Collection<string> TaskData = result.TaskData.ToCollection();
            Collection<string> ResourceCost = result.ResourceData.ToCollection();

            Assert.IsTrue(TaskData != null, "Task Data is not null");
            Assert.IsTrue(ResourceCost != null, "Resource Cost data is not null");

            //  there is no other way to verify the results other than hard coding string values
            Assert.AreEqual("112005,,TOTAL,1.1,012006,LIDN000001,\"PE\",\"MOCK TASK1\",\"mock clin1 title\",\"mock wbs title\",2.2,\"an exercise class\",,,\"BBBBBBB\",\"KristinePO\",\"Comparison\",20,1,1,\"test boe\",\"123\",", TaskData[0], "The first task did not match this value");
            Assert.AreEqual("112005,,TOTAL,,012006,LIDN000002,,\"MOCK TASK2\",\"\",\"mock wbs title\",2.2,,\"where you learn past events\",\"History\",\"BBBBBBB\",\"KristinePO\",\"Factor\",21,2,2,\"test boe\",\"1234\",", TaskData[1], "The second task (first resource) did not match this value");
            Assert.AreEqual("112005,,TOTAL,,012006,LIDN000003,,\"MOCK TASK2\",\"\",\"mock wbs title\",2.2,,\"where you learn past events\",\"History\",\"BBBBBBB\",\"KristinePO\",\"Factor\",21,2,3,\"test boe\",\"1234\",", TaskData[2], "The second task (second resource) did not match this value");

            Assert.AreEqual("500,\"BBBBBBB\",LIDN000001,2.2,,,,D,1.1,\"mock clin1 title\",112010,\"mock wbs title\",20,1,1,\"test boe\",\"KristinePO\",\"MOCK TASK1\",\"123\",200,0,200,", ResourceCost[0], "The first resource did not match this value");
            Assert.AreEqual(",\"BBBBBBB\",LIDN000002,2.2,\"where you learn past events\",\"History\",,D,,\"\",112010,\"mock wbs title\",21,2,2,\"test boe\",\"KristinePO\",\"MOCK TASK2\",\"1234\",150,150,150,", ResourceCost[1], "The second resource did not match this value");
            Assert.AreEqual(",\"BBBBBBB\",LIDN000003,2.2,\"where you learn past events\",\"History\",,D,,\"\",112010,\"mock wbs title\",21,2,3,\"test boe\",\"KristinePO\",\"MOCK TASK2\",\"1234\",50,150,250,", ResourceCost[2], "The third resource did not match this value");
        }

        [TestMethod, System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public void BL_ExportProPricer_TravelTrips()
        {
            DateTime LaborstartDate = Convert.ToDateTime("11/01/2010");
            DateTime LaborendDate = Convert.ToDateTime("01/01/2011");

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsDataLoader.Object);
            FullWorkspace workspace = new FullWorkspace(this.Workspace);

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullBoe>() { new FullBoe(this.Boe1), new FullBoe(this.Boe2) });
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullWbs>() { new FullWbs(this.Wbs) });
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullClin>() { new FullClin(this.Clin1) });
            this.retriever.Setup(x => x.GetClinById(this.Boe1.CLINID.Value)).Returns(this.Clin1);
            this.retriever.Setup(x => x.GetWbsById(this.Wbs.Id)).Returns(this.Wbs);
            PerformingOrgDTO Perforg2 = new PerformingOrgDTO() { Id = 2, IsSystemPerfOrg = true, PerformingOrgDesc = "test2", PerformingOrgName = "test2", Updateable = UpdateType.None };
            this.retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>())).Returns(new List<PerformingOrgDTO> { this.Perforg, Perforg2 });

            // setup resources dto mapper
            Collection<ResourceDTO> resources = new Collection<ResourceDTO>();
            ResourceDTO Laborresource = new ResourceDTO { Id = 2, ResourceName = "BBBBBBB", ResourceDesc = "Lots of Bs", SegRegion = "BB", LaborType = "BBBB", ElementOfCost = ElementOfCostType.LMLabor };
            ResourceDTO TravelResource = new ResourceDTO { Id = 7, ResourceName = "TT", ResourceDesc = "Lots of Ts", SegRegion = "TT", LaborType = "TT", ElementOfCost = ElementOfCostType.Travel, Segment = SegmentType.TS };
            ResourceDTO TravelResource2 = new ResourceDTO { Id = 8, ResourceName = "TD", ResourceDesc = "Lots of TDs", SegRegion = "TD", LaborType = "TD", ElementOfCost = ElementOfCostType.Travel, Segment = SegmentType.DS };

            resources.Add(Laborresource);
            resources.Add(TravelResource);
            resources.Add(TravelResource2);

            this.retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(resources);
            this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(resources);

            BoeTaskElementDTO laborElement1 = new BoeTaskElementDTO
            {
                Id = 6,
                BoeID = this.Boe1.Id,
                BOETaskID = "1234",
                TaskTitle = "MOCK TASK2",
                Description = "MOCK TASK2",
                StartDate = this.Boe1.StartDate,
                EndDate = this.Boe1.EndDate,
                MOQType = MOQType.LevelOfEffort,
                taskElementLabors = new Collection<ResourceTypeDto>{new ResourceTypeDto{Id=7, StartDateValue=LaborstartDate, EndDateValue=LaborendDate, ResourceID=Laborresource.Id, PerformingOrgID=this.Perforg.Id,  SpreadCurveID = SpreadCurves.DiscreteHours,
                            LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=8, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=150 },
                            new ResourceSpreadDto{Id=9, LaborSpreadDate=Convert.ToDateTime("12/01/2010"), LaborSpreadValue=150 },
                            new ResourceSpreadDto{Id=10, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=150 }}
                        }}
            };

            CustomFieldDTO customField1 = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.TaskDisplay, WorkspaceID = workspace.Id };
            CustomFieldDTO customField2 = new CustomFieldDTO { Id = 2, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, WorkspaceID = workspace.Id };

            CustomFieldValueDTO cv = new CustomFieldValueDTO();
            cv.CustomFieldID = customField1.Id;
            cv.CustomFieldValueID = 1;
            cv.CustomFieldValueName = "P<br/><BR /><bR/>E";
            cv.CustomFieldValueDescription = "an exercise<br> class";

            CustomFieldValueDTO cv2 = new CustomFieldValueDTO();
            cv2.CustomFieldID = customField2.Id;
            cv2.CustomFieldValueID = 2;
            cv2.CustomFieldValueName = "Hist<BR />ory";
            cv2.CustomFieldValueDescription = "where you learn past<BR> events";
            this.retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(new Collection<CustomFieldDTO> { customField1, customField2 });
            this.retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new List<CustomFieldValueDTO>() { cv, cv2 });

            TravelTripType travelTrip = new TravelTripType { TravelTripID = 1, TripDate = Convert.ToDateTime("07/01/2011"), BoeID = this.Boe2.Id, GroupID = 1, NumOfDays = 2, NumOfIntervals = 0, NumOfOccurences = 0, NumOfPeople = 4, NumOfTrips = 2, PerfOrgID = this.Perforg.Id, Segment = SegmentType.DS, SystemTripID = 1, CustomFieldValueContainers = new Collection<CustomFieldValueContainer>() { new CustomFieldValueContainer() { ContainerID = 1, CustomFieldValueID = cv2.CustomFieldValueID } } };
            TravelTripType travelTrip2 = new TravelTripType { TravelTripID = 2, TripDate = Convert.ToDateTime("09/01/2011"), BoeID = this.Boe2.Id, GroupID = 2, NumOfDays = 2, NumOfIntervals = 0, NumOfOccurences = 0, NumOfPeople = 4, NumOfTrips = 2, PerfOrgID = this.Perforg.Id, Segment = SegmentType.DS, SystemTripID = 1, CustomFieldValueContainers = new Collection<CustomFieldValueContainer>() { new CustomFieldValueContainer() { ContainerID = 1, CustomFieldValueID = cv2.CustomFieldValueID } } };
            TravelTripType travelTrip3 = new TravelTripType { TravelTripID = 3, TripDate = Convert.ToDateTime("10/01/2011"), BoeID = this.Boe2.Id, GroupID = 3, NumOfDays = 2, NumOfIntervals = 0, NumOfOccurences = 0, NumOfPeople = 4, NumOfTrips = 2, PerfOrgID = this.Perforg.Id, Segment = SegmentType.DS, SystemTripID = 1 };
            TravelTripType travelTrip4 = new TravelTripType { TravelTripID = 4, TripDate = Convert.ToDateTime("12/01/2011"), BoeID = this.Boe2.Id, GroupID = 4, NumOfDays = 2, NumOfIntervals = 0, NumOfOccurences = 0, NumOfPeople = 4, NumOfTrips = 2, PerfOrgID = Perforg2.Id, Segment = SegmentType.DS, SystemTripID = 1 };

            TravelDTO travel = new TravelDTO { Id = 1, TaskTitle = "MOCKTRAVEL", TravelTrips = new Collection<TravelTripType> { travelTrip, travelTrip2, travelTrip3, travelTrip4 }, BoeID = this.Boe2.Id, StartDate = Convert.ToDateTime("07/01/2011"), EndDate = Convert.ToDateTime("12/01/2011") };
            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { laborElement1 });
            this.retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, false)).Returns(new Collection<TravelDTO> { travel });
            this.retriever.Setup(x => x.GetOdcCollectionByBoeIds(new Collection<int> { this.Boe1.Id, this.Boe2.Id }, false)).Returns(new Collection<OtherDirectCostDTO> { });
            this.retriever.Setup(x => x.GetMaterialsByBoeIds(It.IsAny<Collection<int>>(), false)).Returns(new Collection<MaterialDTO> { });
            this.retriever.Setup(x => x.GetMoqTypeSelectionsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<MoqTypeSelection>());
            workspace.UsingTemplateBOE = false;
            
            foreach (FullBoe boe in workspace.Boes)
            {
                if (boe.Id == 20)
                {
                    boe.Title = "Labor/Material BOE";
                }
                else if (boe.Id == 21)
                {
                    boe.Title = "Travel/ODC BOE";
                }
            }

            var TripCalculate = new Mock<TravelTripCostCalculation>();
            var rmsTripCalculate = new Mock<RMSZoneTravelRatesFeesDataLoader>();
            rmsTripCalculate.Setup(x => x.getAllFeesAndCostsByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSTravelNonzoneFeesAndCostsDTO>());
            rmsTripCalculate.Setup(x => x.getAllEscalationRatesByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSEscalationRatesDTO>());

            TripCalculate.Setup(x => x.CalculateTravelCost(travelTrip, workspace)).Returns(new TravelTripCostData(500, 1, 1, 1, 1, 1, 1, 1, 1, 1));
            TripCalculate.Setup(x => x.CalculateTravelCost(travelTrip2, workspace)).Returns(new TravelTripCostData(300, 1, 1, 1, 1, 1, 1, 1, 1, 1));
            TripCalculate.Setup(x => x.CalculateTravelCost(travelTrip3, workspace)).Returns(new TravelTripCostData(500, 1, 1, 1, 1, 1, 1, 1, 1, 1));
            TripCalculate.Setup(x => x.CalculateTravelCost(travelTrip4, workspace)).Returns(new TravelTripCostData(100, 1, 1, 1, 1, 1, 1, 1, 1, 1));

            rmsTripCalculate.Setup(x => x.getAllFeesAndCostsByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSTravelNonzoneFeesAndCostsDTO>());

            ProPricerExporter expReport = new ProPricerExporter(TripCalculate.Object, rmsTripCalculate.Object, this.retriever.Object, this.commonDataMapper.Object);
            ProPricerDTO proPricerExport = SetUpProPricerDTO(this.Workspace.Id);


            PpDataReadyForExport result = expReport.ExportProPricer(proPricerExport, workspace);
            Collection<string> TaskData = result.TaskData.ToCollection();
            Collection<string> ResourceCost = result.ResourceData.ToCollection();

            //should be four cost rows because of the grouping of custom fields and perfor orgs
            Assert.AreEqual(4, ResourceCost.Count);

            //get the values of the cost output

            //because of the grouping by perf org and custom field values we should see 
            Assert.AreEqual(",\"TD\",TIDN000001,2.2,\"where you learn past events\",\"History\",,D,,\"\",072011,\"mock wbs title\",21,1,1 2,\"Travel/ODC BOE\",\"KristinePO\",\"MOCKTRAVEL\",\"\",500,,300,", ResourceCost[1]);
            Assert.AreEqual(",\"TD\",TIDN000001,2.2,,,,D,,\"\",102011,\"mock wbs title\",21,1,3,\"Travel/ODC BOE\",\"KristinePO\",\"MOCKTRAVEL\",\"\",500,", ResourceCost[2]);
            Assert.AreEqual(",\"TD\",TIDN000001,2.2,,,,D,,\"\",122011,\"mock wbs title\",21,1,4,\"Travel/ODC BOE\",\"test2\",\"MOCKTRAVEL\",\"\",100,", ResourceCost[3]);


            //at the task level we should see similar responses.
            Assert.AreEqual(4, TaskData.Count);

            Assert.AreEqual("072011,,TOTAL,,122011,TIDN000001,,\"MOCKTRAVEL\",\"\",\"mock wbs title\",2.2,,\"where you learn past events\",\"History\",21,1,,\"Travel/ODC BOE\",\"\",", TaskData[1]);
            Assert.AreEqual("072011,,TOTAL,,122011,TIDN000001,,\"MOCKTRAVEL\",\"\",\"mock wbs title\",2.2,,,,21,1,,\"Travel/ODC BOE\",\"\",", TaskData[2]);
            Assert.AreEqual("072011,,TOTAL,,122011,TIDN000001,,\"MOCKTRAVEL\",\"\",\"mock wbs title\",2.2,,,,21,1,,\"Travel/ODC BOE\",\"\",", TaskData[3]);
        }

       // [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "TaskData")]
       // [TestMethod, System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        //public void BL_ExportProPricer_MSTTravelTrips()
        //{
        //    DateTime LaborstartDate = Convert.ToDateTime("11/01/2010");
        //    DateTime LaborendDate = Convert.ToDateTime("01/01/2011");

        //    GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
        //    GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
        //    GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
        //    GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsDataLoader.Object);
        //    FullWorkspace workspace = new FullWorkspace(this.Workspace);

        //    this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullBoe>() { new FullBoe(this.Boe1), new FullBoe(this.Boe2) });
        //    this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullWbs>() { new FullWbs(this.Wbs) });
        //    this.retriever.Setup(x => x.GetClinsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullClin>() { new FullClin(this.Clin1) });
        //    this.retriever.Setup(x => x.GetClinById(this.Boe1.CLINID.Value)).Returns(this.Clin1);
        //    this.retriever.Setup(x => x.GetWbsById(this.Wbs.Id)).Returns(this.Wbs);
        //    PerformingOrgDTO Perforg2 = new PerformingOrgDTO() { Id = 2, IsSystemPerfOrg = true, PerformingOrgDesc = "test2", PerformingOrgName = "test2", Updateable = UpdateType.None };
        //    this.retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>())).Returns(new List<PerformingOrgDTO> { this.Perforg, Perforg2 });

        //    // setup resources dto mapper
        //    Collection<ResourceDTO> resources = new Collection<ResourceDTO>();
        //    ResourceDTO Laborresource = new ResourceDTO { Id = 2, ResourceName = "BBBBBBB", ResourceDesc = "Lots of Bs", SegRegion = "BB", LaborType = "BBBB", ElementOfCost = ElementOfCostType.LMLabor };
        //    ResourceDTO TravelResource = new ResourceDTO { Id = 7, ResourceName = "TT", ResourceDesc = "Lots of Ts", SegRegion = "TT", LaborType = "TT", ElementOfCost = ElementOfCostType.Travel, Segment = SegmentType.TS };
        //    ResourceDTO TravelResource2 = new ResourceDTO { Id = 8, ResourceName = "TD", ResourceDesc = "Lots of TDs", SegRegion = "TD", LaborType = "TD", ElementOfCost = ElementOfCostType.Travel, Segment = SegmentType.DS };

        //    resources.Add(Laborresource);
        //    resources.Add(TravelResource);
        //    resources.Add(TravelResource2);

        //    this.retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(resources);
        //    this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(resources);

        //    BoeTaskElementDTO laborElement1 = new BoeTaskElementDTO
        //    {
        //        Id = 6,
        //        BoeID = this.Boe1.Id,
        //        BOETaskID = "1234",
        //        TaskTitle = "MOCK TASK2",
        //        Description = "MOCK TASK2",
        //        StartDate = this.Boe1.StartDate,
        //        EndDate = this.Boe1.EndDate,
        //        MOQType = MOQType.LevelOfEffort,
        //        taskElementLabors = new Collection<ResourceTypeDto>{new ResourceTypeDto{Id=7, StartDateValue=LaborstartDate, EndDateValue=LaborendDate, ResourceID=Laborresource.Id, PerformingOrgID=this.Perforg.Id,  SpreadCurveID = SpreadCurves.DiscreteHours,
        //                    LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=8, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=150 },
        //                    new ResourceSpreadDto{Id=9, LaborSpreadDate=Convert.ToDateTime("12/01/2010"), LaborSpreadValue=150 },
        //                    new ResourceSpreadDto{Id=10, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=150 }}
        //                }}
        //    };

        //    CustomFieldDTO customField1 = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.TaskDisplay, WorkspaceID = workspace.Id };
        //    CustomFieldDTO customField2 = new CustomFieldDTO { Id = 2, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, WorkspaceID = workspace.Id };

        //    CustomFieldValueDTO cv = new CustomFieldValueDTO();
        //    cv.CustomFieldID = customField1.Id;
        //    cv.CustomFieldValueID = 1;
        //    cv.CustomFieldValueName = "P<br/><BR /><bR/>E";
        //    cv.CustomFieldValueDescription = "an exercise<br> class";

        //    CustomFieldValueDTO cv2 = new CustomFieldValueDTO();
        //    cv2.CustomFieldID = customField2.Id;
        //    cv2.CustomFieldValueID = 2;
        //    cv2.CustomFieldValueName = "Hist<BR />ory";
        //    cv2.CustomFieldValueDescription = "where you learn past<BR> events";
        //    this.retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(new Collection<CustomFieldDTO> { customField1, customField2 });
        //    this.retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new List<CustomFieldValueDTO>() { cv, cv2 });


        //    //------------------------------mst travel trips -------------------------------//
        //    MSTTravelTripType MSTTravelTrip1 = new MSTTravelTripType();
        //    MSTTravelTripType MSTTravelTrip2 = new MSTTravelTripType();
        //    MSTTravelTripType MSTTravelTrip3 = new MSTTravelTripType();
        //    MSTTravelTripType MSTTravelTrip4 = new MSTTravelTripType();
        //    MSTTravelTripType MSTTravelTrip5 = new MSTTravelTripType();


        //    //MSTTravelTrip1.Id = 111;
        //    MSTTravelTrip1.ModeID = MSTTravelMode.NonZoneDomestic;
        //    MSTTravelTrip1.GroupID = 111;
        //    MSTTravelTrip1.BoeID = this.Boe1.Id;
        //    MSTTravelTrip1.Cost = 111;
        //    MSTTravelTrip1.CustomFieldValueContainers = new Collection<CustomFieldValueContainer>();
        //    MSTTravelTrip1.EstimateDate = Convert.ToDateTime("01/01/2011");
        //    MSTTravelTrip1.NonZoneAirfareEstimate = 111;
        //    MSTTravelTrip1.NonZoneCarRentalTrans = 111;
        //    MSTTravelTrip1.NonZoneFrom = "NonZoneFrom1";
        //    MSTTravelTrip1.NonZoneNumCars = 2;
        //    MSTTravelTrip1.NonZonePerDiemDaily = 22;
        //    MSTTravelTrip1.NonZoneTo = "NonZoneto";
        //    MSTTravelTrip1.NumOfDays = 5;
        //    MSTTravelTrip1.NumOfPeople = 1;
        //    MSTTravelTrip1.PerfOrgID = Perforg2.Id;
        //    MSTTravelTrip1.Purpose = "MstTravelTrip1.Purpose";
        //    MSTTravelTrip1.Segment = SegmentType.MST;
        //    MSTTravelTrip1.TripDate = Convert.ToDateTime("01/01/2011");
        //    MSTTravelTrip1.UpdateDate = Convert.ToDateTime("01/01/2011");



        //    MSTTravelTrip2.ModeID = MSTTravelMode.ZoneAirfare;
        //    MSTTravelTrip2.GroupID = 222;
        //    MSTTravelTrip2.BoeID = this.Boe1.Id;
        //    MSTTravelTrip2.Cost = 111;
        //    MSTTravelTrip2.CustomFieldValueContainers = new Collection<CustomFieldValueContainer>();
        //    MSTTravelTrip2.EstimateDate = Convert.ToDateTime("01/01/2011");
        //    MSTTravelTrip2.NonZoneAirfareEstimate = 111;
        //    MSTTravelTrip2.NonZoneCarRentalTrans = 111;
        //    MSTTravelTrip2.NonZoneFrom = "NonZoneFrom1";
        //    MSTTravelTrip2.NonZoneNumCars = 2;
        //    MSTTravelTrip2.NonZonePerDiemDaily = 22;
        //    MSTTravelTrip2.NonZoneTo = "NonZoneto";
        //    MSTTravelTrip2.NumOfDays = 5;
        //    MSTTravelTrip2.NumOfPeople = 1;
        //    MSTTravelTrip2.PerfOrgID = Perforg2.Id;
        //    MSTTravelTrip2.Purpose = "MstTravelTrip2.Purpose";
        //    MSTTravelTrip2.Segment = SegmentType.MST;
        //    MSTTravelTrip2.TripDate = Convert.ToDateTime("01/01/2011");
        //    MSTTravelTrip2.UpdateDate = Convert.ToDateTime("01/01/2011");


        //    MSTTravelTrip3.ModeID = MSTTravelMode.NonZoneInternational;
        //    MSTTravelTrip3.GroupID = 333;
        //    MSTTravelTrip3.BoeID = this.Boe1.Id;
        //    MSTTravelTrip3.Cost = 111;
        //    MSTTravelTrip3.CustomFieldValueContainers = new Collection<CustomFieldValueContainer>();
        //    MSTTravelTrip3.EstimateDate = Convert.ToDateTime("01/01/2011");
        //    MSTTravelTrip3.NonZoneAirfareEstimate = 111;
        //    MSTTravelTrip3.NonZoneCarRentalTrans = 111;
        //    MSTTravelTrip3.NonZoneFrom = "NonZoneFrom1";
        //    MSTTravelTrip3.NonZoneNumCars = 2;
        //    MSTTravelTrip3.NonZonePerDiemDaily = 22;
        //    MSTTravelTrip3.NonZoneTo = "NonZoneto";
        //    MSTTravelTrip3.NumOfDays = 5;
        //    MSTTravelTrip3.NumOfPeople = 1;
        //    MSTTravelTrip3.PerfOrgID = Perforg2.Id;
        //    MSTTravelTrip3.Purpose = "MstTravelTrip3.Purpose";
        //    MSTTravelTrip3.Segment = SegmentType.MST;
        //    MSTTravelTrip3.TripDate = Convert.ToDateTime("01/01/2011");
        //    MSTTravelTrip3.UpdateDate = Convert.ToDateTime("01/01/2011");


        //    MSTTravelTrip4.ModeID = MSTTravelMode.ZoneNoAirfare;
        //    MSTTravelTrip4.GroupID = 111;
        //    MSTTravelTrip4.BoeID = this.Boe1.Id;
        //    MSTTravelTrip4.Cost = 444;
        //    MSTTravelTrip4.CustomFieldValueContainers = new Collection<CustomFieldValueContainer>();
        //    MSTTravelTrip4.EstimateDate = Convert.ToDateTime("01/01/2011");
        //    MSTTravelTrip4.NonZoneAirfareEstimate = 111;
        //    MSTTravelTrip4.NonZoneCarRentalTrans = 111;
        //    MSTTravelTrip4.NonZoneFrom = "NonZoneFrom1";
        //    MSTTravelTrip4.NonZoneNumCars = 2;
        //    MSTTravelTrip4.NonZonePerDiemDaily = 22;
        //    MSTTravelTrip4.NonZoneTo = "NonZoneto";
        //    MSTTravelTrip4.NumOfDays = 5;
        //    MSTTravelTrip4.NumOfPeople = 1;
        //    MSTTravelTrip4.PerfOrgID = Perforg2.Id;
        //    MSTTravelTrip4.Purpose = "MstTravelTrip4.Purpose";
        //    MSTTravelTrip4.Segment = SegmentType.MST;
        //    MSTTravelTrip4.TripDate = Convert.ToDateTime("01/01/2011");
        //    MSTTravelTrip4.UpdateDate = Convert.ToDateTime("01/01/2011");


        //    MSTTravelTrip5.ModeID = MSTTravelMode.NonZoneDomestic;
        //    MSTTravelTrip5.GroupID = 555;
        //    MSTTravelTrip5.BoeID = this.Boe1.Id;
        //    MSTTravelTrip5.Cost = 111;
        //    MSTTravelTrip5.CustomFieldValueContainers = new Collection<CustomFieldValueContainer>
        //    {
        //        new CustomFieldValueContainer { CustomFieldValueID = cv.CustomFieldValueID  },
        //    };
        //    MSTTravelTrip5.EstimateDate = Convert.ToDateTime("01/01/2011");
        //    MSTTravelTrip5.NonZoneAirfareEstimate = 111;
        //    MSTTravelTrip5.NonZoneCarRentalTrans = 111;
        //    MSTTravelTrip5.NonZoneFrom = "NonZoneFrom1";
        //    MSTTravelTrip5.NonZoneNumCars = 2;
        //    MSTTravelTrip5.NonZonePerDiemDaily = 22;
        //    MSTTravelTrip5.NonZoneTo = "NonZoneto";
        //    MSTTravelTrip5.NumOfDays = 5;
        //    MSTTravelTrip5.NumOfPeople = 1;
        //    MSTTravelTrip5.PerfOrgID = Perforg2.Id;
        //    MSTTravelTrip5.Purpose = "MstTravelTrip5.Purpose";
        //    MSTTravelTrip5.Segment = SegmentType.MST;
        //    MSTTravelTrip5.TripDate = Convert.ToDateTime("01/01/2011");
        //    MSTTravelTrip5.UpdateDate = Convert.ToDateTime("01/01/2011");

            

        //    TravelDTO travel = new TravelDTO
        //    {
        //        Id = 1,
        //        TaskTitle = "MOCKMSTTRAVEL",
        //        MSTTravelTrips = new Collection<MSTTravelTripType> { MSTTravelTrip1, MSTTravelTrip2, MSTTravelTrip3, MSTTravelTrip4, MSTTravelTrip5 },
        //        BoeID = this.Boe2.Id,
        //        StartDate = Convert.ToDateTime("07/01/2011"),
        //        EndDate = Convert.ToDateTime("12/01/2011")
        //    };


        //    this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, false)).Returns(new Collection<BoeTaskElementDTO> { laborElement1 });
        //    this.retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, false)).Returns(new Collection<TravelDTO> { travel });
        //    this.retriever.Setup(x => x.GetOdcCollectionByBoeIds(new Collection<int> { this.Boe1.Id, this.Boe2.Id }, false)).Returns(new Collection<OtherDirectCostDTO> { });
        //    this.retriever.Setup(x => x.GetMaterialsByBoeIds(It.IsAny<Collection<int>>(), false)).Returns(new Collection<MaterialDTO> { });

        //    foreach (FullBoe boe in workspace.Boes)
        //    {
        //        if (boe.Id == 20)
        //        {
        //            boe.Title = "Labor/Material BOE";
        //        }
        //        else if (boe.Id == 21)
        //        {
        //            boe.Title = "Travel/ODC BOE";
        //        }
        //    }

        //    var TripCalculate = new Mock<TravelTripCostCalculation>();
        //    var rmsTripCalculate = new Mock<RMSZoneTravelRatesFeesDataLoader>();
        //    rmsTripCalculate.Setup(x => x.getAllFeesAndCostsByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSTravelNonzoneFeesAndCostsDTO>());
        //    ProPricerExporter expReport = new ProPricerExporter(TripCalculate.Object, rmsTripCalculate.Object);
        //    ProPricerDTO proPricerExport = SetUpProPricerDTO(this.Workspace.Id);

        //    PpDataReadyForExport result = expReport.ExportProPricer(proPricerExport, workspace);
        //    Collection<string> TaskData = result.TaskData.ToCollection();
        //    Collection<string> ResourceCost = result.ResourceData.ToCollection();


        //    foreach (string t in TaskData) Trace.WriteLine(t);
        //    foreach (string s in ResourceCost) Trace.WriteLine(s);
            
            
        //    //should be four cost rows because of the grouping of custom fields and perfor orgs
        //    Assert.AreEqual(6, ResourceCost.Count);

        //    //get the values of the cost output

        //    //because of the grouping by perf org and custom field values we should see 
        //    Assert.AreEqual(",\"\",TIDN000002,2.2,,,,D,,\"\",012011,\"mock wbs title\",21,1,\"\",\"Travel/ODC BOE\",\"test2\",\"MOCKMSTTRAVEL\",\"\",", ResourceCost[1]);
        //    Assert.AreEqual(",\"\",TIDN000003,2.2,,,,D,,\"\",012011,\"mock wbs title\",21,1,\"\",\"Travel/ODC BOE\",\"test2\",\"MOCKMSTTRAVEL\",\"\",", ResourceCost[2]);
        //    Assert.AreEqual(",\"\",TIDN000004,2.2,,,,D,,\"\",012011,\"mock wbs title\",21,1,\"\",\"Travel/ODC BOE\",\"test2\",\"MOCKMSTTRAVEL\",\"\",", ResourceCost[3]);
        //    Assert.AreEqual(",\"\",TIDN000005,2.2,,,,D,,\"\",012011,\"mock wbs title\",21,1,\"\",\"Travel/ODC BOE\",\"test2\",\"MOCKMSTTRAVEL\",\"\",", ResourceCost[4]);
        //    Assert.AreEqual(",\"\",TIDN000006,2.2,,,,D,,\"\",012011,\"mock wbs title\",21,1,\"\",\"Travel/ODC BOE\",\"test2\",\"MOCKMSTTRAVEL\",\"\",", ResourceCost[5]);


        //    //at the task level we should see similar responses.
        //    Assert.AreEqual(6, TaskData.Count);

        //    Assert.AreEqual("072011,,TOTAL,,122011,TIDN000002,,\"MOCKMSTTRAVEL\",\"\",\"mock wbs title\",2.2,,,,\"\",\"test2\",21,1,\"\",\"Travel/ODC BOE\",\"\",", TaskData[1]);
        //    Assert.AreEqual("072011,,TOTAL,,122011,TIDN000003,,\"MOCKMSTTRAVEL\",\"\",\"mock wbs title\",2.2,,,,\"\",\"test2\",21,1,\"\",\"Travel/ODC BOE\",\"\",", TaskData[2]);
        //    Assert.AreEqual("072011,,TOTAL,,122011,TIDN000004,,\"MOCKMSTTRAVEL\",\"\",\"mock wbs title\",2.2,,,,\"\",\"test2\",21,1,\"\",\"Travel/ODC BOE\",\"\",", TaskData[3]);
        //    Assert.AreEqual("072011,,TOTAL,,122011,TIDN000005,,\"MOCKMSTTRAVEL\",\"\",\"mock wbs title\",2.2,,,,\"\",\"test2\",21,1,\"\",\"Travel/ODC BOE\",\"\",", TaskData[4]);
        //    Assert.AreEqual("072011,,TOTAL,,122011,TIDN000006,,\"MOCKMSTTRAVEL\",\"\",\"mock wbs title\",2.2,,,,\"\",\"test2\",21,1,\"\",\"Travel/ODC BOE\",\"\",", TaskData[5]);
        //}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestMethod]
        public void BL_ExportProPricer_UniqueTrips()
        {
            #region setup
            DateTime LaborstartDate = Convert.ToDateTime("11/01/2010");
            DateTime LaborendDate = Convert.ToDateTime("01/01/2011");

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsDataLoader.Object);
            FullWorkspace workspace = new FullWorkspace(this.Workspace);

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullBoe>() { new FullBoe(this.Boe1), new FullBoe(this.Boe2) });
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullWbs>() { new FullWbs(this.Wbs) });
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullClin>() { new FullClin(this.Clin1) });
            this.retriever.Setup(x => x.GetClinById(this.Boe1.CLINID.Value)).Returns(this.Clin1);
            this.retriever.Setup(x => x.GetWbsById(this.Wbs.Id)).Returns(this.Wbs);
            PerformingOrgDTO Perforg2 = new PerformingOrgDTO() { Id = 2, IsSystemPerfOrg = true, PerformingOrgDesc = "test2", PerformingOrgName = "test2", Updateable = UpdateType.None };
            this.retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>())).Returns(new List<PerformingOrgDTO> { this.Perforg, Perforg2 });

            // setup resources dto mapper
            Collection<ResourceDTO> resources = new Collection<ResourceDTO>();
            ResourceDTO Laborresource = new ResourceDTO { Id = 2, ResourceName = "BBBBBBB", ResourceDesc = "Lots of Bs", SegRegion = "BB", LaborType = "BBBB", ElementOfCost = ElementOfCostType.LMLabor };
            ResourceDTO TravelResource = new ResourceDTO { Id = 7, ResourceName = "TT", ResourceDesc = "Lots of Ts", SegRegion = "TT", LaborType = "TT", ElementOfCost = ElementOfCostType.Travel, Segment = SegmentType.TS };
            ResourceDTO TravelResource2 = new ResourceDTO { Id = 8, ResourceName = "TD", ResourceDesc = "Lots of TDs", SegRegion = "TD", LaborType = "TD", ElementOfCost = ElementOfCostType.Travel, Segment = SegmentType.DS };

            resources.Add(Laborresource);
            resources.Add(TravelResource);
            resources.Add(TravelResource2);

            this.retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(resources);
            this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(resources);

            BoeTaskElementDTO laborElement1 = new BoeTaskElementDTO
            {
                Id = 6,
                BoeID = this.Boe1.Id,
                BOETaskID = "1234",
                TaskTitle = "MOCK TASK2",
                Description = "MOCK TASK2",
                StartDate = this.Boe1.StartDate,
                EndDate = this.Boe1.EndDate,
                MOQType = MOQType.LevelOfEffort,
                taskElementLabors = new Collection<ResourceTypeDto>{new ResourceTypeDto{Id=7, StartDateValue=LaborstartDate, EndDateValue=LaborendDate, ResourceID=Laborresource.Id, PerformingOrgID=this.Perforg.Id,  SpreadCurveID = SpreadCurves.DiscreteHours,
                            LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=8, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=150 },
                            new ResourceSpreadDto{Id=9, LaborSpreadDate=Convert.ToDateTime("12/01/2010"), LaborSpreadValue=150 },
                            new ResourceSpreadDto{Id=10, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=150 }}
                        }}
            };
            #endregion setup

            CustomFieldDTO customField1 = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.TaskDisplay, WorkspaceID = workspace.Id };
            CustomFieldDTO customField2 = new CustomFieldDTO { Id = 2, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, WorkspaceID = workspace.Id };

            CustomFieldValueDTO cv = new CustomFieldValueDTO();
            cv.CustomFieldID = customField1.Id;
            cv.CustomFieldValueID = 1;
            cv.CustomFieldValueName = "<br/><BR /><bR/>PE";
            cv.CustomFieldValueDescription = "an exe<br/><BR /><bR/>rcise class";

            CustomFieldValueDTO cv2 = new CustomFieldValueDTO();
            cv2.CustomFieldID = customField2.Id;
            cv2.CustomFieldValueID = 2;
            cv2.CustomFieldValueName = "History<br/><BR /><bR/>";
            cv2.CustomFieldValueDescription = "where you lear<br/><BR /><bR/>n past events";

            CustomFieldValueDTO cv3 = new CustomFieldValueDTO();
            cv3.CustomFieldID = customField2.Id;
            cv3.CustomFieldValueID = 3;
            cv3.CustomFieldValueName = "His<br/><BR /><bR/>tory2";
            cv3.CustomFieldValueDescription = "where you <br/><BR /><bR/>learn past events2";


            this.retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(new Collection<CustomFieldDTO> { customField1, customField2 });
            this.retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new List<CustomFieldValueDTO>() { cv, cv2, cv3 });

            TravelTripType travelTrip = new TravelTripType { TravelTripID = 1, TripDate = Convert.ToDateTime("07/01/2011"), BoeID = this.Boe2.Id, GroupID = 1, NumOfDays = 2, NumOfIntervals = 0, NumOfOccurences = 0, NumOfPeople = 4, NumOfTrips = 2, PerfOrgID = this.Perforg.Id, Segment = SegmentType.DS, SystemTripID = 1, CustomFieldValueContainers = new Collection<CustomFieldValueContainer>() { new CustomFieldValueContainer() { ContainerID = 1, CustomFieldValueID = cv2.CustomFieldValueID } } };
            TravelTripType travelTrip2 = new TravelTripType { TravelTripID = 2, TripDate = Convert.ToDateTime("09/01/2011"), BoeID = this.Boe2.Id, GroupID = 2, NumOfDays = 2, NumOfIntervals = 0, NumOfOccurences = 0, NumOfPeople = 4, NumOfTrips = 2, PerfOrgID = this.Perforg.Id, Segment = SegmentType.DS, SystemTripID = 1, CustomFieldValueContainers = new Collection<CustomFieldValueContainer>() { new CustomFieldValueContainer() { ContainerID = 2, CustomFieldValueID = cv2.CustomFieldValueID } } };
            TravelTripType travelTrip5 = new TravelTripType { TravelTripID = 5, TripDate = Convert.ToDateTime("09/01/2011"), BoeID = this.Boe2.Id, GroupID = 5, NumOfDays = 2, NumOfIntervals = 0, NumOfOccurences = 0, NumOfPeople = 4, NumOfTrips = 2, PerfOrgID = this.Perforg.Id, Segment = SegmentType.DS, SystemTripID = 1, CustomFieldValueContainers = new Collection<CustomFieldValueContainer>() { new CustomFieldValueContainer() { ContainerID = 3, CustomFieldValueID = cv3.CustomFieldValueID } } };
            TravelTripType travelTrip3 = new TravelTripType { TravelTripID = 3, TripDate = Convert.ToDateTime("10/01/2011"), BoeID = this.Boe2.Id, GroupID = 3, NumOfDays = 2, NumOfIntervals = 0, NumOfOccurences = 0, NumOfPeople = 4, NumOfTrips = 2, PerfOrgID = this.Perforg.Id, Segment = SegmentType.DS, SystemTripID = 1 };
            TravelTripType travelTrip4 = new TravelTripType { TravelTripID = 4, TripDate = Convert.ToDateTime("12/01/2011"), BoeID = this.Boe2.Id, GroupID = 4, NumOfDays = 2, NumOfIntervals = 0, NumOfOccurences = 0, NumOfPeople = 4, NumOfTrips = 2, PerfOrgID = Perforg2.Id, Segment = SegmentType.DS, SystemTripID = 1 };

            TravelDTO travel = new TravelDTO { Id = 1, TaskTitle = "MOCKTRAVEL", TravelTrips = new Collection<TravelTripType> { travelTrip, travelTrip2, travelTrip3, travelTrip4, travelTrip5 }, BoeID = this.Boe2.Id, StartDate = Convert.ToDateTime("07/01/2011"), EndDate = Convert.ToDateTime("12/01/2011") };
            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { laborElement1 });
            this.retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, false)).Returns(new Collection<TravelDTO> { travel });
            this.retriever.Setup(x => x.GetOdcCollectionByBoeIds(new Collection<int> { this.Boe1.Id, this.Boe2.Id }, false)).Returns(new Collection<OtherDirectCostDTO> { });
            this.retriever.Setup(x => x.GetMaterialsByBoeIds(It.IsAny<Collection<int>>(), false)).Returns(new Collection<MaterialDTO> { });
            this.retriever.Setup(x => x.GetMoqTypeSelectionsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<MoqTypeSelection>());
            workspace.UsingTemplateBOE = false;
            
            foreach (FullBoe boe in workspace.Boes)
            {
                if (boe.Id == 20)
                {
                    boe.Title = "Labor/Material BOE";
                }
                else if (boe.Id == 21)
                {
                    boe.Title = "Travel/ODC BOE";
                }
            }

            var TripCalculate = new Mock<TravelTripCostCalculation>();
            var rmsTripCalculate = new Mock<RMSZoneTravelRatesFeesDataLoader>();
            rmsTripCalculate.Setup(x => x.getAllFeesAndCostsByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSTravelNonzoneFeesAndCostsDTO>());
            rmsTripCalculate.Setup(x => x.getAllEscalationRatesByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSEscalationRatesDTO>());

            TripCalculate.Setup(x => x.CalculateTravelCost(travelTrip, workspace)).Returns(new TravelTripCostData(500, 1, 1, 1, 1, 1, 1, 1, 1, 1));
            TripCalculate.Setup(x => x.CalculateTravelCost(travelTrip2, workspace)).Returns(new TravelTripCostData(300, 1, 1, 1, 1, 1, 1, 1, 1, 1));
            TripCalculate.Setup(x => x.CalculateTravelCost(travelTrip3, workspace)).Returns(new TravelTripCostData(500, 1, 1, 1, 1, 1, 1, 1, 1, 1));
            TripCalculate.Setup(x => x.CalculateTravelCost(travelTrip4, workspace)).Returns(new TravelTripCostData(100, 1, 1, 1, 1, 1, 1, 1, 1, 1));
            TripCalculate.Setup(x => x.CalculateTravelCost(travelTrip5, workspace)).Returns(new TravelTripCostData(300, 1, 1, 1, 1, 1, 1, 1, 1, 1));
           
            ProPricerExporter expReport = new ProPricerExporter(TripCalculate.Object, rmsTripCalculate.Object, this.retriever.Object, this.commonDataMapper.Object);
            ProPricerDTO proPricerExport = SetUpProPricerDTO(this.Workspace.Id);
 

            PpDataReadyForExport result = expReport.ExportProPricer(proPricerExport, workspace);
            Collection<string> TaskData = result.TaskData.ToCollection();
            Collection<string> ResourceCost = result.ResourceData.ToCollection();

            //should be four cost rows because of the grouping of custom fields and perf orgs
            Assert.AreEqual(5, ResourceCost.Count);

            //get the values of the cost output

            //because of the grouping by perf org and custom field values we should see 
            Assert.AreEqual(",\"TD\",TIDN000001,2.2,\"where you learn past events\",\"History\",,D,,\"\",072011,\"mock wbs title\",21,1,1 2,\"Travel/ODC BOE\",\"KristinePO\",\"MOCKTRAVEL\",\"\",500,,300,", ResourceCost[1]);
            Assert.AreEqual(",\"TD\",TIDN000001,2.2,,,,D,,\"\",102011,\"mock wbs title\",21,1,3,\"Travel/ODC BOE\",\"KristinePO\",\"MOCKTRAVEL\",\"\",500,", ResourceCost[2]);
            Assert.AreEqual(",\"TD\",TIDN000001,2.2,\"where you learn past events2\",\"History2\",,D,,\"\",092011,\"mock wbs title\",21,1,5,\"Travel/ODC BOE\",\"KristinePO\",\"MOCKTRAVEL\",\"\",300,", ResourceCost[3]);
            Assert.AreEqual(",\"TD\",TIDN000001,2.2,,,,D,,\"\",122011,\"mock wbs title\",21,1,4,\"Travel/ODC BOE\",\"test2\",\"MOCKTRAVEL\",\"\",100,", ResourceCost[4]);


            //at the task level we should see simular responses.
            Assert.AreEqual(5, TaskData.Count);

            Assert.AreEqual("072011,,TOTAL,,122011,TIDN000001,,\"MOCKTRAVEL\",\"\",\"mock wbs title\",2.2,,\"where you learn past events\",\"History\",21,1,,\"Travel/ODC BOE\",\"\",", TaskData[1]);
            Assert.AreEqual("072011,,TOTAL,,122011,TIDN000001,,\"MOCKTRAVEL\",\"\",\"mock wbs title\",2.2,,,,21,1,,\"Travel/ODC BOE\",\"\",", TaskData[2]);
            Assert.AreEqual("072011,,TOTAL,,122011,TIDN000001,,\"MOCKTRAVEL\",\"\",\"mock wbs title\",2.2,,\"where you learn past events2\",\"History2\",21,1,,\"Travel/ODC BOE\",\"\",", TaskData[3]);
            Assert.AreEqual("072011,,TOTAL,,122011,TIDN000001,,\"MOCKTRAVEL\",\"\",\"mock wbs title\",2.2,,,,21,1,,\"Travel/ODC BOE\",\"\",", TaskData[4]);

        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestMethod]
        public void BL_ExportProPricer_NullTrips()
        {
            #region setup
            DateTime LaborstartDate = Convert.ToDateTime("11/01/2010");
            DateTime LaborendDate = Convert.ToDateTime("01/01/2011");

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsDataLoader.Object);
            FullWorkspace workspace = new FullWorkspace(this.Workspace);

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullBoe>() { new FullBoe(this.Boe1), new FullBoe(this.Boe2) });
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullWbs>() { new FullWbs(this.Wbs) });
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullClin>() { new FullClin(this.Clin1) });
            this.retriever.Setup(x => x.GetClinById(this.Boe1.CLINID.Value)).Returns(this.Clin1);
            this.retriever.Setup(x => x.GetWbsById(this.Wbs.Id)).Returns(this.Wbs);
            PerformingOrgDTO Perforg2 = new PerformingOrgDTO() { Id = 2, IsSystemPerfOrg = true, PerformingOrgDesc = "test2", PerformingOrgName = "test2", Updateable = UpdateType.None };
            this.retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>())).Returns(new List<PerformingOrgDTO> { this.Perforg, Perforg2 });

            // setup resources dto mapper
            Collection<ResourceDTO> resources = new Collection<ResourceDTO>();
            ResourceDTO Laborresource = new ResourceDTO { Id = 2, ResourceName = "BBBBBBB", ResourceDesc = "Lots of Bs", SegRegion = "BB", LaborType = "BBBB", ElementOfCost = ElementOfCostType.LMLabor };
            ResourceDTO TravelResource = new ResourceDTO { Id = 7, ResourceName = "TT", ResourceDesc = "Lots of Ts", SegRegion = "TT", LaborType = "TT", ElementOfCost = ElementOfCostType.Travel, Segment = SegmentType.TS };
            ResourceDTO TravelResource2 = new ResourceDTO { Id = 8, ResourceName = "TD", ResourceDesc = "Lots of TDs", SegRegion = "TD", LaborType = "TD", ElementOfCost = ElementOfCostType.Travel, Segment = SegmentType.DS };

            resources.Add(Laborresource);
            resources.Add(TravelResource);
            resources.Add(TravelResource2);

            this.retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(resources);
            this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(resources);

            BoeTaskElementDTO laborElement1 = new BoeTaskElementDTO
            {
                Id = 6,
                BoeID = this.Boe1.Id,
                BOETaskID = "1234",
                TaskTitle = "MOCK TASK2",
                Description = "MOCK TASK2",
                StartDate = this.Boe1.StartDate,
                EndDate = this.Boe1.EndDate,
                MOQType = MOQType.LevelOfEffort,
                taskElementLabors = new Collection<ResourceTypeDto>{new ResourceTypeDto{Id=7, StartDateValue=LaborstartDate, EndDateValue=LaborendDate, ResourceID=Laborresource.Id, PerformingOrgID=this.Perforg.Id,  SpreadCurveID = SpreadCurves.DiscreteHours,
                            LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=8, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=150 },
                            new ResourceSpreadDto{Id=9, LaborSpreadDate=Convert.ToDateTime("12/01/2010"), LaborSpreadValue=150 },
                            new ResourceSpreadDto{Id=10, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=150 }}
                        }}
            };
            #endregion setup

            CustomFieldDTO customField1 = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.TaskDisplay, WorkspaceID = workspace.Id };
            CustomFieldDTO customField2 = new CustomFieldDTO { Id = 2, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, WorkspaceID = workspace.Id };

            CustomFieldValueDTO cv = new CustomFieldValueDTO();
            cv.CustomFieldID = customField1.Id;
            cv.CustomFieldValueID = 1;
            cv.CustomFieldValueName = "<br/><BR /><bR/>PE";
            cv.CustomFieldValueDescription = "an exercise class<br/><BR /><bR/>";

            CustomFieldValueDTO cv2 = new CustomFieldValueDTO();
            cv2.CustomFieldID = customField2.Id;
            cv2.CustomFieldValueID = 2;
            cv2.CustomFieldValueName = "History<br/><BR /><bR/>";
            cv2.CustomFieldValueDescription = "where you le<br/><BR /><bR/>arn past events";

            CustomFieldValueDTO cv3 = new CustomFieldValueDTO();
            cv3.CustomFieldID = customField2.Id;
            cv3.CustomFieldValueID = 3;
            cv3.CustomFieldValueName = "<br/><BR /><bR/>History2";
            cv3.CustomFieldValueDescription = "where <br/><BR /><bR/>you learn past events2";


            this.retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(new Collection<CustomFieldDTO> { customField1, customField2 });
            this.retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new List<CustomFieldValueDTO>() { cv, cv2, cv3 });

            TravelDTO travel = new TravelDTO { Id = 1, TaskTitle = "MOCKTRAVEL", TravelTrips = new Collection<TravelTripType> { }, BoeID = this.Boe2.Id, StartDate = Convert.ToDateTime("07/01/2011"), EndDate = Convert.ToDateTime("12/01/2011") };
            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { laborElement1 });
            this.retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, false)).Returns(new Collection<TravelDTO> { travel });
            this.retriever.Setup(x => x.GetOdcCollectionByBoeIds(new Collection<int> { this.Boe1.Id, this.Boe2.Id }, false)).Returns(new Collection<OtherDirectCostDTO> { });
            this.retriever.Setup(x => x.GetMaterialsByBoeIds(It.IsAny<Collection<int>>(), false)).Returns(new Collection<MaterialDTO> { });
            factory.Setup(x => x.CreateFullBoe(this.Boe1.Id)).Returns(new FullBoe() { Title = "Labor/Material BOE" });
            factory.Setup(x => x.CreateFullBoe(this.Boe2.Id)).Returns(new FullBoe() { Title = "Travel/ODC BOE" });
            this.retriever.Setup(x => x.GetMoqTypeSelectionsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<MoqTypeSelection>());
            workspace.UsingTemplateBOE = false;

            var TripCalculate = new Mock<TravelTripCostCalculation>();
            var rmsTripCalculate = new Mock<RMSZoneTravelRatesFeesDataLoader>();
            rmsTripCalculate.Setup(x => x.getAllFeesAndCostsByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSTravelNonzoneFeesAndCostsDTO>());
            rmsTripCalculate.Setup(x => x.getAllEscalationRatesByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSEscalationRatesDTO>());

            ProPricerExporter expReport = new ProPricerExporter(TripCalculate.Object, rmsTripCalculate.Object, this.retriever.Object, this.commonDataMapper.Object);

            ProPricerDTO proPricerExport = SetUpProPricerDTO(this.Workspace.Id);

            PpDataReadyForExport result = expReport.ExportProPricer(proPricerExport, workspace);
            Collection<string> TaskData = result.TaskData.ToCollection();
            Collection<string> ResourceCost = result.ResourceData.ToCollection();

            //if no trips we shouldnt print anything for travel
            Assert.AreEqual(1, ResourceCost.Count);

            //if no trips we shouldnt print anything for travel
            Assert.AreEqual(1, TaskData.Count);
        }

        [TestMethod, System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public void BL_ExportProPricer_AllEOC()
        {
            DateTime LaborstartDate = Convert.ToDateTime("11/01/2010");
            DateTime LaborendDate = Convert.ToDateTime("01/01/2011");

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsDataLoader.Object);
            FullWorkspace workspace = new FullWorkspace(this.Workspace);

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullBoe>() { new FullBoe(this.Boe1), new FullBoe(this.Boe2), new FullBoe(this.Boe3) });
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullWbs>() { new FullWbs(this.Wbs) });
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullClin>() { new FullClin(this.Clin1) });
            this.retriever.Setup(x => x.GetClinById(this.Boe1.CLINID.Value)).Returns(this.Clin1);
            this.retriever.Setup(x => x.GetWbsById(this.Wbs.Id)).Returns(this.Wbs);

            this.retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>())).Returns(new List<PerformingOrgDTO> { this.Perforg });

            // setup resources dto mapper
            Collection<ResourceDTO> resources = new Collection<ResourceDTO>();
            ResourceDTO LaborResource1 = new ResourceDTO { Id = 2, ResourceName = "BBBBBBB", ResourceDesc = "Lots of Bs", SegRegion = "BB", LaborType = "BBBB", ElementOfCost = ElementOfCostType.LMLabor };
            ResourceDTO LaborResource2 = new ResourceDTO { Id = 9, ResourceName = "EEEEEEE", ResourceDesc = "Lots of Es", SegRegion = "EE", LaborType = "EEEE", ElementOfCost = ElementOfCostType.LMLabor };
            ResourceDTO LaborResource3 = new ResourceDTO { Id = 36, ResourceName = "IIIIIII", ResourceDesc = "Lots of Is", SegRegion = "II", LaborType = "IIII", ElementOfCost = ElementOfCostType.LMLabor };
            ResourceDTO IwtaResource = new ResourceDTO { Id = 3, ResourceName = "CCCC", ResourceDesc = "Lots of Cs", SegRegion = "CC", LaborType = "CCC", ElementOfCost = ElementOfCostType.IWTA };
            ResourceDTO Subresource = new ResourceDTO { Id = 4, ResourceName = "DDDD", ResourceDesc = "Lots of Ds", SegRegion = "DD", LaborType = "DDD", ElementOfCost = ElementOfCostType.Sub };
            ResourceDTO MaterialLaborResource = new ResourceDTO { Id = 12, ResourceName = "FFFFFFF", ResourceDesc = "Lots of Fs", SegRegion = "FF", LaborType = "FFFF", ElementOfCost = ElementOfCostType.Materials };
            ResourceDTO ODCLaborResource = new ResourceDTO { Id = 13, ResourceName = "GGGGGGG", ResourceDesc = "Lots of Gs", SegRegion = "GG", LaborType = "GG", ElementOfCost = ElementOfCostType.ODC };
            ResourceDTO TravelLaborResource = new ResourceDTO { Id = 14, ResourceName = "HHHHHHH", ResourceDesc = "Lots of Hs", SegRegion = "HH", LaborType = "HH", ElementOfCost = ElementOfCostType.Travel };
            ResourceDTO MaterialResource1 = new ResourceDTO { Id = 5, ResourceName = "MM", ResourceDesc = "Lots of Ms", SegRegion = "MM", LaborType = "MM", ElementOfCost = ElementOfCostType.Materials };
            ResourceDTO MaterialResource2 = new ResourceDTO { Id = 10, ResourceName = "MM2", ResourceDesc = "Lots of Ms 2", SegRegion = "MM", LaborType = "MM", ElementOfCost = ElementOfCostType.Materials };
            ResourceDTO ODCResource1 = new ResourceDTO { Id = 6, ResourceName = "OO", ResourceDesc = "Lots of Os", SegRegion = "OO", LaborType = "OO", ElementOfCost = ElementOfCostType.ODC };
            ResourceDTO ODCResource2 = new ResourceDTO { Id = 11, ResourceName = "OO2", ResourceDesc = "Lots of Os 2", SegRegion = "OO", LaborType = "OO", ElementOfCost = ElementOfCostType.ODC };
            ResourceDTO TravelResource = new ResourceDTO { Id = 7, ResourceName = "TT", ResourceDesc = "Lots of Ts", SegRegion = "TT", LaborType = "TT", ElementOfCost = ElementOfCostType.Travel, Segment = SegmentType.TS };
            ResourceDTO TravelResource2 = new ResourceDTO { Id = 8, ResourceName = "TD", ResourceDesc = "Lots of TDs", SegRegion = "TD", LaborType = "TD", ElementOfCost = ElementOfCostType.Travel, Segment = SegmentType.DS };

            resources.Add(LaborResource1);
            resources.Add(LaborResource2);
            resources.Add(LaborResource3);
            resources.Add(IwtaResource);
            resources.Add(Subresource);
            resources.Add(MaterialLaborResource);
            resources.Add(ODCLaborResource);
            resources.Add(TravelLaborResource);
            resources.Add(MaterialResource1);
            resources.Add(MaterialResource2);
            resources.Add(ODCResource1);
            resources.Add(ODCResource2);
            resources.Add(TravelResource);
            resources.Add(TravelResource2);

            this.retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(resources);
            this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(resources);

            CustomFieldDTO customField1 = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.TaskDisplay, WorkspaceID = workspace.Id };
            CustomFieldDTO customField2 = new CustomFieldDTO { Id = 2, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, WorkspaceID = workspace.Id };
            CustomFieldDTO customField3 = new CustomFieldDTO { Id = 3, CustomFieldDisplayID = CustomFieldType.BoeDisplay, WorkspaceID = workspace.Id };

            CustomFieldValueDTO cv = new CustomFieldValueDTO();
            cv.CustomFieldID = customField1.Id;
            cv.CustomFieldValueID = 1;
            cv.CustomFieldValueName = "P<br/><BR /><bR/>E";
            cv.CustomFieldValueDescription = "an exercis<br/><BR /><bR/>e class";

            CustomFieldValueDTO cv2 = new CustomFieldValueDTO();
            cv2.CustomFieldID = customField2.Id;
            cv2.CustomFieldValueID = 2;
            cv2.CustomFieldValueName = "<br/><BR /><bR/>History";
            cv2.CustomFieldValueDescription = "where <br/><BR /><bR/>you learn past events";

            CustomFieldValueDTO cv3 = new CustomFieldValueDTO();
            cv3.CustomFieldID = customField3.Id;
            cv3.CustomFieldValueID = 3;
            cv3.CustomFieldValueName = "Math<br/><BR /><bR/>";
            cv3.CustomFieldValueDescription = "where you learn how to <br/><BR /><bR/>add";

            this.retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(new Collection<CustomFieldDTO> { customField1, customField2, customField3 });
            this.retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new List<CustomFieldValueDTO>() { cv, cv2, cv3 });

            foreach (FullBoe boe in workspace.Boes)
            {
                boe.CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { new CustomFieldValueContainer { CustomFieldValueID = cv3.CustomFieldValueID } };

                //Titles with carriage returns to ensure they are replaced with a space
                if (boe.Id == 20)
                {
                    boe.Title = "Labor/Material\nBOE";
                }
                else if (boe.Id == 21)
                {
                    boe.Title = "Travel/ODC\n\nBOE";
                }
                else
                {
                    //test that a null title is exported as an empty string
                    boe.Title = null;
                }
            }

            BoeTaskElementDTO laborElement1 = new BoeTaskElementDTO
            {
                Id = 6,
                BoeID = this.Boe1.Id,
                BOETaskID = "1234",
                TaskTitle = "MOCK TASK2",
                Description = "MOCK TASK2",
                StartDate = this.Boe1.StartDate,
                EndDate = this.Boe1.EndDate,
                MOQType = MOQType.LevelOfEffort,
                CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { new CustomFieldValueContainer { CustomFieldValueID = cv.CustomFieldValueID } },
                taskElementLabors = new Collection<ResourceTypeDto>{new ResourceTypeDto{Id=7, SpreadType = IES.Common.SpreadType.Hours, StartDateValue=LaborstartDate, EndDateValue=LaborendDate,
                        ResourceID=LaborResource1.Id, PerformingOrgID=this.Perforg.Id,  SpreadCurveID = SpreadCurves.DiscreteHours, CustomFieldValueContainers=new Collection<CustomFieldValueContainer>{new CustomFieldValueContainer{CustomFieldValueID=cv2.CustomFieldValueID}},
                                LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=8, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=150 },
                                new ResourceSpreadDto{Id=9, LaborSpreadDate=Convert.ToDateTime("12/01/2010"), LaborSpreadValue=150 },
                                new ResourceSpreadDto{Id=10, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=150 }}
                                }, new ResourceTypeDto { Id=8, SpreadType = IES.Common.SpreadType.Hours, StartDateValue=LaborstartDate, EndDateValue=LaborendDate, ResourceID=LaborResource2.Id,
                                    PerformingOrgID=this.Perforg.Id,  SpreadCurveID = SpreadCurves.DiscreteHours, CustomFieldValueContainers=new Collection<CustomFieldValueContainer>{new CustomFieldValueContainer{CustomFieldValueID=cv2.CustomFieldValueID}},
                                LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=8, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=200 },
                                new ResourceSpreadDto{Id=9, LaborSpreadDate=Convert.ToDateTime("12/01/2010"), LaborSpreadValue=250 },
                                new ResourceSpreadDto{Id=10, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=10 }}}
                    }
            };
            BoeTaskElementDTO laborElement2 = new BoeTaskElementDTO
            {
                Id = 35,
                BoeID = this.Boe3.Id,
                BOETaskID = "333",
                TaskTitle = "MOCK TASK8",
                Description = "MOCK TASK8",
                StartDate = this.Boe3.StartDate,
                EndDate = this.Boe3.EndDate,
                taskElementLabors = new Collection<ResourceTypeDto>{ new ResourceTypeDto{Id = 36, SpreadType = SpreadType.Hours, StartDateValue = LaborstartDate, EndDateValue = LaborendDate,
                    ResourceID = LaborResource3.Id, PerformingOrgID = this.Perforg.Id, SpreadCurveID = SpreadCurves.DiscreteHours, LaborSpreads = new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id = 37,
                    LaborSpreadDate = Convert.ToDateTime("11/01/2010"), LaborSpreadValue = 400}}}}
            };
            BoeTaskElementDTO IWTAElement1 = new BoeTaskElementDTO
            {
                Id = 11,
                BoeID = this.Boe1.Id,
                BOETaskID = "2345",
                TaskTitle = "MOCK TASK3",
                Description = "MOCK TASK3",
                StartDate = this.Boe1.StartDate,
                EndDate = this.Boe1.EndDate,
                MOQType = MOQType.Judgment,
                CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { new CustomFieldValueContainer { CustomFieldValueID = cv.CustomFieldValueID } },
                taskElementLabors = new Collection<ResourceTypeDto>{new ResourceTypeDto{Id=12, SpreadType = IES.Common.SpreadType.Hours, StartDateValue=LaborstartDate, EndDateValue=LaborendDate,
                        ResourceID=IwtaResource.Id, PerformingOrgID=this.Perforg.Id,  SpreadCurveID = SpreadCurves.DiscreteHours, CustomFieldValueContainers=new Collection<CustomFieldValueContainer>{new CustomFieldValueContainer{CustomFieldValueID=cv2.CustomFieldValueID}},
                                LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=13, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=150 },
                                new ResourceSpreadDto{Id=14, LaborSpreadDate=Convert.ToDateTime("12/15/2010"), LaborSpreadValue=200 },
                                new ResourceSpreadDto{Id=15, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=200 }}
                            }}
            };
            BoeTaskElementDTO SubElement1 = new BoeTaskElementDTO
            {
                Id = 15,
                BoeID = this.Boe1.Id,
                BOETaskID = "3456",
                TaskTitle = "MOCK TASK4",
                Description = "MOCK TASK4",
                StartDate = this.Boe1.StartDate,
                EndDate = this.Boe1.EndDate,
                MOQType = MOQType.EstimatingRelationships,
                CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { new CustomFieldValueContainer { CustomFieldValueID = cv.CustomFieldValueID } },
                taskElementLabors = new Collection<ResourceTypeDto>{new ResourceTypeDto{Id=16, SpreadType = IES.Common.SpreadType.Hours, StartDateValue=LaborstartDate, EndDateValue=LaborendDate,
                        ResourceID=Subresource.Id, PerformingOrgID=this.Perforg.Id,  SpreadCurveID = SpreadCurves.DiscreteHours, CustomFieldValueContainers=new Collection<CustomFieldValueContainer>{new CustomFieldValueContainer{CustomFieldValueID=cv2.CustomFieldValueID}},
                                LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=17, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=150 },
                                new ResourceSpreadDto{Id=18, LaborSpreadDate=Convert.ToDateTime("12/01/2010"), LaborSpreadValue=300 },
                                new ResourceSpreadDto{Id=19, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=300 }}
                            }}
            };
            BoeTaskElementDTO matElement = new BoeTaskElementDTO
            {
                Id = 20,
                BoeID = this.Boe1.Id,
                BOETaskID = "111",
                TaskTitle = "MOCK TASK5",
                Description = "MOCK TASK5",
                StartDate = this.Boe1.StartDate,
                EndDate = this.Boe1.EndDate,
                MOQType = MOQType.LevelOfEffort,
                CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { new CustomFieldValueContainer { CustomFieldValueID = cv.CustomFieldValueID } },
                taskElementLabors = new Collection<ResourceTypeDto>{new ResourceTypeDto{Id = 21, SpreadType = SpreadType.Hours, StartDateValue = LaborstartDate, EndDateValue = LaborendDate,
                        ResourceID = MaterialLaborResource.Id, PerformingOrgID = this.Perforg.Id, SpreadCurveID = SpreadCurves.DiscreteHours, CustomFieldValueContainers=new Collection<CustomFieldValueContainer>{new CustomFieldValueContainer{CustomFieldValueID=cv.CustomFieldValueID}},
                        LaborSpreads = new Collection<ResourceSpreadDto>{ new ResourceSpreadDto{Id = 22, LaborSpreadDate = Convert.ToDateTime("11/01/2010"), LaborSpreadValue = 300 },
                        new ResourceSpreadDto{Id = 23, LaborSpreadDate = Convert.ToDateTime("12/01/2010"), LaborSpreadValue = 50 },
                        new ResourceSpreadDto{Id = 24, LaborSpreadDate = Convert.ToDateTime("01/01/2011"), LaborSpreadValue = 175}}
                        }}
            };
            BoeTaskElementDTO odcElement = new BoeTaskElementDTO
            {
                Id = 25,
                BoeID = this.Boe1.Id,
                BOETaskID = "222",
                TaskTitle = "MOCK TASK6",
                Description = "MOCK TASK6",
                StartDate = this.Boe1.StartDate,
                EndDate = this.Boe1.EndDate,
                MOQType = MOQType.Judgment,
                CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { new CustomFieldValueContainer { CustomFieldValueID = cv2.CustomFieldValueID } },
                taskElementLabors = new Collection<ResourceTypeDto>{new ResourceTypeDto{Id = 26, SpreadType = SpreadType.Hours, StartDateValue = LaborstartDate, EndDateValue = LaborendDate,
                        ResourceID = ODCLaborResource.Id, PerformingOrgID = this.Perforg.Id, SpreadCurveID = SpreadCurves.DiscreteHours, CustomFieldValueContainers=new Collection<CustomFieldValueContainer>{new CustomFieldValueContainer{CustomFieldValueID=cv2.CustomFieldValueID}},
                        LaborSpreads = new Collection<ResourceSpreadDto>{ new ResourceSpreadDto{Id = 27, LaborSpreadDate = Convert.ToDateTime("11/01/2010"), LaborSpreadValue = 30 },
                        new ResourceSpreadDto{Id = 28, LaborSpreadDate = Convert.ToDateTime("12/01/2010"), LaborSpreadValue = 450 },
                        new ResourceSpreadDto{Id = 29, LaborSpreadDate = Convert.ToDateTime("01/01/2011"), LaborSpreadValue = 225}}
                        }}
            };
            BoeTaskElementDTO travelElement = new BoeTaskElementDTO
            {
                Id = 30,
                BoeID = this.Boe1.Id,
                BOETaskID = "333",
                TaskTitle = "MOCK TASK7",
                Description = "MOCK TASK7",
                StartDate = this.Boe1.StartDate,
                EndDate = this.Boe1.EndDate,
                MOQType = MOQType.Factor,
                CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { new CustomFieldValueContainer { CustomFieldValueID = cv.CustomFieldValueID } },
                taskElementLabors = new Collection<ResourceTypeDto>{new ResourceTypeDto{Id = 31, SpreadType = SpreadType.Hours, StartDateValue = LaborstartDate, EndDateValue = LaborendDate,
                        ResourceID = TravelLaborResource.Id, PerformingOrgID = this.Perforg.Id, SpreadCurveID = SpreadCurves.DiscreteHours, CustomFieldValueContainers=new Collection<CustomFieldValueContainer>{new CustomFieldValueContainer{CustomFieldValueID=cv2.CustomFieldValueID}},
                        LaborSpreads = new Collection<ResourceSpreadDto>{ new ResourceSpreadDto{Id = 32, LaborSpreadDate = Convert.ToDateTime("11/01/2010"), LaborSpreadValue = 430 },
                        new ResourceSpreadDto{Id = 33, LaborSpreadDate = Convert.ToDateTime("12/01/2010"), LaborSpreadValue = 150 },
                        new ResourceSpreadDto{Id = 34, LaborSpreadDate = Convert.ToDateTime("01/01/2011"), LaborSpreadValue = 365}}
                        }}
            };

            TravelTripType travelTrip = new TravelTripType { TravelTripID = 1, TripDate = Convert.ToDateTime("07/01/2011"), BoeID = this.Boe2.Id, GroupID = 1, NumOfDays = 2, NumOfIntervals = 0, NumOfOccurences = 0, NumOfPeople = 4, NumOfTrips = 2, PerfOrgID = this.Perforg.Id, Segment = SegmentType.TS, SystemTripID = 1, CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { new CustomFieldValueContainer { CustomFieldValueID = cv2.CustomFieldValueID } } };
            TravelTripType travelTrip2 = new TravelTripType { TravelTripID = 2, TripDate = Convert.ToDateTime("09/01/2011"), BoeID = this.Boe2.Id, GroupID = 2, NumOfDays = 2, NumOfIntervals = 0, NumOfOccurences = 0, NumOfPeople = 4, NumOfTrips = 2, PerfOrgID = this.Perforg.Id, Segment = SegmentType.TS, SystemTripID = 1, CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { new CustomFieldValueContainer { CustomFieldValueID = cv2.CustomFieldValueID } } };
            TravelTripType travelTrip3 = new TravelTripType { TravelTripID = 3, TripDate = Convert.ToDateTime("10/01/2011"), BoeID = this.Boe2.Id, GroupID = 3, NumOfDays = 2, NumOfIntervals = 0, NumOfOccurences = 0, NumOfPeople = 4, NumOfTrips = 2, PerfOrgID = this.Perforg.Id, Segment = SegmentType.TS, SystemTripID = 1, CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { new CustomFieldValueContainer { CustomFieldValueID = cv2.CustomFieldValueID } } };
            TravelTripType travelTrip4 = new TravelTripType { TravelTripID = 4, TripDate = Convert.ToDateTime("12/01/2011"), BoeID = this.Boe2.Id, GroupID = 4, NumOfDays = 2, NumOfIntervals = 0, NumOfOccurences = 0, NumOfPeople = 4, NumOfTrips = 2, PerfOrgID = this.Perforg.Id, Segment = SegmentType.DS, SystemTripID = 1, CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { new CustomFieldValueContainer { CustomFieldValueID = cv2.CustomFieldValueID } } };

            TravelDTO travel = new TravelDTO { Id = 1, TaskTitle = "MOCKTRAVEL", TravelTrips = new Collection<TravelTripType> { travelTrip, travelTrip2, travelTrip3, travelTrip4 }, BoeID = this.Boe2.Id, StartDate = Convert.ToDateTime("07/01/2011"), EndDate = Convert.ToDateTime("12/01/2011"), CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { new CustomFieldValueContainer { CustomFieldValueID = cv.CustomFieldValueID } } };

            //setup a odc element under the second BOE
            OtherDirectCostSpread odcSpread1 = new OtherDirectCostSpread { ODCSpreadID = 1, CostSpreadValue = 2000, BoeID = this.Boe2.Id, ODCSpreadDate = Convert.ToDateTime("01/01/2011") };
            OtherDirectCostSpread odcSpread2 = new OtherDirectCostSpread { ODCSpreadID = 2, CostSpreadValue = 3000, BoeID = this.Boe2.Id, ODCSpreadDate = Convert.ToDateTime("01/01/2011") };
            OtherDirectCostType odcType1 = new OtherDirectCostType { ODCTypeID = 1, BoeID = this.Boe2.Id, PerformingOrgID = this.Perforg.Id, ResourceID = ODCResource1.Id, StartDate = Convert.ToDateTime("01/01/2011"), EndDate = Convert.ToDateTime("01/01/2011"), Cost = 2000, SpreadCurve = SpreadCurves.DiscreteHours, ODCSpreads = new Collection<OtherDirectCostSpread> { odcSpread1 } };
            OtherDirectCostType odcType2 = new OtherDirectCostType { ODCTypeID = 2, BoeID = this.Boe2.Id, PerformingOrgID = this.Perforg.Id, ResourceID = ODCResource2.Id, StartDate = Convert.ToDateTime("01/01/2011"), EndDate = Convert.ToDateTime("01/01/2011"), Cost = 3000, SpreadCurve = SpreadCurves.DiscreteHours, ODCSpreads = new Collection<OtherDirectCostSpread> { odcSpread2 } };
            OtherDirectCostDTO odc = new OtherDirectCostDTO { Id = 1, TaskID = "555", BoeID = this.Boe2.Id, TaskTitle = "MOCKODC", StartDate = Convert.ToDateTime("01/01/2011"), EndDate = Convert.ToDateTime("01/01/2011"), ODCTypes = new Collection<OtherDirectCostType> { odcType1, odcType2 } };


            //setup a material element under the first BOE
            MaterialDTO material = new MaterialDTO { Id = 1, StartDate = Convert.ToDateTime("12/01/2010"), EndDate = Convert.ToDateTime("12/01/2010"), BoeID = this.Boe1.Id, MoqText = "blah", TaskTitle = "MaterialTask"};

            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { laborElement1, laborElement2, IWTAElement1, SubElement1, matElement, odcElement, travelElement });
            this.retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, false)).Returns(new Collection<TravelDTO> { travel });
            this.retriever.Setup(x => x.GetMaterialsByBoeIds(It.IsAny<Collection<int>>(), false)).Returns(new Collection<MaterialDTO> { material });
            this.retriever.Setup(x => x.GetOdcCollectionByBoeIds(It.IsAny<List<int>>(), false)).Returns(new Collection<OtherDirectCostDTO> { odc });
            this.retriever.Setup(x => x.GetMoqTypeSelectionsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<MoqTypeSelection>());
            workspace.UsingTemplateBOE = false;

            var TripCalculate = new Mock<TravelTripCostCalculation>();
            var rmsTripCalculate = new Mock<RMSZoneTravelRatesFeesDataLoader>();
            rmsTripCalculate.Setup(x => x.getAllFeesAndCostsByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSTravelNonzoneFeesAndCostsDTO>());
            rmsTripCalculate.Setup(x => x.getAllEscalationRatesByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSEscalationRatesDTO>());

            TripCalculate.Setup(x => x.CalculateTravelCost(travelTrip, workspace)).Returns(new TravelTripCostData(500, 1, 1, 1, 1, 1, 1, 1, 1, 1));
            TripCalculate.Setup(x => x.CalculateTravelCost(travelTrip2, workspace)).Returns(new TravelTripCostData(300, 1, 1, 1, 1, 1, 1, 1, 1, 1));
            TripCalculate.Setup(x => x.CalculateTravelCost(travelTrip3, workspace)).Returns(new TravelTripCostData(500, 1, 1, 1, 1, 1, 1, 1, 1, 1));
            TripCalculate.Setup(x => x.CalculateTravelCost(travelTrip4, workspace)).Returns(new TravelTripCostData(100, 1, 1, 1, 1, 1, 1, 1, 1, 1));

            ProPricerExporter expReport = new ProPricerExporter(TripCalculate.Object, rmsTripCalculate.Object, this.retriever.Object, this.commonDataMapper.Object);

            ProPricerDTO proPricerExport = SetUpProPricerDTO(this.Workspace.Id);

            PpDataReadyForExport result = expReport.ExportProPricer(proPricerExport, workspace);
            Collection<string> TaskData = result.TaskData.ToCollection();
            Collection<string> ResourceCost = result.ResourceData.ToCollection();

            Assert.IsTrue(TaskData != null, "Task Data is not null");
            Assert.IsTrue(ResourceCost != null, "Resouce Cost data is not null");

            //  there is no other way to verify the results other than hard coding string values
            Assert.AreEqual("112005,,TOTAL,1.1,122010,LIDN000001,\"PE\",\"MOCK TASK2\",\"mock clin1 title\",\"mock wbs title\",2.2,\"an exercise class\",\"where you learn past events\",\"History\",\"BBBBBBB\",\"KristinePO\",\"Level of Effort\",20,6,7,\"Labor/Material BOE\",\"1234\",", TaskData[0], "The first BOE labor task (first resource) did not match this value");
            Assert.AreEqual("112005,,TOTAL,1.1,122010,LIDN000002,\"PE\",\"MOCK TASK2\",\"mock clin1 title\",\"mock wbs title\",2.2,\"an exercise class\",\"where you learn past events\",\"History\",\"EEEEEEE\",\"KristinePO\",\"Level of Effort\",20,6,8,\"Labor/Material BOE\",\"1234\",", TaskData[1], "The first BOE labor task (second resource) did not match this value");
            Assert.AreEqual("112005,,TOTAL,1.1,122010,IIDN000001,\"PE\",\"MOCK TASK3\",\"mock clin1 title\",\"mock wbs title\",2.2,\"an exercise class\",\"where you learn past events\",\"History\",\"CCCC\",\"KristinePO\",\"Judgment\",20,11,12,\"Labor/Material BOE\",\"2345\",", TaskData[2], "The BOE iwta task did not match this value");
            Assert.AreEqual("112005,,TOTAL,1.1,122010,SIDN000001,\"PE\",\"MOCK TASK4\",\"mock clin1 title\",\"mock wbs title\",2.2,\"an exercise class\",\"where you learn past events\",\"History\",\"DDDD\",\"KristinePO\",\"Estimating Relationships\",20,15,16,\"Labor/Material BOE\",\"3456\",", TaskData[3], "The BOE sub task did not match this value");
            Assert.AreEqual("112005,,TOTAL,1.1,122010,MIDN000001,\"PE\",\"MOCK TASK5\",\"mock clin1 title\",\"mock wbs title\",2.2,\"an exercise class\",,,\"FFFFFFF\",\"KristinePO\",\"Level of Effort\",20,20,21,\"Labor/Material BOE\",\"111\",", TaskData[4], "The BOE material labor task did not match this value");
            Assert.AreEqual("112005,,TOTAL,1.1,122010,TIDN000001,\"PE\",\"MOCK TASK7\",\"mock clin1 title\",\"mock wbs title\",2.2,\"an exercise class\",\"where you learn past events\",\"History\",\"HHHHHHH\",\"KristinePO\",\"Factor\",20,30,31,\"Labor/Material BOE\",\"333\",", TaskData[5], "The BOE travel labor task did not match this value");
            Assert.AreEqual("112005,,TOTAL,1.1,122010,OIDN000001,,\"MOCK TASK6\",\"mock clin1 title\",\"mock wbs title\",2.2,,\"where you learn past events\",\"History\",\"GGGGGGG\",\"KristinePO\",\"Judgment\",20,25,26,\"Labor/Material BOE\",\"222\",", TaskData[6], "The BOE odc labor task did not match this value");
            Assert.AreEqual("112006,,TOTAL,,012007,LIDN000003,,\"MOCK TASK8\",\"\",\"mock wbs title\",2.2,,,,\"IIIIIII\",\"KristinePO\",\"\",22,35,36,\"\",\"333\",", TaskData[11], "The second BOE labor task did not match this value");

            //resource data
            Assert.AreEqual(",\"BBBBBBB\",LIDN000001,2.2,\"where you learn past events\",\"History\",,D,1.1,\"mock clin1 title\",112010,\"mock wbs title\",20,6,7,\"Labor/Material BOE\",\"KristinePO\",\"MOCK TASK2\",\"1234\",150,150,150,", ResourceCost[0], "The first BOE labor resource did not match this value");
            Assert.AreEqual(",\"EEEEEEE\",LIDN000002,2.2,\"where you learn past events\",\"History\",,D,1.1,\"mock clin1 title\",112010,\"mock wbs title\",20,6,8,\"Labor/Material BOE\",\"KristinePO\",\"MOCK TASK2\",\"1234\",200,250,10,", ResourceCost[1], "The second BOE labor resource did not match this value");
            Assert.AreEqual(",\"CCCC\",IIDN000001,2.2,\"where you learn past events\",\"History\",,D,1.1,\"mock clin1 title\",112010,\"mock wbs title\",20,11,12,\"Labor/Material BOE\",\"KristinePO\",\"MOCK TASK3\",\"2345\",150,200,200,", ResourceCost[2], "The BOE iwta resource did not match this value");
            Assert.AreEqual(",\"DDDD\",SIDN000001,2.2,\"where you learn past events\",\"History\",,D,1.1,\"mock clin1 title\",112010,\"mock wbs title\",20,15,16,\"Labor/Material BOE\",\"KristinePO\",\"MOCK TASK4\",\"3456\",150,300,300,", ResourceCost[3], "The BOE sub resource did not match this value");
            Assert.AreEqual(",\"FFFFFFF\",MIDN000001,2.2,,,,D,1.1,\"mock clin1 title\",112010,\"mock wbs title\",20,20,21,\"Labor/Material BOE\",\"KristinePO\",\"MOCK TASK5\",\"111\",300,50,175,", ResourceCost[4], "The BOE material labor resource did not match this value");
            Assert.AreEqual(",\"HHHHHHH\",TIDN000001,2.2,\"where you learn past events\",\"History\",,D,1.1,\"mock clin1 title\",112010,\"mock wbs title\",20,30,31,\"Labor/Material BOE\",\"KristinePO\",\"MOCK TASK7\",\"333\",430,150,365,", ResourceCost[5], "The BOE travel labor resource did not match this value");
            Assert.AreEqual(",\"GGGGGGG\",OIDN000001,2.2,\"where you learn past events\",\"History\",,D,1.1,\"mock clin1 title\",112010,\"mock wbs title\",20,25,26,\"Labor/Material BOE\",\"KristinePO\",\"MOCK TASK6\",\"222\",30,450,225,", ResourceCost[6], "The BOE odc labor resource did not match this value");
            Assert.AreEqual(",\"IIIIIII\",LIDN000003,2.2,,,,D,,\"\",112010,\"mock wbs title\",22,35,36,\"\",\"KristinePO\",\"MOCK TASK8\",\"333\",400,0,0,", ResourceCost[11], "The third BOE labor resource did not match this value");
        }

        [TestMethod, ExpectedException(typeof(GenValidationException))]
        public void BL_ExportPropricer_BadResources()
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsDataLoader.Object);

            FullWorkspace workspace = new FullWorkspace(this.Workspace);

            BoeTaskElementDTO laborElement1 = new BoeTaskElementDTO
            {
                Id = 6,
                BoeID = this.Boe1.Id,
                BOETaskID = "1234",
                TaskTitle = "MOCK TASK2",
                Description = "MOCK TASK2",
                StartDate = this.Boe1.StartDate,
                EndDate = this.Boe1.EndDate,
                MOQType = MOQType.LevelOfEffort,
                taskElementLabors =
                    new Collection<ResourceTypeDto>
                    {
                        new ResourceTypeDto
                        {
                            Id=7,
                            StartDateValue = Convert.ToDateTime("11/01/2010"),
                            EndDateValue = Convert.ToDateTime("01/01/2011"),
                            ResourceID = null,
                            PerformingOrgID = this.Perforg.Id,
                            SpreadCurveID = SpreadCurves.DiscreteHours,
                            LaborSpreads = new Collection<ResourceSpreadDto>
                            {
                                new ResourceSpreadDto
                                {
                                    Id=8,
                                    LaborSpreadDate = Convert.ToDateTime("11/01/2010"),
                                    LaborSpreadValue = 150
                                },
                            new ResourceSpreadDto{ Id = 9, LaborSpreadDate = Convert.ToDateTime("12/01/2010"), LaborSpreadValue = 150 },
                            new ResourceSpreadDto{ Id = 10, LaborSpreadDate = Convert.ToDateTime("01/01/2011"), LaborSpreadValue = 150 }}
                        }
                    }
            };


            this.retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>())).Returns(new List<PerformingOrgDTO> { this.Perforg });
            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { laborElement1 });
            this.retriever.Setup(x => x.GetOdcCollectionByBoeIds(It.IsAny<List<int>>(), false)).Returns(new Collection<OtherDirectCostDTO> { });
            this.retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, false)).Returns(new Collection<TravelDTO> { });
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullBoe>() { new FullBoe(this.Boe1), new FullBoe(this.Boe2) });

            var TripCalculate = new Mock<TravelTripCostCalculation>();
            var rmsTripCalculate = new Mock<RMSZoneTravelRatesFeesDataLoader>();
            rmsTripCalculate.Setup(x => x.getAllFeesAndCostsByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSTravelNonzoneFeesAndCostsDTO>());
            rmsTripCalculate.Setup(x => x.getAllEscalationRatesByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSEscalationRatesDTO>());

            ProPricerExporter expReport = new ProPricerExporter(TripCalculate.Object, rmsTripCalculate.Object, this.retriever.Object, this.commonDataMapper.Object);
            ProPricerDTO proPricerExport = SetUpProPricerDTO(this.Workspace.Id);

            expReport.ExportProPricer(proPricerExport, workspace);
        }

        [TestMethod, ExpectedException(typeof(GenValidationException))]
        public void BL_ExportPropricer_BadPerfOrgs1()
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsDataLoader.Object);

            FullWorkspace workspace = new FullWorkspace(this.Workspace);

            BoeTaskElementDTO laborElement1 = new BoeTaskElementDTO
            {
                Id = 6,
                BoeID = this.Boe1.Id,
                BOETaskID = "1234",
                TaskTitle = "MOCK TASK2",
                Description = "MOCK TASK2",
                StartDate = this.Boe1.StartDate,
                EndDate = this.Boe1.EndDate,
                MOQType = MOQType.LevelOfEffort,
                taskElementLabors =
                    new Collection<ResourceTypeDto>
                    {
                        new ResourceTypeDto
                        {
                            Id=7,
                            StartDateValue = Convert.ToDateTime("11/01/2010"),
                            EndDateValue = Convert.ToDateTime("01/01/2011"),
                            ResourceID = 2,
                            PerformingOrgID = null,
                            SpreadCurveID = SpreadCurves.DiscreteHours,
                            LaborSpreads = new Collection<ResourceSpreadDto>
                            {
                                new ResourceSpreadDto
                                {
                                    Id=8,
                                    LaborSpreadDate = Convert.ToDateTime("11/01/2010"),
                                    LaborSpreadValue = 150
                                },
                            new ResourceSpreadDto{ Id = 9, LaborSpreadDate = Convert.ToDateTime("12/01/2010"), LaborSpreadValue = 150 },
                            new ResourceSpreadDto{ Id = 10, LaborSpreadDate = Convert.ToDateTime("01/01/2011"), LaborSpreadValue = 150 }}
                        }
                    }
            };

            this.retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>())).Returns(new List<PerformingOrgDTO> { this.Perforg });
            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { laborElement1 });
            this.retriever.Setup(x => x.GetOdcCollectionByBoeIds(It.IsAny<List<int>>(), false)).Returns(new Collection<OtherDirectCostDTO> { });
            this.retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, false)).Returns(new Collection<TravelDTO> { });
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullBoe>() { new FullBoe(this.Boe1), new FullBoe(this.Boe2) });

            var TripCalculate = new Mock<TravelTripCostCalculation>();
            var rmsTripCalculate = new Mock<RMSZoneTravelRatesFeesDataLoader>();
            rmsTripCalculate.Setup(x => x.getAllFeesAndCostsByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSTravelNonzoneFeesAndCostsDTO>());
            rmsTripCalculate.Setup(x => x.getAllEscalationRatesByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSEscalationRatesDTO>());

            ProPricerExporter expReport = new ProPricerExporter(TripCalculate.Object, rmsTripCalculate.Object, this.retriever.Object, this.commonDataMapper.Object);
            ProPricerDTO proPricerExport = SetUpProPricerDTO(this.Workspace.Id);

            expReport.ExportProPricer(proPricerExport, workspace);
        }

        [TestMethod, ExpectedException(typeof(GenValidationException))]
        public void BL_ExportPropricer_BadPerfOrgs2()
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsDataLoader.Object);

            FullWorkspace workspace = new FullWorkspace(this.Workspace);

            BoeTaskElementDTO laborElement1 = new BoeTaskElementDTO
            {
                Id = 6,
                BoeID = this.Boe1.Id,
                BOETaskID = "1234",
                TaskTitle = "MOCK TASK2",
                Description = "MOCK TASK2",
                StartDate = this.Boe1.StartDate,
                EndDate = this.Boe1.EndDate,
                MOQType = MOQType.LevelOfEffort,
                taskElementLabors =
                    new Collection<ResourceTypeDto>
                    {
                        new ResourceTypeDto
                        {
                            Id=7,
                            StartDateValue = Convert.ToDateTime("11/01/2010"),
                            EndDateValue = Convert.ToDateTime("01/01/2011"),
                            ResourceID = 2,
                            PerformingOrgID = this.Perforg.Id + 2,
                            SpreadCurveID = SpreadCurves.DiscreteHours,
                            LaborSpreads = new Collection<ResourceSpreadDto>
                            {
                                new ResourceSpreadDto
                                {
                                    Id=8,
                                    LaborSpreadDate = Convert.ToDateTime("11/01/2010"),
                                    LaborSpreadValue = 150
                                },
                            new ResourceSpreadDto{ Id = 9, LaborSpreadDate = Convert.ToDateTime("12/01/2010"), LaborSpreadValue = 150 },
                            new ResourceSpreadDto{ Id = 10, LaborSpreadDate = Convert.ToDateTime("01/01/2011"), LaborSpreadValue = 150 }}
                        }
                    }
            };

            this.retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>())).Returns(new List<PerformingOrgDTO> { });
            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { laborElement1 });
            this.retriever.Setup(x => x.GetOdcCollectionByBoeIds(It.IsAny<List<int>>(), false)).Returns(new Collection<OtherDirectCostDTO> { });
            this.retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, false)).Returns(new Collection<TravelDTO> { });
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullBoe>() { new FullBoe(this.Boe1), new FullBoe(this.Boe2) });

            var TripCalculate = new Mock<TravelTripCostCalculation>();
            var rmsTripCalculate = new Mock<RMSZoneTravelRatesFeesDataLoader>();
            rmsTripCalculate.Setup(x => x.getAllFeesAndCostsByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSTravelNonzoneFeesAndCostsDTO>());
            rmsTripCalculate.Setup(x => x.getAllEscalationRatesByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSEscalationRatesDTO>());
            
            ProPricerExporter expReport = new ProPricerExporter(TripCalculate.Object, rmsTripCalculate.Object, this.retriever.Object, this.commonDataMapper.Object);
            ProPricerDTO proPricerExport = SetUpProPricerDTO(this.Workspace.Id);

            expReport.ExportProPricer(proPricerExport, workspace);
        }

        [TestMethod, System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public void BL_ExportProPricer_MultiBOE_NullCLinWbs()
        {
            Collection<BoeDTO> toReturn = new Collection<BoeDTO>();
            toReturn.Add(this.Boe1);

            CustomFieldDTO customField1 = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.TaskDisplay };
            CustomFieldDTO customField2 = new CustomFieldDTO { Id = 2, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay };

            CustomFieldValueDTO cv = new CustomFieldValueDTO();
            cv.CustomFieldID = customField1.Id;
            cv.CustomFieldValueID = 1;
            cv.CustomFieldValueName = "P<br/><BR /><bR/>E";
            cv.CustomFieldValueDescription = "an exe<br/><BR /><bR/>rcise class";

            CustomFieldValueDTO cv2 = new CustomFieldValueDTO();
            cv2.CustomFieldID = customField2.Id;
            cv2.CustomFieldValueID = 2;
            cv2.CustomFieldValueName = "His<br/><BR /><bR/>tory";
            cv2.CustomFieldValueDescription = "where you learn <br/><BR /><bR/>past events";

            DateTime LaborstartDate = Convert.ToDateTime("11/01/2010");
            DateTime LaborendDate = Convert.ToDateTime("01/01/2011");

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsDataLoader.Object);
            FullWorkspace workspace = new FullWorkspace(this.Workspace);

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullBoe>() { new FullBoe(this.Boe1), new FullBoe(this.Boe2) });
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullWbs>() { new FullWbs(this.Wbs) });
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullClin>() { new FullClin(this.Clin1) });
            this.retriever.Setup(x => x.GetClinById(this.Boe1.CLINID.Value)).Returns(this.Clin1);
            this.retriever.Setup(x => x.GetWbsById(this.Wbs.Id)).Returns(this.Wbs);
            this.retriever.Setup(x => x.GetMoqTypeSelectionsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<MoqTypeSelection>());
            workspace.UsingTemplateBOE = false;

            // setup resources dto mapper
            Collection<ResourceDTO> resources = new Collection<ResourceDTO>();
            resources.Add(this.Resource);

            this.retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new List<CustomFieldValueDTO>() { cv, cv2 });
            this.retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>())).Returns(new List<PerformingOrgDTO>() { this.Perforg });
            this.retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(resources);
            this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(resources);
            this.retriever.Setup(x => x.GetFullWorkspaceById(workspace.Id)).Returns(workspace);
            this.factory.Setup(x => x.CreateFullWorkspace(It.IsAny<int>())).Returns(workspace);
            this.factory.Setup(x => x.CreateFullBoes(It.IsAny<ICollection<int>>())).Returns(new List<FullBoe>() { new FullBoe(this.Boe1) });
            foreach (FullBoe boe in workspace.Boes)
            {
                boe.Title = "test boe";
                boe.IsMultiClinWbs = true;
            }

            BoeTaskElementDTO task1 = new BoeTaskElementDTO
            {
                BoeID = this.Boe1.Id,
                Id = 1,
                BOETaskID = "123",
                TaskTitle = "MOCK TASK1",
                Description = "MOCK TASK1",
                StartDate = this.Boe1.StartDate,
                EndDate = this.Boe1.EndDate,
                IMS_ID = "500",
                MOQType = MOQType.Comparison,
                CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { new CustomFieldValueContainer { CustomFieldValueID = cv.CustomFieldValueID } },
                taskElementLabors = new Collection<ResourceTypeDto>{new ResourceTypeDto{Id=1, SpreadType = IES.Common.SpreadType.Hours, StartDateValue=LaborstartDate, EndDateValue=LaborendDate, ResourceID=this.Resource.Id, PerformingOrgID=this.Perforg.Id,SpreadCurveID = SpreadCurves.DiscreteHours,
                            LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=1, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=200 },
                            //discrete values of 0 are not stored in the db so replicate that here by removing a month
                            new ResourceSpreadDto{Id=3, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=200 }}
                        }}
            };
            //Two resources for this task to test multiple rows for tasks with multiple resources
            BoeTaskElementDTO task2 = new BoeTaskElementDTO
            {
                BoeID = this.Boe2.Id,
                Id = 2,
                BOETaskID = "1234",
                TaskTitle = "MOCK TASK2",
                Description = "MOCK TASK2",
                StartDate = this.Boe1.StartDate,
                EndDate = this.Boe1.EndDate,
                MOQType = MOQType.Factor,
                taskElementLabors = new Collection<ResourceTypeDto>{new ResourceTypeDto{Id=2, SpreadType = IES.Common.SpreadType.Hours, StartDateValue=LaborstartDate, EndDateValue=LaborendDate, ResourceID=this.Resource.Id, PerformingOrgID=this.Perforg.Id,  SpreadCurveID = SpreadCurves.DiscreteHours, CustomFieldValueContainers=new Collection<CustomFieldValueContainer>{new CustomFieldValueContainer{CustomFieldValueID=cv2.CustomFieldValueID}},
                            LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=4, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=150 },
                            new ResourceSpreadDto{Id=5, LaborSpreadDate=Convert.ToDateTime("12/01/2010"), LaborSpreadValue=150 },
                            new ResourceSpreadDto{Id=6, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=150 }}
                            },new ResourceTypeDto{Id=3, SpreadType = IES.Common.SpreadType.Hours, StartDateValue=LaborstartDate, EndDateValue=LaborendDate, ResourceID=this.Resource.Id, PerformingOrgID=this.Perforg.Id,  SpreadCurveID = SpreadCurves.DiscreteHours, CustomFieldValueContainers=new Collection<CustomFieldValueContainer>{new CustomFieldValueContainer{CustomFieldValueID=cv2.CustomFieldValueID}},
                            LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=4, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=50 },
                            new ResourceSpreadDto{Id=5, LaborSpreadDate=Convert.ToDateTime("12/01/2010"), LaborSpreadValue=150 },
                            new ResourceSpreadDto{Id=6, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=250 }}
                            }}
            };

            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { task1, task2 });
            this.retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, false)).Returns(new Collection<TravelDTO> { });
            this.retriever.Setup(x => x.GetMaterialsByBoeIds(It.IsAny<Collection<int>>(), false)).Returns(new Collection<MaterialDTO> { });
            this.retriever.Setup(x => x.GetOdcCollectionByBoeIds(It.IsAny<List<int>>(), false)).Returns(new Collection<OtherDirectCostDTO> { });

            retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<CustomFieldDTO> { customField1, customField2 });

            var TripCalculate = new Mock<TravelTripCostCalculation>();
            var rmsTripCalculate = new Mock<RMSZoneTravelRatesFeesDataLoader>();
            rmsTripCalculate.Setup(x => x.getAllFeesAndCostsByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSTravelNonzoneFeesAndCostsDTO>());
            rmsTripCalculate.Setup(x => x.getAllEscalationRatesByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSEscalationRatesDTO>());

            ProPricerExporter expReport = new ProPricerExporter(TripCalculate.Object, rmsTripCalculate.Object, this.retriever.Object, this.commonDataMapper.Object);
            ProPricerDTO proPricerExport = SetUpProPricerDTO(this.Workspace.Id);

            PpDataReadyForExport result = expReport.ExportProPricer(proPricerExport, workspace);
            Collection<string> TaskData = result.TaskData.ToCollection();
            Collection<string> ResourceCost = result.ResourceData.ToCollection();

            Assert.IsTrue(TaskData != null, "Task Data is not null");
            Assert.IsTrue(ResourceCost != null, "Resource Cost data is not null");

            //  there is no other way to verify the results other than hard coding string values
            Assert.AreEqual("112005,,TOTAL,,012006,LIDN000001,\"PE\",\"MOCK TASK1\",\"\",\"\",,\"an exercise class\",,,\"BBBBBBB\",\"KristinePO\",\"Comparison\",20,1,1,\"test boe\",\"123\",", TaskData[0], "The first task did not match this value");
            Assert.AreEqual("112005,,TOTAL,,012006,LIDN000002,,\"MOCK TASK2\",\"\",\"\",,,\"where you learn past events\",\"History\",\"BBBBBBB\",\"KristinePO\",\"Factor\",21,2,2,\"test boe\",\"1234\",", TaskData[1], "The second task (first resource) did not match this value");
            Assert.AreEqual("112005,,TOTAL,,012006,LIDN000003,,\"MOCK TASK2\",\"\",\"\",,,\"where you learn past events\",\"History\",\"BBBBBBB\",\"KristinePO\",\"Factor\",21,2,3,\"test boe\",\"1234\",", TaskData[2], "The second task (second resource) did not match this value");

            Assert.AreEqual("500,\"BBBBBBB\",LIDN000001,,,,,D,,\"\",112010,\"\",20,1,1,\"test boe\",\"KristinePO\",\"MOCK TASK1\",\"123\",200,0,200,", ResourceCost[0], "The first resource did not match this value");
            Assert.AreEqual(",\"BBBBBBB\",LIDN000002,,\"where you learn past events\",\"History\",,D,,\"\",112010,\"\",21,2,2,\"test boe\",\"KristinePO\",\"MOCK TASK2\",\"1234\",150,150,150,", ResourceCost[1], "The second resource did not match this value");
            Assert.AreEqual(",\"BBBBBBB\",LIDN000003,,\"where you learn past events\",\"History\",,D,,\"\",112010,\"\",21,2,3,\"test boe\",\"KristinePO\",\"MOCK TASK2\",\"1234\",50,150,250,", ResourceCost[2], "The third resource did not match this value");
            this.ResetTestData();
        }

        [TestMethod, System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public void BL_ExportProPricer_MultiBOE()
        {
            Collection<BoeDTO> toReturn = new Collection<BoeDTO>();
            toReturn.Add(this.Boe1);

            CustomFieldDTO customField1 = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.TaskDisplay };
            CustomFieldDTO customField2 = new CustomFieldDTO { Id = 2, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay };

            CustomFieldValueDTO cv = new CustomFieldValueDTO();
            cv.CustomFieldID = customField1.Id;
            cv.CustomFieldValueID = 1;
            cv.CustomFieldValueName = "P<br/><BR /><bR/>E";
            cv.CustomFieldValueDescription = "an <br/><BR /><bR/>exercise class";

            CustomFieldValueDTO cv2 = new CustomFieldValueDTO();
            cv2.CustomFieldID = customField2.Id;
            cv2.CustomFieldValueID = 2;
            cv2.CustomFieldValueName = "History<br/><BR /><bR/>";
            cv2.CustomFieldValueDescription = "whe<br/><BR /><bR/>re you learn past events";

            DateTime LaborstartDate = Convert.ToDateTime("11/01/2010");
            DateTime LaborendDate = Convert.ToDateTime("01/01/2011");

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsDataLoader.Object);
            FullWorkspace workspace = new FullWorkspace(this.Workspace);

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullBoe>() { new FullBoe(this.Boe1), new FullBoe(this.Boe2) });
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullWbs>() { new FullWbs(this.Wbs) });
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullClin>() { new FullClin(this.Clin1) });
            this.retriever.Setup(x => x.GetClinById(this.Boe1.CLINID.Value)).Returns(this.Clin1);
            this.retriever.Setup(x => x.GetWbsById(this.Wbs.Id)).Returns(this.Wbs);
            this.retriever.Setup(x => x.GetMoqTypeSelectionsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<MoqTypeSelection>());
            workspace.UsingTemplateBOE = false;

            // setup resources dto mapper
            Collection<ResourceDTO> resources = new Collection<ResourceDTO>();
            resources.Add(this.Resource);

            this.retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new List<CustomFieldValueDTO>() { cv, cv2 });
            this.retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>())).Returns(new List<PerformingOrgDTO>() { this.Perforg });
            this.retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(resources);
            this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(resources);
            this.retriever.Setup(x => x.GetFullWorkspaceById(workspace.Id)).Returns(workspace);
            this.factory.Setup(x => x.CreateFullWorkspace(It.IsAny<int>())).Returns(workspace);
            this.factory.Setup(x => x.CreateFullBoes(It.IsAny<ICollection<int>>())).Returns(new List<FullBoe>() { new FullBoe(this.Boe1) });
            foreach (FullBoe boe in workspace.Boes)
            {
                boe.Title = "test boe";
                boe.IsMultiClinWbs = true;
            }

            BoeTaskElementDTO task1 = new BoeTaskElementDTO
            {
                BoeID = this.Boe1.Id,
                Id = 1,
                BOETaskID = "123",
                TaskTitle = "MOCK TASK1",
                Description = "MOCK TASK1",
                StartDate = this.Boe1.StartDate,
                EndDate = this.Boe1.EndDate,
                IMS_ID = "500",
                MOQType = MOQType.Comparison,
                CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { new CustomFieldValueContainer { CustomFieldValueID = cv.CustomFieldValueID } },
                taskElementLabors = new Collection<ResourceTypeDto>{new ResourceTypeDto{Id=1, SpreadType = IES.Common.SpreadType.Hours, StartDateValue=LaborstartDate, EndDateValue=LaborendDate, ResourceID=this.Resource.Id, PerformingOrgID=this.Perforg.Id,SpreadCurveID = SpreadCurves.DiscreteHours,
                            LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=1, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=200 },
                            //discrete values of 0 are not stored in the db so replicate that here by removing a month
                            new ResourceSpreadDto{Id=3, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=200 }}, WBSID = this.Wbs.Id, CLINID = this.Clin3.Id
                        }}
            };
            //Two resources for this task to test multiple rows for tasks with multiple resources
            BoeTaskElementDTO task2 = new BoeTaskElementDTO
            {
                BoeID = this.Boe2.Id,
                Id = 2,
                BOETaskID = "1234",
                TaskTitle = "MOCK TASK2",
                Description = "MOCK TASK2",
                StartDate = this.Boe1.StartDate,
                EndDate = this.Boe1.EndDate,
                MOQType = MOQType.Factor,
                taskElementLabors = new Collection<ResourceTypeDto>{new ResourceTypeDto{Id=2, SpreadType = IES.Common.SpreadType.Hours, StartDateValue=LaborstartDate, EndDateValue=LaborendDate, ResourceID=this.Resource.Id, PerformingOrgID=this.Perforg.Id,  SpreadCurveID = SpreadCurves.DiscreteHours, CustomFieldValueContainers=new Collection<CustomFieldValueContainer>{new CustomFieldValueContainer{CustomFieldValueID=cv2.CustomFieldValueID}},
                            LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=4, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=150 },
                            new ResourceSpreadDto{Id=5, LaborSpreadDate=Convert.ToDateTime("12/01/2010"), LaborSpreadValue=150 },
                            new ResourceSpreadDto{Id=6, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=150 }}
                            },new ResourceTypeDto{Id=3, SpreadType = IES.Common.SpreadType.Hours, StartDateValue=LaborstartDate, EndDateValue=LaborendDate, ResourceID=this.Resource.Id, PerformingOrgID=this.Perforg.Id,  SpreadCurveID = SpreadCurves.DiscreteHours, CustomFieldValueContainers=new Collection<CustomFieldValueContainer>{new CustomFieldValueContainer{CustomFieldValueID=cv2.CustomFieldValueID}},
                            LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=4, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=50 },
                            new ResourceSpreadDto{Id=5, LaborSpreadDate=Convert.ToDateTime("12/01/2010"), LaborSpreadValue=150 },
                            new ResourceSpreadDto{Id=6, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=250 }}
                            }}
            };

            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { task1, task2 });
            this.retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, false)).Returns(new Collection<TravelDTO> { });
            this.retriever.Setup(x => x.GetMaterialsByBoeIds(It.IsAny<Collection<int>>(), false)).Returns(new Collection<MaterialDTO> { });
            this.retriever.Setup(x => x.GetOdcCollectionByBoeIds(It.IsAny<List<int>>(), false)).Returns(new Collection<OtherDirectCostDTO> { });

            retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<CustomFieldDTO> { customField1, customField2 });

            var TripCalculate = new Mock<TravelTripCostCalculation>();
            var rmsTripCalculate = new Mock<RMSZoneTravelRatesFeesDataLoader>();
            rmsTripCalculate.Setup(x => x.getAllFeesAndCostsByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSTravelNonzoneFeesAndCostsDTO>());
            rmsTripCalculate.Setup(x => x.getAllEscalationRatesByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSEscalationRatesDTO>());

            ProPricerExporter expReport = new ProPricerExporter(TripCalculate.Object, rmsTripCalculate.Object, this.retriever.Object, this.commonDataMapper.Object);

            ProPricerDTO proPricerExport = SetUpProPricerDTO(this.Workspace.Id);

            PpDataReadyForExport result = expReport.ExportProPricer(proPricerExport, workspace);
            Collection<string> TaskData = result.TaskData.ToCollection();
            Collection<string> ResourceCost = result.ResourceData.ToCollection();

            Assert.IsTrue(TaskData != null, "Task Data is not null");
            Assert.IsTrue(ResourceCost != null, "Resource Cost data is not null");

            //  there is no other way to verify the results other than hard coding string values
            Assert.AreEqual("112005,,TOTAL,,012006,LIDN000001,\"PE\",\"MOCK TASK1\",\"\",\"mock wbs title\",2.2,\"an exercise class\",,,\"BBBBBBB\",\"KristinePO\",\"Comparison\",20,1,1,\"test boe\",\"123\",", TaskData[0], "The first task did not match this value");
            Assert.AreEqual("112005,,TOTAL,,012006,LIDN000002,,\"MOCK TASK2\",\"\",\"\",,,\"where you learn past events\",\"History\",\"BBBBBBB\",\"KristinePO\",\"Factor\",21,2,2,\"test boe\",\"1234\",", TaskData[1], "The second task (first resource) did not match this value");
            Assert.AreEqual("112005,,TOTAL,,012006,LIDN000003,,\"MOCK TASK2\",\"\",\"\",,,\"where you learn past events\",\"History\",\"BBBBBBB\",\"KristinePO\",\"Factor\",21,2,3,\"test boe\",\"1234\",", TaskData[2], "The second task (second resource) did not match this value");

            Assert.AreEqual("500,\"BBBBBBB\",LIDN000001,2.2,,,,D,,\"\",112010,\"mock wbs title\",20,1,1,\"test boe\",\"KristinePO\",\"MOCK TASK1\",\"123\",200,0,200,", ResourceCost[0], "The first resource did not match this value");
            Assert.AreEqual(",\"BBBBBBB\",LIDN000002,,\"where you learn past events\",\"History\",,D,,\"\",112010,\"\",21,2,2,\"test boe\",\"KristinePO\",\"MOCK TASK2\",\"1234\",150,150,150,", ResourceCost[1], "The second resource did not match this value");
            Assert.AreEqual(",\"BBBBBBB\",LIDN000003,,\"where you learn past events\",\"History\",,D,,\"\",112010,\"\",21,2,3,\"test boe\",\"KristinePO\",\"MOCK TASK2\",\"1234\",50,150,250,", ResourceCost[2], "The third resource did not match this value");


            this.ResetTestData();
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void BL_ExportPropricer_BadInput1()
        {
            var TripCalculate = new Mock<TravelTripCostCalculation>();
            ProPricerExporter expReport = new ProPricerExporter(TripCalculate.Object, null, this.retriever.Object, this.commonDataMapper.Object);
            expReport.ExportProPricer(null, new FullWorkspace());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void BL_ExportPropricer_BadInput2()
        {
            var TripCalculate = new Mock<TravelTripCostCalculation>();
            ProPricerExporter expReport = new ProPricerExporter(TripCalculate.Object, null, this.retriever.Object, this.commonDataMapper.Object);
            expReport.ExportProPricer(new ProPricerDTO(), null);
        }

        [TestMethod]
        public void BL_ExportProPricer_LaborElementsOnly_Offloaded()
        {
            PpDataReadyForExport result = this.RunProPricerExportProjectMap();

            //  there is no other way to verify the results other than hard coding string values
            //  Removed MOQ and Custom Fields from the hard-coded strings since ProjectMap doesn't save/use them
            Assert.AreEqual(2, result.TaskData.Count);
            Assert.AreEqual("112005,,TOTAL,mock clin1 title,012006,LIDN000001,,\"\",\"mock clin1 title\",\"mock wbs title\",2.2,,,,\"BBBBBBB\",\"KristinePO\",\"\",-100000,-100000,-100000,\"Boe1Title\",\"\",\"NRE\",", result.TaskData[0], "The first task did not match this value");
            Assert.AreEqual("112006,,TOTAL,mock clin1 title,012007,LIDN000002,,\"\",\"mock clin1 title\",\"mock wbs title\",2.2,,,,\"BBBBBBB\",\"KristinePO\",\"\",-100001,-100001,-100001,\"Boe2Title\",\"\",\"NRE\",", result.TaskData[1], "The second task (first resource) did not match this value");

            Assert.AreEqual(",\"BBBBBBB\",LIDN000001,2.2,,,,D,mock clin1 title,\"mock clin1 title\",112005,\"mock wbs title\",-100000,-100000,-100000,\"Boe1Title\",\"KristinePO\",\"\",\"\",133,133,134,", result.ResourceData[0], "The first resource did not match this value");
            Assert.AreEqual(",\"BBBBBBB\",LIDN000002,2.2,,,,D,mock clin1 title,\"mock clin1 title\",112006,\"mock wbs title\",-100001,-100001,-100001,\"Boe2Title\",\"KristinePO\",\"\",\"\",37,38,37,", result.ResourceData[1], "The second resource did not match this value");
            Assert.AreEqual(",\"BBBBBBB\",LIDN000002,2.2,,,,D,mock clin1 title,\"mock clin1 title\",112006,\"mock wbs title\",-100001,-100001,-100002,\"Boe2Title\",\"KristinePO\",\"\",\"\",75,75,75,", result.ResourceData[2], "The third resource did not match this value");

            Assert.AreEqual(1, result.OffloadTaskData.Count);
            Assert.AreEqual("112006,,TOTAL,mock clin1 title,012007,SIDN000001,,\"\",\"mock clin1 title\",\"mock wbs title\",2.2,,,,\"OFFLOADED_RESOURCE\",\"KristinePO\",\"\",-1,-1,-1,\"Boe2TitleOLKristinePOBBBBBBB\",\"\",\"NRE\",", result.OffloadTaskData[0], "The second task (first resource) did not match this value");
            Assert.AreEqual(",\"OFFLOADED_RESOURCE\",SIDN000001,2.2,,,,D,mock clin1 title,\"mock clin1 title\",112006,\"mock wbs title\",-1,-1,-1,\"Boe2TitleOLKristinePOBBBBBBB\",\"KristinePO\",\"\",\"\",760.00,740.00,760.00,", result.OffloadResourceData[0], "The second resource did not match this value");
            Assert.AreEqual(",\"OFFLOADED_RESOURCE\",SIDN000001,2.2,,,,D,mock clin1 title,\"mock clin1 title\",112006,\"mock wbs title\",-1,-1,-2,\"Boe2TitleOLKristinePOBBBBBBB\",\"KristinePO\",\"\",\"\",1500.00,1500.00,1500.00,", result.OffloadResourceData[1], "The third resource did not match this value");

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private PpDataReadyForExport RunProPricerExportProjectMap()
        {
            DateTime LaborstartDate = Convert.ToDateTime("11/01/2010");
            DateTime LaborendDate = Convert.ToDateTime("01/01/2011");

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), this.factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsDataLoader.Object);

            Mock<IProjectMapDataLoader> projectMapLoader = new Mock<IProjectMapDataLoader>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IProjectMapDataLoader), projectMapLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ISystemSettingDTODataLoader), systemSettingLoader.Object);

            ICollection<ProjectMapModelView> modelsWorkspace1 = new List<ProjectMapModelView> {
                new ProjectMapModelView { ClassOfCost = ClassOfCost.NonRecurring.ToDescription(), WbsNumber = this.Wbs.WbsNumber, CostCenter = this.Perforg.PerformingOrgName, ActivityID = "Boe1Title",
                    ActivityName = "Boe1Desc", WbsElementTitle = this.Wbs.WbsTitle, InitialResource = this.Resource.ResourceName, StartDate = this.Boe1.StartDate.Normalize(), EndDate = this.Boe1.EndDate.Normalize(),
                    Clin = this.Clin1.ClinTitle, SowNumber = "sow", SowTitle = "sow title", Task = "MOCK TASK1", Hours = 400, Dollars = 0, Rationale = "rationale1", CamName = "Test Name",
                    Category = "Cat1", Offload = false, AddDelete = "A"},

                new ProjectMapModelView { ClassOfCost = ClassOfCost.NonRecurring.ToDescription(), WbsNumber = this.Wbs.WbsNumber, CostCenter = this.Perforg.PerformingOrgName, ActivityID = "Boe2Title",
                    ActivityName = "Boe2Desc", WbsElementTitle = this.Wbs.WbsTitle, InitialResource = this.Resource.ResourceName, StartDate = this.Boe2.StartDate.Normalize(), EndDate = this.Boe2.EndDate.Normalize(),
                    Clin = this.Clin1.ClinTitle, SowNumber = "sow", SowTitle = "sow title", Task = "MOCK TASK2", Hours = 225, Dollars = 0, Rationale = "rationale1", CamName = "Test Name",
                    Category = "Cat1", Offload = true, AddDelete = "A"},
                new ProjectMapModelView { ClassOfCost = ClassOfCost.NonRecurring.ToDescription(), WbsNumber = this.Wbs.WbsNumber, CostCenter = this.Perforg.PerformingOrgName, ActivityID = "Boe2Title",
                    ActivityName = "Boe2Desc", WbsElementTitle = this.Wbs.WbsTitle, InitialResource = this.Resource.ResourceName, StartDate = this.Boe2.StartDate.Normalize(), EndDate = this.Boe2.EndDate.Normalize(),
                    Clin = this.Clin1.ClinTitle, SowNumber = "sow", SowTitle = "sow title", Task = "MOCK TASK2", Hours = 450, Dollars = 0, Rationale = "rationale1", CamName = "Test Name",
                    Category = "Cat1", Offload = true, AddDelete = "A"}
            };

            projectMapLoader.Setup(x => x.GetByWorkspaceId(this.Workspace.Id)).Returns(modelsWorkspace1);
            this.retriever.Setup(x => x.GetProjectMapDataByWorkspaceId(this.Workspace.Id)).Returns(modelsWorkspace1);

            // setup resources dto mapper
            Collection<ResourceDTO> resources = new Collection<ResourceDTO>();
            resources.Add(this.Resource);
            ResourceDTO subResource = new ResourceDTO()
            {
                Id = 5554,
                ElementOfCost = ElementOfCostType.Sub,
                LaborType = "Sub",
                RateType = RateType.Cost,
                ResourceDesc = "OFFLOADED_RESOURCE",
                ResourceName = "OFFLOADED_RESOURCE"
            };
            resources.Add(subResource);

            this.retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new List<CustomFieldValueDTO>());
            this.retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>())).Returns(new List<PerformingOrgDTO>() { this.Perforg });
            this.retriever.Setup(x => x.GetPerformingOrgsByListId(It.IsAny<int>())).Returns(new List<PerformingOrgDTO>() { this.Perforg });
            this.retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(resources);
            this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(resources);
            FullProjectMapWorkspace workspace = new FullProjectMapWorkspace(this.Workspace);
            workspace.ProjectMapType = ProjectMapType.NonTimePhasedProjectMap;
            workspace.RefreshBoes();
            workspace.LoadBoes();

            this.factory.Setup(x => x.CreateFullWorkspace(It.IsAny<WorkspaceDTO>())).Returns(workspace);
            this.retriever.Setup(x => x.GetFullWorkspaceById(workspace.Id)).Returns(workspace);
            this.factory.Setup(x => x.CreateFullWorkspace(It.IsAny<int>())).Returns(workspace);
            this.factory.Setup(x => x.CreateClonedProjectMapWorkspace(It.IsAny<FullWorkspace>())).Returns(workspace);

            this.factory.Setup(x => x.CreateFullClin(It.IsAny<ClinDTO>())).Returns((ClinDTO c) => new FullClin(c));
            this.factory.Setup(x => x.CreateFullWbs(It.IsAny<WbsDTO>())).Returns((WbsDTO w) => new FullWbs(w));
            this.factory.Setup(x => x.CreateFullBoe()).Returns(CreateFullBoe());
            this.factory.Setup(x => x.CreateFullBoe(It.IsAny<BoeDTO>())).Returns((BoeDTO b) => new FullBoe(b));

            this.retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, false)).Returns(new Collection<TravelDTO> { });
            this.retriever.Setup(x => x.GetMaterialsByBoeIds(It.IsAny<Collection<int>>(), false)).Returns(new Collection<MaterialDTO> { });
            this.retriever.Setup(x => x.GetOdcCollectionByBoeIds(It.IsAny<List<int>>(), false)).Returns(new Collection<OtherDirectCostDTO> { });
            this.retriever.Setup(x => x.GetMoqTypeSelectionsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<MoqTypeSelection>());
            workspace.UsingTemplateBOE = false;

            retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<CustomFieldDTO>());

            var TripCalculate = new Mock<TravelTripCostCalculation>();
            var rmsTripCalculate = new Mock<RMSZoneTravelRatesFeesDataLoader>();
            rmsTripCalculate.Setup(x => x.getAllFeesAndCostsByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSTravelNonzoneFeesAndCostsDTO>());
            rmsTripCalculate.Setup(x => x.getAllEscalationRatesByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSEscalationRatesDTO>());

            Mock<IOffloadRatesDTOLoader> offloadRatesLoader = new Mock<IOffloadRatesDTOLoader>();
            Collection<OffloadRatesDTO> offloadRates = new Collection<OffloadRatesDTO>
            {
                 new OffloadRatesDTO
                {
                    Id = 1,
                    HourlyRate = 20,
                    Percent = 0.50m,
                    PerformingOrg = this.Perforg.PerformingOrgName,
                    Resource = this.Resource.ResourceName,
                    SubResource = subResource.ResourceName,
                    Year = 2006
                },
                 new OffloadRatesDTO
                {
                    Id = 1,
                    HourlyRate = 20,
                    Percent = 0.50m,
                    PerformingOrg = this.Perforg.PerformingOrgName,
                    Resource = this.Resource.ResourceName,
                    SubResource = subResource.ResourceName,
                    Year = 2007
                },
                 new OffloadRatesDTO
                {
                    Id = 1,
                    HourlyRate = 20,
                    Percent = 0.50m,
                    PerformingOrg = this.Perforg.PerformingOrgName,
                    Resource = this.Resource.ResourceName,
                    SubResource = subResource.ResourceName,
                    Year = 2008
                },
                 new OffloadRatesDTO
                {
                    Id = 1,
                    HourlyRate = 20,
                    Percent = 0.50m,
                    PerformingOrg = this.Perforg.PerformingOrgName,
                    Resource = this.Resource.ResourceName,
                    SubResource = subResource.ResourceName,
                    Year = 2009
                },
                 new OffloadRatesDTO
                {
                    Id = 1,
                    HourlyRate = 20,
                    Percent = 0.50m,
                    PerformingOrg = this.Perforg.PerformingOrgName,
                    Resource = this.Resource.ResourceName,
                    SubResource = subResource.ResourceName,
                    Year = 2010
                },
                new OffloadRatesDTO
                {
                    Id = 1,
                    HourlyRate = 20,
                    Percent = 0.50m,
                    PerformingOrg = this.Perforg.PerformingOrgName,
                    Resource = this.Resource.ResourceName,
                    SubResource = subResource.ResourceName,
                    Year = 2010
                },
                new OffloadRatesDTO
                {
                    Id = 1,
                    HourlyRate = 20,
                    Percent = 0.50m,
                    PerformingOrg = this.Perforg.PerformingOrgName,
                    Resource = this.Resource.ResourceName,
                    SubResource = subResource.ResourceName,
                    Year = 2011
                }
            };
            offloadRatesLoader.Setup(x => x.GetByWorkspaceId(workspace.Id)).Returns(offloadRates);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IOffloadRatesDTOLoader), offloadRatesLoader.Object);

            ProPricerExporter expReport = new ProPricerExporter(TripCalculate.Object, rmsTripCalculate.Object, this.retriever.Object, this.commonDataMapper.Object);

            ProPricerDTO proPricerExport = SetUpProPricerDTO(this.Workspace.Id);

            proPricerExport.ProPricerTasks.Where(t => t.CustomFieldID.HasValue).ToList().ForEach(pt => pt.Task = ProPricerField_Task.BLANK);
            proPricerExport.ProPricerTasks.Where(t => t.CustomFieldID.HasValue).ToList().ForEach(pt => pt.CustomFieldID = null);
            proPricerExport.ProPricerResources.Where(t => t.CustomFieldID.HasValue).ToList().ForEach(pt => pt.Resource = ProPricerField_Resources.BLANK);
            proPricerExport.ProPricerResources.Where(t => t.CustomFieldID.HasValue).ToList().ForEach(pt => pt.CustomFieldID = null);

            int maxListOrder = Enumerable.Max(Enumerable.Concat(Enumerable.Select(proPricerExport.ProPricerTasks, task => task.ListOrder), new[] { 0 }));

            proPricerExport.ProPricerTasks.Add(new ProPricerTasks
            {
                Task = ProPricerField_Task.ProjMapClassOfCost,
                ListOrder = maxListOrder + 1
            });

            PpDataReadyForExport result = expReport.ExportProPricer(proPricerExport, workspace);
            Collection<string> TaskData = result.TaskData.ToCollection();
            Collection<string> ResourceCost = result.ResourceData.ToCollection();
            Assert.IsTrue(TaskData != null, "Task Data is null");
            Assert.IsTrue(ResourceCost != null, "Resource Cost data is null");

            Assert.IsTrue(result.TaskData.Any(), "Task Data is empty");
            Assert.IsTrue(result.ResourceData.Any(), "Resource Cost data is empty");
            Assert.IsTrue(result.OffloadTaskData.Any(), "Offload Task Data is empty");
            Assert.IsTrue(result.OffloadResourceData.Any(), "Offload Resource Cost data is empty");

            return result;
        }

        public FullBoe CreateFullBoe()
        {
            FullBoe boe = new FullBoe();
            boe.SetTaskElements(new Collection<BoeTaskElementDTO>());
            return boe;
        }

        /// <summary>
        /// Test export for Workspace using Template BOE
        /// MOQ Type should show "Multiple" when a task has multiple MOQ Types
        /// This test based on BL_ExportProPricer_LaborElementsOnly
        /// </summary>
        [TestMethod, System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public void BL_ExportProPricer_TemplateBoe()
        {
            Collection<BoeDTO> toReturn = new Collection<BoeDTO>();
            toReturn.Add(this.Boe1);

            CustomFieldDTO customField1 = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.TaskDisplay };
            CustomFieldDTO customField2 = new CustomFieldDTO { Id = 2, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay };

            CustomFieldValueDTO cv = new CustomFieldValueDTO();
            cv.CustomFieldID = customField1.Id;
            cv.CustomFieldValueID = 1;
            cv.CustomFieldValueName = "P<br/><BR /><bR/>E";
            cv.CustomFieldValueDescription = "an exercise<BR /> class";

            CustomFieldValueDTO cv2 = new CustomFieldValueDTO();
            cv2.CustomFieldID = customField2.Id;
            cv2.CustomFieldValueID = 2;
            cv2.CustomFieldValueName = "History";
            cv2.CustomFieldValueDescription = "where you<br /><BR/><BR><br><br / > learn past events";

            DateTime LaborstartDate = Convert.ToDateTime("11/01/2010");
            DateTime LaborendDate = Convert.ToDateTime("01/01/2011");

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsDataLoader.Object);

            FullWorkspace workspace = new FullWorkspace(this.Workspace);

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullBoe>() { new FullBoe(this.Boe1), new FullBoe(this.Boe2) });
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullWbs>() { new FullWbs(this.Wbs) });
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullClin>() { new FullClin(this.Clin1) });
            this.retriever.Setup(x => x.GetClinById(this.Boe1.CLINID.Value)).Returns(this.Clin1);
            this.retriever.Setup(x => x.GetWbsById(this.Wbs.Id)).Returns(this.Wbs);

            ICollection<MoqTypeSelection> moqTypes = new Collection<MoqTypeSelection>()
            {
                new MoqTypeSelection() { TaskId = 1, SelectedMOQType = MOQType.SOW },
                new MoqTypeSelection() { TaskId = 2, SelectedMOQType = MOQType.LOE },
                new MoqTypeSelection() { TaskId = 2, SelectedMOQType = MOQType.SME }
            };
            this.retriever.Setup(x => x.GetMoqTypeSelectionsByWorkspaceId(It.IsAny<int>())).Returns(moqTypes);
            workspace.UsingTemplateBOE = true;

            // setup resources dto mapper
            Collection<ResourceDTO> resources = new Collection<ResourceDTO>();
            resources.Add(this.Resource);

            this.retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new List<CustomFieldValueDTO>() { cv, cv2 });
            this.retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>())).Returns(new List<PerformingOrgDTO>() { this.Perforg });
            this.retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(resources);
            this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(resources);
            this.retriever.Setup(x => x.GetFullWorkspaceById(workspace.Id)).Returns(workspace);
            this.factory.Setup(x => x.CreateFullWorkspace(It.IsAny<int>())).Returns(workspace);
            this.factory.Setup(x => x.CreateFullBoes(It.IsAny<ICollection<int>>())).Returns(new List<FullBoe>() { new FullBoe(this.Boe1) });
            foreach (FullBoe boe in workspace.Boes)
            {
                boe.Title = "test boe";
            }

            BoeTaskElementDTO task1 = new BoeTaskElementDTO
            {
                BoeID = this.Boe1.Id,
                Id = 1,
                BOETaskID = "123",
                TaskTitle = "MOCK TASK1",
                Description = "MOCK TASK1",
                StartDate = this.Boe1.StartDate,
                EndDate = this.Boe1.EndDate,
                IMS_ID = "500",
                MOQType = MOQType.Comparison,
                CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { new CustomFieldValueContainer { CustomFieldValueID = cv.CustomFieldValueID } },
                taskElementLabors = new Collection<ResourceTypeDto>{new ResourceTypeDto{Id=1, SpreadType = IES.Common.SpreadType.Hours, StartDateValue=LaborstartDate, EndDateValue=LaborendDate, ResourceID=this.Resource.Id, PerformingOrgID=this.Perforg.Id,SpreadCurveID = SpreadCurves.DiscreteHours,
                            LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=1, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=200 },
                            //discrete values of 0 are not stored in the db so replicate that here by removing a month
                            new ResourceSpreadDto{Id=3, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=200 }}
                        }}
            };
            //Two resources for this task to test multiple rows for tasks with multiple resources
            BoeTaskElementDTO task2 = new BoeTaskElementDTO
            {
                BoeID = this.Boe2.Id,
                Id = 2,
                BOETaskID = "1234",
                TaskTitle = "MOCK TASK2",
                Description = "MOCK TASK2",
                StartDate = this.Boe1.StartDate,
                EndDate = this.Boe1.EndDate,
                MOQType = MOQType.Factor,
                taskElementLabors = new Collection<ResourceTypeDto>{new ResourceTypeDto{Id=2, SpreadType = IES.Common.SpreadType.Hours, StartDateValue=LaborstartDate, EndDateValue=LaborendDate, ResourceID=this.Resource.Id, PerformingOrgID=this.Perforg.Id,  SpreadCurveID = SpreadCurves.DiscreteHours, CustomFieldValueContainers=new Collection<CustomFieldValueContainer>{new CustomFieldValueContainer{CustomFieldValueID=cv2.CustomFieldValueID}},
                            LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=4, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=150 },
                            new ResourceSpreadDto{Id=5, LaborSpreadDate=Convert.ToDateTime("12/01/2010"), LaborSpreadValue=150 },
                            new ResourceSpreadDto{Id=6, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=150 }}
                            },new ResourceTypeDto{Id=3, SpreadType = IES.Common.SpreadType.Hours, StartDateValue=LaborstartDate, EndDateValue=LaborendDate, ResourceID=this.Resource.Id, PerformingOrgID=this.Perforg.Id,  SpreadCurveID = SpreadCurves.DiscreteHours, CustomFieldValueContainers=new Collection<CustomFieldValueContainer>{new CustomFieldValueContainer{CustomFieldValueID=cv2.CustomFieldValueID}},
                            LaborSpreads=new Collection<ResourceSpreadDto>{new ResourceSpreadDto{Id=4, LaborSpreadDate=Convert.ToDateTime("11/01/2010"), LaborSpreadValue=50 },
                            new ResourceSpreadDto{Id=5, LaborSpreadDate=Convert.ToDateTime("12/01/2010"), LaborSpreadValue=150 },
                            new ResourceSpreadDto{Id=6, LaborSpreadDate=Convert.ToDateTime("01/01/2011"), LaborSpreadValue=250 }}
                            }}
            };

            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { task1, task2 });
            this.retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, false)).Returns(new Collection<TravelDTO> { });
            this.retriever.Setup(x => x.GetMaterialsByBoeIds(It.IsAny<Collection<int>>(), false)).Returns(new Collection<MaterialDTO> { });
            this.retriever.Setup(x => x.GetOdcCollectionByBoeIds(It.IsAny<List<int>>(), false)).Returns(new Collection<OtherDirectCostDTO> { });

            retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<CustomFieldDTO> { customField1, customField2 });

            var TripCalculate = new Mock<TravelTripCostCalculation>();
            var rmsTripCalculate = new Mock<RMSZoneTravelRatesFeesDataLoader>();
            rmsTripCalculate.Setup(x => x.getAllFeesAndCostsByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSTravelNonzoneFeesAndCostsDTO>());
            rmsTripCalculate.Setup(x => x.getAllEscalationRatesByWorkspace(workspace.Id)).Returns(new Collection<WorkspaceRMSEscalationRatesDTO>());

            ProPricerExporter expReport = new ProPricerExporter(TripCalculate.Object, rmsTripCalculate.Object, this.retriever.Object, this.commonDataMapper.Object);

            ProPricerDTO proPricerExport = SetUpProPricerDTO(this.Workspace.Id);

            PpDataReadyForExport result = expReport.ExportProPricer(proPricerExport, workspace);
            Collection<string> TaskData = result.TaskData.ToCollection();
            Collection<string> ResourceCost = result.ResourceData.ToCollection();

            Assert.IsTrue(TaskData != null, "Task Data is not null");
            Assert.IsTrue(ResourceCost != null, "Resource Cost data is not null");

            //  there is no other way to verify the results other than hard coding string values
            Assert.AreEqual("112005,,TOTAL,1.1,012006,LIDN000001,\"PE\",\"MOCK TASK1\",\"mock clin1 title\",\"mock wbs title\",2.2,\"an exercise class\",,,\"BBBBBBB\",\"KristinePO\",\"Statement of Work (SOW)\",20,1,1,\"test boe\",\"123\",", TaskData[0], "The first task did not match this value");
            Assert.AreEqual("112005,,TOTAL,,012006,LIDN000002,,\"MOCK TASK2\",\"\",\"mock wbs title\",2.2,,\"where you learn past events\",\"History\",\"BBBBBBB\",\"KristinePO\",\"Multiple\",21,2,2,\"test boe\",\"1234\",", TaskData[1], "The second task (first resource) did not match this value");
            Assert.AreEqual("112005,,TOTAL,,012006,LIDN000003,,\"MOCK TASK2\",\"\",\"mock wbs title\",2.2,,\"where you learn past events\",\"History\",\"BBBBBBB\",\"KristinePO\",\"Multiple\",21,2,3,\"test boe\",\"1234\",", TaskData[2], "The second task (second resource) did not match this value");

            Assert.AreEqual("500,\"BBBBBBB\",LIDN000001,2.2,,,,D,1.1,\"mock clin1 title\",112010,\"mock wbs title\",20,1,1,\"test boe\",\"KristinePO\",\"MOCK TASK1\",\"123\",200,0,200,", ResourceCost[0], "The first resource did not match this value");
            Assert.AreEqual(",\"BBBBBBB\",LIDN000002,2.2,\"where you learn past events\",\"History\",,D,,\"\",112010,\"mock wbs title\",21,2,2,\"test boe\",\"KristinePO\",\"MOCK TASK2\",\"1234\",150,150,150,", ResourceCost[1], "The second resource did not match this value");
            Assert.AreEqual(",\"BBBBBBB\",LIDN000003,2.2,\"where you learn past events\",\"History\",,D,,\"\",112010,\"mock wbs title\",21,2,3,\"test boe\",\"KristinePO\",\"MOCK TASK2\",\"1234\",50,150,250,", ResourceCost[2], "The third resource did not match this value");
        }
    }
}