CREATE OR ALTER PROCEDURE [dbo].[deleteBOETaskElementviaTableParameter]
(
@BOETaskElement [dbo].[TT_BOETaskElement] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [deleteBOETaskElementviaTableParameter]
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
**      6/24/16     twilson3            Fix In-Use Flag for Custom Fields
**		1/16/18		twilson3			BOEJ-2887 Remove Historical Metrics
**		4/2/18		ranzalon			BOEJ-3268 - Update for Open Ended Custom Fields
**		6/5/20		ranzalon			BOEJ-4658 - Update for RTE Template Answers
**		3/6/25		e405721				PROPH-2895 - Update Delete for Skill Mix and Common Disclosure
**		9/30/25		e378233				PROPH-3302 - Update Delete for Skill Mix Summary
*******************************************************************************/
SET NOCOUNT ON 

			-- Delete RTE Template Answers
			DELETE FROM dbo.[RteTemplateAnswer]
			WHERE TaskID IN
				(
					SELECT BOETaskElementID
					FROM @BOETaskElement
				)

			DELETE FROM dbo.BOELaborSpread
				FROM dbo.BOELaborSpread LS
				INNER JOIN dbo.BOELaborType LT ON LS.BOELaborTypeID = LT.BOELaborTypeID
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT
			
			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @LaborTypeCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @LaborTypeCustomFieldXrefs
			SELECT CustomFieldValueID
			FROM dbo.BOELaborTypeCustomFieldValueXREF
			WHERE BOELaborTypeID IN
				(
					SELECT BOELaborTypeID
					FROM dbo.BOELaborType LT
					INNER JOIN @BOETaskElement TT ON 
					LT.BOETaskElementID = TT.BOETaskElementID
				)	
			
			DELETE FROM dbo.BOELaborTypeCustomFieldValueXREF
			FROM dbo.BOELaborTypeCustomFieldValueXREF X
				INNER JOIN dbo.BOELaborType LT ON X.BOELaborTypeID = LT.BOELaborTypeID
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT
				
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
			FROM dbo.BOETaskElementWorkspaceVariableXREF X  
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT

				
			DELETE FROM dbo.SumOfBOE_OrdinaryVariableXREF
			FROM  dbo.SumOfBOE_OrdinaryVariableXREF X
				INNER JOIN dbo.OrdinaryVariable OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
				INNER JOIN  dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT

			

			DELETE FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF
			FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF X
				INNER JOIN dbo.OrdinaryVariable OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
				INNER JOIN  dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
							INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT


				

			DELETE FROM dbo.OrdinaryVariable
			FROM dbo.OrdinaryVariable OV
				INNER JOIN dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT

			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @TaskCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @TaskCustomFieldXrefs
			SELECT CustomFieldValueID
			FROM dbo.BOETaskElementCustomFieldValueXREF
			WHERE BOETaskElementID IN
			(
				SELECT BOETaskElementID
				FROM @BOETaskElement
			)
				
			DELETE FROM dbo.BOETaskElementCustomFieldValueXREF
			FROM dbo.BOETaskElementCustomFieldValueXREF X 
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID 
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT

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
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT


			
 
			DELETE FROM dbo.BOELaborType
				FROM dbo.BOELaborType LT
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT

				
				
			DECLARE @WorkspaceID int
			SELECT @WorkspaceID = WorkspaceID 
			FROM dbo.BOE B
				INNER JOIN dbo.BOETaskElement TE ON B.BOEID = TE.BOEID
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT


			DELETE FROM dbo.CommonDisclosureSkillMix
				FROM dbo.CommonDisclosureSkillMix CS
					INNER JOIN dbo.BOETaskElement TE ON CS.BOETaskElementID = TE.BOETaskElementID
					INNER JOIN @BOETaskElement TT ON 
						TE.BOETaskElementID = TT.BOETaskElementID AND
						TE.UpdateDT = TT.UpdateDT


			DELETE FROM dbo.SkillMixSummary
				FROM dbo.SkillMixSummary SMS
					INNER JOIN dbo.BOETaskElement TE ON SMS.BOETaskElementID = TE.BOETaskElementID
					INNER JOIN @BOETaskElement TT ON 
						TE.BOETaskElementID = TT.BOETaskElementID AND
						TE.UpdateDT = TT.UpdateDT

			DELETE FROM dbo.SkillMix
				FROM dbo.SkillMix S
					INNER JOIN dbo.BOETaskElement TE ON S.BOETaskElementID = TE.BOETaskElementID
					INNER JOIN @BOETaskElement TT ON 
						TE.BOETaskElementID = TT.BOETaskElementID AND
						TE.UpdateDT = TT.UpdateDT
			
			
			DELETE FROM [dbo].[BOETaskElement]
			FROM [dbo].[BOETaskElement] TE
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT



IF @@ERROR <> 0
BEGIN
DECLARE @ErrorMessage varchar (500)
SET @ErrorMessage =   'The BOE Task Element(s) has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
					11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
 
		END
GO