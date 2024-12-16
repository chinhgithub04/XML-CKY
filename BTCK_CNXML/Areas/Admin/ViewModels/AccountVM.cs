using System.ComponentModel.DataAnnotations;

namespace BTCK_CNXML.Areas.Admin.ViewModels
{
    public class AccountVM
    {
        [Required(ErrorMessage = "Tên không được để trống!")]
        public string Name { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ!")]
        public string Email { get; set; }
        [Required]
        [Phone(ErrorMessage = "Định dạng số điện thoại không hợp lệ!")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Mật khẩu không được để trống!")]
        public string Password { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Role { get; set; }
    }
}
