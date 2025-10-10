IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateMetricAnalysisReport]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[CreateMetricAnalysisReport];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CreateMetricAnalysisReport] 
(
	@TypeID int /*0 = Totals/1=Details*/
)
AS
/******************************************************************************
**		 
**		Name: [CreateMetricAnalysisReport]
**		Desc: Creates Metric Analysis Report for SSRS
**
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		7/11/23		twilson3			PROPH-911 Add PTM Number and LOB
**      10/10/25	twilson3			proph-3024 Add Current Workspace
*******************************************************************************/
SET NOCOUNT ON

/* For SSRS to Run Correctly*/
IF @TypeID = -1
BEGIN 
SELECT 
	(SELECT COUNT(*) AS [Total Workspaces] FROM dbo.Workspace) AS [Total Workspaces],
	(SELECT COUNT(*) AS [Total BOEs] FROM dbo.BOE) AS [Total BOEs],
	(
		SELECT SUM (A.[Total Tasks]) AS [Total Tasks]
		FROM 
		(
		SELECT COUNT(*) AS [Total Tasks] FROM [dbo].[BOETaskElement]
		UNION ALL
		SELECT COUNT(*) AS [Total Tasks] FROM [dbo].[MaterialTaskElement]
		UNION ALL
		SELECT COUNT(*) AS [Total Tasks] FROM [dbo].[ODCTaskElement]
		UNION ALL
		SELECT COUNT(*) AS [Total Tasks] FROM [dbo].[TravelTripTaskElement]
		) A
	) AS [Total Tasks],

	   W.[WorkspaceName]
      ,W.[WorkspaceShortName]
	  ,W.[TrackingNumber]
	  ,L.[LineOfBusinessName]
	  ,CAST (W.[UpdateDT] AS [DATE]) AS [Latest Activity Date]

      /*,W.[WorkspaceStateID]*/
	  ,S.WorkspaceState

	  ,	CASE 
			WHEN Copy.TargetWorkspaceID IS NULL THEN CAST (WSH.UpdateDT AS Date)
			WHEN Copy.TargetWorkspaceID IS NOT NULL THEN CAST (Copy.[CreateDate] AS Date)
			ELSE CAST (WSH.UpdateDT AS Date)
		END AS [Workspace Creation Date] 

      /*,W.[CostVolumeLeadPricerUserID]*/
	  ,IsNULL (CostVolume.DisplayName, 'Not Available') AS [CostVolumeLead/Pricer]

      /*,W.[CreatedByETIUserID]*/
	  ,IsNULL (CreatedBy.DisplayName, 'Not Available') AS [Workspace Created By]

	  ,BOECount.[Number of BOEs]

	  ,TaskCount.[Total Tasks] AS [Number of Tasks]
	  ,CASE
	     WHEN W.CurrentPTMWorkspace = 1 THEN 'Yes'
		 ELSE 'No' 
	   END AS [CurrentWorkspace]

FROM [dbo].[Workspace] W 
	INNER JOIN dbo.WorkspaceStateLU S ON W.WorkspaceStateID = S.WorkspaceStateID
	INNER JOIN [dbo].[WorkspaceStateHistory] WSH ON W.[WorkspaceID] = WSH.WorkspaceID
	LEFT OUTER JOIN dbo.ETIuser CostVolume ON W.CostVolumeLeadPricerUserID = CostVolume.ETIUserID
	LEFT OUTER JOIN dbo.LineOfBusiness L ON W.[LineOfBusinessID] = L.[LineOfBusinessID]
	LEFT OUTER JOIN dbo.ETIuser CreatedBy ON W.[CreatedByETIUserID] = CreatedBy.ETIUserID
	INNER JOIN 
		(
			SELECT COUNT (*) AS [Number of BOEs] , WorkspaceID 
				FROM dbo.BOE 
			GROUP BY WorkspaceID
		) BOECount	ON W.WorkspaceID = BOECount.WorkspaceID
/*	INNER JOIN 
		(
			SELECT	COUNT (*) AS [Number of Tasks], B.WorkspaceID 
				FROM dbo.BOETaskElement TE 
						INNER  JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				GROUP BY B.WorkspaceID
		) TaskCount ON W.WorkspaceID = TaskCount.WorkspaceID
*/
	INNER JOIN 
		(
			SELECT SUM (A.[Total Tasks]) AS [Total Tasks], A.WorkspaceID 
			FROM 
				(
					SELECT COUNT(*) AS [Total Tasks], B.WorkspaceID 
					FROM [dbo].[BOETaskElement] TE 
						INNER  JOIN dbo.BOE B ON TE.BOEID = B.BOEID
					GROUP BY B.WorkspaceID

					UNION ALL
					
					SELECT COUNT(*) AS [Total Tasks], B.WorkspaceID 
					FROM [dbo].[MaterialTaskElement] TE 
						INNER  JOIN dbo.BOE B ON TE.BOEID = B.BOEID
					GROUP BY B.WorkspaceID
					
					UNION ALL

					SELECT COUNT(*) AS [Total Tasks], B.WorkspaceID 
					FROM [dbo].[ODCTaskElement]   TE 
						INNER  JOIN dbo.BOE B ON TE.BOEID = B.BOEID
					GROUP BY B.WorkspaceID

					UNION ALL

					SELECT COUNT(*) AS [Total Tasks], B.WorkspaceID 
					FROM [dbo].[TravelTripTaskElement] TE 
						INNER  JOIN dbo.BOE B ON TE.BOEID = B.BOEID
					GROUP BY B.WorkspaceID
		
				) A GROUP BY A.WorkspaceID
			) TaskCount ON W.WorkspaceID = TaskCount.WorkspaceID

	LEFT OUTER JOIN [dbo].[WorkspaceCopyMetric] Copy ON W.WorkspaceID = Copy.TargetWorkspaceID
WHERE 
	(
	[CurrentWorkspaceStateID] = 0 
	) 
END

IF @TypeID = 0 
BEGIN

SELECT 
	(SELECT COUNT(*) AS [Total Workspaces] FROM dbo.Workspace) AS [Total Workspaces],
	(SELECT COUNT(*) AS [Total BOEs] FROM dbo.BOE) AS [Total BOEs],
	(
		SELECT SUM (A.[Total Tasks]) AS [Total Tasks]
		FROM 
		(
		SELECT COUNT(*) AS [Total Tasks] FROM [dbo].[BOETaskElement]
		UNION ALL
		SELECT COUNT(*) AS [Total Tasks] FROM [dbo].[MaterialTaskElement]
		UNION ALL
		SELECT COUNT(*) AS [Total Tasks] FROM [dbo].[ODCTaskElement]
		UNION ALL
		SELECT COUNT(*) AS [Total Tasks] FROM [dbo].[TravelTripTaskElement]
		) A
	) AS [Total Tasks]

END

IF @TypeID = 1
BEGIN
SELECT 
	   W.[WorkspaceName]
      ,W.[WorkspaceShortName]

	  ,CAST (W.[UpdateDT] AS [DATE]) AS [Latest Activity Date]

      /*,W.[WorkspaceStateID]*/
	  ,S.WorkspaceState

	  ,	CASE 
			WHEN Copy.TargetWorkspaceID IS NULL THEN CAST (WSH.UpdateDT AS Date)
			WHEN Copy.TargetWorkspaceID IS NOT NULL THEN CAST (Copy.[CreateDate] AS Date)
			ELSE CAST (WSH.UpdateDT AS Date)
		END AS [Workspace Creation Date] 

      /*,W.[CostVolumeLeadPricerUserID]*/
	  ,IsNULL (CostVolume.DisplayName, 'Not Available') AS [CostVolumeLead/Pricer]

      /*,W.[CreatedByETIUserID]*/
	  ,IsNULL (CreatedBy.DisplayName, 'Not Available') AS [Workspace Created By]

	  ,IsNull(BOECount.[Number of BOEs], 0) AS [Number of BOEs]

	  ,IsNull(TaskCount.[Total Tasks], 0) AS [Number of Tasks]
	  ,W.[TrackingNumber]
	  ,L.[LineOfBusinessName]
	  ,CASE
	     WHEN W.CurrentPTMWorkspace = 1 THEN 'Yes'
		 ELSE 'No' 
	   END AS [CurrentWorkspace]
	  
FROM [dbo].[Workspace] W 
	INNER JOIN dbo.WorkspaceStateLU S ON W.WorkspaceStateID = S.WorkspaceStateID
	INNER JOIN [dbo].[WorkspaceStateHistory] WSH ON W.[WorkspaceID] = WSH.WorkspaceID
	LEFT OUTER JOIN dbo.ETIuser CostVolume ON W.CostVolumeLeadPricerUserID = CostVolume.ETIUserID
	LEFT OUTER JOIN dbo.LineOfBusiness L ON W.[LineOfBusinessID] = L.[LineOfBusinessID]
	LEFT OUTER JOIN dbo.ETIuser CreatedBy ON W.[CreatedByETIUserID] = CreatedBy.ETIUserID
	LEFT OUTER JOIN 
		(
			SELECT COUNT (*) AS [Number of BOEs] , WorkspaceID 
				FROM dbo.BOE 
			GROUP BY WorkspaceID
		) BOECount	ON W.WorkspaceID = BOECount.WorkspaceID
/*	LEFT OUTER JOIN 
		(
			SELECT	COUNT (*) AS [Number of Tasks], B.WorkspaceID 
				FROM dbo.BOETaskElement TE 
						INNER  JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				GROUP BY B.WorkspaceID
		) TaskCount ON W.WorkspaceID = TaskCount.WorkspaceID
*/
	LEFT OUTER JOIN 
		(
			SELECT SUM (A.[Total Tasks]) AS [Total Tasks], A.WorkspaceID 
			FROM 
				(
					SELECT COUNT(*) AS [Total Tasks], B.WorkspaceID 
					FROM [dbo].[BOETaskElement] TE 
						INNER  JOIN dbo.BOE B ON TE.BOEID = B.BOEID
					GROUP BY B.WorkspaceID

					UNION ALL
					
					SELECT COUNT(*) AS [Total Tasks], B.WorkspaceID 
					FROM [dbo].[MaterialTaskElement] TE 
						INNER  JOIN dbo.BOE B ON TE.BOEID = B.BOEID
					GROUP BY B.WorkspaceID
					
					UNION ALL

					SELECT COUNT(*) AS [Total Tasks], B.WorkspaceID 
					FROM [dbo].[ODCTaskElement]   TE 
						INNER  JOIN dbo.BOE B ON TE.BOEID = B.BOEID
					GROUP BY B.WorkspaceID

					UNION ALL

					SELECT COUNT(*) AS [Total Tasks], B.WorkspaceID 
					FROM [dbo].[TravelTripTaskElement] TE 
						INNER  JOIN dbo.BOE B ON TE.BOEID = B.BOEID
					GROUP BY B.WorkspaceID
		
				) A GROUP BY A.WorkspaceID
			) TaskCount ON W.WorkspaceID = TaskCount.WorkspaceID

	LEFT OUTER JOIN [dbo].[WorkspaceCopyMetric] Copy ON W.WorkspaceID = Copy.TargetWorkspaceID
WHERE 
	(
	[CurrentWorkspaceStateID] = 0 
	) 

END

GO