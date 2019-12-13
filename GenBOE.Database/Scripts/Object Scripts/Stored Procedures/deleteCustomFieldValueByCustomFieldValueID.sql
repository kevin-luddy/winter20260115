IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteCustomFieldValueByCustomFieldValueID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteCustomFieldValueByCustomFieldValueID];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteCustomFieldValueByCustomFieldValueID]
(
@CustomFieldValueID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteCustomFieldValueByCustomFieldValueID]
**		Desc: Delete Custom Field Values Based On CustomFieldValueID
**
**		Auth: Don Canuso
**		Date: 2/2/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
******************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDT FROM [dbo].[CustomFieldValue] WHERE CustomFieldValueID = @CustomFieldValueID) = @UpdateDT
		BEGIN
			DELETE FROM [dbo].[CustomFieldValue] WHERE CustomFieldValueID = @CustomFieldValueID AND CustomFieldValueInUseFlag = 0
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
			SET @ErrorMessage =   'The Custom Field with ID ' + CAST(@CustomFieldValueID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END

GO