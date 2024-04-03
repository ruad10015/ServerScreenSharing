using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Server
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            StartListening();
        }

        private void StartListening()
        {
            var ipAddress = IPAddress.Parse("192.168.1.69");
            var port = 27001;

            Task.Run(() =>
            {
                try
                {
                    using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        var ep = new IPEndPoint(ipAddress, port);
                        socket.Bind(ep);
                        socket.Listen(10);

                        MessageBox.Show($"Listening on {socket.LocalEndPoint}", "Listen", MessageBoxButton.OK);

                        while (true)
                        {
                            var client = socket.Accept();
                            Task.Run(() =>
                            {
                                var clientEndPoint = (IPEndPoint)client.RemoteEndPoint;
                                MessageBox.Show($"CLIENT : {clientEndPoint.Address} connected", "Connected", MessageBoxButton.OK);

                                try
                                {
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        int bytesRead;
                                        byte[] buffer = new byte[1024];

                                        
                                        while ((bytesRead = client.Receive(buffer)) > 0)
                                        {
                                            ms.Write(buffer, 0, bytesRead);
                                        }

                                       
                                        Dispatcher.Invoke(() =>
                                        {
                                            AddImageToControl(ms.ToArray());
                                            sendInfo.Content = $"Last image sent from: {clientEndPoint.Address}";
                                        });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error: {ex.Message}");
                                }
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            });
        }

        private void AddImageToControl(byte[] imageData)
        {
            try
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.CreateOptions = BitmapCreateOptions.None;
                bitmapImage.StreamSource = new MemoryStream(imageData);
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                photoCavod.Source = bitmapImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
