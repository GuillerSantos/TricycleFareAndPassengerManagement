using MediatR;
using TricycleFareAndPassengerManagement.Domain.Entities;

namespace TricycleFareAndPassengerManagement.Application.Features.Driver.Commands
{
    public class CreateDriverCommand : IRequest<int>
    {
        #region Properties

        public string FullName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string TricycleNumber { get; set; } = string.Empty;

        #endregion Properties
    }
}