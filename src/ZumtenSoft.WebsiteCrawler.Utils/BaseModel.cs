using System.ComponentModel;

namespace ZumtenSoft.WebsiteCrawler.Utils
{
    public abstract class BaseModel : INotifyPropertyChanged
    {
        protected void Notify(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}