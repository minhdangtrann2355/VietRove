# VietRove - Hệ Thống Quản Lý Hội Viên Hướng Dẫn Viên Du Lịch

Dự án web ASP.NET Core MVC phục vụ quản lý, đăng ký, tra cứu thông tin hội viên và điều hành hoạt động của Hiệp hội Hướng dẫn viên Du lịch Việt Nam.

---

## 📌 1. Tổng Quan & Công Nghệ Sử Dụng

- **Framework:** ASP.NET Core 10.0 (MVC)
- **ORM / Database:** Entity Framework Core, Microsoft SQL Server (MSSQL - VietRoveDB)
- **Mã hóa & Bảo mật:**
  - Thư viện BCrypt.Net-Next (băm mật khẩu chuẩn với Work Factor = 12).
  - Ghim `System.Security.Cryptography.Pkcs` phiên bản 9.0.21 để vá các advisory an ninh từ dependencies.
  - Kiểm tra chữ ký byte thực tế của tệp tin (Magic Byte Verification) chống giả mạo định dạng file độc hại (.jpg, .jpeg, .png, .pdf).
  - Cơ chế xử lý tệp tin an toàn (Action `GetAvatar`, `GetThumbnail`) phòng chống tấn công vượt thư mục (Path Traversal `../..`).
  - Che giấu dữ liệu cá nhân (Data Masking): Ẩn 3 số giữa số điện thoại của hội viên trên giao diện công khai để bảo vệ an toàn thông tin.
  - Bảo vệ CSRF toàn diện bằng `[ValidateAntiForgeryToken]` và AntiForgeryToken tokens trên mọi form POST.
  - Cơ chế phiên xác thực an toàn: Cookie Authentication với `HttpOnly`, `SecurePolicy = Always`, `SameSite = Strict`, trượt hạn 30 phút.
  - Tự động đồng bộ trạng thái tài khoản qua sự kiện `OnValidatePrincipal` trong Cookie Authentication mỗi 30 giây.
- **Frontend:** Razor Views (.cshtml), Bootstrap 5, Google Fonts (Dancing Script handwriting nghệ thuật), JavaScript Fetch API, CSS Responsive tối ưu cho cả máy tính và điện thoại di động.

---

## 📂 2. Cấu Trúc Thư Mục & Vị Trí Các File Đã Triển Khai

```plaintext
VietRove/
│
├── Controllers/
│   ├── HomeController.cs              <-- Trang chủ (Hero Banner, Tin tức), Tra cứu HDV, 5 Trang Giới thiệu
│   ├── LoginController.cs             <-- Xác thực BCrypt, nạp Claims, xử lý thông báo duyệt/khóa, lý do từ chối
│   ├── AdminController.cs             <-- Dashboard chỉ dành cho Admin (RoleId = 1)
│   ├── GuideController.cs             <-- Dashboard, Profile, Contract, Settings của Hội viên HDV (RoleId = 2)
│   └── RegisterController.cs          <-- Đăng ký hội viên, kiểm tra dữ liệu, upload file bảo mật
│
├── Security/
│   └── AppRoles.cs                    <-- Định nghĩa hằng số RoleId và tên role (Admin = 1, Guide = 2)
│
├── Data/
│   └── VietRoveDbContext.cs           <-- Cấu hình Entity Framework Core kết nối SQL Server
│
├── Entities/                          <-- Các thực thể ánh xạ trực tiếp với CSDL
│   ├── User.cs                        (Ánh xạ tblUsers - bổ sung LockReason)
│   ├── Role.cs                        (Ánh xạ tblRoles)
│   ├── GuideProfile.cs                (Ánh xạ tblGuideProfiles)
│   ├── Category.cs                    (Ánh xạ tblCategories)
│   ├── GuideCategory.cs               (Ánh xạ tblGuideCategories)
│   ├── Contract.cs                    (Ánh xạ tblContracts)
│   ├── ViolationReport.cs             (Ánh xạ tblViolationReports)
│   └── Article.cs                     (Ánh xạ tblArticles)
│
├── Models/                            <-- Các ViewModel phục vụ truyền nhận dữ liệu
│   ├── LoginViewModel.cs              (Dữ liệu form đăng nhập)
│   ├── RegisterViewModel.cs           (Dữ liệu form đăng ký hội viên)
│   └── ErrorViewModel.cs              (Xử lý mã lỗi hệ thống)
│
├── Views/
│   ├── _ViewStart.cshtml              <-- Layout gốc mặc định cho toàn bộ ứng dụng (_GuestLayout)
│   ├── Home/
│   │   ├── _ViewStart.cshtml          <-- Tự động chọn layout động theo vai trò (Guide -> _GuideLayout, Guest -> _GuestLayout)
│   │   ├── Home.cshtml                <-- Trang chủ: Khối Hero Banner lớn & Tin tức hoạt động
│   │   ├── Guides.cshtml              <-- Trang Tìm kiếm & Lọc hội viên HDV công khai (Data Masking số điện thoại)
│   │   ├── About.cshtml               <-- Về Chúng Tôi (Cơ sở pháp lý, sứ mệnh, giá trị cốt lõi)
│   │   ├── Vision.cshtml              <-- Tầm Nhìn & Sứ Mệnh (8 mục tiêu chiến lược)
│   │   ├── Regulations.cshtml         <-- Quy Chế Hoạt Động (Văn bản pháp lý 7 chương 30 điều)
│   │   ├── Rules.cshtml               <-- Quy Định Hội Viên & Bảng lệ phí, hội phí
│   │   ├── GuideRegister.cshtml       <-- Hướng dẫn đăng ký hội viên (Hồ sơ & 6 bước trực tuyến)
│   │   └── Privacy.cshtml             <-- Chính sách bảo mật
│   ├── Login/
│   │   └── Login.cshtml               <-- Giao diện Đăng nhập hệ thống (kèm banner thông báo duyệt/khóa tài khoản)
│   ├── Register/
│   │   └── Register.cshtml            <-- Giao diện Đăng ký hội viên độc lập (Dynamic fields thẻ Quốc tế)
│   ├── Guide/
│   │   ├── _ViewStart.cshtml          <-- Áp dụng _GuideLayout cho toàn bộ trang của Hội viên HDV
│   │   ├── Dashboard.cshtml           <-- Bảng điều khiển Hội viên
│   │   ├── Profile.cshtml             <-- Hồ sơ cá nhân hội viên
│   │   ├── Contract.cshtml            <-- Tạo hợp đồng thỏa thuận
│   │   └── Settings.cshtml            <-- Cài đặt tài khoản / Bảo mật
│   ├── Admin/
│   │   ├── _ViewStart.cshtml          <-- Áp dụng _AdminLayout cho toàn bộ trang của Admin
│   │   └── Dashboard.cshtml           <-- Bảng điều khiển Quản trị viên
│   └── Shared/
│       ├── _GuestLayout.cshtml        <-- Layout Khách vãng lai (Navbar Dropdown, Nút Đăng nhập/Đăng ký)
│       ├── _GuideLayout.cshtml        <-- Layout Hội viên HDV (Avatar hình tròn + Dropdown menu 3 phần)
│       ├── _AdminLayout.cshtml        <-- Layout Quản trị viên Admin (Navbar tối màu chuyên nghiệp)
│       ├── _ValidationScriptsPartial.cshtml
│       └── Error.cshtml
│
├── wwwroot/
│   ├── images/
│   │   └── bluetick.png               <-- Huy hiệu tích xanh xác thực tài khoản đã duyệt
│   ├── js/
│   │   └── register.js                <-- Thêm/xóa dòng ngoại ngữ động, validate form client
│   └── ProtectedUploads/              <-- Thư mục bảo mật chứa file upload đổi tên GUID
│
├── VietRoveDB.sql                     <-- Script khởi tạo toàn bộ CSDL SQL Server kèm dữ liệu mẫu
└── appsettings.json                   <-- Cấu hình chuỗi kết nối Database (DefaultConnection)
```

---

## 🚀 3. Các Chức Năng Đã Hoàn Thiện

### 3.1. Phân hệ Khách Vãng Lai (Guest) & Trang Chủ
- **Giao diện Layout Khách vãng lai (`_GuestLayout.cshtml`):**
  - Thanh điều hướng chuẩn Bootstrap, tích hợp **Dropdown Menu "GIỚI THIỆU"** mở mượt mà khi hover trên máy tính hoặc click trên điện thoại.
  - Dropdown điều hướng đầy đủ 5 trang con: *Về Chúng Tôi, Tầm Nhìn & Sứ Mệnh, Quy Chế Hoạt Động, Quy Định Hội Viên, Hướng Dẫn Đăng Ký*.
  - Logo thương hiệu VietRove và cụm nút *Đăng nhập / Đăng ký hội viên*.
- **Khối Hero Banner lớn ở đầu trang:**
  - Chiều cao lớn, thoáng (380px), sử dụng hình ảnh phong cảnh ruộng bậc thang / danh lam thắng cảnh Việt Nam chất lượng cao.
  - Phủ lớp gradient tối nhẹ (overlay) tạo chiều sâu và tôn lên độ nổi bật của văn bản.
- **Tiêu đề nghệ thuật (Handwriting Script Heading):**
  - Áp dụng font chữ viết tay nghệ thuật (*Dancing Script*) cỡ lớn, màu xanh trang nhã `#1e3a8a`, có gạch chân trang trí mỏng tinh tế bên dưới.
  - Loại bỏ hoàn toàn các khung viền xám, fieldset hay card rườm rà; nội dung hiển thị dưới dạng văn bản thuần túy hoặc danh sách thoáng đãng trên nền trắng sạch sẽ.
- **Trang Tin tức (`Home/Home`):**
  - Hiển thị danh sách các bài viết, thông báo chính thức từ Hiệp hội lấy động từ bảng `tblArticles`.
- **Trang Tìm kiếm Hướng dẫn viên (`Home/Guides`):**
  - Thanh tìm kiếm theo họ tên, địa phương và bộ lọc theo loại thẻ (Nội địa / Quốc tế).
  - Lọc chính xác các hội viên đã được kiểm duyệt (`Status == 1`), áp dụng che giấu số điện thoại cá nhân (Data Masking) để bảo vệ thông tin.
- **Bộ 5 trang thông tin giới thiệu chính thức (Nguồn: `hoihuongdanvien.vn`):**
  - **Về Chúng Tôi (`About`):** Lịch sử thành lập 10/10/2017, cơ sở pháp lý Luật Du lịch 2017, kiểm toán minh bạch Deloitte.
  - **Tầm Nhìn & Sứ Mệnh (`Vision`):** 8 mục tiêu chiến lược nâng tầm vị thế HDV Việt Nam trên trường quốc tế.
  - **Quy Chế Hoạt Động (`Regulations`):** Toàn văn văn bản pháp lý 7 chương, 30 điều của Hiệp hội.
  - **Quy Định Hội Viên (`Rules`):** 10 quy tắc ứng xử hành nghề và bảng lệ phí gia nhập (200.000 VNĐ), hội phí thường niên (500.000 VNĐ/năm).
  - **Hướng Dẫn Đăng Ký (`GuideRegister`):** Danh mục 4 loại giấy tờ cần chuẩn bị và quy trình 6 bước đăng ký trực tuyến bám sát theo form đăng ký thực tế.

---

### 3.2. Phân hệ Giao Diện Hội Viên Hướng Dẫn Viên (`_GuideLayout.cshtml`)
- **Tái sử dụng giao diện Guest linh hoạt:** Thư mục `Views/Home` dùng chung cho cả Guest và Hội viên thông qua `Views/Home/_ViewStart.cshtml`. Khi khách truy cập thấy nút Đăng nhập / Đăng ký; khi Hội viên truy cập tự động đổi sang Avatar hình tròn.
- **Cấu trúc Dropdown Menu Avatar (3 phần rõ rệt):**
  - **Phần 1: Header (Thông tin cá nhân - chỉ hiển thị):**
    - Avatar nhỏ bo tròn bên trái (ảnh đại diện thật hoặc chữ cái đầu tên).
    - Họ và tên in đậm.
    - **Nhãn trạng thái (Status):**
      - Tài khoản đã duyệt (`Status = 1`): Hiển thị huy hiệu **Tích xanh** xác thực (`bluetick.png`).
      - Tài khoản chưa duyệt (`Status = 0`): Hiển thị nhãn màu vàng **"Đang chờ duyệt"**.
    - Email tài khoản.
    - Mã thẻ HDV (ví dụ: `Mã thẻ: HDV-10293` hoặc `HDV-Chưa cấp`).
  - **Phần 2: Menu Chức Năng Chính (Ẩn hoàn toàn khi `Status = 0`):**
    - Khi tài khoản chưa duyệt (`Status = 0`), toàn bộ khu vực này bị ẩn, hội viên không thể thao tác các chức năng nghiệp vụ.
    - Khi tài khoản đã duyệt (`Status = 1`), hiển thị các liên kết:
      - *Hồ sơ cá nhân* (`Guide/Profile`)
      - *Tạo hợp đồng thỏa thuận* (`Guide/Contract`)
      - *Cài đặt tài khoản / Bảo mật* (`Guide/Settings`)
  - **Phần 3: Footer (Khu vực Đăng xuất):**
    - Nút Đăng xuất gửi yêu cầu POST bảo mật qua `Login/Logout`, chống tấn công CSRF.

---

### 3.3. Phân hệ Xác Thực & Đồng Bộ Phiên (Authentication & Session Sync)
- **Đăng ký hội viên HDV (`RegisterController` & `register.js`):**
  - Form đăng ký chi tiết từ thông tin cá nhân, CCCD 12 số, thẻ HDV đến upload hồ sơ.
  - Tự động mở rộng phần chọn ngoại ngữ và chứng chỉ đính kèm nếu chọn thẻ Quốc tế.
  - Giới hạn dung lượng tối đa 10MB/tệp, xác thực Magic Bytes chống giả mạo đuôi tệp, mã hóa file bằng GUID lưu tại `ProtectedUploads/`.
  - Mặc định khởi tạo hồ sơ ở trạng thái `Status = 0` (Chờ duyệt).
- **Tối ưu hiệu năng bằng Custom Claims:**
  - Lúc đăng nhập thành công tại `LoginController`, thông tin người dùng được truy vấn 1 lần duy nhất và nạp vào Claims (`NameIdentifier`, `Role`, `Name`, `Email`, `Status`, `CardNumber`, `Avatar`, `LastValidatedUtc`).
  - `_GuideLayout.cshtml` đọc trực tiếp từ Claims của Cookie, loại bỏ hoàn toàn việc truy vấn database ở mỗi lần render view.
- **Tự động đồng bộ trạng thái qua `OnValidatePrincipal` (Program.cs):**
  - Kiểm tra định kỳ mỗi 30 giây: Tự động so sánh trạng thái thực tế trong database với trạng thái đang lưu trong Cookie.
  - Nếu tài khoản được Admin duyệt từ `0` lên `1`: Tự động gắn cookie thông báo `SysAuthMsg = approved`, kết thúc phiên tạm thời để yêu cầu người dùng đăng nhập lại nhận toàn bộ quyền hạn mới kèm banner thông báo xanh: *"Tài khoản của bạn đã được duyệt, vui lòng đăng nhập lại."*
  - Nếu tài khoản bị Admin khóa (`Status = 3`): Tự động gắn cookie `SysAuthMsg = locked`, đăng xuất ngay lập tức kèm thông báo đỏ: *"Phiên đăng nhập đã kết thúc do tài khoản bị khóa."*
- **Xử lý lý do từ chối & lý do khóa tài khoản:**
  - Bổ sung thuộc tính `LockReason` trong entity `User`.
  - Khi đăng nhập tài khoản bị từ chối (`Status = 2`): Hiển thị chi tiết *"Hồ sơ của bạn đã bị từ chối vì đã vi phạm: {RejectReason}"*.
  - Khi đăng nhập tài khoản bị khóa (`Status = 3`): Hiển thị chi tiết *"Tài khoản của bạn đã bị khóa vì vi phạm: {LockReason}"*.

---

### 3.4. Chuẩn Hóa Cấu Trúc File & Phong Cách Code
- **Đổi tên View trực quan, đồng bộ:**
  - `Views/Home/Index.cshtml` ➔ `Views/Home/Home.cshtml`
  - `Views/Login/Index.cshtml` ➔ `Views/Login/Login.cshtml`
  - `Views/Register/Index.cshtml` ➔ `Views/Register/Register.cshtml`
- **Loại bỏ toán tử `??`, `?.`, `? :`:** Toàn bộ mã nguồn Razor và C# được chuẩn hóa bằng các khối lệnh `if - else` tường minh, an toàn dữ liệu và dễ bảo trì.
- **Chuẩn hóa chú thích mã nguồn:** Chỉ đặt comment ngắn gọn dạng `//noi dung` (tiếng Việt không hoa, không dấu) tại các đoạn xử lý dữ liệu phức tạp.

---

## 🔄 4. Luồng Xử Lý Nghiệp Vụ & Quy Ước Trạng Thái

### Quy ước Trạng thái tài khoản / hồ sơ (Status):
- `Status = 0`: **Chờ duyệt (Pending)** – Hồ sơ mới đăng ký, đang chờ Quản trị viên xét duyệt. Dropdown menu chỉ xem được thông tin cá nhân và nhãn "Đang chờ duyệt", các chức năng nghiệp vụ bị ẩn.
- `Status = 1`: **Đã duyệt (Active)** – Hồ sơ hợp lệ, hiển thị tích xanh xác thực, mở khóa toàn bộ menu chức năng (Hồ sơ, Hợp đồng, Cài đặt) và xuất hiện trên trang tìm kiếm công khai.
- `Status = 2`: **Từ chối (Rejected)** – Hồ sơ không đạt yêu cầu kèm lý do vi phạm/từ chối cụ thể (`RejectReason`).
- `Status = 3`: **Bị khóa (Locked)** – Tài khoản bị vô hiệu hóa kèm lý do khóa cụ thể (`LockReason`), bị ngắt phiên đăng nhập tức thì.

---

## 🛠️ 5. Hướng Dẫn Cài Đặt & Chạy Dự Án

### Yêu cầu hệ thống
- [.NET 10.0 SDK](https://dotnet.microsoft.com/)
- [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server) (SQLEXPRESS hoặc bản Standard/Developer)

### Các bước khởi chạy
1. **Khởi tạo Database:**
   - Mở SQL Server Management Studio (SSMS).
   - Mở file `VietRoveDB.sql` trong thư mục gốc và bấm **Execute (F5)** để tạo database `VietRoveDB` và dữ liệu danh mục ban đầu.

2. **Cấu hình chuỗi kết nối:**
   - Mở `appsettings.json`, chỉnh sửa lại chuỗi `DefaultConnection` phù hợp với tên Server SQL của bạn:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=TÊN_SERVER_CỦA_BẠN\\SQLEXPRESS;Database=VietRoveDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```

3. **Restore Packages & Chạy dự án:**
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```
   Sau đó mở trình duyệt truy cập:
   - Trang chủ (Guest / Guide dùng chung): `https://localhost:port/`
   - Tra cứu Hướng Dẫn Viên: `https://localhost:port/Home/Guides`
   - Đăng ký hội viên: `https://localhost:port/Register`
   - Đăng nhập hệ thống: `https://localhost:port/Login`
   - Bảng điều khiển Hội viên HDV: `https://localhost:port/Guide/Dashboard`
   - Bảng điều khiển Quản trị viên: `https://localhost:port/Admin/Dashboard`
