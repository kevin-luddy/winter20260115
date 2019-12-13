IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getMyFunctionalTree]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getMyFunctionalTree];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getMyFunctionalTree]
(
@NTID varchar (1000),
@genTracUserID INT
)
AS
/******************************************************************************
**          
**          Name: [getMyFunctionalTree]
**          Desc: Mimics My Functional Tree (Including Me)
**                
**          
**
**          Auth: Don Canuso
**          Date: 4/11/14
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                       Description:
**          --------    --------                -------------------------------
**			4/11/14		dcanuso					Initial creation.
**			11/29/17	pattoncr				Remove domain.
******************************************************************************/
SET NOCOUNT ON 

/*
TESTING

DECLARE @NTID varchar (100) = 'sipiak'
DECLARE @genTracUserID INT
*/


IF @genTracUserID IS NULL AND @NTID IS NOT NULL
BEGIN
	SELECT @genTracUserID = UserID 
	FROM dbo.genTRACUser 
	WHERE 
		NTID = @NTID
END



IF @genTracUserID IS NOT NULL AND @NTID IS NULL
BEGIN
	SELECT 
		@NTID = NTID
	FROM dbo.genTRACUser 
	WHERE 
		UserID = @genTracUserID
END





/*We want this employee and everyone under this employee - Pricer and Cost Volume Lead columns only*/
DECLARE @EmployeeLMPeopleID varchar (6)

SELECT @EmployeeLMPeopleID = empl_ser_no
FROM DataMart.DataMartEmployee
WHERE 
	nt_account_nm = @NTID




;
WITH MyCTE
AS ( 
SELECT empl_ser_no AS LMPeopleID, empl_first_nm as  FirstName, empl_last_nm as  LastName,
rpt_to_ser_no as  FunctionalManagerLMPeopleID,
nt_account_nm as  NTID
FROM DataMart.DataMartEmployee
WHERE 
empl_ser_no = @EmployeeLMPeopleID

UNION ALL

SELECT empl_ser_no AS LMPeopleID, empl_first_nm as  FirstName, empl_last_nm as  LastName,
rpt_to_ser_no as  FunctionalManagerLMPeopleID,
nt_account_nm as  NTID
FROM DataMart.DataMartEmployee E
	INNER JOIN MyCTE  ON E.rpt_to_ser_no = MyCTE.LMPeopleID
WHERE E.rpt_to_ser_no IS NOT NULL 
)


SELECT 
U.UserID AS genTracUserID
FROM MyCTE C
	INNER JOIN dbo.genTRACUser U ON
		C.NTID = U.NTID

GO