CREATE OR ALTER PROCEDURE dbo.sp_submission_export_mine
    @tenantid  UNIQUEIDENTIFIER,
    @userid    UNIQUEIDENTIFIER,
    @formid    UNIQUEIDENTIFIER,
    @search    NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @like NVARCHAR(102) = NULL;
    DECLARE @maxrows INT = 5000;
    DECLARE @total INT = 0;

    IF @search IS NOT NULL AND LTRIM(RTRIM(@search)) <> ''
        SET @like = N'%' + LTRIM(RTRIM(@search)) + N'%';

    SELECT @total = COUNT(1)
    FROM dbo.form_submissions s
    INNER JOIN dbo.projects p ON p.projectid = s.projectid
    WHERE s.tenantid = @tenantid
      AND s.formid = @formid
      AND s.submittedby = @userid
      AND s.isdeleted = 0
      AND (
            @like IS NULL
            OR p.projectname LIKE @like
            OR EXISTS (
                SELECT 1
                FROM dbo.form_submission_values v
                INNER JOIN dbo.form_fields ff ON ff.fieldid = v.fieldid
                WHERE v.submissionid = s.submissionid
                  AND ff.formid = @formid
                  AND ff.isdeleted = 0
                  AND ff.displayonlist = 1
                  AND ff.controltype <> 7
                  AND v.valuetext LIKE @like
            )
        );

    -- 1: total count
    SELECT @total AS totalcount;

    -- 2: form name
    SELECT f.name
    FROM dbo.forms f
    WHERE f.tenantid = @tenantid
      AND f.formid = @formid
      AND f.isdeleted = 0;

    -- 3: export columns (all non-label fields in form display order)
    SELECT
        ff.fieldid,
        ff.controllabel,
        ff.displayorder
    FROM dbo.form_fields ff
    INNER JOIN dbo.forms f ON f.formid = ff.formid
    INNER JOIN dbo.form_groups g ON g.formgroupid = ff.formgroupid AND g.isdeleted = 0
    WHERE f.tenantid = @tenantid
      AND ff.formid = @formid
      AND ff.isdeleted = 0
      AND f.isdeleted = 0
      AND ff.controltype <> 7
    ORDER BY g.displayorder, ff.displayorder, ff.controllabel;

    -- Over limit: return empty row/value sets (caller fails using totalcount)
    IF @total > @maxrows
    BEGIN
        SELECT
            CAST(NULL AS UNIQUEIDENTIFIER) AS submissionid,
            CAST(NULL AS UNIQUEIDENTIFIER) AS formid,
            CAST(NULL AS UNIQUEIDENTIFIER) AS projectid,
            CAST(NULL AS NVARCHAR(200)) AS projectname,
            CAST(NULL AS UNIQUEIDENTIFIER) AS submittedby,
            CAST(NULL AS NVARCHAR(201)) AS createdbyname,
            CAST(NULL AS DATETIME2) AS submittedat,
            CAST(NULL AS TINYINT) AS status
        WHERE 1 = 0;

        SELECT
            CAST(NULL AS UNIQUEIDENTIFIER) AS submissionid,
            CAST(NULL AS UNIQUEIDENTIFIER) AS fieldid,
            CAST(NULL AS NVARCHAR(MAX)) AS valuetext
        WHERE 1 = 0;

        RETURN;
    END;

    -- 4: matching submissions
    SELECT
        s.submissionid, s.formid, s.projectid, p.projectname,
        s.submittedby,
        LTRIM(RTRIM(CONCAT(u.firstname, N' ', u.lastname))) AS createdbyname,
        s.submittedat, s.status
    FROM dbo.form_submissions s
    INNER JOIN dbo.projects p ON p.projectid = s.projectid
    LEFT JOIN dbo.users u ON u.userid = s.submittedby
    WHERE s.tenantid = @tenantid
      AND s.formid = @formid
      AND s.submittedby = @userid
      AND s.isdeleted = 0
      AND (
            @like IS NULL
            OR p.projectname LIKE @like
            OR EXISTS (
                SELECT 1
                FROM dbo.form_submission_values v
                INNER JOIN dbo.form_fields ff ON ff.fieldid = v.fieldid
                WHERE v.submissionid = s.submissionid
                  AND ff.formid = @formid
                  AND ff.isdeleted = 0
                  AND ff.displayonlist = 1
                  AND ff.controltype <> 7
                  AND v.valuetext LIKE @like
            )
        )
    ORDER BY s.submittedat DESC;

    -- 5: full values for export fields on matching submissions
    ;WITH matched AS (
        SELECT s.submissionid
        FROM dbo.form_submissions s
        INNER JOIN dbo.projects p ON p.projectid = s.projectid
        WHERE s.tenantid = @tenantid
          AND s.formid = @formid
          AND s.submittedby = @userid
          AND s.isdeleted = 0
          AND (
                @like IS NULL
                OR p.projectname LIKE @like
                OR EXISTS (
                    SELECT 1
                    FROM dbo.form_submission_values v
                    INNER JOIN dbo.form_fields ff ON ff.fieldid = v.fieldid
                    WHERE v.submissionid = s.submissionid
                      AND ff.formid = @formid
                      AND ff.isdeleted = 0
                      AND ff.displayonlist = 1
                      AND ff.controltype <> 7
                      AND v.valuetext LIKE @like
                )
            )
    ),
    export_fields AS (
        SELECT ff.fieldid
        FROM dbo.form_fields ff
        INNER JOIN dbo.form_groups g ON g.formgroupid = ff.formgroupid AND g.isdeleted = 0
        WHERE ff.formid = @formid
          AND ff.isdeleted = 0
          AND ff.controltype <> 7
    )
    SELECT
        v.submissionid,
        v.fieldid,
        v.valuetext
    FROM dbo.form_submission_values v
    INNER JOIN matched m ON m.submissionid = v.submissionid
    INNER JOIN export_fields ef ON ef.fieldid = v.fieldid;
END
GO
