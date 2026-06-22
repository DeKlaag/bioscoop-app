using bioscoop_app.Models;
using Xunit;

namespace bioscoop_app.Tests;

public class ReservationModelTests
{
    [Theory]
    [InlineData("cancelled", true)]
    [InlineData("CANCELLED", true)]
    [InlineData("Cancelled", true)]
    [InlineData("confirmed", false)]
    [InlineData(null, false)]
    public void IsCancelled_IsTrueOnlyForCancelledStatus_CaseInsensitive(string? status, bool expected)
    {
        var reservation = new ReservationModel { Status = status };

        Assert.Equal(expected, reservation.IsCancelled);
    }

    [Fact]
    public void IsUpcoming_IsTrueForFutureStartTime_AndFalseForPast()
    {
        var future = new ReservationModel { StartTimeUtc = DateTime.UtcNow.AddHours(1) };
        var past = new ReservationModel { StartTimeUtc = DateTime.UtcNow.AddHours(-1) };

        Assert.True(future.IsUpcoming);
        Assert.False(past.IsUpcoming);
    }

    [Fact]
    public void IsCheckedIn_ReflectsWhetherCheckInTimestampIsSet()
    {
        var notCheckedIn = new ReservationModel { CheckedInAtUtc = null };
        var checkedIn = new ReservationModel { CheckedInAtUtc = DateTime.UtcNow };

        Assert.False(notCheckedIn.IsCheckedIn);
        Assert.True(checkedIn.IsCheckedIn);
    }

    [Fact]
    public void SeatsSummary_JoinsSeatLabelsWithCommas()
    {
        var reservation = new ReservationModel
        {
            Seats =
            [
                new ReservationSeatModel { RowLabel = "A", SeatNumber = 1 },
                new ReservationSeatModel { RowLabel = "A", SeatNumber = 2 },
                new ReservationSeatModel { RowLabel = "B", SeatNumber = 5 },
            ],
        };

        Assert.Equal("A1, A2, B5", reservation.SeatsSummary);
    }

    [Fact]
    public void HallDisplay_FormatsHallNumber()
    {
        var reservation = new ReservationModel { HallNumber = 7 };

        Assert.Equal("Zaal 7", reservation.HallDisplay);
    }

    [Fact]
    public void SeatLabel_CombinesRowLabelAndSeatNumber()
    {
        var seat = new ReservationSeatModel { RowLabel = "C", SeatNumber = 12 };

        Assert.Equal("C12", seat.Label);
    }
}
