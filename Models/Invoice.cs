namespace BillingSystem.Models;

public class InvoiceItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string HSNCode { get; set; } = string.Empty;
    public string Unit { get; set; } = "Pcs";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal GSTPercent { get; set; }
    public decimal TaxableAmount => (UnitPrice * Quantity) - Discount;
    public decimal GSTAmount => Math.Round(TaxableAmount * GSTPercent / 100, 2);
    public decimal TotalAmount => TaxableAmount + GSTAmount;
}

public class Invoice
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string CustomerGST { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; } = DateTime.Today;
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(15);
    public List<InvoiceItem> Items { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;
    public decimal PaidAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public decimal SubTotal => Items.Sum(x => x.TaxableAmount);
    public decimal TotalGST => Items.Sum(x => x.GSTAmount);
    public decimal GrandTotal => Items.Sum(x => x.TotalAmount);
    public decimal BalanceDue => GrandTotal - PaidAmount;
}

public enum InvoiceStatus
{
    Unpaid,
    PartiallyPaid,
    Paid,
    Cancelled
}
