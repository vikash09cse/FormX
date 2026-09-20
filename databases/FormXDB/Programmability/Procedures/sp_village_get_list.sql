CREATE OR ALTER PROCEDURE dbo.sp_village_get_list
    @tenantid UNIQUEIDENTIFIER,
    @blockid  UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT v.villageid, v.blockid, b.name AS blockname, b.districtid, d.name AS districtname,
           d.stateid, s.name AS statename, v.name, v.code, v.status, v.createdat, v.updatedat
    FROM dbo.villages v
    INNER JOIN dbo.blocks b ON b.blockid = v.blockid AND b.isdeleted = 0
    INNER JOIN dbo.districts d ON d.districtid = b.districtid AND d.isdeleted = 0
    INNER JOIN dbo.states s ON s.stateid = d.stateid AND s.isdeleted = 0
    WHERE v.tenantid = @tenantid AND v.isdeleted = 0
      AND (@blockid IS NULL OR v.blockid = @blockid)
    ORDER BY s.name, d.name, b.name, v.name;
END
GO
