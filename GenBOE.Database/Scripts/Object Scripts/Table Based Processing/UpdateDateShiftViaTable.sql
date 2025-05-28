-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateDateShiftviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].updateDateShiftviaTableParameter;
GO

-- Drop the existing table type if it exists
IF  EXISTS (SELECT 1 FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_DateShift' AND ss.name = N'dbo')
    DROP TYPE [dbo].[TT_DateShift];
GO

CREATE TYPE [dbo].[TT_DateShift] AS TABLE(
    [Level] int,
    [Id] int,
    [StartDate] date NULL,
    [EndDate] date NULL,
    [UpdateDate] datetime2,
    [OrderID] int NOT NULL
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
**		Desc: Update date shifts for workspace, BOE, BOE task element, CLIN, and BOELaborType.
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
**		[5/19/25]	[e405721]			Added update for BOELaborType (Resource Type) table
*******************************************************************************/
SET NOCOUNT ON
DECLARE @UpdateDT datetime2
SET @UpdateDT = GETDATE()

/**
* Date Shift Level Values: 
*
*   NotSet = 0,
*   Workspace = 1,
*   CLINCollection = 2,     (OBE Not Used)
*   CLIN = 3,
*   BOE = 4,
*   Task = 5,
*   Travel = 6,             (OBE Not Used)
*   Labor = 7
*/

-- Update the Workspace table
UPDATE w
SET 
    ContractStartDate = ds.StartDate,
    ContractEndDate = ds.EndDate,
    UpdateDT = @UpdateDT
FROM [dbo].[Workspace] w
INNER JOIN @DateShifts ds ON w.WorkspaceID = ds.Id AND ds.[Level] = 1 AND w.UpdateDT = ds.UpdateDate;

-- Update the BOE table
UPDATE b
SET 
    BOEStartDate = ds.StartDate,
    BOEEndDate = ds.EndDate,
    UpdateDT = @UpdateDT
FROM [dbo].[BOE] b
INNER JOIN @DateShifts ds ON b.BOEID = ds.Id AND ds.[Level] = 4 AND b.UpdateDT = ds.UpdateDate;

-- Update the BOETaskElement table
UPDATE bt
SET 
    TaskStartDate = ds.StartDate,
    TaskEndDate = ds.EndDate,
    UpdateDT = @UpdateDT
FROM [dbo].[BOETaskElement] bt
INNER JOIN @DateShifts ds ON bt.BOETaskElementID = ds.Id AND ds.[Level] = 5 AND bt.UpdateDT = ds.UpdateDate;

-- Update the CLIN table
UPDATE c
SET 
    CLINStartDate = ds.StartDate,
    CLINEndDate = ds.EndDate,
    UpdateDT = @UpdateDT
FROM [dbo].[CLIN] c
INNER JOIN @DateShifts ds ON c.CLINID = ds.Id AND ds.[Level] = 3 AND c.UpdateDT = ds.UpdateDate;

-- Update the BOELaborType table (Resource Type)
UPDATE blt
SET 
    BOELaborTypeStartDate = ds.StartDate,
    BOELaborTypeEndDate = ds.EndDate,
    UpdateDT = @UpdateDT
FROM [dbo].[BOELaborType] blt
INNER JOIN @DateShifts ds ON blt.BOELaborTypeID = ds.Id AND ds.[Level] = 7 AND blt.UpdateDT = ds.UpdateDate;

IF @@ERROR = 0
	SELECT 
		T.Id, 
		@UpdateDT AS UpdateDT 
	FROM @DateShifts T
	ORDER BY T.OrderID
GO