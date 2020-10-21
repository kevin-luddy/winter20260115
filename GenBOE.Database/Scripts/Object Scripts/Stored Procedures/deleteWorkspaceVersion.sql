IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkspaceVersion]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkspaceVersion];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE  PROCEDURE [dbo].[deleteWorkspaceVersion]
(
@WorkspaceID int,
@VersionID int
)
/******************************************************************************
**		 
**		Name:	[deleteWorkspaceVersion]
**		Desc:	Deletes all of the data inserted from createWorkspaceVersion
**				including the deletion of the record in the Workspace Version Table
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
**      1/17/17		twilson3			BOEJ-1604 Cleanup DB Project
**		3/3/2017	twilson3			BOEJ-1861 Remove DTS
**		5/4/2017	brunworg			BOEJ-2125 Add T&M Resource Rates
**		9/25/2017	twilson3			BOEJ-2535 Add ProjectMap Tables
**		12/7/2017	twilson3			BOEJ-2250 Remove DTC
**		12/15/17	twilson3			BOEJ-2248 Remove Labor Rates
**		1/11/18		ranzalon			BOEJ-2889 Updated template backup
**		1/16/18		twilson3			BOEJ-2887 Remove Historical Metrics
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
**		12/13/19	twilson3			BOEJ-4434 - RTE Template Answers
**		12/17/19	twilson3			BOEJ-4434 Fix Assigned
**		9/15/20		ranzalon			BOEJ-4776/4825 - MOQ Types update
*******************************************************************************/
AS
SET NOCOUNT ON

IF EXISTS (SELECT 1 FROM dbo.WorkspaceVersion WHERE VersionID = @VersionID AND WorkspaceID = @WorkspaceID)
BEGIN
	BEGIN TRANSACTION
	/*DELETE THE DATA FROM TABLES*/
		
IF EXISTS (SELECT VersionID FROM  [version].[BOE] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[BOE] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[BOEApproval] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[BOEApproval] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[BOEApprovalHistory] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[BOEApprovalHistory] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[BOEComment] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[BOEComment] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[BOECommentHistory] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[BOECommentHistory] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[BOECustomFieldValueXREF] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[BOECustomFieldValueXREF] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[BOEFormIBOE] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[BOEFormIBOE] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[BOEFormIBOEResourcesXREF] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[BOEFormIBOEResourcesXREF] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[BOEFormIBOECLINsXREF] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[BOEFormIBOECLINsXREF] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[BOEFormPBOE] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[BOEFormPBOE] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[BOEFormPBOEResourcesXREF] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[BOEFormPBOEResourcesXREF] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[BOEFormPBOECLINsXREF] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[BOEFormPBOECLINsXREF] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[BOELaborSpread] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[BOELaborSpread] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[BOELaborType] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[BOELaborType] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[BOELaborTypeCustomFieldValueXREF] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[BOELaborTypeCustomFieldValueXREF] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[BOEPotentialRole] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[BOEPotentialRole] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[BOEStateHistory] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[BOEStateHistory] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[BOETaskElement] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[BOETaskElement] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[BOETaskElementCustomFieldValueXREF] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[BOETaskElementCustomFieldValueXREF] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[BOETaskElementWorkspaceVariableXREF] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[BOETaskElementWorkspaceVariableXREF] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[BOEUserRole] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[BOEUserRole] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[BOEUserRoleHistory] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[BOEUserRoleHistory] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[CLIN] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[CLIN] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[ProjectMap] WHERE VersionID = @VersionID)
				DELETE FROM [version].[ProjectMap] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[ProjectMapSpread] WHERE VersionID = @VersionID)
				DELETE FROM [version].[ProjectMapSpread] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[CustomField] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[CustomField] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[CustomFieldValue] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[CustomFieldValue] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[MaterialTaskElement] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[MaterialTaskElement] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[MOQTypeSelection] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[MOQTypeSelection] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[MOQTypeSelectionTableData] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[MOQTypeSelectionTableData] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[ODCSpread] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[ODCSpread] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[ODCTaskElement] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[ODCTaskElement] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[ODCType] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[ODCType] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[OrdinaryVariable] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[OrdinaryVariable] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[OrdinaryVariableSumVariableResourceTypeXREF] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[OrdinaryVariableSumVariableResourceTypeXREF] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[OutputFormatTemplateVersionXREF] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[OutputFormatTemplateVersionXREF] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[OutputFormatTemplateWorkspaceXREF] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[OutputFormatTemplateWorkspaceXREF] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[PerDiem] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[PerDiem] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[PerformingOrganization] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[PerformingOrganization] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[PerformingOrganizationList] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[PerformingOrganizationList] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[ProPricerCustomFieldXREF] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[ProPricerCustomFieldXREF] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[ProPricerExport] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[ProPricerExport] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[ProPricerFieldXREF] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[ProPricerFieldXREF] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[Resource] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[Resource] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[ResourceList] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[ResourceList] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[RteTemplateAnswer] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[RteTemplateAnswer] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[RteTemplateAssigned] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[RteTemplateAssigned] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[RteTemplate] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[RteTemplate] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[RteTemplateQuestion] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[RteTemplateQuestion] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[SumOfBOE_OrdinaryVariableXREF] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[SumOfBOE_OrdinaryVariableXREF] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[SumOfBOE_WorkspaceVariableXREF] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[SumOfBOE_WorkspaceVariableXREF] WHERE VersionID = @VersionID

IF EXISTS (SELECT VersionID FROM  [version].[TravelMiscRate] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[TravelMiscRate] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[TravelTrip] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[TravelTrip] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[TravelTripCustomFieldValueXREF] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[TravelTripCustomFieldValueXREF] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[TravelTripTaskElement] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[TravelTripTaskElement] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[TravelTripTaskElementCustomFieldValueXREF] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[TravelTripTaskElementCustomFieldValueXREF] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[Trip] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[Trip] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[WBS_CLIN_BOE_XREF] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[WBS_CLIN_BOE_XREF] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[WorkBreakdownStructure] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[WorkBreakdownStructure] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[Workspace] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[Workspace] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[WorkspaceContractTypeXREF] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[WorkspaceContractTypeXREF] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[WorkspaceLockedPerDiem] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[WorkspaceLockedPerDiem] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[WorkspaceLockedTravelEscalationRate] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[WorkspaceLockedTravelEscalationRate] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[WorkspaceLockedTravelMiscRate] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[WorkspaceLockedTravelMiscRate] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[WorkspaceLockedTrip] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[WorkspaceLockedTrip] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[WorkspacePerformingOrganization] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[WorkspacePerformingOrganization] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[WorkspaceResource] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[WorkspaceResource] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[TMResourceRate] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[TMResourceRate] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[WorkspaceStateHistory] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[WorkspaceStateHistory] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[WorkspaceUserRole] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[WorkspaceUserRole] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[WorkspaceVariable] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[WorkspaceVariable] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[WorkspaceVariableSumVariableResourceTypeXREF] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[WorkspaceVariableSumVariableResourceTypeXREF] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[WorkspaceRMSTravelEscalationRate] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[WorkspaceRMSTravelEscalationRate] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[WorkspaceRMSTravelNonzoneFeesAndCosts] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[WorkspaceRMSTravelNonzoneFeesAndCosts] WHERE VersionID = @VersionID

-- Start of "RMS Zone Travel"
IF EXISTS (SELECT VersionID FROM  [version].[MSTTravelTrip] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[MSTTravelTrip] WHERE VersionID = @VersionID
IF EXISTS (SELECT VersionID FROM  [version].[MSTTravelTripCustomFieldValueXREF] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[MSTTravelTripCustomFieldValueXREF] WHERE VersionID = @VersionID
-- End of "RMS Zone Travel"

/*
IF EXISTS (SELECT VersionID FROM  [version].[WorkspaceImage] WHERE VersionID = @VersionID)		
				DELETE FROM [version].[WorkspaceImage] WHERE VersionID = @VersionID
*/

		DELETE FROM [dbo].[WorkspaceVersion] WHERE VersionID = @VersionID

END
ELSE
	BEGIN
			DECLARE @ErrorMessage varchar (500)
			SET @ErrorMessage =   'The Workspace Version with ID ' + CAST(@WorkspaceID  AS varchar(10)) +  ' and Version ID ' + CAST(@VersionID  AS varchar(10)) + ' does not exist'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
	END


IF @@ERROR = 0
	BEGIN
		COMMIT TRANSACTION
	END
ELSE
	BEGIN
		ROLLBACK TRANSACTION
	END

GO