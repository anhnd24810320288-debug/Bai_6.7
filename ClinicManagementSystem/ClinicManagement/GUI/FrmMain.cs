namespace ClinicManagement.GUI;

public class FrmMain : Form
{
    private readonly TabControl tab = new() { Dock = DockStyle.Fill };

    public FrmMain()
    {
        Text = "Quản lý phòng khám & kê đơn thuốc";
        Size = new Size(1150, 700);
        MinimumSize = new Size(1000, 600);
        StartPosition = FormStartPosition.CenterScreen;

        AddTab("Tiếp nhận bệnh nhân", new UcTiepNhan());
        AddTab("Danh mục thuốc", new UcThuoc());
        AddTab("Kê đơn & Xuất thuốc", new UcKeDon());
        AddTab("Thanh toán & Hóa đơn", new UcThanhToan());
        Controls.Add(tab);

        tab.SelectedIndexChanged += (s, e) => ReloadCurrent();
        Shown += (s, e) => ReloadCurrent();
    }

    private void AddTab(string title, Control uc)
    {
        uc.Dock = DockStyle.Fill;
        var page = new TabPage(title);
        page.Controls.Add(uc);
        tab.TabPages.Add(page);
    }

    private void ReloadCurrent()
    {
        if (tab.SelectedTab is { Controls.Count: > 0 } page && page.Controls[0] is IReloadable r)
            r.Reload();
    }
}
