﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Business {
    [PropertyChanged.ImplementPropertyChanged]
    public class SettingsFile {
        public string NaturalGroundingFolder { get; set; }
        public string SvpPath { get; set; }
        public bool AutoDownload { get; set; }
        public bool EnableSvp { get; set; }
        public bool EnableMadVR { get; set; }
        public bool Widescreen { get; set; }
        public MediaPlayerApplication MediaPlayerApp { get; set; }
        public int MaxDownloadQuality { get; set; }
        public double Zoom { get; set; }

        public SettingsFile() {
            AutoDownload = true;
            EnableSvp = true;
            EnableMadVR = false;
            MediaPlayerApp = MediaPlayerApplication.Mpc;
            MaxDownloadQuality = 0; // Max
            Zoom = 1;
        }

        /// <summary>
        /// Sets default values for this system.
        /// </summary>
        public static SettingsFile DefaultValues() {
            SettingsFile Result = new SettingsFile();
            string Root = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
            Result.NaturalGroundingFolder = Root + "Natural Grounding\\";
            Result.SvpPath = Root + @"Program Files (x86)\SVP\SVPMgr.exe";
            if (!File.Exists(Result.SvpPath))
                Result.SvpPath = Root + @"Program Files\SVP\SVPMgr.exe";
                //Result.MpcPath = Root + @"Program Files (x86)\SVP\MPC-HC\mpc-hc.exe";
                //Result.MpcPath = Root + @"Program Files\SVP\MPC-HC\mpc-hc.exe";
            return Result;
        }

        /// <summary>
        /// Loads settings from the XML file
        /// </summary>
        /// <returns>The object created from the xml file</returns>
        public static SettingsFile Load() {
            using (var stream = System.IO.File.OpenRead(Settings.SettingsPath)) {
                var serializer = new XmlSerializer(typeof(SettingsFile));
                SettingsFile Result = serializer.Deserialize(stream) as SettingsFile;
                if (Result.Zoom < 1)
                    Result.Zoom = 1;
                else if (Result.Zoom > 1.5)
                    Result.Zoom = 1.5;
                return Result;
            }
        }

        /// <summary>
        /// Saves to an xml file
        /// </summary>
        public void Save() {
            Directory.CreateDirectory(Path.GetDirectoryName(Settings.SettingsPath));
            // Directory.CreateDirectory(NaturalGroundingFolder);

            using (var writer = new System.IO.StreamWriter(Settings.SettingsPath)) {
                var serializer = new XmlSerializer(typeof(SettingsFile));
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                serializer.Serialize(writer, this, ns);
                writer.Flush();
            }
            Settings.RaiseSettingsChanged();
        }

        public string Validate() {
            if (!NaturalGroundingFolder.EndsWith("\\"))
                NaturalGroundingFolder += "\\";
            if (SvpPath.Length == 0)
                EnableSvp = false;
            if (!Path.IsPathRooted(NaturalGroundingFolder))
                return "Invalid Natural Grounding folder";

            if (MediaPlayerApp == MediaPlayerApplication.Mpc) {
                if (MpcConfigBusiness.MpcPath(SvpPath) == null)
                    return "MPC-HC not found";
                if (SvpPath.Length > 0 && !File.Exists(SvpPath))
                    return "Invalid SVP path";
            }

            return null;
        }

        public SettingsFile Copy() {
            SettingsFile Result = new SettingsFile();
            Result.NaturalGroundingFolder = NaturalGroundingFolder;
            Result.SvpPath = SvpPath;
            Result.AutoDownload = AutoDownload;
            Result.EnableSvp = EnableSvp;
            Result.EnableMadVR = EnableMadVR;
            Result.Widescreen = Widescreen;
            Result.MediaPlayerApp = MediaPlayerApp;
            Result.MaxDownloadQuality = MaxDownloadQuality;
            Result.Zoom = Zoom;
            return Result;
        }
    }
}
