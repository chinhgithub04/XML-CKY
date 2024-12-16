using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BTCK_CNXML.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage ="Tên không được để trống!")]
        public string Name { get; set; }
        [Required]
        [EmailAddress(ErrorMessage ="Định dạng Email không hợp lệ!")]
        public string Email { get; set; }
        [Required]
        [Phone(ErrorMessage ="Định dạng số điện thoại không hợp lệ!")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Mật khẩu không được để trống!")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Xác nhận lại mật khẩu!")]
        [Compare("Password", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp!")]
        public string ConfirmPassword { get; set; }
        public string? Role { get; set;}
    }
}
