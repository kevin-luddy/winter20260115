IF OBJECT_ID('dbo.insertRequest', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[insertRequest];
GO

/****** Object:  StoredProcedure [dbo].[insertRequest]    Script Date: 4/21/2025 9:15:37 AM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertRequest]
(
	@RequestTypeID int,
	@Ntid varchar(20),
    @NewRequestID INT OUTPUT
)
AS
/******************************************************************************
**		 
**		Name: insertRequest
**		Desc: Inserts a record into the Current Request table for DTO
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
IF EXISTS (SELECT 1 FROM dbo.CurrentRequest WHERE RequestTypeID = @RequestTypeID AND Ntid = @Ntid)
BEGIN

	DECLARE	@ErrorMessage varchar (500)

	SET @ErrorMessage =   'You cannot add another request of this request type'
	RAISERROR (
		@ErrorMessage, -- Message text.
        11, -- Severity,/*Severity Changed to 11*/
		1 -- State,
		)
	RETURN
END
					
INSERT INTO [dbo].CurrentRequest
           (RequestTypeID
           ,Ntid
           ,[UpdateDT])
     VALUES
           (
            @RequestTypeID,
            @Ntid, 
            GetDate()
            )

SET @NewRequestID = SCOPE_IDENTITY();
GO