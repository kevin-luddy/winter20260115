EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.14'; 
GO


/*
	## START ##

	1/14/2025 [twilson3] - SLMX_POLM_PROPH-2596 - UCOT database updates
*/

-- Add UCOTFactor Column if it does not exist --
	IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Workspace]') AND name = N'UCOTFactor')
	BEGIN
	    ALTER TABLE [dbo].[Workspace] 
		ADD UCOTFactor decimal(7,2) NOT NULL DEFAULT 1;
	END

	IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[version].[Workspace]') AND name = N'UCOTFactor')
	BEGIN
	    ALTER TABLE [version].[Workspace] 
		ADD UCOTFactor decimal(7,2) NOT NULL DEFAULT 1;
	END


/*
	1/14/2025 [twilson3] - SLMX_POLM_PROPH-2596 - UCOT database updates

	## END ##
*/