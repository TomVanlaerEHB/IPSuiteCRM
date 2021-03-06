﻿using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;
using System.Xml;

namespace temp
{
    class Worker
    {
        public static void Main()
        {
            XmlDocument doc = new XmlDocument();
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "task_queue",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                Console.WriteLine(" [*] Waiting for messages.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    //Console.WriteLine(" [x] Received {0}", message);
                    doc.LoadXml(message);

                    Console.WriteLine(doc.SelectSingleNode("./ Invoice / PdfLink").InnerXml);

                    //Console.WriteLine(doc.GetElementById("PdfLink"));
                    Invoice.ImportPDF(doc.SelectSingleNode("./ Invoice / PdfLink").InnerXml);

                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };
                channel.BasicConsume(queue: "task_queue",
                                     autoAck: false,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}