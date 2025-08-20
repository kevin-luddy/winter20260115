EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.12';
GO

-- 8/13/2025	RJ Anzalone (ranzalon),	PROPH-1877 - Update to UpsertBOE SP to clear Task Authors when removed from BOE Author/Subcontract Author role