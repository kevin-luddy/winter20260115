IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[restoreWorkspaceVersion]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[restoreWorkspaceVersion];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE  PROCEDURE [dbo].[restoreWorkspaceVersion]
(
@VersionID int,
@ETIUserID int,
@WorkspaceID int
)
AS
/******************************************************************************
**		 
**		Name: [restoreWorkspaceVersion]
**		Desc:	Creates a System Back Up
**				Deletes the data in the active ([dbo]) Workspace
**				Restores the data in the Back Up ([version]) Workspace
**				Update States and create logs
**
**				Metrics:
**				If a Metric was deleted, it will not be restored.
**				The row in the BOE Task Element  Metric  XREF table will be deleted.
**				
**				Resources and Performing Organizations:
**				If a Resource or a P.O. is deleted, the row in the BOE Labor Type table will now contain NULL 
**				values for P.O. and Resource rather than the ID.
**				A user will need to fix this before submitting for approval.
**				
**				Custom Fields/Output Format Template/ Workspace Variables:
**				Restored exactly how they were backed up.
**				
**				When the restore completes, a string will be sent to the Back End describing what did not work, 
**				for example, Resources XYZ was unable to be restored.
**				
**				Also, when a user starts a restore, the stored procedure will kick off a Back Up with SYSTEM: at the 
**				start of the version
**				
**		Auth: Don Canuso
**		Date: 6/7/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		1/11/18		ranzalon			BOEJ-2889 Updated template backup
**		1/16/18		twilson3			BOEJ-2887 Remove Historical Metrics
**		1/25/18 	twilson3 			BOEJ-2972 Merge Backup Sprocs
**		4/04/2018	brunworg			BOEJ-3177 Added TieredPercentage column.
**		4/13/18		pattoncr			BOEJ-3185 - DB Work (Sikorsky Legacy Resources)
**		5/2/18		ranzalon			BOEJ-3416 - Templates available to all Workspaces
**		6/18/18		ranzalon			BOEJ-3448 - ProPricer API updates
**		10/2/18		ranzalon			BOEJ-3699 - RTE Size Limit
**		06/11/19	ranzalon			BOEJ-4108 - Rename Backup
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
**		7/12/19		twilson3			BOEJ-4037	System Pro Pricer Export 
**		9/26/19		ranzalon			BOEJ-4349 - Revised Submittal Date
**		12/2/19		ranzalon			BOEJ-4464 - Added LaborSortId
**		12/5/19		twilson3			BOEJ-4429 - RTE Templates
**		12/13/19	twilson3			BOEJ-4434 - RTE Template Answers
**		12/17/19	twilson3			BOEJ-4434 - Fix Assigned
**		02/13/20	ranzalon			BOEJ-4506 - Fix RTE Assigned, RTE template deletion order
**		8/27/20		ranzalon			BOEJ-4760 - Template Boe
**		9/15/20		ranzalon			BOEJ-4776/4825 - MOQ Types update
*******************************************************************************/
SET NOCOUNT ON 

--SELECT @VersionID =2,@ETIUserID =1,@WorkspaceID =1

IF EXISTS (SELECT 1 FROM dbo.WorkspaceVersion WHERE VersionID = @VersionID AND WorkspaceID = @WorkspaceID)
BEGIN
BEGIN TRY
--BEGIN TRANSACTION 

/*Create a Back Up of the current Workspace*/
DECLARE @VersionName varchar(50),
		@WorkspaceStateID int
		
SELECT	@VersionName = '[SYSTEM: VERSION RESTORE] ' + CONVERT(varchar, GETDATE(), 22),
		@WorkspaceStateID = WorkspaceStateID 
FROM dbo.Workspace 
WHERE WorkspaceID = @WorkspaceID


EXECUTE  [dbo].[createWorkspaceVersion] @VersionName,0,@WorkspaceStateID,@WorkspaceID


/*DELETE THE CURRENT DATA SO THAT BU DATA CAN BE RESTORED*/
DELETE FROM [dbo].[BOETaskElementMetricDetailXREF]
	FROM [dbo].[BOETaskElementMetricDetailXREF] X
		INNER JOIN [dbo].[BOETaskElement] BTE ON X.BOETaskElementID = BTE.BOETaskElementID
		INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM dbo.WBS_CLIN_BOE_XREF 
	FROM [dbo].[WBS_CLIN_BOE_XREF] X
		INNER JOIN dbo.WorkBreakdownStructure WBS 
			ON X.WBSID = WBS.WBSID AND WBS.WorkspaceID = @WorkspaceID
DELETE FROM dbo.WBS_CLIN_BOE_XREF 
	FROM [dbo].[WBS_CLIN_BOE_XREF] X
		INNER JOIN dbo.CLIN C 
			ON X.CLINID = C.CLINID AND C.WorkspaceID = @WorkspaceID
DELETE FROM dbo.WBS_CLIN_BOE_XREF 
	FROM [dbo].[WBS_CLIN_BOE_XREF] X
		INNER JOIN  dbo.BOE B 
			ON X.BOEID = B.BOEID AND B.WorkspaceID = @WorkspaceID
DELETE FROM dbo.BOEStateHistory  
	FROM [dbo].[BOEStateHistory] BH
		INNER JOIN dbo.BOE B ON BH.BOEID  = B.BOEID
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM dbo.SumOfBOE_WorkspaceVariableXREF 
	FROM [dbo].[SumOfBOE_WorkspaceVariableXREF] X
		INNER JOIN [dbo].[WorkspaceVariable] WSV ON X.WorkspaceVariableID = WSV.WorkspaceVariableID
		INNER JOIN dbo.Workspace WS ON WSV.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM dbo.BOETaskElementWorkspaceVariableXREF 
	FROM [dbo].[BOETaskElementWorkspaceVariableXREF] X
		INNER JOIN [dbo].[BOETaskElement] BTE ON X.BOETaskElementID = BTE.BOETaskElementID
		INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM dbo.SumOfBOE_OrdinaryVariableXREF 
	FROM [dbo].[SumOfBOE_OrdinaryVariableXREF] X
		INNER JOIN [dbo].[OrdinaryVariable] OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
		INNER JOIN [dbo].[BOETaskElement] BTE ON OV.BOETaskElementID = BTE.BOETaskElementID
		INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM dbo.WorkspaceVariableSumVariableResourceTypeXREF
	FROM dbo.WorkspaceVariableSumVariableResourceTypeXREF X
		INNER JOIN [dbo].[WorkspaceVariable] WSV ON X.WorkspaceVariableID = WSV.WorkspaceVariableID
		INNER JOIN dbo.Workspace WS ON WSV.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID	
DELETE FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF
	FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF X 
		INNER JOIN [dbo].[OrdinaryVariable] OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
		INNER JOIN [dbo].[BOETaskElement] BTE ON OV.BOETaskElementID = BTE.BOETaskElementID
		INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM dbo.BOEUserRole 
	FROM [dbo].[BOEUserRole] BUR
		INNER JOIN dbo.BOE B ON BUR.BOEID  = B.BOEID
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
/*WI 5160*/
DELETE FROM dbo.BOEUserRoleHistory
	FROM [dbo].[BOEUserRoleHistory] BUR
		INNER JOIN dbo.BOE B ON BUR.BOEID  = B.BOEID
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM dbo.BOEApprovalHistory 
	FROM [dbo].[BOEApprovalHistory] BH
		INNER JOIN dbo.BOE B ON BH.BOEID  = B.BOEID
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM dbo.BOEApproval 
	FROM [dbo].[BOEApproval] BA
		INNER JOIN dbo.BOE B ON BA.BOEID  = B.BOEID
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM dbo.BOECommentHistory 
	FROM [dbo].[BOECommentHistory] BH
		INNER JOIN dbo.BOE B ON BH.BOEID  = B.BOEID
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM dbo.BOEComment 
	FROM [dbo].[BOEComment] BC
		INNER JOIN dbo.BOE B ON BC.BOEID  = B.BOEID
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM [dbo].[BOELaborSpread] 
	FROM [dbo].[BOELaborSpread] LS
		INNER JOIN [dbo].[BOELaborType] BLT ON LS.BOELaborTypeID = BLT.BOELaborTypeID
		INNER JOIN [dbo].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
		INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM dbo.BOELaborTypeCustomFieldValueXREF 
	FROM [dbo].[BOELaborTypeCustomFieldValueXREF] X 
		INNER JOIN [dbo].[BOELaborType] BLT ON X.BOELaborTypeID = BLT.BOELaborTypeID
		INNER JOIN [dbo].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
		INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM dbo.BOELaborType 
	FROM [dbo].[BOELaborType] BLT
		INNER JOIN [dbo].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
		INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM  [dbo].[OrdinaryVariable] 
	FROM [dbo].[OrdinaryVariable] OV
		INNER JOIN [dbo].[BOETaskElement] BTE ON OV.BOETaskElementID = BTE.BOETaskElementID
		INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM  [dbo].[BOEPotentialRole]
	FROM [dbo].[BOEPotentialRole] BR
		INNER JOIN dbo.Workspace WS ON BR.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM  [dbo].[WorkspaceUserRole]
	FROM [dbo].[WorkspaceUserRole] WUR
		INNER JOIN dbo.Workspace WS ON WUR.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM  [dbo].[WorkspaceVariable]
	FROM [dbo].[WorkspaceVariable] WSV
		INNER JOIN dbo.Workspace WS ON WSV.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM [dbo].[BOETaskElementCustomFieldValueXREF] 
	FROM [dbo].[BOETaskElementCustomFieldValueXREF] X
		INNER JOIN [dbo].[BOETaskElement] BTE ON X.BOETaskElementID = BTE.BOETaskElementID
		INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID

DELETE FROM [dbo].[RteTemplateAnswer]
	FROM [dbo].[RteTemplateAnswer] RTA 
		INNER JOIN [RteTemplateQuestion] RTQ ON RTQ.[QuestionID] = RTA.[QuestionID]
		INNER JOIN dbo.[RteTemplate] RT ON RTQ.TemplateID = RT.TemplateID
	WHERE RT.WorkspaceID = @WorkspaceID

DELETE FROM [dbo].[RteTemplateQuestion]
	FROM [dbo].[RteTemplateQuestion] RTQ
		INNER JOIN dbo.[RteTemplate] RT ON RTQ.TemplateID = RT.TemplateID
	WHERE RT.WorkspaceID = @WorkspaceID

DELETE FROM [dbo].[RteTemplateAssigned]
	FROM [dbo].[RteTemplateAssigned] RTA
		INNER JOIN dbo.[RteTemplate] RT ON RTA.TemplateID = RT.TemplateID
	WHERE RT.WorkspaceID = @WorkspaceID

DELETE FROM [dbo].[RteTemplate]
	WHERE WorkspaceID = @WorkspaceID

DELETE FROM [dbo].[MOQTypeSelection]
	FROM [dbo].[MOQTypeSelection] M
	INNER JOIN [dbo].[BOETaskElement] T on M.TaskId = T.BOETaskElementID
	INNER JOIN dbo.BOE B ON T.BOEID  = B.BOEID
	INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM [dbo].[MOQTypeSelectionTableData]
	FROM [dbo].[MOQTypeSelectionTableData] TD
	INNER JOIN [dbo].[MOQTypeSelection] M ON TD.MOQTypeSelectionId = M.MOQTypeSelectionId
	INNER JOIN [dbo].[BOETaskElement] T on M.TaskId = T.BOETaskElementID
	INNER JOIN dbo.BOE B ON T.BOEID  = B.BOEID
	INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID

DELETE FROM  [dbo].[BOETaskElement]
	FROM [dbo].[BOETaskElement] BTE
		INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM  [dbo].WorkspaceStateHistory	
	FROM [dbo].[WorkspaceStateHistory] WSH
		INNER JOIN dbo.Workspace WS ON WSH.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM  [dbo].BOECommentHistory 
	FROM [dbo].[BOECommentHistory] BCH
		INNER JOIN dbo.BOE B ON BCH.BOEID  = B.BOEID	
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM [dbo].[BOECustomFieldValueXREF] 
	FROM [dbo].[BOECustomFieldValueXREF] X
		INNER JOIN dbo.BOE B ON X.BOEID  = B.BOEID
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM [dbo].[TravelTripTaskElementCustomFieldValueXREF] 	
FROM [dbo].[TravelTripTaskElementCustomFieldValueXREF] X
	INNER JOIN dbo.TravelTripTaskElement TE ON X.TravelTripTaskElementID = TE.TravelTripTaskElementID
	INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
	INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM [dbo].[TravelTripCustomFieldValueXREF] 
  FROM [dbo].[TravelTripCustomFieldValueXREF] X
	INNER JOIN dbo.TravelTrip M ON X.TravelTripID = M.TravelTripID
	INNER JOIN dbo.TravelTripTaskElement TE ON M.TravelTripTaskElementID = TE.TravelTripTaskElementID
	INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
	INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM dbo.ProPricerCustomFieldXREF 
	FROM [dbo].[ProPricerCustomFieldXREF] PX
		INNER JOIN [dbo].[ProPricerExport] PPE ON PX.ProPricerExportID = PPE.ProPricerExportID
		INNER JOIN dbo.Workspace WS ON PPE.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM dbo.ProPricerFieldXREF 
	FROM [dbo].[ProPricerFieldXREF] PX
		INNER JOIN [dbo].[ProPricerExport] PPE ON PX.ProPricerExportID = PPE.ProPricerExportID
		INNER JOIN dbo.Workspace WS ON PPE.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM dbo.ProPricerExport 
	FROM [dbo].[ProPricerExport] PPE
		INNER JOIN dbo.Workspace WS ON PPE.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID

-- Start of "RMS Zone Travel"
DELETE FROM dbo.MSTTravelTripCustomFieldValueXREF
	FROM dbo.MSTTravelTripCustomFieldValueXREF cI
	INNER JOIN dbo.MSTTravelTrip tt ON cI.MSTTravelTripID = tt.MSTTravelTripID
	INNER JOIN dbo.TravelTripTaskElement TE ON tt.TravelTripTaskElementID = TE.TravelTripTaskElementID
	INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
	INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID
	
DELETE FROM dbo.MSTTravelTrip 
  FROM dbo.MSTTravelTrip tt
	INNER JOIN dbo.TravelTripTaskElement TE ON tt.TravelTripTaskElementID = TE.TravelTripTaskElementID
	INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
	INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID
-- End of "RMS Zone Travel"

DELETE FROM [dbo].[CustomFieldValue] 
	FROM [dbo].[CustomFieldValue] CFV
		INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
		INNER JOIN dbo.Workspace WS ON CF.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM [dbo].[CustomField] 
	FROM [dbo].[CustomField] CF
		INNER JOIN dbo.Workspace WS ON CF.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID	

DELETE FROM [dbo].[WorkspaceOffloadRate]
	WHERE WorkspaceID = @WorkspaceID

DELETE FROM [dbo].[ProjectMapSpread]	
	WHERE WorkspaceID = @WorkspaceID

DELETE FROM [dbo].[ProjectMap]	
	WHERE WorkspaceID = @WorkspaceID

DELETE FROM  [dbo].[WorkBreakdownStructure]
	FROM [dbo].[WorkBreakdownStructure] WBS
		INNER JOIN dbo.Workspace WS ON WBS.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID

DELETE FROM dbo.TMResourceRate
	FROM dbo.TMResourceRate TMRR
		INNER JOIN dbo.Workspace WS ON TMRR.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID	

DELETE FROM  [dbo].[CLIN]
	FROM [dbo].[CLIN] C
		INNER JOIN dbo.Workspace WS ON C.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID	
DELETE FROM [dbo].[ODCSpread] 
	FROM [dbo].[ODCSpread] S
		INNER JOIN [dbo].[ODCType] T ON S.ODCTypeID = T.ODCTypeID
		INNER JOIN [dbo].[ODCTaskElement] TE ON T.ODCTaskElementID = TE.ODCTaskElementID
		INNER JOIN dbo.BOE B ON TE.BOEID  = B.BOEID
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM dbo.ODCType 
	FROM [dbo].[ODCType] T
		INNER JOIN [dbo].[ODCTaskElement] TE ON T.ODCTaskElementID = TE.ODCTaskElementID
		INNER JOIN dbo.BOE B ON TE.BOEID  = B.BOEID
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM  [dbo].[ODCTaskElement] 
	FROM [dbo].[ODCTaskElement] TE
		INNER JOIN dbo.BOE B ON TE.BOEID  = B.BOEID
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE  FROM [dbo].[MaterialTaskElement] 
 FROM [dbo].[MaterialTaskElement] TE
	INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
	INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM [dbo].[TravelTrip]
FROM [dbo].[TravelTrip] TT 
	INNER JOIN [dbo].[TravelTripTaskElement] TE ON TT.TravelTripTaskElementID = TE.TravelTripTaskElementID
	INNER JOIN [dbo].[BOE] B ON TE.BOEID = B.BOEID
	INNER JOIN [dbo].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM [dbo].[TravelTripTaskElement]
FROM [dbo].[TravelTripTaskElement] TE 
	INNER JOIN [dbo].[BOE] B ON TE.BOEID = B.BOEID
	INNER JOIN [dbo].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID
--wi 4881
/*
Do not delete Trip and Per Diem as System Admins. are able to update and the ones in the DB
are always the latest
WI 4881 Adding Travel Misc Rate to this list
*/

/* New INL Forms */
DELETE FROM [dbo].[BOEFormIBOEResourcesXREF]
	FROM [dbo].[BOEFormIBOEResourcesXREF] x
		INNER JOIN [dbo].[BOEFormIBOE] BF ON x.[IBOEFormID] = BF.[IBOEFormID]
	WHERE 
		BF.WorkspaceID = @WorkspaceID

DELETE FROM [dbo].[BOEFormIBOECLINsXREF]
	FROM [dbo].[BOEFormIBOECLINsXREF] x
		INNER JOIN [dbo].[BOEFormIBOE] BF ON x.[IBOEFormID] = BF.[IBOEFormID]
	WHERE 
		BF.WorkspaceID = @WorkspaceID

DELETE FROM [dbo].[BOEFormIBOE] WHERE WorkspaceID = @WorkspaceID

DELETE FROM [dbo].[BOEFormPBOEResourcesXREF]
	FROM [dbo].[BOEFormPBOEResourcesXREF] x
		INNER JOIN [dbo].[BOEFormPBOE] BF ON x.[PBOEFormID] = BF.[PBOEFormID]
	WHERE 
		BF.WorkspaceID = @WorkspaceID

DELETE FROM [dbo].[BOEFormPBOECLINsXREF]
	FROM [dbo].[BOEFormPBOECLINsXREF] x
		INNER JOIN [dbo].[BOEFormPBOE] BF ON x.[PBOEFormID] = BF.[PBOEFormID]
	WHERE 
		BF.WorkspaceID = @WorkspaceID

DELETE FROM [dbo].[BOEFormPBOE] WHERE WorkspaceID = @WorkspaceID
		

DELETE FROM dbo.BOE	
	FROM [dbo].[BOE] B
		INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID	
DELETE FROM dbo.OutputFormatTemplateWorkspaceXREF 
	FROM [dbo].[OutputFormatTemplateWorkspaceXREF] OX
		INNER JOIN dbo.Workspace WS ON OX.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
/*WI 8256 LOCKING TABLES*/		
DELETE FROM dbo.[WorkspaceLockedPerDiem] 
	FROM dbo.[WorkspaceLockedPerDiem] L
		INNER JOIN dbo.Workspace WS ON L.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM dbo.[WorkspaceLockedTravelEscalationRate]
	FROM dbo.[WorkspaceLockedTravelEscalationRate] L 			
		INNER JOIN dbo.Workspace WS ON L.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM dbo.[WorkspaceLockedTravelMiscRate] 
	FROM dbo.[WorkspaceLockedTravelMiscRate] L 			
		INNER JOIN dbo.Workspace WS ON L.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM dbo.[WorkspaceLockedTrip] 
	FROM dbo.[WorkspaceLockedTrip] L
		INNER JOIN dbo.Workspace WS ON L.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM dbo.WorkspaceResource
 FROM dbo.WorkspaceResource WR
		INNER JOIN dbo.Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM [dbo].[Resource]
	FROM [dbo].[Resource] R
		INNER JOIN [dbo].[ResourceList] RL ON R.ResourceListID = RL.ResourceListID
		INNER JOIN dbo.Workspace WS ON RL.ResourceListID = WS.ResourceListID
WHERE 
	R.ResourceListID IS NOT NULL AND
	R.ResourceListID <> 1 AND	
	WS.WorkspaceID = @WorkspaceID

DELETE FROM [dbo].[WorkspaceRMSTravelEscalationRate] WHERE WorkspaceID = @WorkspaceID
DELETE FROM [dbo].[WorkspaceRMSTravelNonzoneFeesAndCosts] WHERE WorkspaceID = @WorkspaceID


/*Restore the Workspace*/

DECLARE @ErrorMessage varchar(8000) 
IF EXISTS (SELECT vRL.ResourceListID 
					FROM [version].ResourceList vRL 
						INNER JOIN [version].[Workspace] W ON W.ResourceListID = vRL.ResourceListID
						LEFT OUTER JOIN dbo.ResourceList RL ON vRL.ResourceListID = RL.ResourceListID
					WHERE 
						vRL.VersionID = @VersionID AND
						W.VersionID = @VersionID AND
						W.WorkspaceID = @WorkspaceID AND
						RL.ResourceListID IS NULL
				) --IS NOT NULL
		BEGIN				
			SELECT DISTINCT	@ErrorMessage = 
				IsNull(@ErrorMessage,'WARNING: ') + 
				'Resource List ID: ' + 
				CAST (vRL.ResourceListID AS varchar(10)) + 
				' Resource List Name ' + 
				vRL.ResourceListName + 
				' no longer exists.' 
				+ CHAR(13)
			FROM [version].ResourceList vRL 
						INNER JOIN [version].[Workspace] W ON W.ResourceListID = vRL.ResourceListID
						LEFT OUTER JOIN dbo.ResourceList RL ON vRL.ResourceListID = RL.ResourceListID
					WHERE 
						vRL.VersionID = @VersionID AND
						W.VersionID = @VersionID AND
						W.WorkspaceID = @WorkspaceID AND
						RL.ResourceListID IS NULL		
		END

DELETE FROM dbo.WorkspacePerformingOrganization
 FROM dbo.WorkspacePerformingOrganization WR
		INNER JOIN dbo.Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
DELETE FROM [dbo].[PerformingOrganization]
	FROM [dbo].[PerformingOrganization] R
		INNER JOIN [dbo].[PerformingOrganizationList] RL ON R.PerformingOrganizationListID = RL.PerformingOrganizationListID
		INNER JOIN dbo.Workspace WS ON RL.PerformingOrganizationListID = WS.PerformingOrganizationListID
WHERE 
	R.PerformingOrganizationListID IS NOT NULL AND
	R.PerformingOrganizationListID <> 1 AND	
	WS.WorkspaceID = @WorkspaceID

IF EXISTS (SELECT vPL.PerformingOrganizationListID 
					FROM [version].PerformingOrganizationList vPL 
						INNER JOIN [version].[Workspace] W ON W.PerformingOrganizationListID = vPL.PerformingOrganizationListID
						LEFT OUTER JOIN dbo.PerformingOrganizationList PL ON vPL.PerformingOrganizationListID = PL.PerformingOrganizationListID
					WHERE 
						vPL.VersionID = @VersionID AND
						W.VersionID = @VersionID AND
						W.WorkspaceID = @WorkspaceID AND
						PL.PerformingOrganizationListID IS NULL
				) --IS NOT NULL
		BEGIN
					
			SELECT	DISTINCT	@ErrorMessage = 
				IsNull(@ErrorMessage,'WARNING: ') + 
				'Performing Organization List ID: ' + 
				CAST (vPL.PerformingOrganizationListID AS varchar(10)) + 
				' Performing Organization List Name: ' + 
				vPL.PerformingOrganizationListName + 
				' no longer exists.' + 
				CHAR(13)
					FROM [version].PerformingOrganizationList vPL 
						INNER JOIN [version].[Workspace] W ON W.PerformingOrganizationListID = vPL.PerformingOrganizationListID
						LEFT OUTER JOIN dbo.PerformingOrganizationList PL ON vPL.PerformingOrganizationListID = PL.PerformingOrganizationListID
					WHERE 
						vPL.VersionID = @VersionID AND
						W.VersionID = @VersionID AND
						W.WorkspaceID = @WorkspaceID AND
						PL.PerformingOrganizationListID IS NULL
		END

DELETE FROM dbo.[WorkspaceContractTypeXREF] 
	FROM dbo.[WorkspaceContractTypeXREF] X
		INNER JOIN dbo.Workspace WS ON X.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
			
UPDATE [dbo].[Workspace] 
	SET
	[WorkspaceName] = vW.[WorkspaceName]
	,[WorkspaceShortName] = vW.[WorkspaceShortName]
	,[WorkspaceStateID] =	/*Wireframes:
							If version state is 1 (Initialization) keep at 1
							all others set to 2 (Working)*/  
							CASE vW.[WorkspaceStateID]
								WHEN 1 THEN vW.[WorkspaceStateID]
								ELSE 2
							END
	,[ContractStartDate] = vW.[ContractStartDate]
	,[ContractEndDate] = vW.[ContractEndDate]
	,[ProposalSubmitDate] = vW.[ProposalSubmitDate]
	,[WorkspaceDescription] = vW.[WorkspaceDescription]
	,[CostVolumeLeadPricerUserID] = vW.[CostVolumeLeadPricerUserID]
	,[RFPNumber] = vW.[RFPNumber]
	,[TemplateID] = vW.[TemplateID]
	,[ContainsOCI] = vW.[ContainsOCI]
	,[CreatedByETIUserID] = vW.[CreatedByETIUserID]
	,[AllowSearch] = vW.[AllowSearch]
	,[ResourceListID]=RL.[ResourceListID]
	,[PerformingOrganizationListID]=PL.[PerformingOrganizationListID]
	,[PerformingOrganizationChangeFlag] = vW.[PerformingOrganizationChangeFlag]
	,[TrackingNumber] = vW.[TrackingNumber]
	,[ContainsTemplate] = vW.[ContainsTemplate]
	,[NumProPricerExport] = vW.[NumProPricerExport]
	,[ProposalStatusID] = vW.[ProposalStatusID]
	,[StatusComment] = vW.[StatusComment]
	,[UpdateDT] = vW.[UpdateDT]
	,[IsUsingEquivalentPerson] = vW.[IsUsingEquivalentPerson]
	,[IsUsingTM] = vW.[IsUsingTM]
	,[ProjectMapTypeID] = vW.[ProjectMapTypeID]
	,[AllowGridEdit] = vW.[AllowGridEdit]
	,[CustomSorting] = vW.[CustomSorting]
	,[ResourceSorting] = vw.[ResourceSorting]
	,[PerfOrgSorting] = vw.[PerfOrgSorting]

/*Remove Code for 7681 	,[LaborPrecisionID] = vW.LaborPrecisionID*/
	/*WI8535*/
	,[BOEExportSortByID] = vW.BOEExportSortByID		
	,[SegmentID] = vW.SegmentID
	,[LineOfBusinessID] = vW.LineOfBusinessID
	,[ProposalClassID] = vW.ProposalClassID
	,[ProposalTitle] = vw.[ProposalTitle]
	,[IsDeleted] = vw.[IsDeleted]
	,[DateDeleted] = vw.DateDeleted
	,[ResourcePrecision] = vw.[ResourcePrecision]
	,[RecalculationStartedDate] = vW.[RecalculationStartedDate]
	,[CostPrecision] = vW.[CostPrecision]
	,[LastProPricerInstance] = vW.[LastProPricerInstance]
	,[LastProPricerProposal] = vW.[LastProPricerProposal]
	,[RteSizeLimit] = vW.[RteSizeLimit]
	,[RevisedSubmittalDate] = vW.[RevisedSubmittalDate]
	,[TemplateBoe] = vW.[TemplateBoe]
FROM [dbo].[Workspace] W
	INNER JOIN [version].[Workspace] vW ON W.WorkspaceID = vW.WorkspaceID
	LEFT OUTER JOIN [dbo].[ResourceList] RL ON vW.ResourceListID = RL.ResourceListID
	LEFT OUTER JOIN [dbo].[PerformingOrganizationList] PL ON vW.PerformingOrganizationListID = PL.PerformingOrganizationListID
WHERE 
	vW.WorkspaceID = @WorkspaceID AND 
	vW.VersionID = @VersionID

DECLARE @MissingResource TABLE
	(
		ResourceID int,
		ResourceListID int,
		Processed bit
	)
/*Resource Redesign table*/
IF  EXISTS  (SELECT vR.ResourceID 
			FROM [version].[Resource] vR
						INNER JOIN [version].[WorkspaceResource] WR ON vR.ResourceID = WR.SystemResourceID
						INNER JOIN [version].Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
						LEFT OUTER JOIN dbo.[Resource] R ON vR.ResourceID = R.ResourceID
					WHERE 
						vR.VersionID = @VersionID AND
						WR.VersionID = @VersionID AND
						WS.VersionID = @VersionID AND 
						WS.WorkspaceID = @WorkspaceID AND
						R.ResourceID IS NULL
				) --IS NOT NULL
		BEGIN
			/*Need to Restore the deleted Workspace Resource*/
			INSERT INTO @MissingResource
			SELECT	DISTINCT vR.ResourceID, vR.ResourceListID, 0
			FROM [version].[Resource] vR
					INNER JOIN [version].[WorkspaceResource] WR ON vR.ResourceID = WR.SystemResourceID
					INNER JOIN [version].Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
					LEFT OUTER JOIN dbo.[Resource] R ON vR.ResourceID = R.ResourceID
				WHERE 
					vR.VersionID = @VersionID AND
					WR.VersionID = @VersionID AND
					WS.VersionID = @VersionID AND 
					WS.WorkspaceID = @WorkspaceID AND
					R.ResourceID IS NULL			
						
			SET IDENTITY_INSERT [dbo].[Resource] ON
			INSERT INTO [dbo].[Resource]
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
           )
		SELECT DISTINCT 
			   vR.[ResourceID]
			  ,vR.[UpdateDT]
			  ,vR.[ResourceName]
			  ,vR.[ResourceDescription]
			  ,vR.[SegmentRegion]
			  ,vR.[LaborType]
			  ,vR.[SegmentID]
			  ,vR.[ResourceListID]
			  ,vR.[CostElementID]
			  ,vR.[DeletedFlag]
			  ,vR.[RateTypeID]
			FROM [version].[Resource] vR
					INNER JOIN @MissingResource MR ON vR.ResourceID = MR.ResourceID
					INNER JOIN [version].[WorkspaceResource] WR ON vR.ResourceID = WR.SystemResourceID
					INNER JOIN [version].Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
					LEFT OUTER JOIN dbo.[Resource] R ON vR.ResourceID = R.ResourceID
				WHERE 
					vR.VersionID = @VersionID AND
					WR.VersionID = @VersionID AND
					WS.VersionID = @VersionID AND 
					WS.WorkspaceID = @WorkspaceID AND
					R.ResourceID IS NULL AND
					MR.ResourceListID <> 1
			SET IDENTITY_INSERT [dbo].[Resource] OFF					
		END
IF EXISTS (SELECT 1 FROM [version].[WorkspaceResource] WHERE VersionID = @VersionID)
BEGIN
SET IDENTITY_INSERT [dbo].[WorkspaceResource] ON
INSERT INTO [dbo].[WorkspaceResource]
           ([WorkspaceResourceID]
           ,[SystemResourceID]
           ,[ResourceListID]
           ,[WorkspaceID])
SELECT WR.[WorkspaceResourceID]
      ,WR.[SystemResourceID]
      ,WR.[ResourceListID]
      ,WR.[WorkspaceID]
  FROM [version].[WorkspaceResource] WR
INNER JOIN [version].Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
LEFT OUTER JOIN [dbo].[Resource] R ON WR.SystemResourceID = R.ResourceID
WHERE 
WR.VersionID = @VersionID AND
WS.VersionID = @VersionID AND
WS.WorkspaceID = @WorkspaceID AND
WR.SystemResourceID NOT IN (SELECT ResourceID FROM @MissingResource WHERE ResourceListID = 1)

SET IDENTITY_INSERT [dbo].[WorkspaceResource] OFF
END
IF EXISTS (SELECT 1 FROM [version].[WorkspaceContractTypeXREF] WHERE VersionID = @VersionID)
BEGIN

/*ContractType*/
SET IDENTITY_INSERT [dbo].[WorkspaceContractTypeXREF] ON
INSERT INTO [dbo].[WorkspaceContractTypeXREF]
           ([WorkspaceContractTypeID]
           ,[UpdateDT]
           ,[WorkspaceID]
           ,[ContractTypeID]
           )
SELECT WC.[WorkspaceContractTypeID]
      ,WC.[UpdateDT]
      ,WC.[WorkspaceID]
      ,WC.[ContractTypeID]
FROM [version].[WorkspaceContractTypeXREF] WC
	INNER JOIN [version].Workspace WS ON WC.WorkspaceID = WS.WorkspaceID
WHERE 
WC.VersionID = @VersionID AND
WS.WorkspaceID = @WorkspaceID AND 
WS.VersionID = @VersionID 

SET IDENTITY_INSERT [dbo].[WorkspaceContractTypeXREF] OFF

END
IF EXISTS (SELECT 1 FROM [version].[OutputFormatTemplateVersionXREF] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[OutputFormatTemplate] ON
INSERT INTO [dbo].[OutputFormatTemplate]
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
FROM [version].[OutputFormatTemplate] OFT
INNER JOIN [version].OutputFormatTemplateWorkspaceXREF OX ON OFT.TemplateID = OX.TemplateID
INNER JOIN [version].Workspace WS ON OX.WorkspaceID = WS.WorkspaceID
INNER JOIN [version].[OutputFormatTemplateVersionXREF] OVX ON OFT.BackupTemplateID = OVX.BackupTemplateID
/*
Check the "real" Template table to see if the Template is there and
only restore if it is not there*/
LEFT OUTER JOIN [dbo].[OutputFormatTemplate] dbo_OFT ON OFT.TemplateID = dbo_OFT.TemplateID
WHERE 
OVX.VersionID = @VersionID AND
OX.VersionID = @VersionID AND
WS.WorkspaceID = @WorkspaceID AND 
WS.VersionID = @VersionID AND
dbo_OFT.TemplateID IS NULL

SET IDENTITY_INSERT [dbo].[OutputFormatTemplate] OFF

END
IF EXISTS (SELECT 1 FROM [version].[OutputFormatTemplateWorkspaceXREF] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[OutputFormatTemplateWorkspaceXREF] ON
INSERT INTO [dbo].[OutputFormatTemplateWorkspaceXREF]
([OutputFormatID]
,[WorkspaceID]
,[TemplateID]
)
SELECT OX.[OutputFormatID]
,OX.[WorkspaceID]
,OX.[TemplateID]
FROM [version].[OutputFormatTemplateWorkspaceXREF] OX
INNER JOIN [version].Workspace WS ON OX.WorkspaceID = WS.WorkspaceID
WHERE  
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND 
OX.VersionID = @VersionID

SET IDENTITY_INSERT [dbo].[OutputFormatTemplateWorkspaceXREF] OFF

END
IF EXISTS (SELECT 1 FROM [version].[BOE] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT  [dbo].[BOE] ON
INSERT INTO [dbo].[BOE]
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
,[BOETitle]
,[IsMultiClinWbs]
)
SELECT B.[BOEID]
/*Wireframes - All BOEs status are changed to 2 (DRAFT) B.[BOEStateID]*/
/*WI 5989 Change BOE state back to Draft if its current state is Draft,Awaiting Approval or Approved
Unassigned BOEs should be left as is*/
,CASE B.[BOEStateID]
	WHEN 0 THEN 0
	WHEN 1 THEN 1
	WHEN 2 THEN 2
	WHEN 3 THEN 2
	WHEN 4 THEN 2
	WHEN 6 THEN 2	--BOEJ-471
END AS BOEStateID
,B.[BOEStartDate]
,B.[BOEEndDate]
,B.[BOEDescription]
,B.[DataSource]
,B.[WorkspaceID]
,B.[MetricDisclosureAcknowledge]
,B.[NumAuthorReassigned]
,B.[IsMaterial]
,B.[UpdateDT]
,B.[BOETitle]
,B.[IsMultiClinWbs]
FROM [version].[BOE] B
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE 
B.VersionID = @VersionID AND
WS.VersionID = @VersionID AND
WS.WorkspaceID = @WorkspaceID 
SET IDENTITY_INSERT  [dbo].[BOE] OFF
END
IF EXISTS (SELECT 1 FROM [version].[ODCTaskElement] WHERE VersionID = @VersionID)
BEGIN


SET IDENTITY_INSERT  [dbo].[ODCTaskElement] ON
INSERT INTO [dbo].[ODCTaskElement]
([ODCTaskElementID]
,[ODCTaskTitle]
,[ODCTaskDescription]
,[ODCMOQText]
,[BOEID]
,[ODCTaskID]
,[UpdateDT]
,[TaskStartDate]
,[TaskEndDate]
,[SortOrderID]
)

SELECT TE.[ODCTaskElementID]
,TE.[ODCTaskTitle]
,TE.[ODCTaskDescription]
,TE.[ODCMOQText]
,TE.[BOEID]
,TE.[ODCTaskID]
,TE.[UpdateDT]
,TE.[TaskStartDate]
,TE.[TaskEndDate]
,TE.[SortOrderID]
FROM [version].[ODCTaskElement] TE
INNER JOIN [version].BOE B ON TE.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE   
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND 
TE.VersionID = @VersionID AND 
B.VersionID = @VersionID

SET IDENTITY_INSERT  [dbo].[ODCTaskElement] OFF
END
IF EXISTS (SELECT vR.ResourceID 
					FROM [version].[Resource] vR
						INNER JOIN [version].[ODCType] T ON vR.ResourceID = T.ResourceID
						INNER JOIN [version].[ODCTaskElement] TE ON T.ODCTaskElementID = TE.ODCTaskElementID
						INNER JOIN [version].BOE B ON TE.BOEID  = B.BOEID
						INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
						LEFT OUTER JOIN dbo.[Resource] R ON vR.ResourceID = R.ResourceID
					WHERE 
						vR.VersionID = @VersionID AND
						T.VersionID = @VersionID AND
						TE.VersionID = @VersionID AND
						B.VersionID = @VersionID AND 
						WS.VersionID = @VersionID AND 
						WS.WorkspaceID = @WorkspaceID AND
						R.ResourceID IS NULL
				) --IS NOT NULL
		BEGIN
					
			SELECT	DISTINCT	@ErrorMessage = 
				IsNull(@ErrorMessage,'WARNING: ') + 
				'ResourceID: ' + 
				CAST (vR.ResourceID AS varchar(10)) + 
				' Resource Name: ' + vR.ResourceName + 
				' no longer exists.' + 
				CHAR(13)
			FROM [version].[Resource] vR
						INNER JOIN [version].[ODCType] T ON vR.ResourceID = T.ResourceID
						INNER JOIN [version].[ODCTaskElement] TE ON T.ODCTaskElementID = TE.ODCTaskElementID
						INNER JOIN [version].BOE B ON TE.BOEID  = B.BOEID
						INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
						LEFT OUTER JOIN dbo.[Resource] R ON vR.ResourceID = R.ResourceID
					WHERE 
						vR.VersionID = @VersionID AND
						T.VersionID = @VersionID AND
						TE.VersionID = @VersionID AND
						B.VersionID = @VersionID AND 
						WS.VersionID = @VersionID AND 
						WS.WorkspaceID = @WorkspaceID AND
						R.ResourceID IS NULL			
		END

DECLARE @MissingPerformingOrganization TABLE
	(
		PerformingOrganizationID int,
		PerformingOrganizationListID int,
		Processed bit
	)
/*PerformingOrganization Redesign table*/
IF EXISTS  (SELECT vR.PerformingOrganizationID 
			FROM [version].[PerformingOrganization] vR
						INNER JOIN [version].[WorkspacePerformingOrganization] WR ON vR.PerformingOrganizationID = WR.SystemPerformingOrganizationID
						INNER JOIN [version].Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
						LEFT OUTER JOIN dbo.[PerformingOrganization] R ON vR.PerformingOrganizationID = R.PerformingOrganizationID
					WHERE 
						vR.VersionID = @VersionID AND
						WR.VersionID = @VersionID AND
						WS.VersionID = @VersionID AND 
						WS.WorkspaceID = @WorkspaceID AND
						R.PerformingOrganizationID IS NULL
				) --IS NOT NULL
		BEGIN
			/*Need to Restore the deleted Workspace PerformingOrganization*/
			INSERT INTO @MissingPerformingOrganization
			SELECT	DISTINCT vR.PerformingOrganizationID, vR.PerformingOrganizationListID, 0
			FROM [version].[PerformingOrganization] vR
					INNER JOIN [version].[WorkspacePerformingOrganization] WR ON vR.PerformingOrganizationID = WR.SystemPerformingOrganizationID
					INNER JOIN [version].Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
					LEFT OUTER JOIN dbo.[PerformingOrganization] R ON vR.PerformingOrganizationID = R.PerformingOrganizationID
				WHERE 
					vR.VersionID = @VersionID AND
					WR.VersionID = @VersionID AND
					WS.VersionID = @VersionID AND 
					WS.WorkspaceID = @WorkspaceID AND
					R.PerformingOrganizationID IS NULL			
						
			SET IDENTITY_INSERT [dbo].[PerformingOrganization] ON
			INSERT INTO [dbo].[PerformingOrganization]
           ([PerformingOrganizationID]
           ,[UpdateDT]
           ,[PerformingOrganizationName]
           ,[PerformingOrganizationDescription]
           ,[PerformingOrganizationListID]
           ,[DeletedFlag])
		SELECT DISTINCT 
			   vR.[PerformingOrganizationID]
			  ,vR.[UpdateDT]
			  ,vR.[PerformingOrganizationName]
			  ,vR.[PerformingOrganizationDescription]
			  ,vR.[PerformingOrganizationListID]
			  ,vR.[DeletedFlag]
			FROM [version].[PerformingOrganization] vR
					INNER JOIN @MissingPerformingOrganization MR ON vR.PerformingOrganizationID = MR.PerformingOrganizationID
					INNER JOIN [version].[WorkspacePerformingOrganization] WR ON vR.PerformingOrganizationID = WR.SystemPerformingOrganizationID
					INNER JOIN [version].Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
					LEFT OUTER JOIN dbo.[PerformingOrganization] R ON vR.PerformingOrganizationID = R.PerformingOrganizationID
				WHERE 
					vR.VersionID = @VersionID AND
					WR.VersionID = @VersionID AND
					WS.VersionID = @VersionID AND 
					WS.WorkspaceID = @WorkspaceID AND
					R.PerformingOrganizationID IS NULL AND
					MR.PerformingOrganizationListID <> 1
			SET IDENTITY_INSERT [dbo].[PerformingOrganization] OFF					
		END
IF EXISTS (SELECT 1 FROM [version].[WorkspacePerformingOrganization] WHERE VersionID = @VersionID)
BEGIN
SET IDENTITY_INSERT [dbo].[WorkspacePerformingOrganization] ON
INSERT INTO [dbo].[WorkspacePerformingOrganization]
           ([WorkspacePerformingOrganizationID]
           ,[SystemPerformingOrganizationID]
           ,[PerformingOrganizationListID]
           ,[WorkspaceID])
SELECT WR.[WorkspacePerformingOrganizationID]
      ,WR.[SystemPerformingOrganizationID]
      ,WR.[PerformingOrganizationListID]
      ,WR.[WorkspaceID]
  FROM [version].[WorkspacePerformingOrganization] WR
INNER JOIN [version].Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
LEFT OUTER JOIN [dbo].[PerformingOrganization] R ON WR.SystemPerformingOrganizationID = R.PerformingOrganizationID
WHERE 
WR.VersionID = @VersionID AND
WS.VersionID = @VersionID AND
WS.WorkspaceID = @WorkspaceID AND
WR.SystemPerformingOrganizationID NOT IN (SELECT PerformingOrganizationID FROM @MissingPerformingOrganization WHERE PerformingOrganizationListID = 1)


SET IDENTITY_INSERT [dbo].[WorkspacePerformingOrganization] OFF

END
IF EXISTS  (SELECT vPO.PerformingOrganizationID 
					FROM [version].[PerformingOrganization] vPO
						INNER JOIN [version].[ODCType] T ON vPO.PerformingOrganizationID = T.PerformingOrganizationID
						INNER JOIN [version].[ODCTaskElement] TE ON T.ODCTaskElementID = TE.ODCTaskElementID
						INNER JOIN [version].BOE B ON TE.BOEID  = B.BOEID
						INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
						LEFT OUTER JOIN dbo.[PerformingOrganization] PO ON vPO.PerformingOrganizationID = PO.PerformingOrganizationID
					WHERE 
						vPO.VersionID = @VersionID AND
						T.VersionID = @VersionID AND
						TE.VersionID = @VersionID AND
						B.VersionID = @VersionID AND 
						WS.VersionID = @VersionID AND 
						WS.WorkspaceID = @WorkspaceID AND
						PO.PerformingOrganizationID IS NULL
				) --IS NOT NULL
		BEGIN
				
			SELECT	DISTINCT	@ErrorMessage = 
				IsNull(@ErrorMessage,'WARNING: ') + 
				'PerformingOrganizationID: ' + 
				CAST (vPO.PerformingOrganizationID AS varchar(10)) + 
				' Performing Organization Name: ' + 
				vPO.PerformingOrganizationName + 
				' no longer exists.' + 
				CHAR(13)
			FROM [version].[PerformingOrganization] vPO
						INNER JOIN [version].[ODCType] T ON vPO.PerformingOrganizationID = T.PerformingOrganizationID
						INNER JOIN [version].[ODCTaskElement] TE ON T.ODCTaskElementID = TE.ODCTaskElementID
						INNER JOIN [version].BOE B ON TE.BOEID  = B.BOEID
						INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
						LEFT OUTER JOIN  dbo.[PerformingOrganization] PO ON vPO.PerformingOrganizationID = PO.PerformingOrganizationID
					WHERE 
						vPO.VersionID = @VersionID AND
						T.VersionID = @VersionID AND
						TE.VersionID = @VersionID AND
						B.VersionID = @VersionID AND 
						WS.VersionID = @VersionID AND 
						WS.WorkspaceID = @WorkspaceID AND
						PO.PerformingOrganizationID IS NULL

		END
IF EXISTS (SELECT 1 FROM [version].[ODCType] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[ODCType] ON
INSERT INTO [dbo].[ODCType]
([ODCTypeID]
,[ResourceID]
,[PerformingOrganizationID]
,[ODCTypeStartDate]
,[ODCTypeEndDate]
,[SpreadCurveID]
,[ODCTypeCost]
,[ODCTaskElementID]
,[UpdateDT]
)
SELECT T.[ODCTypeID]
,R.[ResourceID]
,PO.[PerformingOrganizationID]
,T.[ODCTypeStartDate]
,T.[ODCTypeEndDate]
,T.[SpreadCurveID]
,T.[ODCTypeCost]
,T.[ODCTaskElementID]
,T.[UpdateDT]
FROM [version].[ODCType] T
INNER JOIN [version].[ODCTaskElement] TE ON T.ODCTaskElementID = TE.ODCTaskElementID
INNER JOIN [version].BOE B ON TE.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
LEFT OUTER JOIN [dbo].[Resource] R ON T.ResourceID = R.ResourceID
LEFT OUTER JOIN [dbo].[PerformingOrganization] PO ON T.PerformingOrganizationID = PO.PerformingOrganizationID
WHERE 
T.VersionID = @VersionID AND
TE.VersionID = @VersionID AND
B.VersionID = @VersionID AND 
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID


SET IDENTITY_INSERT [dbo].[ODCType] OFF

END
IF EXISTS (SELECT 1 FROM [version].[ODCSpread] WHERE VersionID = @VersionID)
BEGIN
SET IDENTITY_INSERT [dbo].[ODCSpread] ON
INSERT INTO [dbo].[ODCSpread]
([ODCSpreadID]
,[ODCTypeID]
,[ODCSpreadDate]
,[ODCSpreadValue]
)
SELECT S.[ODCSpreadID]
,S.[ODCTypeID]
,S.[ODCSpreadDate]
,S.[ODCSpreadValue]
FROM [version].[ODCSpread] S
INNER JOIN [version].[ODCType] T ON S.ODCTypeID = T.ODCTypeID
INNER JOIN [version].[ODCTaskElement] TE ON T.ODCTaskElementID = TE.ODCTaskElementID
INNER JOIN [version].BOE B ON TE.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE   WS.VersionID = @VersionID AND WS.WorkspaceID = @WorkspaceID AND S.VersionID = @VersionID AND
T.VersionID = @VersionID AND
TE.VersionID = @VersionID AND
B.VersionID = @VersionID

SET IDENTITY_INSERT [dbo].[ODCSpread] OFF

END
IF EXISTS (SELECT 1 FROM [version].[MaterialTaskElement] WHERE VersionID = @VersionID)
BEGIN
SET IDENTITY_INSERT  [dbo].[MaterialTaskElement] ON
INSERT INTO [dbo].[MaterialTaskElement]
           ([MaterialTaskElementID]
           ,[MaterialTaskID]
           ,[MaterialTaskTitle]
           ,[MaterialTaskDescription]
           ,[MaterialMOQText]
           ,[BOEID]
           ,[UpdateDT])
SELECT TE.[MaterialTaskElementID]
      ,TE.[MaterialTaskID]
      ,TE.[MaterialTaskTitle]
      ,TE.[MaterialTaskDescription]
      ,TE.[MaterialMOQText]
      ,TE.[BOEID]
      ,TE.[UpdateDT]
FROM [version].[MaterialTaskElement] TE
INNER JOIN [version].BOE B ON TE.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE   
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND 
TE.VersionID = @VersionID AND 
B.VersionID = @VersionID

SET IDENTITY_INSERT  [dbo].[MaterialTaskElement] OFF

END
IF EXISTS (SELECT 1 FROM [version].[TravelTripTaskElement] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT dbo.TravelTripTaskElement ON

INSERT INTO [dbo].[TravelTripTaskElement]
           ([TravelTripTaskElementID]
           ,[UpdateDT]
           ,[TravelTaskID]
           ,[TravelTaskTitle]
           ,[TravelTaskDescription]
           ,[BOEID]
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
      ,TE.[TaskStartDate]
      ,TE.[TaskEndDate]
	  ,TE.[SortOrderID]
FROM [version].[TravelTripTaskElement] TE 
	INNER JOIN [version].[BOE] B ON TE.BOEID = B.BOEID
	INNER JOIN [version].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
WHERE
	TE.VersionID = @VersionID AND
	B.VersionID = @VersionID AND
	WS.VersionID = @VersionID AND
	WS.WorkspaceID = @WorkspaceID

SET IDENTITY_INSERT dbo.TravelTripTaskElement OFF

END

/*Bug #8146*/
IF  EXISTS  (SELECT vT.TripID
					FROM [version].[Trip] vT 
						INNER JOIN [version].[TravelTrip] TT ON vT.TripID = TT.TripID
						INNER JOIN [version].[TravelTripTaskElement] TE ON TT.TravelTripTaskElementID = TE.TravelTripTaskElementID
						INNER JOIN [version].[BOE] B ON TE.BOEID = B.BOEID
						INNER JOIN [version].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
						LEFT OUTER JOIN [dbo].[Trip] T ON vT.TripID = T.TripID 
	/*WI8256					LEFT OUTER JOIN [dbo].[Trip] OrigTrip ON TT.OriginatingTripID = OrigTrip.TripID*/
					WHERE 
						vT.VersionID = @VersionID AND
						TT.VersionID = @VersionID AND
						TE.VersionID = @VersionID AND
						B.VersionID = @VersionID AND
						WS.VersionID = @VersionID AND
						WS.WorkspaceID = @WorkspaceID AND
						(
							T.TripID IS NULL/*WI8256 AND
							OrigTrip.TripID IS NULL*/
						)
					
				) --IS NOT NULL
		BEGIN

			SELECT	DISTINCT	@ErrorMessage = 
				IsNull(@ErrorMessage,'WARNING: ') + 
				'TripID: ' + 
				CAST (vT.TripID AS varchar(10)) + 
 				CHAR(13)  + 
 				
				' Departure: ' + 
				CAST (Departure.LocationName AS varchar(10)) + 
 				CHAR(13)  + 
 				
				' Destination: ' + 
				CAST (Destination.LocationName AS varchar(10)) + 
 				CHAR(13) +
 													
				' no longer exists.' + 
				CHAR(13)
			FROM [version].[Trip] vT 
						INNER JOIN [version].[TravelTrip] TT ON vT.TripID = TT.TripID
						INNER JOIN [version].[TravelTripTaskElement] TE ON TT.TravelTripTaskElementID = TE.TravelTripTaskElementID
						INNER JOIN [version].[BOE] B ON TE.BOEID = B.BOEID
						INNER JOIN [version].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
						INNER JOIN [dbo].[Location] Departure ON vt.DepartureLocationID = Departure.LocationID
						INNER JOIN [dbo].[Location] Destination ON vt.DestinationLocationID = Destination.LocationID
						LEFT OUTER JOIN [dbo].[Trip] T ON T.TripID = vT.TripID 						
					WHERE 
						vT.VersionID = @VersionID AND
						TT.VersionID = @VersionID AND
						TE.VersionID = @VersionID AND
						B.VersionID = @VersionID AND
						WS.VersionID = @VersionID AND
						WS.WorkspaceID = @WorkspaceID AND 
						T.TripID IS NULL


		END
IF  EXISTS  (SELECT vPO.PerformingOrganizationID 
			FROM [version].[PerformingOrganization] vPO 
						INNER JOIN [version].[TravelTrip] TT ON vPO.PerformingOrganizationID = TT.PerformingOrganizationID
						INNER JOIN [version].[TravelTripTaskElement] TE ON TT.TravelTripTaskElementID = TE.TravelTripTaskElementID
						INNER JOIN [version].[BOE] B ON TE.BOEID = B.BOEID
						INNER JOIN [version].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
						LEFT OUTER JOIN  dbo.[PerformingOrganization] PO ON vPO.PerformingOrganizationID = PO.PerformingOrganizationID
					WHERE 
						vPO.VersionID = @VersionID AND
						TT.VersionID = @VersionID AND
						TE.VersionID = @VersionID AND
						B.VersionID = @VersionID AND
						WS.VersionID = @VersionID AND
						WS.WorkspaceID = @WorkspaceID AND
						PO.PerformingOrganizationID IS NULL
				) --IS NOT NULL
		BEGIN
				
			SELECT	DISTINCT	@ErrorMessage = 
				IsNull(@ErrorMessage,'WARNING: ') + 
				'PerformingOrganizationID: ' + 
				CAST (vPO.PerformingOrganizationID AS varchar(10)) + 
				' Performing Organization Name: ' + 
				vPO.PerformingOrganizationName + 
				' no longer exists.' + 
				CHAR(13)
			FROM [version].[PerformingOrganization] vPO 
						INNER JOIN [version].[TravelTrip] TT ON vPO.PerformingOrganizationID = TT.PerformingOrganizationID
						INNER JOIN [version].[TravelTripTaskElement] TE ON TT.TravelTripTaskElementID = TE.TravelTripTaskElementID
						INNER JOIN [version].[BOE] B ON TE.BOEID = B.BOEID
						INNER JOIN [version].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
						LEFT OUTER JOIN  dbo.[PerformingOrganization] PO ON vPO.PerformingOrganizationID = PO.PerformingOrganizationID
					WHERE 
						vPO.VersionID = @VersionID AND
						TT.VersionID = @VersionID AND
						TE.VersionID = @VersionID AND
						B.VersionID = @VersionID AND
						WS.VersionID = @VersionID AND
						WS.WorkspaceID = @WorkspaceID AND
						PO.PerformingOrganizationID IS NULL

		END
IF EXISTS (SELECT 1 FROM [version].[TravelTrip] WHERE VersionID = @VersionID)
BEGIN
/*Bug 8146*/
SET IDENTITY_INSERT dbo.TravelTrip ON

INSERT INTO [dbo].[TravelTrip]
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
/*WI8256     ,[OriginatingTripID]*/
			)
SELECT TT.[TravelTripID]
      ,TT.[UpdateDT]
      ,TT.[GroupID]
      ,TT.[SegmentID]
      ,PO.[PerformingOrganizationID]
/*WI8256      ,
      /*BUG 8146*/
		CASE	
			WHEN T.[TripID] IS NULL THEN OT.TripID	
			ELSE*/
		, T.[TripID]
/*WI8256		END AS TripID			*/
			
      ,TT.[TripDate]
      ,TT.[NumTrips]
      ,TT.[NumPeople]
      ,TT.[NumDays]
      ,TT.[Purpose]
      ,TT.[TravelTripTaskElementID]
      ,TT.[TripLockedDT]
      /*BUG 8146*/
/*WI8256		, CASE	
			WHEN T.[TripID] IS NULL THEN NULL
			ELSE TT.[OriginatingTripID]
		  END AS [OriginatingTripID]*/
FROM [version].[TravelTrip] TT 
	INNER JOIN [version].[TravelTripTaskElement] TE ON TT.TravelTripTaskElementID = TE.TravelTripTaskElementID
	INNER JOIN [version].[BOE] B ON TE.BOEID = B.BOEID
	INNER JOIN [version].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
	/*INNER JOIN TO CONFIRM ALL DATA IS AVAILABLE*/
	LEFT OUTER JOIN [dbo].[PerformingOrganization] PO ON TT.PerformingOrganizationID = PO.PerformingOrganizationID

	LEFT OUTER JOIN [dbo].[Trip] T ON TT.TripID = T.TripID
/*WI8256	LEFT OUTER JOIN [dbo].[Trip] OT ON TT.OriginatingTripID = OT.TripID	*/
	
	
WHERE
	TT.VersionID = @VersionID AND
	TE.VersionID = @VersionID AND
	B.VersionID = @VersionID AND
	WS.VersionID = @VersionID AND
	WS.WorkspaceID = @WorkspaceID
	
	
SET IDENTITY_INSERT dbo.TravelTrip OFF

END

IF EXISTS (SELECT 1 FROM [version].[WorkspaceOffloadRate] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[WorkspaceOffloadRate] ON
INSERT INTO [dbo].[WorkspaceOffloadRate]
([OffloadRateID]
	,[UpdateDT]
	,[WorkspaceID]
	,[Resource]
	,[PerfOrg]
	,[PercentToOffload]
	,[Year]
	,[SubcontractorResource]
	,[HourlyRate]
)
SELECT WOR.[OffloadRateID]
	,WOR.[UpdateDT]
	,WOR.[WorkspaceID]
	,WOR.[Resource]
	,WOR.[PerfOrg]
	,WOR.[PercentToOffload]
	,WOR.[Year]
	,WOR.[SubcontractorResource]
	,WOR.[HourlyRate]
FROM [version].[WorkspaceOffloadRate] WOR
WHERE 
WOR.VersionID = @VersionID AND
WOR.WorkspaceID = @WorkspaceID
SET IDENTITY_INSERT [dbo].[WorkspaceOffloadRate] OFF

END

IF EXISTS (SELECT 1 FROM [version].[ProjectMap] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[ProjectMap] ON
INSERT INTO [dbo].[ProjectMap]
([ID]
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
)
SELECT PM.[ID]
	,PM.[WorkspaceId]
	,PM.[WbsNumber]
	,PM.[WbsElementTitle]
	,PM.[ActivityID]
	,PM.[ActivityName]
	,PM.[Resource]
	,PM.[CostCenter] 
	,PM.[StartDate] 
	,PM.[EndDate] 
	,PM.[CLIN] 
	,PM.[Task] 
	,PM.[SOW] 
	,PM.[SOWTitle] 
	,PM.[Rationale]
	,PM.[CamName] 
	,PM.[Category] 
	,PM.[Hours]
	,PM.[Dollars] 
	,PM.[CanOffload]
	,PM.[AddOrDelete] 
	,PM.[ClassOfCost] 
	,PM.[OrderID]
	,PM.[TieredPercentage]
	,PM.[LegacyResourceID]
FROM [version].[ProjectMap] PM
WHERE 
PM.VersionID = @VersionID AND
PM.WorkspaceID = @WorkspaceID
SET IDENTITY_INSERT [dbo].[ProjectMap] OFF

INSERT INTO [dbo].[ProjectMapSpread]
           ([WorkspaceId]
		   ,[ProjectMapId]
           ,[SpreadDate]
           ,[SpreadValue]
           )
SELECT 	    S.[WorkspaceId],
			S.[ProjectMapId],
			S.[SpreadDate],
			S.[SpreadValue]
FROM  [version].ProjectMapSpread S
WHERE 
S.VersionID = @VersionID AND 
S.[WorkspaceId] = @WorkspaceID

END

/** Custom Fields **/

IF EXISTS (SELECT 1 FROM [version].[CustomField] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[CustomField] ON
INSERT INTO [dbo].[CustomField]
([CustomFieldID]
,[CustomFieldName]
,[CustomFieldRequired]
,[CustomFieldDisplayID]
,[WorkspaceID]
,[UpdateDT]
,[IsOpenEnded]
)
SELECT CF.[CustomFieldID]
,CF.[CustomFieldName]
,CF.[CustomFieldRequired]
,CF.[CustomFieldDisplayID]
,CF.[WorkspaceID]
,CF.[UpdateDT]
,CF.[IsOpenEnded]
FROM [version].[CustomField] CF
INNER JOIN [version].Workspace WS ON CF.WorkspaceID = WS.WorkspaceID
WHERE 
WS.VersionID = @VersionID AND
WS.WorkspaceID = @WorkspaceID AND 
CF.VersionID = @VersionID
SET IDENTITY_INSERT [dbo].[CustomField] OFF

END
IF EXISTS (SELECT 1 FROM [version].[CLIN] WHERE VersionID = @VersionID)
BEGIN


SET IDENTITY_INSERT [dbo].[CLIN] ON
INSERT INTO [dbo].[CLIN]
([CLINID]
,[CLINNumber]
,[CLINTitle]
,[CLINStartDate]
,[CLINEndDate]
,[ContractTypeID]
,[WorkspaceID]
,[UpdateDT]
,[DisplayedCLINNumber]
)
SELECT C.[CLINID]
,C.[CLINNumber]
,C.[CLINTitle]
,C.[CLINStartDate]
,C.[CLINEndDate]
,C.[ContractTypeID]
,C.[WorkspaceID]
,C.[UpdateDT]
,C.[DisplayedCLINNumber]
FROM [version].[CLIN] C
INNER JOIN [version].Workspace WS ON C.WorkspaceID = WS.WorkspaceID
WHERE WS.VersionID = @VersionID AND WS.WorkspaceID = @WorkspaceID	 AND C.VersionID = @VersionID
SET IDENTITY_INSERT [dbo].[CLIN] OFF


END
IF EXISTS (SELECT 1 FROM [version].[BOEPotentialRole] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[BOEPotentialRole] ON
INSERT INTO [dbo].[BOEPotentialRole]
([BOEPotentialRoleID]
,[ETIUserID]
/*,[ETIGroupID]*/
,[WorkspaceID]
,[RoleID]
,[UserRemoved]
,[UpdateDT]
)
SELECT BR.[BOEPotentialRoleID]
,BR.[ETIUserID]
/*,BR.[ETIGroupID]*/
,BR.[WorkspaceID]
,BR.[RoleID]
,BR.[UserRemoved]
,BR.[UpdateDT]
FROM [version].[BOEPotentialRole] BR
INNER JOIN [version].Workspace WS ON BR.WorkspaceID = WS.WorkspaceID
WHERE WS.VersionID = @VersionID AND  WS.WorkspaceID = @WorkspaceID	 AND BR.VersionID = @VersionID
SET IDENTITY_INSERT [dbo].[BOEPotentialRole] OFF
END
IF EXISTS (SELECT 1 FROM [version].[WorkBreakdownStructure] WHERE VersionID = @VersionID)
BEGIN
SET IDENTITY_INSERT [dbo].[WorkBreakdownStructure] ON
INSERT INTO [dbo].[WorkBreakdownStructure]
(			[WBSID]
           ,[UpdateDT]
           ,[WBSNumber]
           ,[DisplayedWBSNumber]
           ,[WBSTitle]
           ,[WorkspaceID])
SELECT 
WBS.[WBSID]
,WBS.[UpdateDT]
,WBS.[WBSNumber]
,WBS.[DisplayedWBSNumber]
,WBS.[WBSTitle]
,WBS.[WorkspaceID]
FROM [version].[WorkBreakdownStructure] WBS
INNER JOIN [version].Workspace WS ON WBS.WorkspaceID = WS.WorkspaceID
WHERE  WS.VersionID = @VersionID AND WS.WorkspaceID = @WorkspaceID	 AND WBS.VersionID = @VersionID
SET IDENTITY_INSERT [dbo].[WorkBreakdownStructure] OFF
END

IF EXISTS  (SELECT vTMRR.[TMResourceID] 
		FROM [version].[TMResourceRate] vTMRR
			INNER JOIN [version].[Workspace] WS ON vTMRR.WorkspaceID = WS.WorkspaceID			
			LEFT OUTER JOIN [dbo].[Resource] R ON vTMRR.[TMResourceID] = R.ResourceID
		WHERE 
			vTMRR.VersionID = @VersionID AND
			WS.VersionID = @VersionID AND 
			WS.WorkspaceID = @WorkspaceID AND
			R.ResourceID IS NULL
		)
		BEGIN
		
		SELECT 	DISTINCT	@ErrorMessage = 
				IsNull(@ErrorMessage,'WARNING: ') + 
				ErrorMessage.ErrorMessage
		FROM 
			(	SELECT 
				'ResourceID: ' + 
				CAST (vR.ResourceID AS varchar(10)) + 
				' Resource Name: ' + 
				vR.ResourceName + 
				' no longer exists.' + 
				CHAR(13) AS ErrorMessage
		FROM [version].[TMResourceRate] vTMRR
			INNER JOIN [version].[Resource] vR ON vTMRR.[TMResourceID] = vR.ResourceID
			INNER JOIN [version].[Workspace] WS ON vTMRR.WorkspaceID = WS.WorkspaceID			
			LEFT OUTER JOIN [dbo].[Resource] R ON vTMRR.[TMResourceID] =  R.ResourceID
		WHERE 
			vTMRR.VersionID = @VersionID AND
			WS.VersionID = @VersionID AND 
			WS.WorkspaceID = @WorkspaceID AND
			R.ResourceID IS NULL AND
			vR.VersionID = @VersionID 
		) ErrorMessage				
					
		END

IF EXISTS (SELECT 1 FROM [version].[TMResourceRate] WHERE VersionID = @VersionID)
BEGIN
SET IDENTITY_INSERT [dbo].[TMResourceRate] ON

INSERT INTO [dbo].[TMResourceRate]
           ([TMResourceRateID]
           ,[UpdateDT]
           ,[WorkspaceID]
           ,[TMResourceID]
           ,[TMResourceRateStartDate]
           ,[TMResourceRateEndDate]
           ,[TMResourceRate])
SELECT TMRR.[TMResourceRateID]
      ,TMRR.[UpdateDT]
      ,TMRR.[WorkspaceID]
      ,TMRR.[TMResourceID]
      ,TMRR.[TMResourceRateStartDate]
      ,TMRR.[TMResourceRateEndDate]
      ,TMRR.[TMResourceRate]
FROM [version].[TMResourceRate] TMRR
	INNER JOIN [version].Workspace WS ON TMRR.WorkspaceID = WS.WorkspaceID
	LEFT OUTER JOIN [dbo].[Resource] R ON TMRR.TMResourceID = R.ResourceID
WHERE 
TMRR.VersionID = @VersionID AND
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID

SET IDENTITY_INSERT [dbo].[TMResourceRate] OFF

END

IF EXISTS (SELECT 1 FROM [version].[WorkspaceStateHistory] WHERE VersionID = @VersionID)
BEGIN
SET IDENTITY_INSERT [dbo].[WorkspaceStateHistory] ON
INSERT INTO [dbo].[WorkspaceStateHistory]
([WorkspaceStateHistoryID]
,[WorkspaceID]
,[CurrentWorkspaceStateID]
,[UpdatedWorkspaceStateID]
,[ChangedByETIUserID]
,[UpdateDT]
)
SELECT WSH.[WorkspaceStateHistoryID]
,WSH.[WorkspaceID]
,WSH.[CurrentWorkspaceStateID]
,WSH.[UpdatedWorkspaceStateID]
,WSH.[ChangedByETIUserID]
,WSH.[UpdateDT]
FROM [version].[WorkspaceStateHistory] WSH
INNER JOIN [version].Workspace WS ON WSH.WorkspaceID = WS.WorkspaceID
WHERE 
WS.VersionID = @VersionID AND  
WS.WorkspaceID = @WorkspaceID AND 
WSH.VersionID = @VersionID
SET IDENTITY_INSERT [dbo].[WorkspaceStateHistory] OFF

END
IF EXISTS (SELECT 1 FROM [version].[WorkspaceVariable] WHERE VersionID = @VersionID)
BEGIN
SET IDENTITY_INSERT [dbo].[WorkspaceVariable] ON
INSERT INTO [dbo].[WorkspaceVariable]
([WorkspaceVariableID]
,[WorkspaceVariableName]
,[WorkspaceVariableValue]
,[WorkspaceID]
,[SortByID]
,[ValueTypeID]
,[IsPercentage]
,[UpdateDT]
)
SELECT WSV.[WorkspaceVariableID]
,WSV.[WorkspaceVariableName]
,WSV.[WorkspaceVariableValue]
,WSV.[WorkspaceID]
,WSV.[SortByID]
,WSV.[ValueTypeID]
,WSV.[IsPercentage]
,WSV.[UpdateDT]
FROM [version].[WorkspaceVariable] WSV
INNER JOIN [version].Workspace WS ON WSV.WorkspaceID = WS.WorkspaceID
WHERE  
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND 
WSV.VersionID = @VersionID
SET IDENTITY_INSERT [dbo].[WorkspaceVariable] OFF

END
IF EXISTS (SELECT 1 FROM [version].[WorkspaceUserRole] WHERE VersionID = @VersionID)
BEGIN
SET IDENTITY_INSERT [dbo].[WorkspaceUserRole] ON
INSERT INTO [dbo].[WorkspaceUserRole]
([WorkspaceUserRoleID]
,[ETIUserID]
/*,[ETIGroupID]*/
,[RoleID]
,[WorkspaceID]
,[HideHelp]
,[UpdateDT]
)
SELECT WUR.[WorkspaceUserRoleID]
,WUR.[ETIUserID]
/*,WUR.[ETIGroupID]*/
,WUR.[RoleID]
,WUR.[WorkspaceID]
,WUR.[HideHelp]
,WUR.[UpdateDT]
FROM [version].[WorkspaceUserRole] WUR
INNER JOIN [version].Workspace WS ON WUR.WorkspaceID = WS.WorkspaceID
WHERE  
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND 
WUR.VersionID = @VersionID
SET IDENTITY_INSERT [dbo].[WorkspaceUserRole] OFF

END
IF EXISTS (SELECT 1 FROM [version].[ProPricerExport] WHERE VersionID = @VersionID)
BEGIN
SET IDENTITY_INSERT  [dbo].[ProPricerExport] ON
INSERT INTO [dbo].[ProPricerExport]
([ProPricerExportID]
,[ProPricerExportName]
,[WorkspaceID]
,[UpdateDT]
)
SELECT PPE.[ProPricerExportID]
,PPE.[ProPricerExportName]
,PPE.[WorkspaceID]
,PPE.[UpdateDT]
FROM [version].[ProPricerExport] PPE
INNER JOIN [version].Workspace WS ON PPE.WorkspaceID = WS.WorkspaceID
WHERE  
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND 
PPE.VersionID = @VersionID
SET IDENTITY_INSERT  [dbo].[ProPricerExport] OFF


END
IF EXISTS (SELECT 1 FROM [version].[ProPricerFieldXREF] WHERE VersionID = @VersionID)
BEGIN
SET IDENTITY_INSERT  [dbo].[ProPricerFieldXREF] ON
INSERT INTO [dbo].[ProPricerFieldXREF]
([PFID]
,[ProPricerExportID]
,[ProPricerFieldID]
,[ProPricerTypeID]
,[ListOrder]
)
SELECT PX.[PFID]
,PX.[ProPricerExportID]
,PX.[ProPricerFieldID]
,PX.[ProPricerTypeID]
,PX.[ListOrder]
FROM [version].[ProPricerFieldXREF] PX
INNER JOIN [version].[ProPricerExport] PPE ON PX.ProPricerExportID = PPE.ProPricerExportID
INNER JOIN [version].Workspace WS ON PPE.WorkspaceID = WS.WorkspaceID
WHERE  
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND 
PX.VersionID = @VersionID AND
PPE.VersionID = @VersionID
SET IDENTITY_INSERT  [dbo].[ProPricerFieldXREF] OFF
END
IF EXISTS (SELECT 1 FROM [version].[ProPricerCustomFieldXREF] WHERE VersionID = @VersionID)
BEGIN
SET IDENTITY_INSERT [dbo].[ProPricerCustomFieldXREF] ON
INSERT INTO [dbo].[ProPricerCustomFieldXREF]
([PCID]
,[ProPricerExportID]
,[CustomFieldID]
,[ProPricerTypeID]
,[ProPricerCustomFieldSelectionID]
,[ListOrder]
)
SELECT PX.[PCID]
,PX.[ProPricerExportID]
,PX.[CustomFieldID]
,PX.[ProPricerTypeID]
,PX.[ProPricerCustomFieldSelectionID]
,PX.[ListOrder]
FROM [version].[ProPricerCustomFieldXREF] PX
INNER JOIN [version].[ProPricerExport] PPE ON PX.ProPricerExportID = PPE.ProPricerExportID
INNER JOIN [version].Workspace WS ON PPE.WorkspaceID = WS.WorkspaceID
WHERE  
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND 
PX.VersionID = @VersionID AND
PPE.VersionID = @VersionID
SET IDENTITY_INSERT  [dbo].[ProPricerCustomFieldXREF] OFF
END
IF EXISTS (SELECT 1 FROM [version].[WBS_CLIN_BOE_XREF] WHERE VersionID = @VersionID)
BEGIN
SET IDENTITY_INSERT [dbo].[WBS_CLIN_BOE_XREF] ON
INSERT INTO [dbo].[WBS_CLIN_BOE_XREF]
([WCBID]
,[WBSID]
,[CLINID]
,[BOEID]
)
SELECT X.[WCBID], X.[WBSID], X.[CLINID], X.[BOEID]
FROM [version].[WBS_CLIN_BOE_XREF] X 
INNER JOIN [version].WorkBreakdownStructure WBS ON X.WBSID = WBS.WBSID
INNER JOIN [version].Workspace WS ON WBS.WorkspaceID = WS.WorkspaceID
WHERE  
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND
WBS.VersionID = @VersionID AND
X.VersionID = @VersionID
UNION
SELECT X.[WCBID], X.[WBSID], X.[CLINID], X.[BOEID]
FROM [version].[WBS_CLIN_BOE_XREF] X 
INNER JOIN [version].CLIN C ON X.CLINID = C.CLINID 
INNER JOIN [version].Workspace WS ON C.WorkspaceID = WS.WorkspaceID
WHERE  
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND
C.VersionID = @VersionID AND
X.VersionID = @VersionID
UNION
SELECT X.[WCBID], X.[WBSID], X.[CLINID], X.[BOEID]
FROM [version].[WBS_CLIN_BOE_XREF] X 
INNER JOIN [version].BOE B ON X.BOEID = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE  
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND
B.VersionID = @VersionID AND
X.VersionID = @VersionID
SET IDENTITY_INSERT [dbo].[WBS_CLIN_BOE_XREF] OFF
END
IF EXISTS (SELECT 1 FROM [version].[SumOfBOE_WorkspaceVariableXREF] WHERE VersionID = @VersionID)
BEGIN
SET IDENTITY_INSERT [dbo].[SumOfBOE_WorkspaceVariableXREF] ON
INSERT INTO [dbo].[SumOfBOE_WorkspaceVariableXREF]
([WVSumID]
,[WorkspaceVariableID]
,[CLINID]
,[WBSID]
,[BOEID]
)
SELECT X.[WVSumID]
,X.[WorkspaceVariableID]
,X.[CLINID]
,X.[WBSID]
,X.[BOEID]
FROM [version].[SumOfBOE_WorkspaceVariableXREF] X
INNER JOIN [version].[WorkspaceVariable] WSV ON X.WorkspaceVariableID = WSV.WorkspaceVariableID
INNER JOIN [version].Workspace WS ON WSV.WorkspaceID = WS.WorkspaceID
WHERE   
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND 
X.VersionID = @VersionID AND
WSV.VersionID = @VersionID
SET IDENTITY_INSERT [dbo].[SumOfBOE_WorkspaceVariableXREF] OFF
END
IF EXISTS (SELECT 1 FROM [version].[WorkspaceVariableSumVariableResourceTypeXREF] WHERE VersionID = @VersionID)
BEGIN
SET IDENTITY_INSERT [dbo].[WorkspaceVariableSumVariableResourceTypeXREF] ON
INSERT INTO [dbo].[WorkspaceVariableSumVariableResourceTypeXREF]
([WVSVRTID]
,[WorkspaceVariableID]
,[SumVariableResourceTypeID])
SELECT X.[WVSVRTID]
,X.[WorkspaceVariableID]
,X.[SumVariableResourceTypeID]
FROM [version].[WorkspaceVariableSumVariableResourceTypeXREF] X 
INNER JOIN [version].[WorkspaceVariable] WSV ON X.WorkspaceVariableID = WSV.WorkspaceVariableID
INNER JOIN [version].Workspace WS ON WSV.WorkspaceID = WS.WorkspaceID
WHERE   
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND 
X.VersionID = @VersionID AND
WSV.VersionID = @VersionID
SET IDENTITY_INSERT [dbo].[WorkspaceVariableSumVariableResourceTypeXREF] OFF
END
IF EXISTS (SELECT 1 FROM [version].[BOEStateHistory] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[BOEStateHistory] ON
INSERT INTO [dbo].[BOEStateHistory]
           ([BOEStateHistoryID]
           ,[BOEID]
           ,[FieldID]
           ,[CurrentBOEStateID]
           ,[UpdatedBOEStateID]
           ,[ChangedByETIUserID]
           ,[UpdateDT]
           )
SELECT BH.[BOEStateHistoryID]
      ,BH.[BOEID]
      ,BH.[FieldID]
      ,BH.[CurrentBOEStateID]
      ,BH.[UpdatedBOEStateID]
      ,BH.[ChangedByETIUserID]
      ,BH.[UpdateDT]
  FROM [version].[BOEStateHistory] BH
INNER JOIN [version].BOE B ON BH.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE  
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND 
BH.VersionID = @VersionID AND 
B.VersionID = @VersionID
SET IDENTITY_INSERT [dbo].[BOEStateHistory] OFF

END
IF EXISTS (SELECT 1 FROM [version].[BOEUserRoleHistory] WHERE VersionID = @VersionID)
BEGIN

/*WI 5160*/
SET IDENTITY_INSERT  [dbo].[BOEUserRoleHistory] ON
INSERT INTO [dbo].[BOEUserRoleHistory]
           ([BOEUserRoleHistoryID]
           ,[UpdateDT]
           ,[CurrentETIUserID]
           ,[UpdatedETIUserID]
           ,[RoleID]
           ,[BOEID]
           ,[FieldID]
           ,[ChangedByETIUserID])
SELECT BUR.[BOEUserRoleHistoryID]
      ,BUR.[UpdateDT]
      ,BUR.[CurrentETIUserID]
      ,BUR.[UpdatedETIUserID]
      ,BUR.[RoleID]
      ,BUR.[BOEID]
      ,BUR.[FieldID]
      ,BUR.[ChangedByETIUserID]
FROM [version].[BOEUserRoleHistory] BUR
INNER JOIN [version].BOE B ON BUR.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE  
WS.VersionID = @VersionID AND  
WS.WorkspaceID = @WorkspaceID AND 
BUR.VersionID = @VersionID AND 
B.VersionID = @VersionID
SET IDENTITY_INSERT  [dbo].[BOEUserRoleHistory] OFF

END
IF EXISTS (SELECT 1 FROM [version].[BOEUserRole] WHERE VersionID = @VersionID)
BEGIN
SET IDENTITY_INSERT  [dbo].[BOEUserRole] ON
INSERT INTO [dbo].[BOEUserRole]
([BOEUserRoleID]
,[ETIUserID]
,[RoleID]
,[BOEID]
,[UpdateDT]
)
SELECT BUR.[BOEUserRoleID]
,BUR.[ETIUserID]
,BUR.[RoleID]
,BUR.[BOEID]
,BUR.[UpdateDT]
FROM [version].[BOEUserRole] BUR
INNER JOIN [version].BOE B ON BUR.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE  
WS.VersionID = @VersionID AND  
WS.WorkspaceID = @WorkspaceID AND 
BUR.VersionID = @VersionID AND 
B.VersionID = @VersionID
SET IDENTITY_INSERT  [dbo].[BOEUserRole] OFF

END
IF EXISTS (SELECT 1 FROM [version].[BOEApproval] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT  [dbo].[BOEApproval] ON
INSERT INTO [dbo].[BOEApproval]
([BOEApprovalID]
,[BOEID]
,[ApprovalETIUserID]
,[ApprovedFlag]
,[UpdateDT]
)
SELECT BA.[BOEApprovalID]
,BA.[BOEID]
,BA.[ApprovalETIUserID]
,NULL /*WI 9601 Approval Flag Set To NULL In Restore BA.[ApprovedFlag]*/
,BA.[UpdateDT]
FROM [version].[BOEApproval] BA
INNER JOIN [version].BOE B ON BA.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE  
WS.VersionID = @VersionID AND  
WS.WorkspaceID = @WorkspaceID AND 
BA.VersionID = @VersionID AND 
B.VersionID = @VersionID
SET IDENTITY_INSERT  [dbo].[BOEApproval] OFF

END
IF EXISTS (SELECT 1 FROM [version].[BOEComment] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[BOEComment] ON
INSERT INTO [dbo].[BOEComment]
([BOECommentID]
,[FieldID]
,[BOEComments]
,[BOECommentETIUserID]
,[BOEResponseToCommentID]
,[BOEID]
,[UpdateDT]
)
SELECT BC.[BOECommentID]
,BC.[FieldID]
,BC.[BOEComments]
,BC.[BOECommentETIUserID]
,BC.[BOEResponseToCommentID]
,BC.[BOEID]
,BC.[UpdateDT]
FROM [version].[BOEComment] BC
INNER JOIN [version].BOE B ON BC.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE  
WS.VersionID = @VersionID AND  
WS.WorkspaceID = @WorkspaceID AND 
BC.VersionID = @VersionID AND 
B.VersionID = @VersionID
SET IDENTITY_INSERT [dbo].[BOEComment] OFF

END
IF EXISTS (SELECT 1 FROM [version].[BOECommentHistory] WHERE VersionID = @VersionID)
BEGIN
SET IDENTITY_INSERT [dbo].[BOECommentHistory] ON
INSERT INTO [dbo].[BOECommentHistory]
([BOECommentHistoryID]
,[BOECommentID]
,[BOEID]
,[FieldID]
,[CurrentComment]
,[UpdatedComment]
,[ChangedByETIUserID]
,[UpdateDT]
)
SELECT BCH.[BOECommentHistoryID]
,BCH.[BOECommentID]
,BCH.[BOEID]
,BCH.[FieldID]
,BCH.[CurrentComment]
,BCH.[UpdatedComment]
,BCH.[ChangedByETIUserID]
,BCH.[UpdateDT]
FROM [version].[BOECommentHistory] BCH
INNER JOIN [version].[BOEComment] BC ON BCH.BOECommentID = BC.BOECommentID
INNER JOIN [version].BOE B ON BCH.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE   
WS.VersionID = @VersionID AND WS.WorkspaceID = @WorkspaceID AND 
BCH.VersionID = @VersionID AND 
BC.VersionID = @VersionID AND
B.VersionID = @VersionID
SET IDENTITY_INSERT  [dbo].[BOECommentHistory] OFF

END
IF EXISTS (SELECT 1 FROM [version].[CustomFieldValue] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[CustomFieldValue] ON
INSERT INTO [dbo].[CustomFieldValue]
([CustomFieldValueID]
,[CustomFieldValueName]
,[CustomFieldValueDescription]
,[CustomFieldID]
,[CustomFieldValueInUseFlag]
,[UpdateDT]
)
SELECT CFV.[CustomFieldValueID]
,CFV.[CustomFieldValueName]
,CFV.[CustomFieldValueDescription]
,CFV.[CustomFieldID]
,CFV.[CustomFieldValueInUseFlag]
,CFV.[UpdateDT]
FROM [version].[CustomFieldValue] CFV
INNER JOIN [version].CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
INNER JOIN [version].Workspace WS ON CF.WorkspaceID = WS.WorkspaceID
WHERE   
WS.VersionID = @VersionID AND WS.WorkspaceID = @WorkspaceID AND 
CFV.VersionID = @VersionID AND
CF.VersionID = @VersionID
SET IDENTITY_INSERT  [dbo].[CustomFieldValue] OFF

END


-- Start of "RMS Zone Travel"
IF EXISTS (SELECT 1 FROM [version].[MSTTravelTrip] WHERE VersionID = @VersionID)
BEGIN
	SET IDENTITY_INSERT [dbo].[MSTTravelTrip] ON

	INSERT INTO [dbo].[MSTTravelTrip]
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
		FROM [version].[MSTTravelTrip] tt 
			INNER JOIN [version].TravelTripTaskElement TE ON tt.TravelTripTaskElementID = TE.TravelTripTaskElementID
			INNER JOIN [version].BOE B ON TE.BOEID = B.BOEID
			INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE
			tt.VersionID = @VersionID AND
			TE.VersionID = @VersionID AND
			B.VersionID = @VersionID AND
			WS.VersionID = @VersionID AND
			WS.WorkspaceID = @WorkspaceID

	SET IDENTITY_INSERT dbo.[MSTTravelTrip] OFF
END

IF EXISTS (SELECT 1 FROM [version].[MSTTravelTripCustomFieldValueXREF] WHERE VersionID = @VersionID)
BEGIN
	SET IDENTITY_INSERT [dbo].[MSTTravelTripCustomFieldValueXREF] ON

		INSERT INTO [dbo].[MSTTravelTripCustomFieldValueXREF]
			([MSTTCFVID]
			,[MSTTravelTripID]
			,[MSTCustomFieldValueID]
			,[UpdateDT]
			)
		SELECT 
			cI.[MSTTCFVID]
			,cI.[MSTTravelTripID]
			,cI.[MSTCustomFieldValueID]
			,cI.[UpdateDT]
		FROM [version].[MSTTravelTripCustomFieldValueXREF] cI 
			INNER JOIN [version].MSTTravelTrip tt ON cI.MSTTravelTripID = tt.MSTTravelTripID
			INNER JOIN [version].TravelTripTaskElement TE ON tt.TravelTripTaskElementID = TE.TravelTripTaskElementID
			INNER JOIN [version].BOE B ON TE.BOEID = B.BOEID
			INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE
			cI.VersionID = @VersionID AND
			tt.VersionID = @VersionID AND
			TE.VersionID = @VersionID AND
			B.VersionID = @VersionID AND
			WS.VersionID = @VersionID AND
			WS.WorkspaceID = @WorkspaceID

	SET IDENTITY_INSERT dbo.[MSTTravelTripCustomFieldValueXREF] OFF
END
-- End of "RMS Zone Travel"

IF EXISTS (SELECT 1 FROM [version].[BOETaskElement] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT  [dbo].[BOETaskElement] ON
INSERT INTO [dbo].[BOETaskElement]
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
,BTE.[SortOrderID]
FROM [version].[BOETaskElement] BTE
INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE   
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND 
BTE.VersionID = @VersionID AND 
B.VersionID = @VersionID

SET IDENTITY_INSERT  [dbo].[BOETaskElement] OFF

END
IF EXISTS (SELECT 1 FROM [version].[OrdinaryVariable] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[OrdinaryVariable] ON
INSERT INTO [dbo].[OrdinaryVariable]
([OrdinaryVariableID]
,[OrdinaryVariableName]
,[OrdinaryVariableValue]
,[BOETaskElementID]
,[SortByID]
,[ValueTypeID]
,[IsPercentage]
,[UpdateDT]
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
,OV.[DefaultSize]
FROM [version].[OrdinaryVariable] OV
INNER JOIN [version].[BOETaskElement] BTE ON OV.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE   
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND 
OV.VersionID = @VersionID AND 
B.VersionID = @VersionID AND
BTE.VersionID = @VersionID
SET IDENTITY_INSERT  [dbo].[OrdinaryVariable] OFF

END
IF EXISTS (SELECT 1 FROM [version].[BOEApprovalHistory] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[BOEApprovalHistory] ON
INSERT INTO [dbo].[BOEApprovalHistory]
([BOEApprovalHistoryID]
,[BOEID]
,[Approval]
,[ApprovalETIUserID]
,[UpdateDT]
)
SELECT BH.[BOEApprovalHistoryID]
,BH.[BOEID]
,BH.[Approval]
,BH.[ApprovalETIUserID]
,BH.[UpdateDT]
FROM [version].[BOEApprovalHistory] BH
INNER JOIN [version].BOE B ON BH.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE   
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND 
BH.VersionID = @VersionID AND 
		B.VersionID = @VersionID
SET IDENTITY_INSERT [dbo].[BOEApprovalHistory] OFF

END
IF EXISTS   (SELECT vR.ResourceID 
					FROM [version].[Resource] vR
						INNER JOIN [version].[BOELaborType] BLT ON vR.ResourceID = BLT.ResourceID
						INNER JOIN [version].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
						INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
						INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
						LEFT OUTER JOIN dbo.[Resource] R  ON vR.ResourceID = R.ResourceID
					WHERE 
						vR.VersionID = @VersionID AND
						BLT.VersionID = @VersionID AND
						BTE.VersionID = @VersionID AND
						B.VersionID = @VersionID AND 
						WS.VersionID = @VersionID AND 
						WS.WorkspaceID = @WorkspaceID AND
						R.ResourceID IS NULL
				) --IS NOT NULL
		BEGIN

			SELECT DISTINCT	@ErrorMessage = 
				IsNull(@ErrorMessage,'WARNING: ') + 
				'ResourceID: ' + 
				CAST (vR.ResourceID AS varchar(10)) + 
				' Resource Name: ' + 
				vR.ResourceName + 
				' no longer exists.' + 
				CHAR(13) 
			FROM [version].[Resource] vR
						INNER JOIN [version].[BOELaborType] BLT ON vR.ResourceID = BLT.ResourceID
						INNER JOIN [version].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
						INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
						INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
						LEFT OUTER JOIN dbo.[Resource] R  ON vR.ResourceID = R.ResourceID
					WHERE 
						vR.VersionID = @VersionID AND
						BLT.VersionID = @VersionID AND
						BTE.VersionID = @VersionID AND
						B.VersionID = @VersionID AND 
						WS.VersionID = @VersionID AND 
						WS.WorkspaceID = @WorkspaceID AND
						R.ResourceID IS NULL

		END
IF  EXISTS  (SELECT vPO.PerformingOrganizationID 
					FROM [version].[PerformingOrganization] vPO
						INNER JOIN [version].[BOELaborType] BLT ON vPO.PerformingOrganizationID = BLT.PerformingOrganizationID
						INNER JOIN [version].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
						INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
						INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
						LEFT OUTER JOIN dbo.[PerformingOrganization] PO  ON PO.PerformingOrganizationID = vPO.PerformingOrganizationID
					WHERE 
						vPO.VersionID = @VersionID AND
						BLT.VersionID = @VersionID AND
						BTE.VersionID = @VersionID AND
						B.VersionID = @VersionID AND 
						WS.VersionID = @VersionID AND 
						WS.WorkspaceID = @WorkspaceID AND
						PO.PerformingOrganizationID IS NULL
				)-- IS NOT NULL
		BEGIN

			SELECT	DISTINCT	@ErrorMessage = 
				IsNull(@ErrorMessage,'WARNING: ') + 
				'PerformingOrganizationID: ' + 
				CAST (vPO.PerformingOrganizationID AS varchar(10)) + 
				' Performing Organization Name: ' + 
				vPO.PerformingOrganizationName + 
				' no longer exists.' + 
				CHAR(13)
					FROM [version].[PerformingOrganization] vPO
						INNER JOIN [version].[BOELaborType] BLT ON vPO.PerformingOrganizationID = BLT.PerformingOrganizationID
						INNER JOIN [version].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
						INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
						INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
						LEFT OUTER JOIN dbo.[PerformingOrganization] PO  ON PO.PerformingOrganizationID = vPO.PerformingOrganizationID
					WHERE 
						vPO.VersionID = @VersionID AND
						BLT.VersionID = @VersionID AND
						BTE.VersionID = @VersionID AND
						B.VersionID = @VersionID AND 
						WS.VersionID = @VersionID AND 
						WS.WorkspaceID = @WorkspaceID AND
						PO.PerformingOrganizationID IS NULL
		END

/** RTE Templates **/ 

IF EXISTS (SELECT 1 FROM [version].[RteTemplate] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[RteTemplate] ON
INSERT INTO [dbo].[RteTemplate]
([TemplateID],
[UpdateDT],
[WorkspaceID],
[Description],
[AuthorID],
[CreatedOn]
)
SELECT RT.[TemplateID]
,RT.[UpdateDT]
,RT.[WorkspaceID]
,RT.[Description]
,RT.[AuthorID]
,RT.[CreatedOn]
FROM [version].[RteTemplate] RT
WHERE 
RT.VersionID = @VersionID AND
RT.WorkspaceID = @WorkspaceID 
SET IDENTITY_INSERT [dbo].[RteTemplate] OFF

END

IF EXISTS (SELECT 1 FROM [version].[RteTemplateAssigned] WHERE VersionID = @VersionID)
BEGIN

INSERT INTO [dbo].[RteTemplateAssigned]
([TemplateID],
[RteTemplateSourceId]
)
SELECT RTA.[TemplateID]
,RTA.[RteTemplateSourceId]
FROM [version].[RteTemplateAssigned] RTA
INNER JOIN [version].[RteTemplate] RT ON RT.[TemplateID] = RTA.[TemplateID]
WHERE   
RT.VersionID = @VersionID AND RT.WorkspaceID = @WorkspaceID AND 
RTA.VersionID = @VersionID

END

IF EXISTS (SELECT 1 FROM [version].[RteTemplateQuestion] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[RteTemplateQuestion] ON
INSERT INTO [dbo].[RteTemplateQuestion]
([QuestionID],
[UpdateDT],
[TemplateID],
[Text],
[SortOrder],
[Required]
)
SELECT RTQ.[QuestionID]
,RTQ.[UpdateDT]
,RTQ.[TemplateID]
,RTQ.[Text]
,RTQ.[SortOrder]
,RTQ.[Required]
FROM [version].[RteTemplateQuestion] RTQ
INNER JOIN [version].[RteTemplate] RT ON RT.[TemplateID] = RTQ.[TemplateID]
WHERE   
RT.VersionID = @VersionID AND RT.WorkspaceID = @WorkspaceID AND 
RTQ.VersionID = @VersionID
SET IDENTITY_INSERT  [dbo].[RteTemplateQuestion] OFF

END

IF EXISTS (SELECT 1 FROM [version].[RteTemplateAnswer] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[RteTemplateAnswer] ON
INSERT INTO [dbo].[RteTemplateAnswer]
([AnswerID],
[UpdateDT],
[QuestionID],
[BOEID],
[TaskID],
[Text],
[RteTemplateSourceId]
)
SELECT RTA.[AnswerID]
,RTA.[UpdateDT]
,RTA.[QuestionID]
,RTA.[BOEID]
,RTA.[TaskID]
,RTA.[Text]
,RTA.[RteTemplateSourceId]
FROM [version].[RteTemplateAnswer] RTA
INNER JOIN [version].[RteTemplateQuestion] RTQ ON RTA.[QuestionID] = RTQ.[QuestionID]
INNER JOIN [version].[RteTemplate] RT ON RT.[TemplateID] = RTQ.[TemplateID]
WHERE   
RT.VersionID = @VersionID AND RT.WorkspaceID = @WorkspaceID AND 
RTQ.VersionID = @VersionID AND RTA.VersionID = @VersionID
SET IDENTITY_INSERT  [dbo].[RteTemplateAnswer] OFF

END

/** End RTE Templates **/ 

/** Begin MOQ Types **/

IF EXISTS (SELECT 1 FROM [version].[MOQTypeSelection] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[MOQTypeSelection] ON
INSERT INTO [dbo].[MOQTypeSelection]
([MOQTypeSelectionId],
[TaskId],
[MOQTypeSelection],
[UpdateDT],
[Order],
[CERName],
[CERLocation],
[HoursDescription],
[SubjectMatterExpert],
[HoursLogicAndAssumptions],
[DurationLogicAndAssumptions],
[EstimateTasks],
[Rationale],
[SkillMix]
)
SELECT M.[MOQTypeSelectionId],
	M.[TaskId],
	M.[MOQTypeSelection],
	M.[UpdateDT],
	M.[Order],
	M.[CERName],
	M.[CERLocation],
	M.[HoursDescription],
	M.[SubjectMatterExpert],
	M.[HoursLogicAndAssumptions],
	M.[DurationLogicAndAssumptions],
	M.[EstimateTasks],
	M.[Rationale],
	M.[SkillMix]
FROM [version].[MOQTypeSelection] M
INNER JOIN [version].[BOETaskElement] T on M.TaskId = T.BOETaskElementID
INNER JOIN [version].[BOE] B ON T.BOEID  = B.BOEID
INNER JOIN [version].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
WHERE 
M.VersionId = @VersionID AND
T.VersionID = @VersionID AND
B.VersionID = @VersionID AND
WS.VersionID = @VersionID AND
WS.WorkspaceID = @WorkspaceID

SET IDENTITY_INSERT [dbo].[MOQTypeSelection] OFF

END
IF EXISTS (SELECT 1 FROM [version].[MOQTypeSelectionTableData] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[MOQTypeSelectionTableData] ON
INSERT INTO [dbo].[MOQTypeSelectionTableData]
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
[TotalRelevantHoursAfterQueryFilters]
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
TD.[TotalRelevantHoursAfterQueryFilters]
FROM [version].[MOQTypeSelectionTableData] TD
INNER JOIN [version].[MOQTypeSelection] M ON TD.[MOQTypeSelectionId] = M.[MOQTypeSelectionId]
INNER JOIN [version].[BOETaskElement] T on M.TaskId = T.BOETaskElementID
INNER JOIN [version].[BOE] B ON T.BOEID  = B.BOEID
INNER JOIN [version].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
WHERE 
TD.VersionId = @VersionID AND
M.VersionId = @VersionID AND
T.VersionID = @VersionID AND
B.VersionID = @VersionID AND
WS.VersionID = @VersionID AND
WS.WorkspaceID = @WorkspaceID

SET IDENTITY_INSERT [dbo].[MOQTypeSelectionTableData] OFF

END

/** End MOQ Types **/

IF EXISTS (SELECT 1 FROM [version].[BOELaborType] WHERE VersionID = @VersionID)
BEGIN
/*Updated for WI 8398*/
SET IDENTITY_INSERT [dbo].[BOELaborType] ON
INSERT INTO [dbo].[BOELaborType]
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
,[PercentSpreadLocked]
,[HourSpreadLocked]
,[WBSID]
,[CLINID]
,[CanOffload]
,[LaborSortID]
,[MOQTypeSelectionId]
)
SELECT BLT.[BOELaborTypeID]
,R.[ResourceID]
,PO.[PerformingOrganizationID]
,BLT.[BOELaborTypeStartDate]
,BLT.[BOELaborTypeEndDate]
,BLT.[SpreadCurveID]
,BLT.[PercentSpread]
,BLT.[ValueSpread]
,BLT.[BOETaskElementID]
,BLT.[SpreadTypeID]
,BLT.[UpdateDT]
,BLT.[PercentSpreadLocked]
,BLT.[HourSpreadLocked]
,BLT.[WBSID]
,BLT.[CLINID]
,BLT.[CanOffload]
,BLT.[LaborSortID]
,BLT.[MOQTypeSelectionId]
FROM [version].[BOELaborType] BLT
INNER JOIN [version].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
LEFT OUTER JOIN [dbo].[Resource] R ON BLT.ResourceID = R.ResourceID
LEFT OUTER JOIN [dbo].[PerformingOrganization] PO ON BLT.PerformingOrganizationID = PO.PerformingOrganizationID
WHERE 
BLT.VersionID = @VersionID AND
BTE.VersionID = @VersionID AND
B.VersionID = @VersionID AND 
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID


SET IDENTITY_INSERT [dbo].[BOELaborType] OFF


END
IF EXISTS (SELECT 1 FROM [version].[BOETaskElementWorkspaceVariableXREF] WHERE VersionID = @VersionID)
BEGIN
SET IDENTITY_INSERT [dbo].[BOETaskElementWorkspaceVariableXREF] ON
INSERT INTO [dbo].[BOETaskElementWorkspaceVariableXREF]
([BOETaskWSVarID]
,[BOETaskElementID]
,[WorkspaceVariableID]
)
SELECT X.[BOETaskWSVarID]
,X.[BOETaskElementID]
,X.[WorkspaceVariableID]
FROM [version].[BOETaskElementWorkspaceVariableXREF] X
INNER JOIN [version].[BOETaskElement] BTE ON X.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE   
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND 
X.VersionID = @VersionID AND 
		B.VersionID = @VersionID AND 
		BTE.VersionID = @VersionID
SET IDENTITY_INSERT [dbo].[BOETaskElementWorkspaceVariableXREF] OFF
END
IF EXISTS (SELECT 1 FROM [version].[BOETaskElementMetricDetailXREF] WHERE VersionID = @VersionID)
BEGIN
SET IDENTITY_INSERT [dbo].[BOETaskElementMetricDetailXREF] ON
INSERT INTO [dbo].[BOETaskElementMetricDetailXREF]
([BTEMDID]
,[BOETaskElementID]
,[MetricDetailID]
,[UpdateDT]
)
SELECT X.[BTEMDID]
,X.[BOETaskElementID]
,X.[MetricDetailID]
,X.[UpdateDT]
FROM [version].[BOETaskElementMetricDetailXREF] X
INNER JOIN [version].[BOETaskElement] BTE ON X.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE   WS.VersionID = @VersionID AND WS.WorkspaceID = @WorkspaceID AND 
		X.VersionID = @VersionID AND
		BTE.VersionID = @VersionID AND
		B.VersionID = @VersionID 

SET IDENTITY_INSERT [dbo].[BOETaskElementMetricDetailXREF] OFF


END
IF EXISTS (SELECT 1 FROM [version].[BOETaskElementCustomFieldValueXREF] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[BOETaskElementCustomFieldValueXREF] ON
INSERT INTO [dbo].[BOETaskElementCustomFieldValueXREF]
([BTECFVID]
,[BOETaskElementID]
,[CustomFieldValueID]
,[UpdateDT]
)
SELECT X.[BTECFVID]
,X.[BOETaskElementID]
,X.[CustomFieldValueID]
,X.[UpdateDT]
FROM [version].[BOETaskElementCustomFieldValueXREF] X
INNER JOIN [version].[BOETaskElement] BTE ON X.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE  
WS.VersionID = @VersionID AND  WS.WorkspaceID = @WorkspaceID AND 
X.VersionID = @VersionID AND
BTE.VersionID = @VersionID AND
B.VersionID = @VersionID
SET IDENTITY_INSERT [dbo].[BOETaskElementCustomFieldValueXREF] OFF

END
IF EXISTS (SELECT 1 FROM [version].[BOECustomFieldValueXREF] WHERE VersionID = @VersionID)
BEGIN
SET IDENTITY_INSERT [dbo].[BOECustomFieldValueXREF] ON
INSERT INTO [dbo].[BOECustomFieldValueXREF]
([BCFVID]
,[BOEID]
,[CustomFieldValueID]
,[UpdateDT]
)
SELECT X.[BCFVID]
,X.[BOEID]
,X.[CustomFieldValueID]
,X.[UpdateDT]
FROM [version].[BOECustomFieldValueXREF] X
INNER JOIN [version].BOE B ON X.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE   
WS.VersionID = @VersionID AND WS.WorkspaceID = @WorkspaceID AND 
X.VersionID = @VersionID AND 
B.VersionID = @VersionID
SET IDENTITY_INSERT [dbo].[BOECustomFieldValueXREF] OFF

END
IF EXISTS (SELECT 1 FROM [version].[TravelTripTaskElementCustomFieldValueXREF] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[TravelTripTaskElementCustomFieldValueXREF] ON
INSERT INTO [dbo].[TravelTripTaskElementCustomFieldValueXREF]
([TTECFVID]
,[TravelTripTaskElementID]
,[CustomFieldValueID]
,[UpdateDT]
)
SELECT X.[TTECFVID]
,X.[TravelTripTaskElementID]
,X.[CustomFieldValueID]
,X.[UpdateDT]
FROM [version].[TravelTripTaskElementCustomFieldValueXREF] X
INNER JOIN [version].[TravelTripTaskElement] TE ON X.TravelTripTaskElementID = TE.TravelTripTaskElementID
INNER JOIN [version].BOE B ON TE.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE  
WS.VersionID = @VersionID AND  WS.WorkspaceID = @WorkspaceID AND 
X.VersionID = @VersionID AND
TE.VersionID = @VersionID AND
B.VersionID = @VersionID
SET IDENTITY_INSERT [dbo].[TravelTripTaskElementCustomFieldValueXREF] OFF

END
IF EXISTS (SELECT 1 FROM [version].[TravelTripCustomFieldValueXREF] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[TravelTripCustomFieldValueXREF] ON
INSERT INTO [dbo].[TravelTripCustomFieldValueXREF]
([TCFVID]
,[TravelTripID]
,[CustomFieldValueID]
,[UpdateDT]
)
SELECT X.[TCFVID]
,X.[TravelTripID]
,X.[CustomFieldValueID]
,X.[UpdateDT]
FROM [version].[TravelTripCustomFieldValueXREF] X
INNER JOIN [version].TravelTrip T ON X.TravelTripID  = T.TravelTripID
INNER JOIN [version].[TravelTripTaskElement] TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
INNER JOIN [version].BOE B ON TE.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE  
WS.VersionID = @VersionID AND  WS.WorkspaceID = @WorkspaceID AND 
X.VersionID = @VersionID AND
T.VersionID = @VersionID AND
TE.VersionID = @VersionID AND
B.VersionID = @VersionID

SET IDENTITY_INSERT [dbo].[TravelTripCustomFieldValueXREF] OFF


END
IF EXISTS (SELECT 1 FROM [version].[SumOfBOE_OrdinaryVariableXREF] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT  [dbo].[SumOfBOE_OrdinaryVariableXREF] ON
INSERT INTO [dbo].[SumOfBOE_OrdinaryVariableXREF]
([OVSumID]
,[OrdinaryVariableID]
,[CLINID]
,[WBSID]
,[BOEID]
)
SELECT X.[OVSumID]
,X.[OrdinaryVariableID]
,X.[CLINID]
,X.[WBSID]
,X.[BOEID]
FROM [version].[SumOfBOE_OrdinaryVariableXREF] X
INNER JOIN [version].[OrdinaryVariable] OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
INNER JOIN [version].[BOETaskElement] BTE ON OV.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE   
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND 
X.VersionID = @VersionID AND
OV.VersionID = @VersionID AND
BTE.VersionID = @VersionID AND
B.VersionID = @VersionID
SET IDENTITY_INSERT [dbo].[SumOfBOE_OrdinaryVariableXREF] OFF

END
IF EXISTS (SELECT 1 FROM [version].[OrdinaryVariableSumVariableResourceTypeXREF] WHERE VersionID = @VersionID)
BEGIN
SET IDENTITY_INSERT  [dbo].[OrdinaryVariableSumVariableResourceTypeXREF] ON
INSERT INTO [dbo].[OrdinaryVariableSumVariableResourceTypeXREF]
([OVSVRTID]
,[OrdinaryVariableID]
,[SumVariableResourceTypeID])
SELECT X.[OVSVRTID]
,X.[OrdinaryVariableID]
,X.[SumVariableResourceTypeID]
FROM [version].[OrdinaryVariableSumVariableResourceTypeXREF] X
INNER JOIN [version].[OrdinaryVariable] OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
INNER JOIN [version].[BOETaskElement] BTE ON OV.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE   
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND 
X.VersionID = @VersionID AND
OV.VersionID = @VersionID AND
BTE.VersionID = @VersionID AND
B.VersionID = @VersionID
SET IDENTITY_INSERT [dbo].[OrdinaryVariableSumVariableResourceTypeXREF] OFF

END
IF EXISTS (SELECT 1 FROM [version].[BOELaborTypeCustomFieldValueXREF] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT  [dbo].[BOELaborTypeCustomFieldValueXREF] ON
INSERT INTO [dbo].[BOELaborTypeCustomFieldValueXREF]
([BLTCFVID]
,[BOELaborTypeID]
,[CustomFieldValueID]
,[UpdateDT]
)
SELECT X.[BLTCFVID]
,X.[BOELaborTypeID]
,X.[CustomFieldValueID]
,X.[UpdateDT]
FROM [version].[BOELaborTypeCustomFieldValueXREF] X 
INNER JOIN [version].[BOELaborType] BLT ON X.BOELaborTypeID = BLT.BOELaborTypeID
INNER JOIN [version].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE  
WS.VersionID = @VersionID AND  
WS.WorkspaceID = @WorkspaceID AND 
X.VersionID = @VersionID AND
BLT.VersionID = @VersionID AND
BTE.VersionID = @VersionID AND
B.VersionID = @VersionID
SET IDENTITY_INSERT  [dbo].[BOELaborTypeCustomFieldValueXREF] OFF

END
IF EXISTS (SELECT 1 FROM [version].[BOELaborSpread] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[BOELaborSpread] ON
INSERT INTO [dbo].[BOELaborSpread]
([BOELaborSpreadID]
,[BOELaborTypeID]
,[LaborSpreadDate]
,[LaborSpreadValue]
)
SELECT LS.[BOELaborSpreadID]
,LS.[BOELaborTypeID]
,LS.[LaborSpreadDate]
,LS.[LaborSpreadValue]
FROM [version].[BOELaborSpread] LS
INNER JOIN [version].[BOELaborType] BLT ON LS.BOELaborTypeID = BLT.BOELaborTypeID
INNER JOIN [version].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE   
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND 
LS.VersionID = @VersionID AND
BLT.VersionID = @VersionID AND
BTE.VersionID = @VersionID AND
B.VersionID = @VersionID

SET IDENTITY_INSERT [dbo].[BOELaborSpread] OFF


END

/*  New INL Forms */
IF EXISTS (SELECT 1 FROM [version].[BOEFormIBOE] WHERE VersionID = @VersionID)
BEGIN
	
	SET IDENTITY_INSERT [dbo].[BOEFormIBOE] ON
	INSERT INTO [dbo].[BOEFormIBOE]
	([IBOEFormID], UpdateDT, WorkspaceID, FormName, Description, [BasisAndRationale], [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, BusinessArea, Revision,FormVersion)
	SELECT [IBOEFormID], UpdateDT, WorkspaceID, FormName, Description, [BasisAndRationale], [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, BusinessArea, Revision,FormVersion
	FROM [version].[BOEFormIBOE]
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	SET IDENTITY_INSERT [dbo].[BOEFormIBOE] OFF

	INSERT INTO [dbo].[BOEFormIBOEResourcesXREF]
	SELECT x.[IBOEFormID], R.ResourceID
	FROM [version].[BOEFormIBOEResourcesXREF] x
		INNER JOIN [dbo].[BOEFormIBOE] BF ON BF.[IBOEFormID] = x.[IBOEFormID]
		LEFT OUTER JOIN [dbo].[Resource] R ON x.ResourceID = R.ResourceID
	WHERE 
		x.VersionID = @VersionID AND
		BF.WorkspaceID = @WorkspaceID

	INSERT INTO [dbo].[BOEFormIBOECLINsXREF]
	SELECT x.[IBOEFormID], x.[ClinID], x.[ContractType]
	FROM [version].[BOEFormIBOECLINsXREF] x
		INNER JOIN [dbo].[BOEFormIBOE] BF ON BF.[IBOEFormID] = x.[IBOEFormID]
	WHERE 
		x.VersionID = @VersionID AND
		BF.WorkspaceID = @WorkspaceID

END

IF EXISTS (SELECT 1 FROM [version].[BOEFormPBOE] WHERE VersionID = @VersionID)
BEGIN
	
	SET IDENTITY_INSERT [dbo].[BOEFormPBOE] ON
	INSERT INTO [dbo].[BOEFormPBOE]
	([PBOEFormID], UpdateDT, WorkspaceID, FormName, Description, [BasisAndRationale], [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, Revision,FormVersion,  
	[DegreeOfCompetition],[CCoPD],[CCoPDOtherText],[RFP],[ProposalNumber],[SupplierName],[ValidityDate],[SupplierProposalSupportingDataIncluded],[PriceAnalysisIncluded],[CommercialItemDocIncluded],
	[CostAnalysisIncluded],[ShouldCostEstimate],[ShouldCostEstimateDate],[SowWritten],[SowWrittenDate],[RFPRelease],[RFPReleaseDate],[FirmSupplierReceipt],[FirmSupplierReceiptDate],
	[SourceSelection],[SourceSelectionDate],[CID],[CIDDate],[GovtReview],[GovtReviewDate],[PriceAnalysis],[PriceAnalysisDate],[TechnicalEvaluation],[TechnicalEvaluationDate],[FactFinding],
	[FactFindingDate],[CostAnalysis],[CostAnalysisDate],[GovtPricing],[GovtPricingDate],[SupplierNegotiations],[SupplierNegotiationsDate],[MOU],[MOUDate],[Procurement],[ProcurementDate],
	[PlannedDate_WrittenApproval],[PlannedDate_ApprovedSubmission])
	SELECT [PBOEFormID], UpdateDT, WorkspaceID, FormName, Description, [BasisAndRationale], [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, Revision,FormVersion, 
		[DegreeOfCompetition],[CCoPD],[CCoPDOtherText],[RFP],[ProposalNumber],[SupplierName],[ValidityDate],[SupplierProposalSupportingDataIncluded],[PriceAnalysisIncluded],
		[CommercialItemDocIncluded],[CostAnalysisIncluded],[ShouldCostEstimate],[ShouldCostEstimateDate],[SowWritten],[SowWrittenDate],[RFPRelease],[RFPReleaseDate],[FirmSupplierReceipt],
		[FirmSupplierReceiptDate],[SourceSelection],[SourceSelectionDate],[CID],[CIDDate],[GovtReview],[GovtReviewDate],[PriceAnalysis],[PriceAnalysisDate],[TechnicalEvaluation],
		[TechnicalEvaluationDate],[FactFinding],[FactFindingDate],[CostAnalysis],[CostAnalysisDate],[GovtPricing],[GovtPricingDate],[SupplierNegotiations],[SupplierNegotiationsDate],
		[MOU],[MOUDate],[Procurement],[ProcurementDate],[PlannedDate_WrittenApproval],[PlannedDate_ApprovedSubmission]
	  FROM [version].[BOEFormPBOE]
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	SET IDENTITY_INSERT [dbo].[BOEFormPBOE] OFF

	INSERT INTO [dbo].[BOEFormPBOEResourcesXREF]
	SELECT x.[PBOEFormID], R.ResourceID
	FROM [version].[BOEFormPBOEResourcesXREF] x
		INNER JOIN [dbo].[BOEFormPBOE] BF ON BF.[PBOEFormID] = x.[PBOEFormID]
		LEFT OUTER JOIN [dbo].[Resource] R ON x.ResourceID = R.ResourceID
	WHERE 
		x.VersionID = @VersionID AND
		BF.WorkspaceID = @WorkspaceID

	INSERT INTO [dbo].[BOEFormPBOECLINsXREF]
	SELECT x.[PBOEFormID], x.[ClinID], x.[ContractType]
	FROM [version].[BOEFormPBOECLINsXREF] x
		INNER JOIN [dbo].[BOEFormPBOE] BF ON BF.[PBOEFormID] = x.[PBOEFormID]
	WHERE 
		x.VersionID = @VersionID AND
		BF.WorkspaceID = @WorkspaceID

END

INSERT INTO [dbo].[WorkspaceRMSTravelNonzoneFeesAndCosts] ([UpdateDT], [WorkspaceID], ModeID, TravelAgencyFee, MiscOther)
	SELECT [UpdateDT], [WorkspaceID], ModeID, TravelAgencyFee, MiscOther
	FROM [version].[WorkspaceRMSTravelNonzoneFeesAndCosts] WHERE VersionID = @VersionID

INSERT INTO [dbo].[WorkspaceRMSTravelEscalationRate] ([UpdateDT], [WorkspaceID], [Year], [Escalation], [PerDiemRate], [MiscRate])
	SELECT [UpdateDT], [WorkspaceID], [Year], [Escalation], [PerDiemRate], [MiscRate]
	FROM [version].[WorkspaceRMSTravelEscalationRate] WHERE VersionID = @VersionID

/*BOEJ-473 - Locked workspace rates should be deleted when restoring a workspace. When
a workspace is restored it is set to either Initialization or Working. Workspaces in either
of those states should never have their rates locked. The inserts into the 5 locked rate tables
will be commented out:
	WorkspaceLockedPerDiem
	WorkspaceLockedTravelEscalationRate
	WorkspaceLockedTravelMiscRate
	WorkspaceLockedTrip
*/
/*

IF EXISTS (SELECT 1 FROM [version].[WorkspaceLockedPerDiem] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[WorkspaceLockedPerDiem] ON
INSERT INTO [dbo].[WorkspaceLockedPerDiem]
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
           ,[WorkspaceID])
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
  FROM [version].[WorkspaceLockedPerDiem] L
	INNER JOIN [version].Workspace WS ON L.WorkspaceID = WS.WorkspaceID
WHERE   
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND 
L.VersionID = @VersionID
SET IDENTITY_INSERT [dbo].[WorkspaceLockedPerDiem] OFF

END


IF EXISTS (SELECT 1 FROM [version].[WorkspaceLockedTravelEscalationRate] WHERE VersionID = @VersionID)
BEGIN


SET IDENTITY_INSERT [dbo].[WorkspaceLockedTravelEscalationRate] ON
INSERT INTO [dbo].[WorkspaceLockedTravelEscalationRate]
           ([WorkspaceLockedTravelEscalationRateID]
           ,[TravelEscalationRateID]
           ,[UpdateDT]
           ,[Year]
           ,[DevEscalation]
           ,[LMSIEscalation]
		   ,[MiscRate]
           ,[WorkspaceID])
SELECT L.[WorkspaceLockedTravelEscalationRateID]
      ,L.[TravelEscalationRateID]
      ,L.[UpdateDT]
      ,L.[Year]
      ,L.[DevEscalation]
      ,L.[LMSIEscalation]
	  ,L.[MiscRate]
      ,L.[WorkspaceID]
  FROM [version].[WorkspaceLockedTravelEscalationRate] L
	INNER JOIN [version].Workspace WS ON L.WorkspaceID = WS.WorkspaceID
WHERE   
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND 
L.VersionID = @VersionID
SET IDENTITY_INSERT  [dbo].[WorkspaceLockedTravelEscalationRate] OFF

END



IF EXISTS (SELECT 1 FROM [version].[WorkspaceLockedTravelMiscRate] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[WorkspaceLockedTravelMiscRate] ON
INSERT INTO [dbo].[WorkspaceLockedTravelMiscRate]
           ([WorkspaceLockedTravelMiscRateID]
           ,[TravelMiscRateID]
           ,[UpdateDT]
           ,[TransportationMode]
           ,[MiscellaneousRate]
           ,[SortCode]
           ,[MiscRateInUse]
           ,[WorkspaceID])
SELECT L.[WorkspaceLockedTravelMiscRateID]
      ,L.[TravelMiscRateID]
      ,L.[UpdateDT]
      ,L.[TransportationMode]
      ,L.[MiscellaneousRate]
      ,L.[SortCode]
      ,L.[MiscRateInUse]
      ,L.[WorkspaceID]
  FROM [version].[WorkspaceLockedTravelMiscRate] L
	INNER JOIN [version].Workspace WS ON L.WorkspaceID = WS.WorkspaceID
WHERE   
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND 
L.VersionID = @VersionID
SET IDENTITY_INSERT  [dbo].[WorkspaceLockedTravelMiscRate] OFF

END



IF EXISTS (SELECT 1 FROM [version].[WorkspaceLockedTrip] WHERE VersionID = @VersionID)
BEGIN

SET IDENTITY_INSERT [dbo].[WorkspaceLockedTrip] ON 
INSERT INTO [dbo].[WorkspaceLockedTrip]
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
           ,[WorkspaceID])
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
  FROM [version].[WorkspaceLockedTrip] L
	INNER JOIN [version].Workspace WS ON L.WorkspaceID = WS.WorkspaceID
WHERE   
WS.VersionID = @VersionID AND 
WS.WorkspaceID = @WorkspaceID AND 
L.VersionID = @VersionID
SET IDENTITY_INSERT  [dbo].[WorkspaceLockedTrip] OFF

END
*/ --END OF BOEJ-473

/* 
Create a log for auditing
*/
INSERT INTO [dbo].[WorkspaceRestoreLog]
           ([ActionPerformed]
           ,[WorkspaceID]
           ,[WorkspaceName]
           ,[ETIUserID]
           ,[RestoreDT]
           ,[VersionCreated])
SELECT
           'Workspace Restore' /*Action Performed*/
           ,@WorkspaceID 
           ,WorkspaceName
           ,@ETIUserID /*User performing the Restore*/
           ,GETDATE() /*Date the restore was performed*/
           ,WV.VersionCreated /*Date the version was created*/
FROM dbo.Workspace W 
	INNER JOIN dbo.WorkspaceVersion WV ON W.WorkspaceID = WV.WorkspaceID
WHERE 
	W.WorkspaceID = @WorkspaceID AND
	WV.WorkspaceID = @WorkspaceID AND
	WV.VersionID = @VersionID

EXECUTE  [dbo].[updateTravelTripInUse]

SELECT @ErrorMessage AS ErrorMessage 
		
END TRY

BEGIN CATCH	
	/*CONFIRM ALL IDENTITY IS SET CORRECTLY*/
	SET IDENTITY_INSERT [dbo].[BOE] OFF
	SET IDENTITY_INSERT [dbo].[BOEApproval] OFF
	SET IDENTITY_INSERT [dbo].[BOEApprovalHistory] OFF
	SET IDENTITY_INSERT [dbo].[BOEComment] OFF
	SET IDENTITY_INSERT [dbo].[BOECommentHistory] OFF
	SET IDENTITY_INSERT [dbo].[BOECustomFieldValueXREF] OFF
	SET IDENTITY_INSERT [dbo].[BOEFormIBOE] OFF
	SET IDENTITY_INSERT [dbo].[BOEFormPBOE] OFF
	SET IDENTITY_INSERT [dbo].[BOELaborSpread] OFF
	SET IDENTITY_INSERT [dbo].[BOELaborType] OFF
	SET IDENTITY_INSERT [dbo].[BOELaborTypeCustomFieldValueXREF] OFF
	SET IDENTITY_INSERT [dbo].[BOEPotentialRole] OFF
	SET IDENTITY_INSERT [dbo].[BOEStateHistory] OFF
	SET IDENTITY_INSERT [dbo].[BOETaskElement] OFF
	SET IDENTITY_INSERT [dbo].[BOETaskElementCustomFieldValueXREF] OFF
	SET IDENTITY_INSERT [dbo].[BOETaskElementWorkspaceVariableXREF] OFF
	SET IDENTITY_INSERT [dbo].[BOEUserRole] OFF
	SET IDENTITY_INSERT [dbo].[BOEUserRoleHistory] OFF
	SET IDENTITY_INSERT [dbo].[BusinessArea] OFF
	SET IDENTITY_INSERT [dbo].[CLIN] OFF
	SET IDENTITY_INSERT [dbo].[CustomField] OFF
	SET IDENTITY_INSERT [dbo].[CustomFieldValue] OFF
	SET IDENTITY_INSERT [dbo].[ETIGroup] OFF
	SET IDENTITY_INSERT [dbo].[ETIuser] OFF
	SET IDENTITY_INSERT [dbo].[LineOfBusiness] OFF
	SET IDENTITY_INSERT [dbo].[Location] OFF
	SET IDENTITY_INSERT [dbo].[MaterialTaskElement] OFF
	SET IDENTITY_INSERT [dbo].[MileageReimbursementRate] OFF
	SET IDENTITY_INSERT [dbo].[MOQTypeSelection] OFF
	SET IDENTITY_INSERT [dbo].[MOQTypeSelectionTableData] OFF
	SET IDENTITY_INSERT [dbo].[ODCSpread] OFF
	SET IDENTITY_INSERT [dbo].[ODCTaskElement] OFF
	SET IDENTITY_INSERT [dbo].[ODCType] OFF
	SET IDENTITY_INSERT [dbo].[OrdinaryVariable] OFF
	SET IDENTITY_INSERT [dbo].[OrdinaryVariableSumVariableResourceTypeXREF] OFF
	SET IDENTITY_INSERT [dbo].[OutputFormatTemplate] OFF
	SET IDENTITY_INSERT [dbo].[OutputFormatTemplateWorkspaceXREF] OFF
	SET IDENTITY_INSERT [dbo].[PerformingOrganization] OFF
	SET IDENTITY_INSERT [dbo].[PerformingOrganizationList] OFF
	SET IDENTITY_INSERT [dbo].[ProjectMap] OFF
	SET IDENTITY_INSERT [dbo].[ProPricerCustomFieldXREF] OFF
	SET IDENTITY_INSERT [dbo].[ProPricerExport] OFF
	SET IDENTITY_INSERT [dbo].[ProPricerFieldXREF] OFF
	SET IDENTITY_INSERT [dbo].[Resource] OFF
	SET IDENTITY_INSERT [dbo].[ResourceList] OFF
	SET IDENTITY_INSERT [dbo].[RteTemplate] OFF
	SET IDENTITY_INSERT [dbo].[RteTemplateQuestion] OFF
	SET IDENTITY_INSERT [dbo].[RteTemplateAnswer] OFF
	SET IDENTITY_INSERT [dbo].[SumOfBOE_OrdinaryVariableXREF] OFF
	SET IDENTITY_INSERT [dbo].[SumOfBOE_WorkspaceVariableXREF] OFF
	SET IDENTITY_INSERT [dbo].[SystemUserRole] OFF
	SET IDENTITY_INSERT [dbo].[TMResourceRate] OFF
	SET IDENTITY_INSERT [dbo].[TravelEscalationRate] OFF
	SET IDENTITY_INSERT [dbo].[TravelMiscRate] OFF
	SET IDENTITY_INSERT [dbo].[TravelTrip] OFF
	SET IDENTITY_INSERT [dbo].[TravelTripCustomFieldValueXREF] OFF
	SET IDENTITY_INSERT [dbo].[TravelTripTaskElement] OFF
	SET IDENTITY_INSERT [dbo].[TravelTripTaskElementCustomFieldValueXREF] OFF
	SET IDENTITY_INSERT [dbo].[Trip] OFF
	SET IDENTITY_INSERT [dbo].[WBS_CLIN_BOE_XREF] OFF
	SET IDENTITY_INSERT [dbo].[WorkBreakdownStructure] OFF
	SET IDENTITY_INSERT [dbo].[Workspace] OFF
	SET IDENTITY_INSERT [dbo].[WorkspaceContractTypeXREF] OFF
	SET IDENTITY_INSERT [dbo].[WorkspaceLockedPerDiem] OFF
	SET IDENTITY_INSERT [dbo].[WorkspaceLockedTravelEscalationRate] OFF
	SET IDENTITY_INSERT [dbo].[WorkspaceLockedTravelMiscRate] OFF
	SET IDENTITY_INSERT [dbo].[WorkspaceLockedTrip] OFF
	SET IDENTITY_INSERT [dbo].[WorkspacePerformingOrganization] OFF
	SET IDENTITY_INSERT [dbo].[WorkspaceResource] OFF
	SET IDENTITY_INSERT [dbo].[WorkspaceRestoreLog] OFF
	SET IDENTITY_INSERT [dbo].[WorkspaceStateHistory] OFF
	SET IDENTITY_INSERT [dbo].[WorkspaceUserRole] OFF
	SET IDENTITY_INSERT [dbo].[WorkspaceVariable] OFF
	SET IDENTITY_INSERT [dbo].[WorkspaceVariableSumVariableResourceTypeXREF] OFF
	SET IDENTITY_INSERT [dbo].[WorkspaceVersion] OFF
	SET IDENTITY_INSERT [dbo].[MSTTravelTripCustomFieldValueXREF] OFF
	SET IDENTITY_INSERT [dbo].[MSTTravelTrip] OFF
	SET IDENTITY_INSERT [dbo].[WorkspaceOffloadRate] OFF

	SELECT ERROR_MESSAGE()  AS ErrorMessage 
		
	RETURN
	
END CATCH

END
GO


