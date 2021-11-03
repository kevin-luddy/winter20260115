IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProposalContractsOffer]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProposalContractsOffer];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteProposalContractsOffer]
(
@ProposalContractsOfferId INT,
@UpdateDT DATETIME2
)
AS
/******************************************************************************
**
**		Name: [deleteProposalContractsOffer]
**		Desc: Delete Proposal Contracts Offer
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
	IF (SELECT UpdateDT FROM [dbo].[ProposalContractsOffer] WHERE ProposalContractsOfferId = @ProposalContractsOfferID ) = @UpdateDT
		BEGIN
			DELETE FROM dbo.ProposalContractsOffer
			WHERE ProposalContractsOfferId = @ProposalContractsOfferId
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
			SET @ErrorMessage =   'The Proposal Contracts Offer with Id ' + CAST(@ProposalContractsOfferId  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser. Please refresh your data.'
			RAISERROR (@ErrorMessage, 11, 1)
			RETURN
		END
GO