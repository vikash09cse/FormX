CREATE OR ALTER PROCEDURE dbo.sp_form_delete
    @tenantid  UNIQUEIDENTIFIER,
    @formid    UNIQUEIDENTIFIER,
    @updatedby UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.forms
    SET isdeleted = 1, updatedby = @updatedby, updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid AND formid = @formid AND isdeleted = 0;
END
GO
