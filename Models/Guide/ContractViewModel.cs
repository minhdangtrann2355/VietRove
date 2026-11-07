
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VietRove.Models.Guide
{
    public class ContractViewModel : IValidatableObject
    {
        // thong tin cong ty lu hanh
        [Required(ErrorMessage = "Vui long nhap ten cong ty.")]
        [StringLength(150, ErrorMessage = "Ten cong ty khong duoc vuot qua 150 ky tu.")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui long nhap dia chi cong ty.")]
        [StringLength(300, ErrorMessage = "Dia chi khong duoc vuot qua 300 ky tu.")]
        public string CompanyAddress { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Ma so thue khong duoc vuot qua 20 ky tu.")]
        public string? TaxCode { get; set; }

        [Required(ErrorMessage = "Vui long nhap ten nguoi dai dien.")]
        [StringLength(150, ErrorMessage = "Ten nguoi dai dien khong duoc vuot qua 150 ky tu.")]
        public string RepresentativeName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Chuc vu khong duoc vuot qua 100 ky tu.")]
        public string? RepresentativePosition { get; set; }

        [Required(ErrorMessage = "Vui long nhap so dien thoai cong ty.")]
        [Phone(ErrorMessage = "So dien thoai khong hop le.")]
        [StringLength(20, ErrorMessage = "So dien thoai khong duoc vuot qua 20 ky tu.")]
        public string CustomerPhone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email khong hop le.")]
        [StringLength(254, ErrorMessage = "Email khong duoc vuot qua 254 ky tu.")]
        public string? CompanyEmail { get; set; }

        // thong tin hop dong
        [Required(ErrorMessage = "Vui long nhap lich trinh.")]
        [StringLength(5000, ErrorMessage = "Lich trinh khong duoc vuot qua 5000 ky tu.")]
        public string Itinerary { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui long chon ngay bat dau.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Vui long chon ngay ket thuc.")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Vui long nhap chi phi.")]
        [Range(
            typeof(decimal),
            "0.01",
            "9999999999999999.99",
            ErrorMessage = "Chi phi phai lon hon 0 va khong vuot qua gioi han cho phep."
        )]
        public decimal Fee { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate == default)
            {
                yield return new ValidationResult(
                    "Vui long chon ngay bat dau.",
                    new[] { nameof(StartDate) }
                );
            }

            if (EndDate == default)
            {
                yield return new ValidationResult(
                    "Vui long chon ngay ket thuc.",
                    new[] { nameof(EndDate) }
                );
            }

            if (StartDate != default &&
                EndDate != default &&
                EndDate.Date < StartDate.Date)
            {
                yield return new ValidationResult(
                    "Ngay ket thuc phai bang hoac sau ngay bat dau.",
                    new[] { nameof(EndDate) }
                );
            }
        }
    }
}
