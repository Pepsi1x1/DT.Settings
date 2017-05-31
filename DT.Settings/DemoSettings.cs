using System.Collections.Generic;
using System.Linq;

namespace DT.Settings
{
    public class DemoSettings : SettingsBase<DemoSettings>, ISettings
    {
        private List<string> _listOfStrings;

        public List<string> ListOfStrings
        {
            get { return _listOfStrings; }
            set { _listOfStrings = value; }
        }

        public string ASingleString { get; set; }

        public bool SomeBoolean { get; set; }

        public void SetDefaults(bool shouldSave = true)
        {
            bool outOfDate = false;

            if (string.IsNullOrWhiteSpace(Instance.ASingleString))
            {
                outOfDate = true;
                Instance.ASingleString = "1972";
            }

            if (Instance.ListOfStrings == null)
            {
                outOfDate = true;
                Instance.ListOfStrings = new List<string>();
            }

            if (outOfDate)
            {
                if (shouldSave)
                {
                    SaveSettings();
                }
            }
        }

        public override void LogSettingsValues()
        {
            Log.Info("Reloading demo settings");
            Log.Info("");

            Log.Info("ASingleString: {0}", Instance.ASingleString);

            Log.Info("");
            Log.Info("Shared demo reloaded");
            Log.Info("");
        }
    }
}
