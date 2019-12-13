IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkspace_ProcessSoftDelete]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkspace_ProcessSoftDelete];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteWorkspace_ProcessSoftDelete]
AS

DECLARE @vWorkspace TABLE
(
	WorkspaceID int,
	Processed bit DEFAULT (0)
)	
INSERT INTO @vWorkspace 
SELECT WorkspaceID, 0
FROM dbo.Workspace
WHERE 
IsDeleted = 1 AND
getDate() > = DateAdd (dd, 60, DateDeleted) 

DECLARE @WorkspaceID int 
WHILE EXISTS (SELECT 1 FROM @vWorkspace WHERE Processed = 0)
BEGIN
	SELECT TOP 1 @WorkspaceID = WorkspaceID FROM @vWorkspace
	WHERE Processed = 0
	
	EXECUTE [dbo].[deleteFullWorkspace] @WorkspaceID

	UPDATE @vWorkspace
	SET Processed = 1
	WHERE 
		Processed = 0 AND
		WorkspaceID = @WorkspaceID
END

GO