CREATE OR ALTER PROCEDURE dbo.sp_update_tenant_user
    @tenantid             UNIQUEIDENTIFIER,
    @userid               UNIQUEIDENTIFIER,
    @firstname            NVARCHAR(100),
    @lastname             NVARCHAR(100),
    @designation          NVARCHAR(100) = NULL,
    @updatedesignation    BIT = 0,
    @usertype             TINYINT,
    @updatedby            UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.users
    SET firstname = @firstname,
        lastname = @lastname,
        designation = CASE WHEN @updatedesignation = 1 THEN @designation ELSE designation END,
        usertype = @usertype,
        updatedby = @updatedby,
        updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid
      AND userid = @userid
      AND isdeleted = 0;
END
GO
