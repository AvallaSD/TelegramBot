using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.InputFiles;

namespace TelegramBot
{
    /// <summary>
    /// Класс бота-клиета. Содержит методы работы с ботом в рамках поставленной задачи
    /// </summary>
    public class BotClient
    {
        /// <summary>
        /// Активные чаты с ботом
        /// </summary>
        List<long> activeChats;

        /// <summary>
        /// Клиент, использующий Telegram Bot API
        /// </summary>
        TelegramBotClient bot;

        /// <summary>
        /// Список загруженных файлов
        /// </summary>
        List<string> filesNames;

        /// <summary>
        /// Конструктор. Инициализирует основные элементы
        /// </summary>
        /// <param name="token">Токен бота</param>
        public BotClient(string token)
        {
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\\files\\");
            bot = new TelegramBotClient(token);
            bot.OnMessage += MessageListener;
            activeChats = new List<long>();
            filesNames = new List<string>();
            RefreshFilesInfo();
            bot.StartReceiving();
           
        }

        /// <summary>
        /// Получает сообщения и запускает соответствующие методы обработки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MessageListener(object sender, MessageEventArgs e)
        {
            var message = e.Message;

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
                ProcessTextMessage(message);
            }
            else if (message.Type == Telegram.Bot.Types.Enums.MessageType.Photo)
            {
                ProcessPhoto(message);
            }
            else
            {
                ProcessOtherFile(message);
            }
            RefreshFilesInfo();
        }

        /// <summary>
        /// Обновляет список файлов
        /// </summary>
        private void RefreshFilesInfo()
        {
            filesNames.Clear();
            Directory.GetFiles(Directory.GetCurrentDirectory() + @"/files").ToList().ForEach(x => filesNames.Add(x));
        }

        /// <summary>
        /// Обрабатывает текстовоке сообщение
        /// </summary>
        /// <param name="message">Сообщение</param>
        private void ProcessTextMessage(Telegram.Bot.Types.Message message)
        {
            if (message.Text.StartsWith("/delete") && message.Text.Length > 7)
            {
                List<int> indexes = new List<int>();

                indexes = DecomposeIndexes(message);

                try
                {
                    indexes.ForEach(x => File.Delete(filesNames[x]));
                }
                catch (Exception)
                {
                    bot.SendTextMessageAsync(message.Chat.Id, "Удаление не удалось");
                }
            }

            else if (message.Text == "/list")
            {
                string msg = "";
                if (filesNames.Count == 0)
                {
                    msg = "Файлообменник пуст";
                }
                else
                {
                    filesNames.ForEach(x => msg += $"{filesNames.IndexOf(x)}) {x.Substring(x.LastIndexOf(@"\") + 1)}\n");
                }
                bot.SendTextMessageAsync(message.Chat.Id, msg);
            }

            else if (message.Text.StartsWith("/download") && message.Text.Length > 9)
            {
                List<int> indexes = new List<int>();

                indexes = DecomposeIndexes(message);

                try
                {
                    foreach (var index in indexes)
                    {
                        string currentFile = filesNames[index];
                        string currentFileShortName = currentFile.Substring(currentFile.LastIndexOf('/'));
                        string currentFileResolution = currentFile.Substring(filesNames[index].LastIndexOf('.'));
                        Stream stream = new FileStream(currentFile, FileMode.Open);
                        InputOnlineFile sendingFile = new InputOnlineFile(stream, currentFileShortName);

                        switch (currentFileResolution)
                        {
                            case ".ogg":                               
                                bot.SendAudioAsync(message.Chat.Id, sendingFile);
                                break;
                            case ".mov":
                                bot.SendVideoAsync(message.Chat.Id, sendingFile, 0, 0, 0);
                                break;
                            case ".mp3":
                                bot.SendAudioAsync(message.Chat.Id, sendingFile);
                                break;
                            case ".jpeg":
                                bot.SendPhotoAsync(message.Chat.Id, sendingFile);
                                break;
                            default:
                                bot.SendDocumentAsync(message.Chat.Id, sendingFile);
                                break;
                        }
                    }
                }
                catch (Exception)
                {
                    bot.SendTextMessageAsync(message.Chat.Id, "Загрузка не удалась");
                }
            }

            else
            {
                bot.SendTextMessageAsync(message.Chat.Id,
                  "/list - посмотреть все загруженные файлы\n" +
                  "/delete 0 1 2... - удалить файлы c соответствующими индексами\n" +
                  "/download 0 1 2 - скачать файлы с соответствующими индексами\n" +
                  "Для загрузки файла - прикрепите его к сообщению\n" +
                  "Аудиосообщения отправленные боту также сохраняются");
            }

        }

        /// <summary>
        /// Обрабатывает полученные фотографии
        /// </summary>
        /// <param name="message"></param>
        private void ProcessPhoto(Telegram.Bot.Types.Message message)
        {
            string filename = "photo_" + filesNames.Count(x => x.EndsWith(".jpeg")).ToString();
            Stream stream = new FileStream(Directory.GetCurrentDirectory() + @"/files/" + filename + ".jpeg", FileMode.Create);

            bot.GetInfoAndDownloadFileAsync(message.Photo[2].FileId, stream);

            bot.SendTextMessageAsync(message.Chat.Id, $"Файл {filename} загружен!");
        }

        /// <summary>
        /// Обрабатывает полученные файлы
        /// </summary>
        /// <param name="message"></param>
        private void ProcessOtherFile(Telegram.Bot.Types.Message message)
        {
            object fileId = null;
            object filename = null;
            var msgType = message.Type;

            switch (msgType)
            {
                case Telegram.Bot.Types.Enums.MessageType.Audio:
                    fileId = message.Audio.FileId;
                    filename = message.Audio.Title + ".mp3";
                    break;
                case Telegram.Bot.Types.Enums.MessageType.Video:
                    fileId = message.Video.FileId;
                    filename = "video_" + filesNames.Count(x => x.EndsWith(".mov")).ToString() + ".mov";
                    break;
                case Telegram.Bot.Types.Enums.MessageType.Voice:
                    fileId = message.Voice.FileId;
                    filename = "voice_" + filesNames.Count(x => x.EndsWith(".ogg")).ToString() + ".ogg";
                    break;
                case Telegram.Bot.Types.Enums.MessageType.Document:
                    fileId = message.Document.FileId;
                    filename = message.Document.FileName;
                    break;
                default:
                    break;
            }

            Stream stream = new FileStream((Directory.GetCurrentDirectory() + @"/files/" + filename), FileMode.Create);
            bot.GetInfoAndDownloadFileAsync((string)fileId, stream);
            bot.SendTextMessageAsync(message.Chat.Id, $"Файл {(string)filename} загружен!");
        }

        /// <summary>
        /// Извлекает индексы из сообщения-команды
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <returns>Список индексов</returns>
        private List<int> DecomposeIndexes(Telegram.Bot.Types.Message message)
        {
            var indexes = new List<int>();
            try
            {
                message.Text.Substring(message.Text.IndexOf(' ') + 1).Split(' ').ToList().ForEach(x => indexes.Add(int.Parse(x)));
            }
            catch (ArgumentException)
            {
                bot.SendTextMessageAsync(message.Chat.Id, "Ошибка: индексы введены неверно");
                indexes.Clear();
            }
            catch (FormatException)
            {
                bot.SendTextMessageAsync(message.Chat.Id, "Неверное использование команды");
                indexes.Clear();
            }
            catch (Exception ex)
            {
                bot.SendTextMessageAsync(message.Chat.Id, $"Ошибка: {ex.Message}");
                indexes.Clear();
            }
            return indexes;
        }
        public void SendMessage(long chatID, string message)
        {
            bot.SendTextMessageAsync(chatID, message);
        }

        public void AddListener(EventHandler<MessageEventArgs> handler)
        {
            bot.OnMessage += handler;
        }

    }
}
