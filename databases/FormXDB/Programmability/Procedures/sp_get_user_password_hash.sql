CREATE OR ALTER PROCEDURE dbo.sp_get_user_password_hash
    @tenantid UNIQUEIDENTIFIER,
    @userid   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT u.passwordhash
    FROM dbo.users u
    WHERE u.tenantid = @tenantid
      AND u.userid = @userid
      AND u.isdeleted = 0;
END
GO
