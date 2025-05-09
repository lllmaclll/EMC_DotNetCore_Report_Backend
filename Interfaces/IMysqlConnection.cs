using System.Data; // Using directive for DataTable

namespace EMC_DotNetCore_Report_Backend.Interface // Namespace for the interface
{
    public interface IMysqlConnection // Interface for MySQL connection
    {
        DataTable MysqlCmdProcedure(string Query); // Returns a DataTable.
        Task<DataTable> MysqlDataAdapter(string Query); // Returns a Task<DataTable> → Use with async/await (meaning it works asynchronously)
    }
}
