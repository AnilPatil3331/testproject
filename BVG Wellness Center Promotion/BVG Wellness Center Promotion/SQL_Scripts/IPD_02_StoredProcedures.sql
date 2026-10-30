-- ============================================================
-- IPD BED BOOKING MODULE - STORED PROCEDURES
-- BVG Wellness Center
-- Run AFTER IPD_01_CreateTables.sql
-- ============================================================

-- =============================================
-- 1. FLOOR MASTER CRUD
-- =============================================
IF OBJECT_ID('sp_IPD_FloorMaster_SUD', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_FloorMaster_SUD
GO
CREATE PROCEDURE sp_IPD_FloorMaster_SUD
    @Action       VARCHAR(10),   -- Save / Update / Delete
    @FloorId      INT = NULL,
    @FloorName    VARCHAR(100) = NULL,
    @FloorNumber  INT = NULL,
    @IsActive     BIT = 1,
    @CreatedBy    INT = NULL,
    @p_Message    VARCHAR(500) OUTPUT,
    @p_Icon       VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @Action = 'Save'
        BEGIN
            INSERT INTO tbl_IPD_FloorMaster (FloorName, FloorNumber, IsActive, CreatedBy)
            VALUES (@FloorName, @FloorNumber, @IsActive, @CreatedBy)
            SET @p_Message = 'Floor saved successfully'; SET @p_Icon = 'success'
        END
        ELSE IF @Action = 'Update'
        BEGIN
            UPDATE tbl_IPD_FloorMaster SET FloorName=@FloorName, FloorNumber=@FloorNumber, IsActive=@IsActive
            WHERE FloorId=@FloorId
            SET @p_Message = 'Floor updated successfully'; SET @p_Icon = 'success'
        END
        ELSE IF @Action = 'Delete'
        BEGIN
            UPDATE tbl_IPD_FloorMaster SET IsActive=0 WHERE FloorId=@FloorId
            SET @p_Message = 'Floor deleted successfully'; SET @p_Icon = 'success'
        END
    END TRY
    BEGIN CATCH
        SET @p_Message = ERROR_MESSAGE(); SET @p_Icon = 'error'
    END CATCH
END
GO

CREATE PROCEDURE sp_IPD_FloorMaster_List
    @FloorId  INT = NULL
AS
BEGIN
    SELECT FloorId, FloorName, FloorNumber, IsActive, CreatedAt
    FROM tbl_IPD_FloorMaster
    WHERE IsActive = 1 AND (@FloorId IS NULL OR FloorId = @FloorId)
    ORDER BY FloorNumber
END
GO

-- =============================================
-- 2. WARD MASTER CRUD
-- =============================================
IF OBJECT_ID('sp_IPD_WardMaster_SUD', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_WardMaster_SUD
GO
CREATE PROCEDURE sp_IPD_WardMaster_SUD
    @Action     VARCHAR(10),
    @WardId     INT = NULL,
    @WardName   VARCHAR(100) = NULL,
    @WardType   VARCHAR(50) = NULL,
    @IsActive   BIT = 1,
    @CreatedBy  INT = NULL,
    @p_Message  VARCHAR(500) OUTPUT,
    @p_Icon     VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @Action = 'Save'
            INSERT INTO tbl_IPD_WardMaster (WardName, WardType, IsActive, CreatedBy)
            VALUES (@WardName, @WardType, @IsActive, @CreatedBy)
        ELSE IF @Action = 'Update'
            UPDATE tbl_IPD_WardMaster SET WardName=@WardName, WardType=@WardType, IsActive=@IsActive
            WHERE WardId=@WardId
        ELSE IF @Action = 'Delete'
            UPDATE tbl_IPD_WardMaster SET IsActive=0 WHERE WardId=@WardId
        SET @p_Message = 'Ward ' + @Action + 'd successfully'; SET @p_Icon = 'success'
    END TRY
    BEGIN CATCH
        SET @p_Message = ERROR_MESSAGE(); SET @p_Icon = 'error'
    END CATCH
END
GO

CREATE PROCEDURE sp_IPD_WardMaster_List
    @WardId INT = NULL
AS
BEGIN
    SELECT WardId, WardName, WardType, IsActive, CreatedAt
    FROM tbl_IPD_WardMaster
    WHERE IsActive = 1 AND (@WardId IS NULL OR WardId = @WardId)
    ORDER BY WardName
END
GO

-- =============================================
-- 3. CATEGORY MASTER CRUD
-- =============================================
IF OBJECT_ID('sp_IPD_CategoryMaster_SUD', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_CategoryMaster_SUD
GO
CREATE PROCEDURE sp_IPD_CategoryMaster_SUD
    @Action         VARCHAR(10),
    @CategoryId     INT = NULL,
    @CategoryName   VARCHAR(100) = NULL,
    @FoodOption     VARCHAR(20) = 'None',
    @IsActive       BIT = 1,
    @CreatedBy      INT = NULL,
    @p_Message      VARCHAR(500) OUTPUT,
    @p_Icon         VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @Action = 'Save'
            INSERT INTO tbl_IPD_CategoryMaster (CategoryName, FoodOption, IsActive, CreatedBy)
            VALUES (@CategoryName, @FoodOption, @IsActive, @CreatedBy)
        ELSE IF @Action = 'Update'
            UPDATE tbl_IPD_CategoryMaster SET CategoryName=@CategoryName, FoodOption=@FoodOption, IsActive=@IsActive
            WHERE CategoryId=@CategoryId
        ELSE IF @Action = 'Delete'
            UPDATE tbl_IPD_CategoryMaster SET IsActive=0 WHERE CategoryId=@CategoryId
        SET @p_Message = 'Category ' + @Action + 'd successfully'; SET @p_Icon = 'success'
    END TRY
    BEGIN CATCH
        SET @p_Message = ERROR_MESSAGE(); SET @p_Icon = 'error'
    END CATCH
END
GO

CREATE PROCEDURE sp_IPD_CategoryMaster_List
    @CategoryId INT = NULL
AS
BEGIN
    SELECT CategoryId, CategoryName, FoodOption, IsActive, CreatedAt
    FROM tbl_IPD_CategoryMaster
    WHERE IsActive = 1 AND (@CategoryId IS NULL OR CategoryId = @CategoryId)
    ORDER BY CategoryName
END
GO

-- =============================================
-- 4. ROOM MASTER CRUD
-- =============================================
IF OBJECT_ID('sp_IPD_RoomMaster_SUD', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_RoomMaster_SUD
GO
CREATE PROCEDURE sp_IPD_RoomMaster_SUD
    @Action              VARCHAR(10),
    @RoomId              INT = NULL,
    @RoomNumber          VARCHAR(20) = NULL,
    @FloorId             INT = NULL,
    @WardId              INT = NULL,
    @CategoryId          INT = NULL,
    @RoomChargePerDay    DECIMAL(10,2) = 0,
    @IsActive            BIT = 1,
    @CreatedBy           INT = NULL,
    @p_Message           VARCHAR(500) OUTPUT,
    @p_Icon              VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @Action = 'Save'
            INSERT INTO tbl_IPD_RoomMaster (RoomNumber, FloorId, WardId, CategoryId, RoomChargePerDay, IsActive, CreatedBy)
            VALUES (@RoomNumber, @FloorId, @WardId, @CategoryId, @RoomChargePerDay, @IsActive, @CreatedBy)
        ELSE IF @Action = 'Update'
            UPDATE tbl_IPD_RoomMaster
            SET RoomNumber=@RoomNumber, FloorId=@FloorId, WardId=@WardId, CategoryId=@CategoryId,
                RoomChargePerDay=@RoomChargePerDay, IsActive=@IsActive
            WHERE RoomId=@RoomId
        ELSE IF @Action = 'Delete'
            UPDATE tbl_IPD_RoomMaster SET IsActive=0 WHERE RoomId=@RoomId
        SET @p_Message = 'Room ' + @Action + 'd successfully'; SET @p_Icon = 'success'
    END TRY
    BEGIN CATCH
        SET @p_Message = ERROR_MESSAGE(); SET @p_Icon = 'error'
    END CATCH
END
GO

CREATE PROCEDURE sp_IPD_RoomMaster_List
    @RoomId      INT = NULL,
    @FloorId     INT = NULL,
    @WardId      INT = NULL,
    @CategoryId  INT = NULL
AS
BEGIN
    SELECT r.RoomId, r.RoomNumber, r.FloorId, r.WardId, r.CategoryId,
           r.RoomChargePerDay, r.IsActive,
           f.FloorName, w.WardName, c.CategoryName
    FROM tbl_IPD_RoomMaster r
    LEFT JOIN tbl_IPD_FloorMaster f ON r.FloorId = f.FloorId
    LEFT JOIN tbl_IPD_WardMaster w ON r.WardId = w.WardId
    LEFT JOIN tbl_IPD_CategoryMaster c ON r.CategoryId = c.CategoryId
    WHERE r.IsActive = 1
      AND (@RoomId IS NULL OR r.RoomId = @RoomId)
      AND (@FloorId IS NULL OR r.FloorId = @FloorId)
      AND (@WardId IS NULL OR r.WardId = @WardId)
      AND (@CategoryId IS NULL OR r.CategoryId = @CategoryId)
    ORDER BY r.RoomNumber
END
GO

-- =============================================
-- 5. ROOM CHARGES MASTER CRUD
-- =============================================
IF OBJECT_ID('sp_IPD_RoomChargesMaster_SUD', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_RoomChargesMaster_SUD
GO
CREATE PROCEDURE sp_IPD_RoomChargesMaster_SUD
    @Action         VARCHAR(10),
    @ChargeId       INT = NULL,
    @ChargeName     VARCHAR(200) = NULL,
    @ChargeType     VARCHAR(30) = 'ServiceCharge',
    @DefaultAmount  DECIMAL(10,2) = 0,
    @IsActive       BIT = 1,
    @CreatedBy      INT = NULL,
    @p_Message      VARCHAR(500) OUTPUT,
    @p_Icon         VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @Action = 'Save'
            INSERT INTO tbl_IPD_RoomChargesMaster (ChargeName, ChargeType, DefaultAmount, IsActive, CreatedBy)
            VALUES (@ChargeName, @ChargeType, @DefaultAmount, @IsActive, @CreatedBy)
        ELSE IF @Action = 'Update'
            UPDATE tbl_IPD_RoomChargesMaster
            SET ChargeName=@ChargeName, ChargeType=@ChargeType, DefaultAmount=@DefaultAmount, IsActive=@IsActive
            WHERE ChargeId=@ChargeId
        ELSE IF @Action = 'Delete'
            UPDATE tbl_IPD_RoomChargesMaster SET IsActive=0 WHERE ChargeId=@ChargeId
        SET @p_Message = 'Charge ' + @Action + 'd successfully'; SET @p_Icon = 'success'
    END TRY
    BEGIN CATCH
        SET @p_Message = ERROR_MESSAGE(); SET @p_Icon = 'error'
    END CATCH
END
GO

CREATE PROCEDURE sp_IPD_RoomChargesMaster_List
AS
BEGIN
    SELECT ChargeId, ChargeName, ChargeType, DefaultAmount, IsActive
    FROM tbl_IPD_RoomChargesMaster WHERE IsActive=1 ORDER BY ChargeName
END
GO

-- =============================================
-- 6. ROOM CHARGES MAPPING
-- =============================================
IF OBJECT_ID('sp_IPD_RoomChargesMapping_Save', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_RoomChargesMapping_Save
GO
CREATE PROCEDURE sp_IPD_RoomChargesMapping_Save
    @RoomId       INT,
    @ChargeId     INT,
    @ChargeAmount DECIMAL(10,2),
    @IsCompulsory BIT,
    @p_Message    VARCHAR(500) OUTPUT,
    @p_Icon       VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM tbl_IPD_RoomChargesMapping WHERE RoomId=@RoomId AND ChargeId=@ChargeId)
            UPDATE tbl_IPD_RoomChargesMapping
            SET ChargeAmount=@ChargeAmount, IsCompulsory=@IsCompulsory, IsActive=1
            WHERE RoomId=@RoomId AND ChargeId=@ChargeId
        ELSE
            INSERT INTO tbl_IPD_RoomChargesMapping (RoomId, ChargeId, ChargeAmount, IsCompulsory, IsActive)
            VALUES (@RoomId, @ChargeId, @ChargeAmount, @IsCompulsory, 1)
        SET @p_Message = 'Charge mapped successfully'; SET @p_Icon = 'success'
    END TRY
    BEGIN CATCH
        SET @p_Message = ERROR_MESSAGE(); SET @p_Icon = 'error'
    END CATCH
END
GO

IF OBJECT_ID('sp_IPD_RoomChargesMapping_Delete', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_RoomChargesMapping_Delete
GO
CREATE PROCEDURE sp_IPD_RoomChargesMapping_Delete
    @MappingId  INT,
    @p_Message  VARCHAR(500) OUTPUT,
    @p_Icon     VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE tbl_IPD_RoomChargesMapping SET IsActive=0 WHERE MappingId=@MappingId
        SET @p_Message = 'Charge removed'; SET @p_Icon = 'success'
    END TRY
    BEGIN CATCH
        SET @p_Message = ERROR_MESSAGE(); SET @p_Icon = 'error'
    END CATCH
END
GO

IF OBJECT_ID('sp_IPD_RoomChargesMapping_GetByRoom', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_RoomChargesMapping_GetByRoom
GO
CREATE PROCEDURE sp_IPD_RoomChargesMapping_GetByRoom
    @RoomId INT
AS
BEGIN
    SELECT m.MappingId, m.RoomId, m.ChargeId, c.ChargeName, c.ChargeType,
           m.ChargeAmount, m.IsCompulsory, m.IsActive
    FROM tbl_IPD_RoomChargesMapping m
    JOIN tbl_IPD_RoomChargesMaster c ON m.ChargeId = c.ChargeId
    WHERE m.RoomId = @RoomId AND m.IsActive = 1
    ORDER BY m.IsCompulsory DESC, c.ChargeName
END
GO

-- =============================================
-- 7. BED MASTER CRUD
-- =============================================
IF OBJECT_ID('sp_IPD_BedMaster_SUD', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_BedMaster_SUD
GO
CREATE PROCEDURE sp_IPD_BedMaster_SUD
    @Action     VARCHAR(20),
    @BedId      INT = NULL,
    @RoomId     INT = NULL,
    @BedNumber  VARCHAR(20) = NULL,
    @BedStatus  VARCHAR(20) = 'Available',
    @IsActive   BIT = 1,
    @CreatedBy  INT = NULL,
    @p_Message  VARCHAR(500) OUTPUT,
    @p_Icon     VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @Action = 'Save'
            INSERT INTO tbl_IPD_BedMaster (RoomId, BedNumber, BedStatus, IsActive, CreatedBy)
            VALUES (@RoomId, @BedNumber, 'Available', @IsActive, @CreatedBy)
        ELSE IF @Action = 'Update'
            UPDATE tbl_IPD_BedMaster SET BedNumber=@BedNumber, IsActive=@IsActive WHERE BedId=@BedId
        ELSE IF @Action = 'Delete'
            UPDATE tbl_IPD_BedMaster SET IsActive=0 WHERE BedId=@BedId
        ELSE IF @Action = 'UpdateStatus'
            UPDATE tbl_IPD_BedMaster SET BedStatus=@BedStatus WHERE BedId=@BedId
        SET @p_Message = 'Bed ' + @Action + ' successfully'; SET @p_Icon = 'success'
    END TRY
    BEGIN CATCH
        SET @p_Message = ERROR_MESSAGE(); SET @p_Icon = 'error'
    END CATCH
END
GO

CREATE PROCEDURE sp_IPD_BedMaster_List
    @RoomId INT = NULL,
    @BedId  INT = NULL
AS
BEGIN
    SELECT b.BedId, b.RoomId, b.BedNumber, b.BedStatus, b.IsActive,
           r.RoomNumber, f.FloorName, w.WardName, c.CategoryName
    FROM tbl_IPD_BedMaster b
    JOIN tbl_IPD_RoomMaster r ON b.RoomId = r.RoomId
    LEFT JOIN tbl_IPD_FloorMaster f ON r.FloorId = f.FloorId
    LEFT JOIN tbl_IPD_WardMaster w ON r.WardId = w.WardId
    LEFT JOIN tbl_IPD_CategoryMaster c ON r.CategoryId = c.CategoryId
    WHERE b.IsActive = 1
      AND (@RoomId IS NULL OR b.RoomId = @RoomId)
      AND (@BedId IS NULL OR b.BedId = @BedId)
    ORDER BY b.BedNumber
END
GO

-- Get Available beds only (for booking form dropdown)
IF OBJECT_ID('sp_IPD_GetAvailableBeds', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_GetAvailableBeds
GO
CREATE PROCEDURE sp_IPD_GetAvailableBeds
    @RoomId INT
AS
BEGIN
    SELECT BedId, BedNumber, BedStatus
    FROM tbl_IPD_BedMaster
    WHERE RoomId = @RoomId AND BedStatus = 'Available' AND IsActive = 1
    ORDER BY BedNumber
END
GO

-- =============================================
-- 8. FOOD PLAN MASTER CRUD
-- =============================================
IF OBJECT_ID('sp_IPD_FoodPlanMaster_SUD', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_FoodPlanMaster_SUD
GO
CREATE PROCEDURE sp_IPD_FoodPlanMaster_SUD
    @Action       VARCHAR(10),
    @FoodPlanId   INT = NULL,
    @PlanName     VARCHAR(100) = NULL,
    @DailyCharge  DECIMAL(10,2) = 0,
    @IsActive     BIT = 1,
    @CreatedBy    INT = NULL,
    @p_Message    VARCHAR(500) OUTPUT,
    @p_Icon       VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @Action = 'Save'
            INSERT INTO tbl_IPD_FoodPlanMaster (PlanName, DailyCharge, IsActive, CreatedBy)
            VALUES (@PlanName, @DailyCharge, @IsActive, @CreatedBy)
        ELSE IF @Action = 'Update'
            UPDATE tbl_IPD_FoodPlanMaster SET PlanName=@PlanName, DailyCharge=@DailyCharge, IsActive=@IsActive
            WHERE FoodPlanId=@FoodPlanId
        ELSE IF @Action = 'Delete'
            UPDATE tbl_IPD_FoodPlanMaster SET IsActive=0 WHERE FoodPlanId=@FoodPlanId
        SET @p_Message = 'Food Plan ' + @Action + 'd successfully'; SET @p_Icon = 'success'
    END TRY
    BEGIN CATCH
        SET @p_Message = ERROR_MESSAGE(); SET @p_Icon = 'error'
    END CATCH
END
GO

CREATE PROCEDURE sp_IPD_FoodPlanMaster_List
AS
BEGIN
    SELECT FoodPlanId, PlanName, DailyCharge, IsActive
    FROM tbl_IPD_FoodPlanMaster WHERE IsActive=1 ORDER BY PlanName
END
GO

-- =============================================
-- 9. HOSPITAL SETTINGS
-- =============================================
IF OBJECT_ID('sp_IPD_HospitalSetting_Get', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_HospitalSetting_Get
GO
CREATE PROCEDURE sp_IPD_HospitalSetting_Get
AS
BEGIN
    SELECT TOP 1 * FROM tbl_IPD_HospitalSetting
END
GO

IF OBJECT_ID('sp_IPD_HospitalSetting_Save', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_HospitalSetting_Save
GO
CREATE PROCEDURE sp_IPD_HospitalSetting_Save
    @UseFloor                   BIT,
    @UseWard                    BIT,
    @UseCategory                BIT,
    @FoodModuleEnabled          BIT,
    @FoodMandatory              BIT,
    @AdvanceRequired            BIT,
    @BedTransferAllowed         BIT,
    @PackageBillingEnabled      BIT,
    @AttendantAllowed           BIT,
    @AutoReleaseBedOnDischarge  BIT,
    @GSTEnabled                 BIT,
    @GSTRate                    DECIMAL(5,2),
    @UpdatedBy                  INT,
    @p_Message                  VARCHAR(500) OUTPUT,
    @p_Icon                     VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE tbl_IPD_HospitalSetting
        SET UseFloor=@UseFloor, UseWard=@UseWard, UseCategory=@UseCategory,
            FoodModuleEnabled=@FoodModuleEnabled, FoodMandatory=@FoodMandatory,
            AdvanceRequired=@AdvanceRequired, BedTransferAllowed=@BedTransferAllowed,
            PackageBillingEnabled=@PackageBillingEnabled, AttendantAllowed=@AttendantAllowed,
            AutoReleaseBedOnDischarge=@AutoReleaseBedOnDischarge,
            GSTEnabled=@GSTEnabled, GSTRate=@GSTRate,
            UpdatedAt=GETDATE(), UpdatedBy=@UpdatedBy
        SET @p_Message = 'Settings saved successfully'; SET @p_Icon = 'success'
    END TRY
    BEGIN CATCH
        SET @p_Message = ERROR_MESSAGE(); SET @p_Icon = 'error'
    END CATCH
END
GO

-- =============================================
-- 10. STAY BOOKING - ADMIT
-- =============================================
IF OBJECT_ID('sp_IPD_StayBooking_Admit', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_StayBooking_Admit
GO
CREATE PROCEDURE sp_IPD_StayBooking_Admit
    @PatientFid         INT,
    @DoctorId           INT = NULL,
    @AdmissionDate      DATETIME,
    @ExpectedDischarge  DATETIME,
    @AdmissionType      VARCHAR(20),
    @FloorId            INT = NULL,
    @WardId             INT = NULL,
    @CategoryId         INT = NULL,
    @RoomId             INT,
    @BedId              INT,
    @RoomChargeSnapshot DECIMAL(10,2),
    @FoodPlanId         INT = NULL,
    @FoodDailyCharge    DECIMAL(10,2) = 0,
    @AdvanceAmount      DECIMAL(10,2) = 0,
    @AdvancePayMode     VARCHAR(50) = NULL,
    @Remarks            VARCHAR(500) = NULL,
    @CreatedBy          INT = NULL,
    @NewBookingId       INT OUTPUT,
    @p_Message          VARCHAR(500) OUTPUT,
    @p_Icon             VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Generate BookingNo: IPD + Year + 5-digit sequence
        DECLARE @BookingNo VARCHAR(20)
        DECLARE @SeqNum INT
        SELECT @SeqNum = ISNULL(MAX(BookingId), 0) + 1 FROM tbl_IPD_StayBooking
        SET @BookingNo = 'IPD' + CAST(YEAR(@AdmissionDate) AS VARCHAR) + RIGHT('00000' + CAST(@SeqNum AS VARCHAR), 5)

        -- Insert StayBooking header
        INSERT INTO tbl_IPD_StayBooking
            (BookingNo, PatientFid, DoctorId, AdmissionDate, ExpectedDischarge, AdmissionType, Status, Remarks, CreatedBy)
        VALUES
            (@BookingNo, @PatientFid, @DoctorId, @AdmissionDate, @ExpectedDischarge, @AdmissionType, 'Active', @Remarks, @CreatedBy)

        SET @NewBookingId = SCOPE_IDENTITY()

        -- Insert Location
        DECLARE @LocationId INT
        INSERT INTO tbl_IPD_StayBookingLocation
            (BookingId, FloorId, WardId, CategoryId, RoomId, BedId, RoomChargeSnapshot, EffectiveFrom)
        VALUES
            (@NewBookingId, @FloorId, @WardId, @CategoryId, @RoomId, @BedId, @RoomChargeSnapshot, @AdmissionDate)
        SET @LocationId = SCOPE_IDENTITY()

        -- Insert Food if selected
        IF @FoodPlanId IS NOT NULL AND @FoodPlanId > 0
        BEGIN
            DECLARE @FoodPlanName VARCHAR(100)
            SELECT @FoodPlanName = PlanName FROM tbl_IPD_FoodPlanMaster WHERE FoodPlanId = @FoodPlanId
            INSERT INTO tbl_IPD_StayBookingFood (BookingId, FoodPlanId, PlanName, DailyCharge, StartDate)
            VALUES (@NewBookingId, @FoodPlanId, @FoodPlanName, @FoodDailyCharge, @AdmissionDate)
        END

        -- Insert Advance if given
        IF @AdvanceAmount > 0
        BEGIN
            INSERT INTO tbl_IPD_StayBookingAdvance (BookingId, Amount, PaymentMode, PaidAt, CreatedBy)
            VALUES (@NewBookingId, @AdvanceAmount, @AdvancePayMode, GETDATE(), @CreatedBy)
        END

        -- Update Bed Status to Occupied
        UPDATE tbl_IPD_BedMaster SET BedStatus = 'Occupied' WHERE BedId = @BedId

        COMMIT TRANSACTION;
        SET @p_Message = 'Patient admitted successfully. Booking No: ' + @BookingNo
        SET @p_Icon = 'success'
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @NewBookingId = 0
        SET @p_Message = ERROR_MESSAGE(); SET @p_Icon = 'error'
    END CATCH
END
GO

-- =============================================
-- 11. SAVE BOOKING CHARGES (called after Admit)
-- =============================================
IF OBJECT_ID('sp_IPD_StayBookingCharges_Save', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_StayBookingCharges_Save
GO
CREATE PROCEDURE sp_IPD_StayBookingCharges_Save
    @BookingId      INT,
    @LocationId     INT,
    @ChargeId       INT,
    @ChargeName     VARCHAR(200),
    @ChargeType     VARCHAR(30),
    @IsCompulsory   BIT,
    @AmountPerDay   DECIMAL(10,2),
    @IsSelected     BIT,
    @EffectiveFrom  DATETIME
AS
BEGIN
    INSERT INTO tbl_IPD_StayBookingCharges
        (BookingId, LocationId, ChargeId, ChargeName, ChargeType, IsCompulsory, AmountPerDay, IsSelected, EffectiveFrom)
    VALUES
        (@BookingId, @LocationId, @ChargeId, @ChargeName, @ChargeType, @IsCompulsory, @AmountPerDay, @IsSelected, @EffectiveFrom)
END
GO

-- =============================================
-- 12. STAY BOOKING LIST
-- =============================================
IF OBJECT_ID('sp_IPD_StayBooking_List', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_StayBooking_List
GO
CREATE PROCEDURE sp_IPD_StayBooking_List
    @Status     VARCHAR(20) = NULL,
    @FromDate   DATE = NULL,
    @ToDate     DATE = NULL
AS
BEGIN
    SELECT
        sb.BookingId, sb.BookingNo, sb.PatientFid, sb.DoctorId,
        sb.AdmissionDate, sb.ExpectedDischarge, sb.ActualDischarge,
        sb.AdmissionType, sb.Status,
        loc.RoomId, loc.BedId,
        r.RoomNumber, b.BedNumber,
        f.FloorName, w.WardName, c.CategoryName,
        loc.RoomChargeSnapshot,
        DATEDIFF(DAY, sb.AdmissionDate, ISNULL(sb.ActualDischarge, GETDATE())) AS DaysCount,
        ISNULL(bill.NetPayable, 0) AS FinalBillAmount
    FROM tbl_IPD_StayBooking sb
    LEFT JOIN tbl_IPD_StayBookingLocation loc ON sb.BookingId = loc.BookingId AND loc.EffectiveTo IS NULL
    LEFT JOIN tbl_IPD_RoomMaster r ON loc.RoomId = r.RoomId
    LEFT JOIN tbl_IPD_BedMaster b ON loc.BedId = b.BedId
    LEFT JOIN tbl_IPD_FloorMaster f ON loc.FloorId = f.FloorId
    LEFT JOIN tbl_IPD_WardMaster w ON loc.WardId = w.WardId
    LEFT JOIN tbl_IPD_CategoryMaster c ON loc.CategoryId = c.CategoryId
    LEFT JOIN tbl_IPD_Billing bill ON sb.BookingId = bill.BookingId
    WHERE (@Status IS NULL OR sb.Status = @Status)
      AND (@FromDate IS NULL OR CAST(sb.AdmissionDate AS DATE) >= @FromDate)
      AND (@ToDate IS NULL OR CAST(sb.AdmissionDate AS DATE) <= @ToDate)
    ORDER BY sb.BookingId DESC
END
GO

-- =============================================
-- 13. BED TRANSFER
-- =============================================
IF OBJECT_ID('sp_IPD_BedTransfer_Save', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_BedTransfer_Save
GO
CREATE PROCEDURE sp_IPD_BedTransfer_Save
    @BookingId      INT,
    @NewFloorId     INT = NULL,
    @NewWardId      INT = NULL,
    @NewCategoryId  INT = NULL,
    @NewRoomId      INT,
    @NewBedId       INT,
    @NewRoomCharge  DECIMAL(10,2),
    @TransferDate   DATETIME,
    @Reason         VARCHAR(200) = NULL,
    @Remarks        VARCHAR(500) = NULL,
    @TransferredBy  INT = NULL,
    @p_Message      VARCHAR(500) OUTPUT,
    @p_Icon         VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Close current location
        DECLARE @OldLocationId INT, @OldBedId INT
        SELECT @OldLocationId = LocationId, @OldBedId = BedId
        FROM tbl_IPD_StayBookingLocation
        WHERE BookingId = @BookingId AND EffectiveTo IS NULL

        UPDATE tbl_IPD_StayBookingLocation SET EffectiveTo = @TransferDate WHERE LocationId = @OldLocationId

        -- Close current charges
        UPDATE tbl_IPD_StayBookingCharges SET EffectiveTo = @TransferDate
        WHERE BookingId = @BookingId AND EffectiveTo IS NULL

        -- Close current food
        UPDATE tbl_IPD_StayBookingFood SET EndDate = @TransferDate
        WHERE BookingId = @BookingId AND EndDate IS NULL

        -- Open new location
        DECLARE @NewLocationId INT
        INSERT INTO tbl_IPD_StayBookingLocation
            (BookingId, FloorId, WardId, CategoryId, RoomId, BedId, RoomChargeSnapshot, EffectiveFrom)
        VALUES
            (@BookingId, @NewFloorId, @NewWardId, @NewCategoryId, @NewRoomId, @NewBedId, @NewRoomCharge, @TransferDate)
        SET @NewLocationId = SCOPE_IDENTITY()

        -- Update bed statuses
        UPDATE tbl_IPD_BedMaster SET BedStatus = 'Available' WHERE BedId = @OldBedId
        UPDATE tbl_IPD_BedMaster SET BedStatus = 'Occupied' WHERE BedId = @NewBedId

        -- Save transfer history
        INSERT INTO tbl_IPD_BedTransferHistory
            (BookingId, OldLocationId, NewLocationId, TransferDate, Reason, Remarks, TransferredBy)
        VALUES
            (@BookingId, @OldLocationId, @NewLocationId, @TransferDate, @Reason, @Remarks, @TransferredBy)

        COMMIT TRANSACTION;
        SET @p_Message = 'Bed transferred successfully'; SET @p_Icon = 'success'
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @p_Message = ERROR_MESSAGE(); SET @p_Icon = 'error'
    END CATCH
END
GO

-- =============================================
-- 14. GENERATE BILL
-- =============================================
IF OBJECT_ID('sp_IPD_GenerateBill', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_GenerateBill
GO
CREATE PROCEDURE sp_IPD_GenerateBill
    @BookingId INT,
    @DischargeDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @EndDate DATETIME = ISNULL(@DischargeDate, GETDATE());

    -- Room Rent per location period
    SELECT 'RoomRent' AS BillSection,
           r.RoomNumber + ' - Bed ' + b.BedNumber AS Description,
           loc.RoomChargeSnapshot AS RatePerDay,
           DATEDIFF(DAY, loc.EffectiveFrom, ISNULL(loc.EffectiveTo, @EndDate)) + 1 AS Days,
           loc.RoomChargeSnapshot * (DATEDIFF(DAY, loc.EffectiveFrom, ISNULL(loc.EffectiveTo, @EndDate)) + 1) AS Amount,
           loc.EffectiveFrom, loc.EffectiveTo
    FROM tbl_IPD_StayBookingLocation loc
    JOIN tbl_IPD_RoomMaster r ON loc.RoomId = r.RoomId
    JOIN tbl_IPD_BedMaster b ON loc.BedId = b.BedId
    WHERE loc.BookingId = @BookingId

    UNION ALL

    -- Extra Charges (compulsory + selected optional)
    SELECT
        CASE WHEN IsCompulsory=1 THEN 'CompulsoryCharge' ELSE 'OptionalCharge' END AS BillSection,
        ChargeName AS Description,
        AmountPerDay AS RatePerDay,
        DATEDIFF(DAY, EffectiveFrom, ISNULL(EffectiveTo, @EndDate)) + 1 AS Days,
        AmountPerDay * (DATEDIFF(DAY, EffectiveFrom, ISNULL(EffectiveTo, @EndDate)) + 1) AS Amount,
        EffectiveFrom, EffectiveTo
    FROM tbl_IPD_StayBookingCharges
    WHERE BookingId = @BookingId AND IsSelected = 1

    UNION ALL

    -- Food Charges
    SELECT 'FoodCharge' AS BillSection,
           PlanName AS Description,
           DailyCharge AS RatePerDay,
           DATEDIFF(DAY, StartDate, ISNULL(EndDate, @EndDate)) + 1 AS Days,
           DailyCharge * (DATEDIFF(DAY, StartDate, ISNULL(EndDate, @EndDate)) + 1) AS Amount,
           StartDate AS EffectiveFrom, EndDate AS EffectiveTo
    FROM tbl_IPD_StayBookingFood
    WHERE BookingId = @BookingId

    ORDER BY BillSection, EffectiveFrom
END
GO

-- Get Advance Total for a booking
IF OBJECT_ID('sp_IPD_GetAdvanceTotal', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_GetAdvanceTotal
GO
CREATE PROCEDURE sp_IPD_GetAdvanceTotal
    @BookingId INT
AS
BEGIN
    SELECT ISNULL(SUM(Amount), 0) AS TotalAdvance FROM tbl_IPD_StayBookingAdvance WHERE BookingId = @BookingId
END
GO

-- =============================================
-- 15. CONFIRM DISCHARGE
-- =============================================
IF OBJECT_ID('sp_IPD_Discharge_Confirm', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_Discharge_Confirm
GO
CREATE PROCEDURE sp_IPD_Discharge_Confirm
    @BookingId          INT,
    @DischargeDate      DATETIME,
    @AutoReleaseBed     BIT = 1,
    @GeneratedBy        INT = NULL,
    @RoomTotal          DECIMAL(10,2) = 0,
    @CompulsoryTotal    DECIMAL(10,2) = 0,
    @OptionalTotal      DECIMAL(10,2) = 0,
    @FoodTotal          DECIMAL(10,2) = 0,
    @Subtotal           DECIMAL(10,2) = 0,
    @GSTAmount          DECIMAL(10,2) = 0,
    @GrossTotal         DECIMAL(10,2) = 0,
    @AdvancePaid        DECIMAL(10,2) = 0,
    @NetPayable         DECIMAL(10,2) = 0,
    @p_Message          VARCHAR(500) OUTPUT,
    @p_Icon             VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Update booking status
        UPDATE tbl_IPD_StayBooking
        SET Status = 'Discharged', ActualDischarge = @DischargeDate
        WHERE BookingId = @BookingId

        -- Close open location
        DECLARE @BedId INT
        SELECT @BedId = BedId FROM tbl_IPD_StayBookingLocation
        WHERE BookingId = @BookingId AND EffectiveTo IS NULL

        UPDATE tbl_IPD_StayBookingLocation SET EffectiveTo = @DischargeDate
        WHERE BookingId = @BookingId AND EffectiveTo IS NULL

        -- Close open charges
        UPDATE tbl_IPD_StayBookingCharges SET EffectiveTo = @DischargeDate
        WHERE BookingId = @BookingId AND EffectiveTo IS NULL

        -- Close open food
        UPDATE tbl_IPD_StayBookingFood SET EndDate = @DischargeDate
        WHERE BookingId = @BookingId AND EndDate IS NULL

        -- Generate Bill No
        DECLARE @BillNo VARCHAR(30)
        SET @BillNo = 'INV-' + FORMAT(GETDATE(), 'yyyyMM') + '-' + RIGHT('0000' + CAST(@BookingId AS VARCHAR), 4)
        
        DECLARE @PatientFid INT
        SELECT @PatientFid = PatientFid FROM tbl_IPD_StayBooking WHERE BookingId = @BookingId

        -- Insert into Billing
        IF NOT EXISTS (SELECT 1 FROM tbl_IPD_Billing WHERE BookingId = @BookingId)
        BEGIN
            INSERT INTO tbl_IPD_Billing (
                BillNo, BookingId, PatientFid, RoomTotal, CompulsoryTotal, OptionalTotal, FoodTotal, 
                Subtotal, GSTAmount, GrossTotal, AdvancePaid, NetPayable, BillDate, GeneratedBy
            )
            VALUES (
                @BillNo, @BookingId, @PatientFid, @RoomTotal, @CompulsoryTotal, @OptionalTotal, @FoodTotal,
                @Subtotal, @GSTAmount, @GrossTotal, @AdvancePaid, @NetPayable, GETDATE(), @GeneratedBy
            )
        END

        -- Release bed if setting enabled
        IF @AutoReleaseBed = 1 AND @BedId IS NOT NULL
            UPDATE tbl_IPD_BedMaster SET BedStatus = 'Available' WHERE BedId = @BedId

        COMMIT TRANSACTION;
        SET @p_Message = 'Patient discharged successfully'; SET @p_Icon = 'success'
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @p_Message = ERROR_MESSAGE(); SET @p_Icon = 'error'
    END CATCH
END
GO

-- =============================================
-- 16. BED AVAILABILITY DASHBOARD
-- =============================================
IF OBJECT_ID('sp_IPD_BedDashboard', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_BedDashboard
GO
CREATE PROCEDURE sp_IPD_BedDashboard
    @FloorId  INT = NULL,
    @WardId   INT = NULL
AS
BEGIN
    SELECT
        b.BedId, b.BedNumber, b.BedStatus,
        r.RoomId, r.RoomNumber, r.RoomChargePerDay,
        r.FloorId, r.WardId, r.CategoryId,
        f.FloorName, ISNULL(f.FloorNumber, 0) AS FloorNumber,
        w.WardName, w.WardType,
        c.CategoryName
    FROM tbl_IPD_BedMaster b
    JOIN tbl_IPD_RoomMaster r ON b.RoomId = r.RoomId
    LEFT JOIN tbl_IPD_FloorMaster f ON r.FloorId = f.FloorId
    LEFT JOIN tbl_IPD_WardMaster w ON r.WardId = w.WardId
    LEFT JOIN tbl_IPD_CategoryMaster c ON r.CategoryId = c.CategoryId
    WHERE b.IsActive = 1 AND r.IsActive = 1
      AND (@FloorId IS NULL OR r.FloorId = @FloorId)
      AND (@WardId IS NULL OR r.WardId = @WardId)
    ORDER BY ISNULL(f.FloorNumber, 0), w.WardName, r.RoomNumber, b.BedNumber
END
GO

-- =============================================
-- 17. TRANSFER HISTORY for a booking
-- =============================================
IF OBJECT_ID('sp_IPD_TransferHistory_GetByBooking', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_TransferHistory_GetByBooking
GO
CREATE PROCEDURE sp_IPD_TransferHistory_GetByBooking
    @BookingId INT
AS
BEGIN
    SELECT
        th.TransferId, th.TransferDate, th.Reason, th.Remarks,
        oldR.RoomNumber AS OldRoom, oldB.BedNumber AS OldBed,
        newR.RoomNumber AS NewRoom, newB.BedNumber AS NewBed
    FROM tbl_IPD_BedTransferHistory th
    JOIN tbl_IPD_StayBookingLocation oldLoc ON th.OldLocationId = oldLoc.LocationId
    JOIN tbl_IPD_StayBookingLocation newLoc ON th.NewLocationId = newLoc.LocationId
    JOIN tbl_IPD_RoomMaster oldR ON oldLoc.RoomId = oldR.RoomId
    JOIN tbl_IPD_BedMaster oldB ON oldLoc.BedId = oldB.BedId
    JOIN tbl_IPD_RoomMaster newR ON newLoc.RoomId = newR.RoomId
    JOIN tbl_IPD_BedMaster newB ON newLoc.BedId = newB.BedId
    WHERE th.BookingId = @BookingId
    ORDER BY th.TransferDate
END
GO

-- =============================================
-- 18. GET BOOKING DETAIL (for Discharge screen)
-- =============================================
IF OBJECT_ID('sp_IPD_StayBooking_GetById', 'P') IS NOT NULL DROP PROCEDURE sp_IPD_StayBooking_GetById
GO
CREATE PROCEDURE sp_IPD_StayBooking_GetById
    @BookingId INT
AS
BEGIN
    SELECT
        sb.BookingId, sb.BookingNo, sb.PatientFid, sb.DoctorId,
        sb.AdmissionDate, sb.ExpectedDischarge, sb.ActualDischarge,
        sb.AdmissionType, sb.Status, sb.Remarks,
        loc.LocationId, loc.RoomId, loc.BedId, loc.FloorId, loc.WardId, loc.CategoryId,
        loc.RoomChargeSnapshot, loc.EffectiveFrom,
        r.RoomNumber, b.BedNumber,
        f.FloorName, w.WardName, c.CategoryName
    FROM tbl_IPD_StayBooking sb
    LEFT JOIN tbl_IPD_StayBookingLocation loc ON sb.BookingId = loc.BookingId AND loc.EffectiveTo IS NULL
    LEFT JOIN tbl_IPD_RoomMaster r ON loc.RoomId = r.RoomId
    LEFT JOIN tbl_IPD_BedMaster b ON loc.BedId = b.BedId
    LEFT JOIN tbl_IPD_FloorMaster f ON loc.FloorId = f.FloorId
    LEFT JOIN tbl_IPD_WardMaster w ON loc.WardId = w.WardId
    LEFT JOIN tbl_IPD_CategoryMaster c ON loc.CategoryId = c.CategoryId
    WHERE sb.BookingId = @BookingId
END
GO

PRINT 'ALL IPD STORED PROCEDURES CREATED SUCCESSFULLY'
GO
