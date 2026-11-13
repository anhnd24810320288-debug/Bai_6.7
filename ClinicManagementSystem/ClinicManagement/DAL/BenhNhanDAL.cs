using ClinicManagement.DTO;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.DAL;

public class BenhNhanDAL
{
    /// <summary>Tra cứu bệnh nhân theo số điện thoại bằng SqlParameter. Trả về null nếu chưa có.</summary>
    public BenhNhanDTO FindByPhone(string soDienThoai)
    {
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            "SELECT MaBN, HoTen, SoDienThoai, NamSinh FROM BenhNhan WHERE SoDienThoai = @SoDienThoai", conn);
        cmd.Parameters.AddWithValue("@SoDienThoai", soDienThoai);
        conn.Open();
        using var rd = cmd.ExecuteReader();
        if (!rd.Read()) return null;
        return new BenhNhanDTO
        {
            MaBN = rd.GetInt32(0),
            HoTen = rd.GetString(1),
            SoDienThoai = rd.IsDBNull(2) ? null : rd.GetString(2),
            NamSinh = rd.IsDBNull(3) ? null : rd.GetInt32(3)
        };
    }
}
