USE [RohiniAyurvedaa]
GO

-- ============================================================
-- sp_SUD_Patient - UPDATE section madhe Origin protect kara
-- Rule: 
--   - SAVE  → Origin = @p_Origin (AyurMitra set kara)
--   - UPDATE → Origin ja DB madhe ahe tech thev, NULL asel tar mag @p_Origin lav
-- ============================================================
-- NOTE: Fakt UPDATE section madhe Origin = ISNULL(Origin, @p_Origin) kara
-- Khali complete sp_SUD_Patient ahe with this fix
-- ============================================================

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[sp_SUD_Patient]
(
@p_ProcessType nvarchar(15)=null,
@p_Message nvarchar(200)=null output,
@p_Icon nvarchar(10)=null output,
@p_FDate datetime=null,
@p_MacId nvarchar(50)=null,
@p_MacIp nvarchar(50)=null,
------------------------
@p_PatientFid int =null,
@p_PatientFid_Encrypted nvarchar(200)=null,
@p_ClinicFid int=null,
@p_FirstName nvarchar(50)=null,
@p_LName nvarchar(50)=null,
@p_Email nvarchar(100)=null,
@p_ContactNumber nvarchar(15)=null,
@p_DateOfBirth date=null,
@p_BloodGroup nvarchar(20)=null,
@p_Gender nvarchar(15)=null,
@p_Address nvarchar(500)=null,
@p_Country nvarchar(50)=null,
@p_CityName nvarchar(50)=null,
@p_PostalCode nvarchar(20)=null,
@P_PatientProfile nvarchar(200)=null,
@p_Status nvarchar(20)=null,
@p_Age nvarchar(50)=null,	
@p_HealthIssue nvarchar(500)=null,
@p_PatientType nvarchar(50)=null,
@p_PatientReference nvarchar(50)=null,
@p_Weight int=null,
@MUid int=null,
@p_Palse nvarchar(50)=null,
@p_BloodPressure nvarchar(50)=null,
@p_SpO2 nvarchar(50)=null,
@UHIDNO nvarchar(50)= null,
@p_Origin nvarchar(100)=null
)
as
Begin
 begin try

  -- ============================================================
  -- SAVE
  -- ============================================================
  if(@p_ProcessType='Save')
  begin

  IF EXISTS (
      SELECT 1 
      FROM Patient
      WHERE Deactivate = 0
        AND LOWER(LTRIM(RTRIM(ISNULL(FirstName,'')))) = LOWER(LTRIM(RTRIM(ISNULL(@p_FirstName,''))))
        AND LOWER(LTRIM(RTRIM(ISNULL(LName,'')))) = LOWER(LTRIM(RTRIM(ISNULL(@p_LName,''))))
        AND ISNULL(CONVERT(DATE, DateOfBirth),'1900-01-01') = ISNULL(@p_DateOfBirth,'1900-01-01')
  )
  BEGIN
      SET @p_Message = 'Patient with same Name & DOB already exists';
      SET @p_Icon = 'error';
      RETURN;
  END

  IF EXISTS (
      SELECT 1 FROM Patient
      WHERE Deactivate = 0
        AND ContactNumber = @p_ContactNumber
        AND FirstName = @p_FirstName
        AND LName = @p_LName
        AND DateOfBirth = @p_DateOfBirth
  )
  BEGIN
      SET @p_Message = 'Patient already exists';
      SET @p_Icon = 'error';
      RETURN;
  END

  DECLARE @MatchedUserId INT;
  SELECT TOP 1 @MatchedUserId = UserId
  FROM JYAMDB.dbo.UserProfile
  WHERE 
      LOWER(LTRIM(RTRIM(ISNULL(FirstName,'')))) = LOWER(LTRIM(RTRIM(ISNULL(@p_FirstName,''))))
      AND LOWER(LTRIM(RTRIM(ISNULL(LastName,'')))) = LOWER(LTRIM(RTRIM(ISNULL(@p_LName,''))))
      AND ISNULL(CONVERT(DATE, DOB),'1900-01-01') = ISNULL(@p_DateOfBirth,'1900-01-01')
      AND ISNULL(CAST(MobileNo AS NVARCHAR(15)),'') = ISNULL(@p_ContactNumber,'');

  DECLARE @NextPatientNo INT;
  SELECT @NextPatientNo = ISNULL(MAX(CAST(SUBSTRING(UHIDNO, 10, 5) AS INT)), 0) + 1
  FROM Patient WHERE Deactivate = 0;

  DECLARE @YearCode NVARCHAR(2)   = RIGHT(YEAR(GETDATE()),2);
  DECLARE @ClinicCode NVARCHAR(3) = RIGHT('000' + CAST(@p_ClinicFid AS NVARCHAR(3)), 3);
  DECLARE @PatientCode NVARCHAR(5)= RIGHT('00000' + CAST(@NextPatientNo AS NVARCHAR(5)), 5);
  DECLARE @UHID NVARCHAR(20)      = @YearCode + '/' + @ClinicCode + '/' + @PatientCode;

  INSERT INTO Patient
  (
    FDate, MacId, MacIp, MUid, UHIDNO, ClinicFid,
    FirstName, LName, Email, ContactNumber, DateOfBirth,
    BloodGroup, Gender, Address, Country, CityName, PostalCode,
    PatientProfile, Deactivate, Status, Age, HealthIssue,
    PatientType, PatientReference, Weight, Palse, BloodPressure, SpO2,
    Origin    -- ✅ Save madhe Origin set hote (AyurMitra)
  )
  VALUES
  (
    GETDATE(), @p_MacId, @p_MacIp, @MatchedUserId, @UHID, @p_ClinicFid,
    @p_FirstName, @p_LName, @p_Email, @p_ContactNumber, @p_DateOfBirth,
    @p_BloodGroup, @p_Gender, @p_Address, @p_Country, @p_CityName, @p_PostalCode,
    @P_PatientProfile, 0, @p_Status, @p_Age, @p_HealthIssue,
    @p_PatientType, @p_PatientReference, @p_Weight, @p_Palse, @p_BloodPressure, @p_SpO2,
    @p_Origin    -- ✅ 'AyurMitra' yet ahe web app madhtun
  )

  IF(@@ROWCOUNT > 0)
  BEGIN
    SET @p_Message = 'Record Save Successfuly';
    SET @p_Icon = 'success';
    SET @p_PatientFid = @@IDENTITY;
    UPDATE Patient 
    SET PatientFid_Encrypted = (master.dbo.fn_varbintohexstr(HashBytes('SHA2_256 ', CONVERT(nvarchar(70), @p_PatientFid)))) 
    WHERE PatientFid = @p_PatientFid;
  END
  ELSE
  BEGIN
    SET @p_Message = 'Record Not Save';
    SET @p_Icon = 'error';
  END

  END -- end save


  -- ============================================================
  -- UPDATE
  -- ============================================================
  IF(@p_ProcessType = 'Update')
  BEGIN
    UPDATE Patient SET 
      ModifiedDate     = GETDATE(),		
      ClinicFid        = @p_ClinicFid,
      FirstName        = @p_FirstName,
      LName            = @p_LName,
      Email            = @p_Email,
      ContactNumber    = @p_ContactNumber,
      DateOfBirth      = @p_DateOfBirth,
      BloodGroup       = @p_BloodGroup,
      Gender           = @p_Gender,
      Address          = @p_Address,
      Country          = @p_Country,
      CityName         = @p_CityName,
      PostalCode       = @p_PostalCode,
      PatientProfile   = @P_PatientProfile,
      Status           = @p_Status,
      Age              = @p_Age,
      PatientType      = @p_PatientType,
      PatientReference = @p_PatientReference,
      Weight           = @p_Weight,
      Palse            = @p_Palse,
      BloodPressure    = @p_BloodPressure,
      SpO2             = @p_SpO2,
      -- ✅ KEY FIX: Origin ja DB madhe ahe tech thev
      --    Mobile vrun 'AyurMitra' set asel tar to kadhi pan overwrite honar nahi
      --    Fakt NULL asel tarch @p_Origin lagu hoil
      Origin = ISNULL(NULLIF(Origin, ''), @p_Origin)
    WHERE Deactivate = 0 AND PatientFid_Encrypted = @p_PatientFid_Encrypted;

    IF(@@ROWCOUNT > 0)
    BEGIN
        DECLARE @UpdateMUid INT;
        
        SELECT @UpdateMUid = MUid 
        FROM Patient 
        WHERE Deactivate = 0 AND PatientFid_Encrypted = @p_PatientFid_Encrypted;
        
        IF (@UpdateMUid IS NULL OR @UpdateMUid = 0)
        BEGIN
            SELECT TOP 1 @UpdateMUid = UserId
            FROM JYAMDB.dbo.UserProfile
            WHERE 
                LOWER(LTRIM(RTRIM(ISNULL(FirstName,'')))) = LOWER(LTRIM(RTRIM(ISNULL(@p_FirstName,''))))
                AND LOWER(LTRIM(RTRIM(ISNULL(LastName,'')))) = LOWER(LTRIM(RTRIM(ISNULL(@p_LName,''))))
                AND ISNULL(CONVERT(DATE, DOB),'1900-01-01') = ISNULL(@p_DateOfBirth,'1900-01-01')
                AND ISNULL(CAST(MobileNo AS NVARCHAR(15)),'') = ISNULL(@p_ContactNumber,'');
                
            IF (@UpdateMUid IS NOT NULL AND @UpdateMUid > 0)
            BEGIN
                UPDATE Patient 
                SET MUid = @UpdateMUid 
                WHERE Deactivate = 0 AND PatientFid_Encrypted = @p_PatientFid_Encrypted;
            END
        END
        
        IF (@UpdateMUid IS NOT NULL AND @UpdateMUid > 0)
        BEGIN
            UPDATE JYAMDB.dbo.UserProfile
            SET 
                FirstName      = @p_FirstName,
                LastName       = @p_LName,
                EmailId        = @p_Email,
                MobileNo       = @p_ContactNumber,
                DOB            = @p_DateOfBirth,
                BloodGroup     = @p_BloodGroup,
                Gender         = @p_Gender,
                Address        = @p_Address,
                Country        = @p_Country,
                City           = @p_CityName,
                PinCode        = @p_PostalCode,
                Age            = @p_Age,
                ProfilePicture = @P_PatientProfile
            WHERE UserId = @UpdateMUid;
        END

        SET @p_Message = 'Record Updated Successfuly';
        SET @p_Icon = 'success';
    END
    ELSE
    BEGIN
        SET @p_Message = 'Record Not Updated';
        SET @p_Icon = 'error';
    END 
  END -- end update


  -- ============================================================
  -- DELETE
  -- ============================================================
  IF(@p_ProcessType = 'Delete')
  BEGIN
    UPDATE Patient SET Deactivate = 1
    WHERE PatientFid_Encrypted = @p_PatientFid_Encrypted;
    
    IF(@@ROWCOUNT > 0)
    BEGIN
      SET @p_Message = 'Record Deleted Successfuly';
      SET @p_Icon = 'success';
    END
    ELSE
    BEGIN
      SET @p_Message = 'Record not Deleted';
      SET @p_Icon = 'error';
    END
  END -- end delete

 END try
 BEGIN catch
   SET @p_Message = ERROR_MESSAGE();
 END catch
End
GO
