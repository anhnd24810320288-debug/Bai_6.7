namespace ClinicManagement.DTO;

public class ChiTietDonThuocDTO
{
    public int MaDon { get; set; }
    public int MaThuoc { get; set; }
    public string TenThuoc { get; set; }     // hiển thị
    public string DonViTinh { get; set; }    // hiển thị
    public decimal DonGia { get; set; }      // hiển thị (giá hiện hành trong bảng Thuoc)
    public int SoLuong { get; set; }
    public string LieuDung { get; set; }

    public decimal ThanhTien => SoLuong * DonGia;
}
