using MySql.Data.MySqlClient; // This namespace contains the MySqlConnection class which is used to connect to MySQL databases.
using System.Configuration; // This namespace contains the ConfigurationManager class which is used to access configuration settings.
using System.Data; // This namespace contains the DataTable class which is used to represent in-memory data in tabular form.
using System.Drawing.Imaging; // This namespace contains the MySqlConnection class which is used to connect to MySQL databases.
using EMC_DotNetCore_Report_Backend.Interface; // This namespace contains the IMysqlConnection interface which defines methods for MySQL connection.
using static Org.BouncyCastle.Math.EC.ECCurve; // This namespace contains the mysqlConnection class which implements the IMysqlConnection interface.

namespace EMC_DotNetCore_Report_Backend.Class // This namespace contains the mysqlConnection class which implements the IMysqlConnection interface.
{
    public class mysqlConnection: IMysqlConnection // This class implements the IMysqlConnection interface to provide MySQL database connection functionality.
    {
        private readonly IConfiguration _config; // Declare a private field to hold the configuration object
        public mysqlConnection(IConfiguration configuration) // Constructor to inject configuration
        {
            _config = configuration; // Assign the injected configuration to the private field
        }

        // ใช้สำหรับเรียก Stored Procedure (คำสั่งที่อยู่ใน DB เช่น CALL GetReportByDate(...))
        // เป็น synchronous method (รอให้เสร็จก่อนทำงานถัดไป)
        // ใช้เมื่อต้องเรียก stored procedure จาก MySQL แบบเร็วๆ ง่ายๆ ไม่ต้อง async
        public DataTable MysqlCmdProcedure(string Query) // This method is used to execute a stored procedure and return a DataTable.
        {
            MySqlConnection conn = new MySqlConnection(_config.GetConnectionString("mysql")); // Get values ​​from appsettings.json named "mysql".
            MySqlDataAdapter da = new MySqlDataAdapter(); // Create an instance of MySqlDataAdapter.
            DataSet ds = new DataSet(); // Create an instance of DataSet.
            using (MySqlCommand cmd = new MySqlCommand()) // Create an instance of MySqlCommand.
            {            
                cmd.Connection = conn; // Assign the connection to the command.
                cmd.CommandText = Query; // Assign the query to the command.
                cmd.CommandType = CommandType.StoredProcedure; // Assign the command type to StoredProcedure.
                da.SelectCommand = cmd; // Assign the command to the data adapter.
                da.Fill(ds); // Fill the dataset with the data from the command.
            }
            return ds.Tables[0]; // Return the first table in the dataset.
        }

        // ใช้สำหรับรัน SQL Query ธรรมดา เช่น SELECT * FROM users
        // เป็น asynchronous method (ใช้ async/await)
        // ใช้เมื่อคุณต้องการ performance ที่ดีขึ้น โดยไม่ block thread หลัก เช่นใน Web API
        public async Task<DataTable>  MysqlDataAdapter(string Query) // This method is used to execute a query asynchronously and return a DataTable.
        {
            MySqlConnection conn = new MySqlConnection(_config.GetConnectionString("mysql"));  // Get values ​​from appsettings.json named "mysql".
            DataSet ds = new DataSet(); // Create an instance of DataSet.
            using (MySqlDataAdapter da = new MySqlDataAdapter(Query, conn)) // Create an instance of MySqlDataAdapter with the query and connection.
            {         
               await da.FillAsync(ds); // Fill the dataset asynchronously with the data from the query.
            }
            return ds.Tables[0]; // Return the first table in the dataset.
        }

    }
}

// MySqlConnection	ใช้เชื่อมต่อกับ MySQL
// MySqlCommand	ใช้รันคำสั่ง SQL หรือ stored procedure
// MySqlDataAdapter	ดึงข้อมูลจาก MySQL มาใส่ใน DataSet
// DataSet / DataTable	โครงสร้างข้อมูลใน .NET สำหรับจัดการข้อมูลแบบตาราง
// await da.FillAsync()	ดึงข้อมูลแบบไม่บล็อก (asynchronous)
// CommandType.StoredProcedure	กำหนดว่า query เป็น stored procedure