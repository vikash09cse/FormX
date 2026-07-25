-- FormX has no Doctor system role (hospital leftover). Convert any remaining Doctor users to Staff.

UPDATE dbo.users
SET usertype = 2, -- Staff
    updatedat = SYSUTCDATETIME()
WHERE usertype = 3 -- former Doctor
  AND isdeleted = 0;
GO
