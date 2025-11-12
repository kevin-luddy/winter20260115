EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.5';
GO

-- Author: Hazrat Rafiqzadah (e403038)
-- PROPH-2994-Additional EPP Dates - 10/01/2025

-- First Step: Update Existing EPP Date columns in ProposalContractsData Table to show as Planned
GO

EXEC sp_rename '[dbo].[ProposalContractsData].[ProgramEppDate]',  'ScheduledActualProgramEppDate', 'COLUMN';

EXEC sp_rename '[dbo].[ProposalContractsData].[LobEppDate]',  'ScheduledActualLobEppDate', 'COLUMN';

EXEC sp_rename '[dbo].[ProposalContractsData].[PreSpaceEppDate]',  'ScheduledActualPreSpaceEppDate', 'COLUMN';

EXEC sp_rename '[dbo].[ProposalContractsData].[SpaceEppDate]',  'ScheduledActualSpaceEppDate', 'COLUMN';

EXEC sp_rename '[dbo].[ProposalContractsData].[PreCorporateEppDate]',  'ScheduledActualPreCorporateEppDate', 'COLUMN';

EXEC sp_rename '[dbo].[ProposalContractsData].[CorporateEppDate]',  'ScheduledActualCorporateEppDate', 'COLUMN';

EXEC sp_rename '[dbo].[ProposalContractsData].[BidEppDate]',  'ScheduledActualBidEppDate', 'COLUMN';

EXEC sp_rename '[dbo].[ProposalContractsData].[MissionSegmentEppDate]',  'ScheduledActualMissionSegmentEppDate', 'COLUMN';


-- Second Step: Add Scheduled EPP Date columns in ProposalContractsData Table
GO

ALTER TABLE [dbo].[ProposalContractsData]
ADD PlannedProgramEppDate date NULL;

ALTER TABLE [dbo].[ProposalContractsData]
ADD PlannedLobEppDate date NULL;

ALTER TABLE [dbo].[ProposalContractsData]
ADD PlannedPreSpaceEppDate date NULL;

ALTER TABLE [dbo].[ProposalContractsData]
ADD PlannedSpaceEppDate date NULL;

ALTER TABLE [dbo].[ProposalContractsData]
ADD PlannedPreCorporateEppDate date NULL;

ALTER TABLE [dbo].[ProposalContractsData]
ADD PlannedCorporateEppDate date NULL;

ALTER TABLE [dbo].[ProposalContractsData]
ADD PlannedBidEppDate date NULL;

ALTER TABLE [dbo].[ProposalContractsData]
ADD PlannedMissionSegmentEppDate date NULL;

GO