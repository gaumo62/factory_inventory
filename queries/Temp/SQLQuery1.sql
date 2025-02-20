USE [FactoryData]
GO
/****** Object:  StoredProcedure [dbo].[SearchInTable]    Script Date: 05-08-2020 18:16:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--use FactoryData
CREATE PROCEDURE [dbo].[SearchInTable] @tableName nvarchar(50), @searchText nvarchar(50), @date tinyint
AS
SET NOCOUNT ON;
DECLARE @columnName NVARCHAR(100)
DECLARE @sql NVARCHAR(1000) = 'SELECT * FROM ' + @tableName +' WHERE '

DECLARE columns CURSOR FOR
SELECT sys.columns.name FROM sys.tables
INNER JOIN sys.columns ON sys.columns.object_id = sys.tables.object_id
WHERE sys.tables.name = @tableName

OPEN columns
FETCH NEXT FROM columns
INTO @columnName

WHILE @@FETCH_STATUS = 0

BEGIN
	if (@columnName not like '%voucher%') and (@columnName not like '%fiscal%') and (@columnName not like '%deleted%')
    begin
		if(@date = 1)
		begin
			if @columnName like '%date%' 
			begin SET @sql = @sql + @columnName + ' LIKE ''%' + @searchText + '%'' OR ' end
		end
		else
		begin
			if @columnName not like '%date%' 
			begin SET @sql = @sql + @columnName + ' LIKE ''%' + @searchText + '%'' OR ' end
		end
	end
    FETCH NEXT FROM columns
    INTO @columnName    
END

CLOSE columns;    
DEALLOCATE columns;

SET @sql = LEFT(RTRIM(@sql), LEN(@sql) - 2)
--select @sql
EXEC(@sql)
