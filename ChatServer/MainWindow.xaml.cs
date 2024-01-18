using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace ChatServer
{
    public partial class MainWindow : Window
    {
        Socket socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        List<ChatContact> contacts = new List<ChatContact>();
        public MainWindow()
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            IPEndPoint ep = new IPEndPoint(ip, 1024);
            socketServer.Bind(ep);
            socketServer.Listen(10);

            InitializeComponent();
            this.MinHeight = 300;
            this.MinWidth = 500;
            Task.Run(ReadAcceptAsync);
        }

        private void SendAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (stringAnswer.Text == string.Empty && stringAnswer.Text == "Введите сообщение")
            {
                Chat.Items.Add("Введите сообщение!!!");
                return;
            }

            if (contacts.Count > 0)
            {
                SendMessage(new ChatContact("Server", new IPEndPoint(IPAddress.Any, 0)), $"Server отправил сообщение в {DateTime.Now.ToString("HH:mm:ss")}: {stringAnswer.Text}");
                Chat.Items.Add($"Server отправили в {DateTime.Now.ToString("HH:mm:ss")}: {stringAnswer.Text}");
                stringAnswer.Text = "Введите сообщение";
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
        private async Task ReadAcceptAsync()
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
                        string[] texts = Encoding.UTF8.GetString(buffer, 0, result).Split("***");


                        if (texts[0] == "ADDSocket")
                        {
                            ChatContact chatContact = new ChatContact(texts[1], new IPEndPoint(IPAddress.Parse(texts[2]), Int32.Parse(texts[3])));
                            contacts.Add(chatContact);
                            socketClient = ConnectSocketAsync(chatContact.IPEnd);
                            Dispatcher.Invoke(() => Chat.Items.Add($"{chatContact.name} присоединился к чату!!"));
                            await socketClient.SendAsync(Encoding.UTF8.GetBytes("Успешно присоединились к чату!"));
                            SendMessage(chatContact, $"{chatContact.name} присоединился к чату!!");
                        }

                        if (texts[0] == "Message")
                        {
                            ChatContact chatContact = contacts.FirstOrDefault(x => x.name == texts[1])!;
                            SendMessage(chatContact, texts[2]);
                            Dispatcher.Invoke(() => Chat.Items.Add($"{chatContact.name} отправил сообщение в {DateTime.Now.ToString("HH:mm:ss")}: {texts[2]}"));
                        }
                        if (texts[0] == "RemoveSocket")
                        {
                            ChatContact chatContact = contacts.FirstOrDefault(x => x.name == texts[1])!;
                            socketClient = ConnectSocketAsync(chatContact.IPEnd);                           
                            await socketClient.SendAsync(Encoding.UTF8.GetBytes("Успешно покинул чат!"));

                            contacts.Remove(chatContact);
                            SendMessage(chatContact, $"{chatContact.name} покинул наш чат!!");
                            Dispatcher.Invoke(() => Chat.Items.Add($"{chatContact.name} покинул чат в {DateTime.Now.ToString("HH:mm:ss")}"));
                        }
                    }
                    sreadchat.Shutdown(SocketShutdown.Both);
                }
            }
            catch (Exception ex) { Dispatcher.Invoke(() => Chat.Items.Add(ex.Message)); }
        }
        private Socket ConnectSocketAsync(IPEndPoint iPEnd)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.ConnectAsync(iPEnd);
                return socket;
            }
            catch
            {
                return socket;
            }
        }
        private async void SendMessage(ChatContact chatC, string message)
        {
            foreach (var contact in contacts)
            {
                if (chatC.name != contact.name)
                {
                    socketClient = ConnectSocketAsync(contact.IPEnd);
                    await socketClient.SendAsync(Encoding.UTF8.GetBytes(message));
                }
            }
            socketClient.Shutdown(SocketShutdown.Both);
        }
        record ChatContact(string name, IPEndPoint IPEnd);
    }
}