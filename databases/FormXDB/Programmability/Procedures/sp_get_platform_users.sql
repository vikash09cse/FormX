CREATE OR ALTER PROCEDURE dbo.sp_get_platform_users
    @page      INT = 1,
    @pagesize  INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @offset INT = (@page - 1) * @pagesize;

    SELECT
        u.userid,
        u.email,
        u.firstname,
        u.lastname,
        u.userstatus AS status,
        u.createdat,
        COUNT(*) OVER() AS totalcount
    FROM dbo.users u
    WHERE u.tenantid IS NULL
      AND u.isdeleted = 0
      AND u.userid <> '11111111-1111-1111-1111-111111111111'
    ORDER BY u.createdat DESC
    OFFSET @offset ROWS FETCH NEXT @pagesize ROWS ONLY;
END
GO
