using App.Application.Services;
using App.Domain.Enums;
using App.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace App.Tests.Domain;

public class AppointmentStatusMachineTests
{
    [Theory]
    [InlineData(AppointmentStatus.Requested, AppointmentStatus.Confirmed, true)]
    [InlineData(AppointmentStatus.Requested, AppointmentStatus.Cancelled, true)]
    [InlineData(AppointmentStatus.Requested, AppointmentStatus.Completed, false)]
    [InlineData(AppointmentStatus.Confirmed, AppointmentStatus.InService, true)]
    [InlineData(AppointmentStatus.Confirmed, AppointmentStatus.Cancelled, true)]
    [InlineData(AppointmentStatus.InService, AppointmentStatus.Completed, true)]
    [InlineData(AppointmentStatus.InService, AppointmentStatus.NoShow, true)]
    [InlineData(AppointmentStatus.InService, AppointmentStatus.Cancelled, false)]
    [InlineData(AppointmentStatus.Completed, AppointmentStatus.Cancelled, false)]
    [InlineData(AppointmentStatus.Cancelled, AppointmentStatus.Requested, false)]
    [InlineData(AppointmentStatus.NoShow, AppointmentStatus.Completed, false)]
    public void Transition_matrix_matches_br08(AppointmentStatus from, AppointmentStatus to, bool allowed)
    {
        AppointmentStatusMachine.CanTransition(from, to).Should().Be(allowed);
    }

    [Fact]
    public void Invalid_transition_throws_domain_exception()
    {
        var act = () => AppointmentStatusMachine.EnsureCanTransition(AppointmentStatus.Requested, AppointmentStatus.Completed);
        act.Should().Throw<DomainException>().WithMessage("*Requested*Completed*");
    }

    [Fact]
    public void Overlap_formula_detects_partial_overlap()
    {
        var existingStart = new DateTime(2026, 10, 15, 10, 0, 0, DateTimeKind.Utc);
        var newStart = new DateTime(2026, 10, 15, 10, 30, 0, DateTimeKind.Utc);
        AppointmentStatusMachine.Overlaps(existingStart, 60, newStart, 60).Should().BeTrue();
    }
}
