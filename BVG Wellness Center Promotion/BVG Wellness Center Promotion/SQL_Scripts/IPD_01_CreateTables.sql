-- ============================================================
-- IPD BED BOOKING MODULE - CREATE TABLES
-- BVG Wellness Center
-- Run this script ONCE in your database
-- ============================================================

-- 1. FLOOR MASTER
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbl_IPD_FloorMaster' AND xtype='U')
CREATE TABLE tbl_IPD_FloorMaster (
    FloorId       INT IDENTITY(1,1) PRIMARY KEY,
    FloorName     VARCHAR(100) NOT NULL,
    FloorNumber   INT NOT NULL DEFAULT 0,
    IsActive      BIT NOT NULL DEFAULT 1,
    CreatedBy     INT NULL,
    CreatedAt     DATETIME NOT NULL DEFAULT GETDATE()
)
GO

-- 2. WARD MASTER
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbl_IPD_WardMaster' AND xtype='U')
CREATE TABLE tbl_IPD_WardMaster (
    WardId        INT IDENTITY(1,1) PRIMARY KEY,
    WardName      VARCHAR(100) NOT NULL,
    WardType      VARCHAR(50) NOT NULL DEFAULT 'General',  -- General/ICU/NICU/Maternity/Paediatric
    IsActive      BIT NOT NULL DEFAULT 1,
    CreatedBy     INT NULL,
    CreatedAt     DATETIME NOT NULL DEFAULT GETDATE()
)
GO

-- 3. CATEGORY MASTER (label only, no charge here)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbl_IPD_CategoryMaster' AND xtype='U')
CREATE TABLE tbl_IPD_CategoryMaster (
    CategoryId    INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName  VARCHAR(100) NOT NULL,
    FoodOption    VARCHAR(20) NOT NULL DEFAULT 'None',   -- None/Optional/Mandatory
    IsActive      BIT NOT NULL DEFAULT 1,
    CreatedBy     INT NULL,
    CreatedAt     DATETIME NOT NULL DEFAULT GETDATE()
)
GO

-- 4. ROOM MASTER (actual charge lives here)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbl_IPD_RoomMaster' AND xtype='U')
CREATE TABLE tbl_IPD_RoomMaster (
    RoomId              INT IDENTITY(1,1) PRIMARY KEY,
    RoomNumber          VARCHAR(20) NOT NULL,
    FloorId             INT NULL REFERENCES tbl_IPD_FloorMaster(FloorId),
    WardId              INT NULL REFERENCES tbl_IPD_WardMaster(WardId),
    CategoryId          INT NULL REFERENCES tbl_IPD_CategoryMaster(CategoryId),
    RoomChargePerDay    DECIMAL(10,2) NOT NULL DEFAULT 0,
    IsActive            BIT NOT NULL DEFAULT 1,
    CreatedBy           INT NULL,
    CreatedAt           DATETIME NOT NULL DEFAULT GETDATE()
)
GO

-- 5. ROOM CHARGES MASTER (define charge types: Nursing, TV, AC, etc.)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbl_IPD_RoomChargesMaster' AND xtype='U')
CREATE TABLE tbl_IPD_RoomChargesMaster (
    ChargeId      INT IDENTITY(1,1) PRIMARY KEY,
    ChargeName    VARCHAR(200) NOT NULL,
    ChargeType    VARCHAR(30) NOT NULL DEFAULT 'ServiceCharge',  -- RoomCharge/FoodCharge/ServiceCharge
    DefaultAmount DECIMAL(10,2) NOT NULL DEFAULT 0,
    IsActive      BIT NOT NULL DEFAULT 1,
    CreatedBy     INT NULL,
    CreatedAt     DATETIME NOT NULL DEFAULT GETDATE()
)
GO

-- 6. ROOM CHARGES MAPPING (which charges apply to which room)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbl_IPD_RoomChargesMapping' AND xtype='U')
CREATE TABLE tbl_IPD_RoomChargesMapping (
    MappingId     INT IDENTITY(1,1) PRIMARY KEY,
    RoomId        INT NOT NULL REFERENCES tbl_IPD_RoomMaster(RoomId),
    ChargeId      INT NOT NULL REFERENCES tbl_IPD_RoomChargesMaster(ChargeId),
    ChargeAmount  DECIMAL(10,2) NOT NULL DEFAULT 0,  -- room-specific override
    IsCompulsory  BIT NOT NULL DEFAULT 0,            -- 1=locked, 0=optional
    IsActive      BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_RoomCharge UNIQUE (RoomId, ChargeId)
)
GO

-- 7. BED MASTER
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbl_IPD_BedMaster' AND xtype='U')
CREATE TABLE tbl_IPD_BedMaster (
    BedId         INT IDENTITY(1,1) PRIMARY KEY,
    RoomId        INT NOT NULL REFERENCES tbl_IPD_RoomMaster(RoomId),
    BedNumber     VARCHAR(20) NOT NULL,
    BedStatus     VARCHAR(20) NOT NULL DEFAULT 'Available',  -- Available/Occupied/Cleaning/Maintenance/Reserved
    IsActive      BIT NOT NULL DEFAULT 1,
    CreatedBy     INT NULL,
    CreatedAt     DATETIME NOT NULL DEFAULT GETDATE()
)
GO

-- 8. FOOD PLAN MASTER
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbl_IPD_FoodPlanMaster' AND xtype='U')
CREATE TABLE tbl_IPD_FoodPlanMaster (
    FoodPlanId    INT IDENTITY(1,1) PRIMARY KEY,
    PlanName      VARCHAR(100) NOT NULL,
    DailyCharge   DECIMAL(10,2) NOT NULL DEFAULT 0,
    IsActive      BIT NOT NULL DEFAULT 1,
    CreatedBy     INT NULL,
    CreatedAt     DATETIME NOT NULL DEFAULT GETDATE()
)
GO

-- 9. HOSPITAL SETTINGS (single config row)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbl_IPD_HospitalSetting' AND xtype='U')
BEGIN
    CREATE TABLE tbl_IPD_HospitalSetting (
        SettingId                   INT IDENTITY(1,1) PRIMARY KEY,
        UseFloor                    BIT NOT NULL DEFAULT 0,
        UseWard                     BIT NOT NULL DEFAULT 0,
        UseCategory                 BIT NOT NULL DEFAULT 1,
        FoodModuleEnabled           BIT NOT NULL DEFAULT 0,
        FoodMandatory               BIT NOT NULL DEFAULT 0,
        AdvanceRequired             BIT NOT NULL DEFAULT 0,
        BedTransferAllowed          BIT NOT NULL DEFAULT 1,
        PackageBillingEnabled       BIT NOT NULL DEFAULT 0,
        AttendantAllowed            BIT NOT NULL DEFAULT 0,
        AutoReleaseBedOnDischarge   BIT NOT NULL DEFAULT 1,
        GSTEnabled                  BIT NOT NULL DEFAULT 0,
        GSTRate                     DECIMAL(5,2) NOT NULL DEFAULT 5.00,
        UpdatedAt                   DATETIME NULL,
        UpdatedBy                   INT NULL
    )
    -- Insert default row
    INSERT INTO tbl_IPD_HospitalSetting DEFAULT VALUES
END
GO

-- 10. STAY BOOKING HEADER
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbl_IPD_StayBooking' AND xtype='U')
CREATE TABLE tbl_IPD_StayBooking (
    BookingId           INT IDENTITY(1,1) PRIMARY KEY,
    BookingNo           VARCHAR(20) NOT NULL,
    PatientFid          INT NOT NULL,                    -- FK to existing patient table
    DoctorId            INT NULL,                        -- FK to existing doctor table
    AdmissionDate       DATETIME NOT NULL,
    ExpectedDischarge   DATETIME NOT NULL,
    ActualDischarge     DATETIME NULL,
    AdmissionType       VARCHAR(20) NOT NULL DEFAULT 'Planned',  -- Emergency/Planned/DayCare
    Status              VARCHAR(20) NOT NULL DEFAULT 'Active',   -- Active/Discharged/Cancelled
    Remarks             VARCHAR(500) NULL,
    CreatedBy           INT NULL,
    CreatedAt           DATETIME NOT NULL DEFAULT GETDATE()
)
GO

-- 11. STAY BOOKING LOCATION (room/bed snapshot + transfer history)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbl_IPD_StayBookingLocation' AND xtype='U')
CREATE TABLE tbl_IPD_StayBookingLocation (
    LocationId              INT IDENTITY(1,1) PRIMARY KEY,
    BookingId               INT NOT NULL REFERENCES tbl_IPD_StayBooking(BookingId),
    FloorId                 INT NULL,
    WardId                  INT NULL,
    CategoryId              INT NULL,
    RoomId                  INT NOT NULL REFERENCES tbl_IPD_RoomMaster(RoomId),
    BedId                   INT NOT NULL REFERENCES tbl_IPD_BedMaster(BedId),
    RoomChargeSnapshot      DECIMAL(10,2) NOT NULL DEFAULT 0,  -- snapshot at time of booking
    EffectiveFrom           DATETIME NOT NULL,
    EffectiveTo             DATETIME NULL,   -- NULL = currently active
    CreatedAt               DATETIME NOT NULL DEFAULT GETDATE()
)
GO

-- 12. STAY BOOKING CHARGES (charge snapshot per booking period)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbl_IPD_StayBookingCharges' AND xtype='U')
CREATE TABLE tbl_IPD_StayBookingCharges (
    BookingChargeId     INT IDENTITY(1,1) PRIMARY KEY,
    BookingId           INT NOT NULL REFERENCES tbl_IPD_StayBooking(BookingId),
    LocationId          INT NOT NULL REFERENCES tbl_IPD_StayBookingLocation(LocationId),
    ChargeId            INT NOT NULL REFERENCES tbl_IPD_RoomChargesMaster(ChargeId),
    ChargeName          VARCHAR(200) NOT NULL,          -- snapshot
    ChargeType          VARCHAR(30) NOT NULL,
    IsCompulsory        BIT NOT NULL DEFAULT 0,
    AmountPerDay        DECIMAL(10,2) NOT NULL DEFAULT 0,
    IsSelected          BIT NOT NULL DEFAULT 0,         -- 1=included in bill
    EffectiveFrom       DATETIME NOT NULL,
    EffectiveTo         DATETIME NULL
)
GO

-- 13. STAY BOOKING FOOD
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbl_IPD_StayBookingFood' AND xtype='U')
CREATE TABLE tbl_IPD_StayBookingFood (
    FoodId          INT IDENTITY(1,1) PRIMARY KEY,
    BookingId       INT NOT NULL REFERENCES tbl_IPD_StayBooking(BookingId),
    FoodPlanId      INT NOT NULL REFERENCES tbl_IPD_FoodPlanMaster(FoodPlanId),
    PlanName        VARCHAR(100) NOT NULL,      -- snapshot
    DailyCharge     DECIMAL(10,2) NOT NULL DEFAULT 0,  -- snapshot
    StartDate       DATETIME NOT NULL,
    EndDate         DATETIME NULL,             -- NULL = currently active
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE()
)
GO

-- 14. STAY BOOKING ADVANCE
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbl_IPD_StayBookingAdvance' AND xtype='U')
CREATE TABLE tbl_IPD_StayBookingAdvance (
    AdvanceId       INT IDENTITY(1,1) PRIMARY KEY,
    BookingId       INT NOT NULL REFERENCES tbl_IPD_StayBooking(BookingId),
    Amount          DECIMAL(10,2) NOT NULL DEFAULT 0,
    PaymentMode     VARCHAR(50) NOT NULL DEFAULT 'Cash',  -- Cash/Card/UPI/Cheque
    PaidAt          DATETIME NOT NULL DEFAULT GETDATE(),
    Remarks         VARCHAR(200) NULL,
    CreatedBy       INT NULL
)
GO

-- 15. BED TRANSFER HISTORY
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbl_IPD_BedTransferHistory' AND xtype='U')
CREATE TABLE tbl_IPD_BedTransferHistory (
    TransferId          INT IDENTITY(1,1) PRIMARY KEY,
    BookingId           INT NOT NULL REFERENCES tbl_IPD_StayBooking(BookingId),
    OldLocationId       INT NOT NULL REFERENCES tbl_IPD_StayBookingLocation(LocationId),
    NewLocationId       INT NOT NULL REFERENCES tbl_IPD_StayBookingLocation(LocationId),
    TransferDate        DATETIME NOT NULL,
    Reason              VARCHAR(200) NULL,
    Remarks             VARCHAR(500) NULL,
    TransferredBy       INT NULL,
    CreatedAt           DATETIME NOT NULL DEFAULT GETDATE()
)
GO

-- 16. IPD FINAL BILLING (FROZEN SNAPSHOT)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbl_IPD_Billing' AND xtype='U')
CREATE TABLE tbl_IPD_Billing (
    BillId              INT IDENTITY(1,1) PRIMARY KEY,
    BillNo              VARCHAR(30) NOT NULL,
    BookingId           INT NOT NULL REFERENCES tbl_IPD_StayBooking(BookingId),
    PatientFid          INT NOT NULL,
    RoomTotal           DECIMAL(10,2) NOT NULL DEFAULT 0,
    CompulsoryTotal     DECIMAL(10,2) NOT NULL DEFAULT 0,
    OptionalTotal       DECIMAL(10,2) NOT NULL DEFAULT 0,
    FoodTotal           DECIMAL(10,2) NOT NULL DEFAULT 0,
    Subtotal            DECIMAL(10,2) NOT NULL DEFAULT 0,
    GSTAmount           DECIMAL(10,2) NOT NULL DEFAULT 0,
    GrossTotal          DECIMAL(10,2) NOT NULL DEFAULT 0,
    AdvancePaid         DECIMAL(10,2) NOT NULL DEFAULT 0,
    NetPayable          DECIMAL(10,2) NOT NULL DEFAULT 0,
    BillDate            DATETIME NOT NULL DEFAULT GETDATE(),
    GeneratedBy         INT NULL
)
GO

PRINT 'ALL IPD TABLES CREATED SUCCESSFULLY'
GO
