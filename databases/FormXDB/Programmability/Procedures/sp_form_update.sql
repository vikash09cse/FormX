CREATE OR ALTER PROCEDURE dbo.sp_form_update
    @tenantid        UNIQUEIDENTIFIER,
    @formid          UNIQUEIDENTIFIER,
    @projectid       UNIQUEIDENTIFIER,
    @name            NVARCHAR(200),
    @description     NVARCHAR(1000) = NULL,
    @status          TINYINT,
    @displayorder    INT,
    @collectlocation BIT = 1,
    @updatedby       UNIQUEIDENTIFIER
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

    UPDATE dbo.forms
    SET projectid = @projectid,
        name = @name,
        description = @description,
        status = @status,
        displayorder = @displayorder,
        collectlocation = @collectlocation,
        updatedby = @updatedby,
        updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid AND formid = @formid AND isdeleted = 0;
END
GO
