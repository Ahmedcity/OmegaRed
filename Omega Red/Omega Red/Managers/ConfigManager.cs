﻿/*  Omega Red - Client PS2 Emulator for PCs
*
*  Omega Red is free software: you can redistribute it and/or modify it under the terms
*  of the GNU Lesser General Public License as published by the Free Software Found-
*  ation, either version 3 of the License, or (at your option) any later version.
*
*  Omega Red is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
*  without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR
*  PURPOSE.  See the GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License along with Omega Red.
*  If not, see <http://www.gnu.org/licenses/>.
*/

using Omega_Red.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Omega_Red.Managers
{
    class ConfigManager
    {
        enum DisplayMode
        {
            Window = 0,
            Full = 1
        }

        class DisplayModeInfo
        {
            public DisplayMode Value { get; set; }

            public override string ToString()
            {
                return Value == DisplayMode.Window ? App.Current.Resources["DisplayModeWindowTitle"] as String :
                    App.Current.Resources["DisplayModeFullTitle"] as String;
            }
        }

        enum ControlMode
        {
            Button = 0,
            Touch = 1
        }

        class ControlModeInfo
        {
            public ControlMode Value { get; set; }

            public override string ToString()
            {
                return Value == ControlMode.Button ? App.Current.Resources["ControlModeButtonTitle"] as String :
                    App.Current.Resources["ControlModeTouchTitle"] as String;
            }
        }

        private ICollectionView mDisplayModeView = null;

        private readonly ObservableCollection<DisplayModeInfo> _displayModeCollection = new ObservableCollection<DisplayModeInfo>();




        private ICollectionView mControlModeView = null;

        private readonly ObservableCollection<ControlModeInfo> _controlModeCollection = new ObservableCollection<ControlModeInfo>();





        private ICollectionView mLanguageModeView = null;

        private readonly ObservableCollection<String> _languageModeCollection = new ObservableCollection<String>();



        private ResourceDictionary m_languageResource = null;

        private static ConfigManager m_Instance = null;

        public static ConfigManager Instance { get { if (m_Instance == null) m_Instance = new ConfigManager(); return m_Instance; } }

        public event Action<bool> SwitchDisplayModeEvent;

        public event Action<bool> SwitchControlModeEvent;

        public event Action<bool> SwitchTopmostEvent;
        
        private ConfigManager()
        {
            System.Reflection.Assembly l_assembly = Assembly.GetExecutingAssembly();

            _languageModeCollection.Add("Русский");

            foreach (var item in l_assembly.GetManifestResourceNames())
            {
                if (item.Contains("Omega_Red.Captions.Translates."))
                {
                    _languageModeCollection.Add(item.Replace("Omega_Red.Captions.Translates.", "").Replace(".xaml", ""));
                }
            }

            mLanguageModeView = CollectionViewSource.GetDefaultView(_languageModeCollection);

            mLanguageModeView.MoveCurrentTo(Settings.Default.Language);
            



            _displayModeCollection.Add(new DisplayModeInfo() { Value = DisplayMode.Window });

            _displayModeCollection.Add(new DisplayModeInfo() { Value = DisplayMode.Full });

            mDisplayModeView = CollectionViewSource.GetDefaultView(_displayModeCollection);



            _controlModeCollection.Add(new ControlModeInfo() { Value = ControlMode.Button });

            _controlModeCollection.Add(new ControlModeInfo() { Value = ControlMode.Touch });

            mControlModeView = CollectionViewSource.GetDefaultView(_controlModeCollection);



            mDisplayModeView.CurrentChanged += mDisplayModeView_CurrentChanged;

            mControlModeView.CurrentChanged += mControlModeView_CurrentChanged;

            mLanguageModeView.CurrentChanged += mLanguageModeView_CurrentChanged;



            DisplayMode l_DisplayMode = DisplayMode.Window;

            Enum.TryParse<DisplayMode>(Settings.Default.DisplayMode, out l_DisplayMode);

            mDisplayModeView.MoveCurrentToPosition((int)l_DisplayMode);



            ControlMode l_ControlMode = ControlMode.Button;

            Enum.TryParse<ControlMode>(Settings.Default.ControlMode, out l_ControlMode);

            mControlModeView.MoveCurrentToPosition((int)l_ControlMode);

            if (SwitchTopmostEvent != null)
                SwitchTopmostEvent(Settings.Default.Topmost);
        }

        void mLanguageModeView_CurrentChanged(object sender, EventArgs e)
        {
            if (mLanguageModeView == null)
                return;

            if (mLanguageModeView.CurrentItem == null)
                return;

            if (App.Current == null)
                return;

            if (App.Current.MainWindow == null)
                return;

            loadLanguage(mLanguageModeView.CurrentItem as string);

        }

        private void loadLanguage(string a_language)
        {
            System.Reflection.Assembly l_assembly = Assembly.GetExecutingAssembly();

            using (var lStream = l_assembly.GetManifestResourceStream(
                string.Format("Omega_Red.Captions.Translates.{0}.xaml", a_language)))
            {
                if (m_languageResource != null)
                    App.Current.Resources.MergedDictionaries.Remove(m_languageResource);

                m_languageResource = null;

                if (lStream == null)
                {
                    Settings.Default.Language = "Русский";

                    Settings.Default.Save();

                    return;
                }

                m_languageResource = (ResourceDictionary)XamlReader.Load(lStream);

                if (m_languageResource != null)
                {
                    App.Current.Resources.MergedDictionaries.Add(m_languageResource);

                    Settings.Default.Language = a_language;

                    Settings.Default.Save();
                }
            }
        }

        void mControlModeView_CurrentChanged(object sender, EventArgs e)
        {
            if (mControlModeView == null)
                return;

            if (mControlModeView.CurrentItem == null)
                return;

            if (App.Current == null)
                return;

            if (App.Current.MainWindow == null)
                return;

            var l_ControlMode = (ControlModeInfo)mControlModeView.CurrentItem;

            if (SwitchControlModeEvent == null)
                return;

            switch (l_ControlMode.Value)
            {
                case ControlMode.Button:
                    {
                        var l_PanelWidth = App.Current.MainWindow.Resources["mControlWidthOffset"] as Omega_Red.Tools.Converters.WidthConverter;

                        if (l_PanelWidth != null)
                            l_PanelWidth.Offset = 0.0;

                        SwitchControlModeEvent(true);
                    }
                    break;
                case ControlMode.Touch:
                    {

                        var l_TouchDragBtnWidth = (double)App.Current.Resources["TouchDragBtnWidth"];

                        var l_PanelWidth = App.Current.MainWindow.Resources["mControlWidthOffset"] as Omega_Red.Tools.Converters.WidthConverter;

                        if (l_PanelWidth != null)
                            l_PanelWidth.Offset = -l_TouchDragBtnWidth;

                        SwitchControlModeEvent(false);
                    }
                    break;
                default:
                    break;
            }


            Settings.Default.ControlMode = l_ControlMode.Value.ToString();

            Settings.Default.Save();
        }

        void mDisplayModeView_CurrentChanged(object sender, EventArgs e)
        {
            if (mDisplayModeView.CurrentItem == null)
                return;

            var l_DisplayMode = (DisplayModeInfo)mDisplayModeView.CurrentItem;

            if (SwitchDisplayModeEvent == null)
                return;

            switch (l_DisplayMode.Value)
            {
                case DisplayMode.Full:
                    SwitchDisplayModeEvent(true);
                    break;
                case DisplayMode.Window:
                    SwitchDisplayModeEvent(false);
                    break;
                default:
                    break;
            }

            Settings.Default.DisplayMode = l_DisplayMode.Value.ToString();

            Settings.Default.Save();
        }
        
        public ICollectionView DisplayModeCollection
        {
            get { return mDisplayModeView; }
        }

        public ICollectionView ControlModeCollection
        {
            get { return mControlModeView; }
        }

        public ICollectionView LanguageCollection
        {
            get { return mLanguageModeView; }
        }              
    }
}
