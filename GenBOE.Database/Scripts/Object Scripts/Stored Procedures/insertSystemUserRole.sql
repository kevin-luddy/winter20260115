IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertSystemUserRole]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertSystemUserRole];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertSystemUserRole]
(
	@ETIUserID int,
	@RoleID int, 
	@UpdateDT datetime2(7)
)
AS
/******************************************************************************
**		 
**		Name: insertSystemUserRole
**		Desc: Inserts a record into the System User Role table for DTO
**			
**		
**
**		Auth: Don Canuso
**		Date: 11/16/10
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		4/28/11		dcanuso				Adding Duplicate check with SP rather than 
**										Unique Index
**		7/25/12		dcanuso				WI10138 Dups are being added
**		9/4/12		mbasquil			WI 10910 Added IsNull() checks to properly
**										handle null values
**		11/20/13	dcanuso				Permission Story - Remove ETI Group ID
**		12/4/13		dcanuso				WI 24910
**		8/7/14		dcanuso				Update Date is not coming in from 
**										code so the dates are showing 0001 as 
**										the year.  Inserting SQL date to correct.
*******************************************************************************/
SET NOCOUNT ON 



IF EXISTS (SELECT 1 FROM dbo.BOEPotentialRole WHERE RoleID = 9 AND ETIUserID = @ETIUserID)
BEGIN

	DECLARE	@ErrorMessage varchar (500)

	SET @ErrorMessage =   'You can not add any other roles to a user that has the Subcontractor Author role '
	RAISERROR (
		@ErrorMessage, -- Message text.
        11, -- Severity,/*Severity Changed to 11*/
		1 -- State,
		)
	RETURN
END




IF NOT EXISTS	(SELECT 1 FROM [dbo].[SystemUserRole]
						WHERE	ETIUserID = @ETIUserID AND
								/*IsNull(ETIGroupID, -9999) = IsNull(@ETIGroupID, -9999) AND*/
								RoleID = @RoleID
					)
INSERT INTO [dbo].[SystemUserRole]
           ([ETIUserID]
           /*,[ETIGroupID]*/
           ,[RoleID]
           ,[UpdateDT])
     VALUES
           (
            @ETIUserID,
            /*@ETIGroupID,*/
            @RoleID, 
            GetDate()
            )
GO