using ClinicManagement.DTO;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.DAL;

public class ThuocDAL
{
    public List<ThuocDTO> GetAll()
    {
        var list = new List<ThuocDTO>();
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            "SELECT MaThuoc, TenThuoc, DonViTinh, DonGia, SoLuongTon FROM Thuoc ORDER BY TenThuoc", conn);
        conn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new ThuocDTO
            {
                MaThuoc = rd.GetInt32(0),
                TenThuoc = rd.GetString(1),
                DonViTinh = rd.IsDBNull(2) ? null : rd.GetString(2),
                DonGia = rd.GetDecimal(3),
                SoLuongTon = rd.GetInt32(4)
            });
        }
        return list;
    }

    public int Insert(ThuocDTO t)
    {
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            @"INSERT INTO Thuoc (TenThuoc, DonViTinh, DonGia, SoLuongTon)
              VALUES (@TenThuoc, @DonViTinh, @DonGia, @SoLuongTon)", conn);
        cmd.Parameters.AddWithValue("@TenThuoc", t.TenThuoc);
        cmd.Parameters.AddWithValue("@DonViTinh", (object)t.DonViTinh ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DonGia", t.DonGia);
        cmd.Parameters.AddWithValue("@SoLuongTon", t.SoLuongTon);
        conn.Open();
        return cmd.ExecuteNonQuery();
    }

    /// <summary>Cập nhật tên, đơn vị tính, đơn giá (không đụng tới tồn kho).</summary>
    public int Update(ThuocDTO t)
    {
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            @"UPDATE Thuoc SET TenThuoc = @TenThuoc, DonViTinh = @DonViTinh, DonGia = @DonGia
              WHERE MaThuoc = @MaThuoc", conn);
        cmd.Parameters.AddWithValue("@TenThuoc", t.TenThuoc);
        cmd.Parameters.AddWithValue("@DonViTinh", (object)t.DonViTinh ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DonGia", t.DonGia);
        cmd.Parameters.AddWithValue("@MaThuoc", t.MaThuoc);
        conn.Open();
        return cmd.ExecuteNonQuery();
    }

    /// <summary>Nhập thêm tồn kho: SoLuongTon += @SoLuong.</summary>
    public int NhapKho(int maThuoc, int soLuong)
    {
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            "UPDATE Thuoc SET SoLuongTon = SoLuongTon + @SoLuong WHERE MaThuoc = @MaThuoc", conn);
        cmd.Parameters.AddWithValue("@SoLuong", soLuong);
        cmd.Parameters.AddWithValue("@MaThuoc", maThuoc);
        conn.Open();
        return cmd.ExecuteNonQuery();
    }

    public int Delete(int maThuoc)
    {
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand("DELETE FROM Thuoc WHERE MaThuoc = @MaThuoc", conn);
        cmd.Parameters.AddWithValue("@MaThuoc", maThuoc);
        conn.Open();
        return cmd.ExecuteNonQuery();
    }
}
