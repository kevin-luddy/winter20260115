IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOETaskElementCustomFieldValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOETaskElementCustomFieldValue];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteBOETaskElementCustomFieldValue]
(
@BTECFVID [int],
@BOETaskElementID [int],
@CustomFieldValueID [int],
@UpdateDT datetime2,
@IsOpenEnded bit
)
AS
/******************************************************************************
**		 
**		Name: [deleteBOETaskElementCustomFieldValue]
**		Desc: Deletes data from the dbo.BOETaskElementCustomFieldValueXREF table which
**				holds the Custom Field Value for the BOE Task Element
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
**		7/20/11		DCANUSO				WI 4215 - Custom Fields are scope specific
**		12/15/11	dcanuso				WI 6166
**		3/8/18		ranzalon			BOEJ-3079 - Open Ended Custom Fields
*******************************************************************************/
SET NOCOUNT ON 

IF (SELECT UpdateDT FROM [dbo].[BOETaskElementCustomFieldValueXREF] WHERE BTECFVID = @BTECFVID) = @UpdateDT
	BEGIN
		DELETE FROM [dbo].[BOETaskElementCustomFieldValueXREF] WHERE BTECFVID = @BTECFVID

		IF @IsOpenEnded = 0 /*Additional Standard CF work*/
			BEGIN
				/*Only should be looking within the scope
				of the Custom Field, so if the scope is BOE
				then only look in BOECustomFieldValueXREF
				*/
				IF NOT EXISTS (SELECT [CustomFieldValueID] FROM	[BOETaskElementCustomFieldValueXREF]
								WHERE  CustomFieldValueID = @CustomFieldValueID)
					BEGIN
						UPDATE dbo.CustomFieldValue
						SET CustomFieldValueInUseFlag = 0,
							UpdateDT = GETDATE()
						FROM dbo.CustomFieldValue CFV 
						WHERE CFV.CustomFieldValueID = @CustomFieldValueID
					END
			END
		ELSE /*Additional Open Ended work*/
			BEGIN
				DELETE FROM [dbo].[CustomFieldValue] WHERE CustomFieldValueID = @CustomFieldValueID
			END
	END
ELSE
	BEGIN
		DECLARE @ErrorMessage varchar (500)
			SET @ErrorMessage =  'The Task Element Custom Field with ID ' + CAST(@BTECFVID AS varchar(10)) +  ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
		RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
		RETURN
	END

GO