using ClinicManagement.DTO;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.DAL;

public class HoSoKhamDAL
{
    private const string SelectSql =
        @"SELECT h.MaHoSo, h.MaBN, b.HoTen, b.SoDienThoai, h.NgayKham, h.TrieuChung, h.ChanDoan,
                 ISNULL(d.TrangThai, N'Chưa kê đơn') AS TrangThaiDon
          FROM HoSoKham h
          INNER JOIN BenhNhan b ON h.MaBN = b.MaBN
          LEFT JOIN DonThuoc d ON d.MaHoSo = h.MaHoSo ";

    private static HoSoKhamDTO Map(SqlDataReader rd) => new HoSoKhamDTO
    {
        MaHoSo = rd.GetInt32(0),
        MaBN = rd.GetInt32(1),
        HoTen = rd.GetString(2),
        SoDienThoai = rd.IsDBNull(3) ? null : rd.GetString(3),
        NgayKham = rd.GetDateTime(4),
        TrieuChung = rd.IsDBNull(5) ? null : rd.GetString(5),
        ChanDoan = rd.IsDBNull(6) ? null : rd.GetString(6),
        TrangThaiDon = rd.GetString(7)
    };

    public List<HoSoKhamDTO> GetAll()
    {
        var list = new List<HoSoKhamDTO>();
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(SelectSql + "ORDER BY h.MaHoSo DESC", conn);
        conn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read()) list.Add(Map(rd));
        return list;
    }

    /// <summary>Hồ sơ khám chưa có đơn thuốc (đang chờ bác sĩ kê đơn).</summary>
    public List<HoSoKhamDTO> GetChoKeDon()
    {
        var list = new List<HoSoKhamDTO>();
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(SelectSql + "WHERE d.MaDon IS NULL ORDER BY h.MaHoSo", conn);
        conn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read()) list.Add(Map(rd));
        return list;
    }

    /// <summary>
    /// Tiếp nhận: nếu bệnh nhân mới (MaBN = 0) thì INSERT BenhNhan, sau đó INSERT HoSoKham.
    /// Hai bước nằm trong một transaction để không sinh bệnh nhân "mồ côi".
    /// </summary>
    public int TiepNhan(BenhNhanDTO bn, string trieuChung)
    {
        using var conn = DbHelper.CreateConnection();
        conn.Open();
        using var tran = conn.BeginTransaction();
        try
        {
            int maBN = bn.MaBN;
            if (maBN <= 0)
            {
                using var cmd = new SqlCommand(
                    @"INSERT INTO BenhNhan (HoTen, SoDienThoai, NamSinh)
                      VALUES (@HoTen, @SoDienThoai, @NamSinh);
                      SELECT CAST(SCOPE_IDENTITY() AS int);", conn, tran);
                cmd.Parameters.AddWithValue("@HoTen", bn.HoTen);
                cmd.Parameters.AddWithValue("@SoDienThoai", (object)bn.SoDienThoai ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NamSinh", (object)bn.NamSinh ?? DBNull.Value);
                maBN = (int)cmd.ExecuteScalar();
            }

            int maHoSo;
            using (var cmd = new SqlCommand(
                @"INSERT INTO HoSoKham (MaBN, TrieuChung) VALUES (@MaBN, @TrieuChung);
                  SELECT CAST(SCOPE_IDENTITY() AS int);", conn, tran))
            {
                cmd.Parameters.AddWithValue("@MaBN", maBN);
                cmd.Parameters.AddWithValue("@TrieuChung", (object)trieuChung ?? DBNull.Value);
                maHoSo = (int)cmd.ExecuteScalar();
            }

            tran.Commit();
            return maHoSo;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }
}
