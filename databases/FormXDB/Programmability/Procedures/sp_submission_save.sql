CREATE OR ALTER PROCEDURE dbo.sp_submission_save
    @tenantid     UNIQUEIDENTIFIER,
    @userid       UNIQUEIDENTIFIER,
    @submissionid UNIQUEIDENTIFIER,
    @formid       UNIQUEIDENTIFIER,
    @valuesjson   NVARCHAR(MAX), -- [{ "fieldid","valuetext" }]
    @isnew        BIT,
    @actorid      UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @projectid UNIQUEIDENTIFIER;

    SELECT @projectid = f.projectid
    FROM dbo.forms f
    WHERE f.tenantid = @tenantid AND f.formid = @formid AND f.isdeleted = 0 AND f.status = 1;

    IF @projectid IS NULL
    BEGIN
        RAISERROR('Form not found, inactive, or has no project assigned.', 16, 1);
        RETURN;
    END;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.projects
        WHERE tenantid = @tenantid AND projectid = @projectid AND isdeleted = 0 AND status = 1)
    BEGIN
        RAISERROR('Form project not found or inactive.', 16, 1);
        RETURN;
    END;

    -- Form project must be allowed for user (scopes or any active)
    IF EXISTS (SELECT 1 FROM dbo.user_scopes WHERE userid = @userid)
       AND NOT EXISTS (SELECT 1 FROM dbo.user_scopes WHERE userid = @userid AND projectid = @projectid)
    BEGIN
        RAISERROR('Form project is not in your scope.', 16, 1);
        RETURN;
    END;

    BEGIN TRANSACTION;

    IF @isnew = 1
    BEGIN
        INSERT INTO dbo.form_submissions
            (submissionid, formid, tenantid, projectid, submittedby, submittedat, status, createdby, updatedby)
        VALUES
            (@submissionid, @formid, @tenantid, @projectid, @userid, SYSUTCDATETIME(), 1, @actorid, @actorid);
    END
    ELSE
    BEGIN
        IF NOT EXISTS (
            SELECT 1 FROM dbo.form_submissions
            WHERE submissionid = @submissionid AND tenantid = @tenantid AND submittedby = @userid AND isdeleted = 0)
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR('Submission not found.', 16, 1);
            RETURN;
        END;

        UPDATE dbo.form_submissions SET
            projectid = @projectid,
            updatedby = @actorid,
            updatedat = SYSUTCDATETIME()
        WHERE submissionid = @submissionid AND tenantid = @tenantid AND submittedby = @userid AND isdeleted = 0;

        DELETE FROM dbo.form_submission_values WHERE submissionid = @submissionid;
    END;

    IF @valuesjson IS NOT NULL AND LTRIM(RTRIM(@valuesjson)) <> '' AND LTRIM(RTRIM(@valuesjson)) <> '[]'
    BEGIN
        INSERT INTO dbo.form_submission_values (submissionid, fieldid, valuetext)
        SELECT
            @submissionid,
            TRY_CAST(j.fieldid AS UNIQUEIDENTIFIER),
            j.valuetext
        FROM OPENJSON(@valuesjson)
        WITH (
            fieldid   NVARCHAR(50)  '$.fieldid',
            valuetext NVARCHAR(MAX) '$.valuetext'
        ) j
        WHERE TRY_CAST(j.fieldid AS UNIQUEIDENTIFIER) IS NOT NULL
          AND EXISTS (
              SELECT 1 FROM dbo.form_fields ff
              WHERE ff.fieldid = TRY_CAST(j.fieldid AS UNIQUEIDENTIFIER)
                AND ff.formid = @formid AND ff.isdeleted = 0);
    END;

    COMMIT TRANSACTION;

    SELECT @submissionid AS submissionid;
END
GO
