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

using Omega_Red.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Omega_Red.ViewModels
{
    [DataTemplateNameAttribute("ScreenshotInfoItem")]
    class MediaRecorderInfoViewModel : BaseViewModel
    {
        public MediaRecorderInfoViewModel()
        {
            PCSX2Controller.Instance.ChangeStatusEvent += (PCSX2Controller.StatusEnum obj) => {

                switch (obj)
                {
                    case PCSX2Controller.StatusEnum.Stopped:
                    case PCSX2Controller.StatusEnum.Paused:
                        IsCheckedStatus = false;
                        break;
                    case PCSX2Controller.StatusEnum.NoneInitilized:
                    case PCSX2Controller.StatusEnum.Initilized:
                    case PCSX2Controller.StatusEnum.Started:
                    default:
                        break;
                }
            
            };
        }


        public ICommand StartStopRecordingCommand
        {
            get
            {
                return new DelegateCommand<Object>(Omega_Red.Managers.MediaRecorderManager.Instance.StartStop, () =>
                {
                    return PCSX2Controller.Instance.Status == PCSX2Controller.StatusEnum.Started;
                });
            }
        }

        bool m_IsCheckedStatus = false;
        
        public bool IsCheckedStatus
        {
            get { return m_IsCheckedStatus; }
            set
            {
                m_IsCheckedStatus = value;
                RaisePropertyChangedEvent("IsCheckedStatus");
            }
        }
                
        protected override Managers.IManager Manager
        {
            get { return Omega_Red.Managers.MediaRecorderManager.Instance; }
        }
    }
}
