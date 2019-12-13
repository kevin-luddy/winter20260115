IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getLaborSpread]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getLaborSpread];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getLaborSpread]
(
	@WorkspaceID int
)
AS
/******************************************************************************
**		 
**		Name: getLaborSpread
**		Desc: Returns Workspace BOE Labor Types
**			
**		
**
**		Auth: Don Canuso
**		Date: 5/23/14
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

/*DECLARE @WorkspaceID int=1239*/

SELECT 
	[LS].[BOELaborSpreadID], 
	[LS].[BOELaborTypeID], 
	[LS].[LaborSpreadDate], 
	[LS].[LaborSpreadValue], 	 
	[B].[BOEID]
FROM [dbo].[BOELaborSpread] AS [LS]
INNER JOIN	
	(SELECT [BOELaborTypeID], [BOETaskElementID] FROM [dbo].[BOELaborType]) LT 
		ON [LS].[BOELaborTypeID] = [LT].[BOELaborTypeID]
INNER JOIN 
	(SELECT [BOETaskElementID], [BOEID] FROM [dbo].[BOETaskElement]) TE 
		ON [LT].[BOETaskElementID] = [TE].[BOETaskElementID]
INNER JOIN 
	(
		SELECT [BOEID] FROM [dbo].[BOE]
		WHERE [WorkspaceID] = @WorkspaceID
	) B 
		ON [TE].[BOEID] = [B].[BOEID]

GO