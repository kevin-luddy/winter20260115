IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteSumOfBOEByOrdinaryVariableID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteSumOfBOEByOrdinaryVariableID];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteSumOfBOEByOrdinaryVariableID]
(
@OrdinaryVariableID int
)
AS
/******************************************************************************
**		 
**		Name: 
**		Desc: Deletes the SumOfBOE_OrdinaryVariableXREF rows for OrdinaryVariableID
**			
**		
**
**		Auth: Don Canuso
**		Date: 3/17/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
*******************************************************************************/

SET NOCOUNT ON

DELETE FROM [dbo].[SumOfBOE_OrdinaryVariableXREF]
WHERE OrdinaryVariableID = @OrdinaryVariableID

GO