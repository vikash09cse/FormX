CREATE OR ALTER PROCEDURE dbo.sp_document_template_save
    @tenantid            UNIQUEIDENTIFIER,
    @documenttemplateid  UNIQUEIDENTIFIER,
    @name                NVARCHAR(200),
    @subject             NVARCHAR(300) = NULL,
    @bodyhtml            NVARCHAR(MAX),
    @actorid             UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @trimmedname NVARCHAR(200) = LTRIM(RTRIM(@name));
    DECLARE @templatetype TINYINT;

    SELECT @templatetype = dt.templatetype
    FROM dbo.document_templates dt
    WHERE dt.tenantid = @tenantid
      AND dt.documenttemplateid = @documenttemplateid
      AND dt.isdeleted = 0;

    IF @templatetype IS NULL
        THROW 50404, 'Document template not found.', 1;

    IF @trimmedname IS NULL OR @trimmedname = ''
        THROW 50400, 'Template name is required.', 1;

    IF @bodyhtml IS NULL OR LTRIM(RTRIM(@bodyhtml)) = ''
        THROW 50400, 'Template body is required.', 1;

    IF @templatetype = 2 AND (@subject IS NULL OR LTRIM(RTRIM(@subject)) = '')
        THROW 50400, 'Email subject is required.', 1;

    UPDATE dbo.document_templates
    SET name = @trimmedname,
        subject = CASE WHEN @templatetype = 2 THEN LTRIM(RTRIM(@subject)) ELSE NULL END,
        bodyhtml = @bodyhtml,
        updatedby = @actorid,
        updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid
      AND documenttemplateid = @documenttemplateid
      AND isdeleted = 0;

    SELECT @documenttemplateid AS documenttemplateid;
END
GO
