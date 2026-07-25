CREATE OR ALTER PROCEDURE dbo.sp_form_group_save
    @formgroupid          UNIQUEIDENTIFIER,
    @tenantid             UNIQUEIDENTIFIER,
    @formid               UNIQUEIDENTIFIER,
    @groupname            NVARCHAR(200),
    @groupdisplaynamekey  NVARCHAR(200) = NULL,
    @displayorder         INT,
    @actorid              UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.forms WHERE tenantid = @tenantid AND formid = @formid AND isdeleted = 0)
        RETURN;

    IF EXISTS (SELECT 1 FROM dbo.form_groups WHERE formgroupid = @formgroupid AND isdeleted = 0)
    BEGIN
        UPDATE dbo.form_groups
        SET groupname = @groupname, groupdisplaynamekey = @groupdisplaynamekey, displayorder = @displayorder,
            updatedby = @actorid, updatedat = SYSUTCDATETIME()
        WHERE formgroupid = @formgroupid AND formid = @formid AND isdeleted = 0;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.form_groups (formgroupid, formid, groupname, groupdisplaynamekey, displayorder, createdby, updatedby)
        VALUES (@formgroupid, @formid, @groupname, @groupdisplaynamekey, @displayorder, @actorid, @actorid);
    END
    SELECT @formgroupid;
END
GO
