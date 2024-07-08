// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Tests.Core
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Transactions;
	using ActionLogic.Core.ControllerLogic;
	using ActionLogic.Core.Mediator;
	using DataBridge.Loaders;
	using DataBridge.ModelViews;
	using IES.Common.Core;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Exceptions;
	using IES.Common.Core.Models;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.Logging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;

	/// <summary>
	/// Test Class for Revision Loader
	/// </summary>
	[TestClass]
	public class RevisionLoaderTest
	{
		/// <summary>
		/// Handle to the test data
		/// </summary>
		private TestData testData = TestData.GetInstance();

		/// <summary>
		/// Test bad Revision Id.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void TestRevisionLoader_EX1()
		{
			IRevisionMediator sut = this.testData.RevisionMediator;
			sut.GetById(-99);
		}

		/// <summary>
		/// Test GetPriorRevision with null revisions list.
		/// </summary>
		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public void TestGetPriorRevision_EX1()
		{
			IRevisionMediator sut = this.testData.RevisionMediator;
			sut.GetPriorRevision(null, -99);
		}

		/// <summary>
		/// Test GetPriorRevision with empty revisions list.
		/// </summary>
		[TestMethod]
		public void TestGetPriorRevision_EmptyRevisionsList()
		{
			IRevisionMediator sut = this.testData.RevisionMediator;
			IList<RevisionModelView> revisions = new List<RevisionModelView>();
			RevisionModelView priorRevision = sut.GetPriorRevision(revisions, -99);
			Assert.IsNull(priorRevision);
		}

		/// <summary>
		/// Test GetPriorRevision with no prior revision.
		/// </summary>
		[TestMethod]
		public void TestGetPriorRevision_NoPriorRevision()
		{
			IRevisionMediator sut = this.testData.RevisionMediator;
			RevisionModelView item1 = this.testData.GetRevision(false);
			IList<RevisionModelView> revisions = new List<RevisionModelView> { item1 };
			RevisionModelView priorRevision = sut.GetPriorRevision(revisions, item1.Id);
			Assert.IsNull(priorRevision);
		}

		/// <summary>
		/// Test GetPriorRevision happy path.
		/// </summary>
		[TestMethod]
		public void TestGetPriorRevision_Success()
		{
			IRevisionMediator sut = this.testData.RevisionMediator;
			RevisionModelView item1 = this.testData.GetRevision(true);
			RevisionModelView item2 = this.testData.GetRevision(true);
			IList<RevisionModelView> revisions = new List<RevisionModelView> { item1, item2 };
			RevisionModelView priorRevision = sut.GetPriorRevision(revisions, item2.Id);
			Assert.AreEqual(item1.Id, priorRevision.Id);
		}

		/// <summary>
		/// TestVersionCompare
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[TestMethod]
		[Ignore("Original test already failing. Ignore until this one is fixed.")]
		public void TestVersionCompare()
		{
			IRevisionMediator sut = this.testData.RevisionMediator;

			// insert
			RevisionModelView item1 = this.testData.GetRevision(true);

			// publish
			RevisionModelView item2 = this.testData.PublishRevision(item1);

			ICollection<VersionComparisonGridRowModelView> results = sut.GetVersionComparisonRows(item1.Id, item2.Id);

			Assert.AreEqual(0, results.Count);  // no changes expected (publish without changes to WIP)

			// Add data (i.e. sections, rates, burden pools, etc.)
			this.testData.ModifyRevisionData1(item2);

			// publish
			RevisionModelView item3 = this.testData.PublishRevision(item2);

			// verify the GetLastPublishedRevision and GetWipRevision methods
			item1 = sut.GetById(item1.Id);
			item2 = sut.GetById(item2.Id);
			item3 = sut.GetById(item3.Id);
			IList<RevisionModelView> revisions = new List<RevisionModelView> { item1, item2, item3 };

			// Modify data in the new revision
			this.testData.ModifyRevisionData2(item3);

			// Test version comparison results
			results = sut.GetVersionComparisonRows(item2.Id, item3.Id);
			Assert.AreEqual(12, results.Count);
			// Note: MockBurdenPool1 was changed to MockBurdenPool1a.  This generates a delete and an add.
			Assert.AreEqual(1, results.Count(x => x.ChangeType == RDMChangeType.Delete && x.FieldChanged.Equals("Burden Pool") && x.OldValue.Equals("MockBurdenPool1")));
			Assert.AreEqual(1, results.Count(x => x.ChangeType == RDMChangeType.Add && x.FieldChanged.Equals("Burden Pool") && x.NewValue.Equals("MockBurdenPool1a")));
			Assert.AreEqual(1, results.Count(x => x.ChangeType == RDMChangeType.Edit && x.FieldChanged.Equals("Burden Pool") && x.NewValue.Equals("Mock Burden Pool 2a")));
			Assert.AreEqual(1, results.Count(x => x.ChangeType == RDMChangeType.Delete && x.FieldChanged.Equals("Burden Pool") && x.OldValue.Equals("MockBurdenPool3")));
			Assert.AreEqual(1, results.Count(x => x.ChangeType == RDMChangeType.Edit && x.FieldChanged.Equals("Section Content") && x.NewValue.Equals("This section was moved to the beginning.")));
			Assert.AreEqual(1, results.Count(x => x.ChangeType == RDMChangeType.Edit && x.FieldChanged.Equals("Section Content") && x.NewValue.Equals("This section was moved to the end.")));
			Assert.AreEqual(1, results.Count(x => x.ChangeType == RDMChangeType.Edit && x.FieldChanged.Equals("Section Title") && x.NewValue.Equals("Oversight Agencies (modified - added a Rate Table and text content)")));
			Assert.AreEqual(1, results.Count(x => x.ChangeType == RDMChangeType.Edit && x.FieldChanged.Equals("Section Title") && x.NewValue.Equals("Production Overhead (modified)")));
			Assert.AreEqual(1, results.Count(x => x.ChangeType == RDMChangeType.Delete && x.FieldChanged.Equals("Section") && x.OldValue.Equals("<p>2017-2019 rates reference FPRA-16-04, dated July 8, 2016</p>")));
			Assert.AreEqual(1, results.Count(x => x.ChangeType == RDMChangeType.Add && x.FieldChanged.Equals("Section") && x.NewValue.Equals("Added text content blah blah blah...")));
			Assert.AreEqual(1, results.Count(x => x.ChangeType == RDMChangeType.Add && x.FieldChanged.Equals("Section") && x.NewValue.Equals("New Section")));
			Assert.AreEqual(1, results.Count(x => x.ChangeType == RDMChangeType.Add && x.FieldChanged.Equals("Section") && x.NewValue.Equals("New SubSection")));

			ICollection<RevisionOptionModelView> revisionOptions = sut.GetRevisionOptions(revisions);
			ICollection<RateDetailModelView> rateDetails = this.testData.RateDetailLoader.GetComparableRates(revisionOptions.Single(x => x.Id == item3.Id), revisionOptions.Single(x => x.Id == item2.Id));
			Assert.AreEqual(7, rateDetails.Count);
			Assert.AreEqual(1, rateDetails.Count(x => x.RateCode.Equals("MockRate1") && x.CompareState == RateCompareState.Edit.GetDescription()));
			Assert.AreEqual(1, rateDetails.Count(x => x.RateCode.Equals("MockRate1") && x.CompareState == null));
			Assert.AreEqual(1, rateDetails.Count(x => x.RateCode.Equals("MockRate2") && x.CompareState == RateCompareState.Edit.GetDescription()));
			Assert.AreEqual(1, rateDetails.Count(x => x.RateCode.Equals("MockRate2") && x.CompareState == null));
			Assert.AreEqual(1, rateDetails.Count(x => x.RateCode.Equals("MockRate4") && x.CompareState == RateCompareState.Edit.GetDescription()));
			Assert.AreEqual(1, rateDetails.Count(x => x.RateCode.Equals("MockRate4") && x.CompareState == null));
			Assert.AreEqual(1, rateDetails.Count(x => x.RateCode.Equals("MockRate5") && x.CompareState == RateCompareState.Added.GetDescription()));

			// Modify data in the new revision
			this.testData.ModifyRevisionData3(item3);

			// Test version comparison results
			results = sut.GetVersionComparisonRows(item2.Id, item3.Id);
			Assert.AreEqual(12, results.Count); // Same as above since only rate codes were deleted.  No need to re-verify Burden Pool and Section details

			revisionOptions = sut.GetRevisionOptions(revisions);
			rateDetails = this.testData.RateDetailLoader.GetComparableRates(revisionOptions.Single(x => x.Id == item3.Id), revisionOptions.Single(x => x.Id == item2.Id));
			Assert.AreEqual(6, rateDetails.Count);
			Assert.AreEqual(1, rateDetails.Count(x => x.RateCode.Equals("MockRate1") && x.CompareState == RateCompareState.Edit.GetDescription()));
			Assert.AreEqual(1, rateDetails.Count(x => x.RateCode.Equals("MockRate1") && x.CompareState == null));
			Assert.AreEqual(1, rateDetails.Count(x => x.RateCode.Equals("MockRate2") && x.CompareState == RateCompareState.Edit.GetDescription()));
			Assert.AreEqual(1, rateDetails.Count(x => x.RateCode.Equals("MockRate2") && x.CompareState == null));
			Assert.AreEqual(1, rateDetails.Count(x => x.RateCode.Equals("MockRate3") && x.CompareState == RateCompareState.Deleted.GetDescription()));
			Assert.AreEqual(1, rateDetails.Count(x => x.RateCode.Equals("MockRate4") && x.CompareState == RateCompareState.Deleted.GetDescription()));
			// Note: We don't expect a row for MockRate5 here, since it was added then deleted between the last published revision (item2) and the current WIP (item3).
		}

		/// <summary>
		/// Tests insert, update, retrieve and delete
		/// </summary>
		[TestMethod]
		public void TestRevisionLoader_RevisionOnly()
		{
			RevisionLoader sut = this.testData.RevisionLoader as RevisionLoader;

			// insert
			RevisionModelView item = this.testData.GetRevision(true);

			// validate insert was successful
			ICollection<RevisionModelView> results = sut.GetAll();
			Assert.AreEqual(1, results.Count(x => x.Id == item.Id && x.Revision == item.Revision &&
												  x.History == item.History &&
												  x.CreatedBy == item.CreatedBy && x.StartYear == item.StartYear &&
												  x.EndYear == item.EndYear && x.ReleaseNotes == item.ReleaseNotes));

			item.Updateable = UpdateType.Upsert;
			item.History = "Mock Revision (updated)";
			item.StartYear = 2018;
			item.EndYear = 2050;
			item.ReleaseNotes = "Mock Notes (updated)";

			// update
			int? id;
			using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
			{
				id = sut.Upsert(item);
				scope.Complete();
			}

			results = sut.GetAll();
			Assert.AreEqual(1, results.Count(x => x.Id == id && x.Revision == item.Revision &&
												  x.History == item.History &&
												  x.CreatedBy == item.CreatedBy && x.StartYear == item.StartYear &&
												  x.EndYear == item.EndYear && x.ReleaseNotes == item.ReleaseNotes));

			// delete
			item = results.First(x => x.Id == id);
			item.Updateable = UpdateType.Deleted;

			using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
			{
				id = sut.Delete(item);
				scope.Complete();
			}

			results = sut.GetAll();
			Assert.AreEqual(0, results.Count(x => x.Id == id));
		}

		/// <summary>
		/// Tests insert, update, retrieve and delete for a revision with related 
		/// rates, document, and mapping data.
		/// </summary>
		[TestMethod]
		[Ignore("Original test already failing. Ignore until this one is fixed.")]
		public void TestRevisionLoader_RevisionWithData()
		{
			IRevisionMediator sut = this.testData.RevisionMediator;

			// insert
			RevisionModelView item = this.testData.GetRevision(true);
			this.testData.ModifyRevisionData1(item);

			// validate insert was successful
			ICollection<RevisionModelView> results = sut.GetAll();
			Assert.IsNotNull(item);
			Assert.IsTrue(item.Id >= 0);
			Assert.AreEqual(1, results.Count(x => x.Id == item.Id && x.Revision == item.Revision &&
												  x.History == item.History && x.CreatedBy == item.CreatedBy &&
												  x.StartYear == item.StartYear && x.EndYear == item.EndYear &&
												  x.ReleaseNotes == item.ReleaseNotes));

			this.testData.VerifyModifiedRevisionData1(item);

			// publish
			RevisionModelView wipRevision = this.testData.PublishRevision(item);
			Assert.IsNotNull(wipRevision);

			// Verify previous WIP revision has publish date set. 
			RevisionModelView oldRevision = sut.GetById(item.Id);
			Assert.AreEqual(oldRevision.Id, item.Id);
			Assert.IsNotNull(oldRevision.DatePublished);
			Assert.AreEqual(oldRevision.PublishedBy, this.testData.TestUser.DisplayName);
			this.testData.VerifyModifiedRevisionData1(oldRevision); // Verify rate data is still correct

			// Verify new WIP revision has correct revision name and publish date is null.
			string expectedRevision = (int.Parse(oldRevision.Revision) + 1).ToString();
			Assert.AreEqual(expectedRevision, wipRevision.Revision);
			Assert.IsNull(wipRevision.DatePublished);
			this.testData.VerifyModifiedRevisionData1(wipRevision); // Verify rate data was copied

			// Make some changes to the WIP revision and verify
			this.testData.ModifyRevisionData2(wipRevision);
			wipRevision = sut.GetById(wipRevision.Id);
			this.testData.VerifyModifiedRevisionData2(wipRevision);

			// Make some more changes to the WIP revision and verify
			this.testData.ModifyRevisionData3(wipRevision);
			wipRevision = sut.GetById(wipRevision.Id);
			this.testData.VerifyModifiedRevisionData3(wipRevision);

			// rollback
			RevisionModelView rolledBackRevision = this.testData.RollbackRevision(oldRevision, wipRevision);
			Assert.IsNotNull(rolledBackRevision);
			Assert.AreNotEqual(oldRevision.Id, rolledBackRevision.Id);
			Assert.AreNotEqual(wipRevision.Id, rolledBackRevision.Id);

			// Verify rollback cleared changes (from above)
			oldRevision = sut.GetById(item.Id);
			this.testData.VerifyModifiedRevisionData1(oldRevision); // Verify rate data is still correct
			this.testData.VerifyModifiedRevisionData1(rolledBackRevision); // Verify rate data was rolled back
		}

		/// <summary>
		/// Validates the sections in file attachments after a publish.
		/// </summary>
		[TestMethod]
		[Ignore("Original test already failing. Ignore until this one is fixed.")]
		public void ValidatePublishSections()
		{
			RevisionModelView revision = this.testData.GetRevision(true);

			// See if the test data is there, if not then add it
			ICollection<FileAttachmentRowModelView> attachments = this.testData.FileAttachmentLoader.GetByRevision(revision.Id);
			FileAttachmentRowModelView bing = attachments.FirstOrDefault(f => f.Link == "http://www.bing.com" && f.Name == "bing");
			if (bing == null)
			{
				this.testData.AddBaselineSectionsAndRatesData(revision);
			}

			attachments = this.testData.FileAttachmentLoader.GetByRevision(revision.Id);
			bing = attachments.First(f => f.Link == "http://www.bing.com" && f.Name == "bing");
			FileAttachmentRowModelView google = attachments.First(f => f.Link == "http://www.google.com" && f.Name == "Google");

			RevisionModelView item2 = this.testData.PublishRevision(revision);
			ICollection<FileAttachmentRowModelView> attachments2 = this.testData.FileAttachmentLoader.GetByRevision(item2.Id);
			FileAttachmentRowModelView bing2 = attachments2.First(f => f.Link == "http://www.bing.com" && f.Name == "bing");
			FileAttachmentRowModelView google2 = attachments2.First(f => f.Link == "http://www.google.com" && f.Name == "Google");

			// both sections ids should be zero
			Assert.AreEqual(0, google.SectionId);
			Assert.AreEqual(0, google2.SectionId);
			Assert.AreNotEqual(google.Id, google2.Id);
			Assert.AreNotEqual(google.RevisionID, google2.RevisionID);

			// both section ids should not be null, and unique
			Assert.IsNotNull(bing2.SectionId);
			Assert.IsNotNull(bing.SectionId);
			Assert.AreNotEqual(bing.SectionId, bing2.SectionId);

			Assert.AreEqual(this.testData.SectionLoader.RetrieveAllSections(revision).First(s => s.Id == attachments.ElementAt(0).SectionId).Title,
				this.testData.SectionLoader.RetrieveAllSections(item2).First(s => s.Id == attachments2.ElementAt(0).SectionId).Title);

			PPRDControllerLogic sut = new(this.testData.FileAttachmentLoader, null, null, null, null, Mock.Of<IConfiguration>());
			ICollection<OptionModelView> sections = this.testData.SectionLoader.RetrieveSectionsAsOptions(item2);
			ICollection<ValidationMessage> errors = new List<ValidationMessage>();
			ICollection<int> sectionIdsUsed = attachments2.Where(a => a.SectionId.HasValue).Select(f => f.SectionId.Value).Distinct().ToList();

			int goodSectionId = sections.First(s => !sectionIdsUsed.Contains(s.Id)).Id;
			sut.ValidateDeletionSections(new int[] { goodSectionId }, errors, sections, item2);
			Assert.AreEqual(0, errors.Count);

			sut.ValidateDeletionSections(new int[] { bing2.SectionId.Value }, errors, sections, item2);
			Assert.AreEqual(1, errors.Count);

			// rollback
			this.testData.RollbackRevision(revision, item2);
		}

		/// <summary>
		/// Test missing parameters on Publish operation.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ValidatePublish_EX1()
		{
			IRevisionLoader sut = new RevisionLoader(Mock.Of<ILogger<RevisionLoader>>());
			UserData testUser = this.testData.TestUser;
			sut.Publish(null, testUser.DisplayName);
		}

		/// <summary>
		/// Test missing parameters on Publish operation.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ValidatePublish_EX2()
		{
			IRevisionLoader sut = new RevisionLoader(Mock.Of<ILogger<RevisionLoader>>());
			RevisionModelView item = this.testData.GetRevision(false);
			sut.Publish(item, null);
		}

		/// <summary>
		/// Test missing parameters on Rollback operation.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ValidateRollback_EX1()
		{
			IRevisionLoader sut = new RevisionLoader(Mock.Of<ILogger<RevisionLoader>>());
			RevisionModelView item = this.testData.GetRevision(false);
			sut.Rollback(null, item);
		}

		/// <summary>
		/// Test missing parameters on Rollback operation.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ValidateRollback_EX2()
		{
			IRevisionLoader sut = new RevisionLoader(Mock.Of<ILogger<RevisionLoader>>());
			RevisionModelView item = this.testData.GetRevision(false);
			sut.Rollback(item, null);
		}
	}
}
