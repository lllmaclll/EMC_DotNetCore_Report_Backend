namespace EMC_DotNetCore_Report_Backend.Class // This namespace contains the ReturnDayOfWeek class which is used to convert English day names to Thai day names.
{
    public class ReturnDayOfWeek // This class contains a method to convert English day names to Thai day names.
    {

        // Method to convert English day names to Thai day names
        public string DayOfWeekTH(string day) {
            switch (day)
            {
                case "Sunday":
                    return "วันอาทิตย์";
                case "Monday":
                    return "วันจันทร์";
                case "Tuesday":
                    return "วันอังคาร";
                case "Wednesday":
                    return "วันพุธ";
                case "Thursday":
                    return "วันพฤหัสบดี";
                case "Friday":
                    return "วันศุกร์";
                case "Saturday":
                    return "วันเสาร์";
                default:
                    return "";
            }
        }
    }
}
