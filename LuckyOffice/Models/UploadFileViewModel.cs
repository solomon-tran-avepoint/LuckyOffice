using System.ComponentModel.DataAnnotations;

namespace LuckyOffice.Models
{
    public class UploadFileViewModel
    {
        [Display(Name = "Chọn tệp Excel")]
        public List<IFormFile> ExcelFiles { get; set; } = new List<IFormFile>();

        [Display(Name = "Hoặc nhập URL tải xuống")]
        public string FileUrls { get; set; } = string.Empty;
    }
}
