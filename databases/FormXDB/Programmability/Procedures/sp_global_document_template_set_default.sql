CREATE OR ALTER PROCEDURE dbo.sp_global_document_template_set_default
    @globaldocumenttemplateid UNIQUEIDENTIFIER,
    @actorid                  UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @templatetype TINYINT;

    SELECT @templatetype = g.templatetype
    FROM dbo.global_document_templates g
    WHERE g.globaldocumenttemplateid = @globaldocumenttemplateid
      AND g.isdeleted = 0;

    IF @templatetype IS NULL
        THROW 50404, 'Global template not found.', 1;

    BEGIN TRAN;

    UPDATE dbo.global_document_templates
    SET isdefault = 0,
        updatedby = @actorid,
        updatedat = SYSUTCDATETIME()
    WHERE templatetype = @templatetype
      AND isdeleted = 0
      AND isdefault = 1
      AND globaldocumenttemplateid <> @globaldocumenttemplateid;

    UPDATE dbo.global_document_templates
    SET isdefault = 1,
        updatedby = @actorid,
        updatedat = SYSUTCDATETIME()
    WHERE globaldocumenttemplateid = @globaldocumenttemplateid
      AND isdeleted = 0;

    COMMIT TRAN;
END
GO
