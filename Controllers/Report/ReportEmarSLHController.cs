using System.Data; // Import necessary namespaces
using MySql.Data; // Import MySQL data namespace
using Microsoft.AspNetCore.Mvc; // Import ASP.NET Core MVC namespace
using Microsoft.Reporting.NETCore; // Import Reporting namespace for report generation
using MySql.Data.MySqlClient; // Import MySQL client namespace
using EMC_DotNetCore_Report_Backend.Interface; // Import interface namespace
using System; // Import system namespace for basic types
using EMC_DotNetCore_Report_Backend.Class; // Import custom class namespace
using System.Collections.Generic; // Import generic collections namespace
using System.Linq.Expressions; // Import LINQ expressions namespace
using System.Globalization;
namespace EMC_DotNetCore_Report_Backend.Controllers.Report; // Define the namespace for the controller

[ApiController] // ApiController: ทำให้ class นี้กลายเป็น Web API (รับ request/ส่ง response เป็น JSON หรือข้อมูลอื่น ๆ)
[Route("api/report")] // Route("api/report"): เมื่อผู้ใช้เข้าผ่าน URL yourdomain.com/api/report จะเข้ามาที่ Controller นี้
public class ReportEmarSLHController : ControllerBase // ControllerBase: เป็น base class ของ API Controller
{
   
    private readonly ILogger<ReportEmarSLHController> _logger; // _logger: สำหรับบันทึก log (เช่น error หรือข้อมูล debug)
    private readonly IMysqlConnection _mysqlConnection; // _mysqlConnection: interface ที่ใช้เรียก MySQL (ถูก implement ใน mysqlConnection class)
    private readonly IConfiguration _Config; // _connectionString: สำหรับเก็บ connection string ของ MySQL

    // constructor นี้ใช้ Dependency Injection ให้ _logger และ _mysqlConnection เข้ามาอัตโนมัติ
    public ReportEmarSLHController(ILogger<ReportEmarSLHController> logger, IMysqlConnection mysqlConnection,IConfiguration configuration)
    {
        _logger = logger; // Assign logger to the private field
        _mysqlConnection = mysqlConnection; // Assign MySQL connection to the private field
        _Config = configuration; // Assign configuration to the private field
    }

        [HttpGet("{format}/get-report-emar")]
        public async Task<IActionResult> GetReportEmar(
        [FromRoute] string format, // route param เช่น SLH
        [FromQuery] string an,
        [FromQuery] string lang,
        [FromQuery] string? priorityTypeCode,
        [FromQuery] string? drugType,
        [FromQuery] string? sortPattern,
        [FromQuery] string startDate,
        [FromQuery] string endDate)
        {

        //[HttpGet(Name = "GetWeatherForecast")] // เมื่อเรียก GET /api/report API นี้จะทำงาน (คล้ายกับ main() ในภาษาอื่น ๆ)
        //public async Task<IActionResult> Get()
        //{
        string filepath = _Config["pathReport:path"]; // path to your .rdl file
        DataTable dt_PtHeader = new DataTable(); // Create dataTable for patient header
        DataTable dt_PtOrderReport = new DataTable(); // Create dataTable for patient order report
        DataTable dt_PtOrderRecord = new DataTable(); // Create dataTable for patient order record
        DataTable dt = new DataTable(); // Create dataTable for report

        // Define columns for table data reports according to Report Builder. | (column name, type)
        dt.Columns.Add("AN",typeof(string));
        dt.Columns.Add("HN", typeof(string));
        dt.Columns.Add("PatientName", typeof(string));
        dt.Columns.Add("PatientRight", typeof(string));
        dt.Columns.Add("DrugAllergy", typeof(string));
        dt.Columns.Add("DrugName", typeof(string));
        dt.Columns.Add("UsageNote", typeof(string));
        dt.Columns.Add("TakeTime", typeof(string));
        dt.Columns.Add("TakeDate", typeof(string));
        dt.Columns.Add("MedRecodeTime", typeof(string));
        dt.Columns.Add("MedRecordDesc", typeof(string));
        dt.Columns.Add("sortTakeDate", typeof(DateTime));
        dt.Columns.Add("DrugAllergyShort", typeof(string));
        
        // req.query
        DateTime StartDate = DateTime.Parse(startDate); // Start date for report | Get from parameters | แปลง string เป็นวัน
        DateTime StopDate = DateTime.Parse(endDate); // End date for report | Get from parameters | แปลง string เป็นวัน
        // test ok
        //DateTime StartDate = DateTime.Parse("2025-03-28"); // Start date for report | Get from parameters | แปลง string เป็นวัน
        //DateTime StopDate = DateTime.Parse("2025-04-02"); // End date for report | Get from parameters | แปลง string เป็นวัน
        //DateTime StopDate = DateTime.Parse("2025-04-06"); // End date for report | Get from parameters | แปลง string เป็นวัน
        //DateTime StopDate = DateTime.Parse("2025-04-08"); // End date for report | Get from parameters | แปลง string เป็นวัน

        //var lengthDate = (StopDate - StartDate).TotalDays; // .TotalDays = Number of days in two decimal places.
        ReturnDayOfWeek dow = new ReturnDayOfWeek(); // Create instance of class ReturnDayOfWeek

        // Test1
        TimeSpan diff = StopDate - StartDate;
        int totalDaysTest = (int)diff.TotalDays + 1;

        while (totalDaysTest % 5 != 0)
        {
            StopDate = StopDate.AddDays(1);
            totalDaysTest = (int)(StopDate - StartDate).TotalDays + 1;
        }

        // ใช้ StartDate และ StopDate ในการเรียก stored procedure
        string startDateStr = StartDate.ToString("yyyy-MM-dd");
        string stopDateStr = StopDate.ToString("yyyy-MM-dd");

        var lengthDate = (StopDate - StartDate).TotalDays; // .TotalDays = Number of days in two decimal places.

        // test2
        //int totalDays = (int)lengthDate + 1; 
        //var groupedDates = ReturnGroupDays.GetDateGroups(StartDate, totalDays);

        //  int groupIndex = 1;
        // foreach (var group in groupedDates)
        // {
        //     Console.WriteLine($"กลุ่มที่ {groupIndex++}:");
        //     foreach (var date in group)
        //     {
        //         Console.WriteLine(date.ToString("yyyy-MM-dd"));
        //     }
        //     Console.WriteLine("-----------------------");
        // }

        // Call stored procedure to get patient header and order report
        // req.query
        dt_PtHeader = await _mysqlConnection.MysqlDataAdapter($"call TPN_EMC_{format}.spListReportEmar_PtHeader('{an}', '{lang}')");
        // test ok
        //dt_PtHeader = await _mysqlConnection.MysqlDataAdapter("call TPN_EMC_SLH.spListReportEmar_PtHeader('67-02148', 'TH')");

        // req.query
        dt_PtOrderReport = await _mysqlConnection.MysqlDataAdapter($"call TPN_EMC_{format}.spListReportEmar_PtOrderHeader('{an}', '{priorityTypeCode}', '{drugType}', '{startDateStr}', '{stopDateStr}', '{sortPattern}')");
        // test ok
        ////dt_PtOrderReport = await _mysqlConnection.MysqlDataAdapter("call TPN_EMC_SLH.spListReportEmar_PtOrderHeader('67-02148', '', '', '2025-03-28', '2025-04-06', '0')");
        //dt_PtOrderReport = await _mysqlConnection.MysqlDataAdapter($"call TPN_EMC_SLH.spListReportEmar_PtOrderHeader('67-02148', '', '', '{startDateStr}', '{stopDateStr}', '0')");

        string? DrugAllergyForReport = dt_PtHeader.Rows[0].Field<string>("PatientAllergy");
        string? TreatmentForReport = "66666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666";

        //foreach (var group in groupedDates)
        //{
        //    DateTime groupStartDate = group.First();
        //    DateTime groupEndDate = group.Last();

        // Loop เพื่อเตรียมข้อมูลใส่ report
        for (int i = 0; i <= dt_PtOrderReport.Rows.Count - 1; i++) {

            // Get data from dt_PtHeader
            string? AN = dt_PtHeader.Rows[0].Field<string>("AN");
            string? HN = dt_PtHeader.Rows[0].Field<string>("HN");
            string? PatientName = dt_PtHeader.Rows[0].Field<string>("FullName");
            string? PatientRight = dt_PtHeader.Rows[0].Field<string>("PatientRights");
            string? DrugAllergy = dt_PtHeader.Rows[0].Field<string>("PatientAllergy");
            string? DrugAllergyShort = dt_PtHeader.Rows[0].Field<string>("PatientAllergyShort");
            // Get data from dt_PtOrderReport
            string? DrugName = dt_PtOrderReport.Rows[i].Field<string>("DrugName");
            string? UsageNot = dt_PtOrderReport.Rows[i].Field<string>("UsageNote");
            string? TimeTable = dt_PtOrderReport.Rows[i].Field<string>("TimeTable"); // TimeTable = 0900,1200,1500
            string? MedOrderKey = dt_PtOrderReport.Rows[i].Field<string>("MedOrderKey");

            //Call stored procedure to get patient order record
            // req.query
            dt_PtOrderRecord = await _mysqlConnection.MysqlDataAdapter($"call TPN_EMC_SLH.spListReportEmar_PtOrderRecordByDate('{MedOrderKey}', '{startDateStr}', '{stopDateStr}')");

            // test ok
            ////dt_PtOrderRecord = await _mysqlConnection.MysqlDataAdapter($"call TPN_EMC_SLH.spListReportEmar_PtOrderRecordByDate('{MedOrderKey}','2025-03-28', '2025-04-06')");
            ////dt_PtOrderRecord = await _mysqlConnection.MysqlDataAdapter($"call TPN_EMC_SLH.spListReportEmar_PtOrderRecordByDate('{MedOrderKey}', '{groupStartDate:yyyy-MM-dd}', '{groupEndDate:yyyy-MM-dd}')");
            //dt_PtOrderRecord = await _mysqlConnection.MysqlDataAdapter($"call TPN_EMC_SLH.spListReportEmar_PtOrderRecordByDate('{MedOrderKey}', '{startDateStr}', '{stopDateStr}')");

            if (TimeTable != null)
            {
                string[] split_time = TimeTable.Split(",");

                for (int j = 0; j <= split_time.Length - 1; j++)
                {
                    string TakeTime = split_time[j];

                    //foreach (var date in group)
                    //{
                    //    string dayEN = date.DayOfWeek.ToString();
                    //    string dayTH = dow.DayOfWeekTH(dayEN);
                    //    string TakeDate = date.AddYears(543).ToString("dd/MM/yyyy");        // string ReportTakeDate = dayTH + Environment.NewLine + TakeDate; // Convert date to Thai format (dd/MM/yyyy) and get the day of the week in Thai
                    for (int k = 0; k <= lengthDate; k++)
                    {
                    string dayEN = StartDate.AddDays(k).DayOfWeek.ToString(); // Get the day of the week in English
                    string dayTH = dow.DayOfWeekTH(dayEN); // Convert to Thai day of the week
                    string TakeDate = StartDate.AddDays(k).AddYears(543).ToString("dd/MM/yyyy"); // Convert date to Thai format (dd/MM/yyyy) and get the day of the week in Thai
                    string ReportTakeDate = dayTH + Environment.NewLine + TakeDate; // Convert date to Thai format (dd/MM/yyyy) and get the day of the week in Thai

                        //for (int j = 0; j <= split_time.Length - 1; j++)
                        //{
                        //    string TakeTime = split_time[j];

                            string TakeTimeWhere = "";
                        var isNumeric = int.TryParse(TakeTime, out _); // Check if TakeTime is numeric

                        if (isNumeric)
                        {
                            TakeTimeWhere = TakeTime.Substring(0, 2) + ":" + TakeTime.Substring(2, 2); // Convert to time format (HH:mm)
                        }
                        else
                        {
                            TakeTimeWhere = TakeTime; // If not numeric, use the original value
                        }

                        string? MedRecordTime = "";
                        string? MedRecordUserDesc = "";
                        DataRow[] rows = dt_PtOrderRecord.Select($"TakeDate ='{TakeDate}' AND TakeTime = '{TakeTimeWhere}' "); // Select rows from dt_PtOrderRecord where TakeDate and TakeTime match

                        // Check if there are any rows that match the criteria
                        foreach (var items in rows)
                        {
                            MedRecordTime = items.Field<string>("MedRecordTimeDesc");
                            MedRecordUserDesc = items.Field<string>("MedRecordUserDesc");
                        }
                        // Add data to dt (DataTable for report)
                        dt.Rows.Add(AN, HN, PatientName, PatientRight, DrugAllergy, DrugName, UsageNot, TakeTime, ReportTakeDate, MedRecordTime, MedRecordUserDesc, StartDate.AddDays(k), DrugAllergyShort);
                        //dt.Rows.Add(AN, HN, PatientName, PatientRight, DrugAllergy, DrugName, UsageNot, TakeTime, ReportTakeDate, MedRecordTime, MedRecordUserDesc, date, DrugAllergyShort);

                        //}

                    }
                }
            }
            else
            {
                //foreach (var date in group)
                //{
                //    string dayEN = date.DayOfWeek.ToString();
                //    string dayTH = dow.DayOfWeekTH(dayEN);
                //    string TakeDate = date.AddYears(543).ToString("dd/MM/yyyy");
                for (int k = 0; k <= lengthDate; k++)
                {
                    string dayEN = StartDate.AddDays(k).DayOfWeek.ToString(); // Get the day of the week in English
                    string dayTH = dow.DayOfWeekTH(dayEN); // Convert to Thai day of the week
                    string TakeDate = StartDate.AddDays(k).AddYears(543).ToString("dd/MM/yyyy"); // Convert date to Thai format (dd/MM/yyyy) and get the day of the week in Thai
                    string ReportTakeDate = dayTH + Environment.NewLine + TakeDate; // Convert date to Thai format (dd/MM/yyyy) and get the day of the week in Thai
                    string? MedRecordTime = "";
                    string? MedRecordUserDesc = "";
                    DataRow[] rows = dt_PtOrderRecord.Select($"TakeDate ='{TakeDate}' AND (TakeTime = '' or TakeTime IS NULL) "); // Select rows from dt_PtOrderRecord where TakeDate and TakeTime match

                    // Check if there are any rows that match the criteria
                    foreach (var items in rows)
                    {
                        MedRecordTime = items.Field<string>("MedRecordTimeDesc");
                        MedRecordUserDesc = items.Field<string>("MedRecordUserDesc");
                    }
                    // Add data to dt (DataTable for report)
                    dt.Rows.Add(AN, HN, PatientName, PatientRight, DrugAllergy, DrugName, UsageNot, null, ReportTakeDate, MedRecordTime, MedRecordUserDesc, StartDate.AddDays(k), DrugAllergyShort);
                    //dt.Rows.Add(AN, HN, PatientName, PatientRight, DrugAllergy, DrugName, UsageNot, null, ReportTakeDate, MedRecordTime, MedRecordUserDesc, date, DrugAllergyShort);
                }

            }
        }

    //}

        // Check if the DataTable is empty
        using (var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read)) // Open the .rdl file
        {
            LocalReport report = new(); // Create a new LocalReport instance
            report.LoadReportDefinition(fileStream); // Load the report definition from the file stream
            report.DataSources.Add(new ReportDataSource(name: "DataSet1", dt)); // Add the DataTable as a data source to the report
            report.SetParameters(new ReportParameter("drugAlleryForReport", "DrugAllergyForReportXXX"));
            report.SetParameters(new ReportParameter("treatmentForReport", TreatmentForReport));
            byte[] pdfData = report.Render(format: "PDF"); // Render the report as a PDF
            return Ok(File(pdfData, contentType: "application/pdf")); // Return the PDF file as a response
        }
    }

    [HttpGet("test")] // Define a test endpoint
    // [HttpGet(Name = "GetWeatherForecast")] // เมื่อเรียก GET /api/report/test API นี้จะทำงาน (คล้ายกับ main() ในภาษาอื่น ๆ)
    public IActionResult test()
    {
        return Ok("test");
    }
}
