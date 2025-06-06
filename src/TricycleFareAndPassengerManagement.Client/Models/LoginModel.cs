﻿using System.ComponentModel.DataAnnotations;

namespace TricycleFareAndPassengerManagement.Client.Models
{
    public class LoginModel
    {
        #region Properties

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }

        #endregion Properties
    }
}