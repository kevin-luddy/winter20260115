// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.IO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.IO.Import;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.ModelView.Admin;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test class for Training Importer
    /// </summary>
    [TestClass]
    public class TrainingImporterTests
    {
        [TestMethod]
        public void Test_TrainingImport()
        {
            //Current directory the word template is saved and the name of the word template
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GenBOE.Tests.Resources.TINATraining.xlsx");

            AdminControllerLogicSpaceSystems sut = new AdminControllerLogicSpaceSystems(null, null, null);

            ICollection<string> courseIds = sut.GetTrainingCourseGroups().SelectMany(g => g.Courses).Select(c => c.CourseID).ToList();

            ICollection<TrainingModelView> trainings = new TrainingImporter().ImportFromExcelFile(stream, courseIds);

            Assert.IsNotNull(trainings);
            Assert.IsTrue(trainings.Count > 0);
            Assert.AreEqual(1, trainings.Count(t => t.UserId == "111034" && t.CourseId == Constants.TINA_TRAINING_COURSEID && t.UserDisplayName == "Zaremski, Dennis R" && t.LastCompleted == DateTime.Parse("6/4/2018")));           

            ICollection<TrainingModelView> overdue = sut.GetOverdueTraining(new ActiveDirectoryUtilities(), trainings, "TINA Training");
            Assert.IsNotNull(overdue);
            Assert.IsTrue(overdue.Any());

            ICollection<TrainingModelView> missing = sut.GetOverdueTraining(new ActiveDirectoryUtilities(), trainings, "BOE Writing Course");
            Assert.IsNotNull(missing);
            // the original training code
            Assert.IsFalse(missing.Any(t => t.CourseId == Constants.SHARED_BOE_WRITING_COURSE && t.UserId == "111034"));
            // the 2nd training code (BOEJ-4134)
            Assert.IsFalse(missing.Any(t => t.CourseId == Constants.SHARED_BOE_WRITING_COURSE && t.UserId == "017144"));
            Assert.IsTrue(missing.Any(t => t.CourseId == Constants.SHARED_BOE_WRITING_COURSE && t.UserDisplayName == "Felicioni, Frank M (US)" && t.NTID == "ffelicio"));
        }

        [TestMethod]
        public void Test_SSCTraining()
        {
            AdminControllerLogicSpaceSystems sut = new AdminControllerLogicSpaceSystems(null, null, null);
            ICollection<TrainingCourseGroupModelView> groups =  sut.GetTrainingCourseGroups();

            Assert.AreEqual(3, groups.Count);

            // check TINA
            TrainingCourseGroupModelView tina = groups.First(g => g.CourseGroupName.Contains("TINA"));
            Assert.AreEqual(1, tina.Courses.Count);
            Assert.AreEqual(Constants.TINA_TRAINING_COURSEID, tina.Courses.First().CourseID);

            // check subcontractor
            TrainingCourseGroupModelView sub = groups.First(g => g.CourseGroupName.Contains("Sub"));
            Assert.AreEqual(1, sub.Courses.Count);
            Assert.AreEqual(Constants.SUBCONTRACT_TRAINING_COURSEID, sub.Courses.First().CourseID);

            // check boe
            TrainingCourseGroupModelView boe = groups.First(g => g.CourseGroupName.Contains("BOE"));
            Assert.AreEqual(3, boe.Courses.Count);
            Assert.IsTrue(boe.Courses.Select(c => c.CourseID).Contains(Constants.SHARED_BOE_WRITING_COURSE));
            Assert.IsTrue(boe.Courses.Select(c => c.CourseID).Contains(Constants.SSC_OLD_BOE_WRITING_COURSE));
            Assert.IsTrue(boe.Courses.Select(c => c.CourseID).Contains(Constants.SSC_OLD_BOE_WRITING_COURSE2));
        }

        [TestMethod]
        public void Test_RMSTraining()
        {
            AdminControllerLogicMST sut = new AdminControllerLogicMST(null, null, null, null, null, null);
            ICollection<TrainingCourseGroupModelView> groups = sut.GetTrainingCourseGroups();

            Assert.AreEqual(2, groups.Count);

            // check TINA
            TrainingCourseGroupModelView tina = groups.First(g => g.CourseGroupName.Contains("TINA"));
            Assert.AreEqual(1, tina.Courses.Count);
            Assert.AreEqual(Constants.TINA_TRAINING_COURSEID, tina.Courses.First().CourseID);

            // check boe
            TrainingCourseGroupModelView boe = groups.First(g => g.CourseGroupName.Contains("BOE"));
            Assert.AreEqual(1, boe.Courses.Count);
            Assert.IsTrue(boe.Courses.Select(c => c.CourseID).Contains(Constants.SHARED_BOE_WRITING_COURSE));
        }

        [TestMethod]
        public void TestOverdueTrainingLogic()
        {
            AdminControllerLogicTest sut = new AdminControllerLogicTest();
            ICollection<TrainingCourseGroupModelView> groups = sut.GetTrainingCourseGroups();

            // Check TINA
            TrainingCourseGroupModelView tina = groups.First(g => g.CourseGroupName.Contains("TINA"));
            ICollection<TrainingModelView> models = GetModels();
            ICollection<TrainingModelView> overdue = sut.GetOverdueTraining(new ADTest(), models, tina.CourseGroupName);
            // There should be 1 overdue, and 1 missing
            Assert.AreEqual(2, overdue.Count);
            TrainingModelView expired = overdue.FirstOrDefault(o => o.CourseId == Constants.TINA_TRAINING_COURSEID && o.UserDisplayName == ExpiredGuy && o.NTID == ExpiredGuy.ToLower());
            TrainingModelView missing = overdue.FirstOrDefault(o => o.CourseId == Constants.TINA_TRAINING_COURSEID && o.UserDisplayName == MissingGuy && o.NTID == MissingGuy.ToLower());
            Assert.IsNotNull(expired);
            Assert.IsNotNull(missing);
            Assert.AreEqual(ExpiredTime, expired.LastCompleted);
            Assert.IsNull(missing.LastCompleted);
            
            // Check BOE
            TrainingCourseGroupModelView boe = groups.First(g => g.CourseGroupName.Contains("BOE"));
            overdue = sut.GetOverdueTraining(new ADTest(), models, boe.CourseGroupName);
            // There should be 1 overdue, and 1 missing, and 1 invalid
            Assert.AreEqual(3, overdue.Count);
            expired = overdue.FirstOrDefault(o => o.CourseId == Constants.SHARED_BOE_WRITING_COURSE && o.UserDisplayName == ExpiredGuy && o.NTID == ExpiredGuy.ToLower());
            missing = overdue.FirstOrDefault(o => o.CourseId == Constants.SHARED_BOE_WRITING_COURSE && o.UserDisplayName == MissingGuy && o.NTID == MissingGuy.ToLower());
            TrainingModelView invalid = overdue.FirstOrDefault(o => o.CourseId == Constants.SSC_OLD_BOE_WRITING_COURSE && o.UserDisplayName == InvalidGuy);
            Assert.IsNotNull(expired);
            Assert.IsNotNull(missing);
            Assert.IsNotNull(invalid);
            Assert.AreEqual(ExpiredTime, expired.LastCompleted);
            Assert.IsNull(missing.LastCompleted);
            Assert.AreEqual(InvalidTime, invalid.LastCompleted);

            // check subcontractor
            TrainingCourseGroupModelView sub = groups.First(g => g.CourseGroupName.Contains("Sub"));
            overdue = sut.GetOverdueTraining(new ADTest(), models, sub.CourseGroupName);
            // There should be 1 missing
            Assert.AreEqual(1, overdue.Count);
            missing = overdue.FirstOrDefault(o => o.CourseId == Constants.SUBCONTRACT_TRAINING_COURSEID && o.UserDisplayName == MissingGuy && o.NTID == MissingGuy.ToLower());
            Assert.IsNotNull(missing);
            Assert.IsNull(missing.LastCompleted);
        }

        public const int GoodUserId = 5;
        public const int GoodUserId2 = 8;
        public const int GoodUserId3 = 9;
        public const int GoodUserId4 = 44;
        public const int ExpiredUserId = 6;
        public const int MissingUserId = 7;
        public const int InvalidUserId = 10;
        public DateTime ExpiredTime = DateTime.Now.AddYears(-4);
        public DateTime GoodTime = new DateTime(2019, 5, 1);
        public DateTime InvalidTime = new DateTime(2019, 8, 1);
        public const string GoodGuy = "Good guy1";
        public const string GoodGuy2 = "Good guy2";
        public const string GoodGuy3 = "Good guy3";
        public const string GoodGuy4 = "Good guy4";
        public const string InvalidGuy = "Invalid guy";
        public const string ExpiredGuy = "Expired guy";
        public const string MissingGuy = "Missing Guy";


        private ICollection<TrainingModelView> GetModels()
        {
            return new List<TrainingModelView>
            {
                new TrainingModelView
                {
                    CourseId = Constants.TINA_TRAINING_COURSEID,
                    UserDisplayName = GoodGuy,
                    UserId = GoodUserId.ToString(),
                    LastCompleted = DateTime.Now
                },
                new TrainingModelView
                {
                    CourseId = Constants.TINA_TRAINING_COURSEID,
                    UserDisplayName = GoodGuy2,
                    UserId = GoodUserId2.ToString(),
                    LastCompleted = DateTime.Now
                },
                new TrainingModelView
                {
                    CourseId = Constants.TINA_TRAINING_COURSEID,
                    UserDisplayName = GoodGuy3,
                    UserId = GoodUserId3.ToString(),
                    LastCompleted = DateTime.Now
                },
                new TrainingModelView
                {
                    CourseId = Constants.SSC_OLD_BOE_WRITING_COURSE2,
                    UserDisplayName = GoodGuy4,
                    UserId = GoodUserId4.ToString(),
                    LastCompleted = InvalidTime   // good guy 4 has one invalid and one good time, so should be good
                },
                new TrainingModelView
                {
                    CourseId = Constants.SSC_OLD_BOE_WRITING_COURSE2,
                    UserDisplayName = GoodGuy4,
                    UserId = GoodUserId4.ToString(),
                    LastCompleted = GoodTime
                },
                new TrainingModelView
                {
                    CourseId = Constants.TINA_TRAINING_COURSEID,
                    UserDisplayName = InvalidGuy,  // only invalid for BOE
                    UserId = InvalidUserId.ToString(),
                    LastCompleted = DateTime.Now
                },
                new TrainingModelView
                {
                    CourseId = Constants.TINA_TRAINING_COURSEID,
                    UserDisplayName = ExpiredGuy,
                    UserId = ExpiredUserId.ToString(),
                    LastCompleted = ExpiredTime
                },
                new TrainingModelView
                {
                    CourseId = Constants.SHARED_BOE_WRITING_COURSE,
                    UserDisplayName = GoodGuy,
                    UserId = GoodUserId.ToString(),
                    LastCompleted = DateTime.Now
                },
                new TrainingModelView
                {
                    CourseId = Constants.SSC_OLD_BOE_WRITING_COURSE,
                    UserDisplayName = GoodGuy2,
                    UserId = GoodUserId2.ToString(),
                    LastCompleted = GoodTime
                },
                new TrainingModelView
                {
                    CourseId = Constants.SSC_OLD_BOE_WRITING_COURSE2,
                    UserDisplayName = GoodGuy3,
                    UserId = GoodUserId3.ToString(),
                    LastCompleted = GoodTime
                },
                new TrainingModelView
                {
                    CourseId = Constants.SSC_OLD_BOE_WRITING_COURSE,
                    UserDisplayName = InvalidGuy,
                    UserId = InvalidUserId.ToString(),
                    LastCompleted = InvalidTime
                },
                new TrainingModelView
                {
                    CourseId = Constants.SSC_OLD_BOE_WRITING_COURSE,
                    UserDisplayName = ExpiredGuy,
                    UserId = ExpiredUserId.ToString(),
                    LastCompleted = ExpiredTime
                },
                new TrainingModelView
                {
                    CourseId = Constants.SUBCONTRACT_TRAINING_COURSEID,
                    UserDisplayName = "GOOD guy",
                    UserId = GoodUserId.ToString(),
                    LastCompleted = DateTime.Now
                }
            };
        }
    }

    public class AdminControllerLogicTest : AdminControllerLogicSpaceSystems
    {
        public const string TRAINING_COURSE_1 = "Training 1";
        public const string TRAINING_COURSE_2 = "Training 2";

        public AdminControllerLogicTest() : base(null, null, new PermissionsTest())
        { }

    }

    public class PermissionsTest : IPermissionsDTODataLoader
    {
        public Collection<PermissionsDTO> GetAdminPermissions()
        {
            throw new NotImplementedException();
        }

        public Collection<PermissionsDTO> GetAllBOEPermissions()
        {
            throw new NotImplementedException();
        }

        public Collection<PermissionsDTO> GetAllBOEPotentialPermissions()
        {
            throw new NotImplementedException();
        }

        public ICollection<string> GetAllNtIdsForWorkspacePermission(Role role)
        {
            return new string[] { TrainingImporterTests.GoodGuy, TrainingImporterTests.MissingGuy };
        }

        public Collection<PermissionsDTO> GetBOEPermissions(ICollection<int> inBOEIds)
        {
            throw new NotImplementedException();
        }

        public Collection<PermissionsDTO> GetBOEPotentialPermissionsForWorkspace(int inWorkspaceId)
        {
            throw new NotImplementedException();
        }

        public Collection<PermissionsDTO> GetCreateWorkspacePermissions()
        {
            throw new NotImplementedException();
        }

        public ICollection<KeyValuePair<string, string>> GetCreateWorkspaceRolesForPtm(string selectedNtid, string selectedUser)
        {
            throw new NotImplementedException();
        }

        public Collection<PermissionsDTO> GetPermissionsForGridData(int wsId)
        {
            throw new NotImplementedException();
        }

        public Collection<PermissionsDTO> GetUserPermissions(UserDTO User)
        {
            throw new NotImplementedException();
        }

        public Collection<int> GetWorkspaceIdsThatUsersHaveAccessTo(ICollection<int> userIds)
        {
            throw new NotImplementedException();
        }

        public Collection<int> GetWorkspaceIdsWhereUserIsWorkspaceAdmin(ICollection<int> userIds)
        {
            throw new NotImplementedException();
        }

        public Collection<PermissionsDTO> GetWorkspacePermissions(int inWorkspaceId)
        {
            throw new NotImplementedException();
        }

        public Collection<PermissionsDTO> GetWorkspacePermissionsByBoeID(int inBoeId)
        {
            throw new NotImplementedException();
        }

        public void SavePermission(PermissionsDTO dtoToSave)
        {
            throw new NotImplementedException();
        }

        public void SavePermissions(Collection<PermissionsDTO> dtosToSave)
        {
            throw new NotImplementedException();
        }

        public void SaveWorkspaceHideHelp(PermissionsDTO inPermission)
        {
            throw new NotImplementedException();
        }
    }

    public class ADTest : IActiveDirectoryUtilities
    {
        public Dictionary<UserData, bool> CheckUsersBoeAccess(ICollection<UserData> usersToCheck, ICollection<GroupData> groupsToCheckAgainst)
        {
            throw new NotImplementedException();
        }

        public ICollection<UserData> GetAdGroupUsers(string inGroupName)
        {
            return new List<UserData>() {
                new UserData(){
                    Ntid = TrainingImporterTests.GoodGuy,
                    DisplayName = TrainingImporterTests.GoodGuy,
                    EmployeeId = TrainingImporterTests.GoodUserId.ToString()
                },
                new UserData(){
                    Ntid = TrainingImporterTests.GoodGuy2,
                    DisplayName = TrainingImporterTests.GoodGuy2,
                    EmployeeId = TrainingImporterTests.GoodUserId2.ToString()
                },
                new UserData(){
                    Ntid = TrainingImporterTests.GoodGuy3,
                    DisplayName = TrainingImporterTests.GoodGuy3,
                    EmployeeId = TrainingImporterTests.GoodUserId3.ToString()
                },
                new UserData(){
                    Ntid = TrainingImporterTests.MissingGuy,
                    DisplayName = TrainingImporterTests.MissingGuy,
                    EmployeeId = TrainingImporterTests.MissingUserId.ToString()
                },
                new UserData(){
                    Ntid = TrainingImporterTests.ExpiredGuy,
                    DisplayName = TrainingImporterTests.ExpiredGuy,
                    EmployeeId = TrainingImporterTests.ExpiredUserId.ToString()
                },
                new UserData(){
                    Ntid = TrainingImporterTests.InvalidGuy,
                    DisplayName = TrainingImporterTests.InvalidGuy,
                    EmployeeId = TrainingImporterTests.InvalidUserId.ToString()
                }
            };
        }

        public ICollection<GroupData> GetAuthorizationGroupsFromWebConfig()
        {
            return new List<GroupData> { new GroupData { Ntid = "bleh" } };
        }

        public ICollection<GroupData> GetGroupsForUser(string inNtid)
        {
            throw new NotImplementedException();
        }

        public string GetUserAndGroupIdsAsXml(string ntid, ICollection<GroupData> groups)
        {
            throw new NotImplementedException();
        }

        public UserData GetUserByQualifiedAccount(string inNtid, bool isGroup)
        {
            throw new NotImplementedException();
        }

        public bool IsGroup(string inNtID)
        {
            throw new NotImplementedException();
        }

        public bool IsMemberOfADGroup(string inUserName, string inGroupName)
        {
            throw new NotImplementedException();
        }

        public bool IsValidADGroup(string inGroupName)
        {
            throw new NotImplementedException();
        }

        public ICollection<UserData> SearchUsers(string userSearchString, ActiveDirectorySearchBy searchBy, ActiveDirectoryMatchType matchBy)
        {
            throw new NotImplementedException();
        }
    }
}
