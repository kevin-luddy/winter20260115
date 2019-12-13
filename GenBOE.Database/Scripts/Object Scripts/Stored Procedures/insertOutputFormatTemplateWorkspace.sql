IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertOutputFormatTemplateWorkspace]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertOutputFormatTemplateWorkspace];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[insertOutputFormatTemplateWorkspace]
(
@WorkspaceID int,
@TemplateID int
)
AS
/******************************************************************************
**		 
**		Name: [insertOutputFormatTemplateWorkspace]
**		Desc:	Inserts the association of WorkspaceID and
**				selected WorkspaceID into dbo.OutputFormatTemplateWorkspaceXREF
**			
**		
**
**		Auth: Don Canuso
**		Date: 3/30/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

IF NOT EXISTS	(
				SELECT 1 FROM dbo.OutputFormatTemplateWorkspaceXREF
				WHERE TemplateID = @TemplateID AND WorkspaceID = @WorkspaceID 
				)
	BEGIN
		INSERT INTO [dbo].[OutputFormatTemplateWorkspaceXREF]
           ([WorkspaceID]
           ,[TemplateID])
     VALUES
           (@WorkspaceID
           ,@TemplateID)
	END
ELSE
	BEGIN
			DECLARE @ErrorMessage varchar (500)
			SET @ErrorMessage =   'The Output Format Template with Workspace ID ' + CAST(@WorkspaceID  AS varchar(10)) + ' and Template ID ' + CAST(@TemplateID  AS varchar(10)) + ' already exists.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
	END
GO