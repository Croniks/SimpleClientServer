using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Reflection;


namespace Client
{
    enum MenuOptionKey { OpeningMainMenu = 1, ReturnToMainMenu = 2 }
    enum Command { SendMessage = ConsoleKey.D1, GetRandomError = ConsoleKey.D2, ExitProgramm = ConsoleKey.D3 }
    
    class Client
    {
        private IPEndPoint _ipPoint;
        private Socket _socket;

        private int _port;
        private string _adress;
        
        private Dictionary<string, string> menuOptions => menuOptions ?? new Dictionary<string, string>();
        private Dictionary<ConsoleKey, Action> _commands;
        
        
        public Client(int port, string adress)
        {
            CreateCommandsDictionary();
            GetUserCommand(MenuOptionKey.OpeningMainMenu);
        }

        private void CreateCommandsDictionary()
        {
            _commands = new Dictionary<ConsoleKey, Action>();
            Type currentClassType = GetType();

            foreach (MethodInfo methodInfo in currentClassType.GetMethods())
            {
                foreach (string enumName in Enum.GetNames(typeof(Command)))
                {
                    if (methodInfo.Name == enumName)
                    {
                        Command command;
                        if (Enum.TryParse(enumName, true, out command))
                        {
                            _commands.Add((ConsoleKey)command, (Action)methodInfo.CreateDelegate(typeof(Action), this));
                        }
                    }
                }
            }
        }

        public void SendMessage()
        {
            Console.WriteLine("This is \"SendMessage\" method worked");
            GetUserCommand(MenuOptionKey.OpeningMainMenu);
        }
        
        public void GetRandomError()
        {
            Console.WriteLine("This is \"GetRandomError\" method worked");
            GetUserCommand(MenuOptionKey.OpeningMainMenu);
        }

        public void ExitProgramm()
        {
            Environment.Exit(0);
        }
        
        private void PrintMenu(MenuOptionKey key)
        {
            switch (key)
            {
                case MenuOptionKey.OpeningMainMenu:
                    Console.WriteLine("Добро пожаловать в главное меню: ");
                    Console.WriteLine($" - для отправки сообщения на сервер нажмите - \"{(char)(ConsoleKey)Command.SendMessage}\"");
                    Console.WriteLine($" - для получения случайной ошибки с сервера нажмите - \"{(char)(ConsoleKey)Command.GetRandomError}\"");
                    Console.WriteLine($" - для выхода из приложения нажмите - \"{(char)(ConsoleKey)Command.ExitProgramm}\"");
                    break;
                case MenuOptionKey.ReturnToMainMenu:
                    Console.WriteLine("/t- для возврата в главное меню нажмите - \"E\"");
                    Console.WriteLine("/t- для выхода из приложения нажмите - \"Q\"");
                    break;
            }
        }

        private void GetUserCommand(MenuOptionKey menuOption)
        {
            PrintMenu(menuOption);
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            if (_commands.ContainsKey(keyInfo.Key))
            {
                _commands[keyInfo.Key].Invoke();
            }
            else
            {
                GetUserCommand(0);
            }
        }

        private bool CreateSocket(int port, string adress)
        {
            if(port != default && !string.IsNullOrEmpty(adress))
            {
                _ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                return true;
            }
            
            return false;
        }

        private void RequestServer()
        {
            if(_socket == null)
            {
                CreateSocket(_port, _adress);
            }
            
            _socket.Connect(_ipPoint);
            string message = Console.ReadLine();

            //if (message.Trim()[0])
                // Запрос к словарю
            
            byte[] data = Encoding.Unicode.GetBytes(message);
            _socket.Send(data);
        }

       
       
        
        // адрес и порт сервера, к которому будем подключаться
        static int port = 8005; // порт сервера
        static string address = "127.0.0.1"; // адрес сервера

        static void Main(string[] args)
        {
            Client client = new Client(8005, "127.0.0.1");



            //try
            //{
            //    IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);

            //    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //    // подключаемся к удаленному хосту
            //    socket.Connect(ipPoint);
            //    Console.Write("Введите сообщение:");
            //    string message = Console.ReadLine();
            //    byte[] data = Encoding.Unicode.GetBytes(message);
            //    socket.Send(data);

            //    // получаем ответ
            //    data = new byte[256]; // буфер для ответа
            //    StringBuilder builder = new StringBuilder();
            //    int bytes = 0; // количество полученных байт

            //    do
            //    {
            //        bytes = socket.Receive(data, data.Length, 0);
            //        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            //    }
            //    while (socket.Available > 0);
            //    Console.WriteLine("ответ сервера: " + builder.ToString());

            //    // закрываем сокет
            //    socket.Shutdown(SocketShutdown.Both);
            //    socket.Close();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
        }
    }
}