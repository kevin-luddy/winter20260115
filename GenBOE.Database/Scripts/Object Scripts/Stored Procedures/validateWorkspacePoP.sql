IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[validateWorkspacePoP]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[validateWorkspacePoP];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[validateWorkspacePoP]
(
@WorkspaceID int,
@ContractStartDate date,
@ContractEndDate date
)
AS
/******************************************************************************
**		 
**		Name: [validateWorkspacePoP]
**		Desc: Validate all children objects in Workspace to be within PoP 
**			
**		Auth: Tommy Lee
**		Date: 10/30/25
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		10/30/25		e374897				Init
*******************************************************************************/
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Normalize contract dates
        SET @ContractStartDate = CAST(@ContractStartDate AS DATE);
        SET @ContractEndDate = CAST(@ContractEndDate AS DATE);

        -- Check CLIN dates against contract dates
        DECLARE @CLINs TABLE
        (
            CLINID        INT PRIMARY KEY,
            CLINStartDate DATE,
            CLINEndDate   DATE,
            IsValid       BIT
        );

        INSERT INTO @CLINs (CLINID, CLINStartDate, CLINEndDate, IsValid)
        SELECT
            C.CLINID,
            CAST(C.CLINStartDate AS DATE),
            CAST(C.CLINEndDate AS DATE),
            CASE 
                WHEN C.CLINStartDate IS NULL AND C.CLINEndDate IS NULL THEN 1
                WHEN (CAST(C.CLINStartDate AS DATE) >= @ContractStartDate AND CAST(C.CLINEndDate AS DATE) <= @ContractEndDate) THEN 1 ELSE 0 
            END
        FROM dbo.CLIN AS C
        WHERE C.WorkspaceID = @WorkspaceID;

        -- Check if any CLINs are invalid
        IF EXISTS (SELECT 1 FROM @CLINs WHERE IsValid = 0)
        BEGIN
            RAISERROR('CLIN date validation failed for Workspace', 16, 1);
        END

        -- Check BOE dates against parent CLIN dates
        DECLARE @BOEs TABLE
        (
            BOEID        INT PRIMARY KEY,
            BOEStartDate DATE,
            BOEEndDate   DATE,
            IsValid      BIT
        );

        -- BOEs tied to a CLIN
        INSERT INTO @BOEs (BOEID, BOEStartDate, BOEEndDate, IsValid)
        SELECT
            B.BOEID,
            CAST(B.BOEStartDate AS DATE),
            CAST(B.BOEEndDate AS DATE),
            CASE WHEN (CAST(B.BOEStartDate AS DATE) >= C.CLINStartDate AND CAST(B.BOEEndDate AS DATE) <= C.CLINEndDate)
                    THEN 1 ELSE 0 END
        FROM dbo.BOE B
        JOIN dbo.WBS_CLIN_BOE_XREF X ON B.BOEID = X.BOEID
        JOIN @CLINs C ON X.CLINID = C.CLINID
        WHERE B.WorkspaceID = 32864
            AND B.BOEStartDate IS NOT NULL
            AND B.BOEEndDate IS NOT NULL
	        AND C.CLINStartDate IS NOT NULL
	        AND C.CLINEndDate IS NOT NULL;

        -- BOEs tied to a CLIN, but CLIN Start and End Date are NULL so checked against the Workspace dates
        INSERT INTO @BOEs (BOEID, BOEStartDate, BOEEndDate, IsValid)
        SELECT
            B.BOEID,
            CAST(B.BOEStartDate AS DATE),
            CAST(B.BOEEndDate AS DATE),
            CASE WHEN (CAST(B.BOEStartDate AS DATE) >= CAST('2022-02-15' AS DATE) AND CAST(B.BOEEndDate AS DATE) <= CAST('2034-06-15' AS DATE))
                    THEN 1 ELSE 0 END
        FROM dbo.BOE B
        JOIN dbo.WBS_CLIN_BOE_XREF X ON B.BOEID = X.BOEID
        JOIN @CLINs C ON X.CLINID = C.CLINID
        WHERE B.WorkspaceID = 32864
            AND B.BOEStartDate IS NOT NULL
            AND B.BOEEndDate IS NOT NULL
	        AND C.CLINStartDate IS NULL
	        AND C.CLINEndDate IS NULL
	        AND NOT EXISTS(
		        SELECT 1 FROM @BOEs AS BE
		        WHERE B.BOEID = BE.BOEID
	        );

        -- BOEs not tied to a CLIN, so checked against the Workspace dates
        INSERT INTO @BOEs (BOEID, BOEStartDate, BOEEndDate, IsValid)
        SELECT
            B.BOEID,
            CAST(B.BOEStartDate AS DATE),
            CAST(B.BOEEndDate AS DATE),
            CASE WHEN (CAST(B.BOEStartDate AS DATE) >= CAST('2022-02-15' AS DATE) AND CAST(B.BOEEndDate AS DATE) <= CAST('2034-06-15' AS DATE))
                    THEN 1 ELSE 0 END
        FROM dbo.BOE AS B
        WHERE B.WorkspaceID = 32864
            AND B.BOEStartDate IS NOT NULL
            AND B.BOEEndDate IS NOT NULL
            AND NOT EXISTS (
                SELECT 1 
                FROM @BOEs AS X
                WHERE X.BOEID = B.BOEID
        );

        -- Check if any BOEs are invalid
        IF EXISTS (SELECT 1 FROM @BOEs WHERE IsValid = 0)
        BEGIN
            RAISERROR('BOE date validation failed for Workspace', 16, 1);
        END

        -- Check BOETaskElement dates against parent BOE dates
        DECLARE @BOETaskElements TABLE
        (
            BOETaskElementID INT PRIMARY KEY,
            TaskStartDate    DATE,
            TaskEndDate      DATE,
            IsValid          BIT
        );

        INSERT INTO @BOETaskElements (BOETaskElementID, TaskStartDate, TaskEndDate, IsValid)
        SELECT
            BTE.BOETaskElementID,
            CAST(BTE.TaskStartDate AS DATE),
            CAST(BTE.TaskEndDate AS DATE),
            CASE WHEN (CAST(BTE.TaskStartDate AS DATE) >= B.BOEStartDate AND CAST(BTE.TaskEndDate AS DATE) <= B.BOEEndDate)
                 THEN 1 ELSE 0 END
        FROM dbo.BOETaskElement BTE
        JOIN @BOEs B ON BTE.BOEID = B.BOEID
        WHERE BTE.TaskElementTypeID = 1  -- Labor Type
          AND BTE.TaskStartDate IS NOT NULL
          AND BTE.TaskEndDate IS NOT NULL;

        -- Check if any BOETaskElements are invalid
        IF EXISTS (SELECT 1 FROM @BOETaskElements WHERE IsValid = 0)
        BEGIN
            RAISERROR('BOE Task Elements date validation failed for Workspace', 16, 1);
        END

        -- Check BOELaborType dates against parent BOETaskElement dates
        DECLARE @BOELaborTypes TABLE
        (
            BOELaborTypeID       INT,
            BOELaborTypeStartDate DATE,
            BOELaborTypeEndDate   DATE,
            IsValid               BIT
        );

        INSERT INTO @BOELaborTypes (BOELaborTypeID, BOELaborTypeStartDate, BOELaborTypeEndDate, IsValid)
        SELECT
            BLT.BOELaborTypeID,
            CAST(BLT.BOELaborTypeStartDate AS DATE),
            CAST(BLT.BOELaborTypeEndDate AS DATE),
            CASE WHEN (CAST(BLT.BOELaborTypeStartDate AS DATE) >= BTE.TaskStartDate AND CAST(BLT.BOELaborTypeEndDate AS DATE) <= BTE.TaskEndDate)
                 THEN 1 ELSE 0 END
        FROM dbo.BOELaborType BLT
        JOIN @BOETaskElements BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
        WHERE BLT.BOELaborTypeStartDate IS NOT NULL
          AND BLT.BOELaborTypeEndDate IS NOT NULL;

        -- Check if any BOELaborTypes are invalid
        IF EXISTS (SELECT 1 FROM @BOELaborTypes WHERE IsValid = 0)
        BEGIN
            RAISERROR('BOE Labor Type date validation failed for Workspace', 16, 1);
        END

        -- If we got here, everything is valid
        SELECT 1 AS IsValid;
    END TRY
    BEGIN CATCH
        SELECT 0 AS IsValid;
        THROW;
    END CATCH
END
GO
