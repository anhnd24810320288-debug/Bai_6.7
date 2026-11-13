using ClinicManagement.BUS;
using ClinicManagement.DTO;

namespace ClinicManagement.GUI;

public class UcTiepNhan : UserControl, IReloadable
{
    private readonly BenhNhanBUS bnBus = new();
    private readonly HoSoKhamBUS hoSoBus = new();

    private readonly TextBox txtSDT = new() { Left = 65, Top = 15, Width = 160 };
    private readonly Label lblTrangThai = new() { Left = 345, Top = 20, AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
    private readonly TextBox txtHoTen = new() { Left = 80, Top = 55, Width = 250 };
    private readonly NumericUpDown nudNamSinh = new()
    {
        Left = 420, Top = 55, Width = 80,
        Minimum = 1900, Maximum = DateTime.Now.Year, Value = 1990
    };
    private readonly TextBox txtTrieuChung = new()
    {
        Left = 100, Top = 95, Width = 520, Height = 60, Multiline = true, ScrollBars = ScrollBars.Vertical
    };
    private readonly DataGridView grid = UiHelper.CreateGrid();

    private BenhNhanDTO benhNhan = null;   // bệnh nhân cũ tìm được (null = chưa tra cứu / bệnh nhân mới)
    private bool daTraCuu = false;

    public UcTiepNhan()
    {
        var pnl = new Panel { Dock = DockStyle.Top, Height = 215 };
        pnl.Controls.AddRange(new Control[]
        {
            UiHelper.Label("SĐT:", 15, 19), txtSDT,
            UiHelper.Button("Tra cứu", 235, 12, (s, e) => TraCuu(), 95),
            lblTrangThai,
            UiHelper.Label("Họ tên:", 15, 59), txtHoTen,
            UiHelper.Label("Năm sinh:", 350, 59), nudNamSinh,
            UiHelper.Label("Triệu chứng:", 15, 98), txtTrieuChung,
            UiHelper.Button("Tiếp nhận && tạo hồ sơ khám", 15, 170, (s, e) => TiepNhan(), 210),
            UiHelper.Button("Làm mới", 235, 170, (s, e) => { ResetForm(); Reload(); }, 100)
        });

        txtSDT.TextChanged += (s, e) => { daTraCuu = false; benhNhan = null; SetNhapMoi(false); lblTrangThai.Text = ""; };
        txtSDT.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { TraCuu(); e.SuppressKeyPress = true; } };

        Controls.Add(grid);
        Controls.Add(pnl);
    }

    public void Reload()
    {
        try
        {
            UiHelper.Bind(grid, hoSoBus.GetAll(),
                ("MaHoSo", "Mã hồ sơ"), ("HoTen", "Bệnh nhân"), ("SoDienThoai", "SĐT"),
                ("NgayKham", "Ngày khám"), ("TrieuChung", "Triệu chứng"),
                ("ChanDoan", "Chẩn đoán"), ("TrangThaiDon", "Trạng thái đơn"));
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }

    /// <summary>Cho phép / khóa các ô nhập thông tin bệnh nhân.</summary>
    private void SetNhapMoi(bool cho)
    {
        txtHoTen.ReadOnly = !cho;
        nudNamSinh.Enabled = cho;
    }

    private void TraCuu()
    {
        try
        {
            benhNhan = bnBus.TimTheoSDT(txtSDT.Text);   // truy vấn dùng SqlParameter
            daTraCuu = true;
            if (benhNhan != null)
            {
                txtHoTen.Text = benhNhan.HoTen;
                nudNamSinh.Value = benhNhan.NamSinh is >= 1900 ? Math.Min(benhNhan.NamSinh.Value, (int)nudNamSinh.Maximum) : 1990;
                SetNhapMoi(false);
                lblTrangThai.ForeColor = Color.SeaGreen;
                lblTrangThai.Text = "Bệnh nhân đã có trong hệ thống.";
            }
            else
            {
                txtHoTen.Clear();
                nudNamSinh.Value = 1990;
                SetNhapMoi(true);
                lblTrangThai.ForeColor = Color.DarkOrange;
                lblTrangThai.Text = "Bệnh nhân mới - hãy nhập họ tên và năm sinh.";
                txtHoTen.Focus();
            }
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }

    private void TiepNhan()
    {
        try
        {
            if (!daTraCuu)
                throw new InvalidOperationException("Vui lòng tra cứu bệnh nhân theo số điện thoại trước.");

            var bn = benhNhan ?? new BenhNhanDTO
            {
                MaBN = 0,
                HoTen = txtHoTen.Text,
                SoDienThoai = txtSDT.Text,
                NamSinh = (int)nudNamSinh.Value
            };

            int maHoSo = hoSoBus.TiepNhan(bn, txtTrieuChung.Text);
            UiHelper.Info($"Đã tạo hồ sơ khám #{maHoSo}. Bệnh nhân chờ bác sĩ kê đơn.");
            ResetForm();
            Reload();
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }

    private void ResetForm()
    {
        txtSDT.Clear();          // TextChanged sẽ reset trạng thái tra cứu
        txtHoTen.Clear();
        nudNamSinh.Value = 1990;
        txtTrieuChung.Clear();
        lblTrangThai.Text = "";
    }
}
