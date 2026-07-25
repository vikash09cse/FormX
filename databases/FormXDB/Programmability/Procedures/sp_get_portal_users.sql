CREATE OR ALTER PROCEDURE dbo.sp_get_portal_users
    @page     INT = 1,
    @pagesize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @offset INT = (@page - 1) * @pagesize;

    SELECT
        u.userid,
        u.email,
        u.firstname,
        u.lastname,
        u.usertype,
        u.userstatus AS status,
        u.createdat,
        t.tenantid,
        t.hospitalname,
        t.subdomain,
        COUNT(*) OVER() AS totalcount
    FROM dbo.users u
    INNER JOIN dbo.tenants t ON t.tenantid = u.tenantid AND t.isdeleted = 0
    WHERE u.tenantid IS NOT NULL
      AND u.isdeleted = 0
    ORDER BY u.createdat DESC
    OFFSET @offset ROWS FETCH NEXT @pagesize ROWS ONLY;
END
GO
