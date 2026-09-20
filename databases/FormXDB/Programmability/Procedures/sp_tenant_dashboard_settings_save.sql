CREATE OR ALTER PROCEDURE dbo.sp_tenant_dashboard_settings_save
    @tenantid         UNIQUEIDENTIFIER,
    @dashboardlayout  NVARCHAR(20),
    @dashboardtopn    INT,
    @actorid          UNIQUEIDENTIFIER,
    @formids          NVARCHAR(MAX) = NULL -- comma-separated GUIDs; empty/NULL = show all
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @dashboardlayout NOT IN (N'table', N'cards', N'topN')
    BEGIN
        RAISERROR('Invalid dashboard layout.', 16, 1);
        RETURN;
    END;

    IF @dashboardtopn < 5 OR @dashboardtopn > 50
    BEGIN
        RAISERROR('Top N must be between 5 and 50.', 16, 1);
        RETURN;
    END;

    BEGIN TRANSACTION;

    IF EXISTS (SELECT 1 FROM dbo.tenant_settings WHERE tenantid = @tenantid)
    BEGIN
        UPDATE dbo.tenant_settings
        SET dashboardlayout = @dashboardlayout,
            dashboardtopn = @dashboardtopn,
            updatedat = SYSUTCDATETIME()
        WHERE tenantid = @tenantid;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.tenant_settings
            (tenantsettingsid, tenantid, visitfeeamount, freevisitwindowdays, currency,
             dashboardlayout, dashboardtopn)
        VALUES
            (NEWID(), @tenantid, 0, 10, N'INR',
             @dashboardlayout, @dashboardtopn);
    END;

    DELETE FROM dbo.tenant_dashboard_forms WHERE tenantid = @tenantid;

    IF @formids IS NOT NULL AND LTRIM(RTRIM(@formids)) <> N''
    BEGIN
        INSERT INTO dbo.tenant_dashboard_forms (tenantid, formid)
        SELECT DISTINCT
            @tenantid,
            TRY_CAST(LTRIM(RTRIM(value)) AS UNIQUEIDENTIFIER)
        FROM STRING_SPLIT(@formids, N',')
        WHERE TRY_CAST(LTRIM(RTRIM(value)) AS UNIQUEIDENTIFIER) IS NOT NULL
          AND EXISTS (
              SELECT 1 FROM dbo.forms f
              WHERE f.formid = TRY_CAST(LTRIM(RTRIM(value)) AS UNIQUEIDENTIFIER)
                AND f.tenantid = @tenantid
                AND f.isdeleted = 0
                AND f.status = 1
                AND NOT EXISTS (
                    SELECT 1 FROM dbo.form_followup_configs c
                    WHERE c.tenantid = @tenantid
                      AND c.followupformid = f.formid
                      AND c.isdeleted = 0));
    END;

    COMMIT TRANSACTION;

    -- settings
    SELECT
        dashboardlayout,
        dashboardtopn
    FROM dbo.tenant_settings
    WHERE tenantid = @tenantid;

    -- selected form ids
    SELECT formid
    FROM dbo.tenant_dashboard_forms
    WHERE tenantid = @tenantid
    ORDER BY formid;
END
GO
