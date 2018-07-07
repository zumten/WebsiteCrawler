using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ZumtenSoft.WebsiteCrawler.Controls
{
    public class MaskedTextBox : TextBox
    {
        public string Mask
        {
            get { return (string) GetValue(MaskProperty); }
            set { SetValue(MaskProperty, value); }
        }

        public static readonly DependencyProperty MaskProperty = DependencyProperty.Register(
            "Mask", typeof (string), typeof (MaskedTextBox));
        
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, "^" + Mask + "$"))
                e.Handled = true;

            base.OnPreviewTextInput(e);
        }
    }
}
