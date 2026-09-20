CREATE OR ALTER PROCEDURE dbo.sp_submission_save
    @tenantid            UNIQUEIDENTIFIER,
    @userid              UNIQUEIDENTIFIER,
    @submissionid        UNIQUEIDENTIFIER,
    @formid              UNIQUEIDENTIFIER,
    @valuesjson          NVARCHAR(MAX),
    @isnew               BIT,
    @actorid             UNIQUEIDENTIFIER,
    @parentsubmissionid  UNIQUEIDENTIFIER = NULL,
    @issuperadmin        BIT = 0,
    @datascope           TINYINT = 0,
    @cancreate           BIT = 1,
    @canedit             BIT = 1,
    @districtids         NVARCHAR(MAX) = NULL,
    @projectids          NVARCHAR(MAX) = NULL,
    @stateid             UNIQUEIDENTIFIER = NULL,
    @districtid          UNIQUEIDENTIFIER = NULL,
    @blockid             UNIQUEIDENTIFIER = NULL,
    @villageid           UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @projectid UNIQUEIDENTIFIER;
    DECLARE @collectlocation BIT = 0;
    DECLARE @parentformid UNIQUEIDENTIFIER;
    DECLARE @configuredfollowupid UNIQUEIDENTIFIER;
    DECLARE @allowmultiple BIT;
    DECLARE @existingsubmittedby UNIQUEIDENTIFIER;
    DECLARE @existingprojectid UNIQUEIDENTIFIER;

    SELECT @projectid = f.projectid, @collectlocation = f.collectlocation
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

    -- Project scope: datascope Project requires project in list; also legacy user_scopes
    IF @issuperadmin = 0 AND @datascope = 2
       AND (
            @projectids IS NULL OR LTRIM(RTRIM(@projectids)) = N''
            OR NOT EXISTS (
                SELECT 1 FROM STRING_SPLIT(@projectids, N',') d
                WHERE TRY_CAST(LTRIM(RTRIM(d.value)) AS UNIQUEIDENTIFIER) = @projectid)
       )
    BEGIN
        RAISERROR('Form project is not in your scope.', 16, 1);
        RETURN;
    END;

    IF @issuperadmin = 0 AND @datascope <> 2 AND @datascope <> 3
       AND EXISTS (SELECT 1 FROM dbo.user_scopes WHERE userid = @userid)
       AND NOT EXISTS (SELECT 1 FROM dbo.user_scopes WHERE userid = @userid AND projectid = @projectid)
    BEGIN
        RAISERROR('Form project is not in your scope.', 16, 1);
        RETURN;
    END;

    IF @isnew = 1
    BEGIN
        IF @issuperadmin = 0 AND (@cancreate = 0 OR @datascope = 1)
        BEGIN
            RAISERROR('You do not have permission to create submissions.', 16, 1);
            RETURN;
        END;
    END;

    IF @collectlocation = 1
    BEGIN
        IF @stateid IS NULL OR @districtid IS NULL
        BEGIN
            RAISERROR('State and District are required for this form.', 16, 1);
            RETURN;
        END;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.districts d
            WHERE d.tenantid = @tenantid AND d.districtid = @districtid AND d.stateid = @stateid AND d.isdeleted = 0)
        BEGIN
            RAISERROR('Invalid State/District selection.', 16, 1);
            RETURN;
        END;

        IF @blockid IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM dbo.blocks b
            WHERE b.tenantid = @tenantid AND b.blockid = @blockid AND b.districtid = @districtid AND b.isdeleted = 0)
        BEGIN
            RAISERROR('Invalid Block selection.', 16, 1);
            RETURN;
        END;

        IF @villageid IS NOT NULL AND (
            @blockid IS NULL OR NOT EXISTS (
                SELECT 1 FROM dbo.villages v
                WHERE v.tenantid = @tenantid AND v.villageid = @villageid AND v.blockid = @blockid AND v.isdeleted = 0))
        BEGIN
            RAISERROR('Invalid Village selection.', 16, 1);
            RETURN;
        END;
    END
    ELSE
    BEGIN
        SET @stateid = NULL;
        SET @districtid = NULL;
        SET @blockid = NULL;
        SET @villageid = NULL;
    END;

    IF @isnew = 1 AND @parentsubmissionid IS NOT NULL
    BEGIN
        SELECT @parentformid = s.formid
        FROM dbo.form_submissions s
        WHERE s.tenantid = @tenantid
          AND s.submissionid = @parentsubmissionid
          AND dbo.fn_submission_is_visible(@issuperadmin, @datascope, @userid, s.submittedby, s.districtid, s.projectid, @districtids, @projectids) = 1
          AND s.parentsubmissionid IS NULL
          AND s.isdeleted = 0;

        IF @parentformid IS NULL
        BEGIN
            RAISERROR('Parent submission not found.', 16, 1);
            RETURN;
        END;

        SELECT @configuredfollowupid = c.followupformid, @allowmultiple = c.allowmultiple
        FROM dbo.form_followup_configs c
        WHERE c.tenantid = @tenantid AND c.primaryformid = @parentformid AND c.isdeleted = 0;

        IF @configuredfollowupid IS NULL OR @configuredfollowupid <> @formid
        BEGIN
            RAISERROR('This form is not configured as a follow-up for the parent entry.', 16, 1);
            RETURN;
        END;

        IF @allowmultiple = 0
           AND EXISTS (
               SELECT 1 FROM dbo.form_submissions
               WHERE tenantid = @tenantid AND parentsubmissionid = @parentsubmissionid AND isdeleted = 0)
        BEGIN
            RAISERROR('Only one follow-up is allowed for this entry.', 16, 1);
            RETURN;
        END;
    END
    ELSE IF @isnew = 1 AND @parentsubmissionid IS NULL
    BEGIN
        IF EXISTS (
            SELECT 1 FROM dbo.form_followup_configs
            WHERE tenantid = @tenantid AND followupformid = @formid AND isdeleted = 0)
        BEGIN
            RAISERROR('This form is configured as a follow-up form and cannot be used for new primary entries.', 16, 1);
            RETURN;
        END;
    END
    ELSE IF @isnew = 0
    BEGIN
        SELECT
            @parentsubmissionid = parentsubmissionid,
            @existingsubmittedby = submittedby,
            @existingprojectid = projectid
        FROM dbo.form_submissions
        WHERE submissionid = @submissionid AND tenantid = @tenantid AND isdeleted = 0;

        IF @existingsubmittedby IS NULL
           OR dbo.fn_submission_can_mutate(@issuperadmin, @datascope, @canedit, @userid, @existingsubmittedby, @existingprojectid, @projectids) = 0
        BEGIN
            RAISERROR('Submission not found or you cannot edit it.', 16, 1);
            RETURN;
        END;
    END;

    BEGIN TRANSACTION;

    IF @isnew = 1
    BEGIN
        INSERT INTO dbo.form_submissions
            (submissionid, formid, tenantid, projectid, submittedby, submittedat, status,
             parentsubmissionid, stateid, districtid, blockid, villageid, createdby, updatedby)
        VALUES
            (@submissionid, @formid, @tenantid, @projectid, @userid, SYSUTCDATETIME(), 1,
             @parentsubmissionid, @stateid, @districtid, @blockid, @villageid, @actorid, @actorid);
    END
    ELSE
    BEGIN
        UPDATE dbo.form_submissions SET
            projectid = @projectid,
            stateid = @stateid,
            districtid = @districtid,
            blockid = @blockid,
            villageid = @villageid,
            updatedby = @actorid,
            updatedat = SYSUTCDATETIME()
        WHERE submissionid = @submissionid AND tenantid = @tenantid AND isdeleted = 0;

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
