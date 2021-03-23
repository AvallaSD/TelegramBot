using System;
using Telegram.Bot;

namespace TelegramBot
{
    class Program
    {
        static TelegramBotClient bot;
        static void Main(string[] args)
        {
            string token = "";

            bot = new TelegramBotClient(token);

            bot.OnMessage += MesssageListener;

            bot.StartReceiving();

            Console.ReadKey();
        }

        private static void MesssageListener(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;
            
        }
    }
}
