using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text.Json;


namespace Client
{
    enum ErrorType { RandomServerError = 1}
    enum RequestType { Message, WaitingError }
    enum MenuOptionKey { OpeningMainMenu = 1, ReturnToMainMenu = 2 }
    enum Command { SendMessage = ConsoleKey.A, GetRandomError = ConsoleKey.D2, ExitProgramm = ConsoleKey.Q }
    
    class ErrorInfo
    {
        public int ErrorType { get; set; }
        public string ErrorMessage { get; set; }
    }

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
            _port = port;
            _adress = adress;

            CreateCommandsDictionary();
            GetUserCommand();
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
            Console.Write("Введите сообщение, которое вы хотите послать на сервер: ");
            string message = Console.ReadLine();

            RequestServer(message, RequestType.Message);
        }
        
        public void GetRandomError()
        {
            RequestServer("error", RequestType.WaitingError);
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
                    Console.WriteLine($" - для отправки сообщения на сервер нажмите - \"{(char)Command.SendMessage}\"");
                    Console.WriteLine($" - для получения случайной ошибки с сервера нажмите - \"{(char)Command.GetRandomError}\"");
                    Console.WriteLine($" - для выхода из приложения нажмите - \"{(char)Command.ExitProgramm}\"");
                    break;
                case MenuOptionKey.ReturnToMainMenu:
                    Console.WriteLine("/t- для возврата в главное меню нажмите - \"E\"");
                    Console.WriteLine("/t- для выхода из приложения нажмите - \"Q\"");
                    break;
            }
        }
        
        private void GetUserCommand()
        {
            PrintMenu(MenuOptionKey.OpeningMainMenu);

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                if (_commands.ContainsKey(keyInfo.Key))
                {
                    _commands[keyInfo.Key].Invoke();
                }

                PrintMenu(MenuOptionKey.OpeningMainMenu);
            }
        }

        private bool CreateSocket(int port, string adress)
        {
            if(port != default && !string.IsNullOrEmpty(adress))
            {
                _ipPoint = new IPEndPoint(IPAddress.Parse(adress), port);
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                return true;
            }
            else
            {
                Console.WriteLine("Неверное имя порта или адрес!");
            }   

            return false;
        }

        private void RequestServer(string message, RequestType requestType)
        {
            if(!CreateSocket(_port, _adress))
            {
                return;
            }
            
            _socket.Connect(_ipPoint);

            byte[] data = Encoding.Unicode.GetBytes(message);
            _socket.Send(data);

            if (requestType == RequestType.WaitingError)
            {

                data = new byte[256]; 
                StringBuilder builder = new StringBuilder();
                int bytes = 0; // количество полученных байт

                do
                {
                    bytes = _socket.Receive(data, data.Length, 0);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                }
                while (_socket.Available > 0);
            
            
                ErrorInfo errorInfo = JsonSerializer.Deserialize<ErrorInfo>(builder.ToString());

                if ((ErrorType)errorInfo.ErrorType == ErrorType.RandomServerError)
                {
                    Console.WriteLine(errorInfo.ErrorMessage);
                }
            }
            
            // закрываем сокет
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }
        
        // адрес и порт сервера, к которому будем подключаться
        static int port = 8005; // порт сервера
        static string address = "127.0.0.1"; // адрес сервера

        static void Main(string[] args)
        {
            Client client = new Client(8005, "127.0.0.1");
        }
    }
}