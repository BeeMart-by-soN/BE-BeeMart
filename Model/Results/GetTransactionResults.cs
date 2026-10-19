public class GetTransactionResult
{
    public string IdTransaction { get; set; } = string.Empty;
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public decimal TotalPrice { get; set; }
    public int IdPaymentMethod { get; set; }
}
