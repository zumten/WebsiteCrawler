using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ZumtenSoft.WebsiteCrawler.Resources;

namespace ZumtenSoft.WebsiteCrawler.Controls
{
    public class ImageButton : Button
    {
        public BitmapImage Image
        {
            get { return (BitmapImage) GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
            "Image", typeof (BitmapImage), typeof (ImageButton));

        public override void EndInit()
        {
            base.EndInit();

            Content = new Image
            {
                Source = Image,
                Style = FindResource("CommandMenuItemImage") as Style,
                Width = 16,
                Height = 16
            };
        }
    }
}
