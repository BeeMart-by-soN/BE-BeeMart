using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using S1.Data;
using S1.Model;
using S1.Model.Requests;
using S1.Model.Results;
using S1.Services;

namespace S1.Controller;

[Authorize]
[Route("api/v1/transactions")]
[ApiController]
public class TransactionController : ControllerBase
{
    private readonly AppDbContext _context;

    public TransactionController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /api/v1/transactions/next-invoice
    // Preview invoice berikutnya untuk user yang login (format [5-digit IdUser][ddmmyy][3-digit running global/hari]).
    [HttpGet("next-invoice")]
    public async Task<ActionResult<ApiResponse<NextInvoiceResult>>> GetNextInvoice()
    {
        var (idUser, _) = GetCurrentUser();
        if (idUser == null) return Unauthorized(ApiResponse<NextInvoiceResult>.Fail("Unauthorized"));

        var invoiceNo = await ComputeInvoiceNoAsync(idUser);
        return Ok(ApiResponse<NextInvoiceResult>.Ok(new NextInvoiceResult { InvoiceNo = invoiceNo }));
    }

    // POST: /api/v1/transactions
    // Buat transaksi baru. Server menghitung ulang harga/diskon/subtotal/total dari DB,
    // memvalidasi stok, mengurangi stok, dan menyimpan dalam satu DB transaction.
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateTransactionResult>>> Create([FromBody] CreateTransactionRequest req)
    {
        var (idUser, _) = GetCurrentUser();
        if (idUser == null) return Unauthorized(ApiResponse<CreateTransactionResult>.Fail("Unauthorized"));

        // Keranjang tidak boleh kosong
        if (req.Items == null || req.Items.Count == 0)
        {
            return BadRequest(ApiResponse<CreateTransactionResult>.Fail("Keranjang masih kosong. Tambahkan produk terlebih dahulu."));
        }

        // Validasi setiap item: qty minimal 1, productId tidak kosong
        var invalid = req.Items.FirstOrDefault(i => i.Qty < 1 || string.IsNullOrWhiteSpace(i.ProductId));
        if (invalid != null)
        {
            return BadRequest(ApiResponse<CreateTransactionResult>.Fail("Qty setiap item minimal 1 dan productId tidak boleh kosong."));
        }

        // Validasi payment method aktif
        var paymentMethodExists = await _context.LtPaymentMethod
            .AnyAsync(m => m.IdPaymentMethod == req.PaymentMethodId && m.Stsrc);
        if (!paymentMethodExists)
        {
            return BadRequest(ApiResponse<CreateTransactionResult>.Fail("Metode pembayaran tidak valid."));
        }

        // Agregasi qty per produk (jika ada duplikat, jumlahkan & cek total terhadap stok)
        var aggregated = req.Items
            .GroupBy(i => i.ProductId)
            .Select(g => (ProductId: g.Key, Qty: g.Sum(x => x.Qty)))
            .ToList();

        const int maxRetries = 5;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            // Muat ulang produk segar setiap percobaan (tracking on) — agar stok selalu terbaru.
            var productIds = aggregated.Select(a => a.ProductId).ToList();
            var products = await _context.MsProduct
                .Where(p => productIds.Contains(p.IdProduct) && p.Stsrc)
                .ToDictionaryAsync(p => p.IdProduct);

            // Validasi semua produk ada & aktif
            var missingId = aggregated.Select(a => a.ProductId).FirstOrDefault(pid => !products.ContainsKey(pid));
            if (missingId != null)
            {
                return BadRequest(ApiResponse<CreateTransactionResult>.Fail($"Produk tidak ditemukan: {missingId}"));
            }

            // Validasi stok per item
            foreach (var (pid, qty) in aggregated)
            {
                var p = products[pid];
                if (qty > p.Stock)
                {
                    return BadRequest(ApiResponse<CreateTransactionResult>.Fail(
                        $"Stok produk '{p.Name}' tidak mencukupi (sisa: {p.Stock})."));
                }
            }

            // Hitung ulang subtotal & total di server (jangan percaya angka klien)
            var now = WibTime.Now;
            var transaction = new TrTransaction
            {
                IdTransaction = Guid.NewGuid().ToString(),
                InvoiceNo = await ComputeInvoiceNoAsync(idUser),
                TransactionDate = now,
                IdUser = idUser,
                IdPaymentMethod = req.PaymentMethodId,
                TotalPrice = 0m,
                Stsrc = true,
                Datein = now,
                Userin = idUser
            };

            decimal total = 0m;
            var details = new List<TrTransactionDetail>();
            foreach (var (pid, qty) in aggregated)
            {
                var p = products[pid];
                var subtotal = Math.Round(qty * p.Price * (1m - p.Discount / 100m), 2);
                total += subtotal;

                details.Add(new TrTransactionDetail
                {
                    IdTransactionDetail = Guid.NewGuid().ToString(),
                    IdTransaction = transaction.IdTransaction,
                    IdProduct = p.IdProduct,
                    Qty = qty,
                    Price = p.Price,        // snapshot harga saat transaksi
                    Discount = p.Discount,  // snapshot diskon saat transaksi
                    SubTotal = subtotal,
                    Stsrc = true,
                    Datein = now,
                    Userin = idUser
                });

                // Kurangi stok
                p.Stock -= qty;
                p.Dateup = now;
                p.Userup = idUser;
            }
            transaction.TotalPrice = Math.Round(total, 2);

            _context.TrTransaction.Add(transaction);
            _context.TrTransactionDetail.AddRange(details);

            try
            {
                await using var dbTx = await _context.Database.BeginTransactionAsync();
                await _context.SaveChangesAsync();
                await dbTx.CommitAsync();

                return Ok(ApiResponse<CreateTransactionResult>.Ok(
                    new CreateTransactionResult
                    {
                        IdTransaction = transaction.IdTransaction,
                        InvoiceNo = transaction.InvoiceNo,
                        TransactionDate = transaction.TransactionDate,
                        TotalPrice = transaction.TotalPrice
                    },
                    "Transaksi berhasil!"));
            }
            catch (DbUpdateException ex) when (IsUniqueInvoiceConflict(ex))
            {
                // Race condition: invoice No bentrok dengan transaksi lain yang baru masuk.
                // Rollback (otomatis saat dbTx di-dispose tanpa commit), bersihkan tracker, hitung ulang.
                _context.ChangeTracker.Clear();
                continue;
            }
        }

        return StatusCode(StatusCodes.Status500InternalServerError,
            ApiResponse<CreateTransactionResult>.Fail("Gagal membuat transaksi setelah beberapa percobaan. Coba lagi."));
    }

    // GET: /api/v1/transactions/history?date=yyyy-MM-dd&page=1&pageSize=20
    // Cashier -> hanya transaksinya sendiri; Admin -> semua kasir.
    // Pagination maks 20/halaman, order by TransactionDate ASC (tie-break InvoiceNo).
    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<PagedResult<TransactionHistoryItem>>>> History(
        [FromQuery] string? date,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var (idUser, role) = GetCurrentUser();
        if (idUser == null) return Unauthorized(ApiResponse<PagedResult<TransactionHistoryItem>>.Fail("Unauthorized"));

        // Tanggal wajib dipilih
        if (string.IsNullOrWhiteSpace(date) || !DateTime.TryParse(date, out var parsedDate))
        {
            return BadRequest(ApiResponse<PagedResult<TransactionHistoryItem>>.Fail("Pilih tanggal terlebih dahulu sebelum mencari."));
        }

        // Clamp pagination: maks 20/halaman
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 20) pageSize = 20;

        var dateStart = parsedDate.Date;            // 00:00 WIB
        var dateEnd = parsedDate.Date.AddDays(1);   // 00:00 hari berikutnya

        var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

        var baseQuery = _context.TrTransaction
            .Where(x => x.Stsrc && x.TransactionDate >= dateStart && x.TransactionDate < dateEnd);

        // Otorisasi role: Kasir hanya miliknya; Admin semua
        if (!isAdmin)
        {
            baseQuery = baseQuery.Where(x => x.IdUser == idUser);
        }

        var totalItems = await baseQuery.CountAsync();

        var txns = await baseQuery
            .OrderBy(x => x.TransactionDate)
            .ThenBy(x => x.InvoiceNo)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.IdTransaction,
                x.InvoiceNo,
                x.TransactionDate,
                x.TotalPrice,
                Cashier = x.User.FullName,
                PaymentMethod = x.PaymentMethod.MethodName
            })
            .ToListAsync();

        // Total qty per transaksi (sum Qty dari semua detail)
        var txnIds = txns.Select(t => t.IdTransaction).ToList();
        var qtyByTxn = await _context.TrTransactionDetail
            .Where(d => txnIds.Contains(d.IdTransaction))
            .GroupBy(d => d.IdTransaction)
            .Select(g => new { IdTransaction = g.Key, TotalQty = g.Sum(d => d.Qty) })
            .ToDictionaryAsync(x => x.IdTransaction, x => x.TotalQty);

        var items = txns.Select(t => new TransactionHistoryItem
        {
            InvoiceNo = t.InvoiceNo,
            TransactionDate = t.TransactionDate, // sudah WIB (disimpan sebagai WIB)
            Cashier = t.Cashier,
            TotalQty = qtyByTxn.GetValueOrDefault(t.IdTransaction),
            PaymentMethod = t.PaymentMethod,
            TotalPrice = t.TotalPrice
        }).ToList();

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var result = new PagedResult<TransactionHistoryItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };

        return Ok(ApiResponse<PagedResult<TransactionHistoryItem>>.Ok(result));
    }

    // ---- Helpers ----

    /// <summary>Ambil IdUser & Role dari claim token.</summary>
    private (string? IdUser, string? Role) GetCurrentUser()
    {
        var idUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);
        return (idUser, role);
    }

    /// <summary>
    /// Hitung invoice No berikutnya: [5-digit IdUser][ddmmyy WIB][3-digit running global per hari].
    /// Running = jumlah transaksi seluruh kasir pada tanggal itu + 1.
    /// </summary>
    private async Task<string> ComputeInvoiceNoAsync(string idUser)
    {
        var today = WibTime.Now.Date;
        var tomorrow = today.AddDays(1);
        var countToday = await _context.TrTransaction
            .CountAsync(x => x.TransactionDate >= today && x.TransactionDate < tomorrow);
        var running = countToday + 1;
        return $"{idUser}{WibTime.Now:ddMMyy}{running:D3}";
    }

    /// <summary>Deteksi pelanggaran unique index (SQL Server error 2601/2627).</summary>
    private static bool IsUniqueInvoiceConflict(DbUpdateException ex)
    {
        var sql = ex.InnerException as SqlException
               ?? ex.InnerException?.InnerException as SqlException;
        return sql != null && (sql.Number == 2601 || sql.Number == 2627);
    }
}
