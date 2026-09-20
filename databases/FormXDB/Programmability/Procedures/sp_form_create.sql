CREATE OR ALTER PROCEDURE dbo.sp_form_create
    @formid          UNIQUEIDENTIFIER,
    @tenantid        UNIQUEIDENTIFIER,
    @projectid       UNIQUEIDENTIFIER,
    @name            NVARCHAR(200),
    @description     NVARCHAR(1000) = NULL,
    @status          TINYINT,
    @displayorder    INT,
    @collectlocation BIT = 1,
    @createdby       UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.projects
        WHERE tenantid = @tenantid AND projectid = @projectid AND isdeleted = 0 AND status = 1)
    BEGIN
        RAISERROR('Select a valid active project.', 16, 1);
        RETURN;
    END;

    INSERT INTO dbo.forms (formid, tenantid, projectid, name, description, status, displayorder, collectlocation, createdby, updatedby)
    VALUES (@formid, @tenantid, @projectid, @name, @description, @status, @displayorder, @collectlocation, @createdby, @createdby);
    SELECT @formid;
END
GO
