﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace XYZ.CalendarHelper
{
    public static class CalendarExtension
    {
        private const string Style = @"
            <style type='text/css'>
            .cal-body {border-spacing: 3px; border-collapse: separate; margin: 10px 20px 10px 15px;}
                .cal-body th {color: #666;border-bottom: 1px solid #ddd;}
                .cal-body td {text-align: center;padding: 5px 5px 2px 5px;position:relative;}
                .cal-body a {text-decoration:none; color:#000;}
            .cal-x {background-color: transparent;}
            .cal-ar {width: 0; height: 0; border-bottom: 18px solid transparent;border-left: 18px solid;position:absolute;top:0;left:0;}
            .cal-al {width: 0; height: 0; border-top: 18px solid transparent;border-right:18px solid; position:absolute;bottom:0;right:0;}
            </style>
            ";

        public static MvcHtmlString CalendarCss(this HtmlHelper helper)
        {
            return MvcHtmlString.Create(Style);
        }

        public static MvcHtmlString Calendar(this HtmlHelper helper, DateTime monthToRender)
        {
            return Calendar(helper, monthToRender, new List<CalendarEvent>(), null);
        }

        public static MvcHtmlString Calendar(this HtmlHelper helper, DateTime monthToRender, object htmlAttributes)
        {
            return Calendar(helper, monthToRender, new List<CalendarEvent>(), htmlAttributes);
        }

        public static MvcHtmlString Calendar(this HtmlHelper helper, DateTime monthToRender, List<CalendarEvent> blockedDates)
        {
            return Calendar(helper, monthToRender, blockedDates, null);
        }

        public static MvcHtmlString Calendar(this HtmlHelper helper, DateTime monthToRender, List<CalendarEvent> events, object htmlAttributes)
        {
            TagBuilder calendar = new TagBuilder("table");
            calendar.Attributes.Add("class", "cal-body");

            calendar.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));

            if (events == null) events = new List<CalendarEvent>();

            TagBuilder monthName = new TagBuilder("caption");
            monthName.Attributes.Add("style", "font-weight:bold");
            monthName.Attributes.Add("class", "monthname");
            monthName.SetInnerText(monthToRender.ToString("MMMM yyyy"));
            calendar.InnerHtml = monthName.ToString();

            //Build Day Names
            TagBuilder dayNames = new TagBuilder("tr");
            var days = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            var daysShort = new[] { "M", "Tu", "W", "Th", "F", "Sa", "Su" };
            var daysLong = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            for (int i = 0; i < 7; i++)
            {
                TagBuilder dayHeader = new TagBuilder("th");
                dayHeader.Attributes.Add("abbr", days[i]);
                dayHeader.Attributes.Add("title", daysLong[i]);
                dayHeader.SetInnerText(daysShort[i]);
                dayNames.InnerHtml += dayHeader.ToString();
            }

            //Build Calendar
            List<TagBuilder> monthRows = new List<TagBuilder>();
            TagBuilder row = new TagBuilder("tr");
            int columnCount = 0;
            for (int i = 1; i <= DateTime.DaysInMonth(monthToRender.Year, monthToRender.Month); i++)
            {
                DateTime day = new DateTime(monthToRender.Year, monthToRender.Month, i);
                if (i == 1)
                {
                    foreach (string dow in days)
                    {
                        if (dow.Equals(day.DayOfWeek.ToString().Substring(0, 3), StringComparison.OrdinalIgnoreCase)) break;
                        TagBuilder blankTag = new TagBuilder("td");
                        blankTag.Attributes.Add("class", "cal-x");
                        row.InnerHtml += blankTag.ToString();
                        columnCount++;
                    }
                }
                TagBuilder dayTag = new TagBuilder("td");
                //See if there is a match
                var matches = events.Where(b => b.ContainsDate(day));
                if (matches.Count() == 1)
                {
                    var match = matches.FirstOrDefault();
                    dayTag.Attributes.Add("style", "background-color:" + match.DisplayColor);

                    if (match.EndDate.Date.Equals(day))
                    {
                        dayTag.InnerHtml = "<div class=\"cal-ar\" style=\"border-left-color:" + match.DisplayColor + "\"></div>";
                        dayTag.Attributes.Remove("style");
                    }

                    if (!String.IsNullOrEmpty(match.CallbackFunction))
                        dayTag.InnerHtml += String.Format("<a href=\"javascript:void(0)\" onclick=\"{0}\">{1}</a>", match.CallbackFunction, i.ToString());
                    else
                        dayTag.InnerHtml += i.ToString();

                    if (match.StartDate.Date.Equals(day))
                    {
                        dayTag.InnerHtml += "<div class=\"cal-al\" style=\"border-right-color:" + match.DisplayColor + "\"></div>";
                        dayTag.Attributes.Remove("style");
                    }

                }
                else if (matches.Count() > 1)
                {
                    var match = matches.First();

                    if (match.EndDate.Date.Equals(day))
                        dayTag.InnerHtml = "<div class=\"cal-ar\" style=\"border-left-color:" + match.DisplayColor + "\"></div>";

                    match = matches.Last();
                    if (!String.IsNullOrEmpty(match.CallbackFunction))
                        dayTag.InnerHtml += String.Format("<a href=\"javascript:void(0)\" onclick=\"{0}\">{1}</a>", match.CallbackFunction, i.ToString());
                    else
                        dayTag.InnerHtml += i.ToString();

                    if (match.StartDate.Date.Equals(day))
                        dayTag.InnerHtml += "<div class=\"cal-al\" style=\"border-right-color:" + match.DisplayColor + "\"></div>";
                }
                else
                {
                    dayTag.SetInnerText(i.ToString());
                }

                row.InnerHtml += dayTag.ToString();
                columnCount++;
                if (columnCount == 7)
                {
                    monthRows.Add(row);
                    row = new TagBuilder("tr");
                    columnCount = 0;
                }
            }
            while (columnCount < 7)
            {
                TagBuilder blankTag = new TagBuilder("td");
                blankTag.Attributes.Add("class", "cal-x");
                row.InnerHtml += blankTag.ToString();
                columnCount++;
            }
            monthRows.Add(row);

            calendar.InnerHtml += dayNames.ToString();
            foreach (var daysrow in monthRows)
            {
                calendar.InnerHtml += daysrow.ToString();
            }
            return MvcHtmlString.Create(calendar.ToString(TagRenderMode.Normal));
        }
    }    
}
