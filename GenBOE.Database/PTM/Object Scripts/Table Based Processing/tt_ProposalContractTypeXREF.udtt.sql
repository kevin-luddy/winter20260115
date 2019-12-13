-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'tt_ProposalContractTypeXREF' AND ss.name = N'dbo')
	DROP TYPE [dbo].[tt_ProposalContractTypeXREF];
GO

CREATE TYPE [dbo].[tt_ProposalContractTypeXREF] AS  TABLE (
    [ProposalID]     INT NOT NULL,
    [ContractTypeID] INT NOT NULL);

GO