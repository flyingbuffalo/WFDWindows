using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Threading;
using Windows.UI.Core;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.System.Threading;
using System.Diagnostics;
using Buffalo.WiFiDirect;

// 빈 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace WFDWindowsSample
{
    /// <summary>
    /// 자체에서 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private WFDManager manager;
        WFDDevice device;
        WFDPairInfo pairInfo;
        
        DiscoveredListener discoveredListener;

        public MainPage()
        {
            this.InitializeComponent();
            manager = new WFDManager(this);
            discoveredListener = new DiscoveredListener(this);
        }

        private void btnFindDevices_Click(object sender, RoutedEventArgs e)
        {
            manager.getDevicesAsync(discoveredListener);
            tbMessage.Text = "Finding Devices...";
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            device = comboDeviceList.SelectedItem as WFDDevice;
            manager.pairAsync(device, discoveredListener);
        }


        public class DiscoveredListener : WFDDeviceDiscoveredListener, WFDDeviceConnectedListener, WFDPairInfo.PairSocketConnectedListener
        {
            MainPage parent;
            public DiscoveredListener(MainPage parent)
            {
                this.parent = parent;
            }
            
            public async void onDevicesDiscovered(List<WFDDevice> deviceList)
            {
                ObservableCollection<WFDDevice> devList = new ObservableCollection<WFDDevice>(deviceList);
                parent.comboDeviceList.ItemsSource = devList;

                if (deviceList.Count != 0)
                {
                    foreach (WFDDevice dev in deviceList)
                    {
                        Debug.WriteLine(dev.Name);
                    }
                    parent.comboDeviceList.SelectedIndex = 0;
                    parent.tbMessage.Text = "Found " + deviceList.Count;
                }
                else
                {
                    parent.tbMessage.Text = "Found Not";
                }
            }

            public void onDevicesDiscoverFailed(int reasonCode)
            {

            }
    

            //connceted
            public void onDeviceConnected(WFDPairInfo pair)
            {
                parent.pairInfo = pair;
                Debug.WriteLine("paring");

                pair.connectSocketAsync(this);

            }

            public void onDeviceConnectFailed(int reasonCode)
            {
                
            }

            public void onDeviceDisconnected()
            {

            }

            public void onSocketConnected(StreamSocket s)
            {

            }
        }
    }
}




/*
 // Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SDKTemplate.Common;

namespace SDKTemplate
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;

        public MainPage()
        {
            this.InitializeComponent();
            SampleTitle.Text = FEATURE_NAME;

            // This is a static public property that allows downstream pages to get a handle to the MainPage instance
            // in order to call methods that are in this class.
            Current = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Populate the scenario list from the SampleConfiguration.cs file
            ScenarioControl.ItemsSource = scenarios;

            // If we have saved state return to the previously selected scenario  
            if (SuspensionManager.SessionState.ContainsKey("SelectedScenarioIndex"))
            {
                ScenarioControl.SelectedIndex = Convert.ToInt32(SuspensionManager.SessionState["SelectedScenarioIndex"]);
                ScenarioControl.ScrollIntoView(ScenarioControl.SelectedItem);   
            }
            else
            {
                ScenarioControl.SelectedIndex = 0;
            }

        }

        /// <summary>
        /// Called whenever the user changes selection in the scenarios list.  This method will navigate to the respective
        /// sample scenario page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScenarioControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Clear the status block when navigating scenarios.
            NotifyUser(String.Empty, NotifyType.StatusMessage);

            ListBox scenarioListBox = sender as ListBox;
            Scenario s = scenarioListBox.SelectedItem as Scenario;
            if (s != null)
            {
                SuspensionManager.SessionState["SelectedScenarioIndex"] = scenarioListBox.SelectedIndex;
                ScenarioFrame.Navigate(s.ClassType);
            }
                        
        }

        public List<Scenario> Scenarios
        {
            get { return this.scenarios; }
        }

        /// <summary>
        /// Used to display messages to the user
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="type"></param>
        public void NotifyUser(string strMessage, NotifyType type)
        {
            switch (type)
            {
                case NotifyType.StatusMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                    break;
                case NotifyType.ErrorMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                    break;
            }
            StatusBlock.Text = strMessage;

            // Collapse the StatusBlock if it has no text to conserve real estate.
            if (StatusBlock.Text != String.Empty)
            {
                StatusBorder.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                StatusBorder.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        async void Footer_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(((HyperlinkButton)sender).Tag.ToString()));
        }

    }

    public enum NotifyType
    {
        StatusMessage,
        ErrorMessage
    };

    public class ScenarioBindingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Scenario s = value as Scenario;
            return (MainPage.Current.Scenarios.IndexOf(s) + 1) + ") " + s.Title;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return true;
        }
    }

}

 */