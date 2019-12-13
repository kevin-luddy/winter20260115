IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateMileageReimbursementRate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateMileageReimbursementRate];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[updateMileageReimbursementRate]
(
@MileageReimbursementRateID int,
@RatePerMile decimal(6,5),
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [upsertMileageReimbursementRate]
**		Desc: Insert/Update Mileage Reimbursement Rates for Travel
**
**
**		
**
**		Auth: Don Canuso
**		Date: 6/21/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @Inserted AS Table (ID int)
DECLARE	@ErrorMessage varchar (500)

	BEGIN
		IF (SELECT UpdateDT FROM [dbo].[MileageReimbursementRate] WHERE MileageReimbursementRateID = @MileageReimbursementRateID) = @UpdateDT
		BEGIN	
		SET @UpdateDT = GETDATE()
			
		UPDATE [dbo].[MileageReimbursementRate]
			SET [RatePerMile] = @RatePerMile,
				[UpdateDT] = @UpdateDT
		WHERE 
			MileageReimbursementRateID = @MileageReimbursementRateID

		END
	END

IF @@ERROR = 0
	SELECT	@MileageReimbursementRateID AS MileageReimbursementRateID
GO