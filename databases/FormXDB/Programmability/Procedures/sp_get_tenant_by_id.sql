CREATE OR ALTER PROCEDURE dbo.sp_get_tenant_by_id
    @tenantid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        t.tenantid,
        t.hospitalname,
        t.subdomain,
        t.tenantstatus AS status,
        t.primarycontactfirstname,
        t.primarycontactlastname,
        t.primarycontactemail,
        t.primarycontactphone,
        t.tenantaddress AS address,
        t.timezone,
        t.logourl,
        t.website,
        t.createdat,
        t.updatedat
    FROM dbo.tenants t
    WHERE t.tenantid = @tenantid
      AND t.isdeleted = 0;
END
GO
