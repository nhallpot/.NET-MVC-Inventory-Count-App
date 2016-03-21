using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InventoryCountWebApp
{
    public static class DateRangeWeek
    {
        public static SelectListItem Set(this SelectListItem item, string text, string value = null, bool selected = false)
        {
            item.Text = text;
            if (value != null) item.Value = value;
            if (selected) item.Selected = true;
            return item;
        }

        public static IEnumerable<SelectListItem> ToSelectListItems<T>(this IEnumerable<T> items, Func<T, string> textExpression, Func<T, string> valueExpression = null, string blankText = null, string blankValue = null, string currentValue = null)
        {
            if (blankText != null)
            {
                yield return new SelectListItem().Set(blankText, blankValue);
            }

            foreach (T item in items)
            {
                string text = textExpression(item);
                string value = valueExpression == null ? null : valueExpression(item);
                yield return new SelectListItem().Set(text, value, value != null && value == currentValue);
            }
        }
        public static IEnumerable<SelectListItem> ToSelectListItems(this Dictionary<string, string> items, string blankText = null, string blankValue = null)
        {
            if (blankText != null)
            {
                yield return new SelectListItem().Set(blankText, blankValue);
            }

            foreach (var item in items)
            {
                string text = item.Value;
                string value = item.Key;
                yield return new SelectListItem().Set(text, value);
            }
        }
        // Grab Day the week starts
        // Pulled from stack overflow
        public static DateTime FirstDateOfWeek(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }
            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3);
        }
    }
}