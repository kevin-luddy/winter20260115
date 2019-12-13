IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateSystemEmail]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateSystemEmail];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[updateSystemEmail]
(
@EmailID int,
@DefaultOn bit,
@ForcedOn bit = NULL
)
AS
/******************************************************************************
**		 
**		Name:	[updateSystemEmail]
**		Desc:	Updates the System Email (Default On and Forced On)
**			
**
**		Auth: Timothy I. Wilson
**		Date: 3/29/2018
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

Update dbo.EmailLU
	SET DefaultOn = @DefaultOn,
		ForcedOn = @ForcedOn
	WHERE
		EmailID = @EmailID
GO