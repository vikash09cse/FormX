CREATE OR ALTER PROCEDURE dbo.sp_get_tenant_users
    @tenantid UNIQUEIDENTIFIER,
    @page     INT = 1,
    @pagesize INT = 10,
    @filter   NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @offset INT = (@page - 1) * @pagesize;
    DECLARE @like NVARCHAR(102) = NULL;

    IF @filter IS NOT NULL AND LTRIM(RTRIM(@filter)) <> ''
        SET @like = '%' + @filter + '%';

    SELECT
        u.userid,
        u.email,
        u.firstname,
        u.lastname,
        u.designation,
        u.usertype AS role,
        u.userstatus AS status,
        u.lastloginat,
        u.createdat,
        COUNT(*) OVER() AS totalcount
    FROM dbo.users u
    WHERE u.tenantid = @tenantid
      AND u.isdeleted = 0
      AND (@like IS NULL OR u.email LIKE @like OR u.firstname LIKE @like OR u.lastname LIKE @like OR u.designation LIKE @like)
    ORDER BY u.createdat DESC
    OFFSET @offset ROWS FETCH NEXT @pagesize ROWS ONLY;
END
GO
