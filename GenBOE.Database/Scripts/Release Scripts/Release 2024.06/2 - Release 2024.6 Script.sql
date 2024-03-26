EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.6';
GO

/*
	3/25/2025 e374897
	UPDATE to make workspaces that are 1:1 with a tracking number set to Current
*/

UPDATE [genBOESpace_UAT].[dbo].[Workspace]
SET CurrentPTMWorkspace = 1
WHERE [TrackingNumber] IN 
(
	SELECT [TrackingNumber]
	FROM [genBOESpace_UAT].[dbo].[Workspace]
	GROUP BY [TrackingNumber]
	HAVING COUNT([WorkspaceId]) = 1
);
GO