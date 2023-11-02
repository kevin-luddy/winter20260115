EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2023.36';

GO
/**** ALTER TABLE Section to ADD Column IsDisclosureStatementAdequate ****/
ALTER TABLE dbo.Section
ADD IsDisclosureStatementAdequate bit
DEFAULT (0)

GO

GO
/**** ALTER TABLE Section to ADD Column NonComplianceNotification ****/
ALTER TABLE dbo.Section
ADD NonComplianceNotification bit
DEFAULT (0)

GO