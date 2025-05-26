using MediatR;
using TricycleFareAndPassengerManagement.Domain.Interfaces;

namespace TricycleFareAndPassengerManagement.Application.Features.Driver.Commands
{
    public class CreateDriverCommandHandler : IRequestHandler<CreateDriverCommand, int>
    {
        #region Fields

        private readonly IAppDbContext _context;

        #endregion Fields

        #region Public Constructors

        public CreateDriverCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        #endregion Public Constructors

        #region Public Methods

        public async Task<int> Handle(CreateDriverCommand request, CancellationToken cancellationToken)
        {
            var driver = new Domain.Entities.Driver
            {
                FullName = request.FullName,
                LicenseNumber = request.LicenseNumber,
                PhoneNumber = request.PhoneNumber,
                TricycleNumber = request.TricycleNumber,
            
            };

            _context.Drivers.Add(driver);
            await _context.SaveChangesAsync(cancellationToken);

            return driver.Id;
        }

        #endregion Public Methods
    }
}