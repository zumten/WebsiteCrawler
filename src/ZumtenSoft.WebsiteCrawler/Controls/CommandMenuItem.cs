using System.Windows;
using System.Windows.Controls;
using ZumtenSoft.WebsiteCrawler.Resources;

namespace ZumtenSoft.WebsiteCrawler.Controls
{
    public class CommandMenuItem : MenuItem
    {
        public override void EndInit()
        {
            base.EndInit();

            Command cmd = Command as Command;
            if (cmd != null)
            {
                //ToolTip = cmd.Name;

                if (cmd.Icon != null)
                    Icon = new Image
                    {
                        Source = cmd.Icon,
                        Style = FindResource("CommandMenuItemImage") as Style,
                        Width = 16,
                        Height = 16
                    };
            }
        }
    }

    public class CommandButton : Button
    {
        public override void EndInit()
        {
            base.EndInit();

            Command cmd = Command as Command;
            if (cmd != null)
            {
                ToolTip = cmd.Text;
                
                if (cmd.Icon != null)
                {
                    
                    Content = new Image
                    {
                        Source = cmd.Icon,
                        Style = FindResource("CommandMenuItemImage") as Style,
                        Width = 16,
                        Height = 16
                    };
                }
            }
        }
    }
}
