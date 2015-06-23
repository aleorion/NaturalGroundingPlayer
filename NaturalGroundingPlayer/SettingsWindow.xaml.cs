﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using Business;
using DataAccess;
using System.Windows.Navigation;
using System.Diagnostics;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window {
        public static void Instance() {
            SettingsWindow NewForm = new SettingsWindow();
            SessionCore.Instance.Windows.ShowDialog(NewForm);
        }

        private SettingsFile settings;
        private WindowHelper helper;
        private MediaPlayerApplication currentPlayer;

        public static readonly DependencyProperty IsMpcPlayerPropertyProperty =
            DependencyProperty.Register("IsMpcPlayer", typeof(bool), typeof(SettingsWindow), null);
        public bool IsMpcPlayer {
            get { return (bool)GetValue(IsMpcPlayerPropertyProperty); }
            set { SetValue(IsMpcPlayerPropertyProperty, value); }
        }

        public SettingsWindow() {
            InitializeComponent();
            helper = new WindowHelper(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            FillComboBoxes();
            if (Settings.SavedFile != null)
                settings = Settings.SavedFile.Copy();
            else
                settings = SettingsFile.DefaultValues();
            this.DataContext = settings;
            currentPlayer = settings.MediaPlayerApp;
            MediaPlayerCombo_LostKeyboardFocus(null, null);
        }

        private void FillComboBoxes() {
            List<ListItem<double>> ZoomList = new List<ListItem<double>>();
            ZoomList.Add(new ListItem<double>("100%", 1));
            ZoomList.Add(new ListItem<double>("110%", 1.1));
            ZoomList.Add(new ListItem<double>("120%", 1.2));
            ZoomList.Add(new ListItem<double>("130%", 1.3));
            ZoomList.Add(new ListItem<double>("140%", 1.4));
            ZoomList.Add(new ListItem<double>("150%", 1.5));
            ZoomComboBox.ItemsSource = ZoomList;

            List<ListItem<int>> DownloadQualityList = new List<ListItem<int>>();
            DownloadQualityList.Add(new ListItem<int>("Max", 0));
            DownloadQualityList.Add(new ListItem<int>("1080p", 1080));
            DownloadQualityList.Add(new ListItem<int>("720p", 720));
            DownloadQualityList.Add(new ListItem<int>("480p", 480));
            DownloadQualityList.Add(new ListItem<int>("360p", 360));
            DownloadQualityList.Add(new ListItem<int>("240p", 240));
            MaxDownloadQualityCombo.ItemsSource = DownloadQualityList;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e) {
            SaveButton.Focus();
            string Errors = settings.Validate();
            ErrorText.Text = Errors;
            if (Errors == null) {
                bool IsFirstRun = (Settings.SavedFile == null);
                Settings.SavedFile = settings;
                settings.Save();
                this.Close();

                if (settings.MediaPlayerApp == MediaPlayerApplication.Mpc) {
                    MpcConfigBusiness.ConfigureSettings();
                    SessionCore.Instance.Business.ConfigurePlayer();
                }

                if (IsFirstRun || settings.MediaPlayerApp != currentPlayer)
                    SessionCore.Instance.Business.SetPlayer(SessionCore.Instance.GetNewPlayer());

                // Auto-bind files
                EditPlaylistBusiness BindBusiness = new EditPlaylistBusiness();
                SearchSettings BindSettings = new SearchSettings();
                BindSettings.SetCondition(FieldConditionEnum.IsInDatabase, BoolConditionEnum.Yes);
                await BindBusiness.LoadPlaylistAsync(BindSettings);
                await BindBusiness.LoadMediaInfoAsync(null);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (Settings.SavedFile == null)
                App.Current.Shutdown();
        }

        private void BrowseSvp_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            try {
                if (!string.IsNullOrEmpty(settings.SvpPath))
                    dlg.InitialDirectory = Path.GetDirectoryName(settings.SvpPath);
            }
            catch { }
            dlg.Filter = "Executable File|*.exe";
            if (dlg.ShowDialog(IsLoaded ? this : Owner).Value == true) {
                settings.SvpPath = dlg.FileName;
            }
        }

        /// <summary>
        /// Represents an item to display in the ComboBox.
        /// </summary>
        public class ListItem<T> {
            public string Text { get; set; }
            public T Value { get; set; }

            public ListItem() {
            }

            public ListItem(string text, T value) {
                this.Text = text;
                this.Value = value;
            }
        }

        private void MediaPlayerCombo_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            IsMpcPlayer = (settings.MediaPlayerApp == MediaPlayerApplication.Mpc);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
