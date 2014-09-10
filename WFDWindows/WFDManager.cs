using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Windows.UI.Core;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.System.Threading;
using Windows.Devices.WiFiDirect;
using Windows.Devices.Enumeration;


namespace Buffalo.WiFiDirect
{
    public class WFDManager
    {
        private IAsyncAction m_workItem;
        //private delegate void WorkItemHandler(IAsyncAction operation);

        public void getDevicesAsync(WFDDeviceDiscoveredListener l)
        {
            List<WFDDevice> wfdList = new List<WFDDevice>();

            IAsyncAction asyncAction = ThreadPool.RunAsync( async (workItem) =>
            {

                string wfdSelector = WiFiDirectDevice.GetDeviceSelector(); ;
                DeviceInformationCollection devInfoCollection = await DeviceInformation.FindAllAsync(wfdSelector);

                foreach (DeviceInformation devInfo in devInfoCollection)
                {
                    wfdList.Add(new WFDDevice(devInfo));
                }

                if (workItem.Status == AsyncStatus.Canceled)
                {
                    wfdList.Clear();

                    Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher.RunAsync
                        (Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        l.onDevicesDiscovered(wfdList);
                    });
                }
            });

            //m_workItem = asyncAction;

            asyncAction.Completed = new AsyncActionCompletedHandler(
                (IAsyncAction asyncInfo, AsyncStatus asyncStatus) =>
            {
                CoreWindow.GetForCurrentThread().Dispatcher.RunAsync
                    (CoreDispatcherPriority.Normal, () =>
                {
                    l.onDevicesDiscovered(wfdList);
                });

            });
        }

        private WFDDeviceConnectedListener connectedListener = null;
        public void connectAsync(WFDDevice device, WFDDeviceConnectedListener l)
        {
            IAsyncAction asyncAction = ThreadPool.RunAsync(async (workItem) =>
            {
                if (device.IsDevice)
                {
                    DeviceInformation devInfo = (DeviceInformation)device.WFDDeviceInfo;
                    WiFiDirectDevice wfdDevice = await WiFiDirectDevice.FromIdAsync(devInfo.Id);
                    
                    wfdDevice.ConnectionStatusChanged += new TypedEventHandler<WiFiDirectDevice, object>(onDisconnection);

                    var endpointPairCollection = wfdDevice.GetConnectionEndpointPairs();
                    EndpointPair endpointPair = endpointPairCollection[0];

                    StreamSocketListener socketListener = new StreamSocketListener();
                    socketListener.ConnectionReceived += onConnection;
                }
            });
        }
         
        private async void onConnection(StreamSocketListener sender,
            StreamSocketListenerConnectionReceivedEventArgs args)
        {
            StreamSocket s = args.Socket;
            Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher.RunAsync
                (Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                connectedListener.onDeviceConnected(s);
            });
        }

        private async void onDisconnection(object sender, object arg)
        {
            Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher.RunAsync
                (Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                connectedListener.onDeviceDisconnected();
            });
            connectedListener = null;
        }
    }
}
