namespace S1.Model.Results;

/// <summary>Invoice number berikutnya untuk user yang login.</summary>
public class NextInvoiceResult
{
    public string InvoiceNo { get; set; } = string.Empty;
}

/// <summary>Hasil pembuatan transaksi.</summary>
public class CreateTransactionResult
{
    public string IdTransaction { get; set; } = string.Empty;
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public decimal TotalPrice { get; set; }
}

/// <summary>Satu baris riwayat transaksi untuk ditampilkan di tabel History.</summary>
public class TransactionHistoryItem
{
    public string InvoiceNo { get; set; } = string.Empty;
    /// <summary>Waktu transaksi dalam WIB (siap ditampilkan).</summary>
    public DateTime TransactionDate { get; set; }
    /// <summary>Nama lengkap kasir.</summary>
    public string Cashier { get; set; } = string.Empty;
    /// <summary>Total Qty seluruh item dalam transaksi.</summary>
    public int TotalQty { get; set; }
    /// <summary>Nama metode pembayaran.</summary>
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
}
