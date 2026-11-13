using System.ComponentModel;
using ClinicManagement.BUS;
using ClinicManagement.DTO;

namespace ClinicManagement.GUI;

public class UcKeDon : UserControl, IReloadable
{
    private readonly HoSoKhamBUS hoSoBus = new();
    private readonly ThuocBUS thuocBus = new();
    private readonly DonThuocBUS donBus = new();

    private readonly ComboBox cboHoSo = new() { Left = 100, Top = 12, Width = 420, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox txtTrieuChung = new()
    {
        Left = 625, Top = 12, Width = 420, Height = 45, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical
    };
    private readonly TextBox txtChanDoan = new() { Left = 100, Top = 70, Width = 420 };
    private readonly NumericUpDown nudTienKham = new()
    {
        Left = 625, Top = 70, Width = 130, Minimum = 0, Maximum = 100000000,
        Increment = 10000, Value = 100000, ThousandsSeparator = true
    };
    private readonly ComboBox cboThuoc = new() { Left = 100, Top = 110, Width = 420, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown nudSL = new() { Left = 570, Top = 110, Width = 60, Minimum = 1, Maximum = 10000, Value = 1 };
    private readonly TextBox txtLieu = new() { Left = 715, Top = 110, Width = 330 };
    private readonly DataGridView grid = UiHelper.CreateGrid();
    private readonly Label lblTamTinh = new() { Left = 15, Top = 18, AutoSize = true, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
    private readonly BindingList<ChiTietDonThuocDTO> gio = new();
    private List<ThuocDTO> dsThuoc = new();

    public UcKeDon()
    {
        var pnl = new Panel { Dock = DockStyle.Top, Height = 195 };
        pnl.Controls.AddRange(new Control[]
        {
            UiHelper.Label("Hồ sơ khám:", 15, 16), cboHoSo,
            UiHelper.Label("Triệu chứng:", 540, 16), txtTrieuChung,
            UiHelper.Label("Chẩn đoán:", 15, 74), txtChanDoan,
            UiHelper.Label("Tiền khám:", 540, 74), nudTienKham,
            UiHelper.Label("Thuốc:", 15, 114), cboThuoc,
            UiHelper.Label("SL:", 540, 114), nudSL,
            UiHelper.Label("Liều dùng:", 645, 114), txtLieu,
            UiHelper.Button("Thêm thuốc", 100, 152, (s, e) => ThemThuoc(), 110),
            UiHelper.Button("Bỏ dòng chọn", 220, 152, (s, e) => BoThuoc(), 120)
        });

        var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 55 };
        pnlBottom.Controls.AddRange(new Control[]
        {
            lblTamTinh,
            UiHelper.Button("Lưu && Xuất đơn", 560, 12, (s, e) => Luu(), 160)
        });

        cboHoSo.SelectedIndexChanged += (s, e) => OnHoSoChanged();
        nudTienKham.ValueChanged += (s, e) => CapNhatTamTinh();
        gio.ListChanged += (s, e) => CapNhatTamTinh();

        Controls.Add(grid);
        Controls.Add(pnl);
        Controls.Add(pnlBottom);

        grid.DataSource = gio;
        UiHelper.Bind(grid, gio,
            ("TenThuoc", "Tên thuốc"), ("DonViTinh", "ĐVT"), ("SoLuong", "Số lượng"),
            ("LieuDung", "Liều dùng"), ("DonGia", "Đơn giá (đ)"), ("ThanhTien", "Thành tiền (đ)"));
        CapNhatTamTinh();
    }

    public void Reload()
    {
        try
        {
            var hoSo = hoSoBus.GetChoKeDon();
            cboHoSo.DataSource = hoSo;
            cboHoSo.DisplayMember = "TieuDe";
            cboHoSo.ValueMember = "MaHoSo";
            OnHoSoChanged();

            dsThuoc = thuocBus.GetAll();
            cboThuoc.DataSource = dsThuoc;
            cboThuoc.DisplayMember = "TieuDe";
            cboThuoc.ValueMember = "MaThuoc";
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }

    private void OnHoSoChanged()
    {
        if (cboHoSo.SelectedItem is HoSoKhamDTO hs)
        {
            txtTrieuChung.Text = hs.TrieuChung;
            txtChanDoan.Text = hs.ChanDoan ?? "";
        }
        else
        {
            txtTrieuChung.Clear();
            txtChanDoan.Clear();
        }
    }

    private void ThemThuoc()
    {
        if (cboThuoc.SelectedItem is not ThuocDTO t)
        {
            UiHelper.Info("Vui lòng chọn thuốc.");
            return;
        }

        int sl = (int)nudSL.Value;
        var existing = gio.FirstOrDefault(c => c.MaThuoc == t.MaThuoc);
        if (existing != null)
        {
            existing.SoLuong += sl;
            if (!string.IsNullOrWhiteSpace(txtLieu.Text)) existing.LieuDung = txtLieu.Text.Trim();
            gio.ResetBindings();
        }
        else
        {
            gio.Add(new ChiTietDonThuocDTO
            {
                MaThuoc = t.MaThuoc,
                TenThuoc = t.TenThuoc,
                DonViTinh = t.DonViTinh,
                DonGia = t.DonGia,
                SoLuong = sl,
                LieuDung = txtLieu.Text.Trim()
            });
        }
        nudSL.Value = 1;
        txtLieu.Clear();
    }

    private void BoThuoc()
    {
        if (grid.CurrentRow?.DataBoundItem is ChiTietDonThuocDTO ct)
            gio.Remove(ct);
    }

    private void CapNhatTamTinh()
    {
        decimal tienThuoc = DonThuocBUS.TinhTienThuoc(gio);
        lblTamTinh.Text = $"Tiền thuốc: {tienThuoc:N0} đ   |   Tiền khám: {nudTienKham.Value:N0} đ   |   Tạm tính: {tienThuoc + nudTienKham.Value:N0} đ";
    }

    private void Luu()
    {
        try
        {
            if (cboHoSo.SelectedItem is not HoSoKhamDTO hs)
                throw new ArgumentException("Không có hồ sơ khám nào đang chờ kê đơn.");

            var don = new DonThuocDTO
            {
                MaHoSo = hs.MaHoSo,
                ChanDoan = txtChanDoan.Text,
                TienKham = nudTienKham.Value,
                ChiTiet = gio.ToList()
            };

            int maDon = donBus.LuuDon(don);   // SqlTransaction trong DAL
            UiHelper.Info($"Đã lưu & xuất đơn thuốc #{maDon}. Số lượng thuốc tồn đã được trừ.");
            gio.Clear();
            txtChanDoan.Clear();
            Reload();
        }
        catch (InvalidOperationException ex)
        {
            // Thiếu thuốc trong kho -> transaction đã Rollback
            UiHelper.Warn(ex.Message);
            Reload();
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }
}
