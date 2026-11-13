namespace ClinicManagement.GUI;

/// <summary>Các UserControl implement interface này để được nạp lại dữ liệu khi chuyển tab.</summary>
public interface IReloadable
{
    void Reload();
}

public static class UiHelper
{
    public static DataGridView CreateGrid() => new DataGridView
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AllowUserToResizeRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        RowHeadersVisible = false,
        BackgroundColor = SystemColors.Window
    };

    /// <summary>Gán nguồn dữ liệu, chỉ hiển thị các cột được liệt kê (đúng thứ tự, tiêu đề tiếng Việt).</summary>
    public static void Bind(DataGridView grid, object data, params (string Prop, string Header)[] cols)
    {
        grid.DataSource = null;
        grid.DataSource = data;
        var wanted = cols.Select(c => c.Prop).ToHashSet();
        foreach (DataGridViewColumn col in grid.Columns)
            col.Visible = wanted.Contains(col.DataPropertyName);

        int order = 0;
        foreach (var (prop, header) in cols)
        {
            var col = grid.Columns.Cast<DataGridViewColumn>().FirstOrDefault(c => c.DataPropertyName == prop);
            if (col == null) continue;
            col.HeaderText = header;
            col.DisplayIndex = order++;
            if (col.ValueType == typeof(DateTime))
                col.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            else if (col.ValueType == typeof(decimal))
            {
                col.DefaultCellStyle.Format = "N0";
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }
    }

    public static Label Label(string text, int left, int top) =>
        new Label { Text = text, Left = left, Top = top, AutoSize = true };

    public static Button Button(string text, int left, int top, EventHandler onClick, int width = 100)
    {
        var b = new Button { Text = text, Left = left, Top = top, Width = width, Height = 30 };
        b.Click += onClick;
        return b;
    }

    public static void Error(Exception ex) =>
        MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

    public static void Warn(string msg) =>
        MessageBox.Show(msg, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    public static void Info(string msg) =>
        MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

    public static bool Confirm(string msg) =>
        MessageBox.Show(msg, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
}
