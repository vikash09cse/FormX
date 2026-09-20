CREATE OR ALTER PROCEDURE dbo.sp_location_import_villages
    @tenantid  UNIQUEIDENTIFIER,
    @userid    UNIQUEIDENTIFIER,
    @rowsjson  NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @errors TABLE (rownum INT, message NVARCHAR(500));
    DECLARE @imported INT = 0;

    ;WITH src AS (
        SELECT
            CAST([key] AS INT) + 1 AS rownum,
            NULLIF(LTRIM(RTRIM(JSON_VALUE(value, '$.code'))), N'') AS code,
            NULLIF(LTRIM(RTRIM(JSON_VALUE(value, '$.name'))), N'') AS name,
            NULLIF(LTRIM(RTRIM(JSON_VALUE(value, '$.parentCode'))), N'') AS parentcode,
            COALESCE(TRY_CAST(JSON_VALUE(value, '$.status') AS TINYINT), 1) AS status
        FROM OPENJSON(@rowsjson)
    )
    INSERT INTO @errors (rownum, message)
    SELECT rownum,
        CASE
            WHEN code IS NULL OR name IS NULL OR parentcode IS NULL THEN N'Code, Name, and ParentCode (block) are required.'
            WHEN NOT EXISTS (
                SELECT 1 FROM dbo.blocks b
                WHERE b.tenantid = @tenantid AND b.code = parentcode AND b.isdeleted = 0)
                THEN N'Block ParentCode not found.'
            ELSE NULL
        END
    FROM src
    WHERE code IS NULL OR name IS NULL OR parentcode IS NULL
       OR NOT EXISTS (
            SELECT 1 FROM dbo.blocks b
            WHERE b.tenantid = @tenantid AND b.code = parentcode AND b.isdeleted = 0);

    ;WITH src AS (
        SELECT
            CAST([key] AS INT) + 1 AS rownum,
            NULLIF(LTRIM(RTRIM(JSON_VALUE(value, '$.code'))), N'') AS code,
            NULLIF(LTRIM(RTRIM(JSON_VALUE(value, '$.name'))), N'') AS name,
            NULLIF(LTRIM(RTRIM(JSON_VALUE(value, '$.parentCode'))), N'') AS parentcode,
            COALESCE(TRY_CAST(JSON_VALUE(value, '$.status') AS TINYINT), 1) AS status
        FROM OPENJSON(@rowsjson)
    ),
    ok AS (
        SELECT s.*, b.blockid
        FROM src s
        INNER JOIN dbo.blocks b ON b.tenantid = @tenantid AND b.code = s.parentcode AND b.isdeleted = 0
        WHERE NOT EXISTS (SELECT 1 FROM @errors e WHERE e.rownum = s.rownum)
    )
    MERGE dbo.villages AS t
    USING ok AS s
       ON t.tenantid = @tenantid AND t.code = s.code AND t.isdeleted = 0
    WHEN MATCHED THEN
        UPDATE SET blockid = s.blockid, name = s.name, status = s.status,
                   updatedby = @userid, updatedat = SYSUTCDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (villageid, tenantid, blockid, name, code, status, createdby, updatedby)
        VALUES (NEWID(), @tenantid, s.blockid, s.name, s.code, s.status, @userid, @userid);

    SET @imported = @@ROWCOUNT;
    SELECT @imported AS imported, (SELECT COUNT(*) FROM @errors) AS errorcount;
    SELECT rownum, message FROM @errors ORDER BY rownum;
END
GO
