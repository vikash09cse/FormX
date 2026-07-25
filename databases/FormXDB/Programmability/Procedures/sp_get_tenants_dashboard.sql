CREATE OR ALTER PROCEDURE dbo.sp_get_tenants_dashboard
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        t.tenantid,
        t.hospitalname,
        t.subdomain,
        t.tenantstatus AS status,
        t.createdat,
        t.primarycontactemail,
        t.timezone,
        (SELECT COUNT(*) FROM dbo.users u WHERE u.tenantid = t.tenantid AND u.isdeleted = 0) AS totalusers,
        CAST(0 AS INT) AS totalpatients,
        (SELECT MAX(la.createdat) FROM dbo.login_audit la WHERE la.tenantid = t.tenantid AND la.issuccess = 1) AS lastactivityat
    FROM dbo.tenants t
    WHERE t.isdeleted = 0;
END
GO
