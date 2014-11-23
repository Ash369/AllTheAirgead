using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

namespace alltheairgeadApp.Services
{
    class StyleService
    {
        public static List<Color> Colours = new List<Color> { Colors.Blue, Colors.Red, Colors.Yellow, Colors.Green, Colors.Turquoise, Colors.White, Colors.Maroon, Colors.HotPink, Colors.DarkRed, Colors.Chocolate, Colors.Azure, Colors.DarkOrange };

        public static Style LargeDataPoint(Color Colour)
        {
            Style style = new Style(typeof(DataPoint));
            Setter st1 = new Setter(DataPoint.BackgroundProperty, new SolidColorBrush(Colour));
            Setter st2 = new Setter(DataPoint.BorderBrushProperty, new SolidColorBrush(Colors.White));
            Setter st3 = new Setter(DataPoint.HeightProperty, 15);
            Setter st4 = new Setter(DataPoint.WidthProperty, 15);
            style.Setters.Add(st1); style.Setters.Add(st2); style.Setters.Add(st3); style.Setters.Add(st4);
            return style;
        }
    }
}
