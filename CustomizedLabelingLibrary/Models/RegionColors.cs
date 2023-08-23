using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizedLabelingLibrary.Models
{
    public class RegionColors
    {
        public static string GetColor(int value)
        {
            switch (value)
            {
                case 0:
                    return "#00ffff";
                case 1:
                    return "#0000ff";
                case 2:
                    return "#00ff00";
                case 3:
                    return "#ff0000";
                case 4:
                    return "#5f9ea0";
                case 5:
                    return "#ffa500";
                case 6:
                    return "#40e0d0";
                case 7:
                    return "#9fb6cd";
                case 8:
                    return "#8deeee";
                case 9:
                    return "#6f804a";
                case 10:
                    return "#006400";
                case 11:
                    return "#8b6969";
                case 12:
                    return "#f08080";
                default:
                    return "#ffff00";
            }
        }
    }
}
