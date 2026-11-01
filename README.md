# VietRove - Hệ Thống Quản Lý Hội Viên Hướng Dẫn Viên Du Lịch

Dự án web ASP.NET Core MVC phục vụ quản lý, đăng ký, tra cứu thông tin hội viên và điều hành hoạt động của Hiệp hội Hướng dẫn viên Du lịch Việt Nam.

---

## 1. Công nghệ sử dụng

- Framework: ASP.NET Core 10.0 (MVC)
- Database: Microsoft SQL Server, truy vấn qua Entity Framework Core
- Mã hóa mật khẩu: BCrypt.Net-Next (Work Factor = 12)
- Xác thực phiên: Cookie Authentication (HttpOnly, SecurePolicy Always, SameSite Strict, trượt hạn 30 phút)
- Frontend: Razor Views (.cshtml), Bootstrap 5, CSS tùy chỉnh, Google Fonts (Dancing Script), JavaScript Fetch API

---

## 2. Cấu trúc thư mục

```
VietRove/
├── Controllers/
│   ├── HomeController.cs         trang chủ, tra cứu HDV, 5 trang giới thiệu
│   ├── LoginController.cs        xác thực, nạp Claims, xử lý thông báo duyệt/khóa
│   ├── RegisterController.cs     đăng ký hội viên, kiểm tra dữ liệu, upload file
│   ├── GuideController.cs        các trang của hội viên HDV (Profile, Contract, Settings)
│   └── AdminController.cs        dashboard quản trị viên
│
├── Security/
│   └── AppRoles.cs               hằng số RoleId (Admin = 1, Guide = 2)
│
├── Data/
│   └── VietRoveDbContext.cs       cấu hình kết nối SQL Server
│
├── Entities/                      các thực thể ánh xạ với CSDL
│   ├── User.cs                   (tblUsers, có trường LockReason)
│   ├── Role.cs                   (tblRoles)
│   ├── GuideProfile.cs           (tblGuideProfiles)
│   ├── Category.cs               (tblCategories)
│   ├── GuideCategory.cs          (tblGuideCategories)
│   ├── Contract.cs               (tblContracts)
│   ├── ViolationReport.cs        (tblViolationReports)
│   └── Article.cs                (tblArticles)
│
├── Models/
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   └── ErrorViewModel.cs
│
├── Views/
│   ├── _ViewStart.cshtml          layout mặc định toàn ứng dụng (_GuestLayout)
│   ├── Home/
│   │   ├── _ViewStart.cshtml      tự chọn layout theo vai trò (Guide hoặc Guest)
│   │   ├── Home.cshtml            trang chủ: hero banner và tin tức
│   │   ├── Guides.cshtml          tìm kiếm và lọc hội viên HDV công khai
│   │   ├── About.cshtml           về chúng tôi
│   │   ├── Vision.cshtml          tầm nhìn và sứ mệnh
│   │   ├── Regulations.cshtml     quy chế hoạt động
│   │   ├── Rules.cshtml           quy định hội viên và bảng lệ phí
│   │   └── GuideRegister.cshtml   hướng dẫn đăng ký hội viên
│   ├── Login/
│   │   └── Login.cshtml           đăng nhập, hiển thị banner thông báo duyệt/khóa
│   ├── Register/
│   │   └── Register.cshtml        đăng ký hội viên
│   ├── Guide/
│   │   ├── _ViewStart.cshtml      áp dụng _GuideLayout cho tất cả trang hội viên
│   │   ├── Dashboard.cshtml
│   │   ├── Profile.cshtml
│   │   ├── Contract.cshtml
│   │   └── Settings.cshtml
│   ├── Admin/
│   │   ├── _ViewStart.cshtml      áp dụng _AdminLayout cho tất cả trang admin
│   │   └── Dashboard.cshtml
│   └── Shared/
│       ├── _GuestLayout.cshtml    layout khách vãng lai
│       ├── _GuideLayout.cshtml    layout hội viên HDV (avatar dropdown)
│       ├── _AdminLayout.cshtml    layout quản trị viên
│       ├── _HeadPartial.cshtml    thẻ head và toàn bộ CSS dùng chung
│       ├── _HeaderPartial.cshtml  logo, nút toggler, menu điều hướng chính
│       ├── _FooterPartial.cshtml  footer copyright
│       └── _ScriptsPartial.cshtml Bootstrap JS bundle
│
├── wwwroot/
│   ├── images/bluetick.png        huy hiệu tích xanh tài khoản đã duyệt
│   ├── js/register.js             thêm/xóa dòng ngoại ngữ động, validate form client
│   └── ProtectedUploads/          thư mục lưu file upload (đổi tên GUID, không truy cập trực tiếp)
│
├── VietRoveDB.sql                 script tạo toàn bộ CSDL và dữ liệu mẫu
└── appsettings.json               chuỗi kết nối Database
```

---

## 3. Các chức năng đã hoàn thiện

### 3.1. Giao diện khách vãng lai (Guest)

- Thanh điều hướng Bootstrap với dropdown menu "Giới thiệu" gồm 5 trang con: Về Chúng Tôi, Tầm Nhìn & Sứ Mệnh, Quy Chế Hoạt Động, Quy Định Hội Viên, Hướng Dẫn Đăng Ký.
- Nút Đăng nhập và Đăng ký hội viên ở góc phải navbar.
- Trang chủ: hero banner lớn với ảnh phong cảnh Việt Nam, phủ overlay gradient, tiêu đề dùng font chữ viết tay nghệ thuật Dancing Script.
- Tin tức: danh sách bài viết từ bảng `tblArticles`.
- Tìm kiếm hội viên: lọc theo họ tên, địa phương, loại thẻ; chỉ hiển thị tài khoản đã duyệt (Status = 1); che 3 số giữa số điện thoại (Data Masking) để bảo vệ thông tin cá nhân.
- 5 trang thông tin giới thiệu: nội dung dựa theo hoihuongdanvien.vn, gồm cơ sở pháp lý, 8 mục tiêu chiến lược, toàn văn quy chế 7 chương 30 điều, bảng lệ phí hội phí, và 6 bước đăng ký trực tuyến.

### 3.2. Giao diện hội viên HDV (Guide)

Thư mục `Views/Home` dùng chung cho cả Guest lẫn Guide. File `Views/Home/_ViewStart.cshtml` tự động chọn layout theo vai trò: nếu người dùng đã đăng nhập là Guide thì dùng `_GuideLayout`, còn lại dùng `_GuestLayout`. Nhờ vậy mọi trang trong Home không cần viết lại, chỉ thay đổi phần navbar.

Layout hội viên thay nút Đăng nhập / Đăng ký bằng avatar hình tròn. Nhấn vào avatar mở dropdown 3 phần:

**Phần 1 - Thông tin cá nhân (chỉ xem, không click):**
- Avatar nhỏ bo tròn và họ tên in đậm.
- Trạng thái: nếu Status = 1 hiện tích xanh xác thực; nếu Status = 0 hiện nhãn vàng "Đang chờ duyệt".
- Email tài khoản.
- Mã thẻ HDV (hoặc "HDV-Chưa cấp" nếu chưa có).

**Phần 2 - Menu chức năng (ẩn hoàn toàn nếu Status = 0):**
- Hồ sơ cá nhân (Guide/Profile)
- Tạo hợp đồng thỏa thuận (Guide/Contract)
- Cài đặt tài khoản / Bảo mật (Guide/Settings)

**Phần 3 - Đăng xuất:**
- Nút đăng xuất gửi POST đến Login/Logout, có AntiForgeryToken chống CSRF.

### 3.3. Xác thực và đồng bộ phiên

**Đăng ký:**
- Form nhập đầy đủ thông tin cá nhân, CCCD 12 số, loại thẻ HDV.
- Nếu chọn thẻ Quốc tế, form tự động mở phần nhập ngoại ngữ và chứng chỉ bằng JavaScript.
- Upload file: giới hạn 10MB/tệp, kiểm tra Magic Bytes (byte thực tế của file) để chống giả mạo đuôi file, lưu bằng tên GUID vào `ProtectedUploads/`.
- Tài khoản mới tạo luôn có Status = 0 (chờ duyệt).

**Đăng nhập và Claims:**
- Lúc đăng nhập, `LoginController` truy vấn database một lần duy nhất, nạp các thông tin cần thiết vào Claims của Cookie: NameIdentifier, Role, Name, Email, Status, CardNumber, Avatar, LastValidatedUtc.
- `_GuideLayout.cshtml` đọc trực tiếp từ Claims, không truy vấn database mỗi lần render.

**Tự động đồng bộ trạng thái (OnValidatePrincipal trong Program.cs):**
- Cứ mỗi 30 giây, hệ thống kiểm tra lại trạng thái tài khoản trong database so với Claims đang lưu trong Cookie.
- Nếu Admin duyệt tài khoản (Status đổi từ 0 lên 1): hệ thống gắn cookie thông báo `SysAuthMsg = approved`, kết thúc phiên, yêu cầu đăng nhập lại. Trang đăng nhập hiện banner xanh: "Tài khoản của bạn đã được duyệt, vui lòng đăng nhập lại."
- Nếu Admin khóa tài khoản (Status = 3): hệ thống gắn cookie `SysAuthMsg = locked`, đăng xuất tức thì. Trang đăng nhập hiện banner đỏ: "Phiên đăng nhập đã kết thúc do tài khoản bị khóa."

**Xử lý lý do từ chối và khóa:**
- Status = 2 (Từ chối): trang đăng nhập hiện "Hồ sơ của bạn đã bị từ chối vì: {RejectReason}".
- Status = 3 (Khóa): trang đăng nhập hiện "Tài khoản của bạn đã bị khóa vì: {LockReason}".

### 3.4. Bảo mật

- Mật khẩu băm bằng BCrypt Work Factor 12.
- Cookie: HttpOnly, SecurePolicy Always, SameSite Strict, trượt hạn 30 phút.
- CSRF: ValidateAntiForgeryToken trên mọi form POST.
- File upload: kiểm tra Magic Bytes, đổi tên GUID, lưu ngoài wwwroot.
- Phục vụ file an toàn qua action `GetAvatar` / `GetThumbnail`, chống Path Traversal.
- Data Masking: che 3 số giữa số điện thoại trên trang tìm kiếm công khai.

### 3.5. Cấu trúc code và layout

- 3 file Layout tách biệt: `_GuestLayout`, `_GuideLayout`, `_AdminLayout`.
- 4 phần dùng chung tách thành Partial Views: `_HeadPartial`, `_HeaderPartial`, `_FooterPartial`, `_ScriptsPartial` — giảm trùng lặp HTML giữa các layout.
- `_ViewStart.cshtml` phân cấp: Global (mặc định Guest), Home (tự chọn theo vai trò), Guide (luôn dùng GuideLayout), Admin (luôn dùng AdminLayout).
- Không dùng toán tử `??`, `?.`, `? :` — thay bằng `if - else` tường minh toàn bộ.
- Tên file View đặt trùng với chức năng: `Home.cshtml`, `Login.cshtml`, `Register.cshtml` (không dùng `Index.cshtml`).

---

## 4. Trạng thái tài khoản (Status)

| Giá trị | Tên | Ý nghĩa |
|---------|-----|---------|
| 0 | Chờ duyệt | Hồ sơ mới đăng ký, chờ admin xét duyệt. Menu chức năng bị ẩn. |
| 1 | Đã duyệt | Hồ sơ hợp lệ, hiện tích xanh, mở toàn bộ chức năng, xuất hiện trên trang tìm kiếm. |
| 2 | Từ chối | Hồ sơ không đạt, có lý do từ chối (RejectReason). |
| 3 | Bị khóa | Tài khoản bị vô hiệu hóa, có lý do khóa (LockReason), bị đăng xuất tức thì. |

---

## 5. Cài đặt và chạy dự án

**Yêu cầu:**
- .NET 10.0 SDK
- Microsoft SQL Server (SQLEXPRESS hoặc Standard/Developer)

**Các bước:**

1. Mở SQL Server Management Studio, mở file `VietRoveDB.sql` và chạy (F5) để tạo database `VietRoveDB`.

2. Mở `appsettings.json`, sửa chuỗi kết nối cho đúng tên server của bạn:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=TEN_SERVER\\SQLEXPRESS;Database=VietRoveDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

3. Chạy dự án:
```bash
dotnet restore
dotnet run
```

**Các địa chỉ chính sau khi chạy:**

| Trang | Đường dẫn |
|-------|-----------|
| Trang chủ | `https://localhost:port/` |
| Tìm kiếm hội viên | `https://localhost:port/Home/Guides` |
| Đăng ký hội viên | `https://localhost:port/Register` |
| Đăng nhập | `https://localhost:port/Login` |
| Dashboard hội viên | `https://localhost:port/Guide/Dashboard` |
| Dashboard admin | `https://localhost:port/Admin/Dashboard` |
