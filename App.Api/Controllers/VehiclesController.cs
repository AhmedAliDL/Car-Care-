using App.Application.Vehicles.Commands.CreateVehicle;
using App.Application.Vehicles.Dtos;
using App.Application.Vehicles.Queries.GetMyVehicles;
using App.Application.Vehicles.Queries.GetVehicleById;
using App.Application.Vehicles.Queries.GetVehicleServiceHistory;
using App.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[ApiController]
[Route("api/vehicles")]
public sealed class VehiclesController : ControllerBase
{
    private readonly IMediator _mediator;

    public VehiclesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Adds a vehicle to the authenticated customer's garage.
    /// </summary>
    /// <remarks>
    /// Customer role only. The owner is taken from the access token. VIN must be 17 characters and cannot contain I, O or Q.
    /// Sample request:
    /// ```
    /// POST /api/vehicles
    /// {
    ///   "plateNumber": "ABC-1234",
    ///   "vin": "1HGCM82633A004352",
    ///   "make": "Toyota",
    ///   "model": "Corolla",
    ///   "year": 2020
    /// }
    /// ```
    /// </remarks>
    /// <param name="req">Vehicle details: plate number, VIN, make, model and year.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The created vehicle.</returns>
    /// <response code="201">Vehicle created.</response>
    /// <response code="400">Validation failed (e.g. invalid VIN or plate number).</response>
    /// <response code="401">Missing or invalid access token.</response>
    [Authorize(Roles = Roles.Customer)]
    [HttpPost]
    [ProducesResponseType(typeof(VehicleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateVehicleCommand req, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(req, cancellationToken);
        return Created($"/api/vehicles/{result.Id}", result);
    }

    /// <summary>
    /// Lists all vehicles owned by the authenticated customer.
    /// </summary>
    /// <remarks>Customer role only.</remarks>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The current customer's vehicles.</returns>
    /// <response code="200">Vehicles returned.</response>
    /// <response code="401">Missing or invalid access token.</response>
    [Authorize(Roles = Roles.Customer)]
    [HttpGet("my")]
    [ProducesResponseType(typeof(IReadOnlyList<VehicleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyVehiclesQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a single vehicle by id.
    /// </summary>
    /// <remarks>
    /// Available to Customer, Staff and Manager. A customer may only access their own vehicle; other owners yield <c>403</c>.
    /// </remarks>
    /// <param name="id">The vehicle id.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The requested vehicle.</returns>
    /// <response code="200">Vehicle returned.</response>
    /// <response code="403">The vehicle belongs to another customer.</response>
    /// <response code="404">No vehicle with this id.</response>
    [Authorize(Roles = $"{Roles.Customer},{Roles.Staff},{Roles.Manager}")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VehicleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetVehicleByIdQuery { Id = id }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets the service history for a vehicle.
    /// </summary>
    /// <remarks>
    /// Available to Customer, Staff and Manager. Returns appointments with their snapshotted service lines and prices.
    /// A customer may only view history for their own vehicle.
    /// </remarks>
    /// <param name="id">The vehicle id.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The vehicle's service history entries.</returns>
    /// <response code="200">History returned.</response>
    /// <response code="403">The vehicle belongs to another customer.</response>
    /// <response code="404">No vehicle with this id.</response>
    [Authorize(Roles = $"{Roles.Customer},{Roles.Staff},{Roles.Manager}")]
    [HttpGet("{id:guid}/service-history")]
    [ProducesResponseType(typeof(IReadOnlyList<VehicleServiceHistoryItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetServiceHistory(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetVehicleServiceHistoryQuery { VehicleId = id }, cancellationToken);
        return Ok(result);
    }
}
