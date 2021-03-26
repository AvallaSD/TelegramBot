using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Telegram.Bot;
using System.Linq;

namespace TelegramBot
{
    class BotClient
    {
        List<long> activeChats;
        TelegramBotClient bot;

        public BotClient(string token)
        {
            bot = new TelegramBotClient(token);
            bot.OnMessage += MessageListener;
            activeChats = new List<long>();
            bot.StartReceiving();
        }

        private void MessageListener(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;
            Console.WriteLine(message.Text);
            if (message.Text == "/start")
            {
                activeChats.Add(message.Chat.Id);
            }
            if (!activeChats.Contains(message.Chat.Id))
            {
                bot.SendTextMessageAsync(message.Chat.Id, "Добро пожаловать. Для начала введите команду /start");
                return;
            }

            if (message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
            {
                if (message.Text == "/list")
                {
                    var filesNames = Directory.GetFiles(Directory.GetCurrentDirectory() + @"/files").ToList();
                    string msg = "";
                    filesNames.ForEach(x => msg += $"{filesNames.IndexOf(x)}) {x.Substring(x.LastIndexOf(@"\") + 1)}\n");
                    bot.SendTextMessageAsync(message.Chat.Id, msg);
                }
                else
                {

                    bot.SendTextMessageAsync(message.Chat.Id,
                      "/list - посмотреть все загруженные файлы\n" +
                      "/delite - удалить файлы\n" +
                      "/download - скачать файл\n" +
                      "Для загрузки файла - прикрепите его к сообщению\n" +
                      "Аудиосообщения отправленные боту также сохраняются");

                }
            }
            
            



        }
    }
}
