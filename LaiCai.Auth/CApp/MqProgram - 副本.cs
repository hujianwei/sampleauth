using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;

namespace CApp
{
    class MqProgram
    {
        static void Main(string[] args)
        {
            for(int i=0;i<30;i++)
            {
                System.Threading.Thread thread = new System.Threading.Thread(new ParameterizedThreadStart(Send));
                thread.Start(i);
            }
        }

        static void Send(object obj)
        {
            while (true)
            {
                var factory = new ConnectionFactory();
                factory.HostName = "192.168.1.55";
                factory.UserName = "hujianwei";
                factory.Password = "hujianwei";

                    using (var connection = factory.CreateConnection())
                    {
                        using (var channel = connection.CreateModel())
                        {
                        
                            channel.QueueDeclare("hello", false, false, false, null);
                            string message = string.Format("线程ID:{1},Hello World,{0}", DateTime.Now, obj); ;
                            var body = Encoding.UTF8.GetBytes(message);
                            channel.BasicPublish("", "hello", null, body);
                            Console.WriteLine(" set {0}", message);
                        }
                    }
                
            }
        }

        static void Receive()
        {
            var factory = new ConnectionFactory();
            factory.HostName = "192.168.1.55";
            factory.UserName = "hujianwei";
            factory.Password = "hujianwei";

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare("hello", false, false, false, null);

                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume("hello", true, consumer);

                    Console.WriteLine(" waiting for message.");
                    while (true)
                    {
                        var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();

                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine("Received {0}", message);

                    }
                }
            }
            Console.ReadLine();
        }

    }
}
