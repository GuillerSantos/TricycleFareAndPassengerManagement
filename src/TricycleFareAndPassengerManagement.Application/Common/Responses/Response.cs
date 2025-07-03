using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TricycleFareAndPassengerManagement.Application.Common.Responses
{
    public sealed record Response(
        bool Success = false,
        string ErrorMessage = "",
        string Message = ""
        );
}