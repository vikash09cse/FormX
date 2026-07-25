CREATE OR ALTER PROCEDURE dbo.sp_get_platform_dashboard
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        (SELECT COUNT(*) FROM dbo.tenants WHERE isdeleted = 0) AS totaltenants,
        (SELECT COUNT(*) FROM dbo.tenants WHERE isdeleted = 0 AND tenantstatus = 1) AS activetenants,
        (SELECT COUNT(*) FROM dbo.tenants WHERE isdeleted = 0 AND tenantstatus = 2) AS suspendedtenants,
        (SELECT COUNT(*) FROM dbo.users WHERE tenantid IS NOT NULL AND isdeleted = 0) AS totaltenantusers,
        CAST(0 AS INT) AS totalpatients;
END
GO
