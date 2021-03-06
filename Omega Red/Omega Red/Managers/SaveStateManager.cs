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

using Omega_Red.Managers;
using Omega_Red.Models;
using Omega_Red.Properties;
using Omega_Red.Tools;
using Omega_Red.Tools.Panels;
using Omega_Red.Tools.Savestate;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace Omega_Red.Managers
{
    class SaveStateManager : IManager
    {
        class Compare : IEqualityComparer<SaveStateInfo>
        {
            public bool Equals(SaveStateInfo x, SaveStateInfo y)
            {
                if (x.Index == y.Index)
                {
                    return true;
                }
                else { return false; }
            }
            public int GetHashCode(SaveStateInfo codeh)
            {
                return 0;
            }

        }

        private ICollectionView mCustomerView = null;

        private readonly ObservableCollection<SaveStateInfo> _saveStateInfoCollection = new ObservableCollection<SaveStateInfo>();

        private List<int> m_ListSlotIndexes = new List<int>();
        
        private SaveStateInfo m_autoSave = new SaveStateInfo();

        private static SaveStateManager m_Instance = null;

        public static SaveStateManager Instance { get { if (m_Instance == null) m_Instance = new SaveStateManager(); return m_Instance; } }
        
        private SaveStateManager()
        {
            mCustomerView = CollectionViewSource.GetDefaultView(_saveStateInfoCollection);

            mCustomerView.SortDescriptions.Add(
                new SortDescription("DateTime", ListSortDirection.Descending));
        }
        
        public void init()
        {
            if (string.IsNullOrEmpty(Settings.Default.SlotFolder))
                Settings.Default.SlotFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\PCSX2\sstates\";

            if (!System.IO.Directory.Exists(Settings.Default.SlotFolder))
            {
                System.IO.Directory.CreateDirectory(Settings.Default.SlotFolder);
            }

            PCSX2Controller.Instance.ChangeStatusEvent += Instance_m_ChangeStatusEvent;
        }
        
        private void load()
        {
            _saveStateInfoCollection.Clear();

            m_ListSlotIndexes.Clear();

            for (int i = 0; i < 101; i++)
            {
                m_ListSlotIndexes.Add(i);
            }

            if (string.IsNullOrEmpty(Settings.Default.SlotFolder))
                Settings.Default.SlotFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\PCSX2\sstates\";

            if (System.IO.Directory.Exists(Settings.Default.SlotFolder))
            {

                string[] files = System.IO.Directory.GetFiles(Settings.Default.SlotFolder, "*.p2s");

                foreach (string s in files)
                {
                    // Create the FileInfo object only when needed to ensure
                    // the information is as current as possible.
                    System.IO.FileInfo fi = null;
                    try
                    {
                        fi = new System.IO.FileInfo(s);
                    }
                    catch (System.IO.FileNotFoundException e)
                    {
                        // To inform the user and continue is
                        // sufficient for this demonstration.
                        // Your application may require different behavior.
                        Console.WriteLine(e.Message);
                        continue;
                    }

                    if (fi.Name.Contains(PCSX2Controller.Instance.IsoInfo.DiscSerial))
                    {
                        var l_splits = fi.Name.Split(new char[] { '.' });

                        if (l_splits != null && l_splits.Length == 3)
                        {
                            int l_value = 0;

                            if (int.TryParse(l_splits[1], out l_value))
                            {
                                try
                                {
                                    addSaveStateInfo(SStates.Instance.readData(s, l_value));

                                    m_ListSlotIndexes.Remove(l_value);
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }
                }
            }
            
            for (int i = 0; i < 1; i++)
            {
                int l_index = i + 100;

                var lautoSaveState = new SaveStateInfo()
                {
                    IsAutosave = true,
                    FilePath = Settings.Default.SlotFolder +
                        PCSX2Controller.Instance.IsoInfo.DiscSerial + ".auto." +
                        l_index.ToString() + ".p2s"

                };

                if(File.Exists(lautoSaveState.FilePath))
                {
                    try
                    {
                        addSaveStateInfo(SStates.Instance.readData(lautoSaveState.FilePath, l_index, lautoSaveState.IsAutosave));

                        m_ListSlotIndexes.Remove(l_index);
                    }
                    catch (Exception)
                    {
                    }
                }

                m_autoSave = lautoSaveState;
            }
        }
        
        void Instance_m_ChangeStatusEvent(PCSX2Controller.StatusEnum a_Status)
        {
            if (a_Status != PCSX2Controller.StatusEnum.NoneInitilized)
                load();
        }

        private SaveStateInfo createSaveStateInfo()
        {
            int lIndex = -1;

            uint lCheckSum = 0;

            string lFilePath = "";

            if (m_ListSlotIndexes.Count > 0)
            {
                lIndex = m_ListSlotIndexes[0];

                m_ListSlotIndexes.RemoveAt(0);

                lCheckSum = PCSX2Controller.Instance.BiosInfo.CheckSum;

                var lIndexString = lIndex.ToString();
                
                if(lIndexString.Length == 1)
                    lIndexString = lIndexString.PadLeft(2, '0');

                lFilePath = Settings.Default.SlotFolder +
                         PCSX2Controller.Instance.IsoInfo.DiscSerial + "." +
                         lIndexString + ".p2s";

            }

            return new SaveStateInfo() { Index = lIndex, CheckSum = lCheckSum, Visibility = Visibility.Collapsed, FilePath = lFilePath };
        }

        public void addSaveStateInfo()
        {
            addSaveStateInfo(createSaveStateInfo());
        }
        
        private void addSaveStateInfo(SaveStateInfo a_SaveStateInfo)
        {
            if (PCSX2Controller.Instance.BiosInfo.CheckSum != a_SaveStateInfo.CheckSum)
                return;

            if (!_saveStateInfoCollection.Contains(a_SaveStateInfo, new Compare()))
            {
                _saveStateInfoCollection.Add(a_SaveStateInfo);
            }
        }

        private void addSaveStateInfo(string a_file_path, string a_file_name)
        {
            var l_splits = a_file_name.Split(new char[] { '.' });

            if (l_splits != null && l_splits.Length == 3)
            {
                int l_value = 0;

                if (int.TryParse(l_splits[1], out l_value))
                {
                    addSaveStateInfo(SStates.Instance.readData(a_file_path, l_value));
                }
            }
        }

        public void removeSaveStateInfo(SaveStateInfo a_SaveStateInfo)
        {
            removeSaveStateInfo(a_SaveStateInfo, false);
        }

        public void removeSaveStateInfo(SaveStateInfo a_SaveStateInfo, bool a_clear)
        {
            if (_saveStateInfoCollection.Contains(a_SaveStateInfo, new Compare()))
            {
                _saveStateInfoCollection.Remove(a_SaveStateInfo);
            }

            if(a_clear)
            {

                System.Threading.ThreadPool.QueueUserWorkItem((object state) =>
                {
                    
                    System.IO.FileInfo fi = null;

                    try
                    {
                        fi = new System.IO.FileInfo(a_SaveStateInfo.FilePath);
                    }
                    catch (System.IO.FileNotFoundException e)
                    {
                        // To inform the user and continue is
                        // sufficient for this demonstration.
                        // Your application may require different behavior.
                        Console.WriteLine(e.Message);
                    }

                    if (fi.Name.Contains(PCSX2Controller.Instance.IsoInfo.DiscSerial))
                    {
                        var l_splits = fi.Name.Split(new char[] { '.' });

                        if (l_splits != null && l_splits.Length == 3)
                        {
                            int l_value = 0;

                            if (int.TryParse(l_splits[1], out l_value))
                            {
                                var l_SaveStateInfo = SStates.Instance.readData(a_SaveStateInfo.FilePath, l_value);

                                if (l_SaveStateInfo != null)
                                {
                                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (System.Threading.ThreadStart)delegate()
                                    {
                                        addSaveStateInfo(l_SaveStateInfo);
                                    });
                                }
                            }
                        }
                        else
                        if (l_splits != null && l_splits.Length == 4 && l_splits[1] == "auto")
                        {
                            int l_value = 0;

                            if (int.TryParse(l_splits[2], out l_value))
                            {
                                var l_SaveStateInfo = SStates.Instance.readData(a_SaveStateInfo.FilePath, l_value);

                                if (l_SaveStateInfo != null)
                                {
                                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (System.Threading.ThreadStart)delegate()
                                    {
                                        addSaveStateInfo(l_SaveStateInfo);
                                    });
                                }
                            }
                        }
                    }
                });
            }
            else
            {
                if (File.Exists(a_SaveStateInfo.FilePath))
                    File.Delete(a_SaveStateInfo.FilePath);
            }
        }
        
        public void saveState(SaveStateInfo a_SaveStateInfo, string aDate, double aDurationInSeconds)
        {
            if (System.IO.Directory.Exists(Settings.Default.SlotFolder))
            {
                Tools.Savestate.SStates.Instance.Save(a_SaveStateInfo.FilePath, aDate, aDurationInSeconds);

                removeSaveStateInfo(a_SaveStateInfo, true);
            }
        }

        public void loadState(SaveStateInfo a_SaveStateInfo)
        {
            if (System.IO.File.Exists(a_SaveStateInfo.FilePath))
            {
                Tools.Savestate.SStates.Instance.Load(a_SaveStateInfo.FilePath);
            }
        }

        public void autoSave(string aDate, double aDurationInSeconds)
        {
            if (System.IO.Directory.Exists(Settings.Default.SlotFolder))
            {
                m_autoSave.Date = aDate;

                m_autoSave.Duration = TimeSpan.FromSeconds(aDurationInSeconds).ToString(@"dd\.hh\:mm\:ss");

                var l_file_path = m_autoSave.FilePath + "_temp";

                Tools.Savestate.SStates.Instance.Save(l_file_path, aDate, aDurationInSeconds);

                File.Delete(m_autoSave.FilePath);

                File.Move(l_file_path, m_autoSave.FilePath);
            }
        }

        public void removeItem(object a_Item)
        {
            var l_SaveStateInfo = a_Item as SaveStateInfo;

            if (l_SaveStateInfo != null)
            {
                removeSaveStateInfo(l_SaveStateInfo);
            }
        }

        public void createItem(){}

        public ICollectionView Collection
        {
            get { return mCustomerView; }
        }
    }
}
