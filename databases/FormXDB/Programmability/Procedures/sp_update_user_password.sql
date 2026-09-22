CREATE OR ALTER PROCEDURE dbo.sp_update_user_password
    @tenantid        UNIQUEIDENTIFIER,
    @userid          UNIQUEIDENTIFIER,
    @passwordhash    NVARCHAR(256),
    @initialpassword NVARCHAR(100) = NULL,
    @updatedby       UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.users
    SET passwordhash = @passwordhash,
        initialpassword = @initialpassword,
        updatedby = @updatedby,
        updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid
      AND userid = @userid
      AND isdeleted = 0;
END
GO
