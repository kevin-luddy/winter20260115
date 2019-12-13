-- Drop SPs first
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertBOETaskElementWorkspaceVariableviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertBOETaskElementWorkspaceVariableviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_BOETaskElementWorkspaceVariableXREF' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_BOETaskElementWorkspaceVariableXREF];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_BOETaskElementWorkspaceVariableXREF] AS TABLE(
	[BOETaskWSVarID] [int] NOT NULL,
	[BOETaskElementID] [int] NOT NULL,
	[WorkspaceVariableID] [int] NOT NULL,
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[insertBOETaskElementWorkspaceVariableviaTableParameter]
(
@BOETaskElementWorkspaceVariableXREF [dbo].[TT_BOETaskElementWorkspaceVariableXREF] READONLY
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
**      6/4/15      dpalider            BOEJ-126 -> Fix bulk update SPs to handle 
**                                      missing data (moved the ORDER BY OrderID)
**      9/10/15     dpalider			BOEJ-338 -> Fix WS variables not being unlinked 
**										correctly
*******************************************************************************/
SET NOCOUNT ON 

/*Deletes*/
DELETE FROM dbo.BOETaskElementWorkspaceVariableXREF
	FROM dbo.BOETaskElementWorkspaceVariableXREF BW
		LEFT OUTER JOIN @BOETaskElementWorkspaceVariableXREF WS ON 
			BW.WorkspaceVariableID = WS.WorkspaceVariableID AND
			BW.BOETaskElementID = WS.BOETaskElementID
	WHERE	
		BW.BOETaskElementID IN (SELECT DISTINCT BOETaskElementID FROM @BOETaskElementWorkspaceVariableXREF) /* ONLY WANT TO AFFECT BOE TASK ELEMENTS BEING PASSED IN */
		AND BW.BOETaskWSVarID IS NOT NULL /* IN CURRENT TABLE */
		AND WS.BOETaskWSVarID IS NULL /* NO LONGER IN THE LIST SO DELETED */
	
/*INSERT*/		
INSERT INTO [dbo].[BOETaskElementWorkspaceVariableXREF] ([BOETaskElementID],[WorkspaceVariableID])
	SELECT WS.BOETaskElementID, WS.WorkspaceVariableID
		FROM @BOETaskElementWorkspaceVariableXREF WS
			LEFT OUTER JOIN dbo.BOETaskElementWorkspaceVariableXREF BW ON 
					WS.WorkspaceVariableID = BW.WorkspaceVariableID 
					AND WS.BOETaskElementID = BW.BOETaskElementID
		WHERE	
			BW.WorkspaceVariableID IS NULL /* NOT IN THE CURRENT TABLE */
			AND WS.WorkspaceVariableID <> -100 /* IN THE NEW LIST SO ADDED, AND NOT -100, WHICH INDICATES 0 VARIABLES BEING USED */
			AND WS.BOETaskWSVarID < 0 /* New Records have a value of negative */
		ORDER BY OrderID

IF @@ERROR = 0
SELECT BW.[BOETaskWSVarID]
	FROM [dbo].[BOETaskElementWorkspaceVariableXREF] BW
		INNER JOIN @BOETaskElementWorkspaceVariableXREF WS ON 
			BW.WorkspaceVariableID = WS.WorkspaceVariableID AND
			BW.BOETaskElementID = WS.BOETaskElementID
;
GO