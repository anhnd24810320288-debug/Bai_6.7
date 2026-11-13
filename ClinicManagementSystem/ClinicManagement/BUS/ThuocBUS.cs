using ClinicManagement.DAL;
using ClinicManagement.DTO;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.BUS;

public class ThuocBUS
{
    private readonly ThuocDAL dal = new();

    public List<ThuocDTO> GetAll() => dal.GetAll();

    public void Add(ThuocDTO t)
    {
        Validate(t);
        if (t.SoLuongTon < 0) throw new ArgumentException("Số lượng tồn phải >= 0.");
        dal.Insert(t);
    }

    public void Update(ThuocDTO t)
    {
        if (t.MaThuoc <= 0) throw new ArgumentException("Vui lòng chọn thuốc cần cập nhật.");
        Validate(t);
        if (dal.Update(t) == 0) throw new InvalidOperationException("Không tìm thấy thuốc.");
    }

    public void NhapKho(int maThuoc, int soLuong)
    {
        if (maThuoc <= 0) throw new ArgumentException("Vui lòng chọn thuốc cần nhập kho.");
        if (soLuong <= 0) throw new ArgumentException("Số lượng nhập phải lớn hơn 0.");
        if (dal.NhapKho(maThuoc, soLuong) == 0) throw new InvalidOperationException("Không tìm thấy thuốc.");
    }

    public void Delete(int maThuoc)
    {
        if (maThuoc <= 0) throw new ArgumentException("Vui lòng chọn thuốc cần xóa.");
        try { dal.Delete(maThuoc); }
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new InvalidOperationException("Không thể xóa: thuốc này đã được kê trong đơn thuốc.");
        }
    }

    private static void Validate(ThuocDTO t)
    {
        if (string.IsNullOrWhiteSpace(t.TenThuoc))
            throw new ArgumentException("Tên thuốc không được để trống.");
        if (t.DonGia < 0)
            throw new ArgumentException("Đơn giá phải >= 0.");
        t.TenThuoc = t.TenThuoc.Trim();
        t.DonViTinh = string.IsNullOrWhiteSpace(t.DonViTinh) ? null : t.DonViTinh.Trim();
    }
}
