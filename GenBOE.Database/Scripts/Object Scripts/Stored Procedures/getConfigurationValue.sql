IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getConfigurationValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getConfigurationValue];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[getConfigurationValue]
(
	@configurationKey varchar(256)
)
AS
/******************************************************************************
**		 
**		Name: [getConfigurationValue]
**		Desc: Gets a configuration value based upon a configuration key.
**
**		Auth: pattoncr
**		Date: 5/11/17
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		5/11/17		pattoncr			BOEJ-2151 - Initial Creation
******************************************************************************/
SET NOCOUNT ON 

	select ConfigurationValue from [dbo].[Configuration] where ConfigurationKey = @configurationKey;

GO