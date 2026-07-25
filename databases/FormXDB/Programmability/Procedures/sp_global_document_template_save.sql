CREATE OR ALTER PROCEDURE dbo.sp_global_document_template_save
    @globaldocumenttemplateid UNIQUEIDENTIFIER = NULL,
    @templatetype             TINYINT,
    @name                     NVARCHAR(200),
    @subject                  NVARCHAR(300) = NULL,
    @bodyhtml                 NVARCHAR(MAX),
    @isdefault                BIT = 0,
    @actorid                  UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @trimmedname NVARCHAR(200) = LTRIM(RTRIM(@name));
    DECLARE @isinsert BIT = CASE WHEN @globaldocumenttemplateid IS NULL THEN 1 ELSE 0 END;

    IF @templatetype NOT IN (1, 2)
        THROW 50400, 'Invalid template type.', 1;

    IF @trimmedname IS NULL OR @trimmedname = ''
        THROW 50400, 'Template name is required.', 1;

    IF @bodyhtml IS NULL OR LTRIM(RTRIM(@bodyhtml)) = ''
        THROW 50400, 'Template body is required.', 1;

    IF @templatetype = 2 AND (@subject IS NULL OR LTRIM(RTRIM(@subject)) = '')
        THROW 50400, 'Email subject is required.', 1;

    IF @isinsert = 0
       AND NOT EXISTS (
           SELECT 1 FROM dbo.global_document_templates g
           WHERE g.globaldocumenttemplateid = @globaldocumenttemplateid AND g.isdeleted = 0)
        THROW 50404, 'Global template not found.', 1;

    BEGIN TRAN;

    IF @isdefault = 1
    BEGIN
        UPDATE dbo.global_document_templates
        SET isdefault = 0,
            updatedby = @actorid,
            updatedat = SYSUTCDATETIME()
        WHERE templatetype = @templatetype
          AND isdeleted = 0
          AND isdefault = 1
          AND (@isinsert = 1 OR globaldocumenttemplateid <> @globaldocumenttemplateid);
    END

    IF @isinsert = 1
    BEGIN
        SET @globaldocumenttemplateid = NEWID();

        INSERT INTO dbo.global_document_templates (
            globaldocumenttemplateid, templatetype, name, subject, bodyhtml, isdefault,
            createdby, updatedby)
        VALUES (
            @globaldocumenttemplateid, @templatetype, @trimmedname,
            CASE WHEN @templatetype = 2 THEN LTRIM(RTRIM(@subject)) ELSE NULL END,
            @bodyhtml, @isdefault, @actorid, @actorid);

        -- Fan-out: copy new template to every existing tenant
        INSERT INTO dbo.document_templates (
            documenttemplateid, tenantid, globaldocumenttemplateid,
            templatetype, name, subject, bodyhtml, isdefault,
            createdby, updatedby)
        SELECT
            NEWID(),
            t.tenantid,
            @globaldocumenttemplateid,
            @templatetype,
            @trimmedname,
            CASE WHEN @templatetype = 2 THEN LTRIM(RTRIM(@subject)) ELSE NULL END,
            @bodyhtml,
            CASE
                WHEN @isdefault = 1 AND NOT EXISTS (
                    SELECT 1 FROM dbo.document_templates dt
                    WHERE dt.tenantid = t.tenantid
                      AND dt.templatetype = @templatetype
                      AND dt.isdefault = 1
                      AND dt.isdeleted = 0) THEN 1
                ELSE 0
            END,
            @actorid,
            @actorid
        FROM dbo.tenants t
        WHERE t.isdeleted = 0;
    END
    ELSE
    BEGIN
        -- Update global only — do not overwrite tenant copies
        UPDATE dbo.global_document_templates
        SET templatetype = @templatetype,
            name = @trimmedname,
            subject = CASE WHEN @templatetype = 2 THEN LTRIM(RTRIM(@subject)) ELSE NULL END,
            bodyhtml = @bodyhtml,
            isdefault = @isdefault,
            updatedby = @actorid,
            updatedat = SYSUTCDATETIME()
        WHERE globaldocumenttemplateid = @globaldocumenttemplateid
          AND isdeleted = 0;
    END

    COMMIT TRAN;

    SELECT @globaldocumenttemplateid AS globaldocumenttemplateid;
END
GO
