# VietRove – Hệ Thống Quản Lý Hội Viên Hướng Dẫn Viên Du Lịch

VietRove là dự án website được xây dựng bằng **ASP.NET Core MVC**, phục vụ việc đăng ký, quản lý hội viên, tra cứu thông tin hướng dẫn viên du lịch và hỗ trợ công tác quản lý của Hiệp hội Hướng dẫn viên Du lịch Việt Nam.

## 1. Công nghệ sử dụng

| Thành phần | Công nghệ |
|---|---|
| Framework | ASP.NET Core 10.0 (MVC) |
| Ngôn ngữ | C# |
| Database | Microsoft SQL Server |
| ORM | Entity Framework Core |
| Kiến trúc | Controllers – Services – Data |
| Xác thực & Phân quyền | Cookie Authentication, BCrypt.Net-Next |
| Giao diện | Razor Views, Bootstrap 5, Vanilla JavaScript |
| File Storage | Local File Storage, GUID Naming |

## 2. Cấu trúc dự án

Dự án áp dụng mô hình phân lớp nhằm tách biệt nghiệp vụ:
- **Controllers**: Điều hướng request và trả về kết quả (Guest, Admin, Guide, Authentication).
- **Services**: Chứa logic xử lý nghiệp vụ chính (Quản lý hồ sơ, File, Đăng nhập, Bài viết...).
- **Data & Entities**: Truy cập và cấu trúc cơ sở dữ liệu qua Entity Framework.
- **Models**: Chứa các ViewModel phục vụ chuyển đổi dữ liệu hiển thị.
- **wwwroot**: Chứa các tệp tĩnh (CSS, JS, hình ảnh).

## 3. Các chức năng chính

### Khách vãng lai (Guest)
- Tra cứu và xem thông tin hồ sơ của Hướng dẫn viên đã được xác thực (tìm kiếm theo tên, tỉnh thành, loại thẻ, ngoại ngữ).
- Đọc tin tức, bài viết trải nghiệm và quy định của Hiệp hội.
- Gửi báo cáo vi phạm kèm tệp minh chứng đối với Hướng dẫn viên.
- Đăng ký tài khoản hội viên và nộp hồ sơ thẻ Hướng dẫn viên.

### Hướng dẫn viên (Guide)
- Đăng nhập và quản lý thông tin tài khoản cá nhân.
- Cập nhật thông tin liên hệ và hình ảnh đại diện.
- Khai báo và bổ sung ngoại ngữ, kinh nghiệm làm việc (có kèm minh chứng chờ xét duyệt).
- Quản lý hợp đồng hành nghề.

### Quản trị viên (Admin)
- Quản lý trạng thái tài khoản (Khóa/Mở khóa) và hồ sơ (Duyệt/Từ chối) của Hướng dẫn viên.
- Xét duyệt minh chứng ngôn ngữ và kinh nghiệm làm việc.
- Tiếp nhận, xem xét và xử lý các báo cáo vi phạm từ người dùng.
- Quản lý danh mục (tỉnh/thành, ngôn ngữ, loại thẻ).
- Đăng tải và quản lý bài viết tin tức, thông báo.
- Theo dõi thống kê (Dashboard) và lịch sử đăng nhập.

## 4. Cơ sở dữ liệu

Sử dụng **Microsoft SQL Server**, bao gồm 14 bảng chính:
- Hệ thống tài khoản và vai trò (`tblUsers`, `tblRoles`, `tblLoginHistory`).
- Quản lý hồ sơ và chi tiết HDV (`tblGuideProfiles`, `tblGuideProvinces`, `tblGuideLanguages`, `tblGuideExperiences`, `tblGuideDocuments`, `tblContracts`).
- Danh mục hệ thống (`tblCategories`).
- Quản lý báo cáo vi phạm (`tblViolationReports`, `tblViolationReportEvidence`, `tblViolationReportActions`).
- Quản lý bài viết (`tblArticles`).

## 5. Hướng dẫn cài đặt và chạy dự án

1. **Yêu cầu:** .NET 10 SDK, Microsoft SQL Server.
2. **Khởi tạo Database:** Chạy tệp script `VietRoveDB.sql` trong SQL Server Management Studio.
3. **Cấu hình:** Cập nhật chuỗi kết nối (Connection String) trong tệp `appsettings.json`.
4. **Cài đặt thư viện:** Mở Terminal tại thư mục dự án và chạy `dotnet restore`.
5. **Chạy ứng dụng:** Chạy lệnh `dotnet run` và truy cập qua đường dẫn localhost được hiển thị.