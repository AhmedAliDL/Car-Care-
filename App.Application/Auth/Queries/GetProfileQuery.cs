using App.Application.Auth.Dtos;
using MediatR;

namespace App.Application.Auth.Queries;

public sealed record GetProfileQuery : IRequest<ProfileResponse>;
