IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateBOETaskElementLaborTypeWarningFlag]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateBOETaskElementLaborTypeWarningFlag];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[updateBOETaskElementLaborTypeWarningFlag]
(
@BOETaskElementID int,
@LaborTypeWarningFlag bit
)
AS
/******************************************************************************
**		 
**		Name:	[updateBOETaskElementLaborTypeWarningFlag]
**		Desc:	Update BOETaskElement Sets LaborTypeWarningFlag Column to 1 (Yes) or 0 (No)
**			
**		
**
**		Auth: Don Canuso
**		Date: 3/17/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

UPDATE [dbo].[BOETaskElement]
   SET [LaborTypeWarningFlag] = @LaborTypeWarningFlag
 WHERE BOETaskElementID = @BOETaskElementID


GO