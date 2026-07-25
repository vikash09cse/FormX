CREATE OR ALTER PROCEDURE dbo.sp_document_template_get_default
    @tenantid     UNIQUEIDENTIFIER,
    @templatetype TINYINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
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
      AND dt.templatetype = @templatetype
      AND dt.isdeleted = 0
      AND dt.isdefault = 1
    ORDER BY dt.updatedat DESC;
END
GO
