CREATE OR ALTER PROCEDURE dbo.sp_global_document_template_get_by_id
    @globaldocumenttemplateid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        g.globaldocumenttemplateid,
        g.templatetype,
        g.name,
        g.subject,
        g.bodyhtml,
        g.isdefault,
        g.createdat,
        g.updatedat
    FROM dbo.global_document_templates g
    WHERE g.globaldocumenttemplateid = @globaldocumenttemplateid
      AND g.isdeleted = 0;
END
GO
