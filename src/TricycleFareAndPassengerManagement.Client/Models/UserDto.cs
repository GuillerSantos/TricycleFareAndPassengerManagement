﻿namespace TricycleFareAndPassengerManagement.Client.Models
{
    public class UserDto
    {
        #region Properties

        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();

        #endregion Properties
    }
}