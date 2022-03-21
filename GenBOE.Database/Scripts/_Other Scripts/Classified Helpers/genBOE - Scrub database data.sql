/*
	### DO NOT EXECUTE AS A PART OF ANY RELEASE ###
	
	This scrubs out data from genBOE database (2022.7).	
*/

/* <==== Remove this line, to make this script run. this is a precaution.. just in case.....


DELETE FROM ELMAH_Error;

DELETE FROM BOECustomFieldValueXREF;
DELETE FROM BOEFormIBOECLINsXREF;
DELETE FROM BOEFormIBOEResourcesXREF;
DELETE FROM BOEFormPBOECLINsXREF;
DELETE FROM BOEFormPBOEResourcesXREF;
DELETE FROM BOELaborTypeCustomFieldValueXREF;
DELETE FROM BOETaskElementCustomFieldValueXREF;
DELETE FROM BOETaskElementMetricDetailXREF;
DELETE FROM BOETaskElementWorkspaceVariableXREF;
DELETE FROM MoqTypeTableCustomFieldValueXREF;
DELETE FROM MSTTravelTripCustomFieldValueXREF;
DELETE FROM OrdinaryVariableSumVariableResourceTypeXREF;
DELETE FROM OutputFormatTemplateWorkspaceXREF;
DELETE FROM ProPricerCustomFieldXREF;
DELETE FROM ProPricerFieldXREF;
DELETE FROM RealignedWorkspaceXREF;
DELETE FROM SumOfBOE_OrdinaryVariableXREF;
DELETE FROM SumOfBOE_WorkspaceVariableXREF;
DELETE FROM SystemProPricerCustomFieldXREF;
DELETE FROM TravelTripCustomFieldValueXREF;
DELETE FROM TravelTripTaskElementCustomFieldValueXREF;
DELETE FROM WBS_CLIN_BOE_XREF;
DELETE FROM WorkspaceContractTypeXREF;
DELETE FROM WorkspaceEmailXREF;
DELETE FROM WorkspaceUserXREF;
DELETE FROM WorkspaceVariableSumVariableResourceTypeXREF;

DELETE FROM BOECommentHistory;
DELETE FROM BOEComment;
DELETE FROM BOEApprovalHistory;
DELETE FROM BOEApproval;
DELETE FROM BOECopyMetric;
DELETE FROM BOECopySource;
DELETE FROM BOECopyTarget;
DELETE FROM BOEFormIBOE;
DELETE FROM BOEFormPBOE;

DELETE FROM BOELaborSpread;
DELETE FROM BOELaborType;
DELETE FROM BOEPotentialRole;
DELETE FROM BOEStateHistory;
DELETE FROM BOEUserRoleHistory;
DELETE FROM BOEUserRole;
DELETE FROM OrdinaryVariable;

DELETE FROM CLIN;
DELETE FROM CustomFieldValue;
DELETE FROM CustomField;
DELETE FROM MaterialTaskElement;
DELETE FROM MetricDetail;
DELETE FROM MOQTypeSelectionTableData;
DELETE FROM MOQTypeSelection;
DELETE FROM ODCSpread;
DELETE FROM ODCType;
DELETE FROM ODCTaskElement;
DELETE FROM WorkBreakdownStructure;
DELETE FROM ReportXmlData;
DELETE FROM RteTemplateAssigned;
DELETE FROM RteTemplateAnswer;
DELETE FROM RteTemplateQuestion;
DELETE FROM RteTemplate;
DELETE FROM ProPricerExport;
DELETE FROM TMResourceRate;
DELETE FROM UserAccessReport;

DELETE FROM WorkspaceCopyMetric;
DELETE FROM WorkspaceCopySource;
DELETE FROM WorkspaceCopyTarget;
DELETE FROM WorkspaceLockedPerDiem;
DELETE FROM WorkspaceLockedTravelEscalationRate;
DELETE FROM WorkspaceLockedTravelMiscRate;
DELETE FROM WorkspaceLockedTrip;
DELETE FROM WorkspaceOffloadRate;
DELETE FROM WorkspacePerformingOrganization;
DELETE FROM WorkspaceResource;
DELETE FROM WorkspaceRestoreLog;
DELETE FROM WorkspaceRMSTravelEscalationRate;
DELETE FROM WorkspaceRMSTravelNonzoneFeesAndCosts;
DELETE FROM WorkspaceStateHistory;
DELETE FROM WorkspaceUserRole;
DELETE FROM WorkspaceVariable;
DELETE FROM WorkspaceVersion;
DELETE FROM WS_Copy_Stage;
DELETE FROM MSTTravelTrip;
DELETE FROM TravelTripTaskElement;

WHILE EXISTS (SELECT 1 FROM BOETaskElement)
BEGIN
	DELETE FROM BOETaskElement WHERE BOETaskElementID IN (SELECT TOP 10000 BOETaskElementID FROM BOETaskElement);
END

WHILE EXISTS (SELECT 1 FROM BOE)
BEGIN
	DELETE FROM BOE WHERE BOEId IN (SELECT TOP 10000 BOEId FROM BOE);
END

WHILE EXISTS (SELECT 1 FROM Workspace)
BEGIN
	DELETE FROM Workspace WHERE WorkspaceId IN (SELECT TOP 500 WorkspaceId FROM Workspace);
END

DELETE FROM SystemUserRole;
DELETE FROM ETIGroup;
DELETE FROM ETIuser;

DELETE FROM [version].[WorkspaceLockedTravelEscalationRate];
DELETE FROM [version].[BOEFormIBOE];
DELETE FROM [version].[WorkspaceVariableSumVariableResourceTypeXREF];
DELETE FROM [version].[WorkspaceUserRole];
DELETE FROM [version].[ProPricerFieldXREF];
DELETE FROM [version].[WorkspaceLockedTrip];
DELETE FROM [version].[BOELaborType];
DELETE FROM [version].[SumOfBOE_WorkspaceVariableXREF];
DELETE FROM [version].[TravelTripTaskElementCustomFieldValueXREF];
DELETE FROM [version].[TravelTripTaskElement];
DELETE FROM [version].[TMResourceRate];
DELETE FROM [version].[ProjectMapSpread];
DELETE FROM [version].[WorkspaceVariable];
DELETE FROM [version].[OutputFormatTemplate];
DELETE FROM [version].[ProjectMap];
DELETE FROM [version].[WorkspaceLockedPerDiem];
DELETE FROM [version].[WorkspaceOffloadRate];
DELETE FROM [version].[MetricDetail];
DELETE FROM [version].[BOETaskElementWorkspaceVariableXREF];
DELETE FROM [version].[BOETaskElementMetricDetailXREF];
DELETE FROM [version].[CustomField];
DELETE FROM [version].[OrdinaryVariable];
DELETE FROM [version].[BOEStateHistory];
DELETE FROM [version].[RteTemplateAnswer];
DELETE FROM [version].[WorkspaceResource];
DELETE FROM [version].[RteTemplateAssigned];
DELETE FROM [version].[RteTemplateQuestion];
DELETE FROM [version].[WorkspaceLockedTravelMiscRate];
DELETE FROM [version].[ODCType];
DELETE FROM [version].[RteTemplate];
DELETE FROM [version].[BOETaskElementgenDataMetricXREF];
DELETE FROM [version].[MoqTypeTableCustomFieldValueXREF];
DELETE FROM [version].[TravelTrip];
DELETE FROM [version].[OrdinaryVariableSumVariableResourceTypeXREF];
DELETE FROM [version].[ProPricerCustomFieldXREF];
DELETE FROM [version].[BOETaskElement];
DELETE FROM [version].[BOEUserRole];
DELETE FROM [version].[Workspace];
DELETE FROM [version].[BOECommentHistory];
DELETE FROM [version].[OutputFormatTemplateWorkspaceXREF];
DELETE FROM [version].[MOQTypeSelectionTableData];
DELETE FROM [version].[BOEPotentialRole];
DELETE FROM [version].[BOELaborTypeCustomFieldValueXREF];
DELETE FROM [version].[WorkBreakdownStructure];
DELETE FROM [version].[WorkspaceRMSTravelEscalationRate];
DELETE FROM [version].[MaterialTaskElement];
DELETE FROM [version].[MOQTypeSelection];
DELETE FROM [version].[PerformingOrganization];
DELETE FROM [version].[BOEApprovalHistory];
DELETE FROM [version].[WBS_CLIN_BOE_XREF];
DELETE FROM [version].[BOEComment];
DELETE FROM [version].[WorkspaceRMSTravelNonzoneFeesAndCosts];
DELETE FROM [version].[SumOfBOE_OrdinaryVariableXREF];
DELETE FROM [version].[WorkspaceContractTypeXREF];
DELETE FROM [version].[Resource];
DELETE FROM [version].[WorkspacePerformingOrganization];
DELETE FROM [version].[PerDiem];
DELETE FROM [version].[TravelMiscRate];
DELETE FROM [version].[MSTTravelTrip];
DELETE FROM [version].[BOEUserRoleHistory];
DELETE FROM [version].[MSTTravelTripCustomFieldValueXREF];
DELETE FROM [version].[PerformingOrganizationList];
DELETE FROM [version].[ProPricerExport];
DELETE FROM [version].[ODCSpread];
DELETE FROM [version].[Trip];
DELETE FROM [version].[ResourceList];
DELETE FROM [version].[tempPerformingOrganizationMapping];
DELETE FROM [version].[TravelTripCustomFieldValueXREF];
DELETE FROM [version].[OutputFormatTemplateVersionXREF];
DELETE FROM [version].[BOEApproval];
DELETE FROM [version].[ODCTaskElement];
DELETE FROM [version].[BOETaskElementCustomFieldValueXREF];
DELETE FROM [version].[BOEFormIBOECLINsXREF];
DELETE FROM [version].[CustomFieldValue];
DELETE FROM [version].[BOEFormPBOECLINsXREF];
DELETE FROM [version].[BOE];
DELETE FROM [version].[BOECustomFieldValueXREF];
DELETE FROM [version].[BOEFormPBOEResourcesXREF];
DELETE FROM [version].[WorkspaceStateHistory];
DELETE FROM [version].[BOEFormIBOEResourcesXREF];
DELETE FROM [version].[CLIN];
DELETE FROM [version].[BOEFormPBOE];
DELETE FROM [version].[BOELaborSpread];
