IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProposalContractsData]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProposalContractsData];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertProposalContractsData]
(
	@ProposalContractsDataId [int],
	@UpdateDT [datetime2](7),
	@ProposalID [int],
	@PreviouslySubmittedROM [int],
	@CustomerSubmittalDate [date],
	@ContractsCorrespondLogNumber [varchar](20),
	@FinalNegotiatedValue [bigint],
	@FinalNegotiatedDate [date],
	@EppDelegationAuthority [int],
	@PlannedBidEppDate [date],
	@PlannedProgramEppDate [date],
	@PlannedMissionSegmentEppDate [date],
	@PlannedLobEppDate [date],
	@PlannedPreSpaceEppDate [date],
	@PlannedSpaceEppDate [date],
	@PlannedPreCorporateEppDate [date],
	@PlannedCorporateEppDate [date],
	@ScheduledBidEppDate [date],
	@ScheduledProgramEppDate [date],
	@ScheduledMissionSegmentEppDate [date],
	@ScheduledLobEppDate [date],
	@ScheduledPreSpaceEppDate [date],
	@ScheduledSpaceEppDate [date],
	@ScheduledPreCorporateEppDate [date],
	@ScheduledCorporateEppDate [date],
	@EppRosDelegationNotes [varchar](1000),
	@LmWon [bit],
	@ModCompletedDate [date],
	@CageCode [varchar](10),
	@CustomerDueDate [date],
	@IsInsuranceDirect [int] = NULL,
	@InsuranceType [int] = NULL,
	@ProposedInsurance [bigint] = NULL,
	@NegotiatedInsurance [bigint] = NULL
)
AS
/******************************************************************************
**
**		Name: [upsertProposalContractsData]
**		Desc: Upsert Proposal Contracts Data
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
**      02/16/22    Koovackal           Add upsert for EPP fields
**		07/18/22	ranzalon			Add CageCode
**		02/22/23	ranzalon			Add CustomerDueDate
**		11/12/24	twilson3			Add Insurance fields
**		05/12/25	ranzalon			Add Bid and Mission Segment EPP Dates
**		10/02/25	e403038				Additional EPP Dates (Original to Planned and add Scheduled)
*******************************************************************************/
SET NOCOUNT ON

	IF @ProposalContractsDataId < 0 -- Insert new
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			INSERT INTO [dbo].[ProposalContractsData] (UpdateDT, ProposalID, PreviouslySubmittedROM, CustomerSubmittalDate,
														ContractsCorrespondLogNumber, FinalNegotiatedValue, FinalNegotiatedDate,
														EppDelegationAuthority, PlannedProgramEppDate, PlannedLobEppDate, PlannedPreSpaceEppDate, 
														PlannedSpaceEppDate, PlannedPreCorporateEppDate, PlannedCorporateEppDate, EppRosDelegationNotes, 
														LmWon, ModCompletedDate, CageCode, CustomerDueDate, IsInsuranceDirect, 
														InsuranceType, ProposedInsurance, NegotiatedInsurance, PlannedBidEppDate, PlannedMissionSegmentEppDate,
														ScheduledProgramEppDate, ScheduledLobEppDate, ScheduledPreSpaceEppDate, 
														ScheduledSpaceEppDate, ScheduledPreCorporateEppDate, ScheduledCorporateEppDate, ScheduledBidEppDate, ScheduledMissionSegmentEppDate)
				OUTPUT inserted.ProposalContractsDataId INTO @Inserted
				VALUES (GETDATE(), @ProposalID, @PreviouslySubmittedROM, @CustomerSubmittalDate, @ContractsCorrespondLogNumber,
						@FinalNegotiatedValue, @FinalNegotiatedDate, @EppDelegationAuthority, @PlannedProgramEppDate, @PlannedLobEppDate, 
						@PlannedPreSpaceEppDate, @PlannedSpaceEppDate, @PlannedPreCorporateEppDate, @PlannedCorporateEppDate, @EppRosDelegationNotes, @LmWon, 
						@ModCompletedDate, @CageCode, @CustomerDueDate, @IsInsuranceDirect, @InsuranceType, @ProposedInsurance, 
						@NegotiatedInsurance, @PlannedBidEppDate, @PlannedMissionSegmentEppDate,
						@ScheduledProgramEppDate, @ScheduledLobEppDate, @ScheduledPreSpaceEppDate, 
						@ScheduledSpaceEppDate, @ScheduledPreCorporateEppDate, @ScheduledCorporateEppDate, @ScheduledBidEppDate, @ScheduledMissionSegmentEppDate)
			SELECT @ProposalContractsDataId = Id FROM @Inserted
		END
	ELSE -- updating existing
		BEGIN
			IF (SELECT UpdateDT FROM ProposalContractsData WHERE ProposalContractsDataId = @ProposalContractsDataId) = @UpdateDT
				UPDATE ProposalContractsData
					SET UpdateDT = GETDATE(),
						ProposalID = @ProposalID,
						PreviouslySubmittedROM = @PreviouslySubmittedROM,
						CustomerSubmittalDate = @CustomerSubmittalDate,
						ContractsCorrespondLogNumber = @ContractsCorrespondLogNumber,
						FinalNegotiatedValue = @FinalNegotiatedValue, 
						FinalNegotiatedDate = @FinalNegotiatedDate,
						EppDelegationAuthority = @EppDelegationAuthority,
						PlannedBidEppDate = @PlannedBidEppDate,
						PlannedProgramEppDate = @PlannedProgramEppDate,
						PlannedMissionSegmentEppDate = @PlannedMissionSegmentEppDate,
						PlannedLobEppDate = @PlannedLobEppDate,
						PlannedPreSpaceEppDate = @PlannedPreSpaceEppDate,
						PlannedSpaceEppDate = @PlannedSpaceEppDate,
						PlannedPreCorporateEppDate = @PlannedPreCorporateEppDate,
						PlannedCorporateEppDate = @PlannedCorporateEppDate,
						ScheduledBidEppDate = @ScheduledBidEppDate,
						ScheduledProgramEppDate = @ScheduledProgramEppDate,
						ScheduledMissionSegmentEppDate = @ScheduledMissionSegmentEppDate,
						ScheduledLobEppDate = @ScheduledLobEppDate,
						ScheduledPreSpaceEppDate = @ScheduledPreSpaceEppDate,
						ScheduledSpaceEppDate = @ScheduledSpaceEppDate,
						ScheduledPreCorporateEppDate = @ScheduledPreCorporateEppDate,
						ScheduledCorporateEppDate = @ScheduledCorporateEppDate,
						EppRosDelegationNotes = @EppRosDelegationNotes,
						LmWon = @LmWon,
						ModCompletedDate = @ModCompletedDate,
						CageCode = @CageCode,
						CustomerDueDate = @CustomerDueDate,
						IsInsuranceDirect = @IsInsuranceDirect, 
						InsuranceType = @InsuranceType, 
						ProposedInsurance = @ProposedInsurance, 
						NegotiatedInsurance = @NegotiatedInsurance
					WHERE ProposalContractsDataId = @ProposalContractsDataId
			ELSE
				BEGIN
					DECLARE @ErrorMessage varchar (500)
					SET @ErrorMessage = 'The ProposalContractsData with Id ' + CAST(@ProposalContractsDataId  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser. Please refresh your data.'
					RAISERROR (@ErrorMessage, 11, 1)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @ProposalContractsDataId as ProposalContractsDataId

GO