IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getOrdinaryVariableByTaskElementID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getOrdinaryVariableByTaskElementID];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getOrdinaryVariableByTaskElementID]
(
@BOETaskElementID int
)
AS
/******************************************************************************
**		 
**		Name: [getOrdinaryVariableByTaskElementID]
**		Desc: Returns the Ordinary Variables Used in a BOE Task Element
**			
**		
**
**		Auth: Don Canuso
**		Date: 10/4/10
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			----------------------------------------
**		3/17/11		dcanuso				New columns added to Workspace Variable Table
**		8/5/11		dcanuso				Added IsPercentage column
*******************************************************************************/
SET NOCOUNT ON 

SELECT	TE.[BOETaskElementID] AS [BOETaskElementID],
		OV.[OrdinaryVariableID] AS [OrdinaryVariableID],
		OV.[OrdinaryVariableName] AS [OrdinaryVariableName],
		OV.[OrdinaryVariableValue] AS [OrdinaryVariableValue],
		OV.[SortByID] AS [SortByID],
		OV.[ValueTypeID] AS [ValueTypeID],
		OV.[IsPercentage] AS [IsPercentage],
		OV.[UpdateDT] AS [UpdateDT]
FROM [dbo].[BOETaskElement] TE 
	INNER JOIN dbo.OrdinaryVariable OV ON TE.BOETaskElementID = OV.BOETaskElementID
WHERE 
	TE.[BOETaskElementID] = @BOETaskElementID

GO