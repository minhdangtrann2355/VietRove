
using VietRove.Services.Interfaces.FileStorage;

using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VietRove.Data;
using VietRove.Entities;
using VietRove.Models.Guide;
using VietRove.Services.Interfaces.Guide.Contract;

namespace VietRove.Services.Guide.Contract
{
    public class ContractService : IContractService
    {
        private readonly VietRoveDbContext _context;
        private readonly IFileStorageService _fileStorageService;

        public ContractService(
            VietRoveDbContext context,
            IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        public async Task<VietRove.Entities.Contract> CreateContractAsync(
            int userId,
            ContractViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            ValidateContractModel(model);

            var guide = await _context.GuideProfiles
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (guide == null)
            {
                throw new InvalidOperationException("Khong tim thay ho so huong dan vien.");
            }

            if (string.IsNullOrWhiteSpace(guide.FullName))
            {
                throw new InvalidOperationException("Ho so huong dan vien chua co ho ten.");
            }

            var contract = new VietRove.Entities.Contract
            {
                GuideId = guide.GuideId,
                CustomerName = model.CustomerName.Trim(),
                CustomerPhone = model.CustomerPhone.Trim(),
                Itinerary = model.Itinerary.Trim(),
                StartDate = model.StartDate.Date,
                EndDate = model.EndDate.Date,
                Fee = model.Fee,
                PdfFile = null,
                CreatedAt = DateTime.Now
            };

            string? pdfFile = null;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Contracts.Add(contract);
                await _context.SaveChangesAsync();

                byte[] pdfBytes = GenerateContractPdf(
                    guide,
                    model,
                    contract.ContractId
                );

                pdfFile = await _fileStorageService.SaveGeneratedPdfAsync(pdfBytes);

                contract.PdfFile = pdfFile;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return contract;
            }
            catch
            {
                if (string.IsNullOrWhiteSpace(pdfFile) == false)
                {
                    _fileStorageService.DeleteFile(pdfFile);
                }

                throw;
            }
        }

        private void ValidateContractModel(ContractViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.CustomerName))
            {
                throw new InvalidOperationException("Vui long nhap ten cong ty.");
            }

            if (model.CustomerName.Length > 150)
            {
                throw new InvalidOperationException("Ten cong ty khong duoc vuot qua 150 ky tu.");
            }

            if (string.IsNullOrWhiteSpace(model.CompanyAddress))
            {
                throw new InvalidOperationException("Vui long nhap dia chi cong ty.");
            }

            if (model.CompanyAddress.Length > 300)
            {
                throw new InvalidOperationException("Dia chi cong ty khong duoc vuot qua 300 ky tu.");
            }

            if (string.IsNullOrWhiteSpace(model.RepresentativeName))
            {
                throw new InvalidOperationException("Vui long nhap ten nguoi dai dien.");
            }

            if (model.RepresentativeName.Length > 150)
            {
                throw new InvalidOperationException("Ten nguoi dai dien khong duoc vuot qua 150 ky tu.");
            }

            if (string.IsNullOrWhiteSpace(model.CustomerPhone))
            {
                throw new InvalidOperationException("Vui long nhap so dien thoai cong ty.");
            }

            if (model.CustomerPhone.Length > 20)
            {
                throw new InvalidOperationException("So dien thoai khong duoc vuot qua 20 ky tu.");
            }

            if (string.IsNullOrWhiteSpace(model.Itinerary))
            {
                throw new InvalidOperationException("Vui long nhap lich trinh.");
            }

            if (model.Itinerary.Length > 5000)
            {
                throw new InvalidOperationException("Lich trinh khong duoc vuot qua 5000 ky tu.");
            }

            if (model.StartDate == default || model.EndDate == default)
            {
                throw new InvalidOperationException("Ngay hop dong khong hop le.");
            }

            if (model.EndDate.Date < model.StartDate.Date)
            {
                throw new InvalidOperationException("Ngay ket thuc phai bang hoac sau ngay bat dau.");
            }

            if (model.Fee <= 0 || model.Fee > 9999999999999999.99m)
            {
                throw new InvalidOperationException("Chi phi hop dong khong hop le.");
            }
        }

        private byte[] GenerateContractPdf(
            GuideProfile guide,
            ContractViewModel model,
            int contractId)
        {
            var culture = new CultureInfo("vi-VN");

            string contractNumber = "VR-" +
                DateTime.Now.ToString("yyyyMMdd") +
                "-" +
                contractId.ToString("D5");

            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(45);
                    page.PageColor(Colors.White);

                    page.DefaultTextStyle(style => style
                        .FontFamily("Noto Sans")
                        .FontSize(11)
                        .FontColor(Colors.Black));

                    page.Content().Column(column =>
                    {
                        column.Spacing(12);

                        column.Item().AlignCenter()
                            .Text("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM")
                            .Bold()
                            .FontSize(13);

                        column.Item().AlignCenter()
                            .Text("Độc lập - Tự do - Hạnh phúc")
                            .Bold()
                            .FontSize(12);

                        column.Item().PaddingTop(12).AlignCenter()
                            .Text("HỢP ĐỒNG THỎA THUẬN DỊCH VỤ HƯỚNG DẪN DU LỊCH")
                            .Bold()
                            .FontSize(15);

                        column.Item().AlignCenter()
                            .Text("Số hợp đồng: " + contractNumber)
                            .FontSize(10);

                        column.Item().Text(
                            "Căn cứ Bộ luật Dân sự năm 2015, Luật Du lịch năm 2017 " +
                            "và các quy định pháp luật có liên quan;"
                        );

                        column.Item().Text(
                            "Căn cứ nhu cầu sử dụng dịch vụ của Công ty và khả năng cung cấp " +
                            "dịch vụ của Hướng dẫn viên;"
                        );

                        column.Item().Text(
                            "Hôm nay, các bên thống nhất ký kết hợp đồng với các nội dung sau:"
                        );

                        column.Item().Text("BÊN A - ĐƠN VỊ LỮ HÀNH").Bold();

                        column.Item().Text("Tên công ty: " + model.CustomerName.Trim());
                        column.Item().Text("Mã số thuế: " + GetValue(model.TaxCode));
                        column.Item().Text("Địa chỉ: " + model.CompanyAddress.Trim());
                        column.Item().Text("Người đại diện: " + model.RepresentativeName.Trim());
                        column.Item().Text("Chức vụ: " + GetValue(model.RepresentativePosition));
                        column.Item().Text("Số điện thoại: " + model.CustomerPhone.Trim());
                        column.Item().Text("Email: " + GetValue(model.CompanyEmail));

                        column.Item().PaddingTop(5)
                            .Text("BÊN B - HƯỚNG DẪN VIÊN")
                            .Bold();

                        column.Item().Text("Họ và tên: " + guide.FullName);
                        column.Item().Text("Số thẻ hướng dẫn viên: " + GetValue(guide.CardNumber));
                        column.Item().Text("Số điện thoại: " + GetValue(guide.ContactPhone));
                        column.Item().Text("Email: " + GetValue(guide.ContactEmail));

                        AddArticle(
                            column,
                            "ĐIỀU 1. NỘI DUNG DỊCH VỤ",
                            "Bên B đồng ý cung cấp dịch vụ hướng dẫn du lịch theo yêu cầu " +
                            "và lịch trình đã được hai bên thống nhất. Bên B có trách nhiệm " +
                            "thực hiện công việc đúng phạm vi và bảo đảm chất lượng dịch vụ."
                        );

                        AddArticle(
                            column,
                            "ĐIỀU 2. THỜI GIAN VÀ LỊCH TRÌNH",
                            "Thời gian bắt đầu: " + model.StartDate.ToString("dd/MM/yyyy", culture) + ".\n" +
                            "Thời gian kết thúc: " + model.EndDate.ToString("dd/MM/yyyy", culture) + ".\n" +
                            "Lịch trình: " + model.Itinerary.Trim()
                        );

                        AddArticle(
                            column,
                            "ĐIỀU 3. CHI PHÍ VÀ THANH TOÁN",
                            "Tổng chi phí dịch vụ: " + model.Fee.ToString("N0", culture) + " VNĐ.\n" +
                            "Phương thức và thời hạn thanh toán do hai bên thỏa thuận " +
                            "và thực hiện theo quy định pháp luật."
                        );

                        AddArticle(
                            column,
                            "ĐIỀU 4. QUYỀN VÀ NGHĨA VỤ CỦA HƯỚNG DẪN VIÊN",
                            "Thực hiện đúng lịch trình và nội dung công việc đã thỏa thuận; " +
                            "bảo đảm tính chuyên nghiệp, trung thực và an toàn trong quá trình " +
                            "hướng dẫn; thông báo kịp thời cho Bên A khi có sự cố phát sinh."
                        );

                        AddArticle(
                            column,
                            "ĐIỀU 5. QUYỀN VÀ NGHĨA VỤ CỦA ĐƠN VỊ LỮ HÀNH",
                            "Cung cấp thông tin và các điều kiện cần thiết để Hướng dẫn viên " +
                            "thực hiện công việc; thanh toán đầy đủ, đúng hạn; thông báo kịp thời " +
                            "các thay đổi liên quan đến lịch trình và dịch vụ."
                        );

                        AddArticle(
                            column,
                            "ĐIỀU 6. THAY ĐỔI, HỦY BỎ VÀ CHẤM DỨT HỢP ĐỒNG",
                            "Mọi thay đổi về lịch trình, thời gian, chi phí hoặc nội dung dịch vụ " +
                            "phải được hai bên thống nhất. Việc hủy bỏ hoặc chấm dứt hợp đồng " +
                            "được thực hiện theo thỏa thuận của hai bên và quy định pháp luật."
                        );

                        AddArticle(
                            column,
                            "ĐIỀU 7. SỰ KIỆN BẤT KHẢ KHÁNG",
                            "Bên gặp sự kiện bất khả kháng phải thông báo cho bên còn lại trong " +
                            "thời gian sớm nhất. Hai bên phối hợp để hạn chế thiệt hại và thống " +
                            "nhất phương án xử lý phù hợp."
                        );

                        AddArticle(
                            column,
                            "ĐIỀU 8. VI PHẠM VÀ GIẢI QUYẾT TRANH CHẤP",
                            "Bên vi phạm nghĩa vụ phải chịu trách nhiệm theo thỏa thuận và quy " +
                            "định pháp luật. Tranh chấp được ưu tiên giải quyết thông qua thương " +
                            "lượng; trường hợp không đạt được thỏa thuận thì giải quyết theo " +
                            "quy định pháp luật."
                        );

                        AddArticle(
                            column,
                            "ĐIỀU 9. ĐIỀU KHOẢN CUỐI CÙNG",
                            "Hợp đồng có hiệu lực kể từ ngày hai bên ký. Hợp đồng được lập thành " +
                            "02 bản có giá trị như nhau, mỗi bên giữ 01 bản. Hai bên đã đọc, " +
                            "hiểu và đồng ý với toàn bộ nội dung hợp đồng."
                        );

                        column.Item().PaddingTop(25).Row(row =>
                        {
                            row.RelativeItem().AlignCenter().Column(side =>
                            {
                                side.Item().AlignCenter()
                                    .Text("ĐẠI DIỆN BÊN A")
                                    .Bold();

                                side.Item().AlignCenter()
                                    .Text("(Ký, ghi rõ họ tên)");

                                side.Item().PaddingTop(65).AlignCenter()
                                    .Text(model.RepresentativeName.Trim())
                                    .Bold();
                            });

                            row.RelativeItem().AlignCenter().Column(side =>
                            {
                                side.Item().AlignCenter()
                                    .Text("HƯỚNG DẪN VIÊN")
                                    .Bold();

                                side.Item().AlignCenter()
                                    .Text("(Ký, ghi rõ họ tên)");

                                side.Item().PaddingTop(65).AlignCenter()
                                    .Text(guide.FullName)
                                    .Bold();
                            });
                        });

                        column.Item().PaddingTop(15).AlignCenter()
                            .Text("Hợp đồng được tạo bởi hệ thống VietRove.")
                            .Italic()
                            .FontSize(9);
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Trang ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf();
        }

        private void AddArticle(
            ColumnDescriptor column,
            string title,
            string content)
        {
            column.Item().PaddingTop(5)
                .Text(title)
                .Bold();

            column.Item().Text(content);
        }

        private string GetValue(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Không cung cấp";
            }

            return value.Trim();
        }
    }
}
