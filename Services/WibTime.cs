namespace S1.Services;

/// <summary>
/// Helper zona waktu WIB (Asia/Jakarta, UTC+7).
/// Konvensi penyimpanan: seluruh timestamp audit & TransactionDate disimpan dalam waktu WIB
/// agar filter tanggal riwayat, format ddmmyy invoice, dan tampilan UI konsisten.
/// </summary>
public static class WibTime
{
    private static readonly TimeZoneInfo Wib =
        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // UTC+7

    /// <summary>
    /// Waktu sekarang dalam WIB. Kind sengaja di-set Unspecified agar serialisasi
    /// JSON (System.Text.Json) tidak menambahkan offset/Z — sehingga waktu
    /// wall-clock WIB ditampilkan apa adanya di browser zona waktu mana pun.
    /// </summary>
    public static DateTime Now
    {
        get
        {
            var wib = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Wib);
            return DateTime.SpecifyKind(wib, DateTimeKind.Unspecified);
        }
    }

    /// <summary>Tanggal (WIB) dari sebuah DateTime — aman untuk DateTimeKind apa pun.</summary>
    public static DateTime AsWibDate(DateTime value)
    {
        var wib = value.Kind == DateTimeKind.Utc
            ? TimeZoneInfo.ConvertTimeFromUtc(value, Wib)
            : value;
        return wib.Date;
    }
}
