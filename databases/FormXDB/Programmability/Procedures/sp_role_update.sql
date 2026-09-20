CREATE OR ALTER PROCEDURE dbo.sp_role_update
    @tenantid   UNIQUEIDENTIFIER,
    @roleid     UNIQUEIDENTIFIER,
    @name       NVARCHAR(100),
    @isleader   BIT = 0,
    @datascope  TINYINT = 0,
    @cancreate  BIT = 1,
    @canedit    BIT = 1,
    @candelete  BIT = 1,
    @status     TINYINT,
    @updatedby  UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.roles
    SET name = @name,
        isleader = @isleader,
        datascope = @datascope,
        cancreate = @cancreate,
        canedit = @canedit,
        candelete = @candelete,
        rolestatus = @status,
        updatedby = @updatedby,
        updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid AND roleid = @roleid AND isdeleted = 0;
END
GO
