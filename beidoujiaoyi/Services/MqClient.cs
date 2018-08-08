using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace beidoujiaoyi.Services
{
    public class MqComsumer
    {
        string conName = "fanoutTest";
            string conPwd = "123";
            int conPort = 5672;
            string exchangeName = "amq.fanout";
            string type = "fanout";
            string queueName = "queue1";

        public void RecieveMessage()
        {
            
            var factory = new ConnectionFactory();
            factory.HostName = "localhost";
            factory.Port = conPort;
            factory.UserName = conName;
            factory.Password = conPwd;

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout, true, false);
                    channel.QueueDeclare(queueName, true, false, false);//创建一个消息队列
                    channel.BasicQos(0, 1, false); //告诉broker同一时间只处理一个消息
                    channel.QueueBind(queueName, exchangeName, "");  //队列绑定到指定的exchange上面
#if false
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (sender, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine("已接收： {0}", message);
                        //处理完成，告诉Broker可以服务端可以删除消息，分配新的消息过来
                        channel.BasicAck(ea.DeliveryTag, false);
                    };
                    channel.BasicConsume(queueName, false, consumer);

#else
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(queueName, true, consumer);
                    while (true)
                    {
                        //阻塞函数，获取队列中的消息
                        BasicDeliverEventArgs ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                        string message = Encoding.UTF8.GetString(ea.Body);
                        Console.WriteLine("已接收： {0}", message);
                    }
#endif
                }
            }
        }
        
    }

    public class MqProducer
    {
        string conName = "fanoutTest";
        string conPwd = "123";
        int conPort = 5672;
        string exchangeName = "exchange1";
        string type = "fanout";
        string queueName = "queue1";

        public void SendMessage()
        {
            var factory = new ConnectionFactory();
            factory.HostName = "localhost";//RabbitMQ服务在本地运行
            factory.Port = conPort;
            factory.UserName = conName;//用户名
            factory.Password = conPwd;//密码

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout, true, false, null);
                    //channel.QueueDeclare(queueName, true, false, false, null);//创建一个消息队列
                    //channel.QueueBind(queueName, exchangeName, "", null);

                    IBasicProperties props = channel.CreateBasicProperties();

                    //props.ContentType = "text/plain";

                    //props.DeliveryMode = 2;

                    //props.Headers = new Dictionary<string, object>();

                    //props.Headers.Add("latitude", 51.5252949);

                    //props.Headers.Add("longitude", -0.0905493);

                    string message = "Hello World"; //传递的消息内容
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchangeName, "", props, body); //开始传递
                    Console.WriteLine("已发送： {0}", message);
                }
            }
        }
    }
}
