CREATE OR ALTER FUNCTION dbo.fn_submission_is_visible
(
    @issuperadmin BIT,
    @datascope    TINYINT,
    @userid       UNIQUEIDENTIFIER,
    @submittedby  UNIQUEIDENTIFIER,
    @districtid   UNIQUEIDENTIFIER,
    @projectid    UNIQUEIDENTIFIER,
    @districtids  NVARCHAR(MAX),
    @projectids   NVARCHAR(MAX)
)
RETURNS BIT
AS
BEGIN
    IF @issuperadmin = 1 OR @datascope = 3
        RETURN 1;

    IF @datascope = 0 AND @submittedby = @userid
        RETURN 1;

    IF @datascope = 1
       AND @districtid IS NOT NULL
       AND @districtids IS NOT NULL
       AND EXISTS (
            SELECT 1 FROM STRING_SPLIT(@districtids, N',') d
            WHERE TRY_CAST(LTRIM(RTRIM(d.value)) AS UNIQUEIDENTIFIER) = @districtid)
        RETURN 1;

    IF @datascope = 2
       AND @projectids IS NOT NULL
       AND EXISTS (
            SELECT 1 FROM STRING_SPLIT(@projectids, N',') d
            WHERE TRY_CAST(LTRIM(RTRIM(d.value)) AS UNIQUEIDENTIFIER) = @projectid)
        RETURN 1;

    RETURN 0;
END
GO
