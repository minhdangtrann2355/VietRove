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
| Mã hóa mật khẩu | BCrypt.Net-Next (Work Factor = 12) |
| Xác thực | Cookie Authentication |
| Frontend | Razor Views (.cshtml), Bootstrap 5 |
| JavaScript | Vanilla JavaScript, Fetch API |
| Lưu trữ file | Local File Storage, GUID File Naming |

### Cấu hình bảo mật chính

- Cookie sử dụng `HttpOnly`, `SecurePolicy.Always`, `SameSite.Strict`.
- Thời hạn cookie 30 phút và bật Sliding Expiration.
- Kiểm tra trạng thái tài khoản định kỳ thông qua `OnValidatePrincipal`.
- Sử dụng `ValidateAntiForgeryToken` để phòng chống CSRF.
- Kiểm tra định dạng file bằng phần mở rộng và Magic Bytes.
- File tải lên được đổi tên bằng GUID và lưu ngoài thư mục `wwwroot`.

## 2. Cấu trúc thư mục

```text
VietRove/
├── Controllers/
│   ├── HomeController.cs
│   ├── LoginController.cs
│   ├── RegisterController.cs
│   ├── GuideController.cs
│   └── AdminController.cs
│
├── Services/
│   ├── CategoryService.cs
│   ├── FileStorageService.cs
│   ├── GuideProfileService.cs
│   ├── RegisterService.cs
│   └── Interfaces/
│       ├── ICategoryService.cs
│       ├── IFileStorageService.cs
│       ├── IGuideProfileService.cs
│       └── IRegisterService.cs
│
├── Security/
│   ├── AppRoles.cs
│   └── ProfileStatus.cs
│
├── Data/
│   └── VietRoveDbContext.cs
│
├── Entities/
│   ├── User.cs
│   ├── Role.cs
│   ├── LoginHistory.cs
│   ├── Category.cs
│   ├── GuideProfile.cs
│   ├── GuideProvince.cs
│   ├── GuideLanguage.cs
│   ├── GuideExperience.cs
│   ├── GuideDocument.cs
│   ├── Contract.cs
│   ├── ViolationReport.cs
│   ├── ViolationReportEvidence.cs
│   ├── ViolationReportAction.cs
│   └── Article.cs
│
├── Models/
│   ├── ErrorViewModel.cs
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   └── GuideProfileViewModel.cs
│
├── Views/
│   ├── _ViewStart.cshtml
│   ├── Home/
│   ├── Login/
│   ├── Register/
│   ├── Guide/
│   ├── Admin/
│   └── Shared/
│       ├── _GuestLayout.cshtml
│       ├── _GuideLayout.cshtml
│       ├── _AdminLayout.cshtml
│       ├── _HeadPartial.cshtml
│       ├── _HeaderPartial.cshtml
│       ├── _FooterPartial.cshtml
│       └── _ScriptsPartial.cshtml
│
├── wwwroot/
│   ├── css/
│   ├── images/
│   ├── js/
│   │   └── register.js
│   └── lib/
│
├── ProtectedUploads/
├── VietRoveDB.sql
├── appsettings.json
└── Program.cs
```

**Vai trò các thư mục chính:**

- `Controllers`: Tiếp nhận request, kiểm tra dữ liệu đầu vào và điều phối nghiệp vụ.
- `Services`: Xử lý nghiệp vụ, truy vấn và cập nhật dữ liệu.
- `Services/Interfaces`: Khai báo các hợp đồng dịch vụ.
- `Entities`: Các thực thể ánh xạ với bảng trong SQL Server.
- `Models`: ViewModel phục vụ truyền dữ liệu giữa View và Controller.
- `Security`: Khai báo vai trò và trạng thái hồ sơ.
- `Views`: Giao diện Razor theo từng nhóm người dùng.
- `wwwroot`: Chứa CSS, JavaScript, hình ảnh và thư viện frontend.
- `ProtectedUploads`: Lưu trữ file tải lên, không cho phép truy cập trực tiếp từ trình duyệt.

## 3. Các chức năng đã triển khai

### 3.1. Giao diện khách vãng lai (Guest)

- Trang chủ với hero banner, hình ảnh phong cảnh Việt Nam và khu vực tin tức.
- Thanh điều hướng Bootstrap với các trang giới thiệu:
  - Về chúng tôi.
  - Tầm nhìn và sứ mệnh.
  - Quy chế hoạt động.
  - Quy định hội viên.
  - Hướng dẫn đăng ký.
- Hỗ trợ đăng nhập và đăng ký hội viên.
- Tra cứu hướng dẫn viên theo họ tên, địa phương và loại thẻ.
- Chỉ hiển thị hồ sơ hướng dẫn viên đã được duyệt.
- Che một phần số điện thoại khi hiển thị thông tin công khai.
- Tin tức được truy vấn từ bảng `tblArticles`.

### 3.2. Đăng ký hội viên

- Đăng ký tài khoản bằng email và mật khẩu.
- Kiểm tra dữ liệu đầu vào bằng Data Annotations và IValidatableObject.
- Kiểm tra email đã tồn tại.
- Mã hóa mật khẩu bằng BCrypt.
- Nhập thông tin cá nhân, thông tin thẻ hướng dẫn viên và khu vực hoạt động.
- Tải lên ảnh đại diện, giấy tờ tùy thân và thẻ hướng dẫn viên.
- Đăng ký ngoại ngữ kèm chứng chỉ minh chứng.
- Kiểm tra dung lượng, phần mở rộng và chữ ký file trước khi lưu.
- Tài khoản được tạo với trạng thái hoạt động; hồ sơ hướng dẫn viên ban đầu ở trạng thái chờ duyệt.

### 3.3. Quản lý hồ sơ hội viên HDV

Hội viên sau khi đăng nhập có thể truy cập khu vực quản lý cá nhân thông qua `GuideController`.

**Thông tin hồ sơ và liên hệ**

- Xem thông tin hồ sơ cá nhân.
- Cập nhật số điện thoại, email liên hệ và địa chỉ.
- Cập nhật ảnh đại diện.
- Quản lý tỉnh thành hoạt động.

**Quản lý ngoại ngữ**

- Xem danh sách ngoại ngữ đã đăng ký.
- Hiển thị trạng thái xác minh: Chờ duyệt, Đã xác minh hoặc Từ chối.
- Thêm ngoại ngữ mới kèm chứng chỉ minh chứng.
- Xóa ngoại ngữ đã đăng ký.

**Quản lý kinh nghiệm**

- Xem danh sách kinh nghiệm hành nghề.
- Hiển thị thời gian dẫn tour và trạng thái xác minh.
- Thêm kinh nghiệm mới kèm file minh chứng.
- Xóa kinh nghiệm đã khai báo.

Ngoại ngữ và kinh nghiệm mới được lưu ở trạng thái chờ xác minh để phục vụ quy trình kiểm duyệt của Admin.

### 3.4. Đăng nhập và bảo mật

- Xác thực tài khoản bằng email và mật khẩu.
- Kiểm tra trạng thái hoạt động của tài khoản trước khi đăng nhập.
- Phân quyền bằng Cookie Authentication với hai vai trò `Admin` và `Guide`.
- Sử dụng Claims để lưu thông tin cần thiết như UserId, Role, Email và trạng thái hồ sơ.
- Kiểm tra trạng thái tài khoản và vai trò từ database định kỳ.
- Tự động từ chối cookie khi tài khoản bị khóa hoặc thông tin phân quyền không còn hợp lệ.
- Xử lý đăng xuất và xóa cookie xác thực.

### 3.5. Quản lý file tải lên

Nghiệp vụ xử lý file được tách riêng trong `FileStorageService`.

- Kiểm tra file rỗng và dung lượng tối đa 10 MB.
- Kiểm tra phần mở rộng được cho phép.
- Kiểm tra Magic Bytes của các định dạng JPG, JPEG, PNG và PDF.
- Đổi tên file bằng GUID để tránh trùng tên.
- Lưu file trong `ProtectedUploads`, bên ngoài `wwwroot`.
- Xóa file theo tên an toàn, hạn chế Path Traversal.
- Ghi nhận lỗi khi lưu hoặc xóa file.

### 3.6. Tổ chức kiến trúc Services

Dự án áp dụng mô hình phân lớp để tách biệt nghiệp vụ khỏi Controller.

| Service | Trách nhiệm |
|---|---|
| `RegisterService` | Xử lý nghiệp vụ đăng ký tài khoản và hồ sơ HDV |
| `CategoryService` | Truy vấn danh mục tỉnh thành, ngoại ngữ và loại thẻ |
| `GuideProfileService` | Quản lý hồ sơ, liên hệ, ngoại ngữ và kinh nghiệm |
| `FileStorageService` | Kiểm tra, lưu trữ và xóa file |

Các Service được khai báo thông qua Interface và đăng ký Dependency Injection trong `Program.cs`.

## 4. Cơ sở dữ liệu

Database sử dụng Microsoft SQL Server, được truy vấn thông qua Entity Framework Core.

Dự án hiện có 14 bảng:

| STT | Bảng | Chức năng |
|---:|---|---|
| 1 | `tblRoles` | Quản lý vai trò |
| 2 | `tblUsers` | Quản lý tài khoản |
| 3 | `tblLoginHistory` | Lưu lịch sử đăng nhập |
| 4 | `tblCategories` | Danh mục tỉnh thành, ngoại ngữ và loại thẻ |
| 5 | `tblGuideProfiles` | Thông tin hồ sơ hướng dẫn viên |
| 6 | `tblGuideProvinces` | Khu vực hoạt động của hướng dẫn viên |
| 7 | `tblGuideLanguages` | Ngoại ngữ và chứng chỉ |
| 8 | `tblGuideExperiences` | Kinh nghiệm hành nghề |
| 9 | `tblGuideDocuments` | Giấy tờ của hướng dẫn viên |
| 10 | `tblContracts` | Quản lý hợp đồng |
| 11 | `tblViolationReports` | Báo cáo vi phạm |
| 12 | `tblViolationReportEvidence` | Minh chứng vi phạm |
| 13 | `tblViolationReportActions` | Lịch sử xử lý vi phạm |
| 14 | `tblArticles` | Quản lý bài viết |

### Quy ước trạng thái chính

**Trạng thái hồ sơ hướng dẫn viên (`GuideProfile.Status`):**

| Giá trị | Ý nghĩa |
|---:|---|
| 0 | Chờ duyệt |
| 1 | Đã duyệt |
| 2 | Từ chối |
| 3 | Khóa |
| 4 | Yêu cầu chỉnh sửa |

**Trạng thái xác minh ngoại ngữ và giấy tờ:**

| Giá trị | Ý nghĩa |
|---:|---|
| 0 | Chờ duyệt |
| 1 | Đã xác minh |
| 2 | Từ chối |

## 5. Quy ước lập trình

- Sử dụng C# và ASP.NET Core MVC.
- Tách biệt Controller, Service, Interface và Entity.
- Sử dụng Dependency Injection để quản lý các Service.
- Sử dụng Entity Framework Core cho thao tác dữ liệu.
- Sử dụng ViewModel để truyền dữ liệu giữa Controller và View.
- JavaScript sử dụng Vanilla JavaScript và Fetch API.
- Hạn chế viết nghiệp vụ xử lý dữ liệu trực tiếp trong Controller.
- Comment trong code sử dụng dạng `// noi dung cmt`, viết tiếng Việt không dấu.

## 6. Hướng dẫn cài đặt và chạy dự án

### Yêu cầu môi trường

- .NET 10 SDK.
- Microsoft SQL Server.
- SQL Server Management Studio (SSMS).
- Visual Studio hoặc Visual Studio Code.

### Bước 1: Tạo database

Mở SQL Server Management Studio và chạy file:

```text
VietRoveDB.sql
```

Script sẽ tạo database `VietRoveDB`, các bảng, quan hệ, index và dữ liệu danh mục khởi tạo.

### Bước 2: Cấu hình kết nối

Mở `appsettings.json` và cập nhật chuỗi kết nối SQL Server phù hợp với máy đang sử dụng.

Không chia sẻ chuỗi kết nối chứa thông tin đăng nhập thực tế khi công khai mã nguồn.

### Bước 3: Khôi phục package

Mở Terminal tại thư mục chứa file `.csproj` và chạy:

```bash
dotnet restore
```

### Bước 4: Chạy dự án

```bash
dotnet run
```

Mở địa chỉ localhost được hiển thị trong Terminal để truy cập website.

## 7. Định hướng phát triển

Các chức năng quản trị có thể tiếp tục được hoàn thiện:

- Dashboard thống kê hội viên và tình trạng hồ sơ.
- Kiểm duyệt hồ sơ đăng ký hội viên.
- Xác minh ngoại ngữ, chứng chỉ và kinh nghiệm.
- Quản lý tài khoản và trạng thái hội viên.
- Quản lý hợp đồng.
- Tiếp nhận và xử lý báo cáo vi phạm.
- Quản lý bài viết và tin tức.

---

**VietRove – Hệ thống quản lý hội viên hướng dẫn viên du lịch.**