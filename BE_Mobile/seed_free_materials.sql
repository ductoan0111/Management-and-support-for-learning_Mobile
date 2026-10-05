-- Chạy riêng script này trên database đã tạo. Có thể chạy lại mà không thêm bản ghi trùng.
-- Không chạy lại Management and academic support.sql vì script đó tạo lại các bảng.
-- Nếu dùng sqlcmd, thêm -f i:65001 để đọc đúng tệp UTF-8 và giữ nguyên tiếng Việt.
USE QuanLyHoTroHocTap;
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @FreeMaterials TABLE
(
    SectionCode VARCHAR(40) NOT NULL,
    Title NVARCHAR(250) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    ExternalUrl NVARCHAR(1000) NOT NULL
);

INSERT INTO @FreeMaterials(SectionCode, Title, Description, ExternalUrl)
VALUES
    ('INT101-2026-HK1-01', N'MDN - HTML cơ bản', N'Hướng dẫn cấu trúc nội dung trang web bằng HTML.', N'https://developer.mozilla.org/en-US/docs/Learn_web_development/Core/Structuring_content'),
    ('INT101-2026-HK1-01', N'MDN - CSS cơ bản', N'Hướng dẫn tạo kiểu giao diện bằng CSS.', N'https://developer.mozilla.org/en-US/docs/Learn_web_development/Core/Styling_basics/Getting_started'),
    ('INT101-2026-HK1-01', N'MDN - JavaScript cơ bản', N'Hướng dẫn lập trình tương tác trên trang web.', N'https://developer.mozilla.org/en-US/docs/Learn_web_development/Core/Scripting'),
    ('INT102-2026-HK1-01', N'Microsoft Learn - SQL cơ bản', N'Hướng dẫn viết câu lệnh Transact-SQL.', N'https://learn.microsoft.com/en-us/sql/t-sql/tutorial-writing-transact-sql-statements'),
    ('INT102-2026-HK1-01', N'Microsoft Learn - Truy vấn dữ liệu', N'Lộ trình học SELECT, lọc, nối và tổng hợp dữ liệu bằng T-SQL.', N'https://learn.microsoft.com/en-us/training/paths/get-started-querying-with-transact-sql/'),
    ('MOB201-2025-HK2-01', N'Expo - Hướng dẫn làm ứng dụng', N'Thực hành tạo ứng dụng di động với Expo và React Native.', N'https://docs.expo.dev/tutorial/introduction/'),
    ('MOB201-2025-HK2-01', N'React Native - Bắt đầu', N'Tài liệu nhập môn các thành phần và khái niệm React Native.', N'https://reactnative.dev/docs/getting-started.html'),
    ('NN01-2026-HK1-01', N'OpenLearn - Khóa học ngôn ngữ', N'Danh sách khóa học miễn phí cho nhiều ngôn ngữ; chọn ngôn ngữ phù hợp với lớp.', N'https://www.open.edu/openlearn/languages/free-courses'),
    ('NN01-2026-HK1-01', N'OpenLearn - Cách học ngoại ngữ', N'Khóa học miễn phí về phương pháp học một ngôn ngữ mới.', N'https://www.open.edu/openlearn/languages/how-learn-language/content-section-overview');

-- Chỉ sửa bản ghi mẫu example.com; không ghi đè tài liệu riêng của giảng viên.
UPDATE m
SET m.Title = N'MDN - HTML cơ bản',
    m.Description = N'Hướng dẫn cấu trúc nội dung trang web bằng HTML.',
    m.MaterialType = 'LINK', m.FileUrl = NULL,
    m.ExternalUrl = N'https://developer.mozilla.org/en-US/docs/Learn_web_development/Core/Structuring_content',
    m.UpdatedAt = SYSDATETIME()
FROM dbo.Materials AS m
INNER JOIN dbo.CourseSections AS cs ON cs.SectionId = m.SectionId
WHERE cs.SectionCode = 'INT101-2026-HK1-01'
  AND m.FileUrl = N'https://example.com/materials/html-css.pdf';

UPDATE m
SET m.Title = N'Microsoft Learn - SQL cơ bản',
    m.Description = N'Hướng dẫn viết câu lệnh Transact-SQL.',
    m.MaterialType = 'LINK', m.FileUrl = NULL,
    m.ExternalUrl = N'https://learn.microsoft.com/en-us/sql/t-sql/tutorial-writing-transact-sql-statements',
    m.UpdatedAt = SYSDATETIME()
FROM dbo.Materials AS m
INNER JOIN dbo.CourseSections AS cs ON cs.SectionId = m.SectionId
WHERE cs.SectionCode = 'INT102-2026-HK1-01'
  AND m.ExternalUrl = N'https://example.com/materials/sql-basic';

-- Sửa chữ bị lỗi mã hóa từ lần chạy sqlcmd không chỉ định UTF-8.
-- Chỉ khôi phục các URL đã liệt kê và bản ghi có dấu hiệu mojibake.
UPDATE m
SET m.Title = fm.Title,
    m.Description = fm.Description,
    m.UpdatedAt = SYSDATETIME()
FROM dbo.Materials AS m
INNER JOIN dbo.CourseSections AS cs ON cs.SectionId = m.SectionId
INNER JOIN @FreeMaterials AS fm
    ON fm.SectionCode = cs.SectionCode AND fm.ExternalUrl = m.ExternalUrl
WHERE m.Title COLLATE Latin1_General_100_BIN2 LIKE N'%Æ%'
   OR m.Title COLLATE Latin1_General_100_BIN2 LIKE N'%Ã%'
   OR m.Title COLLATE Latin1_General_100_BIN2 LIKE N'%Ä%'
   OR m.Title COLLATE Latin1_General_100_BIN2 LIKE N'%Ă%'
   OR m.Title COLLATE Latin1_General_100_BIN2 LIKE N'%áº%';

INSERT INTO dbo.Materials(SectionId, UploadedByUserId, Title, Description, MaterialType, FileUrl, ExternalUrl, IsVisible)
SELECT cs.SectionId, uploader.UserId, fm.Title, fm.Description, 'LINK', NULL, fm.ExternalUrl, 1
FROM @FreeMaterials AS fm
INNER JOIN dbo.CourseSections AS cs ON cs.SectionCode = fm.SectionCode
CROSS APPLY
(
    SELECT TOP (1) t.UserId
    FROM dbo.SectionTeachers AS st
    INNER JOIN dbo.Teachers AS t ON t.TeacherId = st.TeacherId
    WHERE st.SectionId = cs.SectionId
    ORDER BY st.IsPrimary DESC, st.TeacherId
) AS uploader
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Materials AS m
    WHERE m.SectionId = cs.SectionId AND (m.ExternalUrl = fm.ExternalUrl OR m.Title = fm.Title)
);

COMMIT TRANSACTION;
