CREATE OR ALTER PROCEDURE dbo.sp_role_seed_defaults
    @tenantid  UNIQUEIDENTIFIER,
    @createdby UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @fe UNIQUEIDENTIFIER = NEWID();
    DECLARE @dl UNIQUEIDENTIFIER = NEWID();
    DECLARE @pl UNIQUEIDENTIFIER = NEWID();
    DECLARE @pm UNIQUEIDENTIFIER = NEWID();

    -- Field Executive: Own, CRUD own
    IF NOT EXISTS (SELECT 1 FROM dbo.roles WHERE tenantid = @tenantid AND name = N'Data Entry - Field Executive' AND isdeleted = 0)
    BEGIN
        INSERT INTO dbo.roles (roleid, tenantid, name, isleader, datascope, cancreate, canedit, candelete, rolestatus, createdby, updatedby)
        VALUES (@fe, @tenantid, N'Data Entry - Field Executive', 0, 0, 1, 1, 1, 1, @createdby, @createdby);
        INSERT INTO dbo.role_menus (roleid, menukey) VALUES (@fe, N'dashboard'), (@fe, N'my-forms');
    END;

    -- District Leader: District view only
    IF NOT EXISTS (SELECT 1 FROM dbo.roles WHERE tenantid = @tenantid AND name = N'District Leader' AND isdeleted = 0)
    BEGIN
        INSERT INTO dbo.roles (roleid, tenantid, name, isleader, datascope, cancreate, canedit, candelete, rolestatus, createdby, updatedby)
        VALUES (@dl, @tenantid, N'District Leader', 1, 1, 0, 0, 0, 1, @createdby, @createdby);
        INSERT INTO dbo.role_menus (roleid, menukey) VALUES (@dl, N'dashboard'), (@dl, N'my-forms');
    END;

    -- Project Leader / MIS Officer
    IF NOT EXISTS (SELECT 1 FROM dbo.roles WHERE tenantid = @tenantid AND name = N'Project Leader / MIS Officer' AND isdeleted = 0)
    BEGIN
        INSERT INTO dbo.roles (roleid, tenantid, name, isleader, datascope, cancreate, canedit, candelete, rolestatus, createdby, updatedby)
        VALUES (@pl, @tenantid, N'Project Leader / MIS Officer', 1, 2, 1, 1, 1, 1, @createdby, @createdby);
        INSERT INTO dbo.role_menus (roleid, menukey) VALUES (@pl, N'dashboard'), (@pl, N'my-forms');
    END;

    -- Program Manager (Sub Admin): All + edit/delete
    IF NOT EXISTS (SELECT 1 FROM dbo.roles WHERE tenantid = @tenantid AND name = N'Program Manager (Sub Admin)' AND isdeleted = 0)
    BEGIN
        INSERT INTO dbo.roles (roleid, tenantid, name, isleader, datascope, cancreate, canedit, candelete, rolestatus, createdby, updatedby)
        VALUES (@pm, @tenantid, N'Program Manager (Sub Admin)', 1, 3, 1, 1, 1, 1, @createdby, @createdby);
        INSERT INTO dbo.role_menus (roleid, menukey) VALUES (@pm, N'dashboard'), (@pm, N'my-forms');
    END;
END
GO
