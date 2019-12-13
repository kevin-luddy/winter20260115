IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteSystemSetting]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteSystemSetting];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteSystemSetting]
(
	@Key		VARCHAR(255)
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteSystemSetting]
	**		Desc:	Delete System Setting values
	**			
	**		
	**
	**		Auth: Greg Brunworth
	**		Date: 6/28/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	*******************************************************************************/
	SET NOCOUNT ON 

	DELETE FROM dbo.SystemSetting
		WHERE [Key] = @Key

GO
