IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOELaborTypeCustomFieldValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOELaborTypeCustomFieldValue];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteBOELaborTypeCustomFieldValue]
(
@BLTCFVID [int],
@BOELaborTypeID [int],
@CustomFieldValueID [int],
@UpdateDT datetime2,
@IsOpenEnded bit
)
AS
/******************************************************************************
**		 
**		Name: [deleteBOELaborTypeCustomFieldValue]
**		Desc: Deletes data from the dbo.BOELaborTypeCustomFieldValueXREF table which
**				holds the Custom Field Value for the BOELaborType
**			
**		
**
**		Auth: Don Canuso
**		Date: 2/14/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		4/1/11		dcanuso				Updated order of SP to correct for logic 
**										and updated UPDATE to handle NULL
**		5/26/11		dcanuso				Updated the use of PK and fixed PK Name
**		7/20/11		DCANUSO				WI 4215 - Custom Fields are scope specific
**		3/8/18		ranzalon			BOEJ-3079 - Open Ended Custom Fields
*******************************************************************************/
SET NOCOUNT ON 

IF (SELECT UpdateDT FROM [dbo].[BOELaborTypeCustomFieldValueXREF] WHERE BLTCFVID = @BLTCFVID) = @UpdateDT
	BEGIN		
		DELETE FROM [dbo].[BOELaborTypeCustomFieldValueXREF] WHERE BLTCFVID = @BLTCFVID
		
		IF @IsOpenEnded = 0 /*Additional Standard CF work*/
			BEGIN
				/*Only should be looking within the scope
				of the Custom Field, so if the scope is BOE
				then only look in BOECustomFieldValueXREF
				*/
		
				UPDATE dbo.CustomFieldValue
					SET CustomFieldValueInUseFlag = CASE 
														WHEN X.[CustomFieldValueID] IS NULL THEN 0
														WHEN X.[CustomFieldValueID] IS NOT NULL THEN 1
													END,
						UpdateDT = GETDATE()
				FROM dbo.CustomFieldValue CFV 
					LEFT OUTER JOIN [dbo].[BOELaborTypeCustomFieldValueXREF] X ON
						CFV.[CustomFieldValueID] = X.[CustomFieldValueID]
				WHERE CFV.CustomFieldValueID = @CustomFieldValueID
			END
		ELSE /*Additional Open Ended work*/
			BEGIN
				DELETE FROM [dbo].[CustomFieldValue] WHERE CustomFieldValueID = @CustomFieldValueID
			END
	END
ELSE
	BEGIN
		DECLARE @ErrorMessage varchar (500)
			SET @ErrorMessage =  'The Labor Type Custom Field with ID ' + CAST(@BLTCFVID AS varchar(10)) +  ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
		RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
		RETURN
	END
	
GO