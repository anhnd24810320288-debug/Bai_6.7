namespace ClinicManagement.DTO;

public class DonThuocDTO
{
    public int MaDon { get; set; }
    public int MaHoSo { get; set; }
    public string HoTen { get; set; }          // hiển thị
    public string SoDienThoai { get; set; }    // hiển thị
    public DateTime NgayKham { get; set; }     // hiển thị
    public string ChanDoan { get; set; }       // hiển thị / nhập khi kê đơn
    public decimal TienKham { get; set; }
    public decimal TongTien { get; set; }
    public string TrangThai { get; set; }
    public List<ChiTietDonThuocDTO> ChiTiet { get; set; } = new();
}
