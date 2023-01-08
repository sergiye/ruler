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
            var rulerInfo = new RulerInfo
            {
                Width = int.Parse(args[0]),
                Height = int.Parse(args[1]),
                IsVertical = bool.Parse(args[2]),
                Opacity = double.Parse(args[3]),
                ShowToolTip = bool.Parse(args[4]),
                IsLocked = bool.Parse(args[5]),
                TopMost = bool.Parse(args[6])
            };
            return rulerInfo;
        }

        public static RulerInfo GetDefaultRulerInfo()
        {
            var rulerInfo = new RulerInfo
            {
                Width = 600,
                Height = 150,
                Opacity = 0.9,
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
        public static void CopyInto(this IRulerInfo ruler, IRulerInfo targetInstance)
        {
            targetInstance.IsVertical = ruler.IsVertical;
            targetInstance.Height = ruler.Height;
            targetInstance.Width = ruler.Width;
            targetInstance.Opacity = ruler.Opacity;
            targetInstance.ShowToolTip = ruler.ShowToolTip;
            targetInstance.IsLocked = ruler.IsLocked;
            targetInstance.TopMost = ruler.TopMost;
        }
    }
}