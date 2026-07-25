CREATE OR ALTER PROCEDURE dbo.sp_role_update
    @tenantid  UNIQUEIDENTIFIER,
    @roleid    UNIQUEIDENTIFIER,
    @name      NVARCHAR(100),
    @isleader  BIT,
    @status    TINYINT,
    @updatedby UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.roles
    SET name = @name, isleader = @isleader, rolestatus = @status,
        updatedby = @updatedby, updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid AND roleid = @roleid AND isdeleted = 0;
END
GO
