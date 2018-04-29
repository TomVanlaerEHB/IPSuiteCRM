using System;
using RabbitMQ.Client;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

[XmlRootAttribute("Invoice")]
public class Invoicing
{
    public string FactuurUUID;
    public string AccountUUID;
    public string PdfLink;
}

class NewTask
{
    public static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "task_queue",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var message = GetMessage();
            var body = Encoding.UTF8.GetBytes(message);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: "",
                                 routingKey: "task_queue",
                                 basicProperties: properties,
                                 body: body);
            Console.WriteLine(" [x] Sent {0}", message);
        }

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }

    public static string GetMessage()
    {
        //XmlSerializer xml = new XmlSerializer();
        //FactuurUUID, AccountUUID, PDFLink IN XML!
        XmlSerializer docSer = new XmlSerializer(typeof(Invoicing));
        StringWriter writer = new StringWriter();
        Invoicing invoice = new Invoicing();
        invoice.AccountUUID = "123456789";
        invoice.FactuurUUID = "963258741";
        invoice.PdfLink = "./Factuur-0006.pdf";

        docSer.Serialize(writer, invoice);
        return writer.ToString();
    }
}