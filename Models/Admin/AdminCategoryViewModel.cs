
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VietRove.Models.Admin
{
    public class AdminCategorySearchViewModel
    {
        public string? Search { get; set; }

        public string? CategoryType { get; set; }

        public int Page { get; set; } = 1;
    }

    public class AdminCategoryListItemViewModel
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string CategoryType { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }

    public class AdminCategoryResultViewModel
    {
        public List<AdminCategoryListItemViewModel> Items { get; set; }
            = new List<AdminCategoryListItemViewModel>();

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalRecords { get; set; }

        public int TotalPages { get; set; }
    }

    public class AdminCategoryCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục.")]
        [StringLength(150, ErrorMessage = "Tên danh mục không được vượt quá 150 ký tự.")]
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn loại danh mục.")]
        public string CategoryType { get; set; } = string.Empty;
    }

    public class AdminCategoryUpdateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục.")]
        [StringLength(150, ErrorMessage = "Tên danh mục không được vượt quá 150 ký tự.")]
        public string CategoryName { get; set; } = string.Empty;
    }
}
