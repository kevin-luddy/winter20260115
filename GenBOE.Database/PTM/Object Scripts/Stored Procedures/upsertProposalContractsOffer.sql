IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProposalContractsOffer]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProposalContractsOffer];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertProposalContractsOffer]
(
	@ProposalContractsOfferId [int],
	@UpdateDT [datetime2](7),
	@ContractsDataId [int],
	@CustomerOfferAmount [bigint],
	@CustomerOfferDate [date],
	@LMCounterOfferDate [date],
	@LMCounterOfferCost [bigint],
	@LMCounterOfferCOM [bigint],
	@LMCounterOfferProfitFee [bigint]
)
AS
/******************************************************************************
**
**		Name: [upsertProposalContractsOffer]
**		Desc: Upsert Proposal Contracts Offer
**
**
**		Auth: Ajay Koovackal
**		Date: 10/29/21
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		10/29/21    Koovackal			Creation
*******************************************************************************/
SET NOCOUNT ON

	IF @ProposalContractsOfferId < 0 -- Inserting a new
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			INSERT INTO [dbo].[ProposalContractsOffer] (UpdateDT, ContractsDataId, CustomerOfferAmount, 
														CustomerOfferDate, LMCounterOfferDate, LMCounterOfferCost, 
														LMCounterOfferCOM, LMCounterOfferProfitFee)
				OUTPUT inserted.ProposalContractsOfferId INTO @Inserted
				VALUES (GETDATE(),@ContractsDataId, @CustomerOfferAmount, @CustomerOfferDate, @LMCounterOfferDate, 
						@LMCounterOfferCost, @LMCounterOfferCOM, @LMCounterOfferProfitFee)
			SELECT @ProposalContractsOfferId = Id FROM @Inserted
		END
	ELSE -- updating an existing
		BEGIN
			IF (SELECT UpdateDT FROM ProposalContractsOffer WHERE ProposalContractsOfferId = @ProposalContractsOfferId) = @UpdateDT
				UPDATE ProposalContractsOffer
					SET UpdateDT = GETDATE(),
						ContractsDataId = @ContractsDataId,
						CustomerOfferAmount= @CustomerOfferAmount,
						CustomerOfferDate = @CustomerOfferDate,
						LMCounterOfferDate = @LMCounterOfferDate,
						LMCounterOfferCost = @LMCounterOfferCost,
						LMCounterOfferCOM = @LMCounterOfferCOM,
						LMCounterOfferProfitFee = @LMCounterOfferProfitFee
					WHERE ProposalContractsOfferIdId = ProposalContractsOfferId
			ELSE
				BEGIN
					DECLARE @ErrorMessage varchar (500)
					SET @ErrorMessage = 'The ProposalContractsOffer with Id ' + CAST(@ProposalContractsOfferId AS varchar(10)) + ' has been updated and is out of sync with the data in your browser. Please refresh your data.'
					RAISERROR (@ErrorMessage, 11, 1)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @ProposalContractsOfferId as ProposalContractsOfferId

GO