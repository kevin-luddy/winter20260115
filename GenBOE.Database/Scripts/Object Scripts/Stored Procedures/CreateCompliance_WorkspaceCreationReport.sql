IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateCompliance_WorkspaceCreationReport]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[CreateCompliance_WorkspaceCreationReport];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CreateCompliance_WorkspaceCreationReport]
(
 	@StartDate datetime = NULL,
 	@EndDate datetime = NULL
)
AS

SET NOCOUNT ON 

/*
Complaince  Request:

Include the following:
Create date
Workspace id
Workspace name
Workspace shortname
*/

/*
	Start Date should be 12 AM and End Date should be 11:59 PM
	Start Date should default to January of the current year
	End Date should defualt to last day of the previous month
*/


/*
NOTE 2/23/15
Workspace copies are have the original workspace creation date
Now that we have workspace copy, we can use the date the workspace was created via copy
*/

IF @StartDate IS NULL 
	/*First Day of Year*/
	SET @StartDate = DATEADD(year, DATEDIFF(year, -1, getdate()) - 1, 0) 
ELSE
	SET @StartDate = @StartDate + '00:00:00.000'

IF @EndDate IS NULL 
	/*LastDayPreviousMonthWithTimeStamp*/
	SET @EndDate = DATEADD(ss, -1, DATEADD(month, DATEDIFF(month, 0, getdate()), 0)) 
ELSE
	SET @EndDate = @EndDate + '23:59:59.000'

/*
--TESTING: 
SELECT @StartDate, @EndDate
*/

/*
TESTING:
execute  [dbo].[CreateCompliance_WorkspaceCreationReport] '01-2-2014', '12-5-2014'
execute  [dbo].[CreateCompliance_WorkspaceCreationReport]  NULL, NULL
*/

SELECT DISTINCT 
		CASE 
			WHEN Copy.TargetWorkspaceID IS NULL THEN CAST (WSH.UpdateDT AS Date)
			WHEN Copy.TargetWorkspaceID IS NOT NULL THEN CAST (Copy.[CreateDate] AS Date)
			ELSE CAST (WSH.UpdateDT AS Date)
		END AS [Workspace Creation Date], 
	W.WorkspaceID AS [Workspace ID], 
	W.WorkspaceName  AS [Workspace Name], 
	W.WorkspaceShortName AS [Workspace Short Name]
FROM [dbo].[Workspace] W
	INNER JOIN [dbo].[WorkspaceStateHistory] WSH ON W.[WorkspaceID] = WSH.WorkspaceID
	/*INNER JOIN [dbo].[WorkspaceStateLU] WS ON WSH.[CurrentWorkspaceStateID] = WS.[WorkspaceStateID]*/
	LEFT OUTER JOIN [dbo].[WorkspaceCopyMetric] Copy ON W.WorkspaceID = Copy.TargetWorkspaceID
WHERE 
	(
	[CurrentWorkspaceStateID] = 0 AND 
	WSH.UpdateDT >= @StartDate AND 
	WSH.UpdateDT <= @EndDate
	) OR
	(
	Copy.CreateDate >= @StartDate AND 
	Copy.CreateDate <= @EndDate
	)

GO