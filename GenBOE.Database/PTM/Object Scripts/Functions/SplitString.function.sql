IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SplitString]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
	DROP FUNCTION [dbo].[SplitString]

GO

CREATE FUNCTION [dbo].[SplitString] (
	@List varchar(8000),
	@Delimiter char(1) = ',',
	@EmptyListItem varchar(80) = null -- default is empty table if no matches
) RETURNS @Results TABLE (
	ItemId int IDENTITY(1, 1) NOT NULL PRIMARY KEY,
	Item varchar(8000) NULL
)
AS
/******************************************************************************
**
**	Name: SplitString
**	Desc: Takes in a delimited string and the delimiter,
**			and returns a table with each value in a separate row.
**
**	Example calls:
**		SplitString ('1, 3, 4, 6', DEFAULT, DEFAULT)
**		SELECT Item FROM SplitString('1, 3, 4, 6', ',', DEFAULT)
**		SELECT Item FROM SplitString('1, 3, 4, 6', ',', 0)
**
**	Example of use with an inner join:
**
**		SELECT
**			p.ProgramID,
**			p.ProgramName,
**			idlist.Item
**		FROM
**			Program p
**			INNER JOIN SplitString(@MyIdList, ',', 0) idlist
**				ON CASE WHEN ISNULL(@MyIdList, '') = '' THEN 0 ELSE p.ProgramID END = CONVERT(INT, idlist.Item)
**
**	Example of use with an outer join:
**
**		SELECT
**			p.ProgramID,
**			p.ProgramName,
**			idlist.Item
**		FROM
**			Program p
**			LEFT JOIN SplitString(@MyIdList, ',', DEFAULT) idlist ON p.ProgramID = CONVERT(INT, idlist.Item)
**		WHERE
**			idlist.Item IS NOT NULL
**
**
**	Called by: Numerous stored procedures
**
*******************************************************************************
**	Change History
*******************************************************************************
**	Date:		Author:		Description:
**	--------	--------	---------------------------------------------------
**	2009-07-06	jdalonzo	Initial creation.
**	2011-03-24	sjrosent	Added @EmptyListItem input parameter which allows
**							caller to choose either an empty table result (by
**							default) OR a table with a single (designated) row
**							item.  So the caller can use an inner OR outer
**							join on the function results, if desired, without
**							the need for a lot of extra code.  (See examples)
**
*******************************************************************************/
BEGIN

DECLARE @item varchar(4000)
DECLARE @iPos int

SET @Delimiter = ISNULL(@Delimiter, ',')
SET @List = RTRIM(LTRIM(@List))

IF @List = ''
	SET @List = null  -- treat empty list as null
ELSE IF RIGHT(@List, 1) <> @Delimiter
	SELECT @List = @List + @Delimiter  -- append trailing delimiter (for algorithm) if none

-- if empty/null list and caller has designated an "empty row" value, then apply it
IF @List IS NULL AND @EmptyListItem IS NOT NULL
	INSERT @Results VALUES (@EmptyListItem)

-- get position of first item
SELECT @iPos = CHARINDEX(@Delimiter, @List, 1)

WHILE @iPos > 0
BEGIN
	-- find next item
	SELECT @item = LTRIM(RTRIM(SUBSTRING(@List, 1, @iPos -1)))
	IF @@ERROR <> 0 BREAK

	-- "remove" it from list
	SELECT @List = SUBSTRING(@List, @iPos + 1, Len(@List) - @iPos + 1)
	IF @@ERROR <> 0 BREAK

	-- "save" it
	INSERT @Results VALUES (@item)
	IF @@ERROR <> 0 BREAK

	-- determine position of next item
	SELECT @iPos = CHARINDEX(@Delimiter, @List, 1)
	IF @@ERROR <> 0 BREAK
END

RETURN

END

GO