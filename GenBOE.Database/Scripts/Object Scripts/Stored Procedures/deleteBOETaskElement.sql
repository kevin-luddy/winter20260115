IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOETaskElement]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOETaskElement];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteBOETaskElement]
(
@BOETaskElementID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteBOETaskElement]
**		Desc: Delete Flag set in LM Task Element Section of BOE and all sub-elements (Labor Types and Labor Spread)
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
**		1/16/18		twilson3			BOEJ-2887 Remove Historical Metrics
**		4/2/18		ranzalon			BOEJ-3268 - Update for Open Ended Custom Fields
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
**		12/13/19	twilson3			BOEJ-4434 - RTE Template Answers
**		10/29/20	Dusan				BOEJ-4924 - MOQ Type Selection data
**		1/4/2020	Dusan				BOEJ-4894: Added support for MoqTypeTableCustomFieldValueXREF
*******************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDT FROM [dbo].[BOETaskElement] WHERE BOETaskElementID = @BOETaskElementID) = @UpdateDT
		BEGIN

			-- Delete RTE Template Answers
			DELETE FROM dbo.[RteTemplateAnswer]
			WHERE TaskID = @BOETaskElementID

			DELETE FROM dbo.BOELaborSpread
				FROM dbo.BOELaborSpread LS
				INNER JOIN dbo.BOELaborType LT ON LS.BOELaborTypeID = LT.BOELaborTypeID
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
			WHERE
				TE.BOETaskElementID = @BOETaskElementID
				
			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @LaborTypeCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @LaborTypeCustomFieldXrefs
			SELECT CustomFieldValueID
			FROM dbo.BOELaborTypeCustomFieldValueXREF X
				INNER JOIN dbo.BOELaborType LT ON X.BOELaborTypeID = LT.BOELaborTypeID
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
			WHERE
				TE.BOETaskElementID = @BOETaskElementID

			DELETE FROM dbo.BOELaborTypeCustomFieldValueXREF
			FROM dbo.BOELaborTypeCustomFieldValueXREF X
				INNER JOIN dbo.BOELaborType LT ON X.BOELaborTypeID = LT.BOELaborTypeID
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
			WHERE
				TE.BOETaskElementID = @BOETaskElementID
				
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
				
			DELETE FROM dbo.BOETaskElementWorkspaceVariableXREF
			WHERE
				BOETaskElementID = @BOETaskElementID
				
			DELETE FROM dbo.SumOfBOE_OrdinaryVariableXREF
			FROM  dbo.SumOfBOE_OrdinaryVariableXREF X
				INNER JOIN dbo.OrdinaryVariable OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
				INNER JOIN  dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
			WHERE TE.BOETaskElementID = @BOETaskElementID
			

			DELETE FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF
			FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF X
				INNER JOIN dbo.OrdinaryVariable OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
				INNER JOIN  dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
			WHERE TE.BOETaskElementID = @BOETaskElementID

				

			DELETE FROM dbo.OrdinaryVariable
			FROM dbo.OrdinaryVariable OV
				INNER JOIN dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
			WHERE TE.BOETaskElementID = @BOETaskElementID
				
			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @TaskCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @TaskCustomFieldXrefs
			SELECT CustomFieldValueID
			FROM dbo.BOETaskElementCustomFieldValueXREF
			WHERE BOETaskElementID = @BOETaskElementID

			DELETE FROM dbo.BOETaskElementCustomFieldValueXREF
			FROM dbo.BOETaskElementCustomFieldValueXREF X 
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID 
			WHERE TE.BOETaskElementID = @BOETaskElementID

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
			WHERE TE.BOETaskElementID = @BOETaskElementID	

			
			DELETE FROM dbo.BOELaborType
				FROM dbo.BOELaborType LT
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
			WHERE
				TE.BOETaskElementID = @BOETaskElementID
				
				
			DECLARE @WorkspaceID int
			SELECT @WorkspaceID = WorkspaceID 
			FROM dbo.BOE B
				INNER JOIN dbo.BOETaskElement TE ON B.BOEID = TE.BOEID
			WHERE TE.BOETaskElementID = @BOETaskElementID

			-- MOQ Type Selection data
			DELETE FROM dbo.[MoqTypeTableCustomFieldValueXREF]
					FROM dbo.[MoqTypeTableCustomFieldValueXREF] x
						INNER JOIN MoqTypeSelectionTableData t ON t.MoqTypeSelectionTableDataId = x.MoqTypeTableDataId 
						INNER JOIN MoqTypeSelection mS ON mS.MoqTypeSelectionId = t.MoqTypeSelectionId 
					WHERE mS.TaskId = @BOETaskElementID

			DELETE FROM dbo.MOQTypeSelectionTableData
				FROM dbo.MOQTypeSelectionTableData t
				INNER JOIN dbo.MOQTypeSelection s ON s.MOQTypeSelectionId = t.MOQTypeSelectionId
				INNER JOIN dbo.BOETaskElement TE ON s.TaskId = TE.BOETaskElementID
			WHERE
				TE.BOETaskElementID = @BOETaskElementID

			DELETE FROM dbo.MOQTypeSelection
				FROM dbo.MOQTypeSelection s
				INNER JOIN dbo.BOETaskElement TE ON s.TaskId = TE.BOETaskElementID
			WHERE
				TE.BOETaskElementID = @BOETaskElementID

			-- Task Element
			DELETE FROM dbo.BOETaskElement
			WHERE
				BOETaskElementID = @BOETaskElementID

		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Task Element with ID ' + CAST(@BOETaskElementID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
 
		END

GO