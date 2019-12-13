IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[archiveOutputFormatTemplate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[archiveOutputFormatTemplate];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[archiveOutputFormatTemplate]
(
@TemplateID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [archiveOutputFormatTemplate]
**		Desc: Archive the Output Format Template 
**			
**		
**
**		Auth: Don Canuso
**		Date: 3/30/2011
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		9/6/13		dcanuso				Allowing the deleting of Templates 
**										even if used by System Admins.
**		5/9/18		ranzalon			BOEJ-3412 - Renaming to archiveOutputFormatTemplate
**										from deleteOutputFormatTemplate, also updating
**										to clear workspace selections
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

IF (SELECT UpdateDT FROM [dbo].[OutputFormatTemplate] WHERE TemplateID = @TemplateID) = @UpdateDT
		BEGIN				
				--Clear workspace selections
				DELETE FROM dbo.OutputFormatTemplateWorkspaceXREF
				WHERE 
				TemplateID = @TemplateID
	
				--Set inactive, make sure available to all is unselected
				UPDATE dbo.OutputFormatTemplate
				SET IsActive = 0, IsAvailableToAllWorkspaces = 0
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