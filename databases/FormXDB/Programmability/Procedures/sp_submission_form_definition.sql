CREATE OR ALTER PROCEDURE dbo.sp_submission_form_definition
    @tenantid UNIQUEIDENTIFIER,
    @formid   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- 1: form
    SELECT f.formid, f.name, f.description, f.projectid, p.projectname
    FROM dbo.forms f
    LEFT JOIN dbo.projects p ON p.projectid = f.projectid AND p.tenantid = f.tenantid AND p.isdeleted = 0
    WHERE f.tenantid = @tenantid AND f.formid = @formid AND f.isdeleted = 0 AND f.status = 1;

    -- 2: groups
    SELECT g.formgroupid, g.formid, g.groupname, g.groupdisplaynamekey, g.displayorder
    FROM dbo.form_groups g
    INNER JOIN dbo.forms f ON f.formid = g.formid
    WHERE f.tenantid = @tenantid AND g.formid = @formid AND g.isdeleted = 0 AND f.isdeleted = 0
    ORDER BY g.displayorder, g.groupname;

    -- 3: fields (+ regex pattern)
    SELECT
        ff.fieldid, ff.formid, ff.formgroupid, ff.controllabel, ff.controltype, ff.controlmaxlength,
        ff.controlrequired, ff.displayorder, ff.controlnotes, ff.fieldkey, ff.displaycontrollabel,
        ff.classname, ff.parentfieldid, ff.issendemailnotification, ff.validationregexpresetid,
        rp.pattern AS validationregexpattern, rp.name AS validationregexname
    FROM dbo.form_fields ff
    INNER JOIN dbo.forms f ON f.formid = ff.formid
    LEFT JOIN dbo.validation_regex_presets rp ON rp.validationregexpresetid = ff.validationregexpresetid AND rp.isactive = 1
    WHERE f.tenantid = @tenantid AND ff.formid = @formid AND ff.isdeleted = 0 AND f.isdeleted = 0
    ORDER BY ff.displayorder, ff.controllabel;

    -- 4: options
    SELECT o.optionid, o.fieldid, o.optiontext, o.optionvalue, o.displayorder
    FROM dbo.form_field_options o
    INNER JOIN dbo.form_fields ff ON ff.fieldid = o.fieldid
    INNER JOIN dbo.forms f ON f.formid = ff.formid
    WHERE f.tenantid = @tenantid AND ff.formid = @formid AND o.isdeleted = 0 AND ff.isdeleted = 0 AND f.isdeleted = 0
    ORDER BY o.displayorder, o.optiontext;

    -- 5: parent options
    SELECT po.fieldid, po.optionid
    FROM dbo.form_field_parent_options po
    INNER JOIN dbo.form_fields ff ON ff.fieldid = po.fieldid
    INNER JOIN dbo.forms f ON f.formid = ff.formid
    WHERE f.tenantid = @tenantid AND ff.formid = @formid AND ff.isdeleted = 0 AND f.isdeleted = 0;
END
GO
