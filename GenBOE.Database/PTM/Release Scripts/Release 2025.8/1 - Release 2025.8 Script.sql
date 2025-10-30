EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.8';
GO

/******************************************************************************
** This only servers the purpose of allowing the full release script to generated
** with required changes for Stored Procedures and Views
** 10/30/25		e403038				PROPH-3406 Label Modifications => Current Planned renamed to ScheduledActual and Current Scheduled renamed to Planned

*/