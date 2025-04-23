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
