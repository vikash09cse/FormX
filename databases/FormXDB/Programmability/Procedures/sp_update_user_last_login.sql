CREATE OR ALTER PROCEDURE dbo.sp_update_user_last_login
    @userid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.users
    SET lastloginat = SYSUTCDATETIME()
    WHERE userid = @userid;
END
GO
