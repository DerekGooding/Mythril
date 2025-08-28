namespace Mythril.Data;

public record struct Item(string Name, string Description)
{
    public int Quantity { get; set; } = 0;
}
