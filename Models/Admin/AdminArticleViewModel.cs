
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace VietRove.Models.Admin
{
    public static class AdminArticleDefinitions
    {
        public const int News = 1;
        public const int TravelExperience = 2;
        public const int Announcement = 3;

        public const int Draft = 0;
        public const int Published = 1;
        public const int Hidden = 2;

        public static bool IsValidArticleType(int articleType)
        {
            return articleType == News ||
                   articleType == TravelExperience ||
                   articleType == Announcement;
        }

        public static bool IsValidStatus(int status)
        {
            return status == Draft ||
                   status == Published ||
                   status == Hidden;
        }
    }

    public class AdminArticleSearchViewModel
    {
        public string? Search { get; set; }

        public int? ArticleType { get; set; }

        public int? Status { get; set; }

        public int Page { get; set; } = 1;
    }

    public class AdminArticleListItemViewModel
    {
        public int ArticleId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Thumbnail { get; set; }

        public int ArticleType { get; set; }

        public int Status { get; set; }

        public string AuthorName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? PublishedAt { get; set; }
    }

    public class AdminArticleResultViewModel
    {
        public List<AdminArticleListItemViewModel> Items { get; set; }
            = new List<AdminArticleListItemViewModel>();

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalRecords { get; set; }

        public int TotalPages { get; set; }
    }

    public class AdminArticleDetailViewModel
    {
        public int ArticleId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string? Thumbnail { get; set; }

        public int ArticleType { get; set; }

        public int Status { get; set; }

        public string AuthorName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? PublishedAt { get; set; }
    }

    public class AdminArticleCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài viết.")]
        [StringLength(250, ErrorMessage = "Tiêu đề không được vượt quá 250 ký tự.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập nội dung bài viết.")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn loại bài viết.")]
        [Range(1, 3, ErrorMessage = "Loại bài viết không hợp lệ.")]
        public int ArticleType { get; set; }

        public IFormFile? Thumbnail { get; set; }
    }

    public class AdminArticleUpdateViewModel
    {
        public int ArticleId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài viết.")]
        [StringLength(250, ErrorMessage = "Tiêu đề không được vượt quá 250 ký tự.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập nội dung bài viết.")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn loại bài viết.")]
        [Range(1, 3, ErrorMessage = "Loại bài viết không hợp lệ.")]
        public int ArticleType { get; set; }

        public IFormFile? Thumbnail { get; set; }
    }
}
