using System;
using System.Collections.Generic;
using Telegram.Bot;

namespace TelegramBot
{
    class Program
    {
        static void Main()
        {
            BotClient client = new BotClient("1459208947:AAF4btGXyWkEQIW21eMMs0t38fWtnY6fGyQ");
            while (true)
            {
                Console.WriteLine("Бот работает. Для прекращения введите exit:");
                if (Console.ReadLine() == "exit")
                {
                    break;
                }
            }
        }
        
        
        
    }
}
