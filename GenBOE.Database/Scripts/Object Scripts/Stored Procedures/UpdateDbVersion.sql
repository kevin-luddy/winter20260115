IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateDbVersion]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[UpdateDbVersion];

GO

CREATE PROCEDURE [dbo].[UpdateDbVersion](@DbVersion VARCHAR(100), @AppVersion VARCHAR(100)) 
AS
/******************************************************************************
**		 
**		Name: [UpdateDbVersion]
**		Desc: Updates the DB version, if necessary
**
**		Auth: Dusan
**		Date: 11/2016
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**
******************************************************************************/
	IF(NOT EXISTS(SELECT 1 FROM [dbo].[BoeDatabaseVersion] WHERE DBVersion = @DbVersion AND AppVersion = @AppVersion))
	BEGIN
		INSERT INTO [dbo].[BoeDatabaseVersion] (DBVersion, AppVersion, UpdateDate)
		VALUES (@DbVersion, @AppVersion, GETDATE())
	END

GO