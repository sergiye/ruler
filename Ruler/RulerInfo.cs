namespace Ruler
{
    public interface IRulerInfo
    {
        int Width { get; set; }
        int Height { get; set; }
        bool IsVertical { get; set; }
        double Opacity { get; set; }
        bool ShowToolTip { get; set; }
        bool IsLocked { get; set; }
        bool TopMost { get; set; }
    }

    public class RulerInfo : IRulerInfo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsVertical { get; set; }
        public double Opacity { get; set; }
        public bool ShowToolTip { get; set; }
        public bool IsLocked { get; set; }
        public bool TopMost { get; set; }

        public string ConvertToParameters()
        {
            return string.Format("{0} {1} {2} {3} {4} {5} {6}", Width, Height, IsVertical, Opacity, ShowToolTip, IsLocked, TopMost);
        }

        public static RulerInfo ConvertToRulerInfo(string[] args)
        {
            var width = args[0];
            var height = args[1];
            var isVertical = args[2];
            var opacity = args[3];
            var showToolTip = args[4];
            var isLocked = args[5];
            var topMost = args[6];
            var rulerInfo = new RulerInfo
            {
                Width = int.Parse(width),
                Height = int.Parse(height),
                IsVertical = bool.Parse(isVertical),
                Opacity = double.Parse(opacity),
                ShowToolTip = bool.Parse(showToolTip),
                IsLocked = bool.Parse(isLocked),
                TopMost = bool.Parse(topMost)
            };
            return rulerInfo;
        }

        public static RulerInfo GetDefaultRulerInfo()
        {
            var rulerInfo = new RulerInfo
            {
                Width = 400,
                Height = 80,
                Opacity = 0.8,
                ShowToolTip = false,
                IsLocked = false,
                IsVertical = false,
                TopMost = true
            };
            return rulerInfo;
        }
    }

    public static class IRulerInfoExtentension
    {
        public static void CopyInto<T>(this IRulerInfo ruler, T targetInstance)
            where T : IRulerInfo
        {
            targetInstance.Width = ruler.Width;
            targetInstance.Height = ruler.Height;
            targetInstance.IsVertical = ruler.IsVertical;
            targetInstance.Opacity = ruler.Opacity;
            targetInstance.ShowToolTip = ruler.ShowToolTip;
            targetInstance.IsLocked = ruler.IsLocked;
            targetInstance.TopMost = ruler.TopMost;
        }
    }
}