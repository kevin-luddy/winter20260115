IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOETaskElementOrdinaryVariable]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOETaskElementOrdinaryVariable];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteBOETaskElementOrdinaryVariable]
(
@OrdinaryVariableID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteBOETaskElementOrdinaryVariable]
**		Desc: Deletes the BOE Task Element's use of an Ordinary Variable
**			
**		
**
**		Auth: Don Canuso
**		Date: 10/4/2010
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			----------------------------------------
**		8/31/11		dcanuso				Added Reference tables
*******************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDT FROM [dbo].[OrdinaryVariable] 
			WHERE OrdinaryVariableID = @OrdinaryVariableID) = @UpdateDT
		BEGIN
		
			DELETE FROM dbo.SumOfBOE_OrdinaryVariableXREF WHERE OrdinaryVariableID = @OrdinaryVariableID 
			DELETE FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF  WHERE OrdinaryVariableID = @OrdinaryVariableID

			DELETE FROM dbo.OrdinaryVariable
			WHERE
				OrdinaryVariableID = @OrdinaryVariableID
		END				
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Ordinary Variable with ID ' + CAST(@OrdinaryVariableID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
 
		END

GO