EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.01';

/* Author: Hazrat Rafiqzadah
Story: SLMX_POLM_PROPH-2453 */

-- 01/27/2025 [Hazrat] - PROPH-2453 PPR&D Edit Section Modifications
-- Stored procs updated:
-- upsertSection.sql
-- copyRevision.sql

-- 02/11/2025 [Thomas Asuncion] - PROPH-2454 Added new column RevisionSegmentId to RDSBDocumentInformation table
-- Tables updated:
-- RDSBDocumentInformation
-- Added new LU table RevisionSegmentLU

USE IES_DEV

/**** PROPH-2453 Edit Sections Modifications ****/
/********** ALTER TABLE: dbo.Section ************/
/* Altering existing Columns to be of Type CORE */

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Section]') AND name = N'SectionContainsCasbDisclosure')
BEGIN
    EXEC sp_rename 'dbo.Section.SectionContainsCasbDisclosure', 'SectionContainsCasbDisclosureCore', 'COLUMN';
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Section]') AND name = N'SectionContainsNonCompliance')
BEGIN
    EXEC sp_rename 'dbo.Section.SectionContainsNonCompliance', 'SectionContainsNonComplianceCore', 'COLUMN';
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Section]') AND name = N'IsDisclosureStatementAdequate')
BEGIN
    EXEC sp_rename 'dbo.Section.IsDisclosureStatementAdequate', 'IsDisclosureStatementAdequateCore', 'COLUMN';
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Section]') AND name = N'NonComplianceNotification')
BEGIN
    EXEC sp_rename 'dbo.Section.NonComplianceNotification', 'NonComplianceNotificationCore', 'COLUMN';
END
GO

/* Alter Table to add new columns that align to Service */

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Section]') AND name = N'SectionContainsCasbDisclosureService')
BEGIN
    ALTER TABLE [dbo].[Section] ADD SectionContainsCasbDisclosureService bit NOT NULL CONSTRAINT [Default_SectionContainsCasbDisclosureService] DEFAULT 0
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Section]') AND name = N'SectionContainsNonComplianceService')
BEGIN
    ALTER TABLE [dbo].[Section] ADD SectionContainsNonComplianceService bit NOT NULL CONSTRAINT [Default_SectionContainsNonComplianceService] DEFAULT 0
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Section]') AND name = N'IsDisclosureStatementAdequateService')
BEGIN
    ALTER TABLE [dbo].[Section] ADD IsDisclosureStatementAdequateService bit NULL
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Section]') AND name = N'NonComplianceNotificationService')
BEGIN
    ALTER TABLE [dbo].[Section] ADD NonComplianceNotificationService bit NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RevisionSegmentLU]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RevisionSegmentLU]
    (
        [ID] INT PRIMARY KEY,
        [Description] nvarchar(50) NOT NULL
    );

    INSERT INTO [dbo].[RevisionSegmentLU] ([ID], [Description])
    VALUES
        (0, ''),
        (1, 'Core'),
        (2, 'Services');
END
GO

-- Check if the RevisionSegmentId column exists in the RDSBDocumentInformation table.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RDSBDocumentInformation]') AND name = N'RevisionSegmentId')
BEGIN
    ALTER TABLE [dbo].[RDSBDocumentInformation] 
    ADD [RevisionSegmentId] INT NOT NULL CONSTRAINT [Default_RevisionSegmentId] DEFAULT 0;
END
GO

-- Check if the foreign key constraint exists in the RDSBDocumentInformation table.
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'[dbo].[RDSBDocumentInformation]') AND name = N'FK_RDSBDocumentInformation_RevisionSegmentLU')
BEGIN
    ALTER TABLE [dbo].[RDSBDocumentInformation]
    ADD CONSTRAINT [FK_RDSBDocumentInformation_RevisionSegmentLU]
    FOREIGN KEY ([RevisionSegmentId])
    REFERENCES [dbo].[RevisionSegmentLU]([ID]);
END
GO