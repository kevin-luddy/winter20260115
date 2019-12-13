IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[rsContractType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[rsContractType];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[rsContractType]
(
	@ContractType varchar (8000)
)	
AS
SET NOCOUNT ON
/*
SP Used for SSRS Report Header
*/


/*
Process ContractType
*/
DECLARE @tblContractType TABLE (ContractTypeID int)
IF @ContractType IS NULL OR @ContractType = 'All'
	BEGIN
		INSERT INTO @tblContractType
		SELECT -1 
	END
ELSE	
	BEGIN
		IF RIGHT(@ContractType, 1) <> ','
	      SET @ContractType = @ContractType + ','
	
		WHILE (SELECT CHARINDEX (',', @ContractType) ) > 1
			BEGIN
			      
				  INSERT INTO @tblContractType
				  SELECT LEFT (@ContractType, CHARINDEX (',', @ContractType) -1)
				  SET @ContractType = RIGHT (@ContractType, LEN (@ContractType) - CHARINDEX (',', @ContractType) )
			      
			END
	END



DECLARE @listStr VARCHAR(1000)


IF EXISTS (SELECT 1 FROM @tblContractType WHERE ContractTypeID = -1)
BEGIN
	SET @listStr = 'All'
END
ELSE
BEGIN
SELECT @listStr = COALESCE(@listStr+', ' ,'') + C.ContractType
FROM @tblContractType tC
	INNER JOIN dbo.ContractTypeLU C ON tC.ContractTypeID = C.ContractTypeID
END



SELECT @listStr

GO