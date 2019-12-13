IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateWorkspaceAllowSearch]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateWorkspaceAllowSearch];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[updateWorkspaceAllowSearch]
(
@WorkspaceID int,
@AllowSearch bit,
@ContainsTemplate bit
)
AS
/******************************************************************************
**		 
**		Name:	[updateWorkspaceAllowSearch]
**		Desc:	Update Workspace and set Allow Search Column to 1 (Yes) or 0 (No)
**			
**		
**
**		Auth: Don Canuso
**		Date: 1/19/2011
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		2/17/11		dcanuso				Adding Workspace Template
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

IF EXISTS (SELECT 1 FROM dbo.Workspace WHERE WorkspaceID = @WorkspaceID AND ContainsOCI = 1 AND @AllowSearch = 1)
	BEGIN
		/*
			Workspace can not allow search if OCI is true 
		*/
				SET @ErrorMessage =   'The Workspace with ID ' + CAST (@WorkspaceID AS VARCHAR (10)) + ' contains OCI information.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END
ELSE
	BEGIN			
		UPDATE dbo.Workspace
			SET AllowSearch = @AllowSearch,
				ContainsTemplate = @ContainsTemplate
		WHERE
			WorkspaceID = @WorkspaceID
	END
GO