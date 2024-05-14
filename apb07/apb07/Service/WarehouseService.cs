namespace apb07.Service;

public class WarehouseService
{
    public bool DoesAmountPositive(int amount)
    {
        return amount > 0;
    }
}