IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertOrGetUser]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertOrGetUser]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RealignWorkspace]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[RealignWorkspace]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RealignWorkspacesJob]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[RealignWorkspacesJob]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IsgsPerfOrgXREF]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[IsgsPerfOrgXREF](
	[PerfOrgTransID] [int] IDENTITY(1,1) NOT NULL,
	[ISGSPerfOrgID] [int] NOT NULL,
	[NewPerfOrgID] [int] NOT NULL,
 CONSTRAINT [PK_IsgsPerfOrgXREF] PRIMARY KEY CLUSTERED 
(
	[PerfOrgTransID] ASC
)WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IsgsResourceXREF]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[IsgsResourceXREF](
	[ResTransID] [int] IDENTITY(1,1) NOT NULL,
	[ISGSResourceID] [int] NOT NULL,
	[NewResourceID] [int] NOT NULL,
 CONSTRAINT [PK_IsgsResourceXREF] PRIMARY KEY CLUSTERED 
(
	[ResTransID] ASC
)WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RealignedWorkspaceXREF]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[RealignedWorkspaceXREF](
	[RequestID] [int] IDENTITY(1,1) NOT NULL,
	[IsgsWorkspaceID] [int] NOT NULL,
	[NewWorkspaceID] [int] NULL,
	[UpdateDT] [datetime2](7) NULL,
 CONSTRAINT [PK_RealignedWorkspaceXREF] PRIMARY KEY CLUSTERED 
(
	[RequestID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

EXEC dbo.sp_executesql @statement = N'
CREATE PROCEDURE [dbo].[insertOrGetUser]
(
@isgsUserId int,
@UserInCurrentDb int OUTPUT
)
AS
/******************************************************************************
**		 
**		Name: [insertOrGetUser]
**		Desc: Given an ISGS user ID, the procedure finds & returns equivalent user
**			ID from the current db. If the user does not exist, it creates it
**			and returns the new ID.
**		This is for IS&GS realignment purposes only.
**
**		Auth: Michael Basquill
**		Date: 2/2/2016
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------

*******************************************************************************/

SET NOCOUNT ON 
DECLARE @UpdateDT datetime2  = GetDate()

SELECT @UserInCurrentDb = NULL

SELECT @UserInCurrentDb = ETIUserID
							from ETIuser
							where NTID = (SELECT NTID 
											FROM GenBOE.DBO.ETIuser
											WHERE ETIUserID = @isgsUserId)

							
If @UserInCurrentDb IS Null
	BEGIN
		INSERT into ETIuser(UpdateDT, NTID, NTDomain, DisplayName, EmailAddress, PhoneNumber, FirstName, LastName)
		SELECT @UpdateDT, NTID, NTDomain, DisplayName, EmailAddress, PhoneNumber, FirstName, LastName
		FROM GenBOE.dbo.ETIuser
		WHERE ETIUserID = @isgsUserId

		SELECT @UserInCurrentDb = SCOPE_IDENTITY()
	END
 ' 
GO
EXEC dbo.sp_executesql @statement = N'
CREATE PROCEDURE [dbo].[RealignWorkspace]
	@WorkspaceID int
AS
/******************************************************************************
**		 
**		Name: [RealignWorkspace]
**		Desc: 
**				Copies a Workspace from ISGS to MST or SSC. Does not include
**		Travel Trips, Rates, or DTS. This SP should not be run directly, but
**		instead via the SP dbo.RealignWorkspacesJob.
**		
**		This script will only run on genBOESpace and genBOEMST. So, it will
**		only work in the Dev, Test (main), and Production environments.
**
**		Auth: Michael Basquill
**		Date: 2/16/2016
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		2/16/16		mbasquil			Original Version
**		1/26/17		pattoncr			Updating MOQHoursEquation to varchar(500).
*******************************************************************************/
SET NOCOUNT ON;

BEGIN TRANSACTION

BEGIN TRY



DECLARE @WorkspaceNamePrefix varchar (5) = ''ISGS ''
DECLARE @WorkspaceShortNamePrefix varchar(5) = ''isgs_''
DECLARE @MasterTemplateID int
DECLARE @TransitionedSegmentID int
DECLARE @ProposalTypeID int
DECLARE @NewWorkspaceID int

--SET COMPANY SPECIFIC DEFAULTS
declare @CurrentDb varchar(30)
select @CurrentDb = db_name()

IF (@CurrentDb = ''GenBOESpace'')
BEGIN
	SELECT @MasterTemplateID = 9001
	SELECT @TransitionedSegmentID = 1001
	SELECT @ProposalTypeID = 1001
END
ELSE IF (@CurrentDb = ''GenBOEMST'')
BEGIN
	SELECT @MasterTemplateID = 9001
	SELECT @TransitionedSegmentID = 2001
	SELECT @ProposalTypeID = 2001
END
ELSE
BEGIN
	PRINT ''ERROR: THIS CAN ONLY RUN IN GenBOESpace OR GenBOEMST''
	Return
END
--STOP COMPANY SPECIFIC DEFAULTS


 --START SETUP USERS
DECLARE @UserXREF TABLE
(
	[IsgsUserID] [int] NOT NULL,
	[NewUserID] [int] NULL,
	Processed bit DEFAULT 0
)


--Find all the users that are used in various tables related to the workspace to copy
INSERT INTO @UserXREF
(
[IsgsUserID]
)
(
select distinct CostVolumeLeadPricerUserID as ETIUserID
from [GenBOE].[dbo].[Workspace]
where WorkspaceID = @WorkspaceID
and ISNULL(CostVolumeLeadPricerUserID, 0) != 0
union
select distinct CreatedByETIUserID as ETIUserID
from [GenBOE].[dbo].[Workspace]
where WorkspaceID = @WorkspaceID
and ISNULL(CreatedByETIUserID, 0) != 0
union
select distinct ETIUserID as ETIUserID
from [GenBOE].[dbo].[BOEPotentialRole]
where WorkspaceID = @WorkspaceID
and ISNULL(ETIUserID, 0) != 0
union
select distinct ChangedByETIUserID as ETIUserID
from [GenBOE].[dbo].[WorkspaceStateHistory]
where WorkspaceID = @WorkspaceID
and ISNULL(ChangedByETIUserID, 0) != 0
union
select distinct ETIUserID as ETIUserID
from [GenBOE].[dbo].[WorkspaceUserRole]
where WorkspaceID = @WorkspaceID
and ISNULL(ETIUserID, 0) != 0
union
select distinct ChangedByETIUserID as ETIUserID
from [GenBOE].[dbo].[BOEStateHistory] bh, [GenBOE].[dbo].[BOE] b
where b.WorkspaceID = @WorkspaceID
and bh.boeid = b.boeid
and ISNULL(bh.ChangedByETIUserID, 0) != 0
union
select distinct CurrentETIUserID as ETIUserID
from [GenBOE].[dbo].[BOEUserRoleHistory]bh, [GenBOE].[dbo].[BOE] b
where b.WorkspaceID = @WorkspaceID
and bh.boeid = b.boeid
and ISNULL(bh.CurrentETIUserID, 0) != 0
union
select distinct UpdatedETIUserID as ETIUserID
from [GenBOE].[dbo].[BOEUserRoleHistory]bh, [GenBOE].[dbo].[BOE] b
where b.WorkspaceID = @WorkspaceID
and bh.boeid = b.boeid
and ISNULL(bh.UpdatedETIUserID, 0) != 0
union
select distinct ChangedByETIUserID as ETIUserID
from [GenBOE].[dbo].[BOEUserRoleHistory] bh, [GenBOE].[dbo].[BOE] b
where b.WorkspaceID = @WorkspaceID
and bh.boeid = b.boeid
and ISNULL(bh.ChangedByETIUserID, 0) != 0
union
select distinct ApprovalETIUserID as ETIUserID
from [GenBOE].[dbo].[BOEApproval] ba, [GenBOE].[dbo].[BOE] b
where b.WorkspaceID = @WorkspaceID
and ba.boeid = b.boeid
and ISNULL(ba.ApprovalETIUserID, 0) != 0
union
select distinct BOECommentETIUserID as ETIUserID
from [GenBOE].[dbo].[BOEComment] bc, [GenBOE].[dbo].[BOE] b
where b.WorkspaceID = @WorkspaceID
and bc.boeid = b.boeid
and ISNULL(bc.BOECommentETIUserID, 0) != 0
union
select distinct ChangedByETIUserID as ETIUserID
from [GenBOE].[dbo].[BOECommentHistory] bh, [GenBOE].[dbo].[BOE] b
where b.WorkspaceID = @WorkspaceID
and bh.boeid = b.boeid
and ISNULL(bh.ChangedByETIUserID, 0) != 0
union
select distinct ApprovalETIUserID as ETIUserID
from [GenBOE].[dbo].[BOEApprovalHistory] bh, [GenBOE].[dbo].[BOE] b
where b.WorkspaceID = @WorkspaceID
and bh.boeid = b.boeid
and ISNULL(bh.ApprovalETIUserID, 0) != 0
)

DECLARE @NewUserID int
DECLARE @ETIUserID int

WHILE EXISTS (SELECT 1 FROM @UserXREF WHERE Processed = 0)
BEGIN
SELECT TOP 1 @ETIUserID = IsgsUserID FROM @UserXREF WHERE Processed = 0

declare @UserInCurrentDb int
exec insertOrGetUser @isgsUserId = @ETIUserID, @UserInCurrentDb = @NewUserID OUTPUT

UPDATE @UserXREF 
SET	NewUserID = @NewUserID,
	Processed = 1
WHERE 
	 IsgsUserID = @ETIUserID
END
---STOP USERS SETUP


DECLARE @ResourceListID int
INSERT INTO [dbo].[ResourceList]
           ([UpdateDT]
           ,[ResourceListName])
SELECT RL.[UpdateDT]
      ,[ResourceListName]
  FROM [GenBOE].[dbo].[ResourceList] RL
  INNER JOIN [GenBOE].[dbo].[Workspace] W ON RL.ResourceListID = W.ResourceListID
WHERE W.WorkspaceID = @WorkspaceID
 
SELECT  @ResourceListID = SCOPE_IDENTITY() 

DECLARE @PerformingOrganizationListID int
INSERT INTO [dbo].[PerformingOrganizationList]
           ([UpdateDT]
           ,[PerformingOrganizationListName])
SELECT PL.[UpdateDT]
      ,PL.[PerformingOrganizationListName]
  FROM [GenBOE].[dbo].[PerformingOrganizationList] PL
  INNER JOIN [GenBOE].[dbo].[Workspace] W ON PL.PerformingOrganizationListID = W.PerformingOrganizationListID
WHERE W.WorkspaceID = @WorkspaceID
 
SELECT  @PerformingOrganizationListID = SCOPE_IDENTITY()


/*
Find a unique Shortname to use for new workspace
New shortname will be prefeixed with isgs_ and if necessary
will have a sequential number appended to it to make it unique
*/
DECLARE @NewWorkspaceShortName varchar(21)
DECLARE @PrefixedWorkspaceShortName varchar(21)
DECLARE @WorkspaceShortName varchar(21)
DECLARE @Dup int = 0

SELECT @WorkspaceShortName = WorkspaceShortName
FROM [GenBOE].[dbo].[Workspace]
WHERE WorkspaceID = @WorkspaceID

SET @PrefixedWorkspaceShortName = LEFT(@WorkspaceShortNamePrefix + @WorkspaceShortName, 21)
SET @NewWorkspaceShortName = @PrefixedWorkspaceShortName

WHILE EXISTS	(
					SELECT * FROM dbo.Workspace 
					WHERE WorkspaceShortName = @NewWorkspaceShortName
				)
BEGIN		
	SET @Dup=@Dup+1
	SET @NewWorkspaceShortName = LEFT(@PrefixedWorkspaceShortName, (21-LEN(@DUP))) + CAST(@Dup AS varchar(4))
END	



INSERT INTO [dbo].[Workspace]
           ([UpdateDT]
           ,[WorkspaceName]
           ,[WorkspaceShortName]
           ,[WorkspaceStateID]
           ,[ContractStartDate]
           ,[ContractEndDate]
           ,[ProposalSubmitDate]
           ,[WorkspaceDescription]
           ,[ProposalTypeID]
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
           ,[DTSAutoCalculateID]
           ,[CalculateLaborCostFlag]
           ,[ProductLineID]
           ,[BOEExportSortByID]
           ,[SegmentID]
           ,[LineOfBusinessID]
           ,[ProposalClassID]
		   ,[ProposalTitle]
		   ,[IsDeleted]
		   ,[DateDeleted]
		   ,[ResourcePrecision]
		   ,[RecalculationStartedDate]
		   ,[RestrictDTS]
		   ,[CostPrecision]
           )
SELECT [UpdateDT]
      ,LEFT((@WorkspaceNamePrefix + [WorkspaceName]),115)
      ,@NewWorkspaceShortName
      ,[WorkspaceStateID]
      ,[ContractStartDate]
      ,[ContractEndDate]
      ,[ProposalSubmitDate]
      ,[WorkspaceDescription]
      ,@ProposalTypeID
      ,pricer.NewUserID --[CostVolumeLeadPricerUserID]
      ,[RFPNumber]
      ,@MasterTemplateID --[TemplateID]
      ,[ContainsOCI]
      ,created.NewUserID --[CreatedByETIUserID]
      ,[AllowSearch]
      ,@ResourceListID--[ResourceListID]
      ,@PerformingOrganizationListID--[PerformingOrganizationListID]
      ,[PerformingOrganizationChangeFlag]
      ,[TrackingNumber]
      ,[ContainsTemplate]
      ,[NumProPricerExport]
      ,[ProposalStatusID]
      ,[StatusComment]
      ,1 --[DTSAutoCalculateID] always set DTS off
      ,[CalculateLaborCostFlag]
      ,null --[ProductLineID]
      ,[BOEExportSortByID]
      ,@TransitionedSegmentID --Default segment for company
      ,null --[LineOfBusinessID]
      ,null --[ProposalClassID]
	  ,[ProposalTitle]
      ,[IsDeleted]
	  ,[DateDeleted]  
	  ,[ResourcePrecision]
	  ,[RecalculationStartedDate]
      ,1 --[RestrictDTS] always restrict
	  ,[CostPrecision]
  FROM [genBOE].[dbo].[Workspace] w
  LEFT JOIN @UserXREF created on w.CreatedByETIUserID = created.IsgsUserID
  LEFT JOIN @UserXREF pricer on w.CostVolumeLeadPricerUserID = pricer.IsgsUserID
WHERE WorkspaceID = @WorkspaceID

SELECT @NewWorkspaceID = SCOPE_IDENTITY()

INSERT INTO [dbo].[OutputFormatTemplateWorkspaceXREF]
           ([WorkspaceID]
           ,[TemplateID])
VALUES (@NewWorkspaceID
      ,@MasterTemplateID)


--RESOURCE TABLES
DECLARE @Resource TABLE
(
	[ResourceID] [int] NOT NULL,
	[UpdateDT] [datetime2](7) NOT NULL,
	[ResourceName] [varchar](20) NOT NULL,
	[ResourceDescription] [varchar](100) NULL,
	[SegmentRegion] [varchar](50) NULL,
	[LaborType] [varchar](50) NULL,
	[SegmentID] [int] NULL,
	[ResourceListID] [int] NOT NULL,
	[CostElementID] [int] NOT NULL,
	[DeletedFlag] [bit] NULL,
	[RateTypeID] [int] NULL,
	Processed bit DEFAULT 0,
	NewResourceListID int,
	NewWorkspaceID int,
	NewResourceID int
)

--insert the custom ws resources into temp table
INSERT INTO @Resource
           ([ResourceID]
           ,[UpdateDT]
           ,[ResourceName]
           ,[ResourceDescription]
           ,[SegmentRegion]
           ,[LaborType]
           ,[SegmentID]
           ,[ResourceListID]
           ,[CostElementID]
           ,[DeletedFlag]
           ,[RateTypeID]
           	,Processed
			,NewWorkspaceID
			,NewResourceID)
SELECT R.[ResourceID]
	  ,R.[UpdateDT]
      ,R.[ResourceName]
      ,R.[ResourceDescription]
      ,R.[SegmentRegion]
      ,R.[LaborType]
      ,@TransitionedSegmentID
      ,@ResourceListID
      ,R.[CostElementID] --same across db
      ,R.[DeletedFlag]
      ,R.[RateTypeID] --same across db
      ,0 AS Processed
      ,@NewWorkspaceID AS NewWorkspaceID
      ,NULL AS NewResourceID
  FROM [GenBOE].[dbo].[Resource] R
	INNER JOIN [GenBOE].[dbo].[ResourceList] RL ON R.ResourceListID = RL.ResourceListID
	INNER JOIN [GenBOE].[dbo].[Workspace] W ON RL.ResourceListID = W.ResourceListID
WHERE W.WorkspaceID = @WorkspaceID	


--insert custom ws resources into actual resource table and update temp
DECLARE @ResourceID int
WHILE EXISTS (SELECT 1 FROM @Resource WHERE Processed = 0)
BEGIN
SELECT TOP 1 @ResourceID = ResourceID FROM @Resource WHERE Processed = 0

INSERT INTO [dbo].[Resource]
           (
            [UpdateDT]
           ,[ResourceName]
           ,[ResourceDescription]
           ,[SegmentRegion]
           ,[LaborType]
           ,[SegmentID]
           ,[ResourceListID]
           ,[CostElementID]
           ,[DeletedFlag]
           ,[RateTypeID]
           )
SELECT 
	  R.[UpdateDT]
      ,R.[ResourceName]
      ,R.[ResourceDescription]
      ,R.[SegmentRegion]
      ,R.[LaborType]
      ,R.[SegmentID]
      ,@ResourceListID
      ,R.[CostElementID]
      ,R.[DeletedFlag]
      ,R.[RateTypeID]
FROM @Resource R
WHERE
	ResourceID = @ResourceID


UPDATE @Resource 
SET	NewResourceID = SCOPE_IDENTITY(),
	Processed = 1
WHERE 
	ResourceID = @ResourceID


END

--@WorkspaceResource will contain a pointer to all the custom workspace resources
-- and the system resources that existed at the time the ws was created
DECLARE @WorkspaceResource TABLE
(
	[SystemResourceID] [int],
	[ResourceListID] [int],
	[WorkspaceID] [int],
	NewSystemResourceID [int]
)
INSERT INTO @WorkspaceResource
SELECT 
       [SystemResourceID]
      ,@ResourceListID AS [ResourceListID]
      ,@NewWorkspaceID AS [WorkspaceID]
      ,NULL
FROM [GenBOE].[dbo].[WorkspaceResource]
WHERE WorkspaceID = @WorkspaceID

--This updates the NewResourceID field to be the resource that
--was created specificaly for this workspace (custom resources)
--The resources that aren''t updated are system resources
UPDATE @WorkspaceResource
SET NewSystemResourceID = R.NewResourceID
FROM @WorkspaceResource tWR
	INNER JOIN @Resource R ON tWR.SystemResourceID = R.ResourceID

INSERT INTO [dbo].[WorkspaceResource]
           ([SystemResourceID]
           ,[ResourceListID]
           ,[WorkspaceID])
/*Workspace Resources*/
SELECT 
      [NewSystemResourceID]
      ,[ResourceListID]
      ,[WorkspaceID]
FROM @WorkspaceResource WHERE NewSystemResourceID IS NOT NULL
UNION
/*System Resources*/ -- Need to translate the converted SSC/MST system resource via [IsgsResourceXREF]
SELECT 
      ir.[NewResourceID]
      ,wr.[ResourceListID]
      ,wr.[WorkspaceID]
FROM @WorkspaceResource wr 
	INNER JOIN [dbo].[IsgsResourceXREF] ir on wr.SystemResourceID = ir.ISGSResourceID
	 WHERE wr.NewSystemResourceID IS NULL


UPDATE w
set w.NewSystemResourceID = r.NewResourceID
from @WorkspaceResource w, IsgsResourceXREF r
where w.SystemResourceID = r.ISGSResourceID



--add master resources. need same for perf orgs
/*Original SSC/MST Master System Resources*/ -- Include system resources a new workspace would normally get access to
INSERT INTO [dbo].[WorkspaceResource]
           ([SystemResourceID]
           ,[ResourceListID]
           ,[WorkspaceID])
SELECT 
	r.ResourceID
	,@ResourceListID
	,@NewWorkspaceID
FROM [dbo].[Resource] r 
	 WHERE r.ResourceListID = 1
	 AND	(
					R.DeletedFlag = 0 OR
					R.DeletedFlag IS NULL
				)
	 and r.SegmentID = @TransitionedSegmentID


--STOP OF RESOURCE COPY
    

            

--START PERFORMING ORG COPY
DECLARE @PerformingOrganization TABLE
(
	[PerformingOrganizationID] [int] NOT NULL,
	[UpdateDT] [datetime2](7) NOT NULL,
	[PerformingOrganizationName] [varchar](20) NOT NULL,
	[PerformingOrganizationDescription] [varchar](50) NULL,
	[PerformingOrganizationListID] [int] NOT NULL,
	[DeletedFlag] [bit] NULL,
	Processed bit DEFAULT 0,
	NewPerformingOrganizationListID int,
	NewWorkspaceID int,
	NewPerformingOrganizationID int
)

INSERT INTO @PerformingOrganization
           ([PerformingOrganizationID]
           ,[UpdateDT]
           ,[PerformingOrganizationName]
           ,[PerformingOrganizationDescription]
           ,[PerformingOrganizationListID]
           ,[DeletedFlag]
           	,Processed
			,NewWorkspaceID
			,NewPerformingOrganizationID)
SELECT R.[PerformingOrganizationID]
	  ,R.[UpdateDT]
      ,R.[PerformingOrganizationName]
      ,R.[PerformingOrganizationDescription]
      ,@PerformingOrganizationListID
      ,R.[DeletedFlag]
      ,0 AS Processed
      ,@NewWorkspaceID AS NewWorkspaceID
      ,NULL AS NewPerformingOrganizationID
  FROM [GenBOE].[dbo].[PerformingOrganization] R
	INNER JOIN [GenBOE].[dbo].[PerformingOrganizationList] RL ON R.PerformingOrganizationListID = RL.PerformingOrganizationListID
	INNER JOIN [GenBOE].[dbo].[Workspace] W ON RL.PerformingOrganizationListID = W.PerformingOrganizationListID
WHERE W.WorkspaceID = @WorkspaceID	

DECLARE @PerformingOrganizationID int
WHILE EXISTS (SELECT 1 FROM @PerformingOrganization WHERE Processed = 0)
BEGIN
SELECT TOP 1 @PerformingOrganizationID = PerformingOrganizationID FROM @PerformingOrganization WHERE Processed = 0

INSERT INTO [dbo].[PerformingOrganization]
           (
            [UpdateDT]
           ,[PerformingOrganizationName]
           ,[PerformingOrganizationDescription]
           ,[PerformingOrganizationListID]
           ,[DeletedFlag])
SELECT 
	  R.[UpdateDT]
      ,R.[PerformingOrganizationName]
      ,R.[PerformingOrganizationDescription]
      ,@PerformingOrganizationListID
      ,R.[DeletedFlag]
FROM @PerformingOrganization R
WHERE
	PerformingOrganizationID = @PerformingOrganizationID


UPDATE @PerformingOrganization 
SET	NewPerformingOrganizationID = SCOPE_IDENTITY(),
	Processed = 1
WHERE 
	PerformingOrganizationID = @PerformingOrganizationID


END


--@WorkspacePerformingOrganization will contain a pointer to all the custom workspace perf orgs
-- and the system perf orgs that existed at the time the ws was created
DECLARE @WorkspacePerformingOrganization TABLE
(
	[SystemPerformingOrganizationID] [int],
	[PerformingOrganizationListID] [int],
	[WorkspaceID] [int],
	NewSystemPerformingOrganizationID [int]
)
INSERT INTO @WorkspacePerformingOrganization
SELECT 
       [SystemPerformingOrganizationID]
      ,@PerformingOrganizationListID AS [PerformingOrganizationListID]
      ,@NewWorkspaceID AS [WorkspaceID]
      ,NULL
FROM [GenBOE].[dbo].[WorkspacePerformingOrganization]
WHERE WorkspaceID = @WorkspaceID


--This updates the NewSystemPerformingOrganizationID field to be the org that
--was created specificaly for this workspace (custom perf org)
--The perf orgs that aren''t updated are system resources
UPDATE @WorkspacePerformingOrganization
SET NewSystemPerformingOrganizationID = R.NewPerformingOrganizationID
FROM @WorkspacePerformingOrganization tWR
	INNER JOIN @PerformingOrganization R ON tWR.SystemPerformingOrganizationID = R.PerformingOrganizationID



INSERT INTO [dbo].[WorkspacePerformingOrganization]
           ([SystemPerformingOrganizationID]
           ,[PerformingOrganizationListID]
           ,[WorkspaceID])
/*Workspace PerformingOrganizations*/
SELECT 
      [NewSystemPerformingOrganizationID]
      ,[PerformingOrganizationListID]
      ,[WorkspaceID]
FROM @WorkspacePerformingOrganization WHERE NewSystemPerformingOrganizationID IS NOT NULL
UNION
/*System PerformingOrganizations*/ -- Need to translate the converted SSC/MST system org via [IsgsResourceXREF]
SELECT 
      ip.[NewPerfOrgID]
      ,wp.[PerformingOrganizationListID]
      ,wp.[WorkspaceID]
FROM @WorkspacePerformingOrganization wp
	INNER JOIN [dbo].[IsgsPerfOrgXREF] ip on wp.SystemPerformingOrganizationID = ip.ISGSPerfOrgID
	WHERE wp.NewSystemPerformingOrganizationID IS NULL

--Now update the system perf orgs
UPDATE w
set w.NewSystemPerformingOrganizationID = p.NewPerfOrgID
from @WorkspacePerformingOrganization w, IsgsPerfOrgXREF p
where w.SystemPerformingOrganizationID = p.ISGSPerfOrgID

--add master Perf Orgs
/*Original SSC/MST Master System Perf Orgs*/ -- Include system orgs a new workspace would normally get access to
INSERT INTO [dbo].[WorkspacePerformingOrganization]
           ([SystemPerformingOrganizationID]
           ,[PerformingOrganizationListID]
           ,[WorkspaceID])
SELECT 
	p.PerformingOrganizationID
	,@PerformingOrganizationListID
	,@NewWorkspaceID
FROM [dbo].[PerformingOrganization] p 
	 WHERE p.PerformingOrganizationListID = 1
	 AND	(
					p.DeletedFlag = 0 OR
					p.DeletedFlag IS NULL
				)


--STOP PERF ORG COPY

DECLARE @BOE TABLE
(
	[BOEID] [int] NOT NULL,
	[UpdateDT] [datetime2](7) NOT NULL,
	[BOEStateID] [int] NOT NULL,
	[BOEStartDate] [date] NOT NULL,
	[BOEEndDate] [date] NOT NULL,
	[BOEDescription] [varchar](max) NULL,
	[DataSource] [varchar](max) NULL,
	[WorkspaceID] [int] NOT NULL,
	[MetricDisclosureAcknowledge] [bit] NOT NULL,
	[NumAuthorReassigned] [int] NOT NULL,
	[IsMaterial] [bit] NOT NULL,
	[AutoCalculateDTS] [bit] NOT NULL,
	Processed bit DEFAULT 0,
	NewBOEID int NULL,
	NewWorkspaceID int NOT NULL,
	[BOETitle] varchar (100) NOT NULL,
	[IsMultiClinWbs] [bit] DEFAULT 0
	)
INSERT INTO @BOE	
SELECT [BOEID]
      ,[UpdateDT]
      ,[BOEStateID]
      ,[BOEStartDate]
      ,[BOEEndDate]
      ,[BOEDescription]
      ,[DataSource]
      ,[WorkspaceID]
      ,0 --[MetricDisclosureAcknowledge] default unchecked since no metrics copied
      ,[NumAuthorReassigned]
      ,[IsMaterial]
	  ,0 --[AutoCalculateDTS] always turn off
      ,0 --Processed
      ,NULL --NewBOEID
      ,@NewWorkspaceID
      ,[BOETitle]
	  ,[IsMultiClinWbs]
  FROM [GenBOE].[dbo].[BOE]
WHERE WorkspaceID = @WorkspaceID

DECLARE @BOEID int
WHILE EXISTS (SELECT 1 FROM @BOE WHERE Processed = 0)
BEGIN
SELECT TOP 1 @BOEID = BOEID FROM @BOE WHERE Processed = 0

INSERT INTO [dbo].[BOE]
([UpdateDT]
,[BOEStateID]
,[BOEStartDate]
,[BOEEndDate]
,[BOEDescription]
,[DataSource]
,[WorkspaceID]
,[MetricDisclosureAcknowledge]
,[NumAuthorReassigned]
,[IsMaterial]
,[AutoCalculateDTS]
,[BOETitle]
,[IsMultiClinWbs]
)
SELECT [UpdateDT]
      ,[BOEStateID]
      ,[BOEStartDate]
      ,[BOEEndDate]
      ,[BOEDescription]
      ,[DataSource]
      ,@NewWorkspaceID--[WorkspaceID]
      ,[MetricDisclosureAcknowledge]
      ,[NumAuthorReassigned]
      ,[IsMaterial]
	  ,[AutoCalculateDTS]
      ,[BOETitle]
	  ,[IsMultiClinWbs]
  FROM @BOE
WHERE BOEID = @BOEID 

UPDATE @BOE 
SET	NewBOEID = SCOPE_IDENTITY(),
	Processed = 1
WHERE 
BOEID = @BOEID	

END

--START ODC TASKS
DECLARE @ODCTaskElement TABLE
(
	[ODCTaskElementID] [int] NOT NULL,
	[UpdateDT] [datetime2](7) NOT NULL,
	[ODCTaskTitle] [varchar](100) NOT NULL,
	[ODCTaskDescription] [varchar](max) NOT NULL,
	[ODCMOQText] [varchar](max) NULL,
	[BOEID] [int] NOT NULL,
	[ODCTaskID] [varchar](3) NULL,
	[TaskStartDate] [DATE],
	[TaskEndDate] [DATE],
	Processed bit DEFAULT 0,
	NewODCTaskElementID [int],
	NewBOEID int,
	[SortOrderID] int
)
INSERT INTO @ODCTaskElement
SELECT TE.[ODCTaskElementID]
      ,TE.[UpdateDT]
      ,TE.[ODCTaskTitle]
      ,TE.[ODCTaskDescription]
      ,TE.[ODCMOQText]
      ,TE.[BOEID]
      ,TE.[ODCTaskID]
      ,TE.[TaskStartDate]
      ,TE.[TaskEndDate]
      ,0
      ,NULL
      ,B.NewBOEID
	  ,TE.[SortOrderID]
FROM [GenBOE].[dbo].[ODCTaskElement] TE 
 INNER JOIN @BOE B ON TE.BOEID = B.BOEID


DECLARE @ODCTaskElementID [int] 
WHILE EXISTS (SELECT 1 FROM @ODCTaskElement WHERE Processed = 0)
BEGIN
SELECT TOP 1 @ODCTaskElementID = ODCTaskElementID  FROM @ODCTaskElement WHERE Processed = 0
INSERT INTO [dbo].[ODCTaskElement]
           ([UpdateDT]
           ,[ODCTaskTitle]
           ,[ODCTaskDescription]
           ,[ODCMOQText]
           ,[BOEID]
           ,[ODCTaskID]
           ,[TaskStartDate]
           ,[TaskEndDate]
		   ,[SortOrderID]
           )
SELECT tTE.[UpdateDT]
      ,tTE.[ODCTaskTitle]
      ,tTE.[ODCTaskDescription]
      ,tTE.[ODCMOQText]
      ,tB.NewBOEID--[BOEID]
      ,tTE.[ODCTaskID]
      ,tTE.[TaskStartDate]
      ,tTE.[TaskEndDate]
	  ,tTE.[SortOrderID]
  FROM @ODCTaskElement tTE 
	INNER JOIN @BOE tB ON tTE.BOEID = tB.BOEID
WHERE ODCTaskElementID = @ODCTaskElementID

UPDATE @ODCTaskElement
SET	NewODCTaskElementID = SCOPE_IDENTITY(),
	Processed = 1
WHERE 
	ODCTaskElementID  = @ODCTaskElementID	

END		


DECLARE @ODCType TABLE 
(
	[ODCTypeID] [int] NOT NULL,
	[UpdateDT] [datetime2](7) NOT NULL,
	[ResourceID] [int] NULL,
	[PerformingOrganizationID] [int] NULL,
	[ODCTypeStartDate] [date] NOT NULL,
	[ODCTypeEndDate] [date] NULL,
	[SpreadCurveID] [int] NOT NULL,
	[ODCTypeCost] [decimal](12, 2) NULL,
	[ODCTaskElementID] [int] NOT NULL,
	Processed bit DEFAULT 0,
	NewODCTypeID int,
	NewResourceID int,
	NewPerformingOrganizationID int,
	NewODCTaskElementID int
)
INSERT INTO	@ODCType
SELECT O.[ODCTypeID]
      ,O.[UpdateDT]
      ,O.[ResourceID]
      ,O.[PerformingOrganizationID]
      ,O.[ODCTypeStartDate]
      ,O.[ODCTypeEndDate]
      ,O.[SpreadCurveID]
      ,O.[ODCTypeCost]
      ,O.[ODCTaskElementID]
      ,0
      ,NULL
      ,R.NewSystemResourceID
      ,PO.NewSystemPerformingOrganizationID
      ,TE.NewODCTaskElementID
  FROM [GenBOE].[dbo].[ODCType] O
	INNER JOIN @ODCTaskElement TE ON O.ODCTaskElementID = TE.ODCTaskElementID
	LEFT OUTER JOIN @WorkspaceResource R ON O.ResourceID = R.SystemResourceID
	LEFT OUTER JOIN @WorkspacePerformingOrganization PO ON O.PerformingOrganizationID = PO.SystemPerformingOrganizationID

DECLARE @ODCTypeID int
WHILE EXISTS (SELECT 1 FROM @ODCType WHERE Processed = 0)
BEGIN
SELECT TOP 1 @ODCTypeID = ODCTypeID FROM @ODCType WHERE Processed = 0
INSERT INTO [dbo].[ODCType]
           ([UpdateDT]
           ,[ResourceID]
           ,[PerformingOrganizationID]
           ,[ODCTypeStartDate]
           ,[ODCTypeEndDate]
           ,[SpreadCurveID]
           ,[ODCTypeCost]
           ,[ODCTaskElementID])
SELECT [UpdateDT]
      ,NewResourceID
      ,NewPerformingOrganizationID
      ,[ODCTypeStartDate]
      ,[ODCTypeEndDate]
      ,[SpreadCurveID]
      ,[ODCTypeCost]
      ,NewODCTaskElementID--[ODCTaskElementID]
  FROM @ODCType
WHERE ODCTypeID = @ODCTypeID  

UPDATE @ODCType
SET NewODCTypeID = SCOPE_IDENTITY(),
	Processed = 1
WHERE ODCTypeID = @ODCTypeID  

END

DECLARE @ODCSpread TABLE 
(
	[ODCSpreadID] [int] NOT NULL,
	[ODCSpreadDate] [date] NOT NULL,
	[ODCSpreadValue] [bigint] NOT NULL,
	[ODCTypeID] [int] NOT NULL,
	Processed bit DEFAULT 0,
	NewODCSpreadID [int] NULL,
	NewODCTypeID [int] NULL
)
INSERT INTO @ODCSpread
SELECT OS.[ODCSpreadID]
      ,OS.[ODCSpreadDate]
      ,OS.[ODCSpreadValue]
      ,OS.[ODCTypeID]
      ,0
      ,NULL
      ,OT.NewODCTypeID
  FROM [GenBOE].[dbo].[ODCSpread] OS
INNER JOIN @ODCType OT ON OS.ODCTypeID = OT.ODCTypeID

DECLARE @ODCSpreadID INT
WHILE EXISTS (SELECT 1 FROM @ODCSpread WHERE Processed = 0)
BEGIN
SELECT TOP 1 @ODCSpreadID = ODCSpreadID  FROM @ODCSpread WHERE Processed = 0

INSERT INTO [dbo].[ODCSpread]
           ([ODCSpreadDate]
           ,[ODCSpreadValue]
           ,[ODCTypeID])
SELECT [ODCSpreadDate]
      ,[ODCSpreadValue]
      ,OT.NewODCTypeID--[ODCTypeID]
FROM @ODCSpread OS 
	INNER JOIN @ODCType OT ON OS.ODCTypeID = OT.ODCTypeID
WHERE @ODCSpreadID = @ODCSpreadID	

UPDATE @ODCSpread
SET	NewODCSpreadID = SCOPE_IDENTITY(),
	Processed = 1
WHERE @ODCSpreadID = @ODCSpreadID	
	
END

--STOP ODC TASKS



----Start Custom Fields


INSERT INTO [dbo].[CustomField]
           ([UpdateDT]
           ,[CustomFieldName]
           ,[CustomFieldRequired]
           ,[CustomFieldDisplayID]
           ,[WorkspaceID])
SELECT [UpdateDT]
      ,[CustomFieldName]
      ,[CustomFieldRequired]
      ,[CustomFieldDisplayID]
      ,@NewWorkspaceID--[WorkspaceID]
  FROM [GenBOE].[dbo].[CustomField]
WHERE WorkspaceID = @WorkspaceID

DECLARE @CustomFieldMapping TABLE
(
	OriginalCustomFieldID int,
	NewCustomFieldID int
)
INSERT INTO @CustomFieldMapping
SELECT Original.CustomFieldID, New.CustomFieldID
FROM
	(
	SELECT [CustomFieldID]
      ,[UpdateDT]
      ,[CustomFieldName]
      ,[CustomFieldRequired]
      ,[CustomFieldDisplayID]
      ,[WorkspaceID]
	FROM [GenBOE].[dbo].[CustomField]
	WHERE WorkspaceID = @WorkspaceID
	) Original
	INNER JOIN
	(
	SELECT [CustomFieldID]
      ,[UpdateDT]
      ,[CustomFieldName]
      ,[CustomFieldRequired]
      ,[CustomFieldDisplayID]
      ,[WorkspaceID]
	FROM [dbo].[CustomField] --current db on purpose
	WHERE WorkspaceID = @NewWorkspaceID
	) New ON
      Original.[UpdateDT] = New.UpdateDT AND
      Original.[CustomFieldName] = New.CustomFieldName AND
      Original.[CustomFieldRequired] = New.CustomFieldRequired AND
      Original.[CustomFieldDisplayID] = New.CustomFieldDisplayID


DECLARE @CustomFieldValue TABLE
(
	[CustomFieldValueID] [int] NOT NULL,
	[UpdateDT] [datetime2](7) NOT NULL,
	[CustomFieldValueName] [varchar](20) NOT NULL,
	[CustomFieldValueDescription] [varchar](50) NULL,
	[CustomFieldID] [int] NOT NULL,
	[CustomFieldValueInUseFlag] [bit] NOT NULL,
	Processed bit,
	NewCustomFieldValueID int,
	NewCustomFieldID int
)
INSERT INTO @CustomFieldValue
SELECT [CustomFieldValueID]
      ,[UpdateDT]
      ,[CustomFieldValueName]
      ,[CustomFieldValueDescription]
      ,[CustomFieldID]
      ,[CustomFieldValueInUseFlag]
      ,0
      ,NULL
      ,CFM.NewCustomFieldID
  FROM [GenBOE].[dbo].[CustomFieldValue] CFV
INNER JOIN @CustomFieldMapping CFM ON CFV.CustomFieldID = CFM.OriginalCustomFieldID

DECLARE @CustomFieldValueID int
WHILE EXISTS (SELECT 1 FROM @CustomFieldValue WHERE Processed = 0)
BEGIN
SELECT TOP 1 @CustomFieldValueID = CustomFieldValueID FROM @CustomFieldValue WHERE Processed = 0

INSERT INTO [dbo].[CustomFieldValue]
           ([UpdateDT]
           ,[CustomFieldValueName]
           ,[CustomFieldValueDescription]
           ,[CustomFieldID]
           ,[CustomFieldValueInUseFlag])
SELECT CFV.[UpdateDT]
      ,CFV.[CustomFieldValueName]
      ,CFV.[CustomFieldValueDescription]
      ,CFV.NewCustomFieldID--[CustomFieldID]
      ,CFV.[CustomFieldValueInUseFlag]
  FROM @CustomFieldValue CFV
WHERE CFV.CustomFieldValueID = @CustomFieldValueID  

UPDATE @CustomFieldValue
SET NewCustomFieldValueID = SCOPE_IDENTITY(),
	Processed = 1
WHERE CustomFieldValueID = @CustomFieldValueID  

END

----STOP CUSTOM FIELDS

INSERT INTO [dbo].[BOEPotentialRole]
           ([UpdateDT]
           ,[ETIUserID]
           ,[WorkspaceID]
           ,[RoleID]
           ,[UserRemoved])
SELECT 
      [UpdateDT]
      ,u.NewUserID
      ,@NewWorkspaceID--[WorkspaceID]
      ,[RoleID]
      ,[UserRemoved]
  FROM [GenBOE].[dbo].[BOEPotentialRole] pr
  LEFT JOIN @UserXREF u on pr.ETIUserID = u.IsgsUserID
WHERE pr.WorkspaceID = @WorkspaceID


DECLARE @WorkBreakdownStructure TABLE
(
	[WBSID] [int]  NOT NULL,
	[UpdateDT] [datetime2](7) NOT NULL,
	[WBSNumber] [varchar](119) NULL,
	[DisplayedWBSNumber] [varchar](30) NOT NULL,
	[WBSTitle] [varchar](100) NULL,
	[WorkspaceID] [int] NULL,
	[WorkspaceDTCElementID] [int] NULL,
	Processed bit,
	NewWBSID int,
	NewWorkspaceID int
)
INSERT INTO @WorkBreakdownStructure
SELECT WBS.[WBSID]
      ,WBS.[UpdateDT]
      ,WBS.[WBSNumber]
      ,WBS.[DisplayedWBSNumber]
      ,WBS.[WBSTitle]
      ,WBS.[WorkspaceID]
      ,WBS.[WorkspaceDTCElementID]
      ,0
      ,NULL
      ,@NewWorkspaceID   
  FROM [GenBOE].[dbo].[WorkBreakdownStructure] WBS
WHERE WBS.WorkspaceID = @WorkspaceID

DECLARE @WBSID int
WHILE EXISTS (SELECT 1 FROM @WorkBreakdownStructure WHERE Processed = 0)
BEGIN
SELECT TOP 1 @WBSID = WBSID FROM @WorkBreakdownStructure WHERE Processed = 0

INSERT INTO [dbo].[WorkBreakdownStructure]
           ([UpdateDT]
           ,[WBSNumber]
           ,[DisplayedWBSNumber]
           ,[WBSTitle]
           ,[WorkspaceID]
           ,[WorkspaceDTCElementID])
SELECT [UpdateDT]
      ,[WBSNumber]
      ,[DisplayedWBSNumber]
      ,[WBSTitle]
      ,@NewWorkspaceID--[WorkspaceID]
      ,NULL--[WorkspaceDTCElementID]
  FROM @WorkBreakdownStructure
WHERE WBSID = @WBSID

UPDATE @WorkBreakdownStructure
SET NewWBSID = SCOPE_IDENTITY(),
	Processed = 1
WHERE WBSID = @WBSID

END


INSERT INTO [dbo].[WorkspaceStateHistory]
           ([UpdateDT]
           ,[WorkspaceID]
           ,[CurrentWorkspaceStateID]
           ,[UpdatedWorkspaceStateID]
           ,[ChangedByETIUserID])
SELECT [UpdateDT]
      ,@NewWorkspaceID--[WorkspaceID]
      ,[CurrentWorkspaceStateID]
      ,[UpdatedWorkspaceStateID]
      ,u.NewUserID
  FROM [GenBOE].[dbo].[WorkspaceStateHistory] wh
  LEFT JOIN @UserXREF u on wh.ChangedByETIUserID = u.IsgsUserID
WHERE WorkspaceID = @WorkspaceID


DECLARE @WorkspaceVariable TABLE 
(
	[WorkspaceVariableID] [int] NOT NULL,
	[UpdateDT] [datetime2](7) NOT NULL,
	[WorkspaceVariableName] [varchar](20) NULL,
	[WorkspaceVariableValue] [decimal](29, 10) NULL,
	[WorkspaceID] [int] NOT NULL,
	[SortByID] [int] NOT NULL,
	[ValueTypeID] [int] NOT NULL,
	[IsPercentage] [bit] NULL,
	Processed bit,
	NewWorkspaceVariableID int,
	NewWorkspaceID int
)	
INSERT INTO @WorkspaceVariable
SELECT [WorkspaceVariableID]
      ,[UpdateDT]
      ,[WorkspaceVariableName]
      ,[WorkspaceVariableValue]
      ,[WorkspaceID]
      ,[SortByID]
      ,1 --[ValueTypeID] change all to discrete
      ,[IsPercentage]
      ,0
      ,NULL
      ,@NewWorkspaceID
  FROM [GenBOE].[dbo].[WorkspaceVariable]
WHERE WorkspaceID = @WorkspaceID

DECLARE @WorkspaceVariableID INT
WHILE EXISTS (SELECT 1 FROM @WorkspaceVariable WHERE Processed = 0)
BEGIN
SELECT TOP 1 @WorkspaceVariableID = WorkspaceVariableID FROM  @WorkspaceVariable WHERE Processed = 0
INSERT INTO [dbo].[WorkspaceVariable]
           ([UpdateDT]
           ,[WorkspaceVariableName]
           ,[WorkspaceVariableValue]
           ,[WorkspaceID]
           ,[SortByID]
           ,[ValueTypeID]
           ,[IsPercentage])
SELECT [UpdateDT]
      ,[WorkspaceVariableName]
      ,[WorkspaceVariableValue]
      ,@NewWorkspaceID--[WorkspaceID]
      ,[SortByID]
      ,[ValueTypeID]
      ,[IsPercentage]
  FROM @WorkspaceVariable WV
WHERE WorkspaceVariableID = @WorkspaceVariableID

UPDATE @WorkspaceVariable
SET	NewWorkspaceVariableID = SCOPE_IDENTITY(),
	Processed = 1
WHERE WorkspaceVariableID = @WorkspaceVariableID
END


INSERT INTO [dbo].[WorkspaceUserRole]
           ([UpdateDT]
           ,[ETIUserID]
           ,[RoleID]
           ,[WorkspaceID]
           ,[HideHelp])
SELECT [UpdateDT]
      ,u.NewUserID
      ,[RoleID]
      ,@NewWorkspaceID--[WorkspaceID]
      ,[HideHelp]
  FROM [GenBOE].[dbo].[WorkspaceUserRole] w
  LEFT JOIN @UserXREF u ON w.ETIUserID = u.IsgsUserID
WHERE WorkspaceID = @WorkspaceID




DECLARE @CLIN TABLE 
(
	[CLINID] [int]  NOT NULL,
	[UpdateDT] [datetime2](7) NOT NULL,
	[CLINNumber] [varchar](459) NULL,
	[CLINTitle] [varchar](100) NULL,
	[CLINStartDate] [date] NULL,
	[CLINEndDate] [date] NULL,
	[WorkspaceID] [int] NOT NULL,
	Processed bit,
	NewCLINID int,
	NewWorkspaceID int,
	[DisplayedCLINNumber] [varchar] (20)
)	
INSERT INTO @CLIN
SELECT [CLINID]
      ,[UpdateDT]
      ,[CLINNumber]
      ,[CLINTitle]
      ,[CLINStartDate]
      ,[CLINEndDate]
      ,[WorkspaceID]
      ,0
      ,NULL
      ,@NewWorkspaceID
      ,[DisplayedCLINNumber]
  FROM [GenBOE].[dbo].[CLIN]
WHERE WorkspaceID = @WorkspaceID

DECLARE @CLINID INT
WHILE EXISTS (SELECT 1 FROM @CLIN WHERE Processed = 0)
BEGIN
SELECT TOP 1 @CLINID = CLINID FROM @CLIN WHERE Processed = 0
INSERT INTO [dbo].[CLIN]
           ([UpdateDT]
           ,[CLINNumber]
           ,[CLINTitle]
           ,[CLINStartDate]
           ,[CLINEndDate]
           ,[WorkspaceID]
           ,[DisplayedCLINNumber]
           )
SELECT [UpdateDT]
      ,[CLINNumber]
      ,[CLINTitle]
      ,[CLINStartDate]
      ,[CLINEndDate]
      ,@NewWorkspaceID--[WorkspaceID]
      ,[DisplayedCLINNumber]
  FROM @CLIN
WHERE CLINID = @CLINID

UPDATE @CLIN
SET	NewCLINID = SCOPE_IDENTITY(),
	Processed = 1
WHERE CLINID = @CLINID
END


DECLARE @WBS_CLIN_BOE_XREF TABLE
(
	[WBSID] [int] NULL,
	[CLINID] [int] NULL,
	[BOEID] [int] NULL,
	NewWBSID int,
	NewClinID int,
	NewBOEID int
)	
INSERT INTO @WBS_CLIN_BOE_XREF ([WBSID],[CLINID],[BOEID])
SELECT  X.[WBSID], X.[CLINID], X.[BOEID]
FROM [GenBOE].[dbo].[WBS_CLIN_BOE_XREF] X 
INNER JOIN @WorkBreakdownStructure WBS ON X.WBSID = WBS.WBSID
UNION
SELECT  X.[WBSID], X.[CLINID], X.[BOEID]
FROM [GenBOE].[dbo].[WBS_CLIN_BOE_XREF] X 
INNER JOIN @CLIN C ON X.CLINID = C.CLINID 
UNION
SELECT  X.[WBSID], X.[CLINID], X.[BOEID]
FROM [GenBOE].[dbo].[WBS_CLIN_BOE_XREF] X 
INNER JOIN @BOE B ON X.BOEID = B.BOEID

UPDATE @WBS_CLIN_BOE_XREF
SET NewWBSID = W.NewWBSID
FROM  @WBS_CLIN_BOE_XREF X
	INNER JOIN @WorkBreakdownStructure W ON X.WBSID = W.WBSID

UPDATE @WBS_CLIN_BOE_XREF
SET NewCLINID = C.NewCLINID
FROM  @WBS_CLIN_BOE_XREF X
	INNER JOIN @CLIN C ON X.CLINID = C.CLINID

UPDATE @WBS_CLIN_BOE_XREF
SET NewBOEID = B.NewBOEID
FROM  @WBS_CLIN_BOE_XREF X
	INNER JOIN @BOE B ON X.BOEID = B.BOEID

INSERT INTO [dbo].[WBS_CLIN_BOE_XREF]
           ([WBSID]
           ,[CLINID]
           ,[BOEID])
SELECT DISTINCT NewWBSID, NewCLINID, NewBOEID
FROM @WBS_CLIN_BOE_XREF

INSERT INTO [dbo].[BOEStateHistory]
           ([UpdateDT]
           ,[BOEID]
           ,[FieldID]
           ,[CurrentBOEStateID]
           ,[UpdatedBOEStateID]
           ,[ChangedByETIUserID])
SELECT BH.[UpdateDT]
      ,B.NewBOEID--[BOEID]
      ,BH.[FieldID]
      ,BH.[CurrentBOEStateID]
      ,BH.[UpdatedBOEStateID]
      ,u.NewUserID
  FROM [GenBOE].[dbo].[BOEStateHistory] BH
INNER JOIN @BOE B ON BH.BOEID = B.BOEID
LEFT JOIN @UserXREF u ON BH.[ChangedByETIUserID] = u.IsgsUserID


INSERT INTO [dbo].[BOEUserRoleHistory]
           ([UpdateDT]
           ,[CurrentETIUserID]
           ,[UpdatedETIUserID]
           ,[RoleID]
           ,[BOEID]
           ,[FieldID]
           ,[ChangedByETIUserID])
SELECT H.[UpdateDT]
      ,cur.NewUserID --H.[CurrentETIUserID]
      ,upd.NewUserID --H.[UpdatedETIUserID]
      ,H.[RoleID]
      ,NewBOEID--[BOEID]
      ,H.[FieldID]
      ,chg.NewUserID --H.[ChangedByETIUserID]
  FROM [GenBOE].[dbo].[BOEUserRoleHistory] H
INNER JOIN @BOE B ON H.BOEID = B.BOEID
LEFT JOIN @UserXREF cur on H.CurrentETIUserID = cur.IsgsUserID
LEFT JOIN @UserXREF upd on H.UpdatedETIUserID = upd.IsgsUserID
LEFT JOIN @UserXREF chg on H.ChangedByETIUserID = chg.IsgsUserID


INSERT INTO [dbo].[BOEUserRole]
           ([UpdateDT]
           ,[ETIUserID]
           ,[RoleID]
           ,[BOEID])
SELECT R.[UpdateDT]
      ,U.NewUserID--R.[ETIUserID]
      ,R.[RoleID]
      ,NewBOEID--[BOEID]
  FROM [GenBOE].[dbo].[BOEUserRole] R 
INNER JOIN @BOE B ON R.BOEID = B.BOEID
LEFT JOIN @UserXREF U on R.ETIUserID = U.IsgsUserID

INSERT INTO [dbo].[BOEApproval]
           ([UpdateDT]
           ,[BOEID]
           ,[ApprovalETIUserID]
           ,[ApprovedFlag])
SELECT B.[UpdateDT]
      ,tB.NewBOEID--[BOEID]
      ,U.NewUserID --B.[ApprovalETIUserID]
      ,B.[ApprovedFlag]
  FROM [GenBOE].[dbo].[BOEApproval] B
INNER JOIN @BOE tB ON B.BOEID = tB.BOEID
LEFT JOIN @UserXREF U on B.ApprovalETIUserID = U.IsgsUserID

--START Comments
DECLARE @BOEComment  TABLE
(
	[BOECommentID] [int] NOT NULL,
	[UpdateDT] [datetime2](7) NOT NULL,
	[FieldID] [int] NOT NULL,
	[BOEComments] [varchar](500) NOT NULL,
	[BOECommentETIUserID] [int] NOT NULL,
	[BOEResponseToCommentID] [int] NULL,
	[BOEID] [int] NOT NULL,
	Processed bit,
	NewBOECommentID [int],
	NewBOEID int
)
INSERT INTO @BOEComment
SELECT B.[BOECommentID]
      ,B.[UpdateDT]
      ,B.[FieldID]
      ,B.[BOEComments]
      ,U.NewUserID --B.[BOECommentETIUserID]
      ,B.[BOEResponseToCommentID]
      ,B.[BOEID]
      ,0
      ,NULL
      ,tB.NewBOEID
  FROM [GenBOE].[dbo].[BOEComment] B
INNER JOIN @BOE tB ON B.BOEID = tB.BOEID
LEFT JOIN @UserXREF U on B.BOECommentETIUserID = U.IsgsUserID

DECLARE @BOECommentID int
WHILE EXISTS (SELECT 1 FROM @BOEComment WHERE Processed = 0)
BEGIN
SELECT TOP 1 @BOECommentID = BOECommentID 
	FROM @BOEComment 
	WHERE Processed = 0
	ORDER BY BOECommentID ASC

INSERT INTO [dbo].[BOEComment]
           ([UpdateDT]
           ,[FieldID]
           ,[BOEComments]
           ,[BOECommentETIUserID]
           ,[BOEResponseToCommentID]
           ,[BOEID])
SELECT BC.[UpdateDT]
      ,BC.[FieldID]
      ,BC.[BOEComments]
      ,BC.[BOECommentETIUserID] --Note this is the translated value
      ,tBC.NewBOECommentID--Need to get the updated Comment[BOEResponseToCommentID]
      ,BC.NewBOEID--[BOEID]
  FROM @BOEComment BC
	LEFT OUTER JOIN @BOEComment tBC ON BC.BOEResponseToCommentID = tBC.BOECommentID
WHERE BC.BOECommentID = @BOECommentID  

UPDATE @BOEComment
SET NewBOECommentID = SCOPE_IDENTITY(),	
	Processed = 1
WHERE BOECommentID = @BOECommentID  

END

INSERT INTO [dbo].[BOECommentHistory]
           ([UpdateDT]
           ,[BOECommentID]
           ,[BOEID]
           ,[FieldID]
           ,[CurrentComment]
           ,[UpdatedComment]
           ,[ChangedByETIUserID])
SELECT BCH.[UpdateDT]
      ,tBC.NewBOECommentID--[BOECommentID]
      ,tBC.NewBOEID--[BOEID]
      ,BCH.[FieldID]
      ,BCH.[CurrentComment]
      ,BCH.[UpdatedComment]
      ,U.NewUserID --BCH.[ChangedByETIUserID]
  FROM [GenBOE].[dbo].[BOECommentHistory] BCH
	INNER JOIN   @BOEComment tBC ON BCH.BOECommentID = tBC.BOECommentID
	LEFT JOIN @UserXREF U ON BCH.ChangedByETIUserID = U.IsgsUserID
--STOP Comments

DECLARE @BOETaskElement TABLE 
(
	[BOETaskElementID] [int] NOT NULL,
	[UpdateDT] [datetime2](7) NOT NULL,
	[TaskID] [varchar](3) NULL,
	[TaskTitle] [varchar](100) NOT NULL,
	[TaskDescription] [varchar](max) NULL,
	[TaskStartDate] [date] NULL,
	[TaskEndDate] [date] NULL,
	[MOQHoursEquation] [varchar](500) NULL,
	[MOQCostEquation] [varchar](250) NULL,
	[MOQText] [varchar](max) NULL,
	[MOQTypeID] [int] NULL,
	[BOEID] [int] NULL,
	[LaborTypeWarningFlag] [bit] NOT NULL,
	[IMS_ID] [varchar](20) NULL,
	[TaskElementTypeID] [int] NOT NULL,
	Processed bit,
	NewBOETaskElementID int,
	NewBOEID int,
	[SortOrderID] INT
)	
INSERT INTO @BOETaskElement
SELECT TE.[BOETaskElementID]
      ,TE.[UpdateDT]
      ,TE.[TaskID]
      ,TE.[TaskTitle]
      ,TE.[TaskDescription]
      ,TE.[TaskStartDate]
      ,TE.[TaskEndDate]
      ,TE.[MOQHoursEquation]
      ,TE.[MOQCostEquation]
      ,TE.[MOQText]
      ,NULL --TE.[MOQTypeID]
      ,B.NewBOEID--[BOEID]
      ,TE.[LaborTypeWarningFlag]
      ,TE.[IMS_ID]
      ,TE.[TaskElementTypeID]
      ,0
      ,NULL
      ,B.NewBOEID
	  ,TE.[SortOrderID]
  FROM [GenBOE].[dbo].[BOETaskElement] TE
	INNER JOIN @BOE B ON TE.BOEID = B.BOEID
	WHERE TE.TaskElementTypeID <>  3 -- DO NOT COPY DTS Tasks


/*
Fix Workspace Variables in MOQ Equations
*/
DECLARE @WSVar TABLE
(
MOQHoursEquation varchar (500),
Original varchar(100),
Updated varchar(100),
OriginalID int,
UpdatedID int,
BOETaskElementID int
)
INSERT INTO @WSVar (MOQHoursEquation, BOETaskElementID)
SELECT MOQHoursEquation, BOETaskElementID  
FROM @BOETaskElement 
WHERE MOQHoursEquation LIKE ''%<WSVAR:%''

WHILE EXISTS (SELECT 1 FROM @WSVar WHERE MOQHoursEquation LIKE ''%<WSVAR:%'')
BEGIN
UPDATE @WSVar
SET Original = 
SUBSTRING 
	(
		MOQHoursEquation,
		CHARINDEX (''<WSVAR:'',MOQHoursEquation),
		(CHARINDEX (''>'',MOQHoursEquation) - CHARINDEX (''<WSVAR:'',MOQHoursEquation) + 1)
	)
	


UPDATE @WSVar
SET OriginalID =
 REPLACE (RIGHT (Original,
	(LEN (Original) - CHARINDEX ('':'',Original))),
	''>'', '''')


UPDATE @WSVar
SET UpdatedID = WS.NewWorkspaceVariableID
FROM @WSVar t
	INNER JOIN @WorkspaceVariable WS ON t.OriginalID = WS.WorkspaceVariableID


UPDATE @WSVar
SET Updated = 
REPLACE (Original, OriginalID, UpdatedID) 


UPDATE @BOETaskElement
SET MOQHoursEquation = 
REPLACE (TE.MOQHoursEquation, V.Original, V.Updated)
FROM @BOETaskElement TE
	INNER JOIN @WSVar V ON TE.BOETaskElementID = V. BOETaskElementID

UPDATE @WSVar 
SET MOQHoursEquation = REPLACE (MOQHoursEquation, Original, '''')

END

DECLARE @BOETaskElementID int
WHILE EXISTS (SELECT 1 FROM @BOETaskElement WHERE Processed = 0)
BEGIN
SELECT TOP 1 @BOETaskElementID = BOETaskElementID FROM @BOETaskElement WHERE Processed = 0
INSERT INTO [dbo].[BOETaskElement]
           ([UpdateDT]
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
		   ,[SortOrderID]
		   )
 SELECT [UpdateDT]
      ,[TaskID]
      ,[TaskTitle]
      ,[TaskDescription]
      ,[TaskStartDate]
      ,[TaskEndDate]
      ,[MOQHoursEquation]
      ,[MOQCostEquation]
      ,[MOQText]
      ,[MOQTypeID]
      ,NewBOEID--[BOEID]
      ,[LaborTypeWarningFlag]
      ,[IMS_ID]
      ,[TaskElementTypeID]
	  ,[SortOrderID]
  FROM @BOETaskElement
WHERE BOETaskElementID = @BOETaskElementID
	AND [TaskElementTypeID] <>  3 -- DON''t COPY DTS
UPDATE @BOETaskElement
SET NewBOETaskElementID = SCOPE_IDENTITY(),
	Processed = 1
WHERE BOETaskElementID = @BOETaskElementID

END	






DECLARE @OrdinaryVariable TABLE
(
	[OrdinaryVariableID] [int] NOT NULL,
	[UpdateDT] [datetime2](7) NOT NULL,
	[OrdinaryVariableName] [varchar](50) NOT NULL,
	[OrdinaryVariableValue] [decimal](29, 10) NULL,
	[BOETaskElementID] [int] NOT NULL,
	[SortByID] [int] NOT NULL,
	[ValueTypeID] [int] NOT NULL,
	[IsPercentage] [bit] NULL,
	Processed bit,
	NewOrdinaryVariableID int,
	NewBOETaskElementID int,
	[DefaultSize] varchar(200)
)
INSERT INTO @OrdinaryVariable	
SELECT OV.[OrdinaryVariableID]
      ,OV.[UpdateDT]
      ,OV.[OrdinaryVariableName]
      ,OV.[OrdinaryVariableValue]
      ,OV.[BOETaskElementID]
      ,OV.[SortByID]
      ,1 --OV.[ValueTypeID] Convert all to discrete
      ,OV.[IsPercentage]
      ,0
      ,NULL
      ,TE.NewBOETaskElementID
	  ,OV.[DefaultSize]
  FROM [GenBOE].[dbo].[OrdinaryVariable] OV
INNER JOIN @BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID

DECLARE @OrdinaryVariableID int
WHILE EXISTS (SELECT 1 FROM @OrdinaryVariable WHERE Processed = 0)
BEGIN
SELECT TOP 1 @OrdinaryVariableID = OrdinaryVariableID FROM @OrdinaryVariable WHERE Processed = 0
INSERT INTO [dbo].[OrdinaryVariable]
           ([UpdateDT]
           ,[OrdinaryVariableName]
           ,[OrdinaryVariableValue]
           ,[BOETaskElementID]
           ,[SortByID]
           ,[ValueTypeID]
           ,[IsPercentage]
		   ,[DefaultSize]
		   )
SELECT [UpdateDT]
      ,[OrdinaryVariableName]
      ,[OrdinaryVariableValue]
      ,NewBOETaskElementID--[BOETaskElementID]
      ,[SortByID]
      ,[ValueTypeID]
      ,[IsPercentage]
	  ,[DefaultSize]
  FROM @OrdinaryVariable 
WHERE OrdinaryVariableID = @OrdinaryVariableID  


UPDATE @OrdinaryVariable
SET NewOrdinaryVariableID = SCOPE_IDENTITY(),
	Processed = 1
WHERE OrdinaryVariableID = @OrdinaryVariableID

END


INSERT INTO [dbo].[BOEApprovalHistory]
           ([UpdateDT]
           ,[BOEID]
           ,[Approval]
           ,[ApprovalETIUserID])
SELECT H.[UpdateDT]
      ,B.NewBOEID--[BOEID]
      ,H.[Approval]
      ,U.NewUserID --H.[ApprovalETIUserID]
  FROM [GenBOE].[dbo].[BOEApprovalHistory] H
INNER JOIN @BOE B ON H.BOEID = B.BOEID
LEFT JOIN @UserXREF U on H.ApprovalETIUserID = U.IsgsUserID



--START LABOR TYPE
DECLARE @BOELaborType TABLE 
(
	[BOELaborTypeID] [int] NOT NULL,
	[UpdateDT] [datetime2](7) NOT NULL,
	[ResourceID] [int] NULL,
	[PerformingOrganizationID] [int] NULL,
	[BOELaborTypeStartDate] [date] NOT NULL,
	[BOELaborTypeEndDate] [date] NOT NULL,
	[SpreadCurveID] [int] NOT NULL,
	[PercentSpread] [decimal](38, 6) NULL,
	[ValueSpread] [DECIMAL] (18, 6) NULL,
	[BOETaskElementID] [int] NULL,
	[SpreadTypeID] [int] NULL,
	[PercentSpreadLocked] [bit] NOT NULL,
	[HourSpreadLocked] [bit] NOT NULL,
	[WBSID] [int] NULL,
	[CLINID] [int] NULL,
	Processed bit,
	[NewBOELaborTypeID] [int],
	[NewResourceID] [int],
	[NewPerformingOrganizationID] [int],
	[NewBOETaskElementID] [int],
	[NewWBSID] [int] NULL,
	[NewCLINID] [int] NULL
)	
INSERT INTO @BOELaborType
SELECT LT.[BOELaborTypeID]
      ,LT.[UpdateDT]
      ,LT.[ResourceID]
      ,LT.[PerformingOrganizationID]
      ,LT.[BOELaborTypeStartDate]
      ,LT.[BOELaborTypeEndDate]
      ,LT.[SpreadCurveID]
      ,LT.[PercentSpread]
      ,LT.[ValueSpread]
      ,LT.[BOETaskElementID]
      ,LT.[SpreadTypeID]
      ,LT.[PercentSpreadLocked]
      ,LT.[HourSpreadLocked]
	  ,LT.[WBSID]
	  ,LT.[CLINID]
      ,0/*PROCESSED*/
      ,NULL--[NewBOELaborTypeID]
      ,R.NewSystemResourceID
      ,PO.NewSystemPerformingOrganizationID
      ,TE.NewBOETaskElementID
	  ,W.NewWBSID
	  ,C.NewCLINID
  FROM [GenBOE].[dbo].[BOELaborType] LT
INNER JOIN @BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
LEFT OUTER JOIN @WorkspaceResource R ON LT.ResourceID = R.SystemResourceID
LEFT OUTER JOIN @WorkspacePerformingOrganization PO ON LT.PerformingOrganizationID = PO.SystemPerformingOrganizationID
LEFT OUTER JOIN @WorkBreakdownStructure W on LT.WBSID = W.WBSID
LEFT OUTER JOIN @CLIN C on LT.CLINID = C.CLINID
	
DECLARE @BOELaborTypeID int
WHILE EXISTS (SELECT 1 FROM @BOELaborType WHERE Processed = 0)
BEGIN
SELECT TOP 1 @BOELaborTypeID = BOELaborTypeID FROM @BOELaborType WHERE Processed = 0
INSERT INTO [dbo].[BOELaborType]
           ([UpdateDT]
           ,[ResourceID]
           ,[PerformingOrganizationID]
           ,[BOELaborTypeStartDate]
           ,[BOELaborTypeEndDate]
           ,[SpreadCurveID]
           ,[PercentSpread]
           ,[ValueSpread]
           ,[BOETaskElementID]
           ,[SpreadTypeID]
           ,[PercentSpreadLocked]
           ,[HourSpreadLocked]
		   ,[WBSID]
		   ,[CLINID])
SELECT [UpdateDT]
      ,NewResourceID
      ,NewPerformingOrganizationID
      ,[BOELaborTypeStartDate]
      ,[BOELaborTypeEndDate]
      ,[SpreadCurveID]
      ,[PercentSpread]
      ,[ValueSpread]
      ,NewBOETaskElementID
      ,[SpreadTypeID]
      ,[PercentSpreadLocked]
      ,[HourSpreadLocked]
	  ,NewWBSID
	  ,NewCLINID
  FROM @BOELaborType
WHERE  [BOELaborTypeID] = @BOELaborTypeID
      
UPDATE @BOELaborType
SET [NewBOELaborTypeID] = SCOPE_IDENTITY(),
	Processed = 1
WHERE  [BOELaborTypeID] = @BOELaborTypeID

END

--STOP LABOR TYPE

INSERT INTO [dbo].[BOETaskElementWorkspaceVariableXREF]
           ([BOETaskElementID]
           ,[WorkspaceVariableID])
SELECT TE.NewBOETaskElementID--[BOETaskElementID]
      ,WV.NewWorkspaceVariableID--[WorkspaceVariableID]
  FROM [GenBOE].[dbo].[BOETaskElementWorkspaceVariableXREF] X
	INNER JOIN @BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID
	INNER JOIN @WorkspaceVariable WV ON X.WorkspaceVariableID = WV.WorkspaceVariableID



--START Custom field selections
INSERT INTO [dbo].[BOETaskElementCustomFieldValueXREF]
           ([UpdateDT]
           ,[BOETaskElementID]
           ,[CustomFieldValueID])
SELECT X.[UpdateDT]
      ,TE.NewBOETaskElementID--[BOETaskElementID]
      ,CFV.NewCustomFieldValueID--[CustomFieldValueID]
  FROM [GenBOE].[dbo].[BOETaskElementCustomFieldValueXREF] X
	INNER JOIN @BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID
	INNER JOIN @CustomFieldValue CFV ON X.CustomFieldValueID = CFV.CustomFieldValueID

INSERT INTO [dbo].[BOELaborTypeCustomFieldValueXREF]
           ([UpdateDT]
           ,[BOELaborTypeID]
           ,[CustomFieldValueID])
SELECT X.[UpdateDT]
      ,TE.NewBOELaborTypeID--[BOELaborTypeID]
      ,CFV.NewCustomFieldValueID--[CustomFieldValueID]
  FROM [GenBOE].[dbo].[BOELaborTypeCustomFieldValueXREF] X
	INNER JOIN @BOELaborType TE ON X.BOELaborTypeID = TE.BOELaborTypeID
	INNER JOIN @CustomFieldValue CFV ON X.CustomFieldValueID = CFV.CustomFieldValueID

INSERT INTO [dbo].[BOECustomFieldValueXREF]
           ([UpdateDT]
           ,[BOEID]
           ,[CustomFieldValueID])
SELECT X.[UpdateDT]
      ,B.NewBOEID--[BOEID]
      ,CFV.NewCustomFieldValueID--[CustomFieldValueID]
  FROM [GenBOE].[dbo].[BOECustomFieldValueXREF] X
	INNER JOIN @BOE B ON X.BOEID = B.BOEID
	INNER JOIN @CustomFieldValue CFV ON X.CustomFieldValueID = CFV.CustomFieldValueID
--STOP custom field selections

INSERT INTO [dbo].[BOELaborSpread]
           ([BOELaborTypeID]
           ,[LaborSpreadDate]
           ,[LaborSpreadValue])
SELECT LT.NewBOELaborTypeID--[BOELaborTypeID]
      ,[LaborSpreadDate]
      ,[LaborSpreadValue]
  FROM [GenBOE].[dbo].[BOELaborSpread] LS
	INNER JOIN @BOELaborType LT ON LS.BOELaborTypeID = LT.BOELaborTypeID

UPDATE [dbo].[RealignedWorkspaceXREF]
SET NewWorkspaceID = @NewWorkspaceID,
UpdateDT = GetDate()
WHERE IsgsWorkspaceID = @WorkspaceID
AND NewWorkspaceID IS NULL

--Add default DTC elements to workspace
DECLARE @UpdateDT DATETIME = GetDate()
EXECUTE [dbo].[insertDefaultWorkspaceDTCElement] @NewWorkspaceID, @UpdateDT

IF @@ERROR = 0
	BEGIN
		COMMIT TRANSACTION
	END        

END TRY

BEGIN CATCH
	ROLLBACK TRANSACTION
	

	DECLARE @ErrorMessage varchar (500)
	SELECT @ErrorMessage = ERROR_MESSAGE()
	RAISERROR (
			@ErrorMessage, -- Message text.
	        11, -- Severity,/*Severity Changed to 11*/
			1 -- State,
			)

	UPDATE [dbo].[RealignedWorkspaceXREF]
	SET NewWorkspaceID = 0,
	UpdateDT = GetDate()
	WHERE IsgsWorkspaceID = @WorkspaceID

		
	RETURN
	
END CATCH
' 
GO
EXEC dbo.sp_executesql @statement = N'
CREATE PROCEDURE [dbo].[RealignWorkspacesJob]
AS
/******************************************************************************
**		 
**		Name: [RealignWorkspacesJob]
**		Desc: 
**		Copies all system level Resources and Performing Orgs from ISGS to the 
**		current database (MST or SSC) if they do not already exist. Then, it 
**		copies Workspaces from ISGS that have been queued up in the 
**		RealignedWorkspaceXREF table. Any ISGS workspaces in the table that do
**		not have a NewWorkspaceID populated will be copied into the current
**		databse. 
**
**		Auth: Michael Basquill
**		Date: 2/16/2016
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		2/16/16		mbasquil			Original Version
*******************************************************************************/
BEGIN

SET NOCOUNT ON;

DECLARE @TransitionedSegmentID INT

--SET COMPANY SPECIFIC DEFAULTS
declare @CurrentDb varchar(30)
select @CurrentDb = db_name()


IF (@CurrentDb = ''GenBOESpace'')
BEGIN
	PRINT ''THIS IS SPACE''
	SELECT @TransitionedSegmentID = 1001
END
ELSE IF (@CurrentDb = ''GenBOEMST'')
BEGIN
	PRINT ''THIS IS MST''
	SELECT @TransitionedSegmentID = 2001
END
ELSE
BEGIN
	PRINT ''ERROR: THIS CAN ONLY RUN IN GenBOESpace OR GenBOEMST''
	Return
END
--STOP COMPANY SPECIFIC DEFAULTS

	--MAKE SURE ALL SYSTEM PERFORMING ORGS ARE COPIED OVER


	DECLARE @IsgsPerfOrgListName varchar(50) = ''IS&GS Performing Organizations''
	DECLARE @IsgsPerfOrgListID int

	select @IsgsPerfOrgListID = PerformingOrganizationListID
	from PerformingOrganizationList
	where PerformingOrganizationListName = @IsgsPerfOrgListName

	IF @IsgsPerfOrgListID IS NULL
		BEGIN
			insert into PerformingOrganizationList 
			(
			[UpdateDT]
			,[PerformingOrganizationListName]
			) 
			values (
			GETDATE()
			,@IsgsPerfOrgListName
			)
			SELECT @IsgsPerfOrgListID = SCOPE_IDENTITY()
		END


	DECLARE @SystemPerformingOrganization TABLE
	(
		[PerformingOrganizationID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[PerformingOrganizationName] [varchar](20) NOT NULL,
		[PerformingOrganizationDescription] [varchar](50) NULL,
		[PerformingOrganizationListID] [int] NOT NULL,
		[DeletedFlag] [bit] NULL,
		Processed bit DEFAULT 0,
		NewPerformingOrganizationListID int,
		NewWorkspaceID int,
		NewPerformingOrganizationID int
	)

	INSERT INTO @SystemPerformingOrganization
			   ([PerformingOrganizationID]
			   ,[UpdateDT]
			   ,[PerformingOrganizationName]
			   ,[PerformingOrganizationDescription]
			   ,[PerformingOrganizationListID]
			   ,[DeletedFlag]
           		,Processed
				,NewPerformingOrganizationID)
	SELECT R.[PerformingOrganizationID]
		  ,R.[UpdateDT]
		  ,R.[PerformingOrganizationName]
		  ,R.[PerformingOrganizationDescription]
		  ,R.[PerformingOrganizationListID]
		  ,R.[DeletedFlag]
		  ,0 AS Processed
		  ,NULL AS NewPerformingOrganizationID
	  FROM [GenBOE].[dbo].[PerformingOrganization] R
	  LEFT JOIN [dbo].[IsgsPerfOrgXREF] PT on R.PerformingOrganizationID = PT.ISGSPerfOrgID
	  WHERE R.PerformingOrganizationListID = 1 --MASTER SYSTEM RESOURCE LIST
	  AND PT.ISGSPerfOrgID IS NULL



	DECLARE @PerformingOrganizationID int
	WHILE EXISTS (SELECT 1 FROM @SystemPerformingOrganization WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @PerformingOrganizationID = PerformingOrganizationID FROM @SystemPerformingOrganization WHERE Processed = 0

	DECLARE @NewPerformingOrganizationID int
	INSERT INTO [dbo].[PerformingOrganization]
			   (
				[UpdateDT]
			   ,[PerformingOrganizationName]
			   ,[PerformingOrganizationDescription]
			   ,[PerformingOrganizationListID]
			   ,[DeletedFlag])
	SELECT 
		  R.[UpdateDT]
		  ,R.[PerformingOrganizationName]
		  ,R.[PerformingOrganizationDescription]
		  ,@IsgsPerfOrgListID
		  ,R.[DeletedFlag]
	FROM @SystemPerformingOrganization R
	WHERE
		PerformingOrganizationID = @PerformingOrganizationID
	SELECT  @NewPerformingOrganizationID = SCOPE_IDENTITY() 


	INSERT INTO [dbo].[IsgsPerfOrgXREF]
	(
	ISGSPerfOrgID 
	,NewPerfOrgID
	)
	VALUES
	(
	@PerformingOrganizationID
	,@NewPerformingOrganizationID
	)

	UPDATE @SystemPerformingOrganization 
	SET	NewPerformingOrganizationID = @NewPerformingOrganizationID,
		Processed = 1
	WHERE 
		PerformingOrganizationID = @PerformingOrganizationID
	END
	--STOP SYSTEM PERF ORG COPY



	--START MAKE SURE ALL SYSTEM RESOURCES ARE COPIED
	DECLARE @IsgsListName varchar(50) = ''IS&GS Resources''
	DECLARE @IsgsResourceListID int

	select @IsgsResourceListID = ResourceListID
	from ResourceList
	where ResourceListName = @IsgsListName

	IF @IsgsResourceListID IS NULL
		BEGIN
			insert into ResourceList 
			(
			[UpdateDT]
			,[ResourceListName]
			) 
			values (
			GETDATE()
			,@IsgsListName
			)
			SELECT @IsgsResourceListID = SCOPE_IDENTITY()
		END


	DECLARE @SystemResource TABLE
	(
		[ResourceID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[ResourceName] [varchar](20) NOT NULL,
		[ResourceDescription] [varchar](100) NULL,
		[SegmentRegion] [varchar](50) NULL,
		[LaborType] [varchar](50) NULL,
		[SegmentID] [int] NULL,
		[ResourceListID] [int] NOT NULL,
		[CostElementID] [int] NOT NULL,
		[DeletedFlag] [bit] NULL,
		[RateTypeID] [int] NULL,
		ResourceProcessed bit DEFAULT 0,
		NewResourceID int
	)

	INSERT INTO @SystemResource
			   ([ResourceID]
			   ,[UpdateDT]
			   ,[ResourceName]
			   ,[ResourceDescription]
			   ,[SegmentRegion]
			   ,[LaborType]
			   ,[SegmentID]
			   ,[ResourceListID]
			   ,[CostElementID]
			   ,[DeletedFlag]
			   ,[RateTypeID]
           		,ResourceProcessed
				,NewResourceID)
	SELECT R.[ResourceID]
		  ,R.[UpdateDT]
		  ,R.[ResourceName]
		  ,R.[ResourceDescription]
		  ,R.[SegmentRegion]
		  ,R.[LaborType]
		  ,R.[SegmentID]
		  ,R.[ResourceListID]
		  ,R.[CostElementID]
		  ,R.[DeletedFlag]
		  ,R.[RateTypeID]
		  ,0 AS ResourceProcessed
		  ,NULL AS NewResourceID
	  FROM [GenBOE].[dbo].[Resource] R
		LEFT JOIN [dbo].[IsgsResourceXREF] RL ON R.ResourceID = RL.ISGSResourceID
	WHERE R.ResourceListID = 1 --MASTER SYSTEM RESOURCE LIST
	and RL.ISGSResourceID IS NULL

	DECLARE @ResourceID int
	WHILE EXISTS (SELECT 1 FROM @SystemResource WHERE ResourceProcessed = 0)
	BEGIN
	SELECT TOP 1 @ResourceID = ResourceID FROM @SystemResource WHERE ResourceProcessed = 0

	DECLARE @NewResourceID int
	INSERT INTO [dbo].[Resource]
			   (
				[UpdateDT]
			   ,[ResourceName]
			   ,[ResourceDescription]
			   ,[SegmentRegion]
			   ,[LaborType]
			   ,[SegmentID]
			   ,[ResourceListID]
			   ,[CostElementID]
			   ,[DeletedFlag]
			   ,[RateTypeID]
			   )
	SELECT 
		  R.[UpdateDT]
		  ,R.[ResourceName]
		  ,R.[ResourceDescription]
		  ,R.[SegmentRegion]
		  ,R.[LaborType]
		  ,@TransitionedSegmentID
		  ,@IsgsResourceListID
		  ,R.[CostElementID]
		  ,R.[DeletedFlag]
		  ,R.[RateTypeID]
	FROM @SystemResource R
	WHERE
		ResourceID = @ResourceID
	SELECT  @NewResourceID = SCOPE_IDENTITY() 


	INSERT INTO [dbo].[IsgsResourceXREF]
	(
	ISGSResourceID 
	,NewResourceID
	)
	VALUES
	(
	@ResourceID
	,@NewResourceID
	)


	UPDATE @SystemResource 
	SET	NewResourceID = @NewResourceID,
		ResourceProcessed = 1
	WHERE 
		ResourceID = @ResourceID
	END	
	--STOP SYSTEM RESOURCE COPY



	--WORKSPACE SPECIFIC COPY STARTS HERE
	DECLARE @WorkspaceID int
	WHILE EXISTS (SELECT 1 FROM [dbo].[RealignedWorkspaceXREF] WHERE NewWorkspaceID IS NULL)
	BEGIN
	SELECT TOP 1 @WorkspaceID = IsgsWorkspaceID FROM [dbo].[RealignedWorkspaceXREF] WHERE NewWorkspaceID IS NULL
	print ''Moving ISGS Workspace: '' + CAST(@WorkspaceID AS varchar)

	EXEC dbo.RealignWorkspace @WorkspaceID
	END
END
' 
GO