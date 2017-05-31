using System;
using System.ComponentModel;
using System.IO;
using DT.Common;
using DT.Common.IO;
using Newtonsoft.Json;
using NLog;

namespace DT.Settings
{
    public abstract class SettingsBase<T> where T : SettingsBase<T>, ISettings, new()
    {
        /// <summary>
        /// Gets the only instance of this class
        /// </summary>
        [JsonIgnore] public static T Instance { get; protected set; }

        [JsonIgnore] private FileSystemWatcher _settingsWatcher;

        [JsonIgnore] protected readonly Logger Log = LogManager.GetCurrentClassLogger();

        [JsonIgnore] private bool _loaded;

        [JsonIgnore] private DateTime _lastWriteTime = DateTime.MinValue;

        [JsonIgnore]
        public bool Loaded
        {
            get => _loaded;
            set => _loaded = value;
        }
        
        private void CreateFileSystemWatcher(string settingsFileLocation, string settingsFileName)
        {
            if (_settingsWatcher != null)
                return;

            // Create a new FileSystemWatcher and set its properties.
            _settingsWatcher = new FileSystemWatcher
            {
                Path = settingsFileLocation,
                Filter = settingsFileName,
                /* Watch for changes in LastWrite times, and
                   the renaming of files or directories. */

                NotifyFilter = NotifyFilters.LastWrite
            };

            // Add event handlers.
            _settingsWatcher.Changed += SettingsWatcherOnChanged;

            // Begin watching.
            _settingsWatcher.EnableRaisingEvents = true;
        }

        public void PauseWatcher()
        {
            if (_settingsWatcher != null)
            {
                _settingsWatcher.EnableRaisingEvents = false;
                // Add event handlers.
                _settingsWatcher.Changed -= SettingsWatcherOnChanged;
            }
        }

        public void StartWatcher()
        {
            if (_settingsWatcher == null)
            {
                CreateFileSystemWatcher(_location, Path.GetFileName(_settingsFile));
            }
            else
            {
                // Add event handlers.
                _settingsWatcher.Changed += SettingsWatcherOnChanged;
                _settingsWatcher.EnableRaisingEvents = true;
            }
        }

        [JsonIgnore] private ProgressForm _loading;

        [JsonIgnore] private BackgroundWorker _backgroundWorker;

        [JsonIgnore] private string _location;
        [JsonIgnore] private string _settingsFile;

        public void StartWatcherDelayed()
        {
            if (Environment.UserInteractive)
            {
                ShowProgressForm();

                StartWorkerThreads();
            }
            else
            {
                FileUtils.GetIdleFile(_settingsFile);
                StartWatcher();
            }
        }

        private void ShowProgressForm()
        {
            _loading = new ProgressForm($"Saving {typeof(T).Name} settings");
            _loading.Show();

            System.Windows.Forms.Application.DoEvents();
        }

        private void StartWorkerThreads()
        {
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += (o, ev) =>
            {
                FileUtils.GetIdleFile(_settingsFile);
                StartWatcher();
                ev.Result = true;
            };

            _backgroundWorker.RunWorkerCompleted += (o, args) => { CloseSaveForm(); };

            _backgroundWorker.RunWorkerAsync();
        }

        private void CloseSaveForm()
        {
            if (_loading == null)
                return;

            if (_loading.InvokeRequired)
            {
                try
                {
                    _loading.Invoke(new Action(CloseSaveForm));
                }
                catch (ObjectDisposedException)
                {
                }
            }
            else
            {
                _loading.Close();
            }
        }

        private void SettingsWatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            DateTime lastWriteTime = File.GetLastWriteTime(e.FullPath);
            if (lastWriteTime != _lastWriteTime)
            {
                Load(_location, _settingsFile);
                _lastWriteTime = lastWriteTime;
            }

        }

        public void SaveSettings()
        {
            TryCreateDirectoryIfNotExists();

            try
            {
                PauseWatcher();

                FileStream myWriter = null;
                try
                {
                    myWriter = WriteSettings();

                    UpdateLastWriteTime();
                }
                finally
                {
                    myWriter?.Dispose();
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                if (Environment.UserInteractive)
                {
                    NotifySaveError(e);
                }
            }
            finally
            {
                StartWatcherDelayed();
            }
        }

        private void UpdateLastWriteTime()
        {
            DateTime lastWriteTime = File.GetLastWriteTime(_settingsFile);
            _lastWriteTime = lastWriteTime;
        }

        private void NotifySaveError(Exception e)
        {
            var text = e.GetType() + "\r\n" + e.Message;
            var caption = $"Saving {typeof(T).Name} settings failed";

            var dialogResult = System.Windows.Forms.MessageBox.Show(text,
                caption, System.Windows.Forms.MessageBoxButtons.RetryCancel);

            if (dialogResult == System.Windows.Forms.DialogResult.Retry)
            {
                SaveSettings();
            }
            else
            {
                Log.Error($"Failed to load {typeof(T).Name} settings, reverting to defaults");
            }
        }

        private FileStream WriteSettings()
        {
            FileStream myWriter = new FileStream(_settingsFile, FileMode.Create, FileAccess.Write,
                FileShare.ReadWrite);

            Serialize(this, myWriter);
            //Ensure ALL data is written to file now
            myWriter.Flush(true);
            return myWriter;
        }

        private void TryCreateDirectoryIfNotExists()
        {
            if (!Directory.Exists(_location))
            {
                try
                {
                    Directory.CreateDirectory(_location);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public static void Serialize(object value, Stream s)
        {
            StreamWriter writer = new StreamWriter(s);
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            JsonSerializer ser = new JsonSerializer {Formatting = Formatting.Indented};
            ser.Serialize(jsonWriter, value);
            jsonWriter.Flush();
        }

        public static T Deserialize<T>(Stream s)
        {
            StreamReader reader = new StreamReader(s);
            JsonTextReader jsonReader = new JsonTextReader(reader);

            JsonSerializer ser = new JsonSerializer();
            return ser.Deserialize<T>(jsonReader);
        }

        public virtual void SetDefaults()
        {
        }
        
        protected void Load(string location, string file)
        {

            _location = location;
            _settingsFile = file;

            TryCreateDirectoryIfNotExists();

            if (!File.Exists(_settingsFile))
            {
                lock (this)
                {
                    Instance.SetDefaults();
                }

                try
                {
                    Instance.SaveSettings();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }

                CreateFileSystemWatcher(_location, Path.GetFileName(_settingsFile));

                return;
            }

            try
            {
                if(!LoadSettingsFromFile(_location, _settingsFile))/*if (!MethodUtils.TryAction(LoadSettingsFromFile, _location, _settingsFile, 3))*/
                {
                    Log.Error($"Failed to load {typeof(T).Name} settings, reverting to defaults");
                    Instance.SetDefaults();
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                NewInstance(location, file);
                Instance.SetDefaults();
            }
            finally
            {
                lock (this)
                {
                    if (Instance == null)
                    {
                        NewInstance(location, file);
                    }
                    Instance.SetDefaults();
                }

                CreateFileSystemWatcher(_location, Path.GetFileName(_settingsFile));

                LogSettingsValues();
            }
        }

        private static void NewInstance(string location, string file)
        {
            Instance = new T
            {
                _settingsFile = file,
                _location = location
            };
        }

        //If you're using this in production code, you probably should
        //gtfo or you'd better have a damn good reason!
        public static void TestMethod_NewInstance()
        {
            Instance = new T();
        }

        public virtual void LogSettingsValues()
        {
        }

        private static bool LoadSettingsFromFile(string location, string file)
        {
            FileUtils.GetIdleFile(file);

            FileStream myReader = null;
            try
            {
                myReader = new FileStream(file, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite);
                Instance = Deserialize<T>(myReader);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                myReader?.Dispose();
            }
        }
    }
}
