IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteSystemRoleByID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteSystemRoleByID];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteSystemRoleByID]
(
@UpdateDT datetime2,
@SystemUserRoleID int
)
AS
/******************************************************************************
**		 
**		Name: [deleteSystemRoleByID]
**		Desc: Deletes a System User Role from System User Role
**			
**		   
**		
**
**		Auth: Don Canuso
**		Date: 4/23/13
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		11/20/14	dcanuso				New Proposal Set Up role - Renamed to 
**										reuse Product Line Viewer to Role XREF
**		1/12/2017	brunworg			BOEJ-1688 Update PTM SPs to not display technical details to the user
**		6/06/18		brunworg			BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
******************************************************************************/
SET NOCOUNT ON 

DECLARE @ErrorMessage varchar (500)

IF (SELECT UpdateDT FROM dbo.SystemUserRole WHERE SystemUserRoleID = @SystemUserRoleID) = @UpdateDT
	BEGIN
			
		DELETE FROM dbo.LineOfBusinessRoleXREF			
			FROM dbo.LineOfBusinessRoleXREF X
				INNER JOIN dbo.SystemUserRole S ON X.SystemUserRoleID = S.SystemUserRoleID			
		WHERE S.SystemUserRoleID = @SystemUserRoleID
		
	
		DELETE FROM dbo.SystemUserRole
		WHERE SystemUserRoleID = @SystemUserRoleID
	END
ELSE
	BEGIN
		SET @ErrorMessage =   'The System User Role has been updated and is out of sync with the data in your browser.  Please refresh your data.'
		RAISERROR (
			@ErrorMessage, -- Message text.
	        11, -- Severity,/*Severity Changed to 11*/
			1 -- State,
			)
		RETURN
	END
GO