CREATE OR ALTER PROCEDURE dbo.sp_role_create
    @roleid     UNIQUEIDENTIFIER,
    @tenantid   UNIQUEIDENTIFIER,
    @name       NVARCHAR(100),
    @isleader   BIT = 0,
    @datascope  TINYINT = 0,
    @cancreate  BIT = 1,
    @canedit    BIT = 1,
    @candelete  BIT = 1,
    @status     TINYINT,
    @createdby  UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.roles (
        roleid, tenantid, name, isleader, datascope, cancreate, canedit, candelete,
        rolestatus, createdby, updatedby)
    VALUES (
        @roleid, @tenantid, @name, @isleader, @datascope, @cancreate, @canedit, @candelete,
        @status, @createdby, @createdby);
    SELECT @roleid;
END
GO
