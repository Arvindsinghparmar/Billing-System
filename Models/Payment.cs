namespace BillingSystem.Models;

public class Payment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string InvoiceId { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; } = PaymentMethod.Cash;
    public string Reference { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; } = DateTime.Today;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public enum PaymentMethod
{
    Cash,
    UPI,
    BankTransfer,
    Cheque,
    Card,
    Other
}
