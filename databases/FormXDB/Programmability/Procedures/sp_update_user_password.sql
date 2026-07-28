CREATE OR ALTER PROCEDURE dbo.sp_update_user_password
    @tenantid     UNIQUEIDENTIFIER,
    @userid       UNIQUEIDENTIFIER,
    @passwordhash NVARCHAR(256),
    @updatedby    UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.users
    SET passwordhash = @passwordhash,
        updatedby = @updatedby,
        updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid
      AND userid = @userid
      AND isdeleted = 0;
END
GO
