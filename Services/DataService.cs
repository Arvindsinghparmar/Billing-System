using BillingSystem.Models;
using System.Text.Json;

namespace BillingSystem.Services;

public class DataService
{
    private readonly string _dataFolder;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public DataService()
    {
        _dataFolder = @"C:\BillingSystemData";
        Directory.CreateDirectory(_dataFolder);
        Directory.CreateDirectory(Path.Combine(_dataFolder, "invoices"));
    }

    // ─── Generic Helpers ──────────────────────────────────────────────────────

    private string FilePath(string name) => Path.Combine(_dataFolder, $"{name}.json");

    private List<T> LoadList<T>(string name)
    {
        var path = FilePath(name);
        if (!File.Exists(path)) return new List<T>();
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
    }

    private void SaveList<T>(string name, List<T> list)
    {
        var path = FilePath(name);
        File.WriteAllText(path, JsonSerializer.Serialize(list, _jsonOptions));
    }

    // ─── Company Settings ─────────────────────────────────────────────────────

    public CompanySettings GetSettings()
    {
        var path = FilePath("settings");
        if (!File.Exists(path)) return new CompanySettings();
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<CompanySettings>(json, _jsonOptions) ?? new CompanySettings();
    }

    public void SaveSettings(CompanySettings settings)
    {
        File.WriteAllText(FilePath("settings"), JsonSerializer.Serialize(settings, _jsonOptions));
    }

    // ─── Customers ────────────────────────────────────────────────────────────

    public List<Customer> GetCustomers() => LoadList<Customer>("customers");

    public Customer? GetCustomer(string id) => GetCustomers().FirstOrDefault(c => c.Id == id);

    public void SaveCustomer(Customer customer)
    {
        var list = GetCustomers();
        var idx = list.FindIndex(c => c.Id == customer.Id);
        if (idx >= 0) list[idx] = customer;
        else list.Add(customer);
        SaveList("customers", list);
    }

    public void DeleteCustomer(string id)
    {
        var list = GetCustomers();
        list.RemoveAll(c => c.Id == id);
        SaveList("customers", list);
    }

    // ─── Products ─────────────────────────────────────────────────────────────

    public List<Product> GetProducts() => LoadList<Product>("products");

    public Product? GetProduct(string id) => GetProducts().FirstOrDefault(p => p.Id == id);

    public void SaveProduct(Product product)
    {
        var list = GetProducts();
        var idx = list.FindIndex(p => p.Id == product.Id);
        if (idx >= 0) list[idx] = product;
        else list.Add(product);
        SaveList("products", list);
    }

    public void DeleteProduct(string id)
    {
        var list = GetProducts();
        list.RemoveAll(p => p.Id == id);
        SaveList("products", list);
    }

    // ─── Invoices ─────────────────────────────────────────────────────────────

    public List<Invoice> GetInvoices() => LoadList<Invoice>("invoices_index");

    public Invoice? GetInvoice(string id)
    {
        var path = Path.Combine(_dataFolder, "invoices", $"{id}.json");
        if (!File.Exists(path)) return null;
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<Invoice>(json, _jsonOptions);
    }

    public Invoice CreateInvoice(Invoice invoice)
    {
        var settings = GetSettings();
        invoice.InvoiceNumber = $"{settings.InvoicePrefix}-{settings.NextInvoiceNumber:D4}";
        settings.NextInvoiceNumber++;
        SaveSettings(settings);

        // Save full invoice to individual file
        var path = Path.Combine(_dataFolder, "invoices", $"{invoice.Id}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(invoice, _jsonOptions));

        // Update index
        UpdateInvoiceIndex(invoice);
        return invoice;
    }

    public void UpdateInvoice(Invoice invoice)
    {
        var path = Path.Combine(_dataFolder, "invoices", $"{invoice.Id}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(invoice, _jsonOptions));
        UpdateInvoiceIndex(invoice);
    }

    public void DeleteInvoice(string id)
    {
        var path = Path.Combine(_dataFolder, "invoices", $"{id}.json");
        if (File.Exists(path)) File.Delete(path);

        var list = GetInvoices();
        list.RemoveAll(i => i.Id == id);
        SaveList("invoices_index", list);
    }

    private void UpdateInvoiceIndex(Invoice invoice)
    {
        var list = GetInvoices();
        // Store a lightweight copy in index (no items)
        var indexEntry = new Invoice
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            CustomerId = invoice.CustomerId,
            CustomerName = invoice.CustomerName,
            CustomerPhone = invoice.CustomerPhone,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            Status = invoice.Status,
            PaidAmount = invoice.PaidAmount,
            CreatedAt = invoice.CreatedAt,
            Items = invoice.Items  // kept for totals
        };
        var idx = list.FindIndex(i => i.Id == invoice.Id);
        if (idx >= 0) list[idx] = indexEntry;
        else list.Add(indexEntry);
        SaveList("invoices_index", list);
    }

    // ─── Payments ─────────────────────────────────────────────────────────────

    public List<Payment> GetPayments() => LoadList<Payment>("payments");

    public List<Payment> GetPaymentsForInvoice(string invoiceId) =>
        GetPayments().Where(p => p.InvoiceId == invoiceId).ToList();

    public void SavePayment(Payment payment)
    {
        var list = GetPayments();
        var idx = list.FindIndex(p => p.Id == payment.Id);
        if (idx >= 0) list[idx] = payment;
        else list.Add(payment);
        SaveList("payments", list);

        // Update invoice paid amount + status
        var invoice = GetInvoice(payment.InvoiceId);
        if (invoice != null)
        {
            var totalPaid = list.Where(p => p.InvoiceId == invoice.Id).Sum(p => p.Amount);
            invoice.PaidAmount = totalPaid;
            invoice.Status = totalPaid >= invoice.GrandTotal ? InvoiceStatus.Paid
                : totalPaid > 0 ? InvoiceStatus.PartiallyPaid
                : InvoiceStatus.Unpaid;
            UpdateInvoice(invoice);
        }
    }

    public void DeletePayment(string id)
    {
        var list = GetPayments();
        var payment = list.FirstOrDefault(p => p.Id == id);
        list.RemoveAll(p => p.Id == id);
        SaveList("payments", list);

        if (payment != null)
        {
            var invoice = GetInvoice(payment.InvoiceId);
            if (invoice != null)
            {
                var totalPaid = list.Where(p => p.InvoiceId == invoice.Id).Sum(p => p.Amount);
                invoice.PaidAmount = totalPaid;
                invoice.Status = totalPaid >= invoice.GrandTotal ? InvoiceStatus.Paid
                    : totalPaid > 0 ? InvoiceStatus.PartiallyPaid
                    : InvoiceStatus.Unpaid;
                UpdateInvoice(invoice);
            }
        }
    }

    // ─── Dashboard Stats ──────────────────────────────────────────────────────

    public DashboardStats GetDashboardStats()
    {
        var invoices = GetInvoices();
        var payments = GetPayments();
        var now = DateTime.Now;

        return new DashboardStats
        {
            TotalCustomers = GetCustomers().Count(c => c.IsActive),
            TotalProducts = GetProducts().Count(p => p.IsActive),
            TotalInvoices = invoices.Count,
            TotalRevenue = invoices.Sum(i => i.GrandTotal),
            TotalCollected = payments.Sum(p => p.Amount),
            PendingAmount = invoices.Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled)
                                    .Sum(i => i.BalanceDue),
            OverdueInvoices = invoices.Count(i => i.DueDate < DateTime.Today
                                                && i.Status != InvoiceStatus.Paid
                                                && i.Status != InvoiceStatus.Cancelled),
            ThisMonthRevenue = invoices.Where(i => i.InvoiceDate.Month == now.Month && i.InvoiceDate.Year == now.Year)
                                       .Sum(i => i.GrandTotal),
            RecentInvoices = invoices.OrderByDescending(i => i.CreatedAt).Take(5).ToList()
        };
    }
}

public class DashboardStats
{
    public int TotalCustomers { get; set; }
    public int TotalProducts { get; set; }
    public int TotalInvoices { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal PendingAmount { get; set; }
    public int OverdueInvoices { get; set; }
    public decimal ThisMonthRevenue { get; set; }
    public List<Invoice> RecentInvoices { get; set; } = new();
}
