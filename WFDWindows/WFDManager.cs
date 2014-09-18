using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.System.Threading;
using Windows.Devices.WiFiDirect;
using Windows.Devices.Enumeration;
using System.Diagnostics;

using Windows.Networking.Proximity;

namespace Buffalo.WiFiDirect
{
    public class WFDManager
    {
        private readonly DependencyObject parent;
        public WFDManager(DependencyObject parent)
        {
            this.parent = parent;
            PeerFinder.Start();

            PeerFinder.ConnectionRequested += async (object sender, ConnectionRequestedEventArgs args) =>
            {
                Debug.WriteLine("ConnectionReceived");

                StreamSocket s = await PeerFinder.ConnectAsync(args.PeerInformation);
                
                /*await parent.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Debug.WriteLine("Call onSocketConnected");
                });*/
            };
        }

        //private delegate void WorkItemHandler(IAsyncAction operation);

        public void getDevicesAsync(WFDDeviceDiscoveredListener l)
        {
            List<WFDDevice> wfdList = new List<WFDDevice>();
            IEnumerable<PeerInformation> pList = null;

            bool checkPeerFinder = (PeerFinder.SupportedDiscoveryTypes & PeerDiscoveryTypes.Browse) == PeerDiscoveryTypes.Browse;
            bool allowWifiDirect = PeerFinder.AllowWiFiDirect;

            IAsyncAction asyncAction = ThreadPool.RunAsync( async (workItem) =>
            {
                /*to Android*/
                string wfdSelector = WiFiDirectDevice.GetDeviceSelector();
                DeviceInformationCollection devInfoCollection = await DeviceInformation.FindAllAsync(wfdSelector);

                /*to Windows*/
                if (checkPeerFinder && allowWifiDirect)
                {
                    pList = await PeerFinder.FindAllPeersAsync();
                    if (pList != null)
                    {
                        foreach (PeerInformation peerInfo in pList)
                        { /* to Windows */
                            wfdList.Add(new WFDDevice(peerInfo));
                        }
                    }
                }

                //if (pList == null) throw new Exception("No peer");


                foreach (DeviceInformation devInfo in devInfoCollection)
                { /* to Android */
                    wfdList.Add(new WFDDevice(devInfo));
                }

                

                if (workItem.Status == AsyncStatus.Canceled)
                {
                    wfdList.Clear();
                }

                //add peerfinder

                await parent.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        //call callback
                        l.onDevicesDiscovered(wfdList);
                    });

                
                /*CoreWindow.GetForCurrentThread().Dispatcher.RunAsync
                    (CoreDispatcherPriority.Normal, () =>
                    {
                        l.onDevicesDiscovered(wfdList);
                    });*/
            });

            //onDevicesDiscoverFailed() 추가해야함
        }

        //private WFDDeviceConnectedListener connectedListener = null;
        public void pairAsync(WFDDevice device, WFDDeviceConnectedListener l)
        {
            parent.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                if (device.IsDevice)
                {
                    DeviceInformation devInfo = (DeviceInformation)device.WFDDeviceInfo;
                    WiFiDirectDevice wfdDevice = await WiFiDirectDevice.FromIdAsync(devInfo.Id);

                    wfdDevice.ConnectionStatusChanged += new TypedEventHandler<WiFiDirectDevice, object>(async (WiFiDirectDevice sender, object arg)
                        => {
                            await parent.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                l.onDeviceDisconnected();
                            });
                        });

                    var endpointPairCollection = wfdDevice.GetConnectionEndpointPairs();
                    EndpointPair endpointPair = endpointPairCollection[0];


                    l.onDeviceConnected(new WFDPairInfo(device, endpointPair, parent));
                    //onDeviceConnectFailed(int reasonCode)추가해야함
                }
                else
                {
                    //PeerInformation peerInfo = (PeerInformation)device.WFDDeviceInfo;
                    //StreamSocket socket = await PeerFinder.ConnectAsync(peerInfo);

                    l.onDeviceConnected(new WFDPairInfo(device, parent));
                }
            });
        }

        public void unpair(WFDPairInfo pair)
        {
            if (pair.getWFDDevice().IsDevice)
            {
                (pair.getWFDDevice().WFDDeviceInfo as WiFiDirectDevice).Dispose();
            }
            else
            {
                //PeerFinder.Stop();
                //add peerfinder
            }
        }
    }
}
