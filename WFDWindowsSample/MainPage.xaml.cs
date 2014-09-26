using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Storage.Streams;
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

using Windows.Storage;

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
            manager = new WFDManager(this, discoveredListener, discoveredListener);
            discoveredListener = new DiscoveredListener(this);
        }

        private void btnFindDevices_Click(object sender, RoutedEventArgs e)
        {
            manager.getDevicesAsync();
            tbMessage.Text = "Finding Devices...";
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            device = comboDeviceList.SelectedItem as WFDDevice;
            tbMessage.Text = "Connect to " + device.Name;
            manager.pairAsync(device);
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
                parent.tbMessage.Text = "Device's IP Address : " + pair.getRemoteAddress(); 
              
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
                parent.tbMessage.Text = "Socket Connected.";

                DataWriter writer = new DataWriter(s.OutputStream);
                writer.WriteString("ping~ping~\n");

                writer.StoreAsync();
                writer.FlushAsync();

                parent.tbMessage.Text = "Send Message.";
             //   writer.Dispose();
            //    s.Dispose();

               // parent.manager.unpair(parent.pairInfo);
            }
        }

        int BLOCK_SIZE = 1024;
        private async Task SendFileToPeerAsync(StreamSocket socket, StorageFile selectedFile)
        {
            byte[] buff = new byte[BLOCK_SIZE];
            var prop = await selectedFile.GetBasicPropertiesAsync();
            using (var dw = new DataWriter(socket.OutputStream))
            {

                // 1. Send the filename length
                dw.WriteInt32(selectedFile.Name.Length); // filename length is fixed
                // 2. Send the filename
                dw.WriteString(selectedFile.Name);
                // 3. Send the file length
                dw.WriteUInt64(prop.Size);
                // 4. Send the file
                var fileStream = await selectedFile.OpenStreamForReadAsync();
                while (fileStream.Position < (long)prop.Size)
                {
                    var rlen = await fileStream.ReadAsync(buff, 0, buff.Length);
                    dw.WriteBytes(buff);
                }

                await dw.FlushAsync();
                await dw.StoreAsync();

                await socket.OutputStream.FlushAsync();
            }
        }



        private async Task<string> ReceiveFileFomPeer(StreamSocket socket, StorageFolder folder, string outputFilename = null)
        {
            StorageFile file;
            using (var rw = new DataReader(socket.InputStream))
            {
                // 1. Read the filename length
                await rw.LoadAsync(sizeof(Int32));
                var filenameLength = (uint)rw.ReadInt32();
                // 2. Read the filename
                await rw.LoadAsync(filenameLength);
                var originalFilename = rw.ReadString(filenameLength);
                if (outputFilename == null)
                {
                    outputFilename = originalFilename;
                }
                //3. Read the file length
                await rw.LoadAsync(sizeof(UInt64));
                var fileLength = rw.ReadUInt64();

                // 4. Reading file
                using (var memStream = await DownloadFile(rw, fileLength))
                {

                    file = await folder.CreateFileAsync(outputFilename, CreationCollisionOption.ReplaceExisting);
                    using (var fileStream1 = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await RandomAccessStream.CopyAndCloseAsync(memStream.GetInputStreamAt(0), fileStream1.GetOutputStreamAt(0));
                    }

                    rw.DetachStream();
                }
            }

            return file.Path;
        }

        private async Task<InMemoryRandomAccessStream> DownloadFile(DataReader rw, ulong fileLength)
        {
            var memStream = new InMemoryRandomAccessStream();

            // Download the file
            while (memStream.Position < fileLength)
            {
                var lenToRead = Math.Min(BLOCK_SIZE, (float)(fileLength - memStream.Position));
                await rw.LoadAsync((uint)lenToRead);
                var tempBuff = rw.ReadBuffer((uint)lenToRead);
                await memStream.WriteAsync(tempBuff);
            }

            return memStream;
        }

    }
}

