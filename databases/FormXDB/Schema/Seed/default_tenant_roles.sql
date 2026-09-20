-- Idempotent seed of default tenant roles (NullJournal re-runs seeds)
DECLARE @tenantid UNIQUEIDENTIFIER;
DECLARE @userid UNIQUEIDENTIFIER;

DECLARE tenant_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT tenantid FROM dbo.tenants;

OPEN tenant_cursor;
FETCH NEXT FROM tenant_cursor INTO @tenantid;
WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT TOP 1 @userid = userid
    FROM dbo.users
    WHERE tenantid = @tenantid AND usertype = 1 AND isdeleted = 0
    ORDER BY createdat;

    EXEC dbo.sp_role_seed_defaults @tenantid = @tenantid, @createdby = @userid;

    FETCH NEXT FROM tenant_cursor INTO @tenantid;
END
CLOSE tenant_cursor;
DEALLOCATE tenant_cursor;
GO
