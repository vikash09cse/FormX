CREATE OR ALTER PROCEDURE dbo.sp_get_current_user_profile
    @tenantid UNIQUEIDENTIFIER,
    @userid   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.userid,
        u.email,
        u.firstname,
        u.lastname,
        u.designation,
        u.usertype AS role
    FROM dbo.users u
    WHERE u.tenantid = @tenantid
      AND u.userid = @userid
      AND u.isdeleted = 0;
END
GO
