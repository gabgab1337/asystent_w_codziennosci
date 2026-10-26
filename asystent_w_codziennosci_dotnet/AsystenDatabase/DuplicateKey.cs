using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AssistantDatabase
{
    internal static class DuplicateKey
    {
        public static bool IsViolation(Exception exception)
        {
            for (Exception? current = exception; current != null; current = current.InnerException)
            {
                // 2627 is a primary-key violation, 2601 a unique-index violation.
                if (current is SqlException sql && (sql.Number == 2601 || sql.Number == 2627))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
