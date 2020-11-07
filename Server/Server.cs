using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;


namespace Server
{
    enum ErrorType { RandomServerError = 1 }
    enum RequestType { Message, WaitingError }

    class ErrorInfo
    {
        public int ErrorType { get; set; }
        public string ErrorMessage { get; set; }
    }

    class Server
    {
        // порт для приема входящих запросов
        static int port = 8005; 
        
        static void Main(string[] args)
        {
            // получаем адреса для запуска сокета
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

            // создаем сокет
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // связываем сокет с локальной точкой, по которой будем принимать данные
                listenSocket.Bind(ipPoint);

                // начинаем прослушивание
                listenSocket.Listen(10);

                Console.WriteLine("Сервер запущен. Ожидание подключений...");
                
                while (true)
                {
                    Socket handler = listenSocket.Accept();
                    
                    // получаем сообщение
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0; // количество полученных байтов
                    byte[] data = new byte[256]; // буфер для получаемых данных

                    do
                    {
                        bytes = handler.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (handler.Available > 0);
                    
                    if(!builder.ToString().Equals("error"))
                    {
                        Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + builder.ToString());
                    }
                    else
                    { 
                        // отправляем ответ
                        string message = JsonSerializer.Serialize(new ErrorInfo(){
                            ErrorMessage = "Случайная ошибка от сервера!",
                            ErrorType = (int)ErrorType.RandomServerError
                        });
                        
                        data = Encoding.Unicode.GetBytes(message);
                        handler.Send(data);
                    }
                    
                    // закрываем сокет
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
    }
}