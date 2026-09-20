CREATE OR ALTER PROCEDURE dbo.sp_submission_get_followups
    @tenantid            UNIQUEIDENTIFIER,
    @userid              UNIQUEIDENTIFIER,
    @parentsubmissionid  UNIQUEIDENTIFIER,
    @issuperadmin        BIT = 0,
    @datascope           TINYINT = 0,
    @districtids         NVARCHAR(MAX) = NULL,
    @projectids          NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Parent must exist and be visible to caller
    IF NOT EXISTS (
        SELECT 1 FROM dbo.form_submissions s
        WHERE s.tenantid = @tenantid
          AND s.submissionid = @parentsubmissionid
          AND dbo.fn_submission_is_visible(@issuperadmin, @datascope, @userid, s.submittedby, s.districtid, s.projectid, @districtids, @projectids) = 1
          AND s.parentsubmissionid IS NULL
          AND s.isdeleted = 0)
    BEGIN
        SELECT CAST(NULL AS UNIQUEIDENTIFIER) AS followupformid WHERE 1 = 0;
        SELECT CAST(NULL AS UNIQUEIDENTIFIER) AS submissionid WHERE 1 = 0;
        SELECT CAST(NULL AS UNIQUEIDENTIFIER) AS fieldid WHERE 1 = 0;
        SELECT CAST(NULL AS UNIQUEIDENTIFIER) AS submissionid WHERE 1 = 0;
        RETURN;
    END;

    DECLARE @followupformid UNIQUEIDENTIFIER;
    DECLARE @followupformname NVARCHAR(200);
    DECLARE @allowmultiple BIT;
    DECLARE @primaryformid UNIQUEIDENTIFIER;

    SELECT @primaryformid = s.formid
    FROM dbo.form_submissions s
    WHERE s.submissionid = @parentsubmissionid;

    SELECT
        @followupformid = c.followupformid,
        @followupformname = ff.name,
        @allowmultiple = c.allowmultiple
    FROM dbo.form_followup_configs c
    INNER JOIN dbo.forms ff ON ff.formid = c.followupformid AND ff.isdeleted = 0
    WHERE c.tenantid = @tenantid
      AND c.primaryformid = @primaryformid
      AND c.isdeleted = 0;

    -- 1: follow-up config for parent form (0 or 1 row)
    SELECT
        @followupformid AS followupformid,
        @followupformname AS followupformname,
        ISNULL(@allowmultiple, CAST(1 AS BIT)) AS allowmultiple
    WHERE @followupformid IS NOT NULL;

    -- 2: follow-up headers (oldest first for timeline)
    SELECT
        s.submissionid, s.formid, s.projectid, p.projectname,
        s.submittedby,
        LTRIM(RTRIM(CONCAT(u.firstname, N' ', u.lastname))) AS createdbyname,
        s.submittedat, s.status, s.parentsubmissionid, s.createdat, s.updatedat
    FROM dbo.form_submissions s
    INNER JOIN dbo.projects p ON p.projectid = s.projectid
    LEFT JOIN dbo.users u ON u.userid = s.submittedby
    WHERE s.tenantid = @tenantid
      AND s.parentsubmissionid = @parentsubmissionid
      AND dbo.fn_submission_is_visible(@issuperadmin, @datascope, @userid, s.submittedby, s.districtid, s.projectid, @districtids, @projectids) = 1
      AND s.isdeleted = 0
    ORDER BY s.submittedat ASC;

    -- 3: list columns for the follow-up form (if configured)
    IF @followupformid IS NOT NULL
    BEGIN
        SELECT TOP (5)
            ff.fieldid, ff.controllabel, ff.displayorder
        FROM dbo.form_fields ff
        WHERE ff.formid = @followupformid
          AND ff.isdeleted = 0
          AND ff.displayonlist = 1
          AND ff.controltype <> 7
        ORDER BY ff.displayorder, ff.controllabel;
    END
    ELSE
    BEGIN
        SELECT CAST(NULL AS UNIQUEIDENTIFIER) AS fieldid, CAST(NULL AS NVARCHAR(200)) AS controllabel, CAST(NULL AS INT) AS displayorder
        WHERE 1 = 0;
    END;

    -- 4: values for list fields on follow-ups
    ;WITH followup_ids AS (
        SELECT s.submissionid
        FROM dbo.form_submissions s
        WHERE s.tenantid = @tenantid
          AND s.parentsubmissionid = @parentsubmissionid
          AND dbo.fn_submission_is_visible(@issuperadmin, @datascope, @userid, s.submittedby, s.districtid, s.projectid, @districtids, @projectids) = 1
          AND s.isdeleted = 0
    ),
    list_fields AS (
        SELECT TOP (5) ff.fieldid
        FROM dbo.form_fields ff
        WHERE @followupformid IS NOT NULL
          AND ff.formid = @followupformid
          AND ff.isdeleted = 0
          AND ff.displayonlist = 1
          AND ff.controltype <> 7
        ORDER BY ff.displayorder, ff.controllabel
    )
    SELECT
        v.submissionid,
        v.fieldid,
        LEFT(v.valuetext, 200) AS valuetext
    FROM dbo.form_submission_values v
    INNER JOIN followup_ids f ON f.submissionid = v.submissionid
    INNER JOIN list_fields lf ON lf.fieldid = v.fieldid;
END
GO
