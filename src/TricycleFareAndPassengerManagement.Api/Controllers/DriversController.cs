using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TricycleFareAndPassengerManagement.Application.Common.DTOs;
using TricycleFareAndPassengerManagement.Application.Features.Driver.Commands;
using TricycleFareAndPassengerManagement.Application.Features.Driver.Queries;

namespace TricycleFareAndPassengerManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriverController(IMediator _mediator) : ControllerBase
    {
        #region Public Methods

        [HttpPost("create")]
        public async Task<ActionResult<int>> CreateDriver([FromBody] CreateDriverCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("getall")]
        public async Task<ActionResult<List<DriverDto>>> GetAllDrivers()
        {
            var query = new GetAllDriversQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<ActionResult<DriverDto>> GetDriverById(int id)
        {
            var query = new GetDriverByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound($"Driver with ID {id} not found.");

            return Ok(result);
        }

        [HttpPut("update/{id}")]
        public async Task<ActionResult<bool>> UpdateDriver(int id, [FromBody] UpdateDriverCommand command)
        {
            if (id != command.Id)
                return BadRequest("ID in URL does not match ID in request body.");

            var result = await _mediator.Send(command);

            if (!result)
                return NotFound($"Driver with ID {id} not found.");

            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<bool>> DeleteDriver(int id)
        {
            var command = new DeleteDriverCommand { Id = id };
            var result = await _mediator.Send(command);

            if (!result)
                return NotFound($"Driver with ID {id} not found.");

            return Ok(result);
        }

        #endregion Public Methods
    }
}