CREATE OR ALTER PROCEDURE dbo.sp_update_platform_user
    @userid    UNIQUEIDENTIFIER,
    @firstname NVARCHAR(100),
    @lastname  NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.users
    SET firstname = @firstname,
        lastname  = @lastname,
        updatedat = SYSUTCDATETIME()
    WHERE userid = @userid
      AND tenantid IS NULL
      AND isdeleted = 0;
END
GO
