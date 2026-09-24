using App.Application.Appointments.Commands.CancelAppoint;
using App.Application.Appointments.Commands.CreateAppointment;
using App.Application.Appointments.Commands.UpdateAppointmentStatus;
using App.Application.Appointments.Dtos;
using App.Application.Appointments.Queries;
using App.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[ApiController]
[Route("api/appointments")]
public sealed class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppointmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Books a new appointment for one of the customer's vehicles.
    /// </summary>
    /// <remarks>
    /// Customer role only. Enforces: the vehicle must belong to the caller, all services must be active,
    /// the date must be in the future, and the requested slot must not overlap another active appointment
    /// for the same vehicle. Prices and durations are snapshotted from the catalog at booking time.
    /// Sample request:
    /// ```
    /// POST /api/appointments
    /// {
    ///   "vehicleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "serviceIds": ["11111111-1111-1111-1111-111111111111"],
    ///   "appointmentDate": "2026-10-01T10:00:00Z",
    ///   "notes": "Please call on arrival"
    /// }
    /// ```
    /// </remarks>
    /// <param name="req">Booking details: vehicle id, service ids, appointment start (UTC) and optional notes.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The created appointment with snapshotted services and total price.</returns>
    /// <response code="201">Appointment created.</response>
    /// <response code="400">Validation failed (e.g. past date, inactive service, duplicate service ids).</response>
    /// <response code="403">The vehicle belongs to another customer.</response>
    /// <response code="404">Vehicle or one of the services does not exist.</response>
    /// <response code="409">The slot overlaps an existing appointment for this vehicle.</response>
    [Authorize(Roles = Roles.Customer)]
    [HttpPost]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentCommand req, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(req, cancellationToken);
        return Created($"/api/appointments/{result.Id}", result);
    }

    /// <summary>
    /// Lists all appointments belonging to the authenticated customer.
    /// </summary>
    /// <remarks>Customer role only.</remarks>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The current customer's appointments.</returns>
    /// <response code="200">Appointments returned.</response>
    /// <response code="401">Missing or invalid access token.</response>
    [Authorize(Roles = Roles.Customer)]
    [HttpGet("my")]
    [ProducesResponseType(typeof(IReadOnlyList<AppointmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyAppointmentsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cancels one of the customer's appointments.
    /// </summary>
    /// <remarks>
    /// Customer role only. Only the owning customer may cancel, and only while the appointment is not
    /// already InService or Completed. Cancelled appointments cannot be cancelled again.
    /// </remarks>
    /// <param name="id">The appointment id.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The cancelled appointment.</returns>
    /// <response code="200">Appointment cancelled.</response>
    /// <response code="400">The appointment is in a state that cannot be cancelled.</response>
    /// <response code="403">The appointment belongs to another customer.</response>
    /// <response code="404">No appointment with this id.</response>
    [Authorize(Roles = Roles.Customer)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelAppointmentCommand { Id = id }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Advances an appointment to a new status.
    /// </summary>
    /// <remarks>
    /// Staff and Manager roles only. Transitions must follow the allowed state machine
    /// (Requested → Confirmed → InService → Completed; Requested/Confirmed → Cancelled or NoShow).
    /// An invalid transition returns <c>400</c>.
    /// Sample request:
    /// ```
    /// PUT /api/appointments/{id}/status
    /// { "newStatus": "Confirmed" }
    /// ```
    /// </remarks>
    /// <param name="id">The appointment id (overrides any id in the body).</param>
    /// <param name="req">The target status to transition to.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The updated appointment.</returns>
    /// <response code="200">Status updated.</response>
    /// <response code="400">The transition is not allowed by the state machine.</response>
    /// <response code="403">Caller is not Staff or Manager.</response>
    /// <response code="404">No appointment with this id.</response>
    [Authorize(Roles = $"{Roles.Staff},{Roles.Manager}")]
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateAppointmentStatusCommand req,
        CancellationToken cancellationToken)
    {
        req.Id = id;
        var result = await _mediator.Send(req, cancellationToken);
        return Ok(result);
    }
}
