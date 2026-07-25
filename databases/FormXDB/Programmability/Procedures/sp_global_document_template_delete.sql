CREATE OR ALTER PROCEDURE dbo.sp_global_document_template_delete
    @globaldocumenttemplateid UNIQUEIDENTIFIER,
    @actorid                  UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.global_document_templates g
        WHERE g.globaldocumenttemplateid = @globaldocumenttemplateid AND g.isdeleted = 0)
        THROW 50404, 'Global template not found.', 1;

    UPDATE dbo.global_document_templates
    SET isdeleted = 1,
        isdefault = 0,
        updatedby = @actorid,
        updatedat = SYSUTCDATETIME()
    WHERE globaldocumenttemplateid = @globaldocumenttemplateid
      AND isdeleted = 0;
END
GO
