namespace CashServiceBilling.Models;

public class ProductDetail
{
    public int ResourceId { get; init; }

    public int ProductId { get; init; }

    public int SportType { get; init; }

    public int BetType { get; init; }

    public int LeagueGroupId { get; init; }

    public int TicketType { get; init; }

    public string ToXml() => string.Format(
        "<ProductDetail ProductId=\"{0}\" Sporttype=\"{1}\" Bettype=\"{2}\" LeagueGroupId=\"{3}\" TicketType=\"{4}\"/>",
        ProductId,
        SportType,
        BetType,
        LeagueGroupId,
        TicketType);
}
