using ClinicManagement.DTO;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.DAL;

public class DonThuocDAL
{
    private const string SelectSql =
        @"SELECT d.MaDon, d.MaHoSo, b.HoTen, b.SoDienThoai, h.NgayKham, h.ChanDoan,
                 d.TienKham, d.TongTien, d.TrangThai
          FROM DonThuoc d
          INNER JOIN HoSoKham h ON d.MaHoSo = h.MaHoSo
          INNER JOIN BenhNhan b ON h.MaBN = b.MaBN ";

    private static DonThuocDTO Map(SqlDataReader rd) => new DonThuocDTO
    {
        MaDon = rd.GetInt32(0),
        MaHoSo = rd.GetInt32(1),
        HoTen = rd.GetString(2),
        SoDienThoai = rd.IsDBNull(3) ? null : rd.GetString(3),
        NgayKham = rd.GetDateTime(4),
        ChanDoan = rd.IsDBNull(5) ? null : rd.GetString(5),
        TienKham = rd.GetDecimal(6),
        TongTien = rd.GetDecimal(7),
        TrangThai = rd.GetString(8)
    };

    // ------------------------------------------------------------------
    // LƯU & XUẤT ĐƠN – SqlTransaction
    //   0. Kiểm tra hồ sơ chưa có đơn, ghi chẩn đoán vào HoSoKham
    //   1. Tạo bản ghi DonThuoc
    //   2. Tạo các bản ghi ChiTietDonThuoc
    //   3. Trừ SoLuongTon trong bảng Thuoc (SoLuongTon -= SoLuong)
    //   4. Nếu một thuốc không đủ tồn -> trans.Rollback() và báo lỗi
    // ------------------------------------------------------------------
    public int LuuDon(DonThuocDTO don)
    {
        using var conn = DbHelper.CreateConnection();
        conn.Open();
        SqlTransaction trans = conn.BeginTransaction();
        try
        {
            // 0a. Mỗi hồ sơ chỉ có một đơn thuốc
            using (var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM DonThuoc WHERE MaHoSo = @MaHoSo", conn, trans))
            {
                cmd.Parameters.AddWithValue("@MaHoSo", don.MaHoSo);
                if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                    throw new InvalidOperationException("Hồ sơ khám này đã có đơn thuốc.");
            }

            // 0b. Ghi chẩn đoán của bác sĩ
            using (var cmd = new SqlCommand(
                "UPDATE HoSoKham SET ChanDoan = @ChanDoan WHERE MaHoSo = @MaHoSo", conn, trans))
            {
                cmd.Parameters.AddWithValue("@ChanDoan", (object)don.ChanDoan ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MaHoSo", don.MaHoSo);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("Không tìm thấy hồ sơ khám.");
            }

            // 1. Tạo DonThuoc
            int maDon;
            using (var cmd = new SqlCommand(
                @"INSERT INTO DonThuoc (MaHoSo, TienKham, TongTien, TrangThai)
                  VALUES (@MaHoSo, @TienKham, @TienKham, N'Chờ lấy thuốc');
                  SELECT CAST(SCOPE_IDENTITY() AS int);", conn, trans))
            {
                cmd.Parameters.AddWithValue("@MaHoSo", don.MaHoSo);
                cmd.Parameters.AddWithValue("@TienKham", don.TienKham);
                maDon = (int)cmd.ExecuteScalar();
            }

            // 2. Tạo các ChiTietDonThuoc
            foreach (var ct in don.ChiTiet)
            {
                using var cmd = new SqlCommand(
                    @"INSERT INTO ChiTietDonThuoc (MaDon, MaThuoc, SoLuong, LieuDung)
                      VALUES (@MaDon, @MaThuoc, @SoLuong, @LieuDung)", conn, trans);
                cmd.Parameters.AddWithValue("@MaDon", maDon);
                cmd.Parameters.AddWithValue("@MaThuoc", ct.MaThuoc);
                cmd.Parameters.AddWithValue("@SoLuong", ct.SoLuong);
                cmd.Parameters.AddWithValue("@LieuDung", (object)ct.LieuDung ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }

            // 3. Trừ tồn kho; thiếu thuốc -> ném lỗi để Rollback
            foreach (var ct in don.ChiTiet)
            {
                int rows;
                using (var cmd = new SqlCommand(
                    @"UPDATE Thuoc SET SoLuongTon = SoLuongTon - @SoLuong
                      WHERE MaThuoc = @MaThuoc AND SoLuongTon >= @SoLuong", conn, trans))
                {
                    cmd.Parameters.AddWithValue("@SoLuong", ct.SoLuong);
                    cmd.Parameters.AddWithValue("@MaThuoc", ct.MaThuoc);
                    rows = cmd.ExecuteNonQuery();
                }

                if (rows == 0)
                {
                    string ten = $"mã {ct.MaThuoc}";
                    int ton = 0;
                    using (var cmd = new SqlCommand(
                        "SELECT TenThuoc, SoLuongTon FROM Thuoc WHERE MaThuoc = @MaThuoc", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@MaThuoc", ct.MaThuoc);
                        using var rd = cmd.ExecuteReader();
                        if (rd.Read()) { ten = rd.GetString(0); ton = rd.GetInt32(1); }
                    }
                    throw new InvalidOperationException(
                        $"Thuốc \"{ten}\" không đủ tồn kho (cần {ct.SoLuong}, chỉ còn {ton}). Đơn chưa được lưu.");
                }
            }

            // 4. Tính tổng tiền = TienKham + SUM(SoLuong * DonGia)
            RecalcTongTien(conn, trans, maDon);

            trans.Commit();
            return maDon;
        }
        catch
        {
            trans.Rollback();   // hủy toàn bộ: không có đơn, không có chi tiết, tồn kho giữ nguyên
            throw;
        }
    }

    private static void RecalcTongTien(SqlConnection conn, SqlTransaction trans, int maDon)
    {
        using var cmd = new SqlCommand(
            @"UPDATE DonThuoc
              SET TongTien = TienKham + ISNULL((
                    SELECT SUM(ct.SoLuong * t.DonGia)
                    FROM ChiTietDonThuoc ct
                    INNER JOIN Thuoc t ON ct.MaThuoc = t.MaThuoc
                    WHERE ct.MaDon = @MaDon), 0)
              WHERE MaDon = @MaDon", conn, trans);
        cmd.Parameters.AddWithValue("@MaDon", maDon);
        cmd.ExecuteNonQuery();
    }

    public List<DonThuocDTO> GetByTrangThai(string trangThai)
    {
        var list = new List<DonThuocDTO>();
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            SelectSql + "WHERE (@TrangThai IS NULL OR d.TrangThai = @TrangThai) ORDER BY d.MaDon DESC", conn);
        cmd.Parameters.AddWithValue("@TrangThai", (object)trangThai ?? DBNull.Value);
        conn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read()) list.Add(Map(rd));
        return list;
    }

    public DonThuocDTO GetById(int maDon)
    {
        DonThuocDTO don;
        using (var conn = DbHelper.CreateConnection())
        using (var cmd = new SqlCommand(SelectSql + "WHERE d.MaDon = @MaDon", conn))
        {
            cmd.Parameters.AddWithValue("@MaDon", maDon);
            conn.Open();
            using var rd = cmd.ExecuteReader();
            if (!rd.Read()) return null;
            don = Map(rd);
        }
        don.ChiTiet = GetChiTiet(maDon);
        return don;
    }

    public List<ChiTietDonThuocDTO> GetChiTiet(int maDon)
    {
        var list = new List<ChiTietDonThuocDTO>();
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            @"SELECT ct.MaDon, ct.MaThuoc, t.TenThuoc, t.DonViTinh, t.DonGia, ct.SoLuong, ct.LieuDung
              FROM ChiTietDonThuoc ct
              INNER JOIN Thuoc t ON ct.MaThuoc = t.MaThuoc
              WHERE ct.MaDon = @MaDon", conn);
        cmd.Parameters.AddWithValue("@MaDon", maDon);
        conn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new ChiTietDonThuocDTO
            {
                MaDon = rd.GetInt32(0),
                MaThuoc = rd.GetInt32(1),
                TenThuoc = rd.GetString(2),
                DonViTinh = rd.IsDBNull(3) ? null : rd.GetString(3),
                DonGia = rd.GetDecimal(4),
                SoLuong = rd.GetInt32(5),
                LieuDung = rd.IsDBNull(6) ? null : rd.GetString(6)
            });
        }
        return list;
    }

    /// <summary>
    /// Thanh toán: tính lại TongTien = TienKham + SUM(SoLuong*DonGia) và chuyển trạng thái
    /// sang "Đã thanh toán" trong cùng một câu UPDATE có tham số.
    /// </summary>
    public bool ThanhToan(int maDon)
    {
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            @"UPDATE DonThuoc
              SET TongTien = TienKham + ISNULL((
                    SELECT SUM(ct.SoLuong * t.DonGia)
                    FROM ChiTietDonThuoc ct
                    INNER JOIN Thuoc t ON ct.MaThuoc = t.MaThuoc
                    WHERE ct.MaDon = @MaDon), 0),
                  TrangThai = N'Đã thanh toán'
              WHERE MaDon = @MaDon AND TrangThai = N'Chờ lấy thuốc'", conn);
        cmd.Parameters.AddWithValue("@MaDon", maDon);
        conn.Open();
        return cmd.ExecuteNonQuery() == 1;
    }
}
