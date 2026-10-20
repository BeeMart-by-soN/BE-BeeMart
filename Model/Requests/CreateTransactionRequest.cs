using System.ComponentModel.DataAnnotations;

namespace S1.Model.Requests;

public class CreateTransactionRequest
{
    [Required(ErrorMessage = "Metode pembayaran wajib dipilih")]
    public int PaymentMethodId { get; set; }

    [Required(ErrorMessage = "Item transaksi wajib diisi")]
    public List<TransactionItemRequest> Items { get; set; } = new();
}

public class TransactionItemRequest
{
    [Required(ErrorMessage = "ProductId wajib diisi")]
    public string ProductId { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Qty minimal 1")]
    public int Qty { get; set; }
}
