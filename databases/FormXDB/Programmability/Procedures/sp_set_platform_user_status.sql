CREATE OR ALTER PROCEDURE dbo.sp_set_platform_user_status
    @userid     UNIQUEIDENTIFIER,
    @userstatus TINYINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.users
    SET userstatus = @userstatus,
        updatedat  = SYSUTCDATETIME()
    WHERE userid = @userid
      AND tenantid IS NULL
      AND isdeleted = 0;
END
GO
