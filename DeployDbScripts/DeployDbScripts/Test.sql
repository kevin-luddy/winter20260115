/*
 * This file was used for testing.. There's no real purpose to it otherwise.
 */

CREATE TABLE dbo.TestTable (
	Id		INT				PRIMARY KEY			IDENTITY(1,1),
	Text	VARCHAR(100)	NOT NULL
);

GO

INSERT INTO dbo.TestTable (Text)
	VALUES ('Hello'), ('World'), ('Second Hello'), ('Third Hello');

GO