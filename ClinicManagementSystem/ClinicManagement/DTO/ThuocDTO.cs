namespace ClinicManagement.DTO;

public class ThuocDTO
{
    public int MaThuoc { get; set; }
    public string TenThuoc { get; set; }
    public string DonViTinh { get; set; }
    public decimal DonGia { get; set; }
    public int SoLuongTon { get; set; }

    public string TieuDe => $"{TenThuoc} ({DonViTinh}) - {DonGia:N0} đ - tồn {SoLuongTon}";
}
