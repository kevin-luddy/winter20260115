IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertWorkspaceFavorite]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertWorkspaceFavorite];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertWorkspaceFavorite]
(
@WorkspaceID int,
@ETIUserID int,
@IsFavorite bit
)
AS
/******************************************************************************
**		 
**		Name: [upsertWorkspaceFavorite]
**		Desc: Insert/Update data for Workspace Favorites
**			
**		
**
**		Auth: Timothy I. Wilson
**		Date: 6/28/2018
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @UpdateDT datetime2
SET @UpdateDT = GetDate()
DECLARE @WorkspaceUserXREFID int
SELECT @WorkspaceUserXREFID = (SELECT TOP 1 WorkspaceUserID FROM [dbo].[WorkspaceUserXREF] WHERE [WorkspaceID] = @WorkspaceID AND [ETIUserId] = @ETIUserID)

IF @WorkspaceUserXREFID IS NULL /*Insert Record*/
	BEGIN
		DECLARE @InsertedXREF AS Table (WorkspaceUserID int)

		INSERT INTO [dbo].[WorkspaceUserXREF]
				   ([WorkspaceID]
				   ,[ETIUserId]
				   ,[IsFavorite]
				   ,[UpdateDT])          
			 OUTPUT inserted.WorkspaceUserID INTO @InsertedXREF
			 VALUES
				   (@WorkspaceID
				   ,@ETIUserID
				   ,@IsFavorite
				   ,@UpdateDT)

			SELECT @WorkspaceUserXREFID = WorkspaceUserID FROM @InsertedXREF
	
	END
ELSE
	BEGIN
			UPDATE [dbo].[WorkspaceUserXREF]
			   SET [IsFavorite] = @IsFavorite
				  ,[UpdateDT] = @UpdateDT
			WHERE 
				WorkspaceUserID = @WorkspaceUserXREFID
	END

IF @@ERROR = 0
	SELECT @WorkspaceUserXREFID AS WorkspaceUserXREFID
GO