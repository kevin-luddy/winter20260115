IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteMoqTypeTableCustomFieldValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteMoqTypeTableCustomFieldValue];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteMoqTypeTableCustomFieldValue]
(
	@Id INT,
	@MoqTypeTableDataId INT,
	@CustomFieldValueId INT,
	@UpdateDT DATETIME2(7),
	@IsOpenEnded BIT
)
AS
/******************************************************************************
**		Name: [deleteMoqTypeTableCustomFieldValue]
**		Desc: Deletes data from the dbo.MoqTypeTableCustomFieldValueXREF table which
**				holds the Custom Field Value for the Moq Type Table data
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		2020/12/8	Dusan				Initial Release 
*******************************************************************************/
SET NOCOUNT ON 

IF (SELECT UpdateDT FROM [dbo].[MoqTypeTableCustomFieldValueXREF] WHERE Id = @Id) = @UpdateDT
	BEGIN
		DELETE FROM [dbo].[MoqTypeTableCustomFieldValueXREF] WHERE Id = @Id

		IF @IsOpenEnded = 0 /*Additional Standard CF work*/
			BEGIN
				UPDATE dbo.CustomFieldValue
					SET CustomFieldValueInUseFlag = CASE 
														WHEN X.[CustomFieldValueId] IS NULL THEN 0
														WHEN X.[CustomFieldValueId] IS NOT NULL THEN 1
													END,
						UpdateDT = GETDATE()
				FROM dbo.CustomFieldValue CFV 
					LEFT OUTER JOIN [dbo].[MoqTypeTableCustomFieldValueXREF] X ON
						CFV.[CustomFieldValueId] = X.[CustomFieldValueId]
				WHERE CFV.CustomFieldValueId = @CustomFieldValueId
			END
		ELSE /*Additional Open Ended work*/
			BEGIN
				DELETE FROM [dbo].[CustomFieldValue] WHERE CustomFieldValueId = @CustomFieldValueId
			END
	END
ELSE
	BEGIN
		DECLARE @ErrorMessage varchar (500) = 'The Moq Type Table Custom Field with ID ' + CAST(@Id AS varchar(10)) +  ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
		RAISERROR (@ErrorMessage, 11, 1)
		RETURN
	END
GO

