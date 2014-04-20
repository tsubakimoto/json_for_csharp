using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;

namespace itunes_aspx.Models
{
    public class UsersContext : DbContext
    {
        public UsersContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
    }

    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public string UserName { get; set; }
    }

    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "ユーザー名")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "現在のパスワード")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} の長さは {2} 文字以上である必要があります。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "新しいパスワード")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "新しいパスワードの確認入力")]
        [Compare("NewPassword", ErrorMessage = "新しいパスワードと確認のパスワードが一致しません。")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [Display(Name = "ユーザー名")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "パスワード")]
        public string Password { get; set; }

        [Display(Name = "このアカウントを記憶する")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        [Display(Name = "ユーザー名")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} の長さは {2} 文字以上である必要があります。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "パスワード")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "パスワードの確認入力")]
        [Compare("Password", ErrorMessage = "パスワードと確認のパスワードが一致しません。")]
        public string ConfirmPassword { get; set; }
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }
}
