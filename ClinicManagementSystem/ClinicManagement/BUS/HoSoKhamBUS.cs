using ClinicManagement.DAL;
using ClinicManagement.DTO;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.BUS;

public class HoSoKhamBUS
{
    private readonly HoSoKhamDAL dal = new();

    public List<HoSoKhamDTO> GetAll() => dal.GetAll();

    public List<HoSoKhamDTO> GetChoKeDon() => dal.GetChoKeDon();

    /// <summary>Tiếp nhận: bệnh nhân mới (MaBN = 0) sẽ được tạo mới rồi tạo hồ sơ khám.</summary>
    public int TiepNhan(BenhNhanDTO bn, string trieuChung)
    {
        if (string.IsNullOrWhiteSpace(trieuChung))
            throw new ArgumentException("Vui lòng nhập triệu chứng.");

        if (bn.MaBN <= 0)
        {
            if (string.IsNullOrWhiteSpace(bn.HoTen))
                throw new ArgumentException("Họ tên bệnh nhân không được để trống.");
            if (!BenhNhanBUS.IsValidPhone(bn.SoDienThoai))
                throw new ArgumentException("Số điện thoại không hợp lệ (9–14 chữ số).");
            if (bn.NamSinh is < 1900 || bn.NamSinh > DateTime.Now.Year)
                throw new ArgumentException("Năm sinh không hợp lệ.");
            bn.HoTen = bn.HoTen.Trim();
            bn.SoDienThoai = bn.SoDienThoai.Trim();
        }

        try { return dal.TiepNhan(bn, trieuChung.Trim()); }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new InvalidOperationException("Số điện thoại này đã tồn tại. Hãy tra cứu lại bệnh nhân.");
        }
    }
}
