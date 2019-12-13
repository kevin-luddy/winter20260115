IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkspace]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkspace];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

/****** Object:  StoredProcedure [dbo].[deleteWorkspace]    Script Date: 8/27/2014 10:07:08 AM ******/
CREATE PROCEDURE [dbo].[deleteWorkspace]
(
@WorkspaceID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteWorkspace]
**		Desc: Delete Workspace 
**			
**		
**
**		Auth: Don Canuso
**		Date: 01/10/12
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		8/21/13		dcanuso				workspace can have multiple 
**										contract types
**		8/27/14		dcanuso				Image Story
**		10/3/14		dcanuso				Image Story Removal
**		12/15/17	twilson3			BOEJ-2248 Remove Labor Rates
**		6/29/2018	twilson3			BOEJ-3551 New Homepage Table
*******************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDT FROM [dbo].[Workspace] WHERE WorkspaceID = @WorkspaceID ) = @UpdateDT
		BEGIN
		
			DELETE FROM [dbo].[WorkspaceContractTypeXREF] 
			WHERE
				WorkspaceID = @WorkspaceID

			DELETE FROM [dbo].[WorkspaceUserXREF]
			WHERE
				WorkspaceID = @WorkspaceID
		
			DELETE FROM dbo.Workspace
			WHERE
				WorkspaceID = @WorkspaceID
	
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Workspace with ID ' + CAST(@WorkspaceID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO