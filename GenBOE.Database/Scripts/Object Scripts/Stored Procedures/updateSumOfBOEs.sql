IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateSumOfBOEs]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateSumOfBOEs];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[updateSumOfBOEs]
(
@WBSID int,
@BOEID int
)
AS
/******************************************************************************
**		 
**		Name: 
**		Desc: Updates records in both OV and WSV SumOfBOE
**				Developer Request:
**
**				The SP will take in 2 inputs. WbsId and BoeId
**				
**				The SP will then do an update on 2 tables, both tables the same change:
**				SumOfBOE_OrdinaryVariableXREF
**				SumOfBOE_WorkspaceVariableXREF
**				
**				Where WbsId = @WbsIdInput
**				Set
**				WbsId = NULL
**				BoeId = @BoeIdInput
**		
**
**		Auth: Don Canuso
**		Date: 2/5/13
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/

SET NOCOUNT ON 
DECLARE	@ErrorMessage varchar (500)

IF @WBSID IS NULL AND @BOEID IS NULL
	BEGIN
		SET @ErrorMessage =   'BOE and WBS must not be NULL.'
		RAISERROR (
			@ErrorMessage, -- Message text.
	        11, -- Severity,/*Severity Changed to 11*/
			1 -- State,
			)
		RETURN
	END
	
UPDATE dbo.SumOfBOE_OrdinaryVariableXREF 
SET WBSID = NULL,
	BOEID = @BOEID
WHERE 
	ISNULL(WBSID, -9999) = @WBSID 
	

UPDATE 	dbo.SumOfBOE_WorkspaceVariableXREF
SET WBSID = NULL,
	BOEID = @BOEID
WHERE 
	ISNULL(WBSID, -9999) = @WBSID
GO