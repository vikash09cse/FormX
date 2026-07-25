CREATE OR ALTER PROCEDURE dbo.sp_form_group_get_list
    @tenantid UNIQUEIDENTIFIER,
    @formid   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT g.formgroupid, g.formid, g.groupname, g.groupdisplaynamekey, g.displayorder, g.createdat, g.updatedat
    FROM dbo.form_groups g
    INNER JOIN dbo.forms f ON f.formid = g.formid
    WHERE f.tenantid = @tenantid AND g.formid = @formid AND g.isdeleted = 0 AND f.isdeleted = 0
    ORDER BY g.displayorder, g.groupname;
END
GO
