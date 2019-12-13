DROP FUNCTION [dbo].[udfCreateCommaSeparatedList];
GO

CREATE FUNCTION [dbo].[udfCreateCommaSeparatedList]
(
@ProposalID int,
@ListTypeID int
)
RETURNS varchar(1000)
AS
BEGIN

DECLARE @listStr VARCHAR(1000)


IF @ListTypeID = 1 /*Contract Type*/
BEGIN
	SELECT @listStr = COALESCE(@listStr+',' ,'') + C.ContractType
	FROM dbo.Proposal P
		INNER JOIN dbo.ProposalContractTypeXREF X ON P.ProposalID = X.ProposalID
		INNER JOIN dbo.ContractTypeLU C ON X.ContractTypeID = C.ContractTypeID
	WHERE P.ProposalID = @ProposalID		
END



IF @ListTypeID = 2 /*Cost Element*/
BEGIN
	SELECT @listStr = COALESCE(@listStr+',' ,'') + C.CostElement
	FROM dbo.Proposal P
		INNER JOIN dbo.ProposalCostElementXREF X ON P.ProposalID = X.ProposalID
		INNER JOIN dbo.CostElementLU C ON X.CostElementID = C.CostElementID
	WHERE P.ProposalID = @ProposalID		
END


RETURN @listStr

END

GO