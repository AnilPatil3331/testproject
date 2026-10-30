USE [RohiniAyurvedaa]
GO

-- ============================================================
-- sp_List_Patient - Origin column SELECT madhe add kara
-- 4 SELECT blocks ahet - saglyat Origin add kela ahe
-- ============================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[sp_List_Patient]
(
@p_PatientFid int =null,
@p_PatientFid_Encrypted nvarchar(100)=null,
@p_Message nvarchar(200)=null,
@p_ClinicFid int=null,
@p_PatientContact nvarchar(15)=null
)
as 
Begin
begin try

-- ============================================================
-- BLOCK 1: By PatientFid_Encrypted (Edit/View patient)
-- ============================================================
if(@p_PatientFid_Encrypted is not null)
begin
select 
    FORMAT(Patient.FDate, 'd MMM yyyy') AS FormattedDate,
	Patient.FDate,
	Patient.ClinicFid,
	ISNULL(NULLIF(PatientFid_Encrypted, ''), master.dbo.fn_varbintohexstr(HashBytes('SHA2_256', convert(nvarchar(70), PatientFid)))) AS PatientFid_Encrypted,
	PatientFid,
	Patient.UHIDNO,
	FirstName,
	LName,
	FirstName + ' ' + LName as PatientFullName,
	Email,
	ContactNumber,
	DateOfBirth,
	BloodGroup,
	Patient.Gender,
	Address,
	Patient.Status,
	Patient.Country,
	Patient.CityName,
	PatientProfile,
	Patient.PostalCode,
	PatientProfile,
	Mas_Clinic.ClinicName,
	PatientType,
	PatientReference,
	HealthIssue,
	Age,
	Weight,
	Palse,
	BloodPressure,
	SpO2,
	Patient.Origin    -- ✅ NEW
	from Patient 
	left join Mas_Clinic on Patient.ClinicFid=Mas_Clinic.Fid and Mas_Clinic.Deactivate=0
	where Patient.Deactivate=0 
	and (PatientFid_Encrypted=@p_PatientFid_Encrypted OR master.dbo.fn_varbintohexstr(HashBytes('SHA2_256', convert(nvarchar(70), PatientFid))) = @p_PatientFid_Encrypted)
end

-- ============================================================
-- BLOCK 2: By Contact Number
-- ============================================================
else if (@p_PatientFid_Encrypted is null and @p_PatientContact is not null)
Begin
    -- 2A: Patient table madhe ahe
    IF EXISTS (SELECT 1 FROM Patient WHERE Deactivate = 0 AND ContactNumber = @p_PatientContact)
    BEGIN
        select 
            FORMAT(Patient.FDate, 'd MMM yyyy') AS FormattedDate,
            Patient.FDate,
            Patient.ClinicFid,
            ISNULL(NULLIF(PatientFid_Encrypted, ''), master.dbo.fn_varbintohexstr(HashBytes('SHA2_256', convert(nvarchar(70), PatientFid)))) AS PatientFid_Encrypted,
            PatientFid,
            Patient.UHIDNO,
            FirstName,
            LName,
            FirstName + ' ' + LName as PatientFullName,
            Email,
            ContactNumber,
            DateOfBirth,
            BloodGroup,
            Patient.Gender,
            Address,
            Patient.Status,
            Patient.Country,
            Patient.CityName,
            PatientProfile,
            Patient.PostalCode,
            PatientProfile,
            Mas_Clinic.ClinicName,
            PatientType,
            PatientReference,
            HealthIssue,
            Age,
            Weight,
            Palse,
            BloodPressure,
            SpO2,
            Patient.Origin    -- ✅ NEW
        from Patient 
        left join Mas_Clinic on Patient.ClinicFid=Mas_Clinic.Fid and Mas_Clinic.Deactivate=0
        where Patient.Deactivate=0 
        and Patient.ContactNumber=@p_PatientContact
    END
    ELSE
    BEGIN
        -- 2B: JYAMDB.dbo.UserProfile madhe ahe (self-registered)
        SELECT 
            FORMAT(GETDATE(), 'd MMM yyyy') AS FormattedDate,
            GETDATE() AS FDate,
            @p_ClinicFid AS ClinicFid,
            '' AS PatientFid_Encrypted,
            0 AS PatientFid,
            '' AS UHIDNO,
            FirstName,
            LastName AS LName,
            FirstName + ' ' + LastName AS PatientFullName,
            EmailId AS Email,
            MobileNo AS ContactNumber,
            DOB AS DateOfBirth,
            BloodGroup,
            Gender,
            Address,
            'Active' AS Status,
            Country,
            City AS CityName,
            ProfilePicture AS PatientProfile,
            PinCode AS PostalCode,
            'SelfRegistration' AS PatientType,
            '' AS PatientReference,
            '' AS HealthIssue,
            Age,
            0 AS Weight,
            '' AS Palse,
            '' AS BloodPressure,
            '' AS SpO2,
            'AyurMitra' AS Origin    -- ✅ NEW: JYAMDB users = AyurMitra
        FROM JYAMDB.dbo.UserProfile
        WHERE MobileNo = @p_PatientContact AND ISNULL(Deactivate, 0) = 0
    END
end

-- ============================================================
-- BLOCK 3: Full Patient List (PatientList page)
-- ============================================================
else
begin
select 
	FORMAT(Patient.FDate, 'd MMM yyyy') AS FormattedDate,
	Patient.FDate,
	Patient.ClinicFid,
	ISNULL(NULLIF(PatientFid_Encrypted, ''), master.dbo.fn_varbintohexstr(HashBytes('SHA2_256', convert(nvarchar(70), PatientFid)))) AS PatientFid_Encrypted,
	PatientFid,
	Patient.UHIDNO, 
	FirstName,
	LName,
	FirstName + ' ' + LName as PatientFullName,
	Email,
	ContactNumber,
	DateOfBirth,
	BloodGroup,
	Patient.Gender,
	Address,
	Patient.Status,
	Patient.Country,
	PatientProfile,
	Patient.CityName,
	PatientProfile,
	Patient.PostalCode,
	Mas_Clinic.ClinicName,
	PatientType,
	PatientReference,
	HealthIssue,
	Age,
	Weight,
	Palse,
	BloodPressure,
	SpO2,
	Patient.Origin    -- ✅ NEW
	from Patient 
	left join Mas_Clinic on Patient.ClinicFid=Mas_Clinic.Fid and Mas_Clinic.Deactivate=0
	where Patient.Deactivate=0 
	and Patient.ClinicFid=@p_ClinicFid
	order by PatientFid desc
end

end try
begin catch
Set @p_Message=ERROR_MESSAGE();
end catch
End
GO


-- ============================================================
-- Verify: Recent patients madhe Origin fetch hot ahe ka?
-- ============================================================
EXEC sp_List_Patient @p_ClinicFid = 1;   -- tumcha ClinicFid taka
GO
