CREATE OR ALTER PROCEDURE dbo.sp_delete_platform_user
    @userid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Seed super admin cannot be deleted
    IF @userid = '11111111-1111-1111-1111-111111111111'
        RETURN;

    UPDATE dbo.users
    SET isdeleted = 1,
        updatedat = SYSUTCDATETIME()
    WHERE userid = @userid
      AND tenantid IS NULL
      AND isdeleted = 0;
END
GO
