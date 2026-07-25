CREATE OR ALTER PROCEDURE dbo.sp_document_template_set_default
    @tenantid           UNIQUEIDENTIFIER,
    @documenttemplateid UNIQUEIDENTIFIER,
    @actorid            UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @templatetype TINYINT;

    SELECT @templatetype = dt.templatetype
    FROM dbo.document_templates dt
    WHERE dt.tenantid = @tenantid
      AND dt.documenttemplateid = @documenttemplateid
      AND dt.isdeleted = 0;

    IF @templatetype IS NULL
        THROW 50404, 'Document template not found.', 1;

    BEGIN TRAN;

    UPDATE dbo.document_templates
    SET isdefault = 0,
        updatedby = @actorid,
        updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid
      AND templatetype = @templatetype
      AND isdeleted = 0
      AND isdefault = 1
      AND documenttemplateid <> @documenttemplateid;

    UPDATE dbo.document_templates
    SET isdefault = 1,
        updatedby = @actorid,
        updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid
      AND documenttemplateid = @documenttemplateid
      AND isdeleted = 0;

    COMMIT TRAN;
END
GO
