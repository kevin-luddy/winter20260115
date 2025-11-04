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
@ContractEndDate date,
@IsValid bit
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

    SET @IsValid = 1; -- Assume valid initially

    BEGIN TRY
	    -- Check for any CLINs that fall outside of the Workspace Contract Start and End date
	    DECLARE @CLINViolations TABLE
        (
            CLINID          INT,
            CLINStartDate   DATE,
            CLINEndDate     DATE,
            StartDateTooEarly  BIT,
            EndDateTooLate     BIT
        );

        INSERT INTO @CLINViolations (CLINID, CLINStartDate, CLINEndDate, StartDateTooEarly, EndDateTooLate)
        SELECT
            C.CLINID,
            C.CLINStartDate,
            C.CLINEndDate,
            CASE WHEN C.CLINStartDate < @ContractStartDate THEN 1 ELSE 0 END,
            CASE WHEN C.CLINEndDate   > @ContractEndDate   THEN 1 ELSE 0 END
        FROM dbo.CLIN AS C
        WHERE C.WorkspaceID = @WorkspaceID
          AND C.CLINStartDate IS NOT NULL
          AND C.CLINEndDate   IS NOT NULL
          AND (C.CLINStartDate < @ContractStartDate
               OR C.CLINEndDate  > @ContractEndDate);

        -- Check the CLIN violations
        IF EXISTS (SELECT 1 FROM CLINViolations)
        BEGIN
            RAISERROR (
                'CLIN date validation failed for Workspace',
                16, 
                1
            );
        END

        -- Pull the list of CLINs in Workspace
        DECLARE @ValidCLIN TABLE
        (
            CLINID        INT PRIMARY KEY,
            CLINStartDate DATE,
            CLINEndDate   DATE
        );

        INSERT INTO @ValidCLIN (CLINID, CLINStartDate, CLINEndDate)
        SELECT
            C.CLINID,
            C.CLINStartDate,
            C.CLINEndDate
        FROM dbo.CLIN AS C
        WHERE C.WorkspaceID = @WorkspaceID
          AND C.CLINStartDate IS NOT NULL
          AND C.CLINEndDate   IS NOT NULL;

        -- Check for all BOEs under the valid CLINs
        DECLARE @BOEViolations TABLE
        (
            CLINID          INT,
            CLINStartDate   DATE,
            CLINEndDate     DATE,
            BOEID           INT,
            BOEStartDate    DATE,
            BOEEndDate      DATE,
            BOEStartTooEarly BIT,
            BOEEndTooLate    BIT
        );

        -- Find any BOE that don't fall within the CLIN
        INSERT INTO @BOEViolations (CLINID, CLINStartDate, CLINEndDate, BOEID, BOEStartDate, BOEEndDate, BOEStartTooEarly, BOEEndTooLate)
        SELECT
            V.CLINID,
            V.CLINStartDate,
            V.CLINEndDate,
            B.BOEID,
            B.BOEStartDate,
            B.BOEEndDate,
            CASE WHEN B.BOEStartDate < V.CLINStartDate THEN 1 ELSE 0 END,
            CASE WHEN B.BOEEndDate   > V.CLINEndDate   THEN 1 ELSE 0 END
        FROM   @ValidCLIN                AS V
        INNER JOIN dbo.WBS_CLIN_BOE_XREF AS X
                ON V.CLINID = X.CLINID
        INNER JOIN dbo.BOE               AS B
                ON X.BOEID = B.BOEID
        WHERE  B.BOEStartDate IS NOT NULL
          AND  B.BOEEndDate   IS NOT NULL
          AND (B.BOEStartDate < V.CLINStartDate
               OR B.BOEEndDate   > V.CLINEndDate);

        -- Check the BOE violations
        IF EXISTS (SELECT 1 FROM BOEViolations)
        BEGIN
            RAISERROR (
                'BOE date validation failed for Workspace',
                16, 
                1
            );
        END

        -- Pull the list of valid BOEs in the workspace
        DECLARE @ValidBOE TABLE
        (
            BOEID        INT PRIMARY KEY,
            BOEStartDate DATE,
            BOEEndDate   DATE
        );

        INSERT INTO @ValidBOE (BOEID, BOEStartDate, BOEEndDate)
        SELECT
            B.BOEID,
            B.BOEStartDate,
            B.BOEEndDate
        FROM dbo.BOE AS B
        WHERE B.WorkspaceID = @WorkspaceID
          AND B.BOEStartDate IS NOT NULL
          AND B.BOEEndDate   IS NOT NULL;

        -- Check for invalid BOETaskElements under the valid BOEs
        DECLARE @BOETaskElementViolations TABLE
        (
            BOEID               INT,
            BOEStartDate        DATE,
            BOEEndDate          DATE,
            BOETaskElementID    INT,
            TaskStartDate       DATE,
            TaskEndDate         DATE,
            TaskStartTooEarly   BIT,
            TaskEndTooLate      BIT
        );

        INSERT INTO @BOETaskElementViolations (
            BOEID, BOEStartDate, BOEEndDate, BOETaskElementID, 
            TaskStartDate, TaskEndDate, TaskStartTooEarly, TaskEndTooLate
        )
        SELECT
            V.BOEID,
            V.BOEStartDate,
            V.BOEEndDate,
            BTE.BOETaskElementID,
            BTE.TaskStartDate,
            BTE.TaskEndDate,
            CASE WHEN BTE.TaskStartDate < V.BOEStartDate THEN 1 ELSE 0 END,
            CASE WHEN BTE.TaskEndDate   > V.BOEEndDate   THEN 1 ELSE 0 END
        FROM @ValidBOE AS V
        INNER JOIN dbo.BOETaskElement AS BTE
            ON V.BOEID = BTE.BOEID
        WHERE BTE.TaskElementTypeID = 1  -- Labor Type
          AND BTE.TaskStartDate IS NOT NULL
          AND BTE.TaskEndDate IS NOT NULL
          AND (BTE.TaskStartDate < V.BOEStartDate
               OR BTE.TaskEndDate > V.BOEEndDate);

        -- Check the BOETaskElement violations
        IF EXISTS (SELECT 1 FROM @BOETaskElementViolations)
        BEGIN
            RAISERROR (
                'BOE Task Elements date validation failed for Workspace',
                16, 
                1
            );
        END

        -- Pull the list of valid BOETaskElements
        DECLARE @ValidBOETaskElement TABLE
        (
            BOETaskElementID INT PRIMARY KEY,
            TaskStartDate    DATE,
            TaskEndDate      DATE
        );

        INSERT INTO @ValidBOETaskElement (BOETaskElementID, TaskStartDate, TaskEndDate)
        SELECT
            BTE.BOETaskElementID,
            BTE.TaskStartDate,
            BTE.TaskEndDate
        FROM dbo.BOETaskElement AS BTE
        WHERE BTE.TaskElementTypeID = 1
          AND BTE.TaskStartDate IS NOT NULL
          AND BTE.TaskEndDate IS NOT NULL
          AND BTE.BOEID IN (SELECT BOEID FROM @ValidBOE);

        -- Check for BOELaborType dates outside their parent BOETaskElement dates
        DECLARE @BOELaborTypeViolations TABLE
        (
            BOETaskElementID       INT,
            TaskStartDate          DATE,
            TaskEndDate            DATE,
            BOELaborTypeID         INT,
            BOELaborTypeStartDate  DATE,
            BOELaborTypeEndDate    DATE,
            LaborStartTooEarly     BIT,
            LaborEndTooLate        BIT
        );

        INSERT INTO @BOELaborTypeViolations (
            BOETaskElementID, TaskStartDate, TaskEndDate, 
            BOELaborTypeID, BOELaborTypeStartDate, BOELaborTypeEndDate, 
            LaborStartTooEarly, LaborEndTooLate
        )
        SELECT
            V.BOETaskElementID,
            V.TaskStartDate,
            V.TaskEndDate,
            BLT.BOELaborTypeID,
            BLT.BOELaborTypeStartDate,
            BLT.BOELaborTypeEndDate,
            CASE WHEN BLT.BOELaborTypeStartDate < V.TaskStartDate THEN 1 ELSE 0 END,
            CASE WHEN BLT.BOELaborTypeEndDate   > V.TaskEndDate   THEN 1 ELSE 0 END
        FROM @ValidBOETaskElement AS V
        INNER JOIN dbo.BOELaborType AS BLT
            ON V.BOETaskElementID = BLT.BOETaskElementID
        WHERE BLT.BOELaborTypeStartDate IS NOT NULL
          AND BLT.BOELaborTypeEndDate IS NOT NULL
          AND (BLT.BOELaborTypeStartDate < V.TaskStartDate
               OR BLT.BOELaborTypeEndDate > V.TaskEndDate);

        -- Check the BOELaborType violations
        IF EXISTS (SELECT 1 FROM @BOELaborTypeViolations)
        BEGIN
            RAISERROR (
                'BOE Labor Type date validation failed for Workspace',
                16, 
                1
            );
        END
    END TRY
    BEGIN CATCH
        SET @IsValid = 0;
        THROW;
    END CATCH
END
GO
