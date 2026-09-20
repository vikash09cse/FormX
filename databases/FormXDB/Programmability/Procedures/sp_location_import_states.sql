CREATE OR ALTER PROCEDURE dbo.sp_location_import_states
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
            COALESCE(TRY_CAST(JSON_VALUE(value, '$.status') AS TINYINT), 1) AS status
        FROM OPENJSON(@rowsjson)
    )
    INSERT INTO @errors (rownum, message)
    SELECT rownum, N'Code and Name are required.'
    FROM src WHERE code IS NULL OR name IS NULL;

    ;WITH src AS (
        SELECT
            CAST([key] AS INT) + 1 AS rownum,
            NULLIF(LTRIM(RTRIM(JSON_VALUE(value, '$.code'))), N'') AS code,
            NULLIF(LTRIM(RTRIM(JSON_VALUE(value, '$.name'))), N'') AS name,
            COALESCE(TRY_CAST(JSON_VALUE(value, '$.status') AS TINYINT), 1) AS status
        FROM OPENJSON(@rowsjson)
    ),
    ok AS (
        SELECT s.*
        FROM src s
        WHERE s.code IS NOT NULL AND s.name IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM @errors e WHERE e.rownum = s.rownum)
    )
    MERGE dbo.states AS t
    USING ok AS s
       ON t.tenantid = @tenantid AND t.code = s.code AND t.isdeleted = 0
    WHEN MATCHED THEN
        UPDATE SET name = s.name, status = s.status, updatedby = @userid, updatedat = SYSUTCDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (stateid, tenantid, name, code, status, createdby, updatedby)
        VALUES (NEWID(), @tenantid, s.name, s.code, s.status, @userid, @userid);

    SET @imported = @@ROWCOUNT;

    SELECT @imported AS imported, (SELECT COUNT(*) FROM @errors) AS errorcount;
    SELECT rownum, message FROM @errors ORDER BY rownum;
END
GO
