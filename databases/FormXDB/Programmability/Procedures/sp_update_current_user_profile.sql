CREATE OR ALTER PROCEDURE dbo.sp_update_current_user_profile
    @tenantid    UNIQUEIDENTIFIER,
    @userid      UNIQUEIDENTIFIER,
    @firstname   NVARCHAR(100),
    @lastname    NVARCHAR(100),
    @designation NVARCHAR(100) = NULL,
    @updatedby   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.users
    SET firstname = @firstname,
        lastname = @lastname,
        designation = @designation,
        updatedby = @updatedby,
        updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid
      AND userid = @userid
      AND isdeleted = 0;
END
GO
