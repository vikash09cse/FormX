CREATE OR ALTER PROCEDURE dbo.sp_get_tenant_by_subdomain
    @subdomain NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        t.tenantid,
        t.hospitalname,
        t.subdomain,
        t.tenantstatus AS status,
        t.logourl,
        t.timezone,
        t.primarycontactfirstname,
        t.primarycontactlastname,
        t.primarycontactemail
    FROM dbo.tenants t
    WHERE t.subdomain = @subdomain
      AND t.isdeleted = 0;
END
GO
