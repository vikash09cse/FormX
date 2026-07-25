CREATE OR ALTER PROCEDURE dbo.sp_global_document_template_get_list
    @templatetype TINYINT = NULL
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
    WHERE g.isdeleted = 0
      AND (@templatetype IS NULL OR g.templatetype = @templatetype)
    ORDER BY g.templatetype, g.name;
END
GO
