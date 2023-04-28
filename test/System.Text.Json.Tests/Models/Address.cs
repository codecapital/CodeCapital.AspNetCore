namespace System.Text.Json.Tests.Models;

public class Address
{
    public string? Street { get; set; }
    public bool IsLocal { get; set; }
    public Delivery Delivery { get; set; }

    public Address(string? street, bool isLocal, string? deliveryType)
    {
        Street = street;
        IsLocal = isLocal;
        Delivery = new Delivery(deliveryType);
    }
}