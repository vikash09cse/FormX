CREATE OR ALTER PROCEDURE dbo.sp_role_create
    @roleid    UNIQUEIDENTIFIER,
    @tenantid  UNIQUEIDENTIFIER,
    @name      NVARCHAR(100),
    @isleader  BIT,
    @status    TINYINT,
    @createdby UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.roles (roleid, tenantid, name, isleader, rolestatus, createdby, updatedby)
    VALUES (@roleid, @tenantid, @name, @isleader, @status, @createdby, @createdby);
    SELECT @roleid;
END
GO
