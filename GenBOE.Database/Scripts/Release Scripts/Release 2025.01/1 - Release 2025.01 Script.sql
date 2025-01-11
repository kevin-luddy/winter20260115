EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.01'; 
GO

-- Add missing index
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[BOETaskElementCustomFieldValueXREF]') AND name = N'IX_BOETaskElementCustomFieldValueXREF_BOETaskElementID') 
	DROP INDEX [IX_BOETaskElementCustomFieldValueXREF_BOETaskElementID] ON [dbo].[BOETaskElementCustomFieldValueXREF] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_BOETaskElementCustomFieldValueXREF_BOETaskElementID] ON [dbo].[BOETaskElementCustomFieldValueXREF]
(
	[BOETaskElementID] ASC
)
INCLUDE ([CustomFieldValueID])
GO

-- Add missing index
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[version].[WorkspaceOffloadRate]') AND name = N'IX_WorkspaceOffloadRate_WorkspaceID') 
	DROP INDEX [IX_WorkspaceOffloadRate_WorkspaceID] ON [version].[WorkspaceOffloadRate] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_WorkspaceOffloadRate_WorkspaceID] ON [version].[WorkspaceOffloadRate]
(
	[WorkspaceID] ASC, 
    [VersionID] ASC
)
INCLUDE([OffloadRateID],[UpdateDT],[Resource],[PerfOrg],[PercentToOffload],[Year],[SubcontractorResource],[HourlyRate]);
GO

-- Add missing index
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[WorkspaceEmailXREF]') AND name = N'IX_WorkspaceEmailXREF_WorkspaceID') 
	DROP INDEX [IX_WorkspaceEmailXREF_WorkspaceID] ON [dbo].[WorkspaceEmailXREF] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_WorkspaceEmailXREF_WorkspaceID] ON [dbo].[WorkspaceEmailXREF]([WorkspaceID] ASC);
GO

-- Add [IsMultiClinWbs] to Index
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[BOE]') AND name = N'IX_BOE_WorkspaceID') 
	DROP INDEX [IX_BOE_WorkspaceID] ON [dbo].[BOE] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_BOE_WorkspaceID] ON [dbo].[BOE]
(
	[WorkspaceID] ASC
)
INCLUDE([BOEID],[UpdateDT],[BOEStateID],[BOEStartDate],[BOEEndDate],[BOEDescription],[DataSource],[MetricDisclosureAcknowledge],[NumAuthorReassigned],[IsMaterial],[BOETitle], [IsMultiClinWbs]);
GO

-- Add Include columns to Index
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[WorkspaceUserRole]') AND name = N'IX_WorkspaceUserRole__WorkspaceID') 
	DROP INDEX [IX_WorkspaceUserRole__WorkspaceID] ON [dbo].[WorkspaceUserRole] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_WorkspaceUserRole__WorkspaceID] ON [dbo].[WorkspaceUserRole]
(
	[WorkspaceID] ASC
) INCLUDE ([WorkspaceUserRoleID], [UpdateDT], [ETIUserID], [RoleID], [HideHelp])
GO

-- Add missing Include columns to Index
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[BOELaborType]') AND name = N'IX_BOELaborType_BOETaskElementID') 
	DROP INDEX [IX_BOELaborType_BOETaskElementID] ON [dbo].[BOELaborType] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_BOELaborType_BOETaskElementID] ON [dbo].[BOELaborType]
(
	[BOETaskElementID] ASC
)
INCLUDE ([BOELaborTypeID],[UpdateDT], [ResourceID], [PerformingOrganizationID], [BOELaborTypeStartDate], [BOELaborTypeEndDate], [SpreadCurveID], [PercentSpread], [ValueSpread], [SpreadTypeID], [PercentSpreadLocked], [HourSpreadLocked], [WBSID], [CLINID], [CanOffload], [LaborSortId], [BRCResourceID])
GO

-- Add missing index
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[BOEFormPBOEResourcesXREF]') AND name = N'PX_BOEFormPBOEResourcesXREF_PBOEFormID') 
	DROP INDEX [PX_BOEFormPBOEResourcesXREF_PBOEFormID] ON [dbo].[BOEFormPBOEResourcesXREF] WITH ( ONLINE = OFF )
GO

CREATE CLUSTERED INDEX [PX_BOEFormPBOEResourcesXREF_PBOEFormID] ON [dbo].[BOEFormPBOEResourcesXREF] ([PBOEFormID] ASC)
GO 

-- Add missing index
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MoqTypeTableCustomFieldValueXREF]') AND name = N'IX_MoqTypeTableCustomFieldValueXREF_MoqId') 
	DROP INDEX [IX_MoqTypeTableCustomFieldValueXREF_MoqId] ON [dbo].[MoqTypeTableCustomFieldValueXREF] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_MoqTypeTableCustomFieldValueXREF_MoqId] ON [dbo].[MoqTypeTableCustomFieldValueXREF] ([MoqTypeTableDataId] ASC) INCLUDE ([UpdateDT], [CustomFieldValueId])
GO 

-- Add missing index
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MoqTypeTableCustomFieldValueXREF]') AND name = N'IX_MoqTypeTableCustomFieldValueXREF_CustomFieldId') 
	DROP INDEX [IX_MoqTypeTableCustomFieldValueXREF_CustomFieldId] ON [dbo].[MoqTypeTableCustomFieldValueXREF] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_MoqTypeTableCustomFieldValueXREF_CustomFieldId] ON [dbo].[MoqTypeTableCustomFieldValueXREF] ([CustomFieldValueId] ASC) 
GO 

-- Add missing index
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ETIuser]') AND name = N'IX_ETIuser_NTID') 
	DROP INDEX [IX_ETIuser_NTID] ON [dbo].[ETIuser] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_ETIuser_NTID] ON [dbo].[ETIuser] ([NTID] ASC)  INCLUDE ([DisplayName])
GO 

-- Add missing index
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[BOEStateHistory]') AND name = N'IX_BOEStateHistory_BOEID') 
	DROP INDEX [IX_BOEStateHistory_BOEID] ON [dbo].[BOEStateHistory] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_BOEStateHistory_BOEID] ON [dbo].[BOEStateHistory] ([BOEID]) INCLUDE ([UpdateDT], [FieldID], [CurrentBOEStateID], [UpdatedBOEStateID], [ChangedByETIUserID])
GO

-- Add missing index
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[WorkspaceContractTypeXREF]') AND name = N'IX_WorkspaceContractTypeXREF_WorkspaceID') 
	DROP INDEX [IX_WorkspaceContractTypeXREF_WorkspaceID] ON [dbo].[WorkspaceContractTypeXREF] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_WorkspaceContractTypeXREF_WorkspaceID] ON [dbo].[WorkspaceContractTypeXREF] ([WorkspaceID]) INCLUDE ([UpdateDT], [ContractTypeID])
GO

-- Add missing index
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[TMResourceRate]') AND name = N'IX_TMResourceRate_WorkspaceID') 
	DROP INDEX [IX_TMResourceRate_WorkspaceID] ON [dbo].[TMResourceRate] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_TMResourceRate_WorkspaceID] ON [dbo].[TMResourceRate] ([WorkspaceID]) INCLUDE ([UpdateDT], [TMResourceID], [TMResourceRateStartDate], [TMResourceRateEndDate], [TMResourceRate])
GO

-- Add missing index
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[TMResourceRate]') AND name = N'IX_TMResourceRate_WorkspaceID_TMResourceID') 
	DROP INDEX [IX_TMResourceRate_WorkspaceID_TMResourceID] ON [dbo].[TMResourceRate] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_TMResourceRate_WorkspaceID_TMResourceID] ON [dbo].[TMResourceRate] ([WorkspaceID], [TMResourceID])
GO

-- Add missing index
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ELMAH_Error]') AND name = N'IX_ELMAH_App') 
	DROP INDEX [IX_ELMAH_App] ON [dbo].[ELMAH_Error] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_ELMAH_App] ON [dbo].[ELMAH_Error] ([Application],[TimeUtc], [Sequence]) INCLUDE ([ErrorId], [Host], [Type], [Source], [Message], [User], [StatusCode])
GO

-- Add missing index
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ELMAH_Error]') AND name = N'IX_ELMAH_Time') 
	DROP INDEX [IX_ELMAH_Time] ON [dbo].[ELMAH_Error] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_ELMAH_Time] ON [dbo].[ELMAH_Error] ([TimeUtc])
GO