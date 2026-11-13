using System.Text.RegularExpressions;
using ClinicManagement.DAL;
using ClinicManagement.DTO;

namespace ClinicManagement.BUS;

public class BenhNhanBUS
{
    private readonly BenhNhanDAL dal = new();

    public static bool IsValidPhone(string sdt) =>
        !string.IsNullOrWhiteSpace(sdt) && Regex.IsMatch(sdt.Trim(), @"^\+?\d{9,14}$");

    /// <summary>Tra cứu theo số điện thoại. Trả về null nếu là bệnh nhân mới.</summary>
    public BenhNhanDTO TimTheoSDT(string sdt)
    {
        if (!IsValidPhone(sdt))
            throw new ArgumentException("Số điện thoại không hợp lệ (9–14 chữ số).");
        return dal.FindByPhone(sdt.Trim());
    }
}
