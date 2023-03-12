using Microsoft.AspNetCore.Components;
using Snitch.DTOs;
using Radzen.Blazor;
using System.Globalization;
using Microsoft.Graph;

namespace Snitch.Components
{
    public partial class Chart
    {
        [Parameter]
        public List<ActivityDataDTO> ActivityData { get; set; }


        string FormatAsTime(object value)
        {
            if (value != null)
            {
                return Convert.ToDateTime(value).ToString("HH:mm");
            }

            return string.Empty;
        }

        string FormatAsActivity(object value)
        {
            if (value != null)
            {
                switch (value.ToString())
                {
                    case "0":
                        return "Offline";
                    case "1":
                        return "Away";
                    case "2":
                        return "DoNotDisturb";
                    case "3":
                        return "Busy";
                    case "4":
                        return "Available";
                }
            }

            return string.Empty;
        }
    }
}
