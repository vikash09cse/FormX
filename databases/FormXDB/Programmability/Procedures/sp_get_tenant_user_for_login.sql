CREATE OR ALTER PROCEDURE dbo.sp_get_tenant_user_for_login
    @email NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.userid,
        u.tenantid,
        u.email,
        u.passwordhash,
        u.firstname,
        u.lastname,
        u.designation,
        u.usertype,
        u.userstatus AS status,
        t.hospitalname,
        t.subdomain,
        t.tenantstatus AS tenantstatus
    FROM dbo.users u
    INNER JOIN dbo.tenants t ON t.tenantid = u.tenantid
    WHERE u.email = @email
      AND u.tenantid IS NOT NULL
      AND u.isdeleted = 0
      AND t.isdeleted = 0;
END
GO
