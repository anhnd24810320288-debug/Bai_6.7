using System.Configuration;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.DAL;

public static class DbHelper
{
    public static string ConnectionString =>
        ConfigurationManager.ConnectionStrings["ClinicDB"].ConnectionString;

    public static SqlConnection CreateConnection() => new SqlConnection(ConnectionString);
}
