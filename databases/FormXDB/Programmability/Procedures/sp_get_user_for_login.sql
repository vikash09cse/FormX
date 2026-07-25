CREATE OR ALTER PROCEDURE dbo.sp_get_user_for_login
    @email    NVARCHAR(100),
    @tenantid UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @tenantid IS NULL
    BEGIN
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
            CAST(NULL AS NVARCHAR(200)) AS hospitalname,
            CAST(NULL AS NVARCHAR(50))  AS subdomain,
            CAST(NULL AS TINYINT)       AS tenantstatus
        FROM dbo.users u
        WHERE u.email = @email
          AND u.tenantid IS NULL
          AND u.isdeleted = 0;
    END
    ELSE
    BEGIN
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
          AND u.tenantid = @tenantid
          AND u.isdeleted = 0
          AND t.isdeleted = 0;
    END
END
GO
