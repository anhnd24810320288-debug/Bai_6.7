namespace ClinicManagement.DTO;

public class HoSoKhamDTO
{
    public int MaHoSo { get; set; }
    public int MaBN { get; set; }
    public string HoTen { get; set; }            // hiển thị (JOIN BenhNhan)
    public string SoDienThoai { get; set; }      // hiển thị
    public DateTime NgayKham { get; set; }
    public string TrieuChung { get; set; }
    public string ChanDoan { get; set; }
    public string TrangThaiDon { get; set; }     // "Chưa kê đơn" / "Chờ lấy thuốc" / "Đã thanh toán"

    public string TieuDe => $"#{MaHoSo} - {HoTen} ({NgayKham:dd/MM/yyyy HH:mm})";
}
