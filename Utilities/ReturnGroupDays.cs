using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EMC_DotNetCore_Report_Backend.Class
{
    public class ReturnGroupDays
    {
        public static List<List<DateTime>> GetDateGroups(DateTime startDate, int lengthDate)
        {
            var allDates = new List<DateTime>();

            // สร้างลิสต์วันที่ทั้งหมดตามจำนวนวัน
            for (int i = 0; i < lengthDate; i++)
            {
                allDates.Add(startDate.AddDays(i));
            }

            // ปัดขึ้นให้ครบจำนวนที่หารด้วย 5 ได้ลงตัว
            int remainder = allDates.Count % 5;
            if (remainder != 0)
            {
                int missingDays = 5 - remainder;
                for (int i = 0; i < missingDays; i++)
                {
                    allDates.Add(allDates.Last().AddDays(1)); // เพิ่มวันต่อ ๆ ไป
                }
            }

            // แบ่งกลุ่มละ 5 วัน
            var resultGroups = new List<List<DateTime>>();
            for (int i = 0; i < allDates.Count; i += 5)
            {
                resultGroups.Add(allDates.GetRange(i, Math.Min(5, allDates.Count - i)));
            }

            return resultGroups;
        }
    }
}