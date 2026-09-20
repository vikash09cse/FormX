CREATE OR ALTER PROCEDURE dbo.sp_submission_get_list_mine
    @tenantid     UNIQUEIDENTIFIER,
    @userid       UNIQUEIDENTIFIER,
    @formid       UNIQUEIDENTIFIER,
    @page         INT = 1,
    @pagesize     INT = 25,
    @search       NVARCHAR(100) = NULL,
    @issuperadmin BIT = 0,
    @datascope    TINYINT = 0,
    @districtids  NVARCHAR(MAX) = NULL,
    @projectids   NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @page IS NULL OR @page < 1 SET @page = 1;
    IF @pagesize IS NULL OR @pagesize < 1 SET @pagesize = 25;
    IF @pagesize > 100 SET @pagesize = 100;

    DECLARE @offset INT = (@page - 1) * @pagesize;
    DECLARE @like NVARCHAR(102) = NULL;

    IF @search IS NOT NULL AND LTRIM(RTRIM(@search)) <> ''
        SET @like = N'%' + LTRIM(RTRIM(@search)) + N'%';

    SELECT COUNT(1) AS totalcount
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
                INNER JOIN dbo.form_fields ff ON ff.fieldid = v.fieldid
                WHERE v.submissionid = s.submissionid
                  AND ff.formid = @formid
                  AND ff.isdeleted = 0
                  AND ff.displayonlist = 1
                  AND ff.controltype <> 7
                  AND v.valuetext LIKE @like
            )
        );

    SELECT TOP (5)
        ff.fieldid, ff.controllabel, ff.displayorder
    FROM dbo.form_fields ff
    INNER JOIN dbo.forms f ON f.formid = ff.formid
    WHERE f.tenantid = @tenantid
      AND ff.formid = @formid
      AND ff.isdeleted = 0
      AND f.isdeleted = 0
      AND ff.displayonlist = 1
      AND ff.controltype <> 7
    ORDER BY ff.displayorder, ff.controllabel;

    SELECT
        s.submissionid, s.formid, s.projectid, p.projectname,
        s.submittedby, s.submittedat, s.status,
        s.stateid, s.districtid, s.blockid, s.villageid,
        s.createdat, s.updatedat
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
                INNER JOIN dbo.form_fields ff ON ff.fieldid = v.fieldid
                WHERE v.submissionid = s.submissionid
                  AND ff.formid = @formid
                  AND ff.isdeleted = 0
                  AND ff.displayonlist = 1
                  AND ff.controltype <> 7
                  AND v.valuetext LIKE @like
            )
        )
    ORDER BY s.submittedat DESC
    OFFSET @offset ROWS FETCH NEXT @pagesize ROWS ONLY;

    ;WITH page_ids AS (
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
                    INNER JOIN dbo.form_fields ff ON ff.fieldid = v.fieldid
                    WHERE v.submissionid = s.submissionid
                      AND ff.formid = @formid
                      AND ff.isdeleted = 0
                      AND ff.displayonlist = 1
                      AND ff.controltype <> 7
                      AND v.valuetext LIKE @like
                )
            )
        ORDER BY s.submittedat DESC
        OFFSET @offset ROWS FETCH NEXT @pagesize ROWS ONLY
    ),
    list_fields AS (
        SELECT TOP (5) ff.fieldid
        FROM dbo.form_fields ff
        WHERE ff.formid = @formid
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
    INNER JOIN page_ids p ON p.submissionid = v.submissionid
    INNER JOIN list_fields lf ON lf.fieldid = v.fieldid;
END
GO
