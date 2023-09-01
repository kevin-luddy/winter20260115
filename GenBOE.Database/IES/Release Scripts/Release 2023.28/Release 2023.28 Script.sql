EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2023.28';
GO

/****** Object:  Table [dbo].[DisclosureTypeLU]    Script Date: 8/29/2023 3:42:52 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DisclosureTypeLU](
	[DisclosureTypeID] [int] IDENTITY(1, 1) NOT NULL,
	[DisclosureType] [varchar](50) NOT NULL,
 CONSTRAINT [PK_DisclosureTypeLU] PRIMARY KEY CLUSTERED 
(
	[DisclosureTypeID] ASC
)WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

/*** Generate Insert for DisclosureTypeLU ***/
SET IDENTITY_INSERT DisclosureTypeLU ON

INSERT INTO DisclosureTypeLU 
    (DisclosureTypeID, DisclosureType)
VALUES 
	(1, 'Legacy Space'),
	(2, '1LMX')

SET IDENTITY_INSERT DisclosureTypeLU OFF

GO

/*** Alter Table RateCode to include DisclosureType ID ***/
ALTER TABLE [dbo].[RateCode]
ADD [DisclosureTypeId] INT NULL
DEFAULT (1)

GO

/*** ALTER Table RateCode to ADD Foreign Key Constraint on newly added Column ***/
ALTER TABLE [dbo].[RateCode] WITH CHECK 
ADD CONSTRAINT FK_RateCode_DisclosureTypeLU
FOREIGN KEY (DisclosureTypeID) REFERENCES [dbo].[DisclosureTypeLU] ([DisclosureTypeID])

GO

ALTER TABLE [dbo].[RateCode] CHECK CONSTRAINT [FK_RateCode_DisclosureTypeLU]

/*** UPDATE Existing Table Records to Have 'Legacy Space' Value which is 1 ***/
GO

UPDATE RateCode SET DisclosureTypeID = 1

GO