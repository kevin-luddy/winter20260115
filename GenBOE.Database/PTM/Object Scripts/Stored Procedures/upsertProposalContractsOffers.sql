IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProposalContractsOffers]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProposalContractsOffers];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertProposalContractsOffers]
(
	@ProposalContractsOffersId [int],
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
**		Name: [upsertProposalContractsOffers]
**		Desc: Upsert Proposal Contracts Offers
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

	IF @ProposalContractsOffersId < 0 -- Inserting a new
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			INSERT INTO [dbo].[ProposalContractsOffers] (UpdateDT, ContractsDataId, CustomerOfferAmount, 
														CustomerOfferDate, LMCounterOfferDate, LMCounterOfferCost, 
														LMCounterOfferCOM, LMCounterOfferProfitFee)
				OUTPUT inserted.ProposalContractsOffersId INTO @Inserted
				VALUES (GETDATE(),@ContractsDataId, @CustomerOfferAmount, @CustomerOfferDate, @LMCounterOfferDate, 
						@LMCounterOfferCost, @LMCounterOfferCOM, @LMCounterOfferProfitFee)
			SELECT @ProposalContractsOffersId = Id FROM @Inserted
		END
	ELSE -- updating an existing
		BEGIN
			IF (SELECT UpdateDT FROM ProposalContractsOffers WHERE ProposalContractsOffersId = @ProposalContractsOffersId) = @UpdateDT
				UPDATE ProposalContractsOffers
					SET UpdateDT = GETDATE(),
						ContractsDataId = @ContractsDataId,
						CustomerOfferAmount= @CustomerOfferAmount,
						CustomerOfferDate = @CustomerOfferDate,
						LMCounterOfferDate = @LMCounterOfferDate,
						LMCounterOfferCost = @LMCounterOfferCost,
						LMCounterOfferCOM = @LMCounterOfferCOM,
						LMCounterOfferProfitFee = @LMCounterOfferProfitFee
					WHERE ProposalContractsOffersId = @ProposalContractsOffersId
			ELSE
				BEGIN
					DECLARE @ErrorMessage varchar (500)
					SET @ErrorMessage = 'The ProposalContractsOffers with Id ' + CAST(@ProposalContractsOffersId AS varchar(10)) + ' has been updated and is out of sync with the data in your browser. Please refresh your data.'
					RAISERROR (@ErrorMessage, 11, 1)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @ProposalContractsOffersId as ProposalContractsOffersId

GO