IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateDateShift]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[updateDateShift];
GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[updateDateShift]
(
    @WorkspaceID int,
    @BOEID int,
    @BOEStateID int,
    @BOETaskElementID int,
    @CLINID int,
    @TravelTripTaskElementID int,
    @ContractStartDate date,
    @ContractEndDate date,
    @UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [updateDateShift]
**		Desc: Update date shifts for workspace, BOE, BOE task element, CLIN, and travel trip task element
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

BEGIN
    -- Update workspace
    IF @WorkspaceID IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Workspace] WHERE WorkspaceID = @WorkspaceID)
        BEGIN
            RAISERROR ('The workspace with ID %d does not exist.', 11, 1, @WorkspaceID)
            RETURN
        END

        IF (SELECT UpdateDT FROM [dbo].[Workspace] WHERE WorkspaceID = @WorkspaceID) = @UpdateDT
        BEGIN
            UPDATE [dbo].[Workspace]
            SET 
                ContractStartDate = @ContractStartDate,
                ContractEndDate = @ContractEndDate,
                UpdateDT = GETDATE()
            WHERE 
                WorkspaceID = @WorkspaceID
        END
        ELSE
        BEGIN
            RAISERROR ('The workspace with ID %d has been updated and is out of sync with the data in your browser.  Please refresh your data.', 11, 1, @WorkspaceID)
            RETURN
        END
    END

    -- Update BOE
    IF @BOEID IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[BOE] WHERE BOEID = @BOEID)
        BEGIN
            RAISERROR ('The BOE with ID %d does not exist.', 11, 1, @BOEID)
            RETURN
        END
    
        IF (SELECT UpdateDT FROM [dbo].[BOE] WHERE BOEID = @BOEID) = @UpdateDT
        BEGIN
            UPDATE [dbo].[BOE]
            SET 
                BOEStateID = @BOEStateID,
                BOEStartDate = @ContractStartDate,
                BOEEndDate = @ContractEndDate,
                UpdateDT = GETDATE()
            WHERE 
                BOEID = @BOEID
        END
        ELSE
        BEGIN
            RAISERROR ('The BOE with ID %d has been updated and is out of sync with the data in your browser.  Please refresh your data.', 11, 1, @BOEID)
            RETURN
        END
    END

    -- Update BOE task element
    IF @BOETaskElementID IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[BOETaskElement] WHERE BOETaskElementID = @BOETaskElementID)
        BEGIN
            RAISERROR ('The BOE task element with ID %d does not exist.', 11, 1, @BOETaskElementID)
            RETURN
        END

        IF (SELECT UpdateDT FROM [dbo].[BOETaskElement] WHERE BOETaskElementID = @BOETaskElementID) = @UpdateDT
        BEGIN
            UPDATE [dbo].[BOETaskElement]
            SET 
                TaskStartDate = @ContractStartDate,
                TaskEndDate = @ContractEndDate,
                UpdateDT = GETDATE()
            WHERE 
                BOETaskElementID = @BOETaskElementID
        END
        ELSE
        BEGIN
            RAISERROR ('The BOE task element with ID %d has been updated and is out of sync with the data in your browser.  Please refresh your data.', 11, 1, @BOETaskElementID)
            RETURN
        END
    END

    -- Update CLIN
    IF @CLINID IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[CLIN] WHERE CLINID = @CLINID)
        BEGIN
            RAISERROR ('The CLIN with ID %d does not exist.', 11, 1, @CLINID)
            RETURN
        END

        IF (SELECT UpdateDT FROM [dbo].[CLIN] WHERE CLINID = @CLINID) = @UpdateDT
        BEGIN
            UPDATE [dbo].[CLIN]
            SET 
                CLINStartDate = @ContractStartDate,
                CLINEndDate = @ContractEndDate,
                UpdateDT = GETDATE()
            WHERE 
                CLINID = @CLINID
        END
        ELSE
        BEGIN
            RAISERROR ('The CLIN with ID %d has been updated and is out of sync with the data in your browser.  Please refresh your data.', 11, 1, @CLINID)
            RETURN
        END
    END

    -- Update travel trip task element
    IF @TravelTripTaskElementID IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[TravelTripTaskElement] WHERE TravelTripTaskElementID = @TravelTripTaskElementID)
        BEGIN
            RAISERROR ('The travel trip task element with ID %d does not exist.', 11, 1, @TravelTripTaskElementID)
            RETURN
        END

        IF (SELECT UpdateDT FROM [dbo].[TravelTripTaskElement] WHERE TravelTripTaskElementID = @TravelTripTaskElementID) = @UpdateDT
        BEGIN
            UPDATE [dbo].[TravelTripTaskElement]
            SET 
                TaskStartDate = @ContractStartDate,
                TaskEndDate = @ContractEndDate,
                UpdateDT = GETDATE()
            WHERE 
                TravelTripTaskElementID = @TravelTripTaskElementID
        END
        ELSE
        BEGIN
            RAISERROR ('The travel trip task element with ID %d has been updated and is out of sync with the data in your browser.  Please refresh your data.', 11, 1, @TravelTripTaskElementID)
            RETURN
        END
    END
END
GO

IF @@ERROR = 0
    PRINT 'Update successful'
GO