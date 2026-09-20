CREATE OR ALTER PROCEDURE dbo.sp_block_get_list
    @tenantid   UNIQUEIDENTIFIER,
    @districtid UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT b.blockid, b.districtid, d.name AS districtname, d.stateid, s.name AS statename,
           b.name, b.code, b.status, b.createdat, b.updatedat
    FROM dbo.blocks b
    INNER JOIN dbo.districts d ON d.districtid = b.districtid AND d.isdeleted = 0
    INNER JOIN dbo.states s ON s.stateid = d.stateid AND s.isdeleted = 0
    WHERE b.tenantid = @tenantid AND b.isdeleted = 0
      AND (@districtid IS NULL OR b.districtid = @districtid)
    ORDER BY s.name, d.name, b.name;
END
GO
