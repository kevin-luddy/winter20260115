EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.5';
GO

-- Author: Hazrat Rafiqzadah (e403038)
-- PROPH-2994-Additional EPP Dates - 10/01/2025

-- First Step: Update Existing EPP Date columns in ProposalContractsData Table to show as Planned
GO

EXEC sp_rename '[dbo].[ProposalContractsData].[ProgramEppDate]',  'PlannedProgramEppDate', 'COLUMN';

EXEC sp_rename '[dbo].[ProposalContractsData].[LobEppDate]',  'PlannedLobEppDate', 'COLUMN';

EXEC sp_rename '[dbo].[ProposalContractsData].[PreSpaceEppDate]',  'PlannedPreSpaceEppDate', 'COLUMN';

EXEC sp_rename '[dbo].[ProposalContractsData].[SpaceEppDate]',  'PlannedSpaceEppDate', 'COLUMN';

EXEC sp_rename '[dbo].[ProposalContractsData].[PreCorporateEppDate]',  'PlannedPreCorporateEppDate', 'COLUMN';

EXEC sp_rename '[dbo].[ProposalContractsData].[CorporateEppDate]',  'PlannedCorporateEppDate', 'COLUMN';

EXEC sp_rename '[dbo].[ProposalContractsData].[BidEppDate]',  'PlannedBidEppDate', 'COLUMN';

EXEC sp_rename '[dbo].[ProposalContractsData].[MissionSegmentEppDate]',  'PlannedMissionSegmentEppDate', 'COLUMN';


-- Second Step: Add Scheduled EPP Date columns in ProposalContractsData Table
GO

ALTER TABLE [dbo].[ProposalContractsData]
ADD ScheduledProgramEppDate date NULL;

ALTER TABLE [dbo].[ProposalContractsData]
ADD ScheduledLobEppDate date NULL;

ALTER TABLE [dbo].[ProposalContractsData]
ADD ScheduledPreSpaceEppDate date NULL;

ALTER TABLE [dbo].[ProposalContractsData]
ADD ScheduledSpaceEppDate date NULL;

ALTER TABLE [dbo].[ProposalContractsData]
ADD ScheduledPreCorporateEppDate date NULL;

ALTER TABLE [dbo].[ProposalContractsData]
ADD ScheduledCorporateEppDate date NULL;

ALTER TABLE [dbo].[ProposalContractsData]
ADD ScheduledBidEppDate date NULL;

ALTER TABLE [dbo].[ProposalContractsData]
ADD ScheduledMissionSegmentEppDate date NULL;

GO