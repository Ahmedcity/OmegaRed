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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Omega_Red.Tools.Panels
{
    /// <summary>
    /// Interaction logic for ControlPanel.xaml
    /// </summary>
    public partial class ControlPanel : UserControl
    {
        public ControlPanel()
        {
            InitializeComponent();
            
            m_Panels.AddHandler(Expander.ExpandedEvent, new RoutedEventHandler(Expander_Expanded));

            ConfigManager.Instance.SwitchControlModeEvent += Instance_SwitchControlModeEvent;
        }

        void Instance_SwitchControlModeEvent(bool obj)
        {
            var l_ParentElement = this.Parent as FrameworkElement;

            if (l_ParentElement == null)
                return;

            var l_TouchDragBtnWidth = (double)App.Current.Resources["TouchDragBtnWidth"];

            if(obj)
            {
                var l_PanelWidth = (double)App.Current.Resources["PanelWidth"];

                this.Width = l_PanelWidth;

                Canvas.SetLeft(l_ParentElement, -l_PanelWidth);
            }
            else
            {
                Binding l_Binding = new Binding("ActualWidth");

                l_Binding.Source = App.Current.MainWindow;

                l_Binding.Mode = BindingMode.OneWay;

                l_Binding.Converter = new Omega_Red.Tools.Converters.WidthConverter() { Offset = -16 - l_TouchDragBtnWidth, Scale = 1.0 };

                this.SetBinding(UserControl.WidthProperty, l_Binding);
            }
        }
       
        protected override Size MeasureOverride(Size constraint)
        {
            resize();

            return base.MeasureOverride(constraint);
        }

        private void resize()
        {
            double l_expendedheight = 0.0;

            Expander l_extendedExpander = null;

            var l_actualHeight = m_Panels.ActualHeight;

            for (int i = 0; i < m_Panels.Items.Count; i++)
            {
                Expander l_ItemExpander = m_Panels.Items.GetItemAt(i) as Expander;

                if (l_ItemExpander == null)
                    continue;

                if (!l_ItemExpander.IsExpanded)
                {
                    double l_VertHeight = 0.0;

                    if (l_ItemExpander.Margin != null)
                    {
                        l_VertHeight = l_ItemExpander.Margin.Top + l_ItemExpander.Margin.Bottom;
                    }

                    if (l_ItemExpander.Padding != null)
                    {
                        l_VertHeight += l_ItemExpander.Padding.Top + l_ItemExpander.Padding.Bottom;
                    }

                    l_expendedheight += l_ItemExpander.ActualHeight + l_VertHeight;
                }
                else
                {
                    l_extendedExpander = l_ItemExpander;
                }
            }

            if (l_extendedExpander != null)
            {
                var l_headerexpendedheight = l_expendedheight / ((double)m_Panels.Items.Count);

                var l_control = (l_extendedExpander.Content as FrameworkElement);

                if (l_control != null)
                {
                    var l_newHeight = l_actualHeight - l_expendedheight - l_headerexpendedheight;

                    if (l_newHeight > 0.0)
                        l_control.Height = l_newHeight;
                }
            }
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            Expander l_Expander = e.OriginalSource as Expander;

            if (l_Expander == null)
                return;

            var l_index = m_Panels.Items.IndexOf(l_Expander);

            for (int i = 0; i < m_Panels.Items.Count; i++)
            {
                Expander l_ItemExpander = m_Panels.Items.GetItemAt(i) as Expander;

                if (l_ItemExpander == null || l_index == i)
                    continue;

                l_ItemExpander.IsExpanded = false;
            }

            this.UpdateLayout();
        }

    }
}
