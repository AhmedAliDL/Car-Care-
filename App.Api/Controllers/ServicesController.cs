using App.Application.ServicesCatalog.Commands.CreateService;
using App.Application.ServicesCatalog.Commands.DeactivateService;
using App.Application.ServicesCatalog.Commands.UpdateService;
using App.Application.ServicesCatalog.Dtos;
using App.Application.ServicesCatalog.Queries;
using App.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[ApiController]
[Route("api/services")]
public sealed class ServicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lists all active services in the catalog.
    /// </summary>
    /// <remarks>Anonymous access is allowed so the catalog can be browsed without signing in. Deactivated services are excluded.</remarks>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The active services with their duration and base price.</returns>
    /// <response code="200">Services returned.</response>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ServiceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetActiveServicesQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new service in the catalog.
    /// </summary>
    /// <remarks>
    /// Manager role only.
    /// Sample request:
    /// ```
    /// POST /api/services
    /// {
    ///   "name": "Oil Change",
    ///   "description": "Full synthetic oil and filter change",
    ///   "durationMinutes": 45,
    ///   "basePrice": 350.0
    /// }
    /// ```
    /// </remarks>
    /// <param name="req">Service details: name, description, duration in minutes and base price.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The created service.</returns>
    /// <response code="201">Service created.</response>
    /// <response code="400">Validation failed (e.g. non-positive duration or price).</response>
    /// <response code="403">Caller is not a Manager.</response>
    [Authorize(Roles = Roles.Manager)]
    [HttpPost]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateServiceCommand req, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(req, cancellationToken);
        return Created($"/api/services/{result.Id}", result);
    }

    /// <summary>
    /// Updates an existing service.
    /// </summary>
    /// <remarks>
    /// Manager role only. Updates catalog metadata only; already-booked appointments keep their snapshotted prices.
    /// </remarks>
    /// <param name="id">The service id (overrides any id in the body).</param>
    /// <param name="req">New service details: name, description, duration in minutes and base price.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The updated service.</returns>
    /// <response code="200">Service updated.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="403">Caller is not a Manager.</response>
    /// <response code="404">No service with this id.</response>
    [Authorize(Roles = Roles.Manager)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateServiceCommand req,
        CancellationToken cancellationToken)
    {
        req.Id = id;
        var result = await _mediator.Send(req, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deactivates a service so it can no longer be booked.
    /// </summary>
    /// <remarks>
    /// Manager role only. Soft delete: existing appointments referencing the service are unaffected.
    /// </remarks>
    /// <param name="id">The service id to deactivate.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The deactivated service.</returns>
    /// <response code="200">Service deactivated.</response>
    /// <response code="403">Caller is not a Manager.</response>
    /// <response code="404">No service with this id.</response>
    [Authorize(Roles = Roles.Manager)]
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeactivateServiceCommand { Id = id }, cancellationToken);
        return Ok(result);
    }
}
