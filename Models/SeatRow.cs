namespace bioscoop_app.Models;

// A group of seats sharing the same row label, e.g. all "A" seats.
public class SeatRow : List<SeatModel>
{
    public string RowLabel { get; }

    public SeatRow(string rowLabel, IEnumerable<SeatModel> seats) : base(seats)
        => RowLabel = rowLabel;
}
