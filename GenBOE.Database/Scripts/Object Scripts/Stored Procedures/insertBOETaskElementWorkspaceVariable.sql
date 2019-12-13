IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertBOETaskElementWorkspaceVariable]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertBOETaskElementWorkspaceVariable];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[insertBOETaskElementWorkspaceVariable]
(
@BOETaskElementID int,
@WorkspaceVariableID varchar (100)
)
AS
/******************************************************************************
**		 
**		Name: [insertBOETaskElementWorkspaceVariable]
**		Desc:	Inserts Workspace Variable data into BOE Task Element
**				Handles Deletes
**				No Updates required
**			
**		
**
**		Auth: Don Canuso
**		Date: 11/15/10
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @insertedBOETaskElementWorkspaceVariableXREF AS Table (BOETaskWSVarID int)

DECLARE @tempWorkspaceVariable TABLE
	(
		WorkspaceVariableID int
	)

IF @WorkspaceVariableID IS NOT NULL
	BEGIN 
		WHILE @WorkspaceVariableID IS NOT NULL
			BEGIN
				INSERT INTO @tempWorkspaceVariable (WorkspaceVariableID)
				SELECT LEFT(@WorkspaceVariableID,(CHARINDEX(',',@WorkspaceVariableID)-1))
				
				SET @WorkspaceVariableID = RIGHT(@WorkspaceVariableID, (LEN (@WorkspaceVariableID) - CHARINDEX(',',@WorkspaceVariableID)))

				IF LEN(@WorkspaceVariableID) = 0 SET @WorkspaceVariableID = NULL
			END
END			


/*Deletes*/
DELETE FROM dbo.BOETaskElementWorkspaceVariableXREF
FROM dbo.BOETaskElementWorkspaceVariableXREF BW
	LEFT OUTER JOIN @tempWorkspaceVariable WS ON BW.WorkspaceVariableID = WS.WorkspaceVariableID
WHERE	BW.BOETaskElementID = @BOETaskElementID AND
		BW.WorkspaceVariableID IS NOT NULL AND /*IN CURRENT TABLE*/
		WS.WorkspaceVariableID IS NULL /*NO LONGER IN THE LIST SO DELETED*/	
		
/*INSERT*/		
INSERT INTO [dbo].[BOETaskElementWorkspaceVariableXREF]
([BOETaskElementID],[WorkspaceVariableID])
SELECT @BOETaskElementID, WS.WorkspaceVariableID
FROM @tempWorkspaceVariable WS
	LEFT OUTER JOIN dbo.BOETaskElementWorkspaceVariableXREF BW ON 
			WS.WorkspaceVariableID = BW.WorkspaceVariableID AND
			BW.BOETaskElementID = @BOETaskElementID
WHERE	
		BW.WorkspaceVariableID IS NULL AND /*NOT IN THE CURRENT TABLE*/
		WS.WorkspaceVariableID IS NOT NULL /*IN THE NEW LIST SO ADDED*/


GO