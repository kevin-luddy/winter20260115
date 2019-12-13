IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteODCSpreadByODCTypeID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteODCSpreadByODCTypeID];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteODCSpreadByODCTypeID]
(
@ODCTypeID int
)
AS
/******************************************************************************
**		 
**		Name: [deleteODCSpreadByODCTypeID]
**		Desc: Delete BOE Labor Spread row in table using ODCTypeID
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

DELETE FROM dbo.ODCSpread WHERE	ODCTypeID = @ODCTypeID

GO