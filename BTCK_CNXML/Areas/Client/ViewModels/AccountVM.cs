using BTCK_CNXML.Models;
using System.ComponentModel.DataAnnotations;

namespace BTCK_CNXML.Areas.Client.ViewModels
{
    public class AccountVM
    {
        [Required]
        public User User { get; set; }
        [Required]
        public ChangePasswordVM ChangePassword { get; set; }
    }
}
