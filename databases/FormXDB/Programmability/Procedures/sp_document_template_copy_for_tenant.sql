CREATE OR ALTER PROCEDURE dbo.sp_document_template_copy_for_tenant
    @tenantid UNIQUEIDENTIFIER,
    @actorid  UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.document_templates (
        documenttemplateid, tenantid, globaldocumenttemplateid,
        templatetype, name, subject, bodyhtml, isdefault,
        createdby, updatedby)
    SELECT
        NEWID(), @tenantid, g.globaldocumenttemplateid,
        g.templatetype, g.name, g.subject, g.bodyhtml, g.isdefault,
        @actorid, @actorid
    FROM dbo.global_document_templates g
    WHERE g.isdeleted = 0
      AND NOT EXISTS (
          SELECT 1
          FROM dbo.document_templates dt
          WHERE dt.tenantid = @tenantid
            AND dt.globaldocumenttemplateid = g.globaldocumenttemplateid
            AND dt.isdeleted = 0);
END
GO
