EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.06';
GO

/* Author: Oyeyemi Oyetoro
Story: SLMX_POLM_PROPH-2877 */


-- Drop CurrentRequest first due to FK dependency
IF OBJECT_ID('dbo.CurrentRequest', 'U') IS NOT NULL
    DROP TABLE [dbo].[CurrentRequest];
GO

-- Drop RequestTypeLU
IF OBJECT_ID('dbo.RequestTypeLU', 'U') IS NOT NULL
    DROP TABLE [dbo].[RequestTypeLU];
GO

-- Create RequestTypeLU
CREATE TABLE [dbo].[RequestTypeLU](
    [RequestTypeID] [int] NOT NULL,
	[Name] [varchar](20) NOT NULL,
    CONSTRAINT [PK_RequestTypeLU] PRIMARY KEY CLUSTERED ([RequestTypeID] ASC)
);
GO

-- Insert initial values
INSERT INTO [dbo].[RequestTypeLU] ([RequestTypeID], [Name])
VALUES 
	(1, 'Calculate Actuals'),
    (2, 'Export BOE');
GO

-- Create CurrentRequest
CREATE TABLE [dbo].[CurrentRequest](
    [CurrentRequestID] [int] IDENTITY(1,1) NOT NULL,
	[Ntid] [varchar](20) NOT NULL,
    [RequestTypeID] [int] NOT NULL,
    [UpdateDT] [datetime] NOT NULL,
    CONSTRAINT [PK_CurrentRequest] PRIMARY KEY CLUSTERED ([CurrentRequestID] ASC),
    CONSTRAINT [UQ_CurrentRequest] UNIQUE NONCLUSTERED ([Ntid], [RequestTypeID])
);
GO

-- Add foreign key constraint
ALTER TABLE [dbo].[CurrentRequest]
ADD CONSTRAINT [FK_CurrentRequest_RequestTypeLU]
FOREIGN KEY ([RequestTypeID]) REFERENCES [dbo].[RequestTypeLU] ([RequestTypeID]);
GO

-- Add non-clustered index on Ntid
CREATE NONCLUSTERED INDEX [IX_CurrentRequest_Ntid]
ON [dbo].[CurrentRequest] ([Ntid]);
GO


-- dbo.InsertRequest
IF OBJECT_ID('dbo.insertRequest', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[insertRequest];
GO

/****** Object:  StoredProcedure [dbo].[insertRequest]    Script Date: 4/21/2025 9:15:37 AM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertRequest]
(
	@RequestTypeID int,
	@Ntid varchar(20),
    @NewRequestID INT OUTPUT
)
AS
/******************************************************************************
**		 
**		Name: insertRequest
**		Desc: Inserts a record into the Current Request table for DTO
**			
**		
**
**		Auth: Yemi Oyetoro
**		Date: 04/14/2025
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 
IF EXISTS (SELECT 1 FROM dbo.CurrentRequest WHERE RequestTypeID = @RequestTypeID AND Ntid = @Ntid)
BEGIN

	DECLARE	@ErrorMessage varchar (500)

	SET @ErrorMessage =   'You cannot add another request of this request type'
	RAISERROR (
		@ErrorMessage, -- Message text.
        11, -- Severity,/*Severity Changed to 11*/
		1 -- State,
		)
	RETURN
END
					
INSERT INTO [dbo].CurrentRequest
           (RequestTypeID
           ,Ntid
           ,[UpdateDT])
     VALUES
           (
            @RequestTypeID,
            @Ntid, 
            GetDate()
            )

SET @NewRequestID = SCOPE_IDENTITY();
GO


-- dbo.deleteRequest
IF OBJECT_ID('dbo.deleteRequest', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[deleteRequest];
GO
/****** Object:  StoredProcedure [dbo].[deleteRequest]    Script Date: 4/21/2025 9:17:13 AM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[deleteRequest]
(
	@RequestTypeID int,
	@Ntid varchar(20)
)
AS
/******************************************************************************
**		 
**		Name: deleteRequest
**		Desc: delete a record in the Current Request table
**			
**		
**
**		Auth: Yemi Oyetoro
**		Date: 04/14/2025
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

	DELETE FROM [dbo].CurrentRequest
    WHERE RequestTypeID = @RequestTypeID
    AND Ntid = @Ntid
GO


-- dbo.deleteAllRequests
IF OBJECT_ID('dbo.deleteAllRequests', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[deleteAllRequests];
GO
/****** Object:  StoredProcedure [dbo].[deleteAllRequests]    Script Date: 4/21/2025 9:19:14 AM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[deleteAllRequests]
AS
/******************************************************************************
**		 
**		Name: deleteRequest
**		Desc: delete all records in the Current Request table
**			
**		
**
**		Auth: Yemi Oyetoro
**		Date: 04/14/2025
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 
			
TRUNCATE TABLE [dbo].CurrentRequest
GO

