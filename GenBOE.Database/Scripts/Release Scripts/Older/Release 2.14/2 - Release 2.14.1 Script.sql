/*
	## START ##

	3/9/17 [RJ] -- BOEJ-1944 Approver/Poc fields too small (Phone and Display Name) in P/I BOE forms
*/

ALTER TABLE [dbo].[BOEFormIBOE] ALTER COLUMN [Poc] VARCHAR(65) NOT NULL;
ALTER TABLE [dbo].[BOEFormIBOE] ALTER COLUMN [PocPhone] VARCHAR (60) NOT NULL;
ALTER TABLE [dbo].[BOEFormIBOE] ALTER COLUMN [Approver] VARCHAR(65) NOT NULL;
ALTER TABLE [dbo].[BOEFormIBOE] ALTER COLUMN [ApproverPhone] VARCHAR (60) NOT NULL;

ALTER TABLE [version].[BOEFormIBOE] ALTER COLUMN [Poc] VARCHAR(65) NOT NULL;
ALTER TABLE [version].[BOEFormIBOE] ALTER COLUMN [PocPhone] VARCHAR (60) NOT NULL;
ALTER TABLE [version].[BOEFormIBOE] ALTER COLUMN [Approver] VARCHAR(65) NOT NULL;
ALTER TABLE [version].[BOEFormIBOE] ALTER COLUMN [ApproverPhone] VARCHAR (60) NOT NULL;

ALTER TABLE [dbo].[BOEFormPBOE] ALTER COLUMN [Poc] VARCHAR(65) NOT NULL;
ALTER TABLE [dbo].[BOEFormPBOE] ALTER COLUMN [PocPhone] VARCHAR (60) NOT NULL;
ALTER TABLE [dbo].[BOEFormPBOE] ALTER COLUMN [Approver] VARCHAR(65) NOT NULL;
ALTER TABLE [dbo].[BOEFormPBOE] ALTER COLUMN [ApproverPhone] VARCHAR (60) NOT NULL;

ALTER TABLE [version].[BOEFormPBOE] ALTER COLUMN [Poc] VARCHAR(65) NOT NULL;
ALTER TABLE [version].[BOEFormPBOE] ALTER COLUMN [PocPhone] VARCHAR (60) NOT NULL;
ALTER TABLE [version].[BOEFormPBOE] ALTER COLUMN [Approver] VARCHAR(65) NOT NULL;
ALTER TABLE [version].[BOEFormPBOE] ALTER COLUMN [ApproverPhone] VARCHAR (60) NOT NULL;

/*
	3/9/17 [RJ] -- BOEJ-1944 Approver/Poc fields too small (Phone and Display Name) in P/I BOE forms

	## END ##
*/

/*
	## START ##

	3/30/17 [Tom] -- BOEJ-1993 Zone Travel - Add Travel Mode into ProPricer export
*/

IF NOT EXISTS (SELECT * FROM [dbo].[propricerfieldlu] WHERE [ProPricerField] = 'Trip_TravelMode')
BEGIN
	INSERT INTO dbo.propricerfieldlu VALUES (56, 'Trip_TravelMode', 1, 3);
END

/*
	3/30/17 [Tom] -- BOEJ-1993 Zone Travel - Add Travel Mode into ProPricer export

	## END ##
*/
