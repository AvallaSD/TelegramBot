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
        List<string> filesNames;

        public BotClient(string token)
        {
            bot = new TelegramBotClient(token);
            bot.OnMessage += MessageListener;
            activeChats = new List<long>();
            filesNames = new List<string>();
            RefreshFilesInfo();
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
                if (message.Text.StartsWith("/delete") && message.Text.Length > 7)
                {
                    List<int> indexes = new List<int>();

                    try
                    {
                        message.Text.Substring(8).Split(' ').ToList().ForEach(x => indexes.Add(int.Parse(x)));
                        indexes.ForEach(x => File.Delete(filesNames[x]));
                    }
                    catch (FormatException)
                    {
                        bot.SendTextMessageAsync(message.Chat.Id, "Неверное использование команды /delete!");
                    }
                    catch (Exception ex)
                    {
                        bot.SendTextMessageAsync(message.Chat.Id, $"Ошибка: {ex.Message}");
                    }

                    
                }
                
                if (message.Text == "/list")
                {
                    string msg = "";
                    filesNames.ForEach(x => msg += $"{filesNames.IndexOf(x)}) {x.Substring(x.LastIndexOf(@"\") + 1)}\n");
                    bot.SendTextMessageAsync(message.Chat.Id, msg);
                }
                else
                {

                    bot.SendTextMessageAsync(message.Chat.Id,
                      "/list - посмотреть все загруженные файлы\n" +
                      "/delete 0 1 2... - удалить файлы c соответствующими индексами\n" +
                      "/download 0 - скачать файл с соответствующим индексом\n" +
                      "Для загрузки файла - прикрепите его к сообщению\n" +
                      "Аудиосообщения отправленные боту также сохраняются");

                }
            }




            RefreshFilesInfo();
        }

        private void RefreshFilesInfo()
        {
            Directory.GetFiles(Directory.GetCurrentDirectory() + @"/files").ToList().ForEach(x => filesNames.Add(x));
        }
    }
}
