namespace BillingSystem.Models;

public class CompanySettings
{
    public string CompanyName { get; set; } = "My Company";
    public string OwnerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public string GSTNumber { get; set; } = string.Empty;
    public string PAN { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string IFSC { get; set; } = string.Empty;
    public string InvoicePrefix { get; set; } = "INV";
    public int NextInvoiceNumber { get; set; } = 1;
    public string Currency { get; set; } = "₹";
    public string TermsAndConditions { get; set; } = "Payment due within 15 days.";
}
