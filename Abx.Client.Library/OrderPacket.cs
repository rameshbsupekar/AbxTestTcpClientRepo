namespace Abx.Client.Library;

public class OrderPacket
{
    public string Symbol { get; }
    public char BuySellIndicator { get; }
    public int Quantity { get; }
    public int Price { get; }
    public int Sequence { get; }

    public OrderPacket(string symbol, char buySellIndicator, int quantity, int price, int sequence)
    {
        Symbol = symbol;
        BuySellIndicator = buySellIndicator;
        Quantity = quantity;
        Price = price;
        Sequence = sequence;
    }
}

