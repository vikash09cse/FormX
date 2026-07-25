CREATE OR ALTER PROCEDURE dbo.sp_document_template_get_by_id
    @tenantid            UNIQUEIDENTIFIER,
    @documenttemplateid  UNIQUEIDENTIFIER
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
      AND dt.documenttemplateid = @documenttemplateid
      AND dt.isdeleted = 0;
END
GO
