-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateDateShiftviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].updateDateShiftviaTableParameter;
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

CREATE PROCEDURE [dbo].[updateDateShiftviaTableParameter]
(
    @DateShifts [dbo].[TT_DateShift] READONLY  -- The table-valued parameter containing the date shifts
)
AS
/******************************************************************************
**		 
**		Name: [updateDateShiftviaTableParameter]
**		Desc: Update date shifts for workspace, BOE, BOE task element, and CLIN.
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

-- Update the Workspace table
UPDATE w
SET 
    ContractStartDate = ds.StartDate,
    ContractEndDate = ds.EndDate,
    UpdateDT = ds.UpdateDT
FROM [dbo].[Workspace] w
INNER JOIN @TT_DateShift ds ON w.WorkspaceID = ds.Id AND ds.Level = 'Workspace' AND w.UpdateDT = ds.UpdateDT;

-- Update the BOE table
UPDATE b
SET 
    BOEStartDate = ds.StartDate,
    BOEEndDate = ds.EndDate,
    BOEStateID = ds.BOEStateID,
    UpdateDT = ds.UpdateDT
FROM [dbo].[BOE] b
INNER JOIN @TT_DateShift ds ON b.BOEID = ds.Id AND ds.Level = 'BOE' AND b.UpdateDT = ds.UpdateDT;

-- Update the BOETaskElement table
UPDATE bt
SET 
    TaskStartDate = ds.StartDate,
    TaskEndDate = ds.EndDate,
    UpdateDT = ds.UpdateDT
FROM [dbo].[BOETaskElement] bt
INNER JOIN @TT_DateShift ds ON bt.TaskID = ds.Id AND ds.Level = 'BOETaskElement' AND bt.UpdateDT = ds.UpdateDT;

-- Update the CLIN table
UPDATE c
SET 
    CLINStartDate = ds.StartDate,
    CLINEndDate = ds.EndDate,
    UpdateDT = ds.UpdateDT
FROM [dbo].[CLIN] c
INNER JOIN @TT_DateShift ds ON c.CLINID = ds.Id AND ds.Level = 'CLIN' AND c.UpdateDT = ds.UpdateDT;
GO