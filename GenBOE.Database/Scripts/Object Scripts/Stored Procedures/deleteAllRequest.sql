IF OBJECT_ID('dbo.deleteAllRequests', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[deleteAllRequests];
GO
/****** Object:  StoredProcedure [dbo].[deleteAllRequests]    Script Date: 4/21/2025 9:19:14 AM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[deleteAllRequests]
AS
/******************************************************************************
**		 
**		Name: deleteRequest
**		Desc: delete all records in the Current Request table
**			
**		
**
**		Auth: Yemi Oyetoro
**		Date: 04/14/2025
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 
			
TRUNCATE TABLE [dbo].CurrentRequest
GO