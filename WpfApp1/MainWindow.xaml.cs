using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace WpfApp1
{

    public partial class MainWindow : Window
    {
        Socket socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        string adres = "127.0.0.154";
        static int portServer = 888;
        bool IsConnect = false;
        public MainWindow()
        {
            IPAddress ip = IPAddress.Parse(adres);
            IPEndPoint ep = new IPEndPoint(ip, portServer);
            socketServer.Bind(ep);
            socketServer.Listen(10);
            InitializeComponent();
            this.MinHeight = 300;
            this.MinWidth = 500;

            Task.Run(ReadMessage);
        }


        private async void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                socketClient = await ConnectSocketAsync();
                await socketClient.SendAsync(Encoding.UTF8.GetBytes($"RemoveSocket***{LoginChat.Text}"));

                var data = new Byte[1024];
                StringBuilder awsner = new StringBuilder();
                int count;
                do
                {
                    count = await socketClient.ReceiveAsync(data);
                    awsner.Append(Encoding.UTF8.GetString(data, 0, count));
                } while (count > 0);
                Chat.Items.Add(awsner.ToString());
                IsConnect = false;
                socketClient.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex) { Chat.Items.Add(ex.Message); }
        }


        private async void SendAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (stringAnswer.Text == string.Empty && stringAnswer.Text == "Введите сообщение")
            {
                Chat.Items.Add("Введите сообщение!!!");
                return;
            }


            if (IsConnect)
            {
                socketClient = await ConnectSocketAsync();
                await socketClient.SendAsync(Encoding.UTF8.GetBytes($"Message***{LoginChat.Text}***{stringAnswer.Text}"));
                Chat.Items.Add($"Вы отправили в {DateTime.Now.ToString("HH:mm:ss")}: {stringAnswer.Text}");
                stringAnswer.Text = "Введите сообщение";
                socketClient.Shutdown(SocketShutdown.Both);
            }
            else
            {
                Chat.Items.Add("Нет соединения");
            }
        }

        private void stringAnswer_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (stringAnswer.Text == "Введите сообщение") stringAnswer.Text = string.Empty;
        }

        private void stringAnswer_LostFocus(object sender, RoutedEventArgs e)
        {
            if (stringAnswer.Text == string.Empty) stringAnswer.Text = "Введите сообщение";
        }

        private async void Connnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IPAddress pAddress = IPAddress.Parse("127.0.0.1");
                int port = 1024;
                IPEndPoint pPEndPoint = new IPEndPoint(pAddress, port);
                await socketClient.ConnectAsync(pPEndPoint);
                await socketClient.SendAsync(Encoding.UTF8.GetBytes($"ADDSocket***{LoginChat.Text}***{adres}***{portServer}"));

                byte[] data = new byte[1024];
                StringBuilder builder = new StringBuilder();
                int count;
                do
                {
                    count = await socketClient.ReceiveAsync(data);
                    builder.Append(Encoding.UTF8.GetString(data, 0, count));
                } while (count > 0);
                Dispatcher.Invoke(() => Chat.Items.Add(builder));
                IsConnect = true;
                port++;
                socketClient.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => Chat.Items.Add(ex.Message));
            }
        }

        private async void ReadMessage()
        {
            try
            {
                while (true)
                {
                    using Socket sreadchat = await socketServer.AcceptAsync();
                    if (sreadchat != null)
                    {
                        byte[] buffer = new byte[1024];
                        var result = await sreadchat.ReceiveAsync(buffer);
                        string texts = Encoding.UTF8.GetString(buffer, 0, result);
                        Dispatcher.Invoke(() => Chat.Items.Add(texts));
                    }
                }
            }
            catch (Exception ex) { Dispatcher.Invoke(() => Chat.Items.Add(ex.Message)); }
        }

        private async Task<Socket> ConnectSocketAsync()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress pAddress = IPAddress.Parse("127.0.0.1");
            int port = 1024;
            try
            {
                IPEndPoint pPEndPoint = new IPEndPoint(pAddress, port);
                await socket.ConnectAsync(pPEndPoint);
                return socket;
            }
            catch
            {
                return socket;
            }
        }
    }
}