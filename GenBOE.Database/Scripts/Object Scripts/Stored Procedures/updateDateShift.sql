-- Drop the existing stored procedure if it exists
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateDateShift]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[updateDateShift];
GO

-- Drop the existing table type if it exists
IF  EXISTS (SELECT 1 FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_DateShift' AND ss.name = N'dbo')
    DROP TYPE [dbo].[TT_DateShift];
GO

CREATE TYPE [dbo].[TT_DateShift] AS TABLE(
    [Level] nvarchar(50),
    [Id] int,
    [StartDate] date,
    [EndDate] date,
    [BOEStateID] int NULL,
    [UpdateDT] datetime2
);
GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[updateDateShift]
(
    @DateShifts [dbo].[TT_DateShift] READONLY  -- The table-valued parameter containing the date shifts
)
AS
/******************************************************************************
**		 
**		Name: [updateDateShift]
**		Desc: Update date shifts for workspace, BOE, BOE task element, CLIN, and travel trip task element
**			
**		
**
**		Auth: [e405721]
**		Date: [4/17/25]
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		[4/17/25]	[e405721]			Initial creation
*******************************************************************************/
SET NOCOUNT ON 

-- Declare a table variable to hold the date shifts
DECLARE @TT_DateShift TABLE
(
    [Level] nvarchar(50),
    [Id] int,
    [StartDate] date,
    [EndDate] date,
    [BOEStateID] int NULL,
    [UpdateDT] datetime2
);

-- Insert the date shifts from the table-valued parameter into the table variable
INSERT INTO @TT_DateShift SELECT * FROM @DateShifts;

-- Declare a variable to keep track of the current row
DECLARE @CurrentRow TABLE
(
    [Level] nvarchar(50),
    [Id] int,
    [StartDate] date,
    [EndDate] date,
    [BOEStateID] int NULL,
    [UpdateDT] datetime2
);

-- Loop through the date shifts and update the corresponding tables
WHILE EXISTS (SELECT 1 FROM @TT_DateShift)
BEGIN
    -- Select the top date shift from the table variable
    INSERT INTO @CurrentRow
    SELECT TOP 1 
        [Level],
        [Id],
        [StartDate],
        [EndDate],
        [BOEStateID],
        [UpdateDT]
    FROM @TT_DateShift;

    -- Update the corresponding table based on the level
    IF (SELECT [Level] FROM @CurrentRow) = 'Workspace'
    BEGIN
        -- Update the Workspace table
        UPDATE w
        SET 
            ContractStartDate = (SELECT [StartDate] FROM @CurrentRow),
            ContractEndDate = (SELECT [EndDate] FROM @CurrentRow),
            UpdateDT = (SELECT [UpdateDT] FROM @CurrentRow)
        FROM [dbo].[Workspace] w
        WHERE w.WorkspaceID = (SELECT [Id] FROM @CurrentRow) AND w.UpdateDT = (SELECT [UpdateDT] FROM @CurrentRow);
    END
    ELSE IF (SELECT [Level] FROM @CurrentRow) = 'BOE'
    BEGIN
        -- Update the BOE table
        UPDATE b
        SET 
            BOEStartDate = (SELECT [StartDate] FROM @CurrentRow),
            BOEEndDate = (SELECT [EndDate] FROM @CurrentRow),
            BOEStateID = (SELECT [BOEStateID] FROM @CurrentRow),
            UpdateDT = (SELECT [UpdateDT] FROM @CurrentRow)
        FROM [dbo].[BOE] b
        WHERE b.BOEID = (SELECT [Id] FROM @CurrentRow) AND b.UpdateDT = (SELECT [UpdateDT] FROM @CurrentRow);
    END
    ELSE IF (SELECT [Level] FROM @CurrentRow) = 'BOETaskElement'
    BEGIN
        -- Update the BOETaskElement table
        UPDATE bt
        SET 
            TaskStartDate = (SELECT [StartDate] FROM @CurrentRow),
            TaskEndDate = (SELECT [EndDate] FROM @CurrentRow),
            UpdateDT = (SELECT [UpdateDT] FROM @CurrentRow)
        FROM [dbo].[BOETaskElement] bt
        WHERE bt.BOEID = (SELECT [Id] FROM @CurrentRow) AND bt.UpdateDT = (SELECT [UpdateDT] FROM @CurrentRow);
    END
    ELSE IF (SELECT [Level] FROM @CurrentRow) = 'CLIN'
    BEGIN
        -- Update the CLIN table
        UPDATE c
        SET 
            CLINStartDate = (SELECT [StartDate] FROM @CurrentRow),
            CLINEndDate = (SELECT [EndDate] FROM @CurrentRow),
            UpdateDT = (SELECT [UpdateDT] FROM @CurrentRow)
        FROM [dbo].[CLIN] c
        WHERE c.CLINID = (SELECT [Id] FROM @CurrentRow) AND c.UpdateDT = (SELECT [UpdateDT] FROM @CurrentRow);
    END
    ELSE IF (SELECT [Level] FROM @CurrentRow) = 'TravelTripTaskElement'
    BEGIN
        -- Update the TravelTripTaskElement table
        UPDATE tt
        SET 
            TaskStartDate = (SELECT [StartDate] FROM @CurrentRow),
            TaskEndDate = (SELECT [EndDate] FROM @CurrentRow),
            UpdateDT = (SELECT [UpdateDT] FROM @CurrentRow)
        FROM [dbo].[TravelTripTaskElement] tt
        WHERE tt.TravelTripTaskElementID = (SELECT [Id] FROM @CurrentRow) AND tt.UpdateDT = (SELECT [UpdateDT] FROM @CurrentRow);
    END

    -- Delete the current row from the table variable
    DELETE FROM @TT_DateShift
    WHERE [Level] = (SELECT [Level] FROM @CurrentRow) AND [Id] = (SELECT [Id] FROM @CurrentRow);

    -- Clear the current row table
    DELETE FROM @CurrentRow;
END
GO