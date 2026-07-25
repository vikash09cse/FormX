CREATE OR ALTER PROCEDURE dbo.sp_document_template_get_list
    @tenantid     UNIQUEIDENTIFIER,
    @templatetype TINYINT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        dt.documenttemplateid,
        dt.tenantid,
        dt.globaldocumenttemplateid,
        dt.templatetype,
        dt.name,
        dt.subject,
        dt.bodyhtml,
        dt.isdefault,
        dt.createdat,
        dt.updatedat
    FROM dbo.document_templates dt
    WHERE dt.tenantid = @tenantid
      AND dt.isdeleted = 0
      AND (@templatetype IS NULL OR dt.templatetype = @templatetype)
    ORDER BY dt.templatetype, dt.name;
END
GO
