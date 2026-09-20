CREATE OR ALTER FUNCTION dbo.fn_submission_can_mutate
(
    @issuperadmin BIT,
    @datascope    TINYINT,
    @canflag      BIT,
    @userid       UNIQUEIDENTIFIER,
    @submittedby  UNIQUEIDENTIFIER,
    @projectid    UNIQUEIDENTIFIER,
    @projectids   NVARCHAR(MAX)
)
RETURNS BIT
AS
BEGIN
    IF @issuperadmin = 1
        RETURN 1;

    IF @canflag = 0 OR @datascope = 1
        RETURN 0;

    IF @datascope = 0 AND @submittedby = @userid
        RETURN 1;

    IF @datascope = 3
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
