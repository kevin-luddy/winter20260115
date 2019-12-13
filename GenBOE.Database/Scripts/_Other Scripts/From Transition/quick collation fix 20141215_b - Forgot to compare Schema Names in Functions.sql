--USE [MDM_FMDM]
GO
SET QUOTED_IDENTIFIER ON
--drop table #SQL;Drop Table #Functions;Drop Table #spindtab

--SELECT CONVERT(varchar(100), SERVERPROPERTY('collation'))
declare @p_DatabaseName	sysname 
set @p_DatabaseName = (SELECT DB_NAME() )
declare	@p_NewCollationName	varchar(200)
--SET DESTINATION (NEW) COLLATION HERE
set @p_NewCollationName  = /*(SELECT CONVERT(varchar(100), SERVERPROPERTY('collation'))) -- */'SQL_Latin1_General_CP1_CS_AS'
print @p_NewCollationName
-- other local variables:
declare @all  tinyint,
	@cmd nvarchar(max)
select  @all = 1
declare @textptr binary(16)
declare @SQLSegment nvarchar(4000)
declare @C Cursor
set nocount on

-- create temporary table to hold the commands to be executed:
IF (SELECT object_id('tempdb..#SQL')) IS NOT NULL
	DROP TABLE #SQL;
create table #SQL (ID int primary key identity(1,1),SQL ntext)

--INSERT INTO #SQL DROP STATISTICS
--declare @sql varchar(4000)  --defined above
--declare @counter int		-- defined above
declare @stats table (
id int identity(1,1),
tableName varchar(100),
statName varchar(100)
)
Insert into @stats (tableName,statName)
select o.Name as tableName, i.Name as statName from sysindexes i
inner join sysobjects o
on i.id=o.id
Where left(i.name,5)='hind_'

declare @sql varchar(4000)
declare @counter int
set @counter =1
While @counter<=(select max(ID) from @stats)
	BEGIN
	set @sql = 'DROP STATISTICS ' + (select quotename(tableName) from @stats where ID=@counter) + '.' +
		(select statName from @stats where ID=@counter)
	--print (@sql)
	insert into #SQL (SQL) Select @sql
	--exec (@sql)
	set @counter=@counter+1
	END
--select * from #sql

-- FULL TEXT INDEX DROPS

SET NOCOUNT ON
DECLARE @Catalog NVARCHAR(128),
--		@SQL NVARCHAR(MAX),
		@COLS NVARCHAR(4000),
		@Owner NVARCHAR(128),
		@Table NVARCHAR(128),
		@ObjectID INT,
		@AccentOn BIT,	
		@CatalogID INT,
		@IndexID INT,
		@Max_objectId INT,
		@NL CHAR(2)


DECLARE @FTIs TABLE (
ID INT IDENTITY(1,1)
,FTI VARCHAR(100)
)
INSERT INTO @FTIs (FTI)
SELECT Name FROM sys.fulltext_catalogs
--SELECT * FROM @FTIs

SELECT @NL	= CHAR(13)+CHAR(10) --Carriage Return

DECLARE @fticounter INT
SET @fticounter = 1

WHILE @fticounter <= ( SELECT MAX(ID) FROM @FTIs )
BEGIN
	SET @Catalog = ( SELECT FTI FROM @FTIs WHERE ID = @fticounter )
	BEGIN
				insert into #SQL (SQL)
				SELECT	'DROP FULLTEXT INDEX ON ' + QUOTENAME(u.name)+'.'+QUOTENAME(t.name) + ';'
				FROM	sys.tables as t
				JOIN	sysusers as u
					ON	u.uid = t.schema_id
				JOIN	sys.fulltext_indexes i 
					ON	t.object_id = i.object_id
				JOIN	sys.fulltext_catalogs c 
					ON i.fulltext_catalog_id = c.fulltext_catalog_id
				WHERE	c.Name =  @Catalog
				
		-- Script out catalog
		SET @SQL = 'DROP FULLTEXT CATALOG ' + @Catalog 
		insert into #SQL (SQL)
		SELECT (@sql)
		END

	SET @fticounter = @fticounter + 1
  END



insert into #SQL (SQL)
select 	'Alter table [' + schema_name(o.uid) + '].['+ object_Name(c.id) + '] DROP CONSTRAINT [' + object_name(constid) + ']' 
from 	sysconstraints c
inner join sysobjects o
on o.id = c.id
where 	objectproperty(constid,'IsForeignKey')=1 
and 	constid in  (
	select 	fk.constid
	from 	sysforeignkeys fk
	join 	syscolumns fc
	on	fc.colid = fk.fkey
	and	fc.id = fk.fkeyid
	join 	syscolumns rc
	on	rc.colid = fk.rkey
	and	rc.id = fk.rkeyid
	where	fc.collationid is not null
	or 	rc.collationid is not null --)
	or	@all=1 )  --parameter allows all constraints to be dropped

/*script out dropping of check constraints */
insert into #SQL (SQL)
select 	'Alter table [' + Object_Name(cs.id) + '] drop constraint ['+object_name(cs.constid)+']'
from 	sysconstraints cs
where 	objectproperty(cs.constid,'IsCheckCnst') = 1

--drop Table Functions
Declare	@depth int,
	@soid int

--Drop Table #Functions
IF (SELECT object_id('tempdb..#Functions')) IS NOT NULL
	DROP TABLE #Functions;
CREATE TABLE #Functions (
ID int identity(1,1),
SOID int,
FunctionName varchar(100),
SchemaName varchar(20),
DEPID int,
Depth int,
FText Varchar(4000),
FtextOrder int
)
INSERT INTO #Functions (SOID, FunctionName, SchemaName,DEPID, Depth, FText, FtextOrder)
select distinct so.id SOid,so.name , SchemaName=schema_name(uid), sd.depid, 0, sc.text, sc.COLID	-- SELECT schema_name(uid),*
from sysobjects so
LEFT join sysdepends sd
on so.id = sd.id
join 	syscomments sc
on		so.id = sc.id
where 	objectproperty(so.id,'IsMSShipped')=0 
and ( objectproperty(so.id,'IsTableFunction')=1  
	or objectproperty(so.id,'IsScalarFunction')=1
	or objectproperty(so.id,'IsInlineFunction')=1 
	)


WHILE EXISTS (SELECT * FROM #Functions a
				inner join #Functions b
				on a.DepID=b.SOID 
				and a.Depth=b.Depth
				and a.SchemaName + '.' + a.FunctionName= b.schemaName +'.' + b.FunctionName
				--order by a.FunctionName 
				)
	UPDATE A set A.depth=B.Depth+1	-- select *
	FROM #Functions AS A
	INNER JOIN  #Functions AS B 
	ON A.DepID=B.SOID
	and a.SchemaName + '.' + a.FunctionName= b.schemaName +'.' + b.FunctionName
--	Select * From #Functions order by FunctionName

--- WE DROP VIEWS AFTER DROPPING INDEXES

insert into #SQL (SQL)
select distinct 'DROP FUNCTION ['+ SchemaName+ '].['+ FunctionName+ ']'
from #Functions
/*
from 	sysobjects 
where 	objectproperty(id,'IsMSShipped')=0 
and ( objectproperty(id,'IsTableFunction')=1  
	or objectproperty(id,'IsScalarFunction')=1
	or objectproperty(id,'IsInlineFunction')=1 )
*/
-- script drop of indexes - we will also populate a temp table that helps recreate the indexes later
IF (SELECT object_id('tempdb..#spindtab')) IS NOT NULL
	DROP TABLE #spindtab;
create table #spindtab 
(
	schemaname			sysname collate database_default NOT NULL,
	objectname			sysname collate database_default NOT NULL,
	index_name			sysname	collate database_default NOT NULL,
	stats				int,
	groupname			sysname collate database_default NOT NULL,
	index_keys			nvarchar(3000)	collate database_default NOT NULL, -- see @IX_keys above for length descr
	OrigFillFactor			tinyint
	,includeColumns		nvarchar(3000)
	,xType varchar(10)
	,IX_Filter_Definition NVARCHAR(1000)
)

-- GET INCLUDE columns for indexes
SET NOCOUNT ON
DECLARE @tempIndexesInclude TABLE (
ID int identity(1,1)
,schemaName varchar(100)
,TableID bigint
,tableName varchar(255)
,IndexName varchar(255)
,ColumnName varchar(4000)
,IncludeColumns varchar(4000)
)
DECLARE @tempIndexes TABLE (
ID int identity(1,1)
,schemaName varchar(100)
,TableID bigint
,tableName varchar(255)
,IndexName varchar(500)
,ColumnName varchar(4000)
,ColumnUsage varchar(100)
,index_column_id int
,column_id int
,key_ordinal int
,filter_definition varchar(1000)
)
INSERT INTO @tempIndexes (schemaName,TableID,tableName,IndexName,ColumnName,columnUsage,index_column_id,column_id,key_ordinal,filter_definition)
SELECT schema_name(o.uid) schemaName,i.id TableID, object_name(i.id) tableName, i.name IndexName, c.name columnName
,'column usage' = CASE ic.is_included_column WHEN 0 then 'KEY' ELSE 'INCLUDED' END
,index_column_id,column_id,key_ordinal,ISNULL(i2.filter_definition,'NoFilter')
-- SELECT o.name,object_name(ic.object_id),*
FROM sys.sysindexes i
LEFT JOIN (select name,id,uid from sysobjects) o
ON i.id=o.id
INNER JOIN sys.index_columns ic
ON ic.object_id = o.id
AND ic.index_id = i.indid
INNER JOIN sys.syscolumns c
ON ic.object_id = c.id
AND ic.column_id = c.colid
INNER JOIN sys.indexes i2
ON i2.Object_ID = i.ID
AND i2.index_id=i.indid
WHERE /*id = @IX_objid and */indid > 0 and indid < 255 and (i.status & 64)=0 
AND  objectproperty(i.id,'ISMSSHIPPED')=0
AND objectproperty(i.id,'IsTableFunction')=0 
AND i2.type_desc IN ('CLUSTERED','NONCLUSTERED')
ORDER BY object_name(i.id),indid,ic.index_column_id
--ActionItemID, ActionDate, Status, ActionItem, Comment, Owner
--IX_Report_ProgramID_Deleted_covering_fullInclude

--SELECT * FROM @tempIndexes

DECLARE @schemaName varchar(100)
DECLARE @TableID bigint
DECLARE @tableName varchar(255)
DECLARE @IndexName varchar(500)
DECLARE @ColumnName varchar(4000)
DECLARE @ColumnUsage varchar(100)
DECLARE @indexColumnID int
DECLARE @firstUpdate tinyint
--DECLARE @tempColumnName varchar(4000)

SET @firstUpdate = 1
DECLARE @Includecounter int
SET @Includecounter=1
DECLARE @Includecounter2 int
SET @Includecounter2 = 0
WHILE @Includecounter <= (select max(ID) FROM @tempIndexes)
BEGIN
	SET @schemaName =	(SELECT SchemaName FROM @tempIndexes WHERE ID = @Includecounter)
	SET @TableID =		(SELECT TableID FROM @tempIndexes WHERE ID = @Includecounter)
	SET @tableName =	(SELECT tableName FROM @tempIndexes WHERE ID = @Includecounter)
	SET @IndexName =	(SELECT IndexName FROM @tempIndexes WHERE ID = @Includecounter)
	SET @ColumnName =	(SELECT ColumnName FROM @tempIndexes WHERE ID = @Includecounter)
	SET @ColumnUsage =	(SELECT ColumnUsage FROM @tempIndexes WHERE ID = @Includecounter)
	SET @indexColumnID =(SELECT index_Column_ID FROM @tempIndexes WHERE ID = @Includecounter)
--PRINT @schemaName + ' | ' + convert(varchar,@TableID) + ' | ' + @tableName+ ' | ' +  @IndexName + ' | ' + @ColumnName+ ' | ' +  @ColumnUsage+ ' | ' + convert(varchar, @indexColumnID)
	IF @ColumnUsage = 'KEY' AND @indexColumnID = 1
		BEGIN
		SET @IncludeCounter2=@IncludeCounter2+1
--		Print (@counter2)
		INSERT INTO @tempIndexesInclude(schemaName,TableID,tableName,IndexName,ColumnName,IncludeColumns)
		SELECT quotename(@schemaName),@TableID,quotename(@tableName),@IndexName,quotename(@ColumnName),''
		SET @firstUpdate = 1
		END
	IF @ColumnUsage = 'KEY' AND @indexColumnID <> 1
		BEGIN
--		set @tempColumnName = (SELECT ColumnName FROM @tempIndexesInclude WHERE schemaName=quotename(@schemaName) AND TableID = @TableID AND IndexName = @IndexName )
--		PRINT @tempColumnName +  ', ' + quotename(@ColumnName) 
		UPDATE @tempIndexesInclude
		SET ColumnName = ColumnName +  ', ' + quotename(@ColumnName) 
		WHERE schemaName=quotename(@schemaName)
		AND TableID = @TableID
		AND IndexName = @IndexName
		END
	IF @ColumnUsage <> 'KEY'
		BEGIN
		UPDATE @tempIndexesInclude
		SET IncludeColumns = IncludeColumns + CASE WHEN @firstUpdate = 1 THEN '' ELSE ', ' END + quotename(@ColumnName) 
		WHERE schemaName=quotename(@schemaName)
		AND TableID = @TableID
		AND IndexName = @IndexName
		SET @firstUpdate = 0
		END

--	print @firstupdate
	set @Includecounter = @Includecounter+1
END -- WHILE
--SELECT * FROM @tempIndexesInclude

	--generate SQL to do indexes
	declare 	@IX_indid smallint,	-- the index id of an index
			@IX_groupid smallint,  -- the filegroup id of an index
			@IX_indname sysname,
			@IX_groupname sysname,
			@IX_status int,
			@IX_keys nvarchar(3000),	
			@IX_include nvarchar(3000),
			@IX_dbname	sysname,
			@ix_schemaname sysname,
			@IX_ObjID int,
			@IX_ObjName sysname,
			@IX_OrigFillFactor tinyint,
			@xType varchar(10)
			,@IX_Filter_Definition NVARCHAR(1000)
	-- Check to see the the table exists and initialize @IX_objid.

	-- OPEN CURSOR OVER INDEXES (skip stats: bug shiloh_51196)
	declare ms_crs_ind cursor local static for
	select o.xtype,schema_name(o.uid),i.id, object_name(i.id), indid, groupid, i.name, status, OrigFillFactor, inc.IncludeColumns,ISNULL(i2.filter_definition,'NoFilter')	-- SELECT *
	from sysindexes i 
	left join (select id,uid,xtype from sysobjects) o
	on i.id=o.id
	INNER JOIN sys.indexes i2
	ON i2.object_id = o.id
	AND i2.index_id = i.indid
	LEFT JOIN @tempIndexesInclude inc
	ON quotename(schema_name(o.uid)) = inc.schemaName
	AND i.name = inc.IndexName
	AND quotename(object_name(i.id))= inc.TableName
	where /*id = @IX_objid and */indid > 0 and indid < 255 and (status & 64)=0 
	and  objectproperty(i.id,'ISMSSHIPPED')=0
	and objectproperty(i.id,'IsTableFunction')=0 
	order by object_name(i.id),indid


	open ms_crs_ind
	fetch ms_crs_ind into @xType, @ix_schemaname,@IX_objid, @IX_ObjName,@IX_indid, @IX_groupid, @IX_indname, @IX_status, @IX_OrigFillFactor, @IX_Include,@IX_Filter_Definition

	-- Now check out each index, figure out its type and keys and
	--	save the info in a temporary table that we'll print out at the end.
	while @@fetch_status >= 0
	begin
		-- First we'll figure out what the keys are.
		declare @IX_i int, @IX_thiskey nvarchar(133) -- 128+5
		declare @rebuild_index bit

--select  @ix_schemaname,'[' + index_col(@ix_schemaname + '.' + @IX_objname,  @IX_indid, 1)+']'--, @IX_i = 2, @rebuild_index=@all  --parameter from application can force all to be rebuilt
		select @IX_keys = '[' + index_col(@ix_schemaname + '.' + @IX_objname, @IX_indid, 1)+']', @IX_i = 2, @rebuild_index=@all  --parameter from application can force all to be rebuilt
		if (indexkey_property(@IX_objid, @IX_indid, 1, 'IsDescending') = 1)
			set @IX_keys = @IX_keys  + ' DESC'
--print ('@IX_objid: ' + convert(varchar(20),@IX_objid)  +  '@IX_indid" ' +  convert(varchar(20),@IX_indid) )
		if (select collationid from syscolumns where id=@IX_objid and colid=indexkey_property(@IX_objid, @IX_indid, 1, 'columnid')) is not null
			set @rebuild_index=1

		set @IX_thiskey = '[' + index_col(@ix_schemaname + '.' +@IX_objname, @IX_indid, @IX_i) + ']'
		if ((@IX_thiskey is not null) and (indexkey_property(@IX_objid, @IX_indid, @IX_i, 'IsDescending') = 1))
			set @IX_thiskey = @IX_thiskey + ' DESC'

		if (select collationid from syscolumns where id=@IX_objid and colid=indexkey_property(@IX_objid, @IX_indid, @IX_i, 'columnid')) is not null
			set @rebuild_index=1

		while (@IX_thiskey is not null )
		begin
			select @IX_keys = @IX_keys + ', ' + @IX_thiskey, @IX_i = @IX_i + 1

			if (select collationid from syscolumns where id=@IX_objid and colid=indexkey_property(@IX_objid, @IX_indid, @IX_i, 'columnid')) is not null
				set @rebuild_index=1

			set @IX_thiskey = '[' + index_col(@ix_schemaname + '.' + @IX_objname, @IX_indid, @IX_i) + ']'
			if ((@IX_thiskey is not null) and (indexkey_property(@IX_objid, @IX_indid, @IX_i, 'IsDescending') = 1))
				select @IX_thiskey = @IX_thiskey + ' DESC'
		end

		select @IX_groupname = groupname from sysfilegroups where groupid = @IX_groupid

		-- INSERT ROW FOR INDEX
		if @rebuild_index =1 
--select @IX_ObjName,@IX_indname, @IX_status, @IX_groupname, @IX_keys, @IX_OrigFillFactor

			insert into #spindtab values (@ix_schemaname,@IX_ObjName,@IX_indname, @IX_status, @IX_groupname, @IX_keys, @IX_OrigFillFactor, @IX_Include, @xType,@IX_Filter_Definition)

		-- Next index
		fetch ms_crs_ind into @xType, @ix_schemaname, @IX_objid, @IX_ObjName,@IX_indid, @IX_groupid, @IX_indname, @IX_status, @IX_OrigFillFactor, @IX_Include,@IX_Filter_Definition
	end
	deallocate ms_crs_ind 

	-- SET UP SOME CONSTANT VALUES FOR OUTPUT QUERY
	declare @IX_empty varchar(1) select @IX_empty = ''
	declare @IX_des1			varchar(35),	-- 35 matches spt_values
			@IX_des2		varchar(35),
			@IX_des4		varchar(35),
			@IX_des32		varchar(35),
			@IX_des64		varchar(35),
			@IX_des2048		varchar(35),
			@IX_des4096		varchar(35),
			@IX_des8388608		varchar(35),
			@IX_des16777216	varchar(35)

	select @IX_des1 = name from master.dbo.spt_values where type = 'I' and number = 1 --ignoor duplicate keys
	select @IX_des2 = name from master.dbo.spt_values where type = 'I' and number = 2 --unique
	select @IX_des4 = name from master.dbo.spt_values where type = 'I' and number = 4 --ignoor duplicate rows
	select @IX_des32 = name from master.dbo.spt_values where type = 'I' and number = 32 --hypothetical
	select @IX_des64 = name from master.dbo.spt_values where type = 'I' and number = 64 --statistics
	select @IX_des2048 = name from master.dbo.spt_values where type = 'I' and number = 2048  --primary key
	select @IX_des4096 = name from master.dbo.spt_values where type = 'I' and number = 4096  --unique key
	select @IX_des8388608 = name from master.dbo.spt_values where type = 'I' and number = 8388608  --auto create
	select @IX_des16777216 = name from master.dbo.spt_values where type = 'I' and number = 16777216 --stats no recompute

insert into #SQL
select case when (stats & 4096)<>0 or (stats & 2048) <> 0 then
	--Constraint		
	'ALTER TABLE ['+schemaname+'].['+objectname+'] DROP CONSTRAINT ['+index_name+'] ' 
	else
	-- index
	'DROP INDEX ['+schemaname+'].['+objectname+'].['+ index_name +'] '
	end
from 	#spindtab 

/*drop calculated columns*/
insert into #SQL (SQL)
select 'ALTER TABLE ['+ schema_name(o.uid) +'].['+ object_name(c.id)+ '] drop column ['+c.name+']'
from 	syscolumns  c
inner join sysobjects o
on c.id=o.id
where 	iscomputed=1 
and 	objectproperty(c.id,'IsMSShipped')=0 
and 	objectproperty(c.id,'IsTable')=1


--Drop Table #Views
IF (SELECT object_id('tempdb..#Views')) IS NOT NULL
	DROP TABLE #Views;
CREATE TABLE #Views (
ID int identity(1,1),
SOID int,
ViewName varchar(100),
SchemaName varchar(20),
DEPID int,
Depth int,
FText Varchar(4000),
FtextOrder int
)
INSERT INTO #Views (SOID, ViewName, SchemaName , DEPID, Depth, FText, FtextOrder)
select distinct so.id SOid,so.name , SCHEMA_NAME(uid), sd.depid, 0, sc.text, sc.COLID
from sysobjects so
LEFT join sysdepends sd
on so.id = sd.id
join 	syscomments sc
on		so.id = sc.id
where 	objectproperty(so.id,'IsMSShipped')=0 
and ( 
		( objectproperty(so.id,'IsView')=1 AND  objectproperty(so.id,'IsIndexed')=1 )
	or ( objectproperty(so.id,'IsView')=1 AND  objectproperty(so.id,'IsSchemaBound')=1 )
	)
	
WHILE EXISTS (SELECT 1 FROM #Views a
				inner join #Views b
				on a.DepID=b.SOID 
				and a.Depth=b.Depth)
	UPDATE A set A.depth=B.Depth+1
	FROM #Views AS A
	INNER JOIN  #Views AS B 
	ON (A.DepID=B.SOID)
--	Select * From #Views

insert into #SQL (SQL)
select distinct 'DROP VIEW ['+ schemaName+ '].['+ ViewName+ ']'
from #Views	

--	 SELECT * from #SQL WHERE SQL LIKE '%NarrElemPer%'

-- Set database collation to the new one:
insert into #SQL (SQL) 
--declare @p_DatabaseName varchar(100) set @p_DatabaseName='TheGridCollationTest'
SELECT 'EXEC Master.dbo.Kill_SPIDS ' + quoteName(@p_DatabaseName) + ''
;
insert into #SQL (SQL) 
SELECT 'ALTER DATABASE ' + quoteName(@p_DatabaseName) + ' SET  SINGLE_USER WITH ROLLBACK IMMEDIATE'
;
insert into #SQL (SQL) 
SELECT 'ALTER DATABASE ' + quoteName(@p_DatabaseName) + ' SET  MULTI_USER WITH ROLLBACK IMMEDIATE'
;
insert into #SQL (SQL) 
SELECT 'Alter database ' + @p_DatabaseName + ' COLLATE ' + @p_NewCollationName




-- script out the changing of column level collation
declare @CC_SchemaName sysname,
	@CC_TableName sysname,
	@ColName sysname,
	@CC_Length nvarchar(100),
	@CC_TypeName sysname,
	@CC_OtherText nvarchar(4000),
	@CC_NullText nvarchar(100)

set 	@C = cursor for 
select 		schema_name(o.uid),o.name as tablename, 
		c.name as colname, 
		case when t.name like 'n%' then cast(c.length / 2 as nvarchar(100)) else cast(c.length as nvarchar(100)) end as Length, 
		t.name as typename,
		case when c.isnullable=1 then 'NULL' else 'NOT NULL' end as nullable
from 		sysobjects o
join 		syscolumns c
on		o.id = c.id
join 		systypes t
on		t.xtype = c.xtype
and		t.xusertype = c.xusertype
where 		o.type ='U'
and		objectproperty(o.id,'IsMSShipped')=0
and             c.iscomputed<>1
and 		c.collationid is not null 
and t.name NOT IN (select name from systypes
				where scale is not null
				--order by length
				) order by c.name

--and 		c.collation <> cast(DATABASEPROPERTYEX(DB_NAME(),'collation') as sysname)
--and t.name='binary'
--order by tablename,colname

open @C
fetch next from @C into @CC_SchemaName,@CC_TableName, @ColName, @CC_Length, @CC_TypeName,@CC_NullText
while @@Fetch_Status = 0
begin
	IF ((@CC_Length = '-1') or (@CC_Length = '0' )) 
		set @CC_Length='max'
	if @CC_TypeName COLLATE DATABASE_DEFAULT in ('ntext','text')
	begin
		-- we can not use the alter table statment to change column level collation on text columns
		--we need to do each of these as a separate transaction due to the risks of errors
		if NOT exists (	select 	* 
			from 	sysconstraints  
			where 	id = object_id(@CC_TableName) 
			and 	col_name(id,colid) = @ColName
			and	(status & 5) = 5 )

			set @SQLSegment = 'Alter table ['+@CC_SchemaName+'].['+@CC_TableName+'] Alter Column [' + @ColName + '] [nvarchar]  (MAX) NULL   
			'
		-- if default constraint must add it
		ELSE
			-- if there are default constraints add a bit to do that
			set @SQLSegment = @SQLSegment + 
				(select '
				exec (''Alter table ['+@CC_SchemaName+'].['+@CC_TableName+'] drop constraint  [' + object_name(c.constid) + ']'')
				exec (''Alter table ['+@CC_SchemaName+'].['+@CC_TableName+'] drop column [' + @ColName + ']'')
				exec ('' ALTER TABLE ['+schema_name(o2.uid)+'].['+ object_name(c.id) + '] ADD ['+o.name+'] [' + @CC_TypeName + '] CONSTRAINT [' + object_name(c.constid) + '] DEFAULT ' + replace(t.text,'''','''''') + '  '')'
				from 	sysconstraints  c
				join 	syscolumns o
				on	c.id = o.id
				and	c.colid = o.colid
				join sysobjects o2
				on o2.id=c.id
				join 	syscomments t
				on	t.id = c.constid
				where 	c.id = object_id(@CC_TableName) 
				and 	(c.status & 5) = 5
				and 	col_name(c.id,c.colid) = @ColName) --default constraint
	/*	else
			if @CC_TypeName COLLATE DATABASE_DEFAULT in ('ntext','text')
			set @SQLSegment = @SQLSegment + '
				exec (''Alter table ['+@CC_SchemaName+'].['+@CC_TableName+'] drop column [' + @ColName + ']'')
				exec (''Alter table ['+@CC_SchemaName+'].['+@CC_TableName+'] add [' + @ColName+'] [' + @CC_TypeName + '] '')'
*/
/*
		set @SQLSegment =  @SQLSegment  + '
			exec (''update ['+@CC_SchemaName+'].['+@CC_TableName+'] set [' + @ColName + '] = [____temp] '')
			exec (''alter table ['+@CC_SchemaName+'].['+@CC_TableName+'] drop column [____temp]'')
			'
*/		if @CC_TypeName = 'NOT NULL'
			set @SQLSegment =  @SQLSegment  + '
				exec (''Alter table ['+@CC_SchemaName+'].['+@CC_TableName+'] Alter column [' + @ColName+'] [' + @CC_TypeName + '] '+@CC_NullText+' '')'
--IF @CC_TypeName='varbinary'	SELECT '1',@CC_Length
		insert into #SQL values (@SQLSegment)

	end
	else
	begin
		-- normal columns
		IF @CC_TypeName NOT IN ('timestamp')
		BEGIN
			set @SQLSegment = 'Alter table ['+@CC_SchemaName+'].['+@CC_TableName COLLATE DATABASE_DEFAULT+'] Alter Column ['+@ColName COLLATE DATABASE_DEFAULT+ '] ['+@CC_TypeName COLLATE DATABASE_DEFAULT+'] ' 
			if @CC_TypeName COLLATE DATABASE_DEFAULT in ('nvarchar', 'varchar','char','nchar','binary','varbinary','text','ntext')
				set @SQLSegment = @SQLSegment COLLATE DATABASE_DEFAULT +' ('+@CC_Length COLLATE DATABASE_DEFAULT  + ') '
			set @SQLSegment = @SQLSegment	COLLATE DATABASE_DEFAULT +  /*' COLLATE DATABASE_DEFAULT ' +*/ @CC_NullText COLLATE DATABASE_DEFAULT + '
	' 
		END
--IF @CC_TypeName='varbinary'	SELECT '2',@CC_Length
		insert into #SQL values (@SQLSegment)
	end	
	fetch next from @C into @CC_SchemaName, @CC_TableName, @ColName, @CC_Length, @CC_TypeName,@CC_NullText
end

close @C
deallocate @C

--script out recreation of calculated columns
insert into #SQL
select 'ALTER TABLE ['+schema_name(o.uid)+'].['+ object_name(c.id)+ '] ADD ['+c.name+'] AS '+sc.text --	SELECT *
from 	syscolumns c
inner join sysobjects o
on o.id=c.id
join 	syscomments sc
on	c.id = sc.id
and	c.colid = sc.number
where 	c.iscomputed=1 
and 	objectproperty(c.id,'IsMSShipped')=0 
and 	objectproperty(c.id,'IsTable')=1 


-- script out recreation of check constraints
insert into #SQL
select 		'Alter table ['+schema_name(o.uid)+'].[' + Object_Name(cs.id) + '] WITH NOCHECK ADD CONSTRAINT ['+object_name(cs.constid)+'] CHECK '+sc.text + '
' + case when objectproperty(cs.constid,'CnstIsDisabled') = 1 then 'Alter table [' + Object_Name(cs.id) + '] NOCHECK CONSTRAINT ['+object_name(cs.constid)+']' else '' end
from 		sysconstraints cs
join 		syscomments sc
on		sc.id = cs.constid
join	sysobjects o
on o.id=sc.id
where 	objectproperty(cs.constid,'IsCheckCnst') = 1 


-- Script out the creation of the table indexed and schemabound views before indexes!

--	 SELECT * from #SQL WHERE SQL LIKE '%NarrElemPer%'

--first switch to use the correct database
insert into #SQL (SQL) values ('')

--get a text pointer
SELECT @textptr = TEXTPTR(SQL) FROM #SQL where ID = (select max(ID) from #SQL)

set @SQLSegment = '
USE ' + @p_DatabaseName + '
'
UPDATETEXT #SQL.SQL @textptr NULL 0 @SQLSegment

declare @ViewName sysname,
	@LastViewName sysname

set 	@LastViewName =''
set @C = cursor for
	Select max(depth) depth, SOID, ViewName, SchemaName,Ftext--, FtextOrder
	From #Views --Created earlier in the code, at the DROP FUNCTIONS LINE
	group by SOID, SchemaName , ViewName, Ftext, FtextOrder 
--	order by depth, SchemaName, ViewName, FtextOrder
/*
select 		name as functionName, 
		sc.text, sc.id, sc.colid
from 		sysobjects o
join 		syscomments sc
on		o.id = sc.id
where (	objectproperty(o.id,'IsTableFunction') =1
	or objectproperty(o.id,'IsScalarFunction')=1
	or objectproperty(o.id,'IsInlineFunction')=1 )
order by	o.name, 
		sc.colid  
*/

		insert into #SQL (SQL) values ('')

open @C
fetch next from @C into @depth, @soid, @ViewName, @schemaName, @SQLSegment
while @@Fetch_Status=0
begin
	if @ViewName<>@LastViewName
	begin
		insert into #SQL (SQL) values ('')
		--get a text pointer
		SELECT @textptr = TEXTPTR(SQL) FROM #SQL where ID = (select max(ID) from #SQL)
		set @LastViewName =@ViewName
	end
	UPDATETEXT #SQL.SQL @textptr NULL 0 @SQLSegment
		insert into #SQL (SQL) values ('')

--		insert into #SQL (SQL) values (@SQLSegment)
	fetch next from @C into @depth, @soid, @ViewName, @schemaName, @SQLSegment
end
Close @C
deallocate @C


-- script out the recreation of indexes
	-- DISPLAY THE RESULTS
	UPDATE a SET includeColumns = SUBSTRING(includeColumns,2,LEN(includeColumns) - 1)
	--	SELECT SUBSTRING(includeColumns,2,LEN(includeColumns) - 1),* 
	FROM #spindtab a
	WHERE LEFT(includeColumns,1) = ',';
--	SELECT includeColumns FROM #spindtab

-- Have to update the temp table since below contrainsts and indexes were NULLed out from the CASE statement
UPDATE a
SET ix_filter_definition = 'WHERE ' + ix_filter_definition
FROM	-- SELECT * FROM
 #spindtab a
 Where ix_filter_definition <> 'NoFilter'
UPDATE a
SET ix_filter_definition = ' '
FROM	-- SELECT * FROM
#spindtab a
Where ix_filter_definition = 'NoFilter'

	insert into #SQL
	select 
		--schemaname,objectname,index_name,index_keys,includecolumns,ix_filter_definition,origfillfactor,@ix_empty,groupname,
		case when (stats & 4096)<>0 or (stats & 2048) <> 0 then 
		--Constraint		
		'ALTER TABLE ['+schemaname+'].['+objectname+'] ADD CONSTRAINT ['+index_name+'] '
		+ case when (stats & 2048)<>0 then 'PRIMARY KEY ' else 'UNIQUE ' end
		+ case when (stats & 16)<>0 then 'clustered' else 'nonclustered' end
		+ ' ('+index_keys+')'
		+ CASE WHEN includeColumns <> '' THEN ' INCLUDE ('+ includeColumns + ') ' ELSE '' END
		+ /*case WHEN IX_Filter_Definition <> 'NoFilter' THEN 'WHERE ' +*/ IX_Filter_Definition --END
		+ case when OrigFillFactor > 0 then ' WITH FILLFACTOR =' + cast(OrigFillFactor as nvarchar(3)) else @IX_empty end
		+ ' ON ['+groupname+']
' collate database_default
		else 
		-- index
		'CREATE ' + case when (stats & 2)<>0 then @IX_des2 +' ' else @IX_empty end +case when (stats & 16)<>0 then 'clustered' else 'nonclustered' end +' INDEX'
		+ ' ['+ index_name +'] on ['+schemaname+'].['+objectname+'] ('+index_keys+')'
		+ CASE WHEN includeColumns <> '' THEN ' INCLUDE ('+ includeColumns + ') ' ELSE '' END
		+ /*case WHEN IX_Filter_Definition <> 'NoFilter' THEN 'WHERE ' +*/ IX_Filter_Definition --END
		+ case when (OrigFillFactor >0 or (stats & 1) <> 0 or (stats & 16777216) <> 0 ) AND xType <> 'V' then ' WITH ' else @IX_empty end
		+ case when OrigFillFactor >0 then 'PAD_INDEX, FILLFACTOR = ' +cast(OrigFillFactor as nvarchar(3) ) else @IX_empty end
		+ case when (stats & 1) <> 0 then ', '+ @IX_des1  else @IX_empty end
		+ case when (stats & 16777216) <> 0 AND xType <> 'V' then ', '+ @IX_des16777216  else @IX_empty end
		+ ' ON ['+groupname+']
'
			end
		-- SELECT *, stats & 4096,stats & 2048,stats & 16777216
	from 	#spindtab --where left(index_name ,4) = 'pk_r'
--	WHERE objectname = 'ivw_OBS_Hierarchy_Top_Level_Structures'
	--drop table #spindtab
	-- SELECT * FROM #SQL WHERE SQL LIKE '%PK_R%'
-- script recreation of foiegn keys

-- script out foreign keys
declare	@FK_KeyName sysname,
	@FK_SchemaName sysname,
	@FK_TableName sysname,
	@FK_ReferencedTable sysname,
	@ConstID int,
	@Col1 sysname,
	@Col2 sysname,
	@ColList1 nvarchar(2000),
	@ColList2 nvarchar(2000),
	@CnstIsUpdateCascade bit,
	@CnstIsNotRepl bit,
	@CnstIsDeleteCascade bit,
	@CnstIsDisabled bit,
	@C2 cursor

set @C = cursor for 
select DISTINCT schema_name(o.uid) as SchemaName,	
	object_Name(c.id) as TableName,
	object_name(c.constid) as KeyName,
	object_name(rkeyid),--(select distinct object_name(rkeyid) from sysforeignkeys fk where fk.constid = c.constid) as ReferencedTable,
	c.constid,
	objectproperty(c.constid,'CnstIsUpdateCascade') CnstIsUpdateCascade,
	objectproperty(c.constid,'CnstIsDeleteCascade') CnstIsDeleteCascade,
	objectproperty(c.constid,'CnstIsNotRepl') CnstIsNotRepl,
	objectproperty(c.constid,'CnstIsDisabled') CnstIsDisabled
	-- SELECT *
from 	sysconstraints c
inner join sysobjects o
on o.id = c.id
INNER JOIN sysforeignkeys fk
ON fk.constid = c.constid 
where 	objectproperty(c.constid,'IsForeignKey')=1 
and 	c.constid in  (
	select 	fk.constid
	from 	sysforeignkeys fk
	join 	syscolumns fc
	on	fc.colid = fk.fkey
	and	fc.id = fk.fkeyid
	join 	syscolumns rc
	on	rc.colid = fk.rkey
	and	rc.id = fk.rkeyid
	where	fc.collationid is not null
	or 	rc.collationid is not null 
	or	@all=1)  --paramater allows all constraints to be dropped
	
open @C 
fetch next from @C into @FK_SchemaName, @FK_TableName, @FK_KeyName, @FK_ReferencedTable,@ConstID, @CnstIsUpdateCascade, @CnstIsDeleteCascade, @CnstIsNotRepl,@CnstIsDisabled
while @@fetch_Status =0
begin
	set @ColList1 = ''
	set @ColList2 = ''
	set @C2 = Cursor for
	select  fc.name,
		rc.name
	from 	sysforeignkeys fk
	join 	syscolumns fc
	on	fc.colid = fk.fkey
	and	fc.id = fk.fkeyid
	join 	syscolumns rc
	on	rc.colid = fk.rkey
	and	rc.id = fk.rkeyid
	where 	fk.constid = @ConstID 

	open @C2
	fetch next from @C2 into @Col1, @Col2
	while @@Fetch_status=0
	begin
		if len(@ColList1) > 0 
			set @ColList1 = @ColList1 collate database_default+', '
		if len(@ColList2) > 0 
			set @ColList2 = @ColList2 collate database_default+', '
		set @ColList1 = @ColList1 collate database_default +'[' + @Col1 collate database_default + ']'
		set @ColList2 = @ColList2 collate database_default +'[' + @Col2 collate database_default + ']'
		fetch next from @C2 into @Col1, @Col2
	end
	close @C2
	deallocate @C2

	set @SQLSegment = 'Alter table [' + @FK_SchemaName + '].['+ @FK_TableName collate database_default + '] WITH NOCHECK ADD CONSTRAINT [' 
		+ @FK_KeyName collate database_default + '] FOREIGN KEY ('+@ColList1 collate database_default 
		+ ') REFERENCES [' + @FK_SchemaName + '].[' + @FK_ReferencedTable collate database_default+'] ('+ @ColList2 collate database_default +')'
	if @CnstIsUpdateCascade =1
		set @SQLSegment =@SQLSegment + ' ON UPDATE CASCADE'
	if @CnstIsDeleteCascade =1
		set @SQLSegment =@SQLSegment + ' ON DELETE CASCADE'
	if @CnstIsNotRepl =1
		set @SQLSegment =@SQLSegment + ' NOT FOR REPLICATION'
	set @SQLSegment = @SQLSegment +'
'
	insert into #SQL values (@SQLSegment)

	if @CnstIsDisabled=1
	begin
		set @SQLSegment = 'Alter table ['+ @FK_TableName + '] NOCHECK CONSTRAINT [' + @FK_KeyName + ']
'
		insert into #SQL values (@SQLSegment)
	end
	fetch next from @C into @FK_SchemaName, @FK_TableName, @FK_KeyName, @FK_ReferencedTable,@ConstID, @CnstIsUpdateCascade, @CnstIsDeleteCascade, @CnstIsNotRepl,@CnstIsDisabled
end

close @C
deallocate @C 

-- SCRIPT RECREATION OF FULL TEXT INDEXES
DECLARE @Icounter INT

SET @fticounter = 1
WHILE @fticounter <= ( SELECT MAX(ID) FROM @FTIs )
BEGIN
	SET @Catalog = ( SELECT FTI FROM @FTIs WHERE ID = @fticounter )
	BEGIN
		-- Store the catalog details
		SELECT	@CatalogID = i.fulltext_catalog_id
				,@ObjectID = 0
				,@Max_objectId = MAX(object_id)
				,@AccentOn = is_accent_sensitivity_on				
		FROM	sys.fulltext_index_catalog_usages as i
		JOIN	sys.fulltext_catalogs c 
			ON	i.fulltext_catalog_id = c.fulltext_catalog_id
		WHERE	c.Name = @Catalog
		GROUP BY i.fulltext_catalog_id,is_accent_sensitivity_on

		-- Script out catalog
		SET @SQL = 'CREATE FULLTEXT CATALOG ' + @Catalog + ' WITH ACCENT_SENSITIVITY = ' + CASE @AccentOn WHEN 1 THEN 'ON' ELSE 'OFF' END + ';'
		insert into #SQL (SQL)
		SELECT @SQL

		IF (SELECT object_id('tempdb..#FIndexes')) IS NOT NULL
			DROP TABLE #FIndexes;
		CREATE TABLE #FIndexes (
		ID INT IDENTITY(1,1)
		,ObjectID BIGINT
		,[Owner] VARCHAR(100)
		,[Table] VARCHAR(100)
		,IndexID BIGINT
		)
		TRUNCATE TABLE #Findexes
		INSERT INTO #FIndexes (ObjectID, [Owner],[Table],IndexID )
		SELECT	
				ObjectID = i.object_id
				,[Owner] = u.Name
				,[Table] = t.Name
				,IndexID = unique_index_id	-- SELECT *
		FROM	sys.tables as t
		JOIN	sysusers as u
			ON	u.uid = t.schema_id
		JOIN	sys.fulltext_indexes i 
			ON	t.object_id = i.object_id
		JOIN	sys.fulltext_catalogs c 
			ON i.fulltext_catalog_id = c.fulltext_catalog_id
		WHERE	c.Name =  @Catalog

		-- Loop through all fulltext indexes within catalog
		SET @Icounter = 1
		WHILE @Icounter <= ( SELECT MAX(ID) FROM #FIndexes )
		  BEGIN
				SELECT	
						@ObjectID = ObjectID
						,@Owner = [Owner]
						,@Table = [Table]
						,@IndexID = IndexID	-- SELECT *
				FROM #FIndexes
				WHERE ID = @Icounter

				-- Script Fulltext Index
				SELECT	@COLS = NULL,
						@SQL =  'CREATE FULLTEXT INDEX ON ' + QUOTENAME(@Owner)+'.'+QUOTENAME(@Table)+' ('+@NL

				-- Script columns in index
				SELECT	@COLS = COALESCE(@COLS+',','') + c.Name + ' Language ' + CAST(Language_id as varchar) +' '+@NL
				FROM	sys.fulltext_index_columns as fi
				JOIN	sys.columns as c
					ON	c.object_id = fi.object_id
						AND c.column_id = fi.column_id
				WHERE	fi.object_id = @ObjectID
				
				-- Script unique key index
				SELECT	@SQL = @SQL + @COLS + ') ' + @NL + 'KEY INDEX '+ i.Name + @NL+
						'ON ' + @Catalog + @NL +
						'WITH CHANGE_TRACKING ' + fi.change_tracking_state_desc + @NL + ';' + @NL
				FROM	sys.indexes as i
				JOIN	sys.fulltext_indexes as fi
					ON	i.object_id = fi.object_id
				WHERE	i.Object_ID=@ObjectID
						AND Index_Id = @IndexID
				
				-- Output script SQL
--				PRINT @SQL
				SET @Icounter = @Icounter + 1
				-- Output script SQL
				insert into #SQL (SQL)
				SELECT @SQL
				
		  END
	END

	SET @fticounter = @fticounter + 1
  END
-- Script out the creation of the table functions last!

--first switch to use the correct database
insert into #SQL (SQL) values ('')

--get a text pointer
SELECT @textptr = TEXTPTR(SQL) FROM #SQL where ID = (select max(ID) from #SQL)

set @SQLSegment = '
USE ' + @p_DatabaseName + '
'
UPDATETEXT #SQL.SQL @textptr NULL 0 @SQLSegment

declare @FunctionName sysname,
	@LastFunctionName sysname

IF (SELECT object_id('tempdb..#C')) IS NOT NULL
	DROP TABLE #C;
CREATE TABLE #C (
ID INT IDENTITY (1,1)
,depth INT
,SOID BIGINT
,FunctionName VARCHAR(50)
,SchemaName VARCHAR(50)
,Ftext VARCHAR(MAX)
)
set 	@LastFunctionName =''
--set @C = cursor for
INSERT INTO #c
	Select max(depth) depth, SOID, FunctionName, SchemaName, Ftext--, FtextOrder
	From #Functions --Created earlier in the code, at the DROP FUNCTIONS LINE
	group by SOID, SchemaName, FunctionName, Ftext, FtextOrder 
--	order by ID,depth, SchemaName, FunctionName, FtextOrder
/*
select 		name as functionName, 
		sc.text, sc.id, sc.colid
from 		sysobjects o
join 		syscomments sc
on		o.id = sc.id
where (	objectproperty(o.id,'IsTableFunction') =1
	or objectproperty(o.id,'IsScalarFunction')=1
	or objectproperty(o.id,'IsInlineFunction')=1 )
order by	o.name, 
		sc.colid  
*/

		insert into #SQL (SQL) values ('')

--open @C
--DECLARE @depth INT,@SOID BIGINT,@FunctionName VARCHAR(50),@SchemaName VARCHAR(50),@SQLSegment VARCHAR(MAX), @counter INT
--fetch next from @C into @depth, @soid, @FunctionName, @SchemaName, @SQLSegment
DECLARE @LastSChemaName VARCHAR(50)
SET @LastSChemaName = ' '
SET @counter = 1
--while @@Fetch_Status=0
WHILE @counter <= ( SELECT MAX(ID) FROM #c )
begin
	SET @FunctionName = ( SELECT FunctionName FROM #C WHERE ID = @counter )
	SET @SchemaName = ( SELECT SchemaName FROM #C WHERE ID = @counter )
	SET @SQLSegment = ( SELECT Ftext FROM #C WHERE ID = @counter )
	if @SchemaName + '.' + @FunctionName<> @LastSchemaName + '.' + @LastFunctionName
	begin
		--get a text pointer
		SELECT @textptr = TEXTPTR(SQL) FROM #SQL where ID = (select max(ID) from #SQL)
		set @LastFunctionName =@FunctionName
		set @LastSChemaName =@SchemaName
		insert into #SQL (SQL) values ('')
	end
	UPDATETEXT #SQL.SQL @textptr NULL 0 @SQLSegment
--		insert into #SQL (SQL) values ('')

--		insert into #SQL (SQL) values (@SQLSegment)
--	fetch next from @C into @depth, @soid, @FunctionName, @schemaName, @SQLSegment
	SET @counter = @counter + 1
end
--Close @C
--deallocate @C



select SQL from #SQL order by ID


--exec ('alter database ' + @p_DatabaseName + ' set read_write')
--set noexec on
--set xact_abort off

--	declare @cmd nvarchar(max)
declare curs_exec cursor for 
select convert(nvarchar(max),SQL)
 from #SQL --where len(convert(nvarchar(max),SQL))>1000 or left(convert(nvarchar(max),SQL),13)='DROP FUNCTION'
order by ID
open curs_exec
fetch curs_exec into @cmd
while @@FETCH_STATUS = 0
BEGIN		
	print (@cmd)
--	select len(@cmd)
	exec (@cmd)
	if @@error <> 0 
	begin
		print 'ERROR!!!' + '  -    Row ID: ' 
		--exec ('alter database ' + @p_DatabaseName + ' set multi_user')
		print @cmd
		--exec (@cmd)
	--	return -1
	end
	fetch curs_exec into @cmd
END
deallocate curs_exec
--set noexec off
--set xact_abort on

--declare @p_DatabaseName	sysname ;set @p_DatabaseName= 'TheGrid'
-- finally set back to multi user access
--exec ('alter database ' + @p_DatabaseName + ' set read_only')
--return
-------------------------------------------------------
--select * from #SQL

Drop table #SQL
Drop Table #Functions
Drop Table #C