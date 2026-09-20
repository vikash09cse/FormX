CREATE OR ALTER PROCEDURE dbo.sp_submission_export_mine
    @tenantid     UNIQUEIDENTIFIER,
    @userid       UNIQUEIDENTIFIER,
    @formid       UNIQUEIDENTIFIER,
    @search       NVARCHAR(100) = NULL,
    @issuperadmin BIT = 0,
    @datascope    TINYINT = 0,
    @districtids  NVARCHAR(MAX) = NULL,
    @projectids   NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @like NVARCHAR(102) = NULL;
    DECLARE @maxrows INT = 5000;
    DECLARE @total INT = 0;
    DECLARE @followupformid UNIQUEIDENTIFIER = NULL;

    IF @search IS NOT NULL AND LTRIM(RTRIM(@search)) <> ''
        SET @like = N'%' + LTRIM(RTRIM(@search)) + N'%';

    SELECT @followupformid = c.followupformid
    FROM dbo.form_followup_configs c
    INNER JOIN dbo.forms ff ON ff.formid = c.followupformid AND ff.isdeleted = 0 AND ff.status = 1
    WHERE c.tenantid = @tenantid
      AND c.primaryformid = @formid
      AND c.isdeleted = 0;

    SELECT @total = COUNT(1)
    FROM dbo.form_submissions s
    INNER JOIN dbo.projects p ON p.projectid = s.projectid
    WHERE s.tenantid = @tenantid
      AND s.formid = @formid
      AND dbo.fn_submission_is_visible(@issuperadmin, @datascope, @userid, s.submittedby, s.districtid, s.projectid, @districtids, @projectids) = 1
      AND s.isdeleted = 0
      AND s.parentsubmissionid IS NULL
      AND (
            @like IS NULL
            OR p.projectname LIKE @like
            OR EXISTS (
                SELECT 1
                FROM dbo.form_submission_values v
                INNER JOIN dbo.form_fields fld ON fld.fieldid = v.fieldid
                WHERE v.submissionid = s.submissionid
                  AND fld.formid = @formid
                  AND fld.isdeleted = 0
                  AND fld.displayonlist = 1
                  AND fld.controltype <> 7
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

    -- 3: primary export columns
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

    -- Over limit: empty primary + follow-up sets (caller fails using totalcount)
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

        SELECT CAST(NULL AS UNIQUEIDENTIFIER) AS followupformid WHERE 1 = 0;

        SELECT
            CAST(NULL AS UNIQUEIDENTIFIER) AS fieldid,
            CAST(NULL AS NVARCHAR(500)) AS controllabel,
            CAST(NULL AS INT) AS displayorder
        WHERE 1 = 0;

        SELECT
            CAST(NULL AS UNIQUEIDENTIFIER) AS submissionid,
            CAST(NULL AS UNIQUEIDENTIFIER) AS parentsubmissionid,
            CAST(NULL AS DATETIME2) AS submittedat,
            CAST(NULL AS NVARCHAR(201)) AS createdbyname
        WHERE 1 = 0;

        SELECT
            CAST(NULL AS UNIQUEIDENTIFIER) AS submissionid,
            CAST(NULL AS UNIQUEIDENTIFIER) AS fieldid,
            CAST(NULL AS NVARCHAR(MAX)) AS valuetext
        WHERE 1 = 0;

        RETURN;
    END;

    -- 4: matching primary submissions
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
      AND dbo.fn_submission_is_visible(@issuperadmin, @datascope, @userid, s.submittedby, s.districtid, s.projectid, @districtids, @projectids) = 1
      AND s.isdeleted = 0
      AND s.parentsubmissionid IS NULL
      AND (
            @like IS NULL
            OR p.projectname LIKE @like
            OR EXISTS (
                SELECT 1
                FROM dbo.form_submission_values v
                INNER JOIN dbo.form_fields fld ON fld.fieldid = v.fieldid
                WHERE v.submissionid = s.submissionid
                  AND fld.formid = @formid
                  AND fld.isdeleted = 0
                  AND fld.displayonlist = 1
                  AND fld.controltype <> 7
                  AND v.valuetext LIKE @like
            )
        )
    ORDER BY s.submittedat DESC;

    -- 5: primary values
    ;WITH matched AS (
        SELECT s.submissionid
        FROM dbo.form_submissions s
        INNER JOIN dbo.projects p ON p.projectid = s.projectid
        WHERE s.tenantid = @tenantid
          AND s.formid = @formid
          AND dbo.fn_submission_is_visible(@issuperadmin, @datascope, @userid, s.submittedby, s.districtid, s.projectid, @districtids, @projectids) = 1
          AND s.isdeleted = 0
          AND s.parentsubmissionid IS NULL
          AND (
                @like IS NULL
                OR p.projectname LIKE @like
                OR EXISTS (
                    SELECT 1
                    FROM dbo.form_submission_values v
                    INNER JOIN dbo.form_fields fld ON fld.fieldid = v.fieldid
                    WHERE v.submissionid = s.submissionid
                      AND fld.formid = @formid
                      AND fld.isdeleted = 0
                      AND fld.displayonlist = 1
                      AND fld.controltype <> 7
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

    -- 6: follow-up form id (0 or 1 row)
    SELECT @followupformid AS followupformid
    WHERE @followupformid IS NOT NULL;

    -- 7: follow-up columns
    IF @followupformid IS NOT NULL
    BEGIN
        SELECT
            ff.fieldid,
            ff.controllabel,
            ff.displayorder
        FROM dbo.form_fields ff
        INNER JOIN dbo.form_groups g ON g.formgroupid = ff.formgroupid AND g.isdeleted = 0
        WHERE ff.formid = @followupformid
          AND ff.isdeleted = 0
          AND ff.controltype <> 7
        ORDER BY g.displayorder, ff.displayorder, ff.controllabel;
    END
    ELSE
    BEGIN
        SELECT
            CAST(NULL AS UNIQUEIDENTIFIER) AS fieldid,
            CAST(NULL AS NVARCHAR(500)) AS controllabel,
            CAST(NULL AS INT) AS displayorder
        WHERE 1 = 0;
    END;

    -- 8: follow-up headers (children of visible matched primaries)
    IF @followupformid IS NOT NULL
    BEGIN
        ;WITH matched AS (
            SELECT s.submissionid
            FROM dbo.form_submissions s
            INNER JOIN dbo.projects p ON p.projectid = s.projectid
            WHERE s.tenantid = @tenantid
              AND s.formid = @formid
              AND dbo.fn_submission_is_visible(@issuperadmin, @datascope, @userid, s.submittedby, s.districtid, s.projectid, @districtids, @projectids) = 1
              AND s.isdeleted = 0
              AND s.parentsubmissionid IS NULL
              AND (
                    @like IS NULL
                    OR p.projectname LIKE @like
                    OR EXISTS (
                        SELECT 1
                        FROM dbo.form_submission_values v
                        INNER JOIN dbo.form_fields fld ON fld.fieldid = v.fieldid
                        WHERE v.submissionid = s.submissionid
                          AND fld.formid = @formid
                          AND fld.isdeleted = 0
                          AND fld.displayonlist = 1
                          AND fld.controltype <> 7
                          AND v.valuetext LIKE @like
                    )
                )
        )
        SELECT
            fu.submissionid,
            fu.parentsubmissionid,
            fu.submittedat,
            LTRIM(RTRIM(CONCAT(u.firstname, N' ', u.lastname))) AS createdbyname
        FROM dbo.form_submissions fu
        INNER JOIN matched m ON m.submissionid = fu.parentsubmissionid
        LEFT JOIN dbo.users u ON u.userid = fu.submittedby
        WHERE fu.tenantid = @tenantid
          AND fu.formid = @followupformid
          AND fu.isdeleted = 0
          AND fu.parentsubmissionid IS NOT NULL
          AND dbo.fn_submission_is_visible(@issuperadmin, @datascope, @userid, fu.submittedby, fu.districtid, fu.projectid, @districtids, @projectids) = 1
        ORDER BY fu.parentsubmissionid, fu.submittedat ASC;
    END
    ELSE
    BEGIN
        SELECT
            CAST(NULL AS UNIQUEIDENTIFIER) AS submissionid,
            CAST(NULL AS UNIQUEIDENTIFIER) AS parentsubmissionid,
            CAST(NULL AS DATETIME2) AS submittedat,
            CAST(NULL AS NVARCHAR(201)) AS createdbyname
        WHERE 1 = 0;
    END;

    -- 9: follow-up values
    IF @followupformid IS NOT NULL
    BEGIN
        ;WITH matched AS (
            SELECT s.submissionid
            FROM dbo.form_submissions s
            INNER JOIN dbo.projects p ON p.projectid = s.projectid
            WHERE s.tenantid = @tenantid
              AND s.formid = @formid
              AND dbo.fn_submission_is_visible(@issuperadmin, @datascope, @userid, s.submittedby, s.districtid, s.projectid, @districtids, @projectids) = 1
              AND s.isdeleted = 0
              AND s.parentsubmissionid IS NULL
              AND (
                    @like IS NULL
                    OR p.projectname LIKE @like
                    OR EXISTS (
                        SELECT 1
                        FROM dbo.form_submission_values v
                        INNER JOIN dbo.form_fields fld ON fld.fieldid = v.fieldid
                        WHERE v.submissionid = s.submissionid
                          AND fld.formid = @formid
                          AND fld.isdeleted = 0
                          AND fld.displayonlist = 1
                          AND fld.controltype <> 7
                          AND v.valuetext LIKE @like
                    )
                )
        ),
        followups AS (
            SELECT fu.submissionid
            FROM dbo.form_submissions fu
            INNER JOIN matched m ON m.submissionid = fu.parentsubmissionid
            WHERE fu.tenantid = @tenantid
              AND fu.formid = @followupformid
              AND fu.isdeleted = 0
              AND fu.parentsubmissionid IS NOT NULL
              AND dbo.fn_submission_is_visible(@issuperadmin, @datascope, @userid, fu.submittedby, fu.districtid, fu.projectid, @districtids, @projectids) = 1
        ),
        fu_fields AS (
            SELECT ff.fieldid
            FROM dbo.form_fields ff
            INNER JOIN dbo.form_groups g ON g.formgroupid = ff.formgroupid AND g.isdeleted = 0
            WHERE ff.formid = @followupformid
              AND ff.isdeleted = 0
              AND ff.controltype <> 7
        )
        SELECT
            v.submissionid,
            v.fieldid,
            v.valuetext
        FROM dbo.form_submission_values v
        INNER JOIN followups f ON f.submissionid = v.submissionid
        INNER JOIN fu_fields ef ON ef.fieldid = v.fieldid;
    END
    ELSE
    BEGIN
        SELECT
            CAST(NULL AS UNIQUEIDENTIFIER) AS submissionid,
            CAST(NULL AS UNIQUEIDENTIFIER) AS fieldid,
            CAST(NULL AS NVARCHAR(MAX)) AS valuetext
        WHERE 1 = 0;
    END;
END
GO
