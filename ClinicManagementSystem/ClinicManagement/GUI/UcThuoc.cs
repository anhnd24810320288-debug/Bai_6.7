using ClinicManagement.BUS;
using ClinicManagement.DTO;

namespace ClinicManagement.GUI;

public class UcThuoc : UserControl, IReloadable
{
    private static readonly string[] DonVi = { "Viên", "Vỉ", "Chai", "Gói", "Ống", "Lọ", "Tuýp", "Hộp" };

    private readonly ThuocBUS bus = new();
    private readonly TextBox txtTen = new() { Left = 85, Top = 15, Width = 260 };
    private readonly ComboBox cboDonVi = new() { Left = 440, Top = 15, Width = 100, DropDownStyle = ComboBoxStyle.DropDown };
    private readonly NumericUpDown nudDonGia = new()
    {
        Left = 640, Top = 15, Width = 120, Minimum = 0, Maximum = 1000000000,
        Increment = 500, ThousandsSeparator = true
    };
    private readonly NumericUpDown nudTon = new() { Left = 880, Top = 15, Width = 80, Minimum = 0, Maximum = 1000000 };
    private readonly NumericUpDown nudNhap = new() { Left = 585, Top = 62, Width = 80, Minimum = 1, Maximum = 1000000, Value = 100 };
    private readonly DataGridView grid = UiHelper.CreateGrid();
    private int selectedId = 0;

    public UcThuoc()
    {
        cboDonVi.Items.AddRange(DonVi);

        var pnl = new Panel { Dock = DockStyle.Top, Height = 110 };
        pnl.Controls.AddRange(new Control[]
        {
            UiHelper.Label("Tên thuốc:", 15, 19), txtTen,
            UiHelper.Label("ĐVT:", 395, 19), cboDonVi,
            UiHelper.Label("Đơn giá:", 580, 19), nudDonGia,
            UiHelper.Label("Tồn ban đầu:", 790, 19), nudTon,
            UiHelper.Button("Thêm thuốc", 15, 55, (s, e) => Add(), 110),
            UiHelper.Button("Cập nhật giá/thông tin", 135, 55, (s, e) => Edit(), 170),
            UiHelper.Button("Xóa", 315, 55, (s, e) => Remove(), 70),
            UiHelper.Button("Làm mới", 395, 55, (s, e) => { ClearInput(); Reload(); }, 90),
            UiHelper.Label("Nhập thêm:", 510, 66), nudNhap,
            UiHelper.Button("Nhập kho", 675, 55, (s, e) => NhapKho(), 100)
        });

        grid.SelectionChanged += (s, e) => OnRowSelected();
        Controls.Add(grid);
        Controls.Add(pnl);
    }

    public void Reload()
    {
        try
        {
            UiHelper.Bind(grid, bus.GetAll(),
                ("MaThuoc", "Mã thuốc"), ("TenThuoc", "Tên thuốc"), ("DonViTinh", "ĐVT"),
                ("DonGia", "Đơn giá (đ)"), ("SoLuongTon", "Số lượng tồn"));
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }

    private void OnRowSelected()
    {
        if (grid.CurrentRow?.DataBoundItem is ThuocDTO t)
        {
            selectedId = t.MaThuoc;
            txtTen.Text = t.TenThuoc;
            cboDonVi.Text = t.DonViTinh ?? "";
            nudDonGia.Value = Math.Min(t.DonGia, nudDonGia.Maximum);
            nudTon.Value = Math.Min(t.SoLuongTon, (int)nudTon.Maximum);
        }
    }

    private ThuocDTO ReadInput() => new()
    {
        MaThuoc = selectedId,
        TenThuoc = txtTen.Text,
        DonViTinh = cboDonVi.Text,
        DonGia = nudDonGia.Value,
        SoLuongTon = (int)nudTon.Value
    };

    private void ClearInput()
    {
        selectedId = 0;
        txtTen.Clear();
        cboDonVi.Text = "";
        nudDonGia.Value = 0;
        nudTon.Value = 0;
    }

    private void Add()
    {
        try
        {
            var t = ReadInput();
            t.MaThuoc = 0;
            bus.Add(t);
            UiHelper.Info("Đã thêm thuốc mới.");
            ClearInput();
            Reload();
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }

    private void Edit()
    {
        try
        {
            bus.Update(ReadInput());   // không thay đổi tồn kho
            UiHelper.Info("Đã cập nhật thông tin / đơn giá thuốc.");
            Reload();
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }

    private void NhapKho()
    {
        try
        {
            bus.NhapKho(selectedId, (int)nudNhap.Value);
            UiHelper.Info($"Đã nhập thêm {(int)nudNhap.Value} vào tồn kho.");
            Reload();
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }

    private void Remove()
    {
        try
        {
            if (selectedId <= 0) throw new ArgumentException("Vui lòng chọn thuốc cần xóa.");
            if (!UiHelper.Confirm("Bạn có chắc muốn xóa thuốc này?")) return;
            bus.Delete(selectedId);
            ClearInput();
            Reload();
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }
}
