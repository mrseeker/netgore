﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GoreUpdater.Manager
{
    /// <summary>
    /// Delegate for handling events from the <see cref="ManagerSettings"/>.
    /// </summary>
    /// <param name="sender">The <see cref="ManagerSettings"/> that the event occured on.</param>
    public delegate void ManagerSettingsEventHandler(ManagerSettings sender);

    /// <summary>
    /// Contains settings for the GoreUpdater manager.
    /// </summary>
    public class ManagerSettings
    {
        const string _headerDelimiter = "=";
        const string _headerLiveVersion = "LIVEVERSION";
        const string _settingsFile = "settings.txt";

        static readonly ManagerSettings _instance;

        readonly string _filePath;

        int _liveVersion = 0;

        /// <summary>
        /// Gets the path to the settings file.
        /// </summary>
        public string FilePath { get { return _filePath; } }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="ManagerSettings"/> is reclaimed by garbage collection.
        /// </summary>
        ~ManagerSettings()
        {
            Save();
        }

        /// <summary>
        /// Notifies listeners when the live version has changed.
        /// </summary>
        public event ManagerSettingsEventHandler LiveVersionChanged;

        /// <summary>
        /// Initializes the <see cref="ManagerSettings"/> class.
        /// </summary>
        static ManagerSettings()
        {
            var filePath = PathHelper.CombineDifferentPaths(Application.StartupPath, _settingsFile);
            _instance = new ManagerSettings(filePath);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerSettings"/> class.
        /// </summary>
        /// <param name="filePath">The settings file path.</param>
        ManagerSettings(string filePath)
        {
            _filePath = filePath;

            Load(FilePath);
        }

        /// <summary>
        /// Loads the settings from file.
        /// </summary>
        void Load(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            var lines = File.ReadAllLines(filePath);
            var values = lines.Select(x => x.Trim()).Where(x => x.Length > 0).Select(GetSettingLine);

            foreach (var v in values)
            {
                switch (v.Key)
                {
                    case _headerLiveVersion:
                        int i;
                        if (!int.TryParse(v.Value, out i))
                        {
                            MessageBox.Show(string.Format("Encountered invalid `{0}` value in settings file.", _headerLiveVersion));
                            break;
                        }

                        _liveVersion = i;
                        break;

                    default:
                        MessageBox.Show("Encountered unknown setting: " + v.Key);
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the global <see cref="ManagerSettings"/> instance.
        /// </summary>
        public static ManagerSettings Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Gets the current live version number.
        /// </summary>
        public int LiveVersion
        {
            get { return _liveVersion; }
        }

        /// <summary>
        /// Adds a settings line to the <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/>.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        static void AddSetting(StringBuilder sb, string key, string value)
        {
            sb.AppendLine(key + _headerDelimiter + value);
        }

        /// <summary>
        /// Reads a settings line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns>The settings line.</returns>
        static KeyValuePair<string, string> GetSettingLine(string line)
        {
            var s = line.Split(new string[] {_headerDelimiter},2, StringSplitOptions.None);
            return new KeyValuePair<string, string>(s[0], s[1]);
        }

        readonly object _saveSync = new object();

        /// <summary>
        /// Saves the settings to file.
        /// </summary>
        void Save()
        {
            lock (_saveSync)
            {
                var sb = new StringBuilder();

                // Write the settings
                AddSetting(sb, _headerLiveVersion, LiveVersion.ToString());

                // Save the file
                var tmpFile = FilePath + ".tmp";
                if (File.Exists(tmpFile))
                    File.Delete(tmpFile);

                File.WriteAllText(tmpFile, sb.ToString());

                File.Copy(tmpFile, FilePath, true);

                try
                {
                    if (File.Exists(tmpFile))
                        File.Delete(tmpFile);
                }
                catch (Exception ex)
                {
                    Debug.Fail(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Tries to set the live version to a new value.
        /// </summary>
        /// <param name="newVersion">The new live version.</param>
        /// <param name="showMessageBox">If true, a <see cref="MessageBox"/> is shown why the change failed.</param>
        /// <returns>True if the live version was successfully changed; otherwise false.</returns>
        public bool TrySetLiveVersion(int newVersion, bool showMessageBox = true)
        {
            if (newVersion < LiveVersion)
            {
                if (showMessageBox)
                    MessageBox.Show("Cannot set the live version to a version earlier than the current live version!");
                return false;
            }

            if (newVersion == LiveVersion)
                return false;

            // TODO: Make sure the version exists

            _liveVersion = newVersion;

            Save();

            if (LiveVersionChanged != null)
                LiveVersionChanged(this);

            return true;
        }
    }
}