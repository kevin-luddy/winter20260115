IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[createWorkspaceVersion]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[createWorkspaceVersion];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[createWorkspaceVersion]
(
@VersionName varchar(50),
@CreatedByETIUserID int,
@WorkspaceStateID int,
@WorkspaceID int
)
AS
/******************************************************************************
**		 
**		Name: [createWorkspaceVersion]
**		Desc:	Inserts a record in the Workspace Version Table
**				Then, copies every table from dbo to version schema
**				copying all of the Workspace data
**			
**		
**
**		Auth: Don Canuso
**		Date: 5/17/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		4/04/2018	brunworg			BOEJ-3177 Added TieredPercentage column.
**		4/13/18		pattoncr			BOEJ-3185 - DB Work (Sikorsky Legacy Resources)
**		5/2/18		ranzalon			BOEJ-3416 - Templates available to all Workspaces
**		6/18/18		ranzalon			BOEJ-3448 - ProPricer API updates
**		10/2/18		ranzalon			BOEJ-3699 - RTE Size Limit
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
**		7/12/19		twilson3			BOEJ-4037	System Pro Pricer Export 
**		9/26/19		ranzalon			BOEJ-4349 - Revised Submittal Date
**		12/2/19		ranzalon			BOEJ-4464 - Added LaborSortId
**		12/5/19		twilson3			BOEJ-4429 - RTE Templates
**		12/13/19	twilson3			BOEJ-4434 - RTE Template Answers
**		12/17/19	twilson3			BOEJ-4434 Fix Assigned
**		8/27/20		ranzalon			BOEJ-4760 - Template Boe
**		9/15/20		ranzalon			BOEJ-4776/4825 - MOQ Types update
**		12/8/2020	ranzalon			BOEJ-4972 - remove CER location and BOELaborType MOQTypeSelectionId fields
**		1/4/2021	Dusan				BOEJ-4894: Added support for MoqTypeTableCustomFieldValueXREF
**		1/31/23		e405721				ACV-221 - Enable SAP Connection
**		1/18/24		ranzalon			PROPH-1070 Update for HistoricalReferenceExplanation
*******************************************************************************/
SET NOCOUNT ON 
--BEGIN TRANSACTION 

DECLARE @Inserted AS Table (ID int)
DECLARE @VersionID int,
@CreateDate datetime2 = GetDate()

INSERT INTO [dbo].[WorkspaceVersion]
([VersionName]
,[VersionCreated]
,[CreatedByETIUserID]
,[WorkspaceStateID]
,[WorkspaceID])
OUTPUT inserted.VersionID INTO @Inserted           
VALUES
(@VersionName
,@CreateDate
,@CreatedByETIUserID
,@WorkspaceStateID
,@WorkspaceID)

SELECT @VersionID = ID FROM @Inserted

INSERT INTO [version].[PerformingOrganizationList]
([PerformingOrganizationListID]
,[PerformingOrganizationListName]
,[UpdateDT]
,[VersionID])
SELECT PO.[PerformingOrganizationListID]
,PO.[PerformingOrganizationListName]
,PO.[UpdateDT]
,@VersionID
FROM [dbo].[PerformingOrganizationList] PO
INNER JOIN dbo.Workspace WS ON WS.PerformingOrganizationListID = PO.PerformingOrganizationListID
WHERE
WS.WorkspaceID = @WorkspaceID	

INSERT INTO [version].[ResourceList]
([ResourceListID]
,[ResourceListName]
,[UpdateDT]
,[VersionID])
SELECT R.[ResourceListID]
,R.[ResourceListName]
,R.[UpdateDT]
,@VersionID
FROM [dbo].[ResourceList] R
INNER JOIN dbo.Workspace WS ON WS.ResourceListID = R.ResourceListID
WHERE
WS.WorkspaceID = @WorkspaceID	

INSERT INTO [version].[Workspace]
([WorkspaceID]
,[WorkspaceName]
,[WorkspaceShortName]
,[WorkspaceStateID]
,[ContractStartDate]
,[ContractEndDate]
,[ProposalSubmitDate]
,[WorkspaceDescription]
,[CostVolumeLeadPricerUserID]
,[RFPNumber]
,[TemplateID]
,[ContainsOCI]
,[CreatedByETIUserID]
,[AllowSearch]
,[ResourceListID]
,[PerformingOrganizationListID]
--,[ResourceChangeFlag]
,[PerformingOrganizationChangeFlag]
,[TrackingNumber]
,[ContainsTemplate]
,[NumProPricerExport]
,[ProposalStatusID]
,[StatusComment]
,[UpdateDT]
,[VersionID]
/*Remove Code for 7681 ,[LaborPrecisionID]*/
/*WI9756*/
,[BOEExportSortByID]
,[SegmentID]
,[LineOfBusinessID]
,[ProposalClassID]
,[ProposalTitle]
,[IsDeleted]
,[DateDeleted]
,[ResourcePrecision]
,[RecalculationStartedDate]
,[CostPrecision]
,[IsUsingEquivalentPerson]
,[IsUsingTM]
,[ProjectMapTypeID]
,[AllowGridEdit]
,[CustomSorting]
,[ResourceSorting]
,[PerfOrgSorting]
,[LastProPricerInstance]
,[LastProPricerProposal]
,[RteSizeLimit]
,[RevisedSubmittalDate]
,[TemplateBoe]
,[EnableSAPConnection])
SELECT [WorkspaceID]
,[WorkspaceName]
,[WorkspaceShortName]
,[WorkspaceStateID]
,[ContractStartDate]
,[ContractEndDate]
,[ProposalSubmitDate]
,[WorkspaceDescription]
,[CostVolumeLeadPricerUserID]
,[RFPNumber]
,[TemplateID]
,[ContainsOCI]
,[CreatedByETIUserID]
,[AllowSearch]
,[ResourceListID]
,[PerformingOrganizationListID]
,[PerformingOrganizationChangeFlag]
,[TrackingNumber]
,[ContainsTemplate]
,[NumProPricerExport]
,[ProposalStatusID]
,[StatusComment]
,[UpdateDT]
,@VersionID
/*Remove Code for 7681 ,[LaborPrecisionID]*/
,[BOEExportSortByID]
,[SegmentID]
,[LineOfBusinessID]
,[ProposalClassID]
,[ProposalTitle]
,[IsDeleted]
,[DateDeleted]
,[ResourcePrecision]
,[RecalculationStartedDate]
,[CostPrecision]
,[IsUsingEquivalentPerson]
,[IsUsingTM]
,[ProjectMapTypeID]
,[AllowGridEdit]
,[CustomSorting]
,[ResourceSorting]
,[PerfOrgSorting]
,[LastProPricerInstance]
,[LastProPricerProposal]
,[RteSizeLimit]
,[RevisedSubmittalDate]
,[TemplateBoe]
,[EnableSAPConnection]
FROM [dbo].[Workspace]
WHERE WorkspaceID = @WorkspaceID

INSERT INTO [version].[WorkspaceContractTypeXREF]
	     ([WorkspaceContractTypeID]
	     ,[UpdateDT]
	     ,[WorkspaceID]
	     ,[ContractTypeID]
	     ,[VersionID])
SELECT WR.[WorkspaceContractTypeID]
	,WR.[UpdateDT]
	,WR.[WorkspaceID]
	,WR.[ContractTypeID]
	,@VersionID
FROM [dbo].[WorkspaceContractTypeXREF] WR
	INNER JOIN dbo.Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
WHERE 
	WS.WorkspaceID = @WorkspaceID	

INSERT INTO [version].[OutputFormatTemplate]
([TemplateID]
,[Template]
,[TemplateDescription]
,[TemplateFile]
,[UpdateDT]
,[IsActive]
,[ParentTemplateID]
,[IsAvailableToAllWorkspaces]
)
SELECT OFT.[TemplateID]
,OFT.[Template]
,OFT.[TemplateDescription]
,OFT.[TemplateFile]
,OFT.[UpdateDT]
,OFT.[IsActive]
,OFT.[ParentTemplateID]
,OFT.[IsAvailableToAllWorkspaces]
FROM [dbo].[OutputFormatTemplate] OFT
INNER JOIN dbo.OutputFormatTemplateWorkspaceXREF OX ON OFT.TemplateID = OX.TemplateID
INNER JOIN dbo.Workspace WS ON OX.WorkspaceID = WS.WorkspaceID
WHERE 
WS.WorkspaceID = @WorkspaceID
AND NOT EXISTS
(
	SELECT 1
	FROM [version].[OutputFormatTemplate] VOFT
	WHERE VOFT.TemplateID = OFT.TemplateID AND VOFT.UpdateDT = OFT.UpdateDT
)

INSERT INTO [version].[OutputFormatTemplateVersionXREF]
([VersionID]
,[BackupTemplateID]
)
SELECT
@VersionID
,OVX.[BackupTemplateID]
FROM [dbo].[OutputFormatTemplate] OFT
INNER JOIN dbo.OutputFormatTemplateWorkspaceXREF OX ON OFT.TemplateID = OX.TemplateID
INNER JOIN dbo.Workspace WS ON OX.WorkspaceID = WS.WorkspaceID
INNER JOIN [version].[OutputFormatTemplate] OVX ON OFT.TemplateID = OVX.TemplateID AND OFT.UpdateDT = OVX.UpdateDT	
WHERE 
WS.WorkspaceID = @WorkspaceID 

INSERT INTO [version].[BOE]
([BOEID]
,[BOEStateID]
,[BOEStartDate]
,[BOEEndDate]
,[BOEDescription]
,[DataSource]
,[WorkspaceID]
,[MetricDisclosureAcknowledge]
,[NumAuthorReassigned]
,[IsMaterial]
,[UpdateDT]
,[VersionID]
,[BOETitle]
,[IsMultiClinWbs]
)
SELECT B.[BOEID]
,B.[BOEStateID]
,B.[BOEStartDate]
,B.[BOEEndDate]
,B.[BOEDescription]
,B.[DataSource]
,B.[WorkspaceID]
,B.[MetricDisclosureAcknowledge]
,B.[NumAuthorReassigned]
,B.[IsMaterial]
,B.[UpdateDT]
,@VersionID
,B.BOETitle
,B.IsMultiClinWbs
FROM [dbo].[BOE] B
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID	

INSERT INTO [version].[WorkspaceOffloadRate]
	([OffloadRateID]
	,[VersionID]
	,[UpdateDT]
	,[WorkspaceID]
	,[Resource]
	,[PerfOrg]
	,[PercentToOffload]
	,[Year]
	,[SubcontractorResource]
	,[HourlyRate])
SELECT [OffloadRateID]
	,@VersionID
	,[UpdateDT]
	,[WorkspaceID]
	,[Resource]
	,[PerfOrg]
	,[PercentToOffload]
	,[Year]
	,[SubcontractorResource]
	,[HourlyRate]
FROM [dbo].[WorkspaceOffloadRate]
WHERE WorkspaceID = @WorkspaceID

INSERT INTO [version].[ProjectMap]
	([VersionID]
	,[ID]
	,[WorkspaceId]
	,[WbsNumber]
	,[WbsElementTitle]
	,[ActivityID]
	,[ActivityName]
	,[Resource]
	,[CostCenter] 
	,[StartDate] 
	,[EndDate] 
	,[CLIN] 
	,[Task] 
	,[SOW] 
	,[SOWTitle] 
	,[Rationale]
	,[CamName] 
	,[Category] 
	,[Hours]
	,[Dollars] 
	,[CanOffload]
	,[AddOrDelete] 
	,[ClassOfCost] 
	,[OrderID]
	,[TieredPercentage]
	,[LegacyResourceID])
SELECT 
	@VersionID
	,[ID]
	,[WorkspaceId]
	,[WbsNumber]
	,[WbsElementTitle]
	,[ActivityID]
	,[ActivityName]
	,[Resource]
	,[CostCenter] 
	,[StartDate] 
	,[EndDate] 
	,[CLIN] 
	,[Task] 
	,[SOW] 
	,[SOWTitle] 
	,[Rationale]
	,[CamName] 
	,[Category] 
	,[Hours]
	,[Dollars] 
	,[CanOffload]
	,[AddOrDelete] 
	,[ClassOfCost] 
	,[OrderID]
	,[TieredPercentage]
	,[LegacyResourceID]
FROM [dbo].[ProjectMap]
WHERE WorkspaceID = @WorkspaceID

INSERT INTO [version].[ProjectMapSpread]
	     ([VersionID]
	     ,[ID]
		   ,[WorkspaceId]
		   ,[ProjectMapId]
	     ,[SpreadDate]
	     ,[SpreadValue]
	     )
SELECT 	    @VersionID,
			[ID],
			[WorkspaceId],
			[ProjectMapId],
			[SpreadDate],
			[SpreadValue]
FROM  [dbo].ProjectMapSpread S
WHERE [WorkspaceId] = @WorkspaceID

/** RTE Templates **/

INSERT INTO [version].[RteTemplate]
			([TemplateID],
			[UpdateDT],
			[WorkspaceID],
			[Description],
			[AuthorID],
			[CreatedOn],
			[VersionID])
			SELECT [TemplateID],
		[UpdateDT]
	,[WorkspaceID]
	,[Description]
	,[AuthorID]
	,[CreatedOn]
	  ,@VersionID
  FROM [dbo].[RteTemplate]
WHERE WorkspaceID = @WorkspaceID

INSERT INTO [version].[RteTemplateAssigned]
	([TemplateID],
	[RteTemplateSourceId],
	VersionID)
	SELECT RA.[TemplateID],
		RA.[RteTemplateSourceId],
		@VersionID
	FROM [dbo].[RteTemplateAssigned] RA
	INNER JOIN [dbo].[RteTemplate] R on R.[TemplateID] = RA.[TemplateID]
WHERE R.WorkspaceID = @WorkspaceID

INSERT INTO [version].[RteTemplateQuestion]
	     ([QuestionID],
		    [UpdateDT],
			[TemplateID],
			[Text],
			[SortOrder],
			[Required],
			[VersionID])
SELECT RTQ.[QuestionID]
	  ,RTQ.[UpdateDT]
	,RTQ.[TemplateID]
	,RTQ.[Text]
	,RTQ.[SortOrder]
	,RTQ.[Required]
	  ,@VersionID
  FROM [dbo].[RteTemplateQuestion] RTQ
  INNER JOIN dbo.[RteTemplate] RT ON RT.TemplateID = RTQ.TemplateID
  WHERE RT.WorkspaceID = @WorkspaceID

INSERT INTO [version].[RteTemplateAnswer]
	([AnswerID],
	[UpdateDT],
	[QuestionID],
	[BOEID],
	[TaskID],
	[Text],
	[RteTemplateSourceId],
	[VersionID])
SELECT RTA.[AnswerID],
		RTA.[UpdateDT],
		RTA.[QuestionID],
		RTA.[BOEID],
		RTA.[TaskID],
		RTA.[Text],
		RTA.[RteTemplateSourceId],
		@VersionID
	FROM [dbo].[RteTemplateAnswer] RTA
	INNER JOIN [dbo].[RteTemplateQuestion] RTQ ON RTA.[QuestionID] = RTQ.[QuestionID]
	INNER JOIN dbo.[RteTemplate] RT ON RT.TemplateID = RTQ.TemplateID
	WHERE RT.WorkspaceID = @WorkspaceID

/** Custom Fields **/

INSERT INTO [version].[CustomField]
([CustomFieldID]
,[CustomFieldName]
,[CustomFieldRequired]
,[CustomFieldDisplayID]
,[WorkspaceID]
,[UpdateDT]
,[IsOpenEnded]
,[VersionID])
SELECT CF.[CustomFieldID]
,CF.[CustomFieldName]
,CF.[CustomFieldRequired]
,CF.[CustomFieldDisplayID]
,CF.[WorkspaceID]
,CF.[UpdateDT]
,CF.[IsOpenEnded]
,@VersionID
FROM [dbo].[CustomField] CF
INNER JOIN dbo.Workspace WS ON CF.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID	

INSERT INTO [version].[CLIN]
(
 [CLINID]
,[CLINNumber]
,[CLINTitle]
,[CLINStartDate]
,[CLINEndDate]
,[ContractTypeID]
,[WorkspaceID]
,[UpdateDT]
,[VersionID]
,[DisplayedCLINNumber]
)
SELECT 
 C.[CLINID]
,C.[CLINNumber]
,C.[CLINTitle]
,C.[CLINStartDate]
,C.[CLINEndDate]
,C.[ContractTypeID]
,C.[WorkspaceID]
,C.[UpdateDT]
,@VersionID
,C.[DisplayedCLINNumber]
FROM [dbo].[CLIN] C
INNER JOIN dbo.Workspace WS ON C.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID	

INSERT INTO [version].[BOEPotentialRole]
([BOEPotentialRoleID]
,[ETIUserID]
/*,[ETIGroupID]*/
,[WorkspaceID]
,[RoleID]
,[UserRemoved]
,[UpdateDT]
,[VersionID])
SELECT BR.[BOEPotentialRoleID]
,BR.[ETIUserID]
/*,BR.[ETIGroupID]*/
,BR.[WorkspaceID]
,BR.[RoleID]
,BR.[UserRemoved]
,BR.[UpdateDT]
,@VersionID
FROM [dbo].[BOEPotentialRole] BR
INNER JOIN dbo.Workspace WS ON BR.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID	

INSERT INTO [version].[WorkBreakdownStructure]
	     ([WBSID]
	     ,[UpdateDT]
	     ,[WBSNumber]
	     ,[DisplayedWBSNumber]
	     ,[WBSTitle]
	     ,[WorkspaceID]
	     ,[VersionID])
SELECT 
 WBS.[WBSID]
,WBS.[UpdateDT] 
,WBS.[WBSNumber]
,WBS.[DisplayedWBSNumber]
,WBS.[WBSTitle]
,WBS.[WorkspaceID]
,@VersionID
FROM [dbo].[WorkBreakdownStructure] WBS
	INNER JOIN dbo.Workspace WS ON WBS.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID	

INSERT INTO [version].[TMResourceRate]
	     ([TMResourceRateID]
	     ,[UpdateDT]
	     ,[WorkspaceID]
	     ,[TMResourceID]
	     ,[TMResourceRateStartDate]
	     ,[TMResourceRateEndDate]
	     ,[TMResourceRate]
	     ,[VersionID])
SELECT TMRR.[TMResourceRateID]
	,TMRR.[UpdateDT]
	,TMRR.[WorkspaceID]
	,TMRR.[TMResourceID]
	,TMRR.[TMResourceRateStartDate]
	,TMRR.[TMResourceRateEndDate]
	,TMRR.[TMResourceRate]
	,@VersionID
FROM [dbo].[TMResourceRate] TMRR
	INNER JOIN dbo.Workspace WS ON TMRR.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID	

INSERT INTO [version].[WorkspaceStateHistory]
([WorkspaceStateHistoryID]
,[WorkspaceID]
,[CurrentWorkspaceStateID]
,[UpdatedWorkspaceStateID]
,[ChangedByETIUserID]
,[UpdateDT]
,[VersionID])
SELECT WSH.[WorkspaceStateHistoryID]
,WSH.[WorkspaceID]
,WSH.[CurrentWorkspaceStateID]
,WSH.[UpdatedWorkspaceStateID]
,WSH.[ChangedByETIUserID]
,WSH.[UpdateDT]
,@VersionID
FROM [dbo].[WorkspaceStateHistory] WSH
INNER JOIN dbo.Workspace WS ON WSH.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[WorkspaceVariable]
([WorkspaceVariableID]
,[WorkspaceVariableName]
,[WorkspaceVariableValue]
,[WorkspaceID]
,[SortByID]
,[ValueTypeID]
,[IsPercentage]
,[UpdateDT]
,[VersionID])
SELECT WSV.[WorkspaceVariableID]
,WSV.[WorkspaceVariableName]
,WSV.[WorkspaceVariableValue]
,WSV.[WorkspaceID]
,WSV.[SortByID]
,WSV.[ValueTypeID]
,WSV.[IsPercentage]
,WSV.[UpdateDT]
,@VersionID
FROM [dbo].[WorkspaceVariable] WSV
INNER JOIN dbo.Workspace WS ON WSV.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[WorkspaceVariableSumVariableResourceTypeXREF]
([WVSVRTID]
,[WorkspaceVariableID]
,[SumVariableResourceTypeID]
,[VersionID])
SELECT X.[WVSVRTID]
,X.[WorkspaceVariableID]
,X.[SumVariableResourceTypeID]
,@VersionID
FROM [dbo].[WorkspaceVariableSumVariableResourceTypeXREF] X
INNER JOIN [dbo].[WorkspaceVariable] WSV ON X.WorkspaceVariableID = WSV.WorkspaceVariableID
INNER JOIN dbo.Workspace WS ON WSV.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[WorkspaceUserRole]
([WorkspaceUserRoleID]
,[ETIUserID]
/*,[ETIGroupID]*/
,[RoleID]
,[WorkspaceID]
,[HideHelp]
,[UpdateDT]
,[VersionID])
SELECT WUR.[WorkspaceUserRoleID]
,WUR.[ETIUserID]
/*,WUR.[ETIGroupID]*/
,WUR.[RoleID]
,WUR.[WorkspaceID]
,WUR.[HideHelp]
,WUR.[UpdateDT]
,@VersionID
FROM [dbo].[WorkspaceUserRole] WUR
INNER JOIN dbo.Workspace WS ON WUR.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[ProPricerExport]
([ProPricerExportID]
,[ProPricerExportName]
,[WorkspaceID]
,[UpdateDT]
,[VersionID])
SELECT PPE.[ProPricerExportID]
,PPE.[ProPricerExportName]
,PPE.[WorkspaceID]
,PPE.[UpdateDT]
,@VersionID
FROM [dbo].[ProPricerExport] PPE
INNER JOIN dbo.Workspace WS ON PPE.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[Resource]
	     ([ResourceID]
	     ,[ResourceName]
	     ,[ResourceDescription]
	     ,[SegmentRegion]
	     ,[LaborType]
	     ,[SegmentID]
	     ,[ResourceListID]
	     /*,[ResourceInUseFlag]*/
	     ,[CostElementID]
	     ,[UpdateDT]
		   ,[VersionID]
		   ,[DeletedFlag]
		   ,[RateTypeID]
		   )
SELECT R.[ResourceID]
	,R.[ResourceName]
	,R.[ResourceDescription]
	,R.[SegmentRegion]
	,R.[LaborType]
	,R.[SegmentID]
	,R.[ResourceListID]
	/*,R.[ResourceInUseFlag]*/
	,R.[CostElementID]
	,R.[UpdateDT]
	,@VersionID
	,R.[DeletedFlag]
	,R.RateTypeID
FROM [dbo].[Resource] R 
--INNER JOIN [dbo].[BOELaborType] BLT ON R.ResourceID = BLT.ResourceID
--INNER JOIN [dbo].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
--INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON R.ResourceListID = WS.ResourceListID
--B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[WorkspaceResource]
	     ([WorkspaceResourceID]
	     ,[SystemResourceID]
	     ,[ResourceListID]
	     ,[WorkspaceID]
	     ,[VersionID])
SELECT WR.[WorkspaceResourceID]
	,WR.[SystemResourceID]
	,WR.[ResourceListID]
	,WR.[WorkspaceID]
	,@VersionID
FROM [dbo].[WorkspaceResource] WR
INNER JOIN dbo.Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

/*
INSERT INTO [version].[PerformingOrganization]
([PerformingOrganizationID]
,[PerformingOrganizationName]
,[PerformingOrganizationDescription]
,[PerformingOrganizationListID]
,[PerformingOrganizationInUseFlag]
,[UpdateDT]
,[VersionID])
SELECT PO.[PerformingOrganizationID]
	,PO.[PerformingOrganizationName]
	,PO.[PerformingOrganizationDescription]
	,PO.[PerformingOrganizationListID]
	,PO.[PerformingOrganizationInUseFlag]
	,PO.[UpdateDT]
	,@VersionID
FROM [dbo].[PerformingOrganization] PO
INNER JOIN [dbo].[BOELaborType] BLT ON PO.PerformingOrganizationID = BLT.PerformingOrganizationID
INNER JOIN [dbo].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID
*/

INSERT INTO [version].[PerformingOrganization]
	     ([PerformingOrganizationID]
	     ,[PerformingOrganizationName]
	     ,[PerformingOrganizationDescription]
	     ,[PerformingOrganizationListID]
	     /*,[PerformingOrganizationInUseFlag]*/
	     ,[UpdateDT]
		   ,[VersionID]
		   ,[DeletedFlag])
SELECT R.[PerformingOrganizationID]
	,R.[PerformingOrganizationName]
	,R.[PerformingOrganizationDescription]
	,R.[PerformingOrganizationListID]
	/*,R.[PerformingOrganizationInUseFlag]*/
	,R.[UpdateDT]
	,@VersionID
	,R.[DeletedFlag]
FROM [dbo].[PerformingOrganization] R 
--INNER JOIN [dbo].[BOELaborType] BLT ON R.PerformingOrganizationID = BLT.PerformingOrganizationID
--INNER JOIN [dbo].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
--INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON R.PerformingOrganizationListID = WS.PerformingOrganizationListID
--B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[WorkspacePerformingOrganization]
	     ([WorkspacePerformingOrganizationID]
	     ,[SystemPerformingOrganizationID]
	     ,[PerformingOrganizationListID]
	     ,[WorkspaceID]
	     ,[VersionID])
SELECT WR.[WorkspacePerformingOrganizationID]
	,WR.[SystemPerformingOrganizationID]
	,WR.[PerformingOrganizationListID]
	,WR.[WorkspaceID]
	,@VersionID
FROM [dbo].[WorkspacePerformingOrganization] WR
INNER JOIN dbo.Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[OutputFormatTemplateWorkspaceXREF]
([OutputFormatID]
,[WorkspaceID]
,[TemplateID]
,[VersionID])
SELECT OX.[OutputFormatID]
,OX.[WorkspaceID]
,OX.[TemplateID]
,@VersionID
FROM [dbo].[OutputFormatTemplateWorkspaceXREF] OX
INNER JOIN dbo.Workspace WS ON OX.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[ProPricerFieldXREF]
([PFID]
,[ProPricerExportID]
,[ProPricerFieldID]
,[ProPricerTypeID]
,[ListOrder]
,[VersionID])
SELECT PX.[PFID]
,PX.[ProPricerExportID]
,PX.[ProPricerFieldID]
,PX.[ProPricerTypeID]
,PX.[ListOrder]
,@VersionID
FROM [dbo].[ProPricerFieldXREF] PX
INNER JOIN [dbo].[ProPricerExport] PPE ON PX.ProPricerExportID = PPE.ProPricerExportID
INNER JOIN dbo.Workspace WS ON PPE.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[ProPricerCustomFieldXREF]
([PCID]
,[ProPricerExportID]
,[CustomFieldID]
,[ProPricerTypeID]
,[ProPricerCustomFieldSelectionID]
,[ListOrder]
,[VersionID])
SELECT PX.[PCID]
,PX.[ProPricerExportID]
,PX.[CustomFieldID]
,PX.[ProPricerTypeID]
,PX.[ProPricerCustomFieldSelectionID]
,PX.[ListOrder]
,@VersionID
FROM [dbo].[ProPricerCustomFieldXREF] PX
INNER JOIN [dbo].[ProPricerExport] PPE ON PX.ProPricerExportID = PPE.ProPricerExportID
INNER JOIN dbo.Workspace WS ON PPE.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[WBS_CLIN_BOE_XREF]
([WCBID]
,[WBSID]
,[CLINID]
,[BOEID]
,[VersionID])
SELECT X.[WCBID], X.[WBSID], X.[CLINID], X.[BOEID], @VersionID
FROM [dbo].[WBS_CLIN_BOE_XREF] X INNER JOIN dbo.WorkBreakdownStructure WBS ON X.WBSID = WBS.WBSID AND WBS.WorkspaceID = @WorkspaceID
UNION
SELECT X.[WCBID], X.[WBSID], X.[CLINID], X.[BOEID], @VersionID
FROM [dbo].[WBS_CLIN_BOE_XREF] X INNER JOIN dbo.CLIN C ON X.CLINID = C.CLINID AND C.WorkspaceID = @WorkspaceID
UNION
SELECT X.[WCBID], X.[WBSID], X.[CLINID], X.[BOEID], @VersionID
FROM [dbo].[WBS_CLIN_BOE_XREF] X INNER JOIN dbo.BOE B ON X.BOEID = B.BOEID AND B.WorkspaceID = @WorkspaceID

INSERT INTO [version].[SumOfBOE_WorkspaceVariableXREF]
([WVSumID]
,[WorkspaceVariableID]
,[CLINID]
,[WBSID]
,[BOEID]
,[VersionID])
SELECT X.[WVSumID]
,X.[WorkspaceVariableID]
,X.[CLINID]
,X.[WBSID]
,X.[BOEID]
,@VersionID
FROM [dbo].[SumOfBOE_WorkspaceVariableXREF] X
INNER JOIN [dbo].[WorkspaceVariable] WSV ON X.WorkspaceVariableID = WSV.WorkspaceVariableID
INNER JOIN dbo.Workspace WS ON WSV.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOEStateHistory]
	     ([BOEStateHistoryID]
	     ,[BOEID]
	     ,[FieldID]
	     ,[CurrentBOEStateID]
	     ,[UpdatedBOEStateID]
	     ,[ChangedByETIUserID]
	     ,[UpdateDT]
	     ,[VersionID])
SELECT BH.[BOEStateHistoryID]
	,BH.[BOEID]
	,BH.[FieldID]
	,BH.[CurrentBOEStateID]
	,BH.[UpdatedBOEStateID]
	,BH.[ChangedByETIUserID]
	,BH.[UpdateDT]
	,@VersionID
  FROM [dbo].[BOEStateHistory] BH
INNER JOIN dbo.BOE B ON BH.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOECommentHistory]
([BOECommentHistoryID]
,[BOECommentID]
,[BOEID]
,[FieldID]
,[CurrentComment]
,[UpdatedComment]
,[ChangedByETIUserID]
,[UpdateDT]
,[VersionID])
SELECT BH.[BOECommentHistoryID]
,BH.[BOECommentID]
,BH.[BOEID]
,BH.[FieldID]
,BH.[CurrentComment]
,BH.[UpdatedComment]
,BH.[ChangedByETIUserID]
,BH.[UpdateDT]
,@VersionID
FROM [dbo].[BOECommentHistory] BH
INNER JOIN [dbo].[BOEComment] BC ON BH.BOECommentID = BC.BOECommentID
INNER JOIN dbo.BOE B ON BH.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOEUserRole]
([BOEUserRoleID]
,[ETIUserID]
,[RoleID]
,[BOEID]
,[UpdateDT]
,[VersionID])
SELECT BUR.[BOEUserRoleID]
,BUR.[ETIUserID]
,BUR.[RoleID]
,BUR.[BOEID]
,BUR.[UpdateDT]
,@VersionID
FROM [dbo].[BOEUserRole] BUR
INNER JOIN dbo.BOE B ON BUR.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

/*WI 5160*/
INSERT INTO [version].[BOEUserRoleHistory]
	     (
		[BOEUserRoleHistoryID]
	     ,[UpdateDT]
	     ,[CurrentETIUserID]
	     ,[UpdatedETIUserID]
	     ,[RoleID]
	     ,[BOEID]
	     ,[FieldID]
	     ,[ChangedByETIUserID]
	     ,[VersionID]
	     )
SELECT BUR.[BOEUserRoleHistoryID]
	,BUR.[UpdateDT]
	,BUR.[CurrentETIUserID]
	,BUR.[UpdatedETIUserID]
	,BUR.[RoleID]
	,BUR.[BOEID]
	,BUR.[FieldID]
	,BUR.[ChangedByETIUserID]
	,@VersionID
FROM [dbo].[BOEUserRoleHistory] BUR
INNER JOIN dbo.BOE B ON BUR.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOEApproval]
([BOEApprovalID]
,[BOEID]
,[ApprovalETIUserID]
,[ApprovedFlag]
,[UpdateDT]
,[VersionID])
SELECT BA.[BOEApprovalID]
,BA.[BOEID]
,BA.[ApprovalETIUserID]
,BA.[ApprovedFlag]
,BA.[UpdateDT]
,@VersionID
FROM [dbo].[BOEApproval] BA
INNER JOIN dbo.BOE B ON BA.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOEComment]
([BOECommentID]
,[FieldID]
,[BOEComments]
,[BOECommentETIUserID]
,[BOEResponseToCommentID]
,[BOEID]
,[UpdateDT]
,[VersionID])
SELECT BC.[BOECommentID]
,BC.[FieldID]
,BC.[BOEComments]
,BC.[BOECommentETIUserID]
,BC.[BOEResponseToCommentID]
,BC.[BOEID]
,BC.[UpdateDT]
,@VersionID
FROM [dbo].[BOEComment] BC
INNER JOIN dbo.BOE B ON BC.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[CustomFieldValue]
([CustomFieldValueID]
,[CustomFieldValueName]
,[CustomFieldValueDescription]
,[CustomFieldID]
,[CustomFieldValueInUseFlag]
,[UpdateDT]
,[VersionID])
SELECT CFV.[CustomFieldValueID]
,CFV.[CustomFieldValueName]
,CFV.[CustomFieldValueDescription]
,CFV.[CustomFieldID]
,CFV.[CustomFieldValueInUseFlag]
,CFV.[UpdateDT]
,@VersionID
FROM [dbo].[CustomFieldValue] CFV
INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
INNER JOIN dbo.Workspace WS ON CF.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOETaskElement]
([BOETaskElementID]
,[TaskID]
,[TaskTitle]
,[TaskDescription]
,[TaskStartDate]
,[TaskEndDate]
,[MOQHoursEquation]
,[MOQCostEquation]
,[MOQText]
,[MOQTypeID]
,[BOEID]
,[LaborTypeWarningFlag]
,[IMS_ID]
,[TaskElementTypeID]
,[UpdateDT]
,[VersionID]
,[SortOrderID]
)
SELECT BTE.[BOETaskElementID]
,BTE.[TaskID]
,BTE.[TaskTitle]
,BTE.[TaskDescription]
,BTE.[TaskStartDate]
,BTE.[TaskEndDate]
,BTE.[MOQHoursEquation]
,BTE.[MOQCostEquation]
,BTE.[MOQText]
,BTE.[MOQTypeID]
,BTE.[BOEID]
,BTE.[LaborTypeWarningFlag]
,BTE.[IMS_ID]
,BTE.[TaskElementTypeID]
,BTE.[UpdateDT]
,@VersionID
,BTE.[SortOrderID]
FROM [dbo].[BOETaskElement] BTE
INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[OrdinaryVariable]
([OrdinaryVariableID]
,[OrdinaryVariableName]
,[OrdinaryVariableValue]
,[BOETaskElementID]
,[SortByID]
,[ValueTypeID]
,[IsPercentage]
,[UpdateDT]
,[VersionID]
,[DefaultSize]
)
SELECT OV.[OrdinaryVariableID]
,OV.[OrdinaryVariableName]
,OV.[OrdinaryVariableValue]
,OV.[BOETaskElementID]
,OV.[SortByID]
,OV.[ValueTypeID]
,OV.[IsPercentage]
,OV.[UpdateDT]
,@VersionID
,OV.[DefaultSize]
FROM [dbo].[OrdinaryVariable] OV
INNER JOIN [dbo].[BOETaskElement] BTE ON OV.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[OrdinaryVariableSumVariableResourceTypeXREF]
([OVSVRTID]
,[OrdinaryVariableID]
,[SumVariableResourceTypeID]
,[VersionID])
SELECT X.[OVSVRTID]
,X.[OrdinaryVariableID]
,X.[SumVariableResourceTypeID]
,@VersionID
FROM [dbo].[OrdinaryVariableSumVariableResourceTypeXREF] X
INNER JOIN [dbo].[OrdinaryVariable] OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
INNER JOIN [dbo].[BOETaskElement] BTE ON OV.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOEApprovalHistory]
([BOEApprovalHistoryID]
,[BOEID]
,[Approval]
,[ApprovalETIUserID]
,[UpdateDT]
,[VersionID])
SELECT BH.[BOEApprovalHistoryID]
,BH.[BOEID]
,BH.[Approval]
,BH.[ApprovalETIUserID]
,BH.[UpdateDT]
,@VersionID
FROM [dbo].[BOEApprovalHistory] BH
INNER JOIN dbo.BOE B ON BH.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[MOQTypeSelection]
([MOQTypeSelectionId],
[TaskId],
[MOQTypeSelection],
[UpdateDT],
[Order],
[CERName],
[HoursDescription],
[SubjectMatterExpert],
[HoursLogicAndAssumptions],
[DurationLogicAndAssumptions],
[EstimateTasks],
[Rationale],
[SkillMix],
[HistoricalReferenceExplanation],
[VersionId]
)
SELECT M.[MOQTypeSelectionId],
M.[TaskId],
M.[MOQTypeSelection],
M.[UpdateDT],
M.[Order],
M.[CERName],
M.[HoursDescription],
M.[SubjectMatterExpert],
M.[HoursLogicAndAssumptions],
M.[DurationLogicAndAssumptions],
M.[EstimateTasks],
M.[Rationale],
M.[SkillMix],
M.[HistoricalReferenceExplanation],
@VersionID
FROM [dbo].[MOQTypeSelection] M
INNER JOIN [dbo].[BOETaskElement] T ON M.TaskId = T.BOETaskElementID
INNER JOIN dbo.BOE B ON T.BOEID  = B.BOEID
INNER JOIN dbo.Workspace W ON B.WorkspaceID = W.WorkspaceID
WHERE W.WorkspaceID = @WorkspaceID

INSERT INTO [version].[MOQTypeSelectionTableData]
([MOQTypeSelectionTableDataId],
[MOQTypeSelectionId],
[UpdateDT],
[Order],
[TableName],
[RepositoryName],
[QueryType],
[DateOfReport],
[HistoricalProgramName],
[ContractNumber],
[WbsElement],
[PeriodOfPerformanceStartDate],
[PeriodOfPerformanceEndDate],
[TotalWbsHours],
[AdditionalQueryFilters],
[TotalRelevantHoursAfterQueryFilters],
[VersionId]
)
SELECT TD.[MOQTypeSelectionTableDataId],
TD.[MOQTypeSelectionId],
TD.[UpdateDT],
TD.[Order],
TD.[TableName],
TD.[RepositoryName],
TD.[QueryType],
TD.[DateOfReport],
TD.[HistoricalProgramName],
TD.[ContractNumber],
TD.[WbsElement],
TD.[PeriodOfPerformanceStartDate],
TD.[PeriodOfPerformanceEndDate],
TD.[TotalWbsHours],
TD.[AdditionalQueryFilters],
TD.[TotalRelevantHoursAfterQueryFilters],
@VersionID
FROM [dbo].[MOQTypeSelectionTableData] TD
INNER JOIN [dbo].[MOQTypeSelection] M ON TD.MOQTypeSelectionId = M.MOQTypeSelectionId
INNER JOIN [dbo].[BOETaskElement] T ON M.TaskId = T.BOETaskElementID
INNER JOIN dbo.BOE B ON T.BOEID  = B.BOEID
INNER JOIN dbo.Workspace W ON B.WorkspaceID = W.WorkspaceID
WHERE W.WorkspaceID = @WorkspaceID

/*Updated for WI 8398*/
INSERT INTO [version].[BOELaborType]
([BOELaborTypeID]
,[ResourceID]
,[PerformingOrganizationID]
,[BOELaborTypeStartDate]
,[BOELaborTypeEndDate]
,[SpreadCurveID]
,[PercentSpread]
,[ValueSpread]
,[BOETaskElementID]
,[SpreadTypeID]
,[UpdateDT]
,[VersionID]
,[PercentSpreadLocked]
,[HourSpreadLocked]
,[WBSID]
,[CLINID]
,[CanOffload]
,[LaborSortId]
)
SELECT BLT.[BOELaborTypeID]
,BLT.[ResourceID]
,BLT.[PerformingOrganizationID]
,BLT.[BOELaborTypeStartDate]
,BLT.[BOELaborTypeEndDate]
,BLT.[SpreadCurveID]
,BLT.[PercentSpread]
,BLT.[ValueSpread]
,BLT.[BOETaskElementID]
,BLT.[SpreadTypeID]
,BLT.[UpdateDT]
,@VersionID
,BLT.PercentSpreadLocked
,BLT.HourSpreadLocked
,BLT.WBSID
,BLT.CLINID
,BLT.[CanOffload]
,BLT.[LaborSortId]
FROM [dbo].[BOELaborType] BLT
INNER JOIN [dbo].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOETaskElementWorkspaceVariableXREF]
([BOETaskWSVarID]
,[BOETaskElementID]
,[WorkspaceVariableID]
,[VersionID])
SELECT X.[BOETaskWSVarID]
,X.[BOETaskElementID]
,X.[WorkspaceVariableID]
,@VersionID
FROM [dbo].[BOETaskElementWorkspaceVariableXREF] X
INNER JOIN [dbo].[BOETaskElement] BTE ON X.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOETaskElementMetricDetailXREF]
([BTEMDID]
,[BOETaskElementID]
,[MetricDetailID]
,[UpdateDT]
,[VersionID])
SELECT X.[BTEMDID]
,X.[BOETaskElementID]
,X.[MetricDetailID]
,X.[UpdateDT]
,@VersionID
FROM [dbo].[BOETaskElementMetricDetailXREF] X
INNER JOIN [dbo].[BOETaskElement] BTE ON X.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOETaskElementCustomFieldValueXREF]
([BTECFVID]
,[BOETaskElementID]
,[CustomFieldValueID]
,[UpdateDT]
,[VersionID])
SELECT X.[BTECFVID]
,X.[BOETaskElementID]
,X.[CustomFieldValueID]
,X.[UpdateDT]
,@VersionID
FROM [dbo].[BOETaskElementCustomFieldValueXREF] X
INNER JOIN [dbo].[BOETaskElement] BTE ON X.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOECustomFieldValueXREF]
([BCFVID]
,[BOEID]
,[CustomFieldValueID]
,[UpdateDT]
,[VersionID])
SELECT X.[BCFVID]
,X.[BOEID]
,X.[CustomFieldValueID]
,X.[UpdateDT]
,@VersionID
FROM [dbo].[BOECustomFieldValueXREF] X
INNER JOIN dbo.BOE B ON X.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[MoqTypeTableCustomFieldValueXREF] ([Id], [UpdateDT], [MoqTypeTableDataId], [CustomFieldValueId], [VersionID])
    SELECT x.[Id], x.[UpdateDT], x.[MoqTypeTableDataId], x.[CustomFieldValueId], @VersionID
	  FROM [dbo].[MoqTypeTableCustomFieldValueXREF] x, MoqTypeSelectionTableData t, MoqTypeSelection mS, BoeTaskElement tE, dbo.BOE B
	  WHERE 
		t.MoqTypeSelectionTableDataId = x.MoqTypeTableDataId AND mS.MoqTypeSelectionId = t.MoqTypeSelectionId 
		AND tE.BoeTaskElementId = mS.TaskId AND tE.BOEID = B.BOEID AND B.WorkspaceID = @WorkspaceID

INSERT INTO [version].[SumOfBOE_OrdinaryVariableXREF]
([OVSumID]
,[OrdinaryVariableID]
,[CLINID]
,[WBSID]
,[BOEID]
,[VersionID])
SELECT X.[OVSumID]
,X.[OrdinaryVariableID]
,X.[CLINID]
,X.[WBSID]
,X.[BOEID]
,@VersionID
FROM [dbo].[SumOfBOE_OrdinaryVariableXREF] X
INNER JOIN [dbo].[OrdinaryVariable] OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
INNER JOIN [dbo].[BOETaskElement] BTE ON OV.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOELaborTypeCustomFieldValueXREF]
([BLTCFVID]
,[BOELaborTypeID]
,[CustomFieldValueID]
,[UpdateDT]
,[VersionID])
SELECT X.[BLTCFVID]
,X.[BOELaborTypeID]
,X.[CustomFieldValueID]
,X.[UpdateDT]
,@VersionID
FROM [dbo].[BOELaborTypeCustomFieldValueXREF] X 
INNER JOIN [dbo].[BOELaborType] BLT ON X.BOELaborTypeID = BLT.BOELaborTypeID
INNER JOIN [dbo].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOELaborSpread]
([BOELaborSpreadID]
,[BOELaborTypeID]
,[LaborSpreadDate]
,[LaborSpreadValue]
,[VersionID])
SELECT LS.[BOELaborSpreadID]
,LS.[BOELaborTypeID]
,LS.[LaborSpreadDate]
,LS.[LaborSpreadValue]
,@VersionID
FROM [dbo].[BOELaborSpread] LS
INNER JOIN [dbo].[BOELaborType] BLT ON LS.BOELaborTypeID = BLT.BOELaborTypeID
INNER JOIN [dbo].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[ODCTaskElement]
	     ([ODCTaskElementID]
	     ,[ODCTaskTitle]
	     ,[ODCTaskDescription]
	     ,[ODCMOQText]
	     ,[BOEID]
	     ,[ODCTaskID]
	     ,[UpdateDT]
	     ,[VersionID]
	     ,[TaskStartDate]
	     ,[TaskEndDate]
		   ,[SortOrderID]
	     )
SELECT OTE.[ODCTaskElementID]
	,OTE.[ODCTaskTitle]
	,OTE.[ODCTaskDescription]
	,OTE.[ODCMOQText]
	,OTE.[BOEID]
	,OTE.[ODCTaskID]
	,OTE.[UpdateDT]
	,@VersionID
	,OTE.[TaskStartDate]
	,OTE.[TaskEndDate]
	  ,OTE.[SortOrderID]
  FROM [dbo].[ODCTaskElement] OTE
	INNER JOIN dbo.BOE B ON OTE.BOEID = B.BOEID
	INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[ODCType]
	     ([ODCTypeID]
		   ,[ResourceID]
	     ,[PerformingOrganizationID]
	     ,[ODCTypeStartDate]
	     ,[ODCTypeEndDate]
	     ,[SpreadCurveID]
	     ,[ODCTypeCost]
	     ,[ODCTaskElementID]
	     ,[UpdateDT]
	     ,[VersionID])
SELECT OT.[ODCTypeID]
	,OT.[ResourceID]
	,OT.[PerformingOrganizationID]
	,OT.[ODCTypeStartDate]
	,OT.[ODCTypeEndDate]
	,OT.[SpreadCurveID]
	,OT.[ODCTypeCost]
	,OT.[ODCTaskElementID]
	,OT.[UpdateDT]
	,@VersionID
 FROM [dbo].[ODCType] OT 
	INNER JOIN dbo.ODCTaskElement OTE ON OT.ODCTaskElementID = OTE.ODCTaskElementID
	INNER JOIN dbo.BOE B ON OTE.BOEID = B.BOEID
	INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[ODCSpread]
	     ([ODCSpreadID]
		   ,[ODCSpreadDate]
	     ,[ODCSpreadValue]
	     ,[ODCTypeID]
	     ,[VersionID])
SELECT OS.[ODCSpreadID]
	,OS.[ODCSpreadDate]
	,OS.[ODCSpreadValue]
	,OS.[ODCTypeID]
	,@VersionID
FROM [dbo].[ODCSpread] OS
	INNER JOIN dbo.ODCType OT ON OS.ODCTypeID = OT.ODCTypeID
	INNER JOIN dbo.ODCTaskElement OTE ON OT.ODCTaskElementID = OTE.ODCTaskElementID
	INNER JOIN dbo.BOE B ON OTE.BOEID = B.BOEID
	INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[MaterialTaskElement]
	     ([MaterialTaskElementID]
	     ,[MaterialTaskID]
	     ,[MaterialTaskTitle]
	     ,[MaterialTaskDescription]
	     ,[MaterialMOQText]
	     ,[BOEID]
	     ,[UpdateDT]
	     ,[VersionID])
SELECT TE.[MaterialTaskElementID]
	,TE.[MaterialTaskID]
	,TE.[MaterialTaskTitle]
	,TE.[MaterialTaskDescription]
	,TE.[MaterialMOQText]
	,TE.[BOEID]
	,TE.[UpdateDT]
	,@VersionID
  FROM [dbo].[MaterialTaskElement] TE
	INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
	INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID
	
INSERT INTO [version].[PerDiem]
	     ([PerDiemID]
		   ,[UpdateDT]
	     ,[PerDiemDestination]
	     ,[Qualification]
	     ,[HotelRate]
	     ,[MIERate]
	     ,[PerDiemNotes]
	     /*,[RentalCarRate]*/
	     ,[PerDiemLastUpdateETIUserID]
	     ,[PerDiemLastUpdateDT]
	     /*WI8256,[LockedRate]*/
	     ,[VersionID])
SELECT PD.[PerDiemID]
	,PD.[UpdateDT]
	,PD.[PerDiemDestination]
	,PD.[Qualification]
	,PD.[HotelRate]
	,PD.[MIERate]
	,PD.[PerDiemNotes]
	/*,PD.[RentalCarRate]*/
	,PD.[PerDiemLastUpdateETIUserID]
	,PD.[PerDiemLastUpdateDT]
	/*WI8256,PD.[LockedRate]*/
	,@VersionID
  FROM [dbo].[PerDiem] PD
	INNER JOIN [dbo].[Trip] T ON PD.PerDiemID = T.PerDiemID
	INNER JOIN [dbo].[TravelTrip] TT ON T.TripID = TT.TripID
	INNER JOIN [dbo].[TravelTripTaskElement] TE ON TT.TravelTripTaskElementID = TE.TravelTripTaskElementID
	INNER JOIN [dbo].[BOE] B ON TE.BOEID = B.BOEID
	INNER JOIN [dbo].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

--WI 4881
INSERT INTO [version].[TravelMiscRate]
	     ([TravelMiscRateID]
	     ,[UpdateDT]
	     ,[TransportationMode]
	     ,[MiscellaneousRate]
	     ,[SortCode]
	     ,[MiscRateInUse]
/*WI8256           ,[LockedRate]*/
	     ,[VersionID])
SELECT MR.[TravelMiscRateID]
	,MR.[UpdateDT]
	,MR.[TransportationMode]
	,MR.[MiscellaneousRate]
	,MR.[SortCode]
	,MR.[MiscRateInUse]
/*WI8256      ,MR.[LockedRate]*/
	,@VersionID
  FROM [dbo].[TravelMiscRate] MR
	INNER JOIN [dbo].[Trip] T ON MR.TravelMiscRateID = T.TravelMiscRateID
	INNER JOIN [dbo].[TravelTrip] TT ON T.TripID = TT.TripID
	INNER JOIN [dbo].[TravelTripTaskElement] TE ON TT.TravelTripTaskElementID = TE.TravelTripTaskElementID
	INNER JOIN [dbo].[BOE] B ON TE.BOEID = B.BOEID
	INNER JOIN [dbo].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

/*WI6261*/
INSERT INTO [version].[Trip]
	     ([TripID]
	     ,[UpdateDT]
	     ,[TravelMiscRateID]
	     ,[DepartureLocationID]
	     ,[DestinationLocationID]
	     ,[PerDiemID]
	     ,[TransportationFare]
	     ,[RoundTripMiles]
	     ,[FareLastUpdateDT]
	     ,[FareLastUpdateETIUserID]
	     ,[TripInUse]
	     ,[LastUsedDT]
/*WI8256           ,[LockedRate]*/
	     ,[VersionID]
/*WI8256           ,[Year]
	     ,[DevEscalation]
	     ,[LMSIEscalation]*/
	     ,[RentalCarRate]
	     ,[DestinationLocationCode]
	     ,[DepartureLocationCode]
	     )
SELECT T.[TripID]
	,T.[UpdateDT]
	,T.[TravelMiscRateID]
	,T.[DepartureLocationID]
	,T.[DestinationLocationID]
	,T.[PerDiemID]
	,T.[TransportationFare]
	,T.[RoundTripMiles]
	,T.[FareLastUpdateDT]
	,T.[FareLastUpdateETIUserID]
	,T.[TripInUse]
	,T.[LastUsedDT]
/*WI8256      ,T.[LockedRate]*/
	,@VersionID
/*WI8256      ,T.[Year]
	,T.[DevEscalation]
	,T.[LMSIEscalation]*/
	,T.[RentalCarRate]
	,T.[DestinationLocationCode]
	,T.[DepartureLocationCode]
FROM [dbo].[Trip] T 
	INNER JOIN [dbo].[TravelTrip] TT ON T.TripID = TT.TripID
	INNER JOIN [dbo].[TravelTripTaskElement] TE ON TT.TravelTripTaskElementID = TE.TravelTripTaskElementID
	INNER JOIN [dbo].[BOE] B ON TE.BOEID = B.BOEID
	INNER JOIN [dbo].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[TravelTrip]
	     ([TravelTripID]
	     ,[UpdateDT]
	     ,[GroupID]
	     ,[SegmentID]
	     ,[PerformingOrganizationID]
	     ,[TripID]
	     ,[TripDate]
	     ,[NumTrips]
	     ,[NumPeople]
	     ,[NumDays]
	     ,[Purpose]           
	     ,[TravelTripTaskElementID]
	     ,[TripLockedDT]
 /*WI8256          ,[OriginatingTripID]*/
	     ,[VersionID])
SELECT TT.[TravelTripID]
	,TT.[UpdateDT]
	,TT.[GroupID]
	,TT.[SegmentID]
	,TT.[PerformingOrganizationID]
	,TT.[TripID]
	,TT.[TripDate]
	,TT.[NumTrips]
	,TT.[NumPeople]
	,TT.[NumDays]
	,TT.[Purpose]      
	,TT.[TravelTripTaskElementID]
	,TT.[TripLockedDT]
 /*WI8256     ,TT.[OriginatingTripID]*/
	,@VersionID
  FROM [dbo].[TravelTrip] TT
	INNER JOIN [dbo].[TravelTripTaskElement] TE ON TT.TravelTripTaskElementID = TE.TravelTripTaskElementID
	INNER JOIN [dbo].[BOE] B ON TE.BOEID = B.BOEID
	INNER JOIN [dbo].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[TravelTripTaskElement]
	     ([TravelTripTaskElementID]
	     ,[UpdateDT]
	     ,[TravelTaskID]
	     ,[TravelTaskTitle]
	     ,[TravelTaskDescription]
	     ,[BOEID]
	     ,[VersionID]
	     ,[TaskStartDate]
	     ,[TaskEndDate]
		   ,[SortOrderID]
	     )
SELECT TE.[TravelTripTaskElementID]
	,TE.[UpdateDT]
	,TE.[TravelTaskID]
	,TE.[TravelTaskTitle]
	,TE.[TravelTaskDescription]
	,TE.[BOEID]
	,@VersionID
	,TE.[TaskStartDate]
	,TE.[TaskEndDate]
	  ,TE.[SortOrderID]
  FROM [dbo].[TravelTripTaskElement] TE 
	INNER JOIN [dbo].[BOE] B ON TE.BOEID = B.BOEID
	INNER JOIN [dbo].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[TravelTripTaskElementCustomFieldValueXREF]
	     ([TTECFVID]
	     ,[TravelTripTaskElementID]
	     ,[CustomFieldValueID]
	     ,[UpdateDT]
	     ,[VersionID])
SELECT X.[TTECFVID]
	,X.[TravelTripTaskElementID]
	,X.[CustomFieldValueID]
	,X.[UpdateDT]
	,@VersionID
FROM [dbo].[TravelTripTaskElementCustomFieldValueXREF] X
	INNER JOIN dbo.TravelTripTaskElement TE ON X.TravelTripTaskElementID = TE.TravelTripTaskElementID
	INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
	INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[TravelTripCustomFieldValueXREF]
	     ([TCFVID]
	     ,[TravelTripID]
	     ,[CustomFieldValueID]
	     ,[UpdateDT]
	     ,[VersionID])
SELECT X.[TCFVID]
	,X.[TravelTripID]
	,X.[CustomFieldValueID]
	,X.[UpdateDT]
	,@VersionID
  FROM [dbo].[TravelTripCustomFieldValueXREF] X
	INNER JOIN dbo.TravelTrip M ON X.TravelTripID = M.TravelTripID
	INNER JOIN dbo.TravelTripTaskElement TE ON M.TravelTripTaskElementID = TE.TravelTripTaskElementID
	INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
	INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

-- Start of "RMS Zone Travel"
INSERT INTO [version].[MSTTravelTrip]
		([MSTTravelTripID]
		,[ModeID]
		,[TravelTripTaskElementID]
		,[UpdateDT]
		,[GroupID]
		,[SegmentID]
		,[Purpose]
		,[PerformingOrganizationID]
		,[TripDate]
		,[EstimateDate]
		,[NumPeople]
		,[NumDays]
		,[ZoneOriginID]
		,[ZoneDestCity]
		,[ZoneDestinationID]
		,[ZoneResourceID]
		,[NonZoneFrom]
		,[NonZoneTo]
		,[NonZoneAirFareEstimate]
		,[NonZonePerDiemDaily]
		,[NonZoneCarRentalTrans]
		,[NonZoneNumCars]
		,[NonZoneResourceID]
		,[ClinId]
		,[WbsId]
	  ,[VersionID]
		)
	SELECT 
		tt.[MSTTravelTripID]
		,tt.[ModeID]
		,tt.[TravelTripTaskElementID]
		,tt.[UpdateDT]
		,tt.[GroupID]
		,tt.[SegmentID]
		,tt.[Purpose]
		,tt.[PerformingOrganizationID]
		,tt.[TripDate]
		,tt.[EstimateDate]
		,tt.[NumPeople]
		,tt.[NumDays]
		,tt.[ZoneOriginID]
		,tt.[ZoneDestCity]
		,tt.[ZoneDestinationID]
		,tt.[ZoneResourceID]
		,tt.[NonZoneFrom]
		,tt.[NonZoneTo]
		,tt.[NonZoneAirFareEstimate]
		,tt.[NonZonePerDiemDaily]
		,tt.[NonZoneCarRentalTrans]
		,tt.[NonZoneNumCars]
		,tt.[NonZoneResourceID]
		,tt.[ClinId]
		,tt.[WbsId]
		,@VersionID
		FROM [dbo].[MSTTravelTrip] tt 
			INNER JOIN [dbo].TravelTripTaskElement TE ON tt.TravelTripTaskElementID = TE.TravelTripTaskElementID
			INNER JOIN [dbo].BOE B ON TE.BOEID = B.BOEID
			INNER JOIN [dbo].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE
			WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[MSTTravelTripCustomFieldValueXREF]
			([MSTTCFVID]
			,[MSTTravelTripID]
			,[MSTCustomFieldValueID]
			,[UpdateDT]
			,[VersionID])
		SELECT 
			cI.[MSTTCFVID]
			,cI.[MSTTravelTripID]
			,cI.[MSTCustomFieldValueID]
			,cI.[UpdateDT]
			,@VersionID
		FROM [dbo].[MSTTravelTripCustomFieldValueXREF] cI 
			INNER JOIN [dbo].MSTTravelTrip tt ON cI.MSTTravelTripID = tt.MSTTravelTripID
			INNER JOIN [dbo].TravelTripTaskElement TE ON tt.TravelTripTaskElementID = TE.TravelTripTaskElementID
			INNER JOIN [dbo].BOE B ON TE.BOEID = B.BOEID
			INNER JOIN [dbo].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE
			WS.WorkspaceID = @WorkspaceID
-- End of "RMS Zone Travel"

/*LOCKING TABLES*/
INSERT INTO [version].[WorkspaceLockedPerDiem]
	     ([WorkspaceLockedPerDiemID]
	     ,[PerDiemID]
	     ,[UpdateDT]
	     ,[PerDiemDestination]
	     ,[Qualification]
	     ,[HotelRate]
	     ,[MIERate]
	     ,[PerDiemNotes]
	     ,[PerDiemLastUpdateETIUserID]
	     ,[PerDiemLastUpdateDT]
	     ,[WorkspaceID]
	     ,[VersionID])
SELECT L.[WorkspaceLockedPerDiemID]
	,L.[PerDiemID]
	,L.[UpdateDT]
	,L.[PerDiemDestination]
	,L.[Qualification]
	,L.[HotelRate]
	,L.[MIERate]
	,L.[PerDiemNotes]
	,L.[PerDiemLastUpdateETIUserID]
	,L.[PerDiemLastUpdateDT]
	,L.[WorkspaceID]
	,@VersionID
  FROM [dbo].[WorkspaceLockedPerDiem] L
	INNER JOIN dbo.Workspace WS ON L.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[WorkspaceLockedTravelEscalationRate]
	     ([WorkspaceLockedTravelEscalationRateID]
	     ,[TravelEscalationRateID]
	     ,[UpdateDT]
	     ,[Year]
	     ,[DevEscalation]
	     ,[LMSIEscalation]
		   ,[MiscRate]
	     ,[WorkspaceID]
	     ,[VersionID])
SELECT L.[WorkspaceLockedTravelEscalationRateID]
	,L.[TravelEscalationRateID]
	,L.[UpdateDT]
	,L.[Year]
	,L.[DevEscalation]
	,L.[LMSIEscalation]
	  ,L.[MiscRate]
	,L.[WorkspaceID]
	,@VersionID
  FROM [dbo].[WorkspaceLockedTravelEscalationRate] L
	INNER JOIN dbo.Workspace WS ON L.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[WorkspaceLockedTravelMiscRate]
	     ([WorkspaceLockedTravelMiscRateID]
	     ,[TravelMiscRateID]
	     ,[UpdateDT]
	     ,[TransportationMode]
	     ,[MiscellaneousRate]
	     ,[SortCode]
	     ,[MiscRateInUse]
	     ,[WorkspaceID]
	     ,[VersionID])
SELECT L.[WorkspaceLockedTravelMiscRateID]
	,L.[TravelMiscRateID]
	,L.[UpdateDT]
	,L.[TransportationMode]
	,L.[MiscellaneousRate]
	,L.[SortCode]
	,L.[MiscRateInUse]
	,L.[WorkspaceID]
	,@VersionID      
  FROM [dbo].[WorkspaceLockedTravelMiscRate] L
	INNER JOIN dbo.Workspace WS ON L.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[WorkspaceLockedTrip]
	     ([WorkspaceLockedTripID]
	     ,[TripID]
	     ,[UpdateDT]
	     ,[TravelMiscRateID]
	     ,[DepartureLocationID]
	     ,[DestinationLocationID]
	     ,[PerDiemID]
	     ,[TransportationFare]
	     ,[RoundTripMiles]
	     ,[FareLastUpdateETIUserID]
	     ,[FareLastUpdateDT]
	     ,[TripInUse]
	     ,[LastUsedDT]
	     ,[RentalCarRate]
	     ,[DepartureLocationCode]
	     ,[DestinationLocationCode]
	     ,[WorkspaceID]
	     ,[VersionID])
SELECT L.[WorkspaceLockedTripID]
	,L.[TripID]
	,L.[UpdateDT]
	,L.[TravelMiscRateID]
	,L.[DepartureLocationID]
	,L.[DestinationLocationID]
	,L.[PerDiemID]
	,L.[TransportationFare]
	,L.[RoundTripMiles]
	,L.[FareLastUpdateETIUserID]
	,L.[FareLastUpdateDT]
	,L.[TripInUse]
	,L.[LastUsedDT]
	,L.[RentalCarRate]
	,L.[DepartureLocationCode]
	,L.[DestinationLocationCode]
	,L.[WorkspaceID]
	,@VersionID
  FROM  [dbo].[WorkspaceLockedTrip] L
	INNER JOIN dbo.Workspace WS ON L.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

/* Copy New INL Forms */
INSERT INTO [version].[BOEFormIBOE]
([IBOEFormID], UpdateDT, WorkspaceID, FormName, Description, [BasisAndRationale], [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, BusinessArea, Revision,FormVersion, VersionId)
SELECT [IBOEFormID], UpdateDT, WorkspaceID, FormName, Description, [BasisAndRationale], [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, BusinessArea, Revision,FormVersion, @VersionID
  FROM [dbo].[BOEFormIBOE]
WHERE WorkspaceID = @WorkspaceID 

INSERT INTO [version].[BOEFormIBOEResourcesXREF]
SELECT x.[IBOEFormID]
	  ,[ResourceID]
	  , @VersionID
FROM [dbo].[BOEFormIBOEResourcesXREF] x
	INNER JOIN [dbo].[BOEFormIBOE] B ON B.[IBOEFormID] = x.[IBOEFormID]
WHERE B.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOEFormIBOECLINsXREF]
SELECT x.[IBOEFormID]
	  ,[ClinID]
	  ,[ContractType]
	  ,@VersionID
FROM [dbo].[BOEFormIBOECLINsXREF] x
	INNER JOIN [dbo].[BOEFormIBOE] B ON B.[IBOEFormID] = x.[IBOEFormID]
WHERE B.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOEFormPBOE]
([PBOEFormID], UpdateDT, WorkspaceID, FormName, Description, [BasisAndRationale], [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, Revision,FormVersion,  
	[DegreeOfCompetition],	[CCoPD],	[CCoPDOtherText],	[RFP],	[ProposalNumber],	[SupplierName],	[ValidityDate],		[SupplierProposalSupportingDataIncluded],	[PriceAnalysisIncluded],	[CommercialItemDocIncluded],
	[CostAnalysisIncluded],	[ShouldCostEstimate],	[ShouldCostEstimateDate],	[SowWritten],	[SowWrittenDate],	[RFPRelease],	[RFPReleaseDate],	[FirmSupplierReceipt],	[FirmSupplierReceiptDate],	[SourceSelection],
	[SourceSelectionDate],	[CID],	[CIDDate],	[GovtReview],	[GovtReviewDate],	[PriceAnalysis],	[PriceAnalysisDate],	[TechnicalEvaluation],	[TechnicalEvaluationDate],	[FactFinding],	[FactFindingDate],
	[CostAnalysis],	[CostAnalysisDate],	[GovtPricing],	[GovtPricingDate],	[SupplierNegotiations],	[SupplierNegotiationsDate],	[MOU],	[MOUDate],	[Procurement],	[ProcurementDate],
	[PlannedDate_WrittenApproval],	[PlannedDate_ApprovedSubmission], VersionId, [CIDText],
	[GovtReviewText],
	[PriceAnalysisText],
	[TechnicalEvaluationText],
	[FactFindingText],
	[CostAnalysisText],
	[GovtPricingText],
	[SupplierNegotiationsText],
	[MOUText],
	[ProcurementText],
	ShouldCostEstimateText,
	SowWrittenText,
	RFPReleaseText,
	FirmSupplierReceiptText,
	SourceSelectionText
	,[SupplierCCoPD]
	,[SourceSelectionDescription]
	,[CommercialityDescription]
	,[TechnicalEvaluationDescription]
	,[PriceAnalysisDescription]
	,[CostAnalysisDescription]
	,[RationaleValueSummary]
	,[GovtPricingReceived]
	,[GovtPricingReceivedDate]
	,[GovtPricingReceivedText]
	,[CostAnalysisUnqual]
	,[CostAnalysisUnqualDate]
	,[CostAnalysisUnqualText]
	,[VendorId]
	,[SupplierProposedValue])
SELECT [PBOEFormID], UpdateDT, WorkspaceID, FormName, Description, [BasisAndRationale], [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, Revision,FormVersion, [DegreeOfCompetition],
	[CCoPD],	[CCoPDOtherText],	[RFP],	[ProposalNumber],	[SupplierName],	[ValidityDate],	[SupplierProposalSupportingDataIncluded],	[PriceAnalysisIncluded],	[CommercialItemDocIncluded],
	[CostAnalysisIncluded],	[ShouldCostEstimate],	[ShouldCostEstimateDate],	[SowWritten],	[SowWrittenDate],	[RFPRelease],	[RFPReleaseDate],	[FirmSupplierReceipt],	[FirmSupplierReceiptDate],
	[SourceSelection],	[SourceSelectionDate],	[CID],	[CIDDate],	[GovtReview],	[GovtReviewDate],	[PriceAnalysis],	[PriceAnalysisDate],	[TechnicalEvaluation],	[TechnicalEvaluationDate],
	[FactFinding],	[FactFindingDate],	[CostAnalysis],	[CostAnalysisDate],	[GovtPricing],	[GovtPricingDate],	[SupplierNegotiations],	[SupplierNegotiationsDate],	[MOU],
	[MOUDate],	[Procurement],	[ProcurementDate],	[PlannedDate_WrittenApproval],	[PlannedDate_ApprovedSubmission], @VersionID, [CIDText],
	[GovtReviewText],
	[PriceAnalysisText],
	[TechnicalEvaluationText],
	[FactFindingText],
	[CostAnalysisText],
	[GovtPricingText],
	[SupplierNegotiationsText],
	[MOUText],
	[ProcurementText],
	ShouldCostEstimateText,
	SowWrittenText,
	RFPReleaseText,
	FirmSupplierReceiptText,
	SourceSelectionText
	,[SupplierCCoPD]
	,[SourceSelectionDescription]
	,[CommercialityDescription]
	,[TechnicalEvaluationDescription]
	,[PriceAnalysisDescription]
	,[CostAnalysisDescription]
	,[RationaleValueSummary]
	,[GovtPricingReceived]
	,[GovtPricingReceivedDate]
	,[GovtPricingReceivedText]
	,[CostAnalysisUnqual]
	,[CostAnalysisUnqualDate]
	,[CostAnalysisUnqualText]
	,[VendorId]
	,[SupplierProposedValue]
  FROM [dbo].[BOEFormPBOE]
WHERE WorkspaceID = @WorkspaceID 

INSERT INTO [version].[BOEFormPBOEResourcesXREF]
SELECT x.[PBOEFormID]
	  ,[ResourceID]
	  ,@VersionID
FROM [dbo].[BOEFormPBOEResourcesXREF] x
	INNER JOIN [dbo].[BOEFormPBOE] B ON B.[PBOEFormID] = x.[PBOEFormID]
WHERE B.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOEFormPBOECLINsXREF]
SELECT x.[PBOEFormID]
	  ,[ClinID]
	  ,[ContractType]
	  ,@VersionID
FROM [dbo].[BOEFormPBOECLINsXREF] x
	INNER JOIN [dbo].[BOEFormPBOE] B ON B.[PBOEFormID] = x.[PBOEFormID]
WHERE B.WorkspaceID = @WorkspaceID

INSERT INTO [version].[WorkspaceRMSTravelEscalationRate] ([TravelEscalationRateID], UpdateDt, WorkspaceId, Year, Escalation, VersionId, MiscRate, PerDiemRate)
SELECT [TravelEscalationRateID]
		,[UpdateDT]
		,[WorkspaceID]
		,[Year]
		,[Escalation]
		,@VersionID
		,[MiscRate]
		,[PerDiemRate]
FROM [dbo].[WorkspaceRMSTravelEscalationRate] w
WHERE w.WorkspaceID = @WorkspaceID

INSERT INTO [version].[WorkspaceRMSTravelNonzoneFeesAndCosts]
SELECT FeesAndCostsID
		,[UpdateDT]
		,[WorkspaceID]
		,ModeID
		,TravelAgencyFee
		,MiscOther
		,@VersionID
FROM [dbo].[WorkspaceRMSTravelNonzoneFeesAndCosts] w
WHERE w.WorkspaceID = @WorkspaceID
/*
IF @@ERROR = 0
	BEGIN
		COMMIT TRANSACTION
	END
ELSE
	BEGIN
		ROLLBACK TRANSACTION
	END
*/

IF LEFT (@VersionName,  8) <> '[SYSTEM:' AND LEFT(@VersionName, 8) <> '[SYS:RPC'
	SELECT ID AS VersionID FROM @Inserted

GO