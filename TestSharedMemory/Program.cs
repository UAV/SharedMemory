using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BoTechnologies.IPC;
namespace TestSharedMemory
{
    class Program
    {
        static SharedMemory consumer;
        static SharedMemory producer;
        /// <summary>
        /// Handles the consumer receive event.
        /// </summary>
        /// <param name="item">The item.</param>
        public static void HandleConsumerReceiveEvent(byte[] item)
        {
            System.Threading.Thread.Sleep(100);

            string newMessage = ASCIIEncoding.ASCII.GetString(item).Split(':')[0];
            string print = ASCIIEncoding.ASCII.GetString(item).Split('\0')[0];
            Console.WriteLine("Recevie " + print );
            if (newMessage == "PING")
            {
                Console.WriteLine("Sending PONG");

                producer.Write(ASCIIEncoding.ASCII.GetBytes("PONG: " + DateTime.Now.ToString()));


            }
            else
            {
                Console.WriteLine("Sending PING");

                producer.Write(ASCIIEncoding.ASCII.GetBytes("PING: " + DateTime.Now.ToString()));

            }
        }

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                producer = new SharedMemory("noname1", 255);
                consumer = new SharedMemory("noname2", 255);
            }
            else
            {
                producer = new SharedMemory("noname2", 255);
                consumer = new SharedMemory("noname1", 255);
            }
       
            consumer.ReceiveHandler += new ReceivedEventHandler<byte[]>(Program.HandleConsumerReceiveEvent);

            //Console.WriteLine("Press return to start ");
            //Console.ReadLine();
            if (args.Length > 0)
            {
                Console.WriteLine("Sending PING");
                producer.Write(ASCIIEncoding.ASCII.GetBytes("PING: " + DateTime.Now.ToString()));
            }

            Console.WriteLine("Press return to exit ");
            Console.ReadLine(); 
            consumer.Close();
            producer.Close();

        }
    }
}
