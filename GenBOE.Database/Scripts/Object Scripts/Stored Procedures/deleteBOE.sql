IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOE]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOE];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteBOE]
(
@BOEID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteBOE]
**		Desc: Delete all parts of BOE
**			
**		
**
**		Auth: Don Canuso
**		Date: 8/2010
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**      1/17/17		twilson3			BOEJ-1604 Cleanup DB Project
**		7/21/17		twilson3			BOEJ-2408 Remove WBS/CLIN/BOE Ref only when this is last WBS->CLIN ref
**		8/30/2017	Dusan				Fixing up performance
**		1/16/18		twilson3			BOEJ-2887 Remove Historical Metrics
**		4/2/18		ranzalon			BOEJ-3268 - Update for Open Ended Custom Fields
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
**		12/13/19	twilson3			BOEJ-4434 - RTE Template Answers
**		10/29/20	Dusan				BOEJ-4924 - MOQ Type Selection data
**		1/4/2021	Dusan				BOEJ-4894: Added support for MoqTypeTableCustomFieldValueXREF
**		7/17/2024	e405721				PROPH-2163: Update for Skill Mix, Common Disclosure Skill Mix, MOQ Type Selection Table Data Resource Hours
*****************************************************************************/
SET NOCOUNT ON 

	IF (SELECT UpdateDT FROM [dbo].[BOE] WHERE BOEID = @BOEID) = @UpdateDT
		BEGIN
			SET @UpdateDT = GETDATE()
			
			DELETE FROM dbo.BOELaborSpread
			FROM dbo.BOELaborSpread LS
				INNER JOIN dbo.BOELaborType LT ON LS.BOELaborTypeID = LT.BOELaborTypeID
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
			WHERE TE.BOEID = @BOEID

			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @LaborTypeCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @LaborTypeCustomFieldXrefs
			SELECT CustomFieldValueID
			FROM dbo.BOELaborTypeCustomFieldValueXREF X
				INNER JOIN dbo.BOELaborType LT ON X.BOELaborTypeID = LT.BOELaborTypeID
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
			WHERE TE.BOEID = @BOEID
			
			DELETE FROM dbo.BOELaborTypeCustomFieldValueXREF
			FROM dbo.BOELaborTypeCustomFieldValueXREF X
				INNER JOIN dbo.BOELaborType LT ON X.BOELaborTypeID = LT.BOELaborTypeID
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
			WHERE TE.BOEID = @BOEID
			
			--Delete Custom Field Values for deleted Open Ended Custom Fields
			DELETE FROM dbo.CustomFieldValue
			WHERE CustomFieldValueID in
			(
				SELECT x.CustomFieldValueID
				FROM @LaborTypeCustomFieldXrefs x
				JOIN dbo.CustomFieldValue v on x.CustomFieldValueID = v.CustomFieldValueID
				JOIN dbo.CustomField c on v.CustomFieldId = c.CustomFieldID
				WHERE c.IsOpenEnded = 1
			)
				
			DELETE FROM dbo.SumOfBOE_OrdinaryVariableXREF
			FROM dbo.SumOfBOE_OrdinaryVariableXREF X
				INNER JOIN dbo.OrdinaryVariable OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
				INNER JOIN dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
			WHERE TE.BOEID = @BOEID
								
			DELETE FROM dbo.SumOfBOE_OrdinaryVariableXREF WHERE BOEID = @BOEID
				
			DELETE FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF
			FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF X
				INNER JOIN dbo.OrdinaryVariable OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
				INNER JOIN dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
			WHERE TE.BOEID = @BOEID
				
			DELETE FROM dbo.OrdinaryVariable
			FROM dbo.OrdinaryVariable OV
				INNER JOIN dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
			WHERE TE.BOEID = @BOEID

			-- Delete RTE Template Answers
			DELETE FROM dbo.[RteTemplateAnswer]
			WHERE BOEID = @BOEID
				
			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @BoeCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @BoeCustomFieldXrefs
			SELECT CustomFieldValueID
			FROM dbo.BOECustomFieldValueXREF
			WHERE BOEID = @BOEID

			DELETE FROM dbo.BOECustomFieldValueXREF WHERE BOEID = @BOEID
			
			--Delete Custom Field Values for deleted Open Ended Custom Fields
			DELETE FROM dbo.CustomFieldValue
			WHERE CustomFieldValueID in
			(
				SELECT x.CustomFieldValueID
				FROM @BoeCustomFieldXrefs x
				JOIN dbo.CustomFieldValue v on x.CustomFieldValueID = v.CustomFieldValueID
				JOIN dbo.CustomField c on v.CustomFieldId = c.CustomFieldID
				WHERE c.IsOpenEnded = 1
			)

			DELETE FROM dbo.BOELaborType
			FROM dbo.BOELaborType LT 
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
			WHERE TE.BOEID = @BOEID
				
			DELETE FROM dbo.BOEApprovalHistory WHERE BOEID = @BOEID
			DELETE FROM dbo.BOECommentHistory WHERE BOEID = @BOEID
								
			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @TaskCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @TaskCustomFieldXrefs
			SELECT CustomFieldValueID
			FROM dbo.BOETaskElementCustomFieldValueXREF X
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID 
			WHERE TE.BOEID = @BOEID

			DELETE FROM dbo.BOETaskElementCustomFieldValueXREF
			FROM dbo.BOETaskElementCustomFieldValueXREF X 
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID 
			WHERE TE.BOEID = @BOEID
		
			--Delete Custom Field Values for deleted Open Ended Custom Fields
			DELETE FROM dbo.CustomFieldValue
			WHERE CustomFieldValueID in
			(
				SELECT x.CustomFieldValueID
				FROM @TaskCustomFieldXrefs x
				JOIN dbo.CustomFieldValue v on x.CustomFieldValueID = v.CustomFieldValueID
				JOIN dbo.CustomField c on v.CustomFieldId = c.CustomFieldID
				WHERE c.IsOpenEnded = 1
			)

			DELETE FROM [dbo].[BOETaskElementMetricDetailXREF]
				FROM [dbo].[BOETaskElementMetricDetailXREF] X 
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID 
			WHERE TE.BOEID = @BOEID
				
			DELETE FROM dbo.BOETaskElementWorkspaceVariableXREF
			FROM  dbo.BOETaskElementWorkspaceVariableXREF X
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID
			WHERE TE.BOEID = @BOEID
			
			-- MOQ Type Selection data
			DELETE FROM dbo.[MoqTypeTableCustomFieldValueXREF]
					FROM dbo.[MoqTypeTableCustomFieldValueXREF] x
						INNER JOIN MoqTypeSelectionTableData t ON t.MoqTypeSelectionTableDataId = x.MoqTypeTableDataId 
						INNER JOIN MoqTypeSelection mS ON mS.MoqTypeSelectionId = t.MoqTypeSelectionId 
						INNER JOIN BoeTaskElement tE ON tE.BoeTaskElementId = mS.TaskId 
					WHERE tE.BOEID = @BOEID
			
			DELETE m
					FROM dbo.[MOQTypeSelectionTableDataResourceHours] m
			WHERE m.BOEID = @BOEID
			
			DELETE FROM dbo.MOQTypeSelectionTableData
				FROM dbo.MOQTypeSelectionTableData t
				INNER JOIN dbo.MOQTypeSelection s ON s.MOQTypeSelectionId = t.MOQTypeSelectionId
				INNER JOIN dbo.BOETaskElement TE ON s.TaskId = TE.BOETaskElementID
			WHERE TE.BOEID = @BOEID

			DELETE cd
					FROM dbo.CommonDisclosureSkillMix cd
			WHERE cd.BOEID = @BOEID

			DELETE sm
					FROM dbo.[SkillMix] sm
			WHERE sm.BOEID = @BOEID

			DELETE FROM dbo.MOQTypeSelection
				FROM dbo.MOQTypeSelection s
				INNER JOIN dbo.BOETaskElement TE ON s.TaskId = TE.BOETaskElementID
			WHERE TE.BOEID = @BOEID

			DELETE FROM dbo.SumOfBOE_WorkspaceVariableXREF WHERE BOEID = @BOEID
			DELETE FROM dbo.BOEUserRoleHistory WHERE BOEID = @BOEID
			DELETE FROM dbo.BOEUserRole WHERE BOEID = @BOEID
			DELETE FROM dbo.BOETaskElement WHERE BOEID = @BOEID
			DELETE FROM dbo.BOEComment WHERE BOEID = @BOEID					
			DELETE FROM dbo.BOEApproval WHERE BOEID = @BOEID	
			DELETE FROM dbo.BOEStateHistory WHERE BOEID = @BOEID
			DELETE FROM dbo.SumOfBOE_WorkspaceVariableXREF WHERE BOEID = @BOEID
			DELETE FROM dbo.WBS_CLIN_BOE_XREF WHERE BOEID = @BOEID AND (CLINID IS NULL OR WBSID IS NULL)

			-- Delete the XREF if there are other XREFs for this WBSID
			-- Otherwise, only remove the BOEID from the XREF

			DECLARE @NumberXREFs int
			DECLARE @WBSID int
			SELECT @WBSID = WBSID FROM dbo.WBS_CLIN_BOE_XREF WHERE BOEID = @BOEID

			SELECT @NumberXREFs = COUNT(*) FROM dbo.WBS_CLIN_BOE_XREF WHERE WBSID = @WBSID

			IF (@NumberXREFs > 1)
				BEGIN
					DELETE FROM dbo.WBS_CLIN_BOE_XREF WHERE BOEID = @BOEID
				END
			ELSE
				BEGIN
					UPDATE dbo.WBS_CLIN_BOE_XREF SET BOEID = NULL WHERE BOEID = @BOEID
				END

			DELETE FROM dbo.MaterialTaskElement WHERE BOEID = @BOEID
			
			DELETE FROM dbo.ODCSpread                 
			FROM dbo.ODCSpread S
				INNER JOIN dbo.ODCType T ON S.ODCTypeID = T.ODCTypeID
				INNER JOIN dbo.ODCTaskElement TE ON T.ODCTaskElementID = TE.ODCTaskElementID
			WHERE TE.BOEID = @BOEID
			
			DELETE FROM dbo.ODCType                 
			FROM dbo.ODCType T 
				INNER JOIN dbo.ODCTaskElement TE ON T.ODCTaskElementID = TE.ODCTaskElementID
			WHERE TE.BOEID = @BOEID

			DELETE FROM dbo.ODCTaskElement WHERE BOEID = @BOEID

			DELETE FROM dbo.TravelTripCustomFieldValueXREF
				FROM dbo.TravelTripCustomFieldValueXREF X
				INNER JOIN dbo.TravelTrip T ON X.TravelTripID = T.TravelTripID
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
			WHERE TE.BOEID = @BOEID

			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @MSTTripCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @MSTTripCustomFieldXrefs
			SELECT MSTCustomFieldValueID
			FROM dbo.MSTTravelTripCustomFieldValueXREF X
				INNER JOIN dbo.MSTTravelTrip T ON X.mstTravelTripID = T.mstTravelTripID
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
			WHERE TE.BOEID = @BOEID

			DELETE FROM dbo.MSTTravelTripCustomFieldValueXREF
				FROM dbo.MSTTravelTripCustomFieldValueXREF X
				INNER JOIN dbo.MSTTravelTrip T ON X.mstTravelTripID = T.mstTravelTripID
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
			WHERE TE.BOEID = @BOEID
			
			--Delete Custom Field Values for deleted Open Ended Custom Fields
			DELETE FROM dbo.CustomFieldValue
			WHERE CustomFieldValueID in
			(
				SELECT x.CustomFieldValueID
				FROM @MSTTripCustomFieldXrefs x
				JOIN dbo.CustomFieldValue v on x.CustomFieldValueID = v.CustomFieldValueID
				JOIN dbo.CustomField c on v.CustomFieldId = c.CustomFieldID
				WHERE c.IsOpenEnded = 1
			)

			DECLARE @TripAffected TABLE (TripID int)					
			INSERT INTO @TripAffected 
			SELECT TripID
			FROM dbo.TravelTrip T
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
			WHERE TE.BOEID = @BOEID
			
			DELETE FROM dbo.TravelTrip                 
			FROM dbo.TravelTrip T 
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
			WHERE TE.BOEID = @BOEID

			DELETE FROM dbo.mstTravelTrip                 
			FROM dbo.mstTravelTrip T 
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
			WHERE TE.BOEID = @BOEID

			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @TravelTaskCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @TravelTaskCustomFieldXrefs
			SELECT CustomFieldValueID
			FROM dbo.TravelTripTaskElementCustomFieldValueXREF X
				INNER JOIN dbo.TravelTripTaskElement TE ON X.TravelTripTaskElementID = TE.TravelTripTaskElementID
			WHERE TE.BOEID = @BOEID

			DELETE FROM dbo.TravelTripTaskElementCustomFieldValueXREF				
				FROM dbo.TravelTripTaskElementCustomFieldValueXREF X
				INNER JOIN dbo.TravelTripTaskElement TE ON X.TravelTripTaskElementID = TE.TravelTripTaskElementID
			WHERE TE.BOEID = @BOEID

			--Delete Custom Field Values for deleted Open Ended Custom Fields
			DELETE FROM dbo.CustomFieldValue
			WHERE CustomFieldValueID in
			(
				SELECT x.CustomFieldValueID
				FROM @TravelTaskCustomFieldXrefs x
				JOIN dbo.CustomFieldValue v on x.CustomFieldValueID = v.CustomFieldValueID
				JOIN dbo.CustomField c on v.CustomFieldId = c.CustomFieldID
				WHERE c.IsOpenEnded = 1
			)

			DELETE FROM dbo.TravelTripTaskElement WHERE BOEID = @BOEID

			DECLARE @WorkspaceID int
			SELECT @WorkspaceID = WorkspaceID FROM dbo.BOE WHERE BOEID = @BOEID

			DELETE FROM dbo.BOE	WHERE BOEID = @BOEID
				
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =    'The BOE with ID ' + CAST(@BOEID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END

GO