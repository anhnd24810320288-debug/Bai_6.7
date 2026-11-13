using System.Text;
using ClinicManagement.DAL;
using ClinicManagement.DTO;

namespace ClinicManagement.BUS;

public class DonThuocBUS
{
    private readonly DonThuocDAL dal = new();

    /// <summary>Lưu & xuất đơn (SqlTransaction nằm ở DAL). Trả về mã đơn.</summary>
    public int LuuDon(DonThuocDTO don)
    {
        if (don.MaHoSo <= 0) throw new ArgumentException("Vui lòng chọn hồ sơ khám.");
        if (string.IsNullOrWhiteSpace(don.ChanDoan)) throw new ArgumentException("Bác sĩ chưa nhập chẩn đoán.");
        if (don.TienKham < 0) throw new ArgumentException("Tiền khám phải >= 0.");
        if (don.ChiTiet == null || don.ChiTiet.Count == 0)
            throw new ArgumentException("Đơn thuốc phải có ít nhất một loại thuốc.");
        if (don.ChiTiet.Any(c => c.SoLuong <= 0)) throw new ArgumentException("Số lượng thuốc phải lớn hơn 0.");

        don.ChanDoan = don.ChanDoan.Trim();
        // Gộp thuốc trùng (khóa chính ChiTietDonThuoc là (MaDon, MaThuoc))
        don.ChiTiet = don.ChiTiet
            .GroupBy(c => c.MaThuoc)
            .Select(g => new ChiTietDonThuocDTO
            {
                MaThuoc = g.Key,
                SoLuong = g.Sum(x => x.SoLuong),
                LieuDung = g.Select(x => x.LieuDung).LastOrDefault(l => !string.IsNullOrWhiteSpace(l))
            })
            .ToList();

        return dal.LuuDon(don);
    }

    public List<DonThuocDTO> GetByTrangThai(string trangThai) => dal.GetByTrangThai(trangThai);

    public DonThuocDTO GetById(int maDon) => dal.GetById(maDon);

    public void ThanhToan(int maDon)
    {
        if (maDon <= 0) throw new ArgumentException("Vui lòng chọn đơn thuốc.");
        if (!dal.ThanhToan(maDon))
            throw new InvalidOperationException("Đơn không tồn tại hoặc đã được thanh toán trước đó.");
    }

    /// <summary>Tính tổng chi phí = TienKham + SUM(SoLuong x DonGia).</summary>
    public static decimal TinhTienThuoc(IEnumerable<ChiTietDonThuocDTO> ds) => ds.Sum(c => c.ThanhTien);

    public string TaoNoiDungHoaDon(int maDon)
    {
        var d = dal.GetById(maDon) ?? throw new InvalidOperationException("Không tìm thấy đơn thuốc.");
        bool daThanhToan = d.TrangThai == "Đã thanh toán";
        decimal tienThuoc = daThanhToan ? d.TongTien - d.TienKham : TinhTienThuoc(d.ChiTiet);
        decimal tong = d.TienKham + tienThuoc;

        var line = new string('=', 62);
        var sb = new StringBuilder();
        sb.AppendLine(line);
        sb.AppendLine("                 PHÒNG KHÁM ĐA KHOA");
        sb.AppendLine("           HÓA ĐƠN KHÁM BỆNH VÀ THUỐC");
        sb.AppendLine(line);
        sb.AppendLine($"Mã đơn      : {d.MaDon}");
        sb.AppendLine($"Bệnh nhân   : {d.HoTen}");
        sb.AppendLine($"Điện thoại  : {d.SoDienThoai}");
        sb.AppendLine($"Ngày khám   : {d.NgayKham:dd/MM/yyyy HH:mm}");
        sb.AppendLine($"Chẩn đoán   : {d.ChanDoan}");
        sb.AppendLine(new string('-', 62));
        sb.AppendLine($"{"STT",-4}{"Tên thuốc",-24}{"SL",4} {"Đơn giá",10} {"Thành tiền",13}");
        int i = 1;
        foreach (var c in d.ChiTiet)
        {
            var ten = c.TenThuoc.Length > 22 ? c.TenThuoc[..22] : c.TenThuoc;
            sb.AppendLine($"{i,-4}{ten,-24}{c.SoLuong,4} {c.DonGia,10:N0} {c.ThanhTien,13:N0}");
            if (!string.IsNullOrWhiteSpace(c.LieuDung))
                sb.AppendLine($"    Liều dùng: {c.LieuDung}");
            i++;
        }
        sb.AppendLine(new string('-', 62));
        sb.AppendLine($"{"Tiền khám:",-40}{d.TienKham,18:N0} đ");
        sb.AppendLine($"{"Tiền thuốc:",-40}{tienThuoc,18:N0} đ");
        sb.AppendLine($"{"TỔNG CỘNG:",-40}{tong,18:N0} đ");
        sb.AppendLine(line);
        sb.AppendLine($"Trạng thái  : {d.TrangThai}");
        sb.AppendLine();
        sb.AppendLine("         Cảm ơn quý khách. Chúc sớm bình phục!");
        return sb.ToString();
    }
}
