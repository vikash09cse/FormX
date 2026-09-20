CREATE OR ALTER PROCEDURE dbo.sp_tenant_dashboard_get
    @tenantid     UNIQUEIDENTIFIER,
    @userid       UNIQUEIDENTIFIER,
    @issuperadmin BIT,
    @datascope    TINYINT = 0,
    @districtids  NVARCHAR(MAX) = NULL,
    @projectids   NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @adminformfilter BIT = 0;
    IF @issuperadmin = 1
       AND EXISTS (SELECT 1 FROM dbo.tenant_dashboard_forms WHERE tenantid = @tenantid)
        SET @adminformfilter = 1;

    SELECT
        ISNULL(ts.dashboardlayout, N'table') AS dashboardlayout,
        ISNULL(ts.dashboardtopn, 12) AS dashboardtopn
    FROM (SELECT 1 AS x) d
    LEFT JOIN dbo.tenant_settings ts ON ts.tenantid = @tenantid;

    CREATE TABLE #form_counts (
        formid       UNIQUEIDENTIFIER NOT NULL,
        name         NVARCHAR(200)    NOT NULL,
        displayorder INT              NOT NULL,
        projectid    UNIQUEIDENTIFIER NULL,
        entrycount   INT              NOT NULL
    );

    INSERT INTO #form_counts (formid, name, displayorder, projectid, entrycount)
    SELECT
        f.formid,
        f.name,
        f.displayorder,
        f.projectid,
        COUNT(s.submissionid) AS entrycount
    FROM dbo.forms f
    LEFT JOIN dbo.form_submissions s
        ON s.formid = f.formid
       AND s.tenantid = @tenantid
       AND s.isdeleted = 0
       AND s.parentsubmissionid IS NULL
       AND dbo.fn_submission_is_visible(@issuperadmin, @datascope, @userid, s.submittedby, s.districtid, s.projectid, @districtids, @projectids) = 1
    WHERE f.tenantid = @tenantid
      AND f.isdeleted = 0
      AND f.status = 1
      AND NOT EXISTS (
          SELECT 1 FROM dbo.form_followup_configs c
          WHERE c.tenantid = @tenantid
            AND c.followupformid = f.formid
            AND c.isdeleted = 0)
      AND (
          @issuperadmin = 1
          OR EXISTS (
              SELECT 1
              FROM dbo.form_roles fr
              INNER JOIN dbo.user_roles ur ON ur.roleid = fr.roleid AND ur.userid = @userid
              INNER JOIN dbo.roles r ON r.roleid = ur.roleid
                  AND r.tenantid = @tenantid AND r.isdeleted = 0 AND r.rolestatus = 1
              WHERE fr.formid = f.formid
          )
      )
      AND (
          @adminformfilter = 0
          OR EXISTS (
              SELECT 1 FROM dbo.tenant_dashboard_forms tdf
              WHERE tdf.tenantid = @tenantid AND tdf.formid = f.formid
          )
      )
    GROUP BY f.formid, f.name, f.displayorder, f.projectid;

    SELECT
        COUNT(*) AS formcount,
        ISNULL(SUM(entrycount), 0) AS entrycount
    FROM #form_counts;

    SELECT
        fc.formid,
        fc.name,
        p.projectname,
        fc.entrycount,
        fc.displayorder
    FROM #form_counts fc
    LEFT JOIN dbo.projects p ON p.projectid = fc.projectid
    ORDER BY fc.displayorder, fc.name;

    DROP TABLE #form_counts;

    SELECT formid
    FROM dbo.tenant_dashboard_forms
    WHERE tenantid = @tenantid
    ORDER BY formid;

    IF @issuperadmin = 1
    BEGIN
        SELECT f.formid, f.name, f.displayorder
        FROM dbo.forms f
        WHERE f.tenantid = @tenantid
          AND f.isdeleted = 0
          AND f.status = 1
          AND NOT EXISTS (
              SELECT 1 FROM dbo.form_followup_configs c
              WHERE c.tenantid = @tenantid
                AND c.followupformid = f.formid
                AND c.isdeleted = 0)
        ORDER BY f.displayorder, f.name;
    END
    ELSE
    BEGIN
        SELECT CAST(NULL AS UNIQUEIDENTIFIER) AS formid, CAST(NULL AS NVARCHAR(200)) AS name, CAST(NULL AS INT) AS displayorder
        WHERE 1 = 0;
    END;
END
GO
