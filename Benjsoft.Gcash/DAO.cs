using System.Configuration;
using System.Data.SQLite;

namespace Benjsoft.Gcash
{
    public class DAO
    {
        public static SQLiteConnection LiteDbConnection()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            return new SQLiteConnection(connectionString);
        }
    }
}
