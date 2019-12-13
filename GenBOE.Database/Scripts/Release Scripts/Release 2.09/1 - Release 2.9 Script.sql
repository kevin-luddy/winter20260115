/*
	## START OF SCRIPTS FROM MIKE ##

	10/22/2015 [Dusan]: BOEJ-341, BOEJ-342 -> Scripts from Mike & Matt


-- FILE 1 - Add default elements to workspaces

The "Multi BOE" feature allows a WBS/CLIN to be assigned at the resource type level.
In order for a BOE to use this feature, it must be marked as "MULTI." A "Multi" BOE must
have its WBS and CLIN set to "MULTI." This script creates default MULTI WBS and CLIN
elements for all existing workspaces, ans well as all existing workspace versions.

Going forward, any newly created workspaces will have these default elements
created upon workspace creation, as the upsertWorkspace SP has been modified
to call the insertDefaultMultiClinWBS stored procedure.
*/

DECLARE @WBSLoop int
DECLARE @WSLoop int
DECLARE @VersionLoop int
DECLARE @ClinLoop int

--Add default "Multi" WBS Elements to DBO table for all existing workspaces
SET @WBSLoop = (SELECT MIN(WorkspaceID) FROM dbo.workspace)
WHILE @WBSLoop is not null 
BEGIN
	IF NOT EXISTS (SELECT 1
		FROM dbo.WorkBreakdownStructure
		WHERE WorkspaceID = @WBSLoop
		AND WBSNumber = 'MULTI')
	BEGIN
		DECLARE @UpdateDT datetime = GETDATE() 
		insert into dbo.WorkBreakdownStructure (UpdateDT,WBSNumber, DisplayedWBSNumber, WBSTitle, WorkspaceID, WorkspaceDTCElementID)
			VALUES (@UpdateDT, 'MULTI', 'MULTI', 'MULTI', @WBSLoop, null);
	END

	SET @WBSLoop = (SELECT MIN(WorkspaceID) FROM dbo.workspace
		WHERE @WBSLoop < WorkspaceID)
END

--Add default "Multi" WBS Elements to Version table for all workspace versions
SET @WSLoop = (SELECT MIN(WorkspaceID) FROM version.Workspace)
WHILE @WSLoop is not null 
BEGIN
	SET @VersionLoop = (SELECT MIN(VersionID) FROM version.Workspace
	where WorkspaceID = @WSLoop)
	WHILE @VersionLoop is not null
	BEGIN
		IF NOT EXISTS (SELECT 1
			FROM version.WorkBreakdownStructure
			WHERE WorkspaceID = @WSLoop
			AND VersionID = @VersionLoop
			AND WBSNumber = 'MULTI')
		BEGIN
			insert into version.WorkBreakdownStructure (WBSID, UpdateDT,WBSNumber, DisplayedWBSNumber, WBSTitle, WorkspaceID,VersionID, WorkspaceDTCElementID)
				(select WBSID, UpdateDT,WBSNumber, DisplayedWBSNumber, WBSTitle, @WSLoop,@VersionLoop, WorkspaceDTCElementID
					from dbo.WorkBreakdownStructure
					where WorkspaceID = @WSLoop
					and WBSNumber = 'MULTI'
					);
		END --if not exists
		SET @VersionLoop = (SELECT MIN(VersionID) FROM version.Workspace
		WHERE @VersionLoop < VersionID
		and workspaceid = @WSLoop)
	END --version
	SET @WSLoop = (SELECT MIN(WorkspaceID) FROM version.Workspace
	WHERE @WSLoop < WorkspaceID)
END --workspace while

--Add default "Multi" CLIN Elements to DBO table for all existing workspaces
SET @ClinLoop = (SELECT MIN(WorkspaceID) FROM dbo.workspace)
WHILE @ClinLoop is not null
BEGIN
	IF NOT EXISTS (SELECT 1 
		FROM dbo.CLIN
		WHERE WorkspaceID = @ClinLoop
		AND CLINNumber = 'MULTI')
	BEGIN
		DECLARE @UpdateDT2 datetime = GETDATE() 
		insert into dbo.CLIN (UpdateDT,CLINNumber, ClinTitle, CLINStartDate, CLINEndDate, WorkspaceID, DisplayedCLINNumber)
			VALUES (@UpdateDT2, 'MULTI', 'MULTI', NULL,NULL,  @ClinLoop, 'MULTI');
	END
	
	SET @ClinLoop = (SELECT MIN(WorkspaceID) FROM dbo.workspace
		WHERE @ClinLoop < WorkspaceID)
END

--Add default "Multi" CLIN Elements to Version table for all versions
SET @WSLoop = (SELECT MIN(WorkspaceID) FROM version.Workspace)
WHILE @WSLoop is not null 
BEGIN
               SET @VersionLoop = (SELECT MIN(VersionID) FROM version.Workspace
               where WorkspaceID = @WSLoop)
               WHILE @VersionLoop is not null
               BEGIN
                              IF NOT EXISTS (SELECT 1
                                             FROM version.CLIN
                                             WHERE WorkspaceID = @WSLoop
                                             AND VersionID = @VersionLoop
                                             AND CLINNumber = 'MULTI')
                              BEGIN
                                             insert into version.CLIN(CLINID, UpdateDT,CLINNumber, CLINTitle, CLINStartDate, CLINEndDate, WorkspaceID,VersionID, DisplayedCLINNumber)
                                             (select CLINID, UpdateDT,CLINNumber, CLINTitle, NULL, NULL, @WSLoop, @VersionLoop, DisplayedCLINNumber 
                                             from dbo.CLIN
                                             where WorkspaceID = @WSLoop
                                             and CLINNumber = 'MULTI');
                              END --if not exists
                              SET @VersionLoop = (SELECT MIN(VersionID) FROM version.Workspace
                              WHERE @VersionLoop < VersionID
                              and workspaceid = @WSLoop)
               END --version
               SET @WSLoop = (SELECT MIN(WorkspaceID) FROM version.Workspace
               WHERE @WSLoop < WorkspaceID)
END --workspace while

GO

/*
-- FILE 2 - BOE and BOELaborType Table Updates

Add IsMultiClinWbs field to BOE table.
Field is not nullable and should default to 0.
Field should also be in version table.
*/

IF NOT EXISTS (
				SELECT * FROM sys.all_columns C
					INNER JOIN sys.tables T on C.object_id = T.object_id
					INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
				WHERE
					T.name = 'BOE' AND
					C.name = 'IsMultiClinWbs' AND
					S.name = 'dbo'
				)
	BEGIN
		ALTER TABLE [dbo].[BOE] ADD [IsMultiClinWbs] bit NOT NULL CONSTRAINT [DF_BOE_IsMultiClinWbs] DEFAULT 0 
	END
GO
IF NOT EXISTS (
				SELECT * FROM sys.all_columns C
					INNER JOIN sys.tables T on C.object_id = T.object_id
					INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
				WHERE
					T.name = 'BOE' AND
					C.name = 'IsMultiClinWbs' AND
					S.name = 'version'
				)
	BEGIN
		ALTER TABLE [version].[BOE] ADD [IsMultiClinWbs] [bit]
	END
GO

/*
  Add WBSID, CLINID to BOELaborType table.
	Fields are FK to WorkBreadownStructure and CLINID
	Fields are nullable
*/

IF NOT EXISTS (
				SELECT * FROM sys.all_columns C
					INNER JOIN sys.tables T on C.object_id = T.object_id
					INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
				WHERE
					T.name = 'BOELaborType' AND
					C.name = 'WBSID' AND
					S.name = 'dbo'
				)
	BEGIN
		ALTER TABLE [dbo].[BOELaborType] ADD [WBSID] [int]
	END
GO
IF NOT EXISTS (
				SELECT * FROM sys.all_columns C
					INNER JOIN sys.tables T on C.object_id = T.object_id
					INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
				WHERE
					T.name = 'BOELaborType' AND
					C.name = 'WBSID' AND
					S.name = 'version'
				)
	BEGIN
		ALTER TABLE [version].[BOELaborType] ADD [WBSID] [int]
	END
GO
IF NOT EXISTS (
				SELECT * FROM sys.all_columns C
					INNER JOIN sys.tables T on C.object_id = T.object_id
					INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
				WHERE
					T.name = 'BOELaborType' AND
					C.name = 'CLINID' AND
					S.name = 'dbo'
				)
	BEGIN
		ALTER TABLE [dbo].[BOELaborType] ADD [CLINID] [int]
	END
GO
IF NOT EXISTS (
				SELECT * FROM sys.all_columns C
					INNER JOIN sys.tables T on C.object_id = T.object_id
					INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
				WHERE
					T.name = 'BOELaborType' AND
					C.name = 'CLINID' AND
					S.name = 'version'
				)
	BEGIN
		ALTER TABLE [version].[BOELaborType] ADD [CLINID] [int]
	END
GO

-- Add the Foreign Key constraints for WBSID and CLINID
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_BOELaborType_WorkBreakdownStructure]') AND parent_object_id = OBJECT_ID(N'[dbo].[BOELaborType]'))
	ALTER TABLE [dbo].[BOELaborType] DROP CONSTRAINT [FK_BOELaborType_WorkBreakdownStructure]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_BOELaborType_WorkBreakdownStructure]') AND parent_object_id = OBJECT_ID(N'[dbo].[BOELaborType]'))
	ALTER TABLE [dbo].[BOELaborType]  WITH CHECK ADD  CONSTRAINT [FK_BOELaborType_WorkBreakdownStructure] FOREIGN KEY([WBSID])
			REFERENCES [dbo].[WorkBreakdownStructure] ([WBSID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_BOELaborType_WorkBreakdownStructure]') AND parent_object_id = OBJECT_ID(N'[dbo].[BOELaborType]'))
	ALTER TABLE [dbo].[BOELaborType] CHECK CONSTRAINT [FK_BOELaborType_WorkBreakdownStructure]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_BOELaborType_CLIN]') AND parent_object_id = OBJECT_ID(N'[dbo].[BOELaborType]'))
	ALTER TABLE [dbo].[BOELaborType] DROP CONSTRAINT [FK_BOELaborType_CLIN]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_BOELaborType_CLIN]') AND parent_object_id = OBJECT_ID(N'[dbo].[BOELaborType]'))
	ALTER TABLE [dbo].[BOELaborType]  WITH CHECK ADD  CONSTRAINT [FK_BOELaborType_CLIN] FOREIGN KEY([CLINID])
REFERENCES [dbo].[CLIN] ([CLINID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_BOELaborType_CLIN]') AND parent_object_id = OBJECT_ID(N'[dbo].[BOELaborType]'))
	ALTER TABLE [dbo].[BOELaborType] CHECK CONSTRAINT [FK_BOELaborType_CLIN]
GO

/*
	FILE 3 - Update TT and related SPs
	FILE 4 - insertDefaultMultiClinWBS
	FILE 5 - copyWorkspace
	FILE 6 - createSystemWorkspaceVersion
	FILE 7 - createWorkspaceVersion
	FILE 8 - getLaborType
	FILE 9 - restoreWorkspaceVersion
	FILE 10 - upsertBOE
	FILE 11 - upsertBOELaborType
	FILE 12 - upsertWorkspace
	   -- all changes were moved into the individual objects..

	10/22/2015 [Dusan]: BOEJ-341, BOEJ-342 -> Scripts from Mike & Matt

	## END OF SCRIPTS FROM MIKE ##

	10/26/2015 [Dusan]: Mike's changes for BOEJ-471.
      -- was moved into the individual objects..
	10/28/2015 [Dusan]: Mike's changes for BOEJ-473.
	  -- was moved into the individual objects..
*/

/*
	## START FOR BOEJ-511 ##

	11/12/2015 [Dusan]: BOEJ-511 -> Some of the resource Type Start/End dates as well as Resource Spread dates are not set to the 15th of the month.
*/

PRINT '### Normalizing Resource Type and Spread dates (Labor and ODC). This may take a little while, depending on the data. ###';
GO
UPDATE dbo.BoeLaborType
	SET BoeLaborTypeStartDate = DATEADD(DAY, 15 - DAY(BoeLaborTypeStartDate), BoeLaborTypeStartDate),
		BoeLaborTypeEndDate = DATEADD(DAY, 15 - DAY(BoeLaborTypeEndDate), BoeLaborTypeEndDate) 
	WHERE DAY(BoeLaborTypeStartDate) <> 15
		OR DAY(BoeLaborTypeEndDate) <> 15;
GO
UPDATE dbo.BoeLaborSpread
	SET LaborSpreadDate = DATEADD(DAY, 15 - DAY(LaborSpreadDate), LaborSpreadDate) 
	WHERE DAY(LaborSpreadDate) <> 15;
GO
UPDATE dbo.ODCTaskElement
	SET TaskStartDate = DATEADD(DAY, 15 - DAY(TaskStartDate), TaskStartDate),
		TaskEndDate = DATEADD(DAY, 15 - DAY(TaskEndDate), TaskEndDate) 
	WHERE DAY(TaskStartDate) <> 15
		OR DAY(TaskEndDate) <> 15;
GO
UPDATE dbo.ODCSpread
	SET ODCSpreadDate = DATEADD(DAY, 15 - DAY(ODCSpreadDate), ODCSpreadDate) 
	WHERE DAY(ODCSpreadDate) <> 15;
GO
PRINT '';
PRINT '### Done w/ date normalization. ###';
GO

/*
	11/12/2015 [Dusan]: BOEJ-511 -> Resource Type and spread dates are not set to the 15th of the month.

	## END FOR BOEJ-511 ##
*/

/*
	## START OF INDEX UPDATES ##

	12/03/2015 [Dusan]: Dan's indexing changes from 12/1/2015.
*/
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[BOELaborType]') AND name = N'IX_BOELaborType__PerformingOrganizationID') 
	DROP INDEX [IX_BOELaborType__PerformingOrganizationID] ON [dbo].[BOELaborType] WITH ( ONLINE = OFF )
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[IndexFragmentation]') AND name = N'IX_IndexFragmentation__TableName_IndexName_FillFactor') 
	DROP INDEX [IX_IndexFragmentation__TableName_IndexName_FillFactor] ON [dbo].[IndexFragmentation] WITH ( ONLINE = OFF )
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Resource]') AND name = N'IX_Resource__RateTypeID') 
	DROP INDEX [IX_Resource__RateTypeID] ON [dbo].[Resource] WITH ( ONLINE = OFF )
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[TravelTrip]') AND name = N'IX_TravelTrip__TripID') 
	DROP INDEX [IX_TravelTrip__TripID] ON [dbo].[TravelTrip] WITH ( ONLINE = OFF )
GO

CREATE INDEX [IX_BOELaborType__PerformingOrganizationID] ON [dbo].[BOELaborType] ([PerformingOrganizationID]) INCLUDE ([BOETaskElementID]);
CREATE INDEX [IX_IndexFragmentation__TableName_IndexName_FillFactor] ON [dbo].[IndexFragmentation] ([TableName], [IndexName], [FillFactor]) INCLUDE ([Processed]);
CREATE INDEX [IX_Resource__RateTypeID] ON [dbo].[Resource] ([RateTypeID]) INCLUDE ([ResourceID], [CostElementID]);
CREATE INDEX [IX_TravelTrip__TripID] ON [dbo].[TravelTrip] ([TripID]) INCLUDE ([TravelTripID], [UpdateDT], [GroupID], [SegmentID], [PerformingOrganizationID], [TripDate], [NumTrips], [NumPeople], [NumDays], [Purpose], [TravelTripTaskElementID], [TripLockedDT]);
GO

/*
	12/03/2015 [Dusan]: Dan's indexing changes from 12/1/2015.

	## END OF INDEX UPDATES ##
*/

/*
	## START OF INDEX UPDATES ##

	01/04/2016 [Dusan]: Dan's indexing changes from 1/1/2016.
*/

IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[BOECommentHistory]') AND name = N'IX_BOECommentHistory__BOECommentID') 
	DROP INDEX [IX_BOECommentHistory__BOECommentID] ON [dbo].[BOECommentHistory] WITH ( ONLINE = OFF )
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[OutputFormatTemplateWorkspaceXREF]') AND name = N'IX_OutputFormatTemplateWorkspaceXREF__WorkspaceID') 
	DROP INDEX [IX_OutputFormatTemplateWorkspaceXREF__WorkspaceID] ON [dbo].[OutputFormatTemplateWorkspaceXREF] WITH ( ONLINE = OFF )
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PerformingOrganization]') AND name = N'IX_PerformingOrganization__PerformingOrganizationListID_DeletedFlag') 
	DROP INDEX [IX_PerformingOrganization__PerformingOrganizationListID_DeletedFlag] ON [dbo].[PerformingOrganization] WITH ( ONLINE = OFF )
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProPricerFieldXREF]') AND name = N'IX_ProPricerFieldXREF__ProPricerExportID_ProPricerFieldID_ProPricerTypeID') 
	DROP INDEX [IX_ProPricerFieldXREF__ProPricerExportID_ProPricerFieldID_ProPricerTypeID] ON [dbo].[ProPricerFieldXREF] WITH ( ONLINE = OFF )
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[SumOfBOE_OrdinaryVariableXREF]') AND name = N'IX_SumOfBOE_OrdinaryVariableXREF__BOEID') 
	DROP INDEX [IX_SumOfBOE_OrdinaryVariableXREF__BOEID] ON [dbo].[SumOfBOE_OrdinaryVariableXREF] WITH ( ONLINE = OFF )
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[WorkspaceLockedPerDiem]') AND name = N'IX_WorkspaceLockedPerDiem__WorkspaceID_PerDiemID') 
	DROP INDEX [IX_WorkspaceLockedPerDiem__WorkspaceID_PerDiemID] ON [dbo].[WorkspaceLockedPerDiem] WITH ( ONLINE = OFF )
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[WorkspaceLockedTravelEscalationRate]') AND name = N'IX_WorkspaceLockedTravelEscalationRate__WorkspaceID') 
	DROP INDEX [IX_WorkspaceLockedTravelEscalationRate__WorkspaceID] ON [dbo].[WorkspaceLockedTravelEscalationRate] WITH ( ONLINE = OFF )
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[WorkspaceLockedTrip]') AND name = N'IX_WorkspaceLockedTrip__WorkspaceID_TripID') 
	DROP INDEX [IX_WorkspaceLockedTrip__WorkspaceID_TripID] ON [dbo].[WorkspaceLockedTrip] WITH ( ONLINE = OFF )
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[WorkspacePerformingOrganization]') AND name = N'IX_WorkspacePerformingOrganization__SystemPerformingOrganizationID') 
	DROP INDEX [IX_WorkspacePerformingOrganization__SystemPerformingOrganizationID] ON [dbo].[WorkspacePerformingOrganization] WITH ( ONLINE = OFF )
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[WorkspaceResource]') AND name = N'IX_WorkspaceResource__ResourceListID_2') 
	DROP INDEX [IX_WorkspaceResource__ResourceListID_2] ON [dbo].[WorkspaceResource] WITH ( ONLINE = OFF )
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[WorkspaceResourceRate]') AND name = N'IX_WorkspaceResourceRate__WorkspaceID') 
	DROP INDEX [IX_WorkspaceResourceRate__WorkspaceID] ON [dbo].[WorkspaceResourceRate] WITH ( ONLINE = OFF )
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[WorkspaceResourceRate]') AND name = N'IX_WorkspaceResourceRate__WorkspaceID_SystemResourceID') 
	DROP INDEX [IX_WorkspaceResourceRate__WorkspaceID_SystemResourceID] ON [dbo].[WorkspaceResourceRate] WITH ( ONLINE = OFF )
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[WorkspaceResourceRate]') AND name = N'IX_WorkspaceResourceRate__WorkspaceID_WorkspaceResourceID') 
	DROP INDEX [IX_WorkspaceResourceRate__WorkspaceID_WorkspaceResourceID] ON [dbo].[WorkspaceResourceRate] WITH ( ONLINE = OFF )
GO

CREATE INDEX [IX_BOECommentHistory__BOECommentID] ON [dbo].[BOECommentHistory] ([BOECommentID]) INCLUDE ([UpdateDT], [FieldID], [CurrentComment], [UpdatedComment], [ChangedByETIUserID])
CREATE INDEX [IX_OutputFormatTemplateWorkspaceXREF__WorkspaceID] ON [dbo].[OutputFormatTemplateWorkspaceXREF] ([WorkspaceID])
CREATE INDEX [IX_PerformingOrganization__PerformingOrganizationListID_DeletedFlag] ON [dbo].[PerformingOrganization] ([PerformingOrganizationListID], [DeletedFlag]) INCLUDE ([PerformingOrganizationID])
CREATE INDEX [IX_ProPricerFieldXREF__ProPricerExportID_ProPricerFieldID_ProPricerTypeID] ON [dbo].[ProPricerFieldXREF] ([ProPricerExportID], [ProPricerFieldID], [ProPricerTypeID])
CREATE INDEX [IX_SumOfBOE_OrdinaryVariableXREF__BOEID] ON [dbo].[SumOfBOE_OrdinaryVariableXREF] ([BOEID])
CREATE INDEX [IX_WorkspaceLockedPerDiem__WorkspaceID_PerDiemID] ON [dbo].[WorkspaceLockedPerDiem] ([WorkspaceID],[PerDiemID])INCLUDE ([WorkspaceLockedPerDiemID])
CREATE INDEX [IX_WorkspaceLockedTravelEscalationRate__WorkspaceID] ON [dbo].[WorkspaceLockedTravelEscalationRate] ([WorkspaceID]) INCLUDE ([WorkspaceLockedTravelEscalationRateID], [TravelEscalationRateID], [UpdateDT], [Year], [DevEscalation], [LMSIEscalation])
CREATE INDEX [IX_WorkspaceLockedTrip__WorkspaceID_TripID] ON [dbo].[WorkspaceLockedTrip] ([WorkspaceID],[TripID])  INCLUDE ([WorkspaceLockedTripID])
CREATE INDEX [IX_WorkspacePerformingOrganization__SystemPerformingOrganizationID] ON [dbo].[WorkspacePerformingOrganization] ([SystemPerformingOrganizationID])
CREATE INDEX [IX_WorkspaceResource__ResourceListID_2] ON [dbo].[WorkspaceResource] ([ResourceListID]) INCLUDE ([SystemResourceID], [WorkspaceID])
CREATE INDEX [IX_WorkspaceResourceRate__WorkspaceID] ON [dbo].[WorkspaceResourceRate] ([WorkspaceID]) INCLUDE ([WorkspaceResourceRateID], [UpdateDT], [WorkspaceResourceID], [WorkspaceResourceRateStartDate], [WorkspaceResourceRateEndDate], [WorkspaceResourceRate], [SystemResourceID])
CREATE INDEX [IX_WorkspaceResourceRate__WorkspaceID_SystemResourceID] ON [dbo].[WorkspaceResourceRate] ([WorkspaceID], [SystemResourceID]) INCLUDE ([WorkspaceResourceRateID])
CREATE INDEX [IX_WorkspaceResourceRate__WorkspaceID_WorkspaceResourceID] ON [dbo].[WorkspaceResourceRate] ([WorkspaceID],[WorkspaceResourceID]) INCLUDE ([WorkspaceResourceRateID], [UpdateDT], [WorkspaceResourceRateStartDate], [WorkspaceResourceRateEndDate], [WorkspaceResourceRate], [SystemResourceID]) 
GO

/*
	01/04/2016 [Dusan]: Dan's indexing changes from 1/1/2016.

	## END OF INDEX UPDATES ##
*/

/*
	01/04/2016 [Dusan]: Dan's indexing changes from 1/1/2016.

	## END OF INDEX UPDATES ##
*/

/*
	## START OF Matt's UPDATES ##

	01/26/2016 [Dusan]: Matt's changes related to the SP updates
*/


IF((SELECT COL_LENGTH('dbo.Resource', 'ResourceDescription') AS 'VarChar') = 50)
   BEGIN
     ALTER TABLE dbo.Resource
         ALTER COLUMN ResourceDescription VARCHAR(100)
   END
ELSE
   BEGIN
     PRINT N'Already Updated Resource table for Desc to 100 characters in dbo'
   END
GO

IF((SELECT COL_LENGTH('version.Resource', 'ResourceDescription') AS 'VarChar') = 50)
   BEGIN
     ALTER TABLE version.Resource
         ALTER COLUMN ResourceDescription VARCHAR(100)
   END
ELSE
   BEGIN
     PRINT N'Already Updated Resource table for Desc to 100 characters in version'
   END
GO

/*
	01/26/2016 [Dusan]: Matt's changes related to the SP updates

	## END OF Matt's UPDATES ##
*/
