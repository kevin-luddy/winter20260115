IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOELaborType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOELaborType];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteBOELaborType]
(
@BOELaborTypeID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteBOELaborType]
**		Desc: Delete Flag set in Labor Type Section of BOE and sub-elements (Labor Spread)
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
**		8/19/10		dcanuso				RETURN was not working properly - corrected
**		9/13/10		dcanuso				Soft Deletes removed
**		3/29/11		dcanuso				Developer noticed Flags were not being 
**										updated - added code to correct
**		4/21/11		dcanuso				Added DELETE for Custom Field
**		8/18/11		dcanuso				Added In Use Processing of Custom Fields
**		11/25/11	dcanuso				Fixing In Use
**		12/15/11	dcanuso				WI 6166
**		12/21/11	dcanuso				WI 6021
**										Need to check if a Travel Resource Rate 
**										is being used/not used
**										and update accordingly
**		1/10/12		dcanuso				WI 
**										6470: Trip In Use Fixes
**										https://eureka.isgs.lmco.com/#activity/151789
**										6624: After WI 6622, we now need to check that 
**										the originating Labor Resource Rate is in 
**										use as we did for trips.
**										6339: Confirm in-use is working for workspace
**										and labor resource rates
**		8/7/12		dcanuso				WI 10278: Resource Redesign 
**										In Use will no longer be stored in DB
**		2/4/13		dcanuso				WI14842 Redesign Performing Organization
**      6/24/16     twilson3            Fix In-Use Flag for Custom Fields
**		4/2/18		ranzalon			BOEJ-3268 - Update for Open Ended Custom Fields
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
*******************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDT FROM [dbo].[BOELaborType] WHERE BOELaborTypeID = @BOELaborTypeID ) = @UpdateDT
		BEGIN
		
			DELETE FROM dbo.BOELaborSpread
			WHERE
				BOELaborTypeID = @BOELaborTypeID
	
	
				/*Variables Used for InUse Function*/
			DECLARE		@CurrentResourceID int,
						@CurrentPerformingOrganizationID int
			
			SELECT	@CurrentResourceID  = ResourceID,
					@CurrentPerformingOrganizationID = PerformingOrganizationID
			FROM [dbo].[BOELaborType]
			WHERE BOELaborTypeID = @BOELaborTypeID
			
			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @CustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @CustomFieldXrefs
			SELECT CustomFieldValueID
			FROM dbo.BOELaborTypeCustomFieldValueXREF
			WHERE BOELaborTypeID = @BOELaborTypeID
	
			--Delete Custom Field Value Xrefs
			DELETE FROM dbo.BOELaborTypeCustomFieldValueXREF
			WHERE BOELaborTypeID = @BOELaborTypeID

			--Delete Custom Field Values for deleted Open Ended Custom Fields
			DELETE FROM dbo.CustomFieldValue
			WHERE CustomFieldValueID in
			(
				SELECT x.CustomFieldValueID
				FROM @CustomFieldXrefs x
				JOIN dbo.CustomFieldValue v on x.CustomFieldValueID = v.CustomFieldValueID
				JOIN dbo.CustomField c on v.CustomFieldId = c.CustomFieldID
				WHERE c.IsOpenEnded = 1
			)
	
			DELETE FROM dbo.BOELaborType
			WHERE
				BOELaborTypeID = @BOELaborTypeID
	
	
	
				
				/*	Update In Use Flag
					Fix All In Use Flags
					Check if Resource is used elsewhere
				*/
				/*EXEC  [dbo].[updateResourceInUseFlagByResourceID] @CurrentResourceID*/
					 
				/*EXECUTE [dbo].[updatePerformingOrganizationInUseFlagByPerformingOrganizationID] @CurrentPerformingOrganizationID*/
					
					


			DECLARE @WorkspaceID int
			SELECT @WorkspaceID = B.WorkspaceID 
				FROM dbo.BOE B
					INNER JOIN dbo.BOETaskElement TE ON B.BOEID = TE.BOEID
					INNER JOIN dbo.BOELaborType LT ON TE.BOETaskElementID = LT.BOETaskElementID
				WHERE LT.BOELaborTypeID = @BOELaborTypeID
			
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Labor Type with ID ' + CAST(@BOELaborTypeID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END

GO