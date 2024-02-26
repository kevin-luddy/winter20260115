IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getLaborType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getLaborType];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getLaborType]
(
	@WorkspaceID int
)
AS
/******************************************************************************
**		 
**		Name: getLaborType
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
**		10/14/15	mbasquil			BOEJ-342 Resource level WBS/CLIN
**		4/25/2017	twilson3			BOEJ-2121 Project Map updates
**		5/22/2017	Dusan				BOEJ-2181 Add Add/Delete column
**		8/17/2017	Dusan				BOEJ-2469 Add Old Resource (2.16.1)
**		10/2/2017	twilson3			BOEJ-2520 Cleanup DB, remove old ProjectMap columns
**		1/28/24		e302876  			PROPH-1492 ADD BRC to Copy BOEs, Copy WS, Archive/Restore
*******************************************************************************/
SET NOCOUNT ON 
/*DECLARE @WorkspaceID int=1239*/

SELECT 
	[LT].[BOELaborTypeID], 
	[LT].[UpdateDT], 
	[LT].[ResourceID], 
	[LT].[PerformingOrganizationID], 
	[LT].[BOELaborTypeStartDate], 
	[LT].[BOELaborTypeEndDate], 
	[LT].[SpreadCurveID],
	 [LT].[PercentSpread], 
	 [LT].[ValueSpread], 
	 [LT].[BOETaskElementID], 
	 [LT].[SpreadTypeID], 
	 [LT].[PercentSpreadLocked], 
	 [LT].[HourSpreadLocked],
	 [LT].WBSID,
	 [LT].CLINID,
	 [B].[BOEID],
	 [LT].[CanOffload],
	 [LT].[BRCResourceID]
FROM [dbo].[BOELaborType] AS LT
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