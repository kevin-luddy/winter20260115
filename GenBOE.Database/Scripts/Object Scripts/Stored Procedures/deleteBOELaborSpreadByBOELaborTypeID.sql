IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOELaborSpreadByBOELaborTypeID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOELaborSpreadByBOELaborTypeID];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteBOELaborSpreadByBOELaborTypeID]
(
@BOELaborTypeID int
)
AS
/******************************************************************************
**		 
**		Name: [deleteBOELaborSpreadByBOELaborTypeID]
**		Desc: Delete BOE Labor Spread row in table using BOELaborTypeID
**			
**		
**
**		Auth: Don Canuso
**		Date: 10/13/10
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
*******************************************************************************/
SET NOCOUNT ON 


DELETE FROM dbo.BOELaborSpread
WHERE
	BOELaborTypeID = @BOELaborTypeID

GO