IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteOrdinaryVariableResourceType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteOrdinaryVariableResourceType];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteOrdinaryVariableResourceType]
(
@OrdinaryVariableID int
)
AS
/******************************************************************************
**		 
**		Name: [deleteOrdinaryVariableResourceType]
**		Desc: deletes Ordinary Variable and Resource Type
**			
**		
**
**		Auth: Don Canuso
**		Date: 09/13/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			----------------------------------------
*******************************************************************************/
SET NOCOUNT ON 
				
DELETE FROM [dbo].[OrdinaryVariableSumVariableResourceTypeXREF]
WHERE [OrdinaryVariableID] = @OrdinaryVariableID

GO