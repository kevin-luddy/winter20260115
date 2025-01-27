EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.01';

/* Author: Hazrat Rafiqzadah
Story: SLMX_POLM_PROPH-2453 */

-- 01/27/2025 [Hazrat] - PROPH-2453 PPR&D Edit Section Modifications
-- Stored procs updated:
-- upsertSection.sql
-- copyRevision.sql


USE IES_DEV

/**** PROPH-2453 Edit Sections Modifications ****/
/********** ALTER TABLE: dbo.Section ************/
/* Altering existing Columns to be of Type CORE */

GO
EXEC sp_rename 'dbo.Section.SectionContainsCasbDisclosure', 'SectionContainsCasbDisclosureCore', 'COLUMN';
GO

GO
EXEC sp_rename 'dbo.Section.SectionContainsNonCompliance', 'SectionContainsNonComplianceCore', 'COLUMN';
GO

GO
EXEC sp_rename 'dbo.Section.IsDisclosureStatementAdequate', 'IsDisclosureStatementAdequateCore', 'COLUMN';
GO

GO
EXEC sp_rename 'dbo.Section.NonComplianceNotification', 'NonComplianceNotificationCore', 'COLUMN';
GO

/* Alter Table to add new columns that align to Service */

GO
ALTER TABLE [dbo].[Section] ADD SectionContainsCasbDisclosureService bit NOT NULL CONSTRAINT [Default_SectionContainsCasbDisclosureService] DEFAULT 0
GO

GO
ALTER TABLE [dbo].[Section] ADD SectionContainsNonComplianceService bit NOT NULL CONSTRAINT [Default_SectionContainsNonComplianceService] DEFAULT 0
GO

GO
ALTER TABLE [dbo].[Section] ADD IsDisclosureStatementAdequateService bit NULL
GO

GO
ALTER TABLE [dbo].[Section] ADD NonComplianceNotificationService bit NULL
GO
