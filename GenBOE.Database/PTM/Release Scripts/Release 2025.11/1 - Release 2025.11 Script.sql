EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.11';
GO

-- Author: Tim Wilson
-- PROPH-3244 - 11/25/2025
-- Overhaul PTM Dashboard Report -- Store SSRS Xml in DB

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReportXmlData]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[ReportXmlData](
		Nonce VARCHAR(40) NOT NULL,
		[Xml] VARCHAR(MAX) NOT NULL,
		UpdateDT Datetime2(7) NOT NULL,
	)
END
GO

