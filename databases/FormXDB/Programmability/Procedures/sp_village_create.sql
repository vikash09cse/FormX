CREATE OR ALTER PROCEDURE dbo.sp_village_create
    @villageid UNIQUEIDENTIFIER,
    @tenantid  UNIQUEIDENTIFIER,
    @blockid   UNIQUEIDENTIFIER,
    @name      NVARCHAR(200),
    @code      NVARCHAR(50),
    @status    TINYINT,
    @createdby UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.blocks WHERE tenantid = @tenantid AND blockid = @blockid AND isdeleted = 0)
    BEGIN
        RAISERROR('Invalid block.', 16, 1);
        RETURN;
    END;
    INSERT INTO dbo.villages (villageid, tenantid, blockid, name, code, status, createdby, updatedby)
    VALUES (@villageid, @tenantid, @blockid, @name, @code, @status, @createdby, @createdby);
    SELECT @villageid;
END
GO
