using System.ComponentModel.DataAnnotations;

namespace BTCK_CNXML.Areas.Client.ViewModels
{
    public class ChangePasswordVM
    {
        [Required(ErrorMessage = "Mật khẩu cũ không được để trống")]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "Xác nhận mật khẩu mới không được để trống")]
        public string ConfirmNewPassword { get; set; }
    }
}