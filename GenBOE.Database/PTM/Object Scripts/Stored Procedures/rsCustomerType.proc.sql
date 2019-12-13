IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[rsCustomerType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[rsCustomerType];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[rsCustomerType]
(
	@CustomerType varchar (8000)
)	
AS
SET NOCOUNT ON
/*
SP Used for SSRS Report Header
*/


/*
Process CustomerType
*/
DECLARE @tblCustomerType TABLE (CustomerTypeID int)
IF @CustomerType IS NULL OR @CustomerType = 'All'
	BEGIN
		INSERT INTO @tblCustomerType
		SELECT -1 
	END
ELSE	
	BEGIN
		IF RIGHT(@CustomerType, 1) <> ','
	      SET @CustomerType = @CustomerType + ','
	
		WHILE (SELECT CHARINDEX (',', @CustomerType) ) > 1
			BEGIN
			      
				  INSERT INTO @tblCustomerType
				  SELECT LEFT (@CustomerType, CHARINDEX (',', @CustomerType) -1)
				  SET @CustomerType = RIGHT (@CustomerType, LEN (@CustomerType) - CHARINDEX (',', @CustomerType) )
			      
			END
	END



DECLARE @listStr VARCHAR(1000)


IF EXISTS (SELECT 1 FROM @tblCustomerType WHERE CustomerTypeID = -1)
BEGIN
	SET @listStr = 'All'
END
ELSE
BEGIN
SELECT @listStr = COALESCE(@listStr+', ' ,'') + C.CustomerType
FROM @tblCustomerType tC
	INNER JOIN dbo.CustomerTypeLU C ON tC.CustomerTypeID = C.CustomerTypeID
END



SELECT @listStr

GO

GRANT EXECUTE ON OBJECT::dbo.rsCustomerType TO generationReporter;
GO