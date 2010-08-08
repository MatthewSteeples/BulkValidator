using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Validator
{
    [Serializable]
    public class VerifiedPage : INotifyPropertyChanged
    {
        string url;
        DateTime lastChecked;
        string result;
        DateTime lastModified;
        

        public string Url { get { return url; } set { url = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Url")); } }
        public DateTime LastChecked { get { return lastChecked; } set { lastChecked = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("LastChecked")); } }
        public string Result { get { return result; } set { result = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Result")); } }
        public DateTime LastModified { get { return lastModified; } set { lastModified = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("LastModified")); } }

        public string ETag { get; set; }
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
