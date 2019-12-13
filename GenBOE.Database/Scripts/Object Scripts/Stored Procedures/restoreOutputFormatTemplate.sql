IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[restoreOutputFormatTemplate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[restoreOutputFormatTemplate];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[restoreOutputFormatTemplate]
(
@TemplateID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [restoreOutputFormatTemplate]
**		Desc: Restores an archived Output Format Template 
**			
**		
**
**		Auth: RJ Anzalone
**		Date: 5/9/18
*******************************************************************************
**		Change History
*******************************************************************************
**		
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

IF (SELECT UpdateDT FROM [dbo].[OutputFormatTemplate] WHERE TemplateID = @TemplateID) = @UpdateDT
		BEGIN					
				UPDATE dbo.OutputFormatTemplate
				SET IsActive = 1
				WHERE  TemplateID = @TemplateID 
		END
	ELSE
		BEGIN
				SET @ErrorMessage =   'The Output Format Template with ID ' + CAST(@TemplateID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
						@ErrorMessage, -- Message text.
						11, -- Severity,/*Severity Changed to 11*/
						1 -- State,
						)
				RETURN
		END

IF @@ERROR = 0
	SELECT	@TemplateID AS TemplateID

GO