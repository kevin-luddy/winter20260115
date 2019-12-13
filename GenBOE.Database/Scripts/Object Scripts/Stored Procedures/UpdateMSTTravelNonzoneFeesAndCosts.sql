IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateMSTTravelNonzoneFeesAndCosts]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[UpdateMSTTravelNonzoneFeesAndCosts];
GO

CREATE PROCEDURE [dbo].[UpdateMSTTravelNonzoneFeesAndCosts]
(
@ModeID int,
@TravelAgencyFee money,
@MiscOther money
)
AS
/******************************************************************************
**		 
**		Name: [UpdateMSTTravelNonzoneFeesAndCosts]
**		Desc: Update the Travel Agency Fee and Misc/Other cost for an RMS Travel Nonzone Mode.
**
**		Auth: RJ Anzalone
**		Date: 8/16/16
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		8/16/16		ranzalon			SP created. 
**		11/23/16	buckwalj			Added UpdateDT (BOEJ-1544)
******************************************************************************/

UPDATE [dbo].[MSTTravelNonzoneFeesAndCosts]
	SET [TravelAgencyFee]=@TravelAgencyFee, [MiscOther]=@MiscOther, UpdateDT=GETDATE()
	WHERE [ModeID]=@ModeID


GO