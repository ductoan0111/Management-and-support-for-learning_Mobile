
USE master;
GO

IF DB_ID(N'QuanLyHoTroHocTap') IS NULL
BEGIN
    CREATE DATABASE QuanLyHoTroHocTap;
END
GO

USE QuanLyHoTroHocTap;
GO

/*==============================================================================
  1. ROLE / USER
==============================================================================*/

CREATE TABLE dbo.Roles
(
    RoleId          TINYINT IDENTITY(1,1) NOT NULL,
    RoleCode        VARCHAR(20) NOT NULL,
    RoleName        NVARCHAR(50) NOT NULL,
    Description     NVARCHAR(255) NULL,

    CONSTRAINT PK_Roles PRIMARY KEY (RoleId),
    CONSTRAINT UQ_Roles_RoleCode UNIQUE (RoleCode)
);
GO

CREATE TABLE dbo.Users
(
    UserId          BIGINT IDENTITY(1,1) NOT NULL,
    RoleId          TINYINT NOT NULL,
    Username        VARCHAR(100) NOT NULL,
    Email           VARCHAR(150) NOT NULL,
    PasswordHash    NVARCHAR(500) NOT NULL,
    FullName        NVARCHAR(150) NOT NULL,
    Phone           VARCHAR(20) NULL,
    DateOfBirth     DATE NULL,
    Gender          TINYINT NULL,        -- 0: Khác, 1: Nam, 2: Nữ
    AvatarUrl       NVARCHAR(1000) NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSDATETIME()),
    UpdatedAt       DATETIME2(0) NULL,
    LastLoginAt     DATETIME2(0) NULL,

    CONSTRAINT PK_Users PRIMARY KEY (UserId),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId),
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT CK_Users_Gender CHECK (Gender IS NULL OR Gender IN (0,1,2))
);
GO

/*==============================================================================
  2. KHOA / NGÀNH / LỚP HÀNH CHÍNH
==============================================================================*/

CREATE TABLE dbo.Departments
(
    DepartmentId    INT IDENTITY(1,1) NOT NULL,
    DepartmentCode  VARCHAR(20) NOT NULL,
    DepartmentName  NVARCHAR(150) NOT NULL,
    Description     NVARCHAR(500) NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_Departments_IsActive DEFAULT (1),
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Departments_CreatedAt DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Departments PRIMARY KEY (DepartmentId),
    CONSTRAINT UQ_Departments_Code UNIQUE (DepartmentCode)
);
GO

CREATE TABLE dbo.Majors
(
    MajorId         INT IDENTITY(1,1) NOT NULL,
    DepartmentId    INT NOT NULL,
    MajorCode       VARCHAR(20) NOT NULL,
    MajorName       NVARCHAR(150) NOT NULL,
    Description     NVARCHAR(500) NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_Majors_IsActive DEFAULT (1),

    CONSTRAINT PK_Majors PRIMARY KEY (MajorId),
    CONSTRAINT FK_Majors_Departments
        FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(DepartmentId),
    CONSTRAINT UQ_Majors_Code UNIQUE (MajorCode)
);
GO

CREATE TABLE dbo.AcademicClasses
(
    AcademicClassId INT IDENTITY(1,1) NOT NULL,
    MajorId         INT NOT NULL,
    ClassCode       VARCHAR(30) NOT NULL,
    ClassName       NVARCHAR(150) NOT NULL,
    IntakeYear      SMALLINT NOT NULL,
    GraduationYear  SMALLINT NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_AcademicClasses_IsActive DEFAULT (1),

    CONSTRAINT PK_AcademicClasses PRIMARY KEY (AcademicClassId),
    CONSTRAINT FK_AcademicClasses_Majors
        FOREIGN KEY (MajorId) REFERENCES dbo.Majors(MajorId),
    CONSTRAINT UQ_AcademicClasses_Code UNIQUE (ClassCode),
    CONSTRAINT CK_AcademicClasses_Years
        CHECK (GraduationYear IS NULL OR GraduationYear >= IntakeYear)
);
GO

/*==============================================================================
  3. PROFILE SINH VIÊN / GIẢNG VIÊN
==============================================================================*/

CREATE TABLE dbo.Students
(
    StudentId       BIGINT IDENTITY(1,1) NOT NULL,
    UserId          BIGINT NOT NULL,
    StudentCode     VARCHAR(30) NOT NULL,
    AcademicClassId INT NULL,
    MajorId         INT NOT NULL,
    EnrollmentYear  SMALLINT NOT NULL,
    Status          TINYINT NOT NULL CONSTRAINT DF_Students_Status DEFAULT (1),
    -- 0: Bảo lưu/nghỉ, 1: Đang học, 2: Tốt nghiệp

    CONSTRAINT PK_Students PRIMARY KEY (StudentId),
    CONSTRAINT FK_Students_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Students_AcademicClasses
        FOREIGN KEY (AcademicClassId) REFERENCES dbo.AcademicClasses(AcademicClassId),
    CONSTRAINT FK_Students_Majors
        FOREIGN KEY (MajorId) REFERENCES dbo.Majors(MajorId),
    CONSTRAINT UQ_Students_User UNIQUE (UserId),
    CONSTRAINT UQ_Students_Code UNIQUE (StudentCode),
    CONSTRAINT CK_Students_Status CHECK (Status IN (0,1,2))
);
GO

CREATE TABLE dbo.Teachers
(
    TeacherId       BIGINT IDENTITY(1,1) NOT NULL,
    UserId          BIGINT NOT NULL,
    TeacherCode     VARCHAR(30) NOT NULL,
    DepartmentId    INT NOT NULL,
    AcademicTitle   NVARCHAR(100) NULL,
    Specialization  NVARCHAR(255) NULL,
    Status          TINYINT NOT NULL CONSTRAINT DF_Teachers_Status DEFAULT (1),
    -- 0: Nghỉ/ngưng công tác, 1: Đang công tác

    CONSTRAINT PK_Teachers PRIMARY KEY (TeacherId),
    CONSTRAINT FK_Teachers_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Teachers_Departments
        FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(DepartmentId),
    CONSTRAINT UQ_Teachers_User UNIQUE (UserId),
    CONSTRAINT UQ_Teachers_Code UNIQUE (TeacherCode),
    CONSTRAINT CK_Teachers_Status CHECK (Status IN (0,1))
);
GO

/*==============================================================================
  4. HỌC KỲ / MÔN HỌC
==============================================================================*/

CREATE TABLE dbo.Semesters
(
    SemesterId      INT IDENTITY(1,1) NOT NULL,
    SemesterCode    VARCHAR(30) NOT NULL,
    SemesterName    NVARCHAR(100) NOT NULL,
    AcademicYear    VARCHAR(20) NOT NULL,   -- Ví dụ: 2026-2027
    StartDate       DATE NOT NULL,
    EndDate         DATE NOT NULL,
    IsCurrent       BIT NOT NULL CONSTRAINT DF_Semesters_IsCurrent DEFAULT (0),

    CONSTRAINT PK_Semesters PRIMARY KEY (SemesterId),
    CONSTRAINT UQ_Semesters_Code UNIQUE (SemesterCode),
    CONSTRAINT CK_Semesters_Date CHECK (EndDate >= StartDate)
);
GO

CREATE TABLE dbo.Courses
(
    CourseId        INT IDENTITY(1,1) NOT NULL,
    DepartmentId    INT NOT NULL,
    CourseCode      VARCHAR(30) NOT NULL,
    CourseName      NVARCHAR(200) NOT NULL,
    Credits         TINYINT NOT NULL,
    Description     NVARCHAR(MAX) NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_Courses_IsActive DEFAULT (1),
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Courses_CreatedAt DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Courses PRIMARY KEY (CourseId),
    CONSTRAINT FK_Courses_Departments
        FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(DepartmentId),
    CONSTRAINT UQ_Courses_Code UNIQUE (CourseCode),
    CONSTRAINT CK_Courses_Credits CHECK (Credits BETWEEN 1 AND 15)
);
GO

/*==============================================================================
  5. LỚP HỌC PHẦN / PHÂN CÔNG GIẢNG VIÊN / SINH VIÊN
==============================================================================*/

CREATE TABLE dbo.CourseSections
(
    SectionId       BIGINT IDENTITY(1,1) NOT NULL,
    CourseId        INT NOT NULL,
    SemesterId      INT NOT NULL,
    SectionCode     VARCHAR(40) NOT NULL,
    SectionName     NVARCHAR(200) NULL,
    MaxStudents     INT NULL,
    Status          TINYINT NOT NULL CONSTRAINT DF_CourseSections_Status DEFAULT (1),
    -- 0: Đóng, 1: Đang mở, 2: Đã kết thúc
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_CourseSections_CreatedAt DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_CourseSections PRIMARY KEY (SectionId),
    CONSTRAINT FK_CourseSections_Courses
        FOREIGN KEY (CourseId) REFERENCES dbo.Courses(CourseId),
    CONSTRAINT FK_CourseSections_Semesters
        FOREIGN KEY (SemesterId) REFERENCES dbo.Semesters(SemesterId),
    CONSTRAINT UQ_CourseSections_Code UNIQUE (SectionCode),
    CONSTRAINT CK_CourseSections_MaxStudents
        CHECK (MaxStudents IS NULL OR MaxStudents > 0),
    CONSTRAINT CK_CourseSections_Status CHECK (Status IN (0,1,2))
);
GO

-- Cho phép 1 lớp học phần có thể có 1 hoặc nhiều giảng viên.
CREATE TABLE dbo.SectionTeachers
(
    SectionId       BIGINT NOT NULL,
    TeacherId       BIGINT NOT NULL,
    IsPrimary       BIT NOT NULL CONSTRAINT DF_SectionTeachers_IsPrimary DEFAULT (0),
    AssignedAt      DATETIME2(0) NOT NULL CONSTRAINT DF_SectionTeachers_AssignedAt DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_SectionTeachers PRIMARY KEY (SectionId, TeacherId),
    CONSTRAINT FK_SectionTeachers_Sections
        FOREIGN KEY (SectionId) REFERENCES dbo.CourseSections(SectionId),
    CONSTRAINT FK_SectionTeachers_Teachers
        FOREIGN KEY (TeacherId) REFERENCES dbo.Teachers(TeacherId)
);
GO

CREATE TABLE dbo.Enrollments
(
    EnrollmentId    BIGINT IDENTITY(1,1) NOT NULL,
    SectionId       BIGINT NOT NULL,
    StudentId       BIGINT NOT NULL,
    EnrolledAt      DATETIME2(0) NOT NULL CONSTRAINT DF_Enrollments_EnrolledAt DEFAULT (SYSDATETIME()),
    Status          TINYINT NOT NULL CONSTRAINT DF_Enrollments_Status DEFAULT (1),
    -- 0: Hủy, 1: Đang học, 2: Hoàn thành
    FinalScore10    DECIMAL(5,2) NULL,
    LetterGrade     VARCHAR(5) NULL,

    CONSTRAINT PK_Enrollments PRIMARY KEY (EnrollmentId),
    CONSTRAINT FK_Enrollments_Sections
        FOREIGN KEY (SectionId) REFERENCES dbo.CourseSections(SectionId),
    CONSTRAINT FK_Enrollments_Students
        FOREIGN KEY (StudentId) REFERENCES dbo.Students(StudentId),
    CONSTRAINT UQ_Enrollments_Section_Student UNIQUE (SectionId, StudentId),
    CONSTRAINT CK_Enrollments_Status CHECK (Status IN (0,1,2)),
    CONSTRAINT CK_Enrollments_FinalScore
        CHECK (FinalScore10 IS NULL OR FinalScore10 BETWEEN 0 AND 10)
);
GO

/*==============================================================================
  6. THỜI KHÓA BIỂU
==============================================================================*/

CREATE TABLE dbo.ClassSchedules
(
    ScheduleId      BIGINT IDENTITY(1,1) NOT NULL,
    SectionId       BIGINT NOT NULL,
    DayOfWeek       TINYINT NOT NULL,        -- 2: Thứ 2 ... 8: Chủ nhật
    StartTime       TIME(0) NOT NULL,
    EndTime         TIME(0) NOT NULL,
    Room            NVARCHAR(50) NULL,
    Building        NVARCHAR(100) NULL,
    EffectiveFrom   DATE NOT NULL,
    EffectiveTo     DATE NOT NULL,
    Note            NVARCHAR(500) NULL,

    CONSTRAINT PK_ClassSchedules PRIMARY KEY (ScheduleId),
    CONSTRAINT FK_ClassSchedules_Sections
        FOREIGN KEY (SectionId) REFERENCES dbo.CourseSections(SectionId),
    CONSTRAINT CK_ClassSchedules_Day CHECK (DayOfWeek BETWEEN 2 AND 8),
    CONSTRAINT CK_ClassSchedules_Time CHECK (EndTime > StartTime),
    CONSTRAINT CK_ClassSchedules_Date CHECK (EffectiveTo >= EffectiveFrom)
);
GO

/*==============================================================================
  7. LỊCH KIỂM TRA / LỊCH THI
==============================================================================*/

CREATE TABLE dbo.Exams
(
    ExamId           BIGINT IDENTITY(1,1) NOT NULL,
    SectionId        BIGINT NOT NULL,
    CreatedByUserId  BIGINT NOT NULL,
    ExamName         NVARCHAR(200) NOT NULL,
    ExamType         TINYINT NOT NULL,
    -- 1: Kiểm tra, 2: Giữa kỳ, 3: Cuối kỳ, 4: Khác
    ExamDate         DATE NOT NULL,
    StartTime        TIME(0) NOT NULL,
    DurationMinutes  SMALLINT NOT NULL,
    Room             NVARCHAR(50) NULL,
    Note             NVARCHAR(500) NULL,
    CreatedAt        DATETIME2(0) NOT NULL CONSTRAINT DF_Exams_CreatedAt DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Exams PRIMARY KEY (ExamId),
    CONSTRAINT FK_Exams_Sections
        FOREIGN KEY (SectionId) REFERENCES dbo.CourseSections(SectionId),
    CONSTRAINT FK_Exams_CreatedBy
        FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT CK_Exams_Type CHECK (ExamType IN (1,2,3,4)),
    CONSTRAINT CK_Exams_Duration CHECK (DurationMinutes > 0)
);
GO

/*==============================================================================
  8. BÀI TẬP / NỘP BÀI / CHẤM BÀI
==============================================================================*/

CREATE TABLE dbo.Assignments
(
    AssignmentId        BIGINT IDENTITY(1,1) NOT NULL,
    SectionId           BIGINT NOT NULL,
    CreatedByUserId     BIGINT NOT NULL,
    Title               NVARCHAR(250) NOT NULL,
    Description         NVARCHAR(MAX) NULL,
    AttachmentUrl       NVARCHAR(1000) NULL,
    OpenAt              DATETIME2(0) NULL,
    DueAt               DATETIME2(0) NOT NULL,
    MaxScore            DECIMAL(5,2) NOT NULL CONSTRAINT DF_Assignments_MaxScore DEFAULT (10),
    AllowLateSubmission BIT NOT NULL CONSTRAINT DF_Assignments_AllowLate DEFAULT (0),
    IsPublished         BIT NOT NULL CONSTRAINT DF_Assignments_IsPublished DEFAULT (1),
    CreatedAt           DATETIME2(0) NOT NULL CONSTRAINT DF_Assignments_CreatedAt DEFAULT (SYSDATETIME()),
    UpdatedAt           DATETIME2(0) NULL,

    CONSTRAINT PK_Assignments PRIMARY KEY (AssignmentId),
    CONSTRAINT FK_Assignments_Sections
        FOREIGN KEY (SectionId) REFERENCES dbo.CourseSections(SectionId),
    CONSTRAINT FK_Assignments_CreatedBy
        FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT CK_Assignments_OpenDue
        CHECK (OpenAt IS NULL OR DueAt > OpenAt),
    CONSTRAINT CK_Assignments_MaxScore CHECK (MaxScore > 0)
);
GO

CREATE TABLE dbo.AssignmentSubmissions
(
    SubmissionId       BIGINT IDENTITY(1,1) NOT NULL,
    AssignmentId       BIGINT NOT NULL,
    StudentId          BIGINT NOT NULL,
    TextContent        NVARCHAR(MAX) NULL,
    FileUrl            NVARCHAR(1000) NULL,
    SubmittedAt        DATETIME2(0) NOT NULL CONSTRAINT DF_Submissions_SubmittedAt DEFAULT (SYSDATETIME()),
    IsLate             BIT NOT NULL CONSTRAINT DF_Submissions_IsLate DEFAULT (0),
    Status             TINYINT NOT NULL CONSTRAINT DF_Submissions_Status DEFAULT (1),
    -- 1: Đã nộp, 2: Đã chấm, 3: Yêu cầu nộp lại
    Score              DECIMAL(5,2) NULL,
    Feedback           NVARCHAR(MAX) NULL,
    GradedByUserId     BIGINT NULL,
    GradedAt           DATETIME2(0) NULL,

    CONSTRAINT PK_AssignmentSubmissions PRIMARY KEY (SubmissionId),
    CONSTRAINT FK_Submissions_Assignments
        FOREIGN KEY (AssignmentId) REFERENCES dbo.Assignments(AssignmentId),
    CONSTRAINT FK_Submissions_Students
        FOREIGN KEY (StudentId) REFERENCES dbo.Students(StudentId),
    CONSTRAINT FK_Submissions_GradedBy
        FOREIGN KEY (GradedByUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT UQ_Submissions_Assignment_Student UNIQUE (AssignmentId, StudentId),
    CONSTRAINT CK_Submissions_Status CHECK (Status IN (1,2,3)),
    CONSTRAINT CK_Submissions_Score CHECK (Score IS NULL OR Score >= 0),
    CONSTRAINT CK_Submissions_Content CHECK (TextContent IS NOT NULL OR FileUrl IS NOT NULL)
);
GO

/*==============================================================================
  9. TÀI LIỆU HỌC TẬP
==============================================================================*/

CREATE TABLE dbo.Materials
(
    MaterialId       BIGINT IDENTITY(1,1) NOT NULL,
    SectionId        BIGINT NOT NULL,
    UploadedByUserId BIGINT NOT NULL,
    Title            NVARCHAR(250) NOT NULL,
    Description      NVARCHAR(MAX) NULL,
    MaterialType     VARCHAR(30) NULL,      -- PDF, DOCX, PPTX, LINK...
    FileUrl          NVARCHAR(1000) NULL,
    ExternalUrl      NVARCHAR(1000) NULL,
    IsVisible        BIT NOT NULL CONSTRAINT DF_Materials_IsVisible DEFAULT (1),
    CreatedAt        DATETIME2(0) NOT NULL CONSTRAINT DF_Materials_CreatedAt DEFAULT (SYSDATETIME()),
    UpdatedAt        DATETIME2(0) NULL,

    CONSTRAINT PK_Materials PRIMARY KEY (MaterialId),
    CONSTRAINT FK_Materials_Sections
        FOREIGN KEY (SectionId) REFERENCES dbo.CourseSections(SectionId),
    CONSTRAINT FK_Materials_UploadedBy
        FOREIGN KEY (UploadedByUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT CK_Materials_Source CHECK (FileUrl IS NOT NULL OR ExternalUrl IS NOT NULL)
);
GO

/*==============================================================================
  10. CẤU TRÚC ĐIỂM / ĐIỂM SINH VIÊN
==============================================================================*/

CREATE TABLE dbo.GradeComponents
(
    GradeComponentId BIGINT IDENTITY(1,1) NOT NULL,
    SectionId        BIGINT NOT NULL,
    ComponentName    NVARCHAR(100) NOT NULL,
    WeightPercent    DECIMAL(5,2) NOT NULL,
    MaxScore         DECIMAL(5,2) NOT NULL CONSTRAINT DF_GradeComponents_MaxScore DEFAULT (10),
    DisplayOrder     INT NOT NULL CONSTRAINT DF_GradeComponents_DisplayOrder DEFAULT (1),

    CONSTRAINT PK_GradeComponents PRIMARY KEY (GradeComponentId),
    CONSTRAINT FK_GradeComponents_Sections
        FOREIGN KEY (SectionId) REFERENCES dbo.CourseSections(SectionId),
    CONSTRAINT UQ_GradeComponents_Section_Name UNIQUE (SectionId, ComponentName),
    CONSTRAINT CK_GradeComponents_Weight CHECK (WeightPercent > 0 AND WeightPercent <= 100),
    CONSTRAINT CK_GradeComponents_MaxScore CHECK (MaxScore > 0)
);
GO

CREATE TABLE dbo.StudentGrades
(
    StudentGradeId    BIGINT IDENTITY(1,1) NOT NULL,
    GradeComponentId  BIGINT NOT NULL,
    StudentId         BIGINT NOT NULL,
    Score             DECIMAL(5,2) NULL,
    Note              NVARCHAR(500) NULL,
    GradedByUserId    BIGINT NOT NULL,
    GradedAt          DATETIME2(0) NOT NULL CONSTRAINT DF_StudentGrades_GradedAt DEFAULT (SYSDATETIME()),
    UpdatedAt         DATETIME2(0) NULL,

    CONSTRAINT PK_StudentGrades PRIMARY KEY (StudentGradeId),
    CONSTRAINT FK_StudentGrades_Components
        FOREIGN KEY (GradeComponentId) REFERENCES dbo.GradeComponents(GradeComponentId),
    CONSTRAINT FK_StudentGrades_Students
        FOREIGN KEY (StudentId) REFERENCES dbo.Students(StudentId),
    CONSTRAINT FK_StudentGrades_GradedBy
        FOREIGN KEY (GradedByUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT UQ_StudentGrades_Component_Student UNIQUE (GradeComponentId, StudentId),
    CONSTRAINT CK_StudentGrades_Score CHECK (Score IS NULL OR Score >= 0)
);
GO

/*==============================================================================
  11. THÔNG BÁO
==============================================================================*/

CREATE TABLE dbo.Announcements
(
    AnnouncementId  BIGINT IDENTITY(1,1) NOT NULL,
    CreatedByUserId BIGINT NOT NULL,
    SectionId       BIGINT NULL,     -- NULL = thông báo toàn hệ thống
    Title           NVARCHAR(250) NOT NULL,
    Content         NVARCHAR(MAX) NOT NULL,
    AnnouncementType TINYINT NOT NULL CONSTRAINT DF_Announcements_Type DEFAULT (1),
    -- 1: Hệ thống, 2: Lớp học, 3: Bài tập, 4: Lịch thi, 5: Điểm
    PublishedAt     DATETIME2(0) NOT NULL CONSTRAINT DF_Announcements_PublishedAt DEFAULT (SYSDATETIME()),
    ExpiresAt       DATETIME2(0) NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_Announcements_IsActive DEFAULT (1),

    CONSTRAINT PK_Announcements PRIMARY KEY (AnnouncementId),
    CONSTRAINT FK_Announcements_CreatedBy
        FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Announcements_Sections
        FOREIGN KEY (SectionId) REFERENCES dbo.CourseSections(SectionId),
    CONSTRAINT CK_Announcements_Type CHECK (AnnouncementType IN (1,2,3,4,5)),
    CONSTRAINT CK_Announcements_Expires CHECK (ExpiresAt IS NULL OR ExpiresAt >= PublishedAt)
);
GO

-- Lưu trạng thái đã đọc theo từng người dùng.
CREATE TABLE dbo.AnnouncementReads
(
    AnnouncementId BIGINT NOT NULL,
    UserId          BIGINT NOT NULL,
    IsRead          BIT NOT NULL CONSTRAINT DF_AnnouncementReads_IsRead DEFAULT (0),
    ReadAt          DATETIME2(0) NULL,

    CONSTRAINT PK_AnnouncementReads PRIMARY KEY (AnnouncementId, UserId),
    CONSTRAINT FK_AnnouncementReads_Announcements
        FOREIGN KEY (AnnouncementId) REFERENCES dbo.Announcements(AnnouncementId),
    CONSTRAINT FK_AnnouncementReads_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
);
GO

/*==============================================================================
  12. MỤC TIÊU HỌC TẬP CÁ NHÂN
==============================================================================*/

CREATE TABLE dbo.StudyGoals
(
    GoalId          BIGINT IDENTITY(1,1) NOT NULL,
    StudentId       BIGINT NOT NULL,
    Title           NVARCHAR(250) NOT NULL,
    Description     NVARCHAR(MAX) NULL,
    GoalType        TINYINT NOT NULL,
    -- 1: GPA, 2: Điểm môn, 3: Số giờ học, 4: Hoàn thành bài tập, 5: Khác
    TargetValue     DECIMAL(10,2) NULL,
    CurrentValue    DECIMAL(10,2) NULL,
    StartDate       DATE NOT NULL,
    EndDate         DATE NULL,
    Status          TINYINT NOT NULL CONSTRAINT DF_StudyGoals_Status DEFAULT (1),
    -- 1: Đang thực hiện, 2: Hoàn thành, 3: Hủy
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_StudyGoals_CreatedAt DEFAULT (SYSDATETIME()),
    UpdatedAt       DATETIME2(0) NULL,

    CONSTRAINT PK_StudyGoals PRIMARY KEY (GoalId),
    CONSTRAINT FK_StudyGoals_Students
        FOREIGN KEY (StudentId) REFERENCES dbo.Students(StudentId),
    CONSTRAINT CK_StudyGoals_Type CHECK (GoalType IN (1,2,3,4,5)),
    CONSTRAINT CK_StudyGoals_Status CHECK (Status IN (1,2,3)),
    CONSTRAINT CK_StudyGoals_Date CHECK (EndDate IS NULL OR EndDate >= StartDate)
);
GO

/*==============================================================================
  13. KẾ HOẠCH / TO-DO HỌC TẬP
==============================================================================*/

CREATE TABLE dbo.StudyTasks
(
    StudyTaskId     BIGINT IDENTITY(1,1) NOT NULL,
    StudentId       BIGINT NOT NULL,
    CourseId        INT NULL,
    Title           NVARCHAR(250) NOT NULL,
    Description     NVARCHAR(MAX) NULL,
    StartAt         DATETIME2(0) NULL,
    DueAt           DATETIME2(0) NULL,
    ReminderAt      DATETIME2(0) NULL,
    Priority        TINYINT NOT NULL CONSTRAINT DF_StudyTasks_Priority DEFAULT (2),
    -- 1: Thấp, 2: Trung bình, 3: Cao
    Status          TINYINT NOT NULL CONSTRAINT DF_StudyTasks_Status DEFAULT (1),
    -- 1: Chưa làm, 2: Đang làm, 3: Hoàn thành, 4: Hủy
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_StudyTasks_CreatedAt DEFAULT (SYSDATETIME()),
    UpdatedAt       DATETIME2(0) NULL,
    CompletedAt     DATETIME2(0) NULL,

    CONSTRAINT PK_StudyTasks PRIMARY KEY (StudyTaskId),
    CONSTRAINT FK_StudyTasks_Students
        FOREIGN KEY (StudentId) REFERENCES dbo.Students(StudentId),
    CONSTRAINT FK_StudyTasks_Courses
        FOREIGN KEY (CourseId) REFERENCES dbo.Courses(CourseId),
    CONSTRAINT CK_StudyTasks_Priority CHECK (Priority IN (1,2,3)),
    CONSTRAINT CK_StudyTasks_Status CHECK (Status IN (1,2,3,4)),
    CONSTRAINT CK_StudyTasks_Time CHECK (DueAt IS NULL OR StartAt IS NULL OR DueAt >= StartAt)
);
GO

/*==============================================================================
  14. EXPO PUSH TOKEN
==============================================================================*/

CREATE TABLE dbo.UserDevices
(
    UserDeviceId    BIGINT IDENTITY(1,1) NOT NULL,
    UserId          BIGINT NOT NULL,
    ExpoPushToken   NVARCHAR(500) NOT NULL,
    DeviceName      NVARCHAR(150) NULL,
    Platform        VARCHAR(20) NULL,     -- android / ios
    IsActive        BIT NOT NULL CONSTRAINT DF_UserDevices_IsActive DEFAULT (1),
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_UserDevices_CreatedAt DEFAULT (SYSDATETIME()),
    LastUsedAt      DATETIME2(0) NULL,

    CONSTRAINT PK_UserDevices PRIMARY KEY (UserDeviceId),
    CONSTRAINT FK_UserDevices_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT UQ_UserDevices_ExpoPushToken UNIQUE (ExpoPushToken)
);
GO

/*==============================================================================
  15. REFRESH TOKEN
==============================================================================*/

CREATE TABLE dbo.RefreshTokens
(
    RefreshTokenId  BIGINT IDENTITY(1,1) NOT NULL,
    UserId          BIGINT NOT NULL,
    TokenHash       NVARCHAR(500) NOT NULL,
    ExpiresAt       DATETIME2(0) NOT NULL,
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_RefreshTokens_CreatedAt DEFAULT (SYSDATETIME()),
    RevokedAt       DATETIME2(0) NULL,

    CONSTRAINT PK_RefreshTokens PRIMARY KEY (RefreshTokenId),
    CONSTRAINT FK_RefreshTokens_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
);
GO

/*==============================================================================
  16. NHẬT KÝ HỆ THỐNG
==============================================================================*/

CREATE TABLE dbo.AuditLogs
(
    AuditLogId      BIGINT IDENTITY(1,1) NOT NULL,
    UserId          BIGINT NULL,
    Action          NVARCHAR(150) NOT NULL,
    EntityName      NVARCHAR(100) NULL,
    EntityId        NVARCHAR(100) NULL,
    Description     NVARCHAR(MAX) NULL,
    IpAddress       VARCHAR(50) NULL,
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_AuditLogs PRIMARY KEY (AuditLogId),
    CONSTRAINT FK_AuditLogs_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
);
GO

/*==============================================================================
  17. INDEX
==============================================================================*/

CREATE INDEX IX_Users_RoleId
    ON dbo.Users(RoleId);
GO

CREATE INDEX IX_Students_ClassId
    ON dbo.Students(AcademicClassId);
GO

CREATE INDEX IX_Students_MajorId
    ON dbo.Students(MajorId);
GO

CREATE INDEX IX_SectionTeachers_TeacherId
    ON dbo.SectionTeachers(TeacherId);
GO

CREATE INDEX IX_CourseSections_Course_Semester
    ON dbo.CourseSections(CourseId, SemesterId);
GO

CREATE INDEX IX_Enrollments_StudentId
    ON dbo.Enrollments(StudentId);
GO

CREATE INDEX IX_ClassSchedules_Section_Day
    ON dbo.ClassSchedules(SectionId, DayOfWeek);
GO

CREATE INDEX IX_Exams_Section_Date
    ON dbo.Exams(SectionId, ExamDate);
GO

CREATE INDEX IX_Assignments_Section_DueAt
    ON dbo.Assignments(SectionId, DueAt);
GO

CREATE INDEX IX_Submissions_StudentId
    ON dbo.AssignmentSubmissions(StudentId);
GO

CREATE INDEX IX_Materials_SectionId
    ON dbo.Materials(SectionId);
GO

CREATE INDEX IX_StudentGrades_StudentId
    ON dbo.StudentGrades(StudentId);
GO

CREATE INDEX IX_Announcements_Section_PublishedAt
    ON dbo.Announcements(SectionId, PublishedAt DESC);
GO

CREATE INDEX IX_AnnouncementReads_User_IsRead
    ON dbo.AnnouncementReads(UserId, IsRead);
GO

CREATE INDEX IX_StudyTasks_Student_Status_Due
    ON dbo.StudyTasks(StudentId, Status, DueAt);
GO

CREATE INDEX IX_AuditLogs_User_CreatedAt
    ON dbo.AuditLogs(UserId, CreatedAt DESC);
GO

/*==============================================================================
  18. ROLE MẶC ĐỊNH
==============================================================================*/

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleCode = 'STUDENT')
BEGIN
    INSERT INTO dbo.Roles(RoleCode, RoleName, Description)
    VALUES ('STUDENT', N'Sinh viên', N'Người học sử dụng ứng dụng để quản lý và hỗ trợ học tập');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleCode = 'TEACHER')
BEGIN
    INSERT INTO dbo.Roles(RoleCode, RoleName, Description)
    VALUES ('TEACHER', N'Giảng viên', N'Quản lý lớp học phần, bài tập, tài liệu và điểm');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleCode = 'ADMIN')
BEGIN
    INSERT INTO dbo.Roles(RoleCode, RoleName, Description)
    VALUES ('ADMIN', N'Quản trị viên', N'Quản trị tài khoản và dữ liệu toàn hệ thống');
END;
GO

/*==============================================================================
  19. FUNCTION QUY ĐỔI ĐIỂM HỆ 10 -> HỆ 4
  Có thể chỉnh lại thang điểm theo quy định của trường.
==============================================================================*/

CREATE OR ALTER FUNCTION dbo.fn_Score10ToGPA4
(
    @Score10 DECIMAL(5,2)
)
RETURNS DECIMAL(3,2)
AS
BEGIN
    RETURN
    (
        CASE
            WHEN @Score10 IS NULL THEN NULL
            WHEN @Score10 >= 8.5 THEN 4.00
            WHEN @Score10 >= 8.0 THEN 3.50
            WHEN @Score10 >= 7.0 THEN 3.00
            WHEN @Score10 >= 6.5 THEN 2.50
            WHEN @Score10 >= 5.5 THEN 2.00
            WHEN @Score10 >= 5.0 THEN 1.50
            WHEN @Score10 >= 4.0 THEN 1.00
            ELSE 0.00
        END
    );
END;
GO

/*==============================================================================
  20. VIEW - THÔNG TIN LỚP HỌC PHẦN CỦA SINH VIÊN
==============================================================================*/

CREATE OR ALTER VIEW dbo.vw_StudentSections
AS
SELECT
    e.StudentId,
    e.EnrollmentId,
    cs.SectionId,
    cs.SectionCode,
    cs.SectionName,
    c.CourseId,
    c.CourseCode,
    c.CourseName,
    c.Credits,
    sem.SemesterId,
    sem.SemesterCode,
    sem.SemesterName,
    sem.AcademicYear,
    e.Status AS EnrollmentStatus,
    e.FinalScore10,
    e.LetterGrade
FROM dbo.Enrollments e
INNER JOIN dbo.CourseSections cs
    ON cs.SectionId = e.SectionId
INNER JOIN dbo.Courses c
    ON c.CourseId = cs.CourseId
INNER JOIN dbo.Semesters sem
    ON sem.SemesterId = cs.SemesterId;
GO

/*==============================================================================
  21. VIEW - THỜI KHÓA BIỂU SINH VIÊN
==============================================================================*/

CREATE OR ALTER VIEW dbo.vw_StudentSchedule
AS
SELECT
    e.StudentId,
    cs.SectionId,
    cs.SectionCode,
    c.CourseCode,
    c.CourseName,
    sch.ScheduleId,
    sch.DayOfWeek,
    sch.StartTime,
    sch.EndTime,
    sch.Room,
    sch.Building,
    sch.EffectiveFrom,
    sch.EffectiveTo
FROM dbo.Enrollments e
INNER JOIN dbo.CourseSections cs
    ON cs.SectionId = e.SectionId
INNER JOIN dbo.Courses c
    ON c.CourseId = cs.CourseId
INNER JOIN dbo.ClassSchedules sch
    ON sch.SectionId = cs.SectionId
WHERE e.Status = 1;
GO

/*==============================================================================
  22. VIEW - DEADLINE BÀI TẬP SINH VIÊN
==============================================================================*/

CREATE OR ALTER VIEW dbo.vw_StudentAssignmentDeadlines
AS
SELECT
    e.StudentId,
    cs.SectionId,
    c.CourseCode,
    c.CourseName,
    a.AssignmentId,
    a.Title,
    a.OpenAt,
    a.DueAt,
    a.MaxScore,
    a.AllowLateSubmission,
    s.SubmissionId,
    s.SubmittedAt,
    s.Score,
    CASE
        WHEN s.SubmissionId IS NULL AND SYSDATETIME() > a.DueAt THEN N'Quá hạn'
        WHEN s.SubmissionId IS NULL THEN N'Chưa nộp'
        WHEN s.Status = 1 THEN N'Đã nộp'
        WHEN s.Status = 2 THEN N'Đã chấm'
        WHEN s.Status = 3 THEN N'Yêu cầu nộp lại'
        ELSE N'Không xác định'
    END AS SubmissionStatus
FROM dbo.Enrollments e
INNER JOIN dbo.CourseSections cs
    ON cs.SectionId = e.SectionId
INNER JOIN dbo.Courses c
    ON c.CourseId = cs.CourseId
INNER JOIN dbo.Assignments a
    ON a.SectionId = cs.SectionId
LEFT JOIN dbo.AssignmentSubmissions s
    ON s.AssignmentId = a.AssignmentId
   AND s.StudentId = e.StudentId
WHERE e.Status = 1
  AND a.IsPublished = 1;
GO

/*==============================================================================
  23. VIEW - ĐIỂM TỔNG HỢP THEO LỚP HỌC PHẦN
==============================================================================*/

CREATE OR ALTER VIEW dbo.vw_StudentSectionScores
AS
SELECT
    e.StudentId,
    e.SectionId,
    c.CourseCode,
    c.CourseName,
    c.Credits,
    CAST
    (
        SUM
        (
            CASE
                WHEN sg.Score IS NULL THEN 0
                ELSE (sg.Score / NULLIF(gc.MaxScore, 0))
                     * gc.WeightPercent
                     / 10.0
            END
        )
        AS DECIMAL(5,2)
    ) AS WeightedScore10
FROM dbo.Enrollments e
INNER JOIN dbo.CourseSections cs
    ON cs.SectionId = e.SectionId
INNER JOIN dbo.Courses c
    ON c.CourseId = cs.CourseId
INNER JOIN dbo.GradeComponents gc
    ON gc.SectionId = cs.SectionId
LEFT JOIN dbo.StudentGrades sg
    ON sg.GradeComponentId = gc.GradeComponentId
   AND sg.StudentId = e.StudentId
GROUP BY
    e.StudentId,
    e.SectionId,
    c.CourseCode,
    c.CourseName,
    c.Credits;
GO

/*==============================================================================
  24. VIEW - GPA TÍCH LŨY
==============================================================================*/

CREATE OR ALTER VIEW dbo.vw_StudentGPA
AS
SELECT
    x.StudentId,
    CAST
    (
        SUM(dbo.fn_Score10ToGPA4(x.WeightedScore10) * x.Credits)
        / NULLIF(SUM(x.Credits), 0)
        AS DECIMAL(4,2)
    ) AS GPA
FROM dbo.vw_StudentSectionScores x
GROUP BY x.StudentId;
GO

/*==============================================================================
  25. VIEW - DANH SÁCH LỚP GIẢNG VIÊN PHỤ TRÁCH
==============================================================================*/

CREATE OR ALTER VIEW dbo.vw_TeacherSections
AS
SELECT
    st.TeacherId,
    st.IsPrimary,
    cs.SectionId,
    cs.SectionCode,
    cs.SectionName,
    c.CourseCode,
    c.CourseName,
    c.Credits,
    sem.SemesterCode,
    sem.SemesterName,
    sem.AcademicYear,
    cs.Status
FROM dbo.SectionTeachers st
INNER JOIN dbo.CourseSections cs
    ON cs.SectionId = st.SectionId
INNER JOIN dbo.Courses c
    ON c.CourseId = cs.CourseId
INNER JOIN dbo.Semesters sem
    ON sem.SemesterId = cs.SemesterId;
GO

/*==============================================================================
  26. STORED PROCEDURE - DASHBOARD SINH VIÊN
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.sp_GetStudentDashboard
    @StudentId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- Thông tin sinh viên + GPA
    SELECT
        s.StudentId,
        s.StudentCode,
        u.FullName,
        u.Email,
        m.MajorName,
        ac.ClassName,
        g.GPA
    FROM dbo.Students s
    INNER JOIN dbo.Users u
        ON u.UserId = s.UserId
    INNER JOIN dbo.Majors m
        ON m.MajorId = s.MajorId
    LEFT JOIN dbo.AcademicClasses ac
        ON ac.AcademicClassId = s.AcademicClassId
    LEFT JOIN dbo.vw_StudentGPA g
        ON g.StudentId = s.StudentId
    WHERE s.StudentId = @StudentId;

    -- 10 deadline gần nhất
    SELECT TOP (10)
        StudentId,
        SectionId,
        CourseCode,
        CourseName,
        AssignmentId,
        Title,
        DueAt,
        MaxScore,
        SubmissionStatus,
        SubmittedAt,
        Score
    FROM dbo.vw_StudentAssignmentDeadlines
    WHERE StudentId = @StudentId
      AND DueAt >= SYSDATETIME()
    ORDER BY DueAt ASC;

    -- 10 lịch thi gần nhất
    SELECT TOP (10)
        e.StudentId,
        c.CourseCode,
        c.CourseName,
        ex.ExamId,
        ex.ExamName,
        ex.ExamType,
        ex.ExamDate,
        ex.StartTime,
        ex.DurationMinutes,
        ex.Room
    FROM dbo.Enrollments e
    INNER JOIN dbo.CourseSections cs
        ON cs.SectionId = e.SectionId
    INNER JOIN dbo.Courses c
        ON c.CourseId = cs.CourseId
    INNER JOIN dbo.Exams ex
        ON ex.SectionId = cs.SectionId
    WHERE e.StudentId = @StudentId
      AND e.Status = 1
      AND ex.ExamDate >= CAST(GETDATE() AS DATE)
    ORDER BY ex.ExamDate ASC, ex.StartTime ASC;
END;
GO

/*==============================================================================
  27. STORED PROCEDURE - DANH SÁCH SINH VIÊN TRONG LỚP HỌC PHẦN
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.sp_GetStudentsBySection
    @SectionId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.StudentId,
        s.StudentCode,
        u.FullName,
        u.Email,
        u.Phone,
        e.EnrolledAt,
        e.Status,
        e.FinalScore10,
        e.LetterGrade
    FROM dbo.Enrollments e
    INNER JOIN dbo.Students s
        ON s.StudentId = e.StudentId
    INNER JOIN dbo.Users u
        ON u.UserId = s.UserId
    WHERE e.SectionId = @SectionId
    ORDER BY s.StudentCode;
END;
GO

/*==============================================================================
  28. STORED PROCEDURE - ĐÁNH DẤU THÔNG BÁO ĐÃ ĐỌC
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.sp_MarkAnnouncementAsRead
    @AnnouncementId BIGINT,
    @UserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.AnnouncementReads
        WHERE AnnouncementId = @AnnouncementId
          AND UserId = @UserId
    )
    BEGIN
        UPDATE dbo.AnnouncementReads
        SET IsRead = 1,
            ReadAt = SYSDATETIME()
        WHERE AnnouncementId = @AnnouncementId
          AND UserId = @UserId;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.AnnouncementReads
        (
            AnnouncementId,
            UserId,
            IsRead,
            ReadAt
        )
        VALUES
        (
            @AnnouncementId,
            @UserId,
            1,
            SYSDATETIME()
        );
    END
END;
GO

/*==============================================================================
  29. STORED PROCEDURE - THỐNG KÊ DASHBOARD ADMIN
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.sp_GetAdminDashboard
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        (SELECT COUNT(*) FROM dbo.Students WHERE Status = 1) AS ActiveStudents,
        (SELECT COUNT(*) FROM dbo.Teachers WHERE Status = 1) AS ActiveTeachers,
        (SELECT COUNT(*) FROM dbo.Courses WHERE IsActive = 1) AS ActiveCourses,
        (SELECT COUNT(*) FROM dbo.CourseSections WHERE Status = 1) AS OpenSections,
        (SELECT COUNT(*) FROM dbo.Users WHERE IsActive = 1) AS ActiveUsers;
END;
GO

/*==============================================================================
  30. GỢI Ý TRUY VẤN KIỂM TRA
==============================================================================*/

-- SELECT * FROM dbo.Roles;
-- EXEC dbo.sp_GetAdminDashboard;
-- SELECT * FROM dbo.vw_StudentSections WHERE StudentId = 1;
-- SELECT * FROM dbo.vw_StudentSchedule WHERE StudentId = 1;
-- SELECT * FROM dbo.vw_StudentAssignmentDeadlines WHERE StudentId = 1;
-- SELECT * FROM dbo.vw_StudentGPA WHERE StudentId = 1;
-- SELECT * FROM dbo.vw_TeacherSections WHERE TeacherId = 1;

