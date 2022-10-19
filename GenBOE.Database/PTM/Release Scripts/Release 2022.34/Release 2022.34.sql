EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2022.34';
GO

/*
                ## START ##

                10/12/2022 [RJ] - IES-1933 - ACV Utilization Question
*/

IF OBJECT_ID('dbo.CostVolumeToolLU', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CostVolumeToolLU
    (
        [CostVolumeToolID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	    [CostVolumeTool] [varchar](50) NOT NULL,
	    [IsActive] [bit] NOT NULL
    );

    SET IDENTITY_INSERT dbo.CostVolumeToolLU ON;

    INSERT INTO dbo.CostVolumeToolLU (CostVolumeToolID, CostVolumeTool, IsActive)
    VALUES (0, 'None', 1), 
        (1, 'ACV', 1),
        (2, 'Word', 1),
        (3, 'Other', 1),
        (4, 'N/A', 1);
        
    SET IDENTITY_INSERT dbo.CostVolumeToolLU OFF;

    ALTER TABLE dbo.Proposal
        ADD CostVolumeToolID INT NOT NULL DEFAULT 0,
        FOREIGN KEY(CostVolumeToolID) References dbo.CostVolumeToolLU(CostVolumeToolID);
        
    ALTER TABLE dbo.Proposal
        ADD CostVolumeToolName VARCHAR(50) NULL;
END
GO

/*
                ## END ##

                10/12/2022 [RJ] - IES-1933 - ACV Utilization Question
*/