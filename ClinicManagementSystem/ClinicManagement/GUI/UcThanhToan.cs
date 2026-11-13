using System.Drawing.Printing;
using ClinicManagement.BUS;
using ClinicManagement.DTO;

namespace ClinicManagement.GUI;

public class UcThanhToan : UserControl, IReloadable
{
    private readonly DonThuocBUS bus = new();

    private readonly ComboBox cboTrangThai = new() { Left = 85, Top = 15, Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DataGridView gridDon = UiHelper.CreateGrid();
    private readonly DataGridView gridCT = UiHelper.CreateGrid();
    private readonly Label lblTong = new() { Left = 15, Top = 14, AutoSize = true, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };

    public UcThanhToan()
    {
        cboTrangThai.Items.AddRange(new object[] { "Tất cả", "Chờ lấy thuốc", "Đã thanh toán" });
        cboTrangThai.SelectedIndex = 0;
        cboTrangThai.SelectionChangeCommitted += (s, e) => LoadDon();

        var pnl = new Panel { Dock = DockStyle.Top, Height = 100 };
        pnl.Controls.AddRange(new Control[]
        {
            UiHelper.Label("Trạng thái:", 15, 19), cboTrangThai,
            UiHelper.Button("Làm mới", 260, 12, (s, e) => LoadDon(), 90),
            UiHelper.Button("Thanh toán", 15, 55, (s, e) => ThanhToan(), 130),
            UiHelper.Button("In hóa đơn", 155, 55, (s, e) => InHoaDon(), 130)
        });

        gridDon.SelectionChanged += (s, e) => LoadChiTiet();

        var grpDon = new GroupBox { Text = "Danh sách đơn thuốc", Dock = DockStyle.Fill };
        grpDon.Controls.Add(gridDon);

        var pnlTong = new Panel { Dock = DockStyle.Bottom, Height = 45 };
        pnlTong.Controls.Add(lblTong);
        var grpCT = new GroupBox { Text = "Chi tiết đơn thuốc đang chọn", Dock = DockStyle.Fill };
        grpCT.Controls.Add(gridCT);
        grpCT.Controls.Add(pnlTong);

        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        table.Controls.Add(grpDon, 0, 0);
        table.Controls.Add(grpCT, 0, 1);

        Controls.Add(table);
        Controls.Add(pnl);
    }

    public void Reload() => LoadDon();

    private void LoadDon()
    {
        try
        {
            string tt = cboTrangThai.SelectedIndex > 0 ? cboTrangThai.Text : null;
            UiHelper.Bind(gridDon, bus.GetByTrangThai(tt),
                ("MaDon", "Mã đơn"), ("HoTen", "Bệnh nhân"), ("NgayKham", "Ngày khám"),
                ("ChanDoan", "Chẩn đoán"), ("TienKham", "Tiền khám (đ)"),
                ("TongTien", "Tổng tiền (đ)"), ("TrangThai", "Trạng thái"));
            LoadChiTiet();
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }

    private void LoadChiTiet()
    {
        try
        {
            if (gridDon.CurrentRow?.DataBoundItem is DonThuocDTO row)
            {
                var don = bus.GetById(row.MaDon);
                UiHelper.Bind(gridCT, don.ChiTiet,
                    ("TenThuoc", "Tên thuốc"), ("DonViTinh", "ĐVT"), ("SoLuong", "Số lượng"),
                    ("DonGia", "Đơn giá (đ)"), ("ThanhTien", "Thành tiền (đ)"), ("LieuDung", "Liều dùng"));

                bool daTra = don.TrangThai == "Đã thanh toán";
                decimal tienThuoc = daTra ? don.TongTien - don.TienKham : DonThuocBUS.TinhTienThuoc(don.ChiTiet);
                decimal tong = don.TienKham + tienThuoc;
                lblTong.Text = $"Tiền khám: {don.TienKham:N0} đ   |   Tiền thuốc: {tienThuoc:N0} đ   |   TỔNG CỘNG: {tong:N0} đ   [{don.TrangThai}]";
            }
            else
            {
                gridCT.DataSource = null;
                lblTong.Text = "";
            }
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }

    private DonThuocDTO Selected() =>
        gridDon.CurrentRow?.DataBoundItem as DonThuocDTO
        ?? throw new ArgumentException("Vui lòng chọn một đơn thuốc.");

    private void ThanhToan()
    {
        try
        {
            var d = Selected();
            if (d.TrangThai == "Đã thanh toán")
                throw new InvalidOperationException("Đơn này đã được thanh toán.");
            if (!UiHelper.Confirm($"Xác nhận thanh toán đơn #{d.MaDon} của {d.HoTen}?")) return;

            bus.ThanhToan(d.MaDon);
            UiHelper.Info("Thanh toán thành công. Bạn có thể in hóa đơn.");
            LoadDon();
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }

    private void InHoaDon()
    {
        try
        {
            var d = Selected();
            string noiDung = bus.TaoNoiDungHoaDon(d.MaDon);

            using var pd = new PrintDocument();
            pd.PrintPage += (s, e) =>
            {
                using var font = new Font("Consolas", 10);
                e.Graphics.DrawString(noiDung, font, Brushes.Black, e.MarginBounds);
            };
            using var dlg = new PrintPreviewDialog { Document = pd, Width = 850, Height = 750 };
            dlg.ShowDialog(FindForm());
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }
}
