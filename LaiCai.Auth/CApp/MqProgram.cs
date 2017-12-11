using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Util;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;

namespace CApp
{
    class MqProgram
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < 100; i++)
            {
                System.Threading.Thread thread = new System.Threading.Thread(new ParameterizedThreadStart(Send));
                thread.Start(i);

            }

            //Receive();
        }

        static void Send(object obj)
        {
            while (true)
            {
                System.Threading.Thread.Sleep(5);
                var factory = new ConnectionFactory();
                factory.HostName = "192.168.1.55";
                factory.Port = 4671;
                //factory.HostName = "192.168.56.30"; //虚拟机
                //factory.Port = 4672;    //虚拟机端口
                factory.UserName = "hujianwei";
                factory.Password = "hujianwei";
                

                try
                {
                    using (var connection = factory.CreateConnection())
                    {
                        using (var channel = connection.CreateModel())
                        {

                            channel.ExchangeDeclare("logs", "fanout");

                            string queueName = channel.QueueDeclare().QueueName;
                            channel.QueueDeclare("tt", true, false, false, null);

                            channel.QueueBind(queueName, "logs", "");
                            string message = string.Format("线程ID:{1},Hello World,{0}", DateTime.Now, obj); ;
                            var body = Encoding.UTF8.GetBytes(message);
                            channel.BasicPublish("logs", "", null, body);
                            Console.WriteLine(" set {0}", message);
                        }
                    }
                }
                catch(Exception e)
                {
                    Thread.Sleep(100);
                }

            }
        }

        static void Receive()
        {
            var factory = new ConnectionFactory();
            factory.HostName = "192.168.56.30";
            factory.UserName = "hujianwei";
            factory.Password = "hujianwei";
            factory.Port = 4672;
            while (true)
            {
                try
                {
                    using (var connection = factory.CreateConnection())
                    {
                        using (var channel = connection.CreateModel())
                        {
                            channel.ExchangeDeclare("logs", "fanout");
                            string queueName = channel.QueueDeclare().QueueName;
                            channel.QueueBind(queueName, "logs", "");
                            var consumer = new QueueingBasicConsumer(channel);
                            //var consumer = new DefaultBasicConsumer(channel);

                            channel.BasicConsume(queueName, false, consumer);
                            Console.WriteLine(" waiting for message.");
                            while (true)
                            {

                                var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();

                                var body = ea.Body;
                                var message = Encoding.UTF8.GetString(body);
                                Console.WriteLine("Received {0}", message);
                                channel.BasicAck(ea.DeliveryTag, false);
                            }
                        }
                    }
                }
                catch
                {
                    Thread.Sleep(100);
                }
            }
            Console.ReadLine();
        }

    }
}
