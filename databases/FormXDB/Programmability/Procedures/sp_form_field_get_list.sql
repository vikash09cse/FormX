CREATE OR ALTER PROCEDURE dbo.sp_form_field_get_list
    @tenantid UNIQUEIDENTIFIER,
    @formid   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Result set 1: fields
    SELECT
        ff.fieldid, ff.formid, ff.formgroupid, ff.controllabel, ff.controltype, ff.controlmaxlength,
        ff.controlrequired, ff.displayorder, ff.controlnotes, ff.fieldkey, ff.displaycontrollabel,
        ff.classname, ff.parentfieldid, ff.issendemailnotification, ff.validationregexpresetid,
        ff.createdat, ff.updatedat
    FROM dbo.form_fields ff
    INNER JOIN dbo.forms f ON f.formid = ff.formid
    WHERE f.tenantid = @tenantid AND ff.formid = @formid AND ff.isdeleted = 0 AND f.isdeleted = 0
    ORDER BY ff.displayorder, ff.controllabel;

    -- Result set 2: options for those fields
    SELECT o.optionid, o.fieldid, o.optiontext, o.optionvalue, o.displayorder
    FROM dbo.form_field_options o
    INNER JOIN dbo.form_fields ff ON ff.fieldid = o.fieldid
    INNER JOIN dbo.forms f ON f.formid = ff.formid
    WHERE f.tenantid = @tenantid AND ff.formid = @formid AND o.isdeleted = 0 AND ff.isdeleted = 0 AND f.isdeleted = 0
    ORDER BY o.displayorder, o.optiontext;

    -- Result set 3: parent option links
    SELECT po.fieldid, po.optionid
    FROM dbo.form_field_parent_options po
    INNER JOIN dbo.form_fields ff ON ff.fieldid = po.fieldid
    INNER JOIN dbo.forms f ON f.formid = ff.formid
    WHERE f.tenantid = @tenantid AND ff.formid = @formid AND ff.isdeleted = 0 AND f.isdeleted = 0;
END
GO
