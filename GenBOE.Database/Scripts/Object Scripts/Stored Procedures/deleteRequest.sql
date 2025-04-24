IF OBJECT_ID('dbo.deleteRequest', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[deleteRequest];
GO
/****** Object:  StoredProcedure [dbo].[deleteRequest]    Script Date: 4/21/2025 9:17:13 AM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[deleteRequest]
(
	@RequestTypeID int,
	@Ntid varchar(20)
)
AS
/******************************************************************************
**		 
**		Name: deleteRequest
**		Desc: delete a record in the Current Request table
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

	DELETE FROM [dbo].CurrentRequest
    WHERE RequestTypeID = @RequestTypeID
    AND Ntid = @Ntid
GO