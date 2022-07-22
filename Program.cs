
using System;
using System.IO;
using VkNet;
using VkNet.Model;
using VkNet.Enums.SafetyEnums;
using VkNet.Enums.Filters;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace Program
{
    class database
    {
        public database()
        {
            if (!File.Exists("userdata.db"))
            {
                using (var connection = new SqliteConnection("Data Source = userdata.db"))
                {
                    connection.Open();

                    SqliteCommand command = new SqliteCommand();
                    command.Connection = connection;
                    command.CommandText = "CREATE TABLE Users(id INTEGER PRIMARY KEY NOT NULL UNIQUE, money INTEGER NOT NULL DEFAULT(10000))";
                    command.ExecuteNonQuery();
                    command.CommandText = "CREATE TABLE DuelLobbys(chatId INTEGER PRIMARY KEY NOT NULL UNIQUE, FstPlayer INTEGER, SndPlayer INTEGER, FOREIGN KEY (FstPlayer) REFERENCES Users (id), FOREIGN KEY (SndPlayer) REFERENCES Users (id))";
                    command.ExecuteNonQuery();

                    Console.WriteLine("База данных пользователей была создана");
                }
            }
            else
            {
                Console.WriteLine("База данных уже имеется");
                using (var connection = new SqliteConnection("Data Source = userdata.db"))
                {
                    connection.Close();
                }
            }
        }
        public bool checkid(long? id)
        {
            using (var connection = new SqliteConnection("Data source = userdata.db"))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT EXISTS(SELECT id FROM Users WHERE id = {id})";
                if (Convert.ToBoolean(command.ExecuteScalar()) == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool addUser(long? id)
        {
            using (var connection = new SqliteConnection("Data source = userdata.db"))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT EXISTS(SELECT id FROM Users WHERE id = {id})";
                if (Convert.ToBoolean(command.ExecuteScalar()) == false)
                {
                    command.CommandText = $"INSERT INTO Users VALUES({id}, 10000)";
                    command.ExecuteNonQuery();
                    Console.WriteLine("Пользователь добавлен");
                    return false;
                }
                else
                {
                    Console.WriteLine("Пользователь уже есть");
                    return true;
                }
            }
        }
        public bool accrualMoney(long? id, long bet)
        {
            using (var connection = new SqliteConnection("Data source = userdata.db"))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT EXISTS(SELECT id FROM Users WHERE id = {id})";
                if (Convert.ToBoolean(command.ExecuteScalar()) == true)
                {
                    command.CommandText = $"UPDATE Users SET money = (SELECT money FROM Users WHERE id = {id}) + {bet} WHERE id = {id}";
                    command.ExecuteNonQuery();
                    Console.WriteLine("Значение изменено");
                    return true;
                }
                else
                {
                    Console.WriteLine("Пользователь не найден");
                    return false;
                }
            }
        }
        public long getbalance(long? id)
        {
            using (var connection = new SqliteConnection("Data source = userdata.db"))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT EXISTS(SELECT id FROM Users WHERE id = {id})";

                if (Convert.ToBoolean(command.ExecuteScalar()) == true)
                {
                    command.CommandText = $"SELECT money FROM Users WHERE id = {id}";
                    Console.WriteLine("Баланс отправлен");
                    return Convert.ToInt64(command.ExecuteScalar());
                }
                else
                {
                    Console.WriteLine("Пользователь не найден");
                    return -404;
                }
            }
        }
        public bool checklobby(long? chatId)
        {
            using (var connection = new SqliteConnection("Data source = userdata.db"))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT EXISTS(SELECT chatId FROM DuelLobbys WHERE chatId = {chatId})";
                if (Convert.ToBoolean(command.ExecuteScalar()) == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public void addlobby(long? chatId)
        {
            using (var connection = new SqliteConnection("Data source = userdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"INSERT INTO DuelLobbys VALUES({chatId}, NULL, NULL)";
                command.ExecuteNonQuery();
            }
        }
        public bool addusertolobby(long? chatId, long? id)
        {
            using (var connection = new SqliteConnection("Data source = userdata.db"))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT FstPlayer FROM DuelLobbys WHERE chatID = {chatId}";

                var x = command.ExecuteScalar().ToString();

                if (x == "")
                {
                    command.CommandText = $"UPDATE DuelLobbys SET FstPlayer = {id} WHERE chatId = {chatId}";
                    command.ExecuteNonQuery();
                    return false;
                }
                else
                {
                    command.CommandText = $"UPDATE DuelLobbys SET SndPlayer = {id} WHERE chatId = {chatId}";
                    command.ExecuteNonQuery();
                    return true;
                }
            }
        }
        public void deletelobby(long? chatId)
        {
            using (var connection = new SqliteConnection("Data source = userdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"DELETE FROM DuelLobbys WHERE chatId = {chatId}";
                command.ExecuteNonQuery();
            }
        }
        public long FstPlayer(long? chatId)
        {
            using (var connection = new SqliteConnection("Data source = userdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT FstPlayer FROM DuelLobbys WHERE chatId = {chatId}";
                return Convert.ToInt64(command.ExecuteScalar());
            }
        }
        public long SndPlayer(long? chatId)
        {
            using (var connection = new SqliteConnection("Data source = userdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT SndPlayer FROM DuelLobbys WHERE chatId = {chatId}";
                return Convert.ToInt64(command.ExecuteScalar());
            }
        }

    }
    class vknet
    {
        public VkApi api = new VkApi();
        public Random rand = new Random();
        public vknet()
        {
            api.Authorize(new ApiAuthParams { AccessToken = "7e47e6963f9dd3706247f72496b4873a9902e8bf0338aacf449c96e8dde786b907b466c03494893a2e2a8" });
        }
        public string username(long? id)
        {
            var user = api.Users.Get(new long[] { (long)id }).FirstOrDefault();
            return user.FirstName + " " + user.LastName;
        }
        public bool isMessageFromChat(long? id, long? peerId)
        {
            if (id == peerId) return false;
            else return true;
        }
        public void messageSend(long? id, long? chatid, string message)
        {
            if (id == chatid)
            {
                api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams
                {
                    Message = message,
                    UserId = id,
                    RandomId = rand.Next(),
                });
            }
            else
            {
                api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams
                {
                    Message = message,
                    ChatId = chatid - 2000000000,
                    RandomId = rand.Next(),
                });
            }
        }
        public long OneHandBandit(long? id, long? chatId, long bet)
        {
            string[] slots = new string[]
            {
                "&#128163;",
                "&#127819;",
                "&#127826;",
                "&#128276;",
                "&#128142;"
            };
            string[] combination = new string[3];
            Random rand = new Random();
            for (int i = 0; i < 3; i++)
            {
                combination[i] += slots[rand.Next(0, 5)];
            }
            if (combination[0] == combination[1] && combination[1] == combination[2] && combination[0] != slots[0])
            {
                string message = username(id) + ", ваша комбинация:\n" + combination[0] + combination[1] + combination[2] + '\n' + "Вы выиграли:" + bet * Array.IndexOf(slots, combination[0]);
                messageSend(id, chatId, message);
                return bet * Array.IndexOf(slots, combination[0]);
            }
            else
            {
                string message = username(id) + ", ваша комбинация:\n" + combination[0] + combination[1] + combination[2] + '\n' + "Вы ничего не выиграли";
                messageSend(id, chatId, message);
                return -bet;
            }


        }
    }
    public class Program
    {
        static void Main()
        {
            database vk = new database();
            vknet vknet = new vknet();
            while (true)
            {
                var s = vknet.api.Groups.GetLongPollServer(186588300);
                var poll = vknet.api.Groups.GetBotsLongPollHistory(new VkNet.Model.RequestParams.BotsLongPollHistoryParams
                {
                    Server = s.Server,
                    Ts = s.Ts,
                    Key = s.Key,
                    Wait = 25
                });

                if (poll?.Updates == null) continue;
                else
                {
                    foreach (var a in poll.Updates)
                    {
                        if (a.Type == GroupUpdateType.MessageNew)
                        {
                            var options = new JsonSerializerOptions
                            {
                                WriteIndented = true,
                            };
                            //Console.WriteLine(JsonSerializer.Serialize(a.MessageNew.Message));

                            string[] userMessage = a.MessageNew.Message.Text.Split(new char[] { ' ' });
                            long? userid = a.MessageNew.Message.FromId;
                            long? chatid = a.MessageNew.Message.PeerId;

                            switch (userMessage[0])
                            {
                                case "/reg":
                                    if (!vk.addUser(a.MessageNew.Message.FromId))
                                    {
                                        vknet.api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams
                                        {
                                            Message = "Вы успешно зарегистрировались",
                                            ChatId = a.MessageNew.Message.PeerId - 2000000000,
                                            RandomId = vknet.rand.NextInt64(),
                                        });
                                    }
                                    else
                                    {
                                        vknet.api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams
                                        {
                                            Message = "Вы уже зарегистрированы",
                                            ChatId = a.MessageNew.Message.PeerId - 2000000000,
                                            RandomId = vknet.rand.NextInt64(),
                                        });
                                    }
                                    break;

                                case "/balance":
                                    long money = Convert.ToInt64(vk.getbalance(a.MessageNew.Message.FromId));
                                    if (money != -404)
                                    {
                                        vknet.api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams
                                        {
                                            Message = $"Ваш баланс:{money}",
                                            ChatId = a.MessageNew.Message.PeerId - 2000000000,
                                            RandomId = vknet.rand.NextInt64(),
                                        });
                                    }
                                    else
                                    {
                                        vknet.api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams
                                        {
                                            Message = "Вы не зарегистрированы",
                                            ChatId = a.MessageNew.Message.PeerId - 2000000000,
                                            RandomId = vknet.rand.NextInt64(),
                                        });
                                    }
                                    break;

                                case "/help":
                                    break;

                                case "/duel":
                                    if (vknet.isMessageFromChat(userid, chatid))
                                    {
                                        if (vk.checklobby(chatid))
                                        {
                                            if (vk.addusertolobby(chatid, userid))
                                            {
                                                vknet.messageSend(userid, chatid, $"{vknet.username(userid)} добавлен в дуэль");
                                                var random = vknet.rand.Next(0, 2);
                                                if (random == 0)
                                                {
                                                    vknet.messageSend(userid, chatid, $"{vknet.username(vk.FstPlayer(chatid))} победил в дуэли против {vknet.username(vk.SndPlayer(chatid))}");
                                                    vk.deletelobby(chatid);
                                                }
                                                else
                                                {
                                                    vknet.messageSend(userid, chatid, $"{vknet.username(vk.SndPlayer(chatid))} победил в дуэли против {vknet.username(vk.FstPlayer(chatid))}");
                                                    vk.deletelobby(chatid);
                                                }

                                            }
                                            else
                                            {
                                                vknet.messageSend(userid, chatid, $"{vknet.username(userid)} добавлен в дуэль");
                                            }
                                        }
                                        else
                                        {
                                            vk.addlobby(chatid);
                                            vk.addusertolobby(chatid, userid);
                                            vknet.messageSend(userid, chatid, $"{vknet.username(userid)} добавлен в дуэль");
                                        }
                                    }
                                    else
                                    {
                                        vknet.messageSend(userid, chatid, "Команда недоступна в личных сообщениях");
                                    }
                                    break;

                                case "/who":
                                    if(vknet.isMessageFromChat(userid, chatid))
                                    {
                                        var chat = vknet.api.Messages.GetConversationMembers((long)chatid);
                                        if(userMessage.Length == 1)
                                        {
                                            Console.WriteLine(JsonSerializer.Serialize(chat.Items, options));
                                            long? randomuser = -1;
                                            do
                                            {
                                                randomuser = chat.Items[vknet.rand.Next(0, (int)chat.Count)].MemberId;
                                            }while(randomuser < 0);    
                                            vknet.messageSend(userid, chatid, $"{vknet.username(randomuser)} я выбираю тебя!");
                                        }
                                        else if(userMessage.Length == 2)
                                        {
                                            long? randomuser = -1;
                                            do
                                            {
                                                randomuser = chat.Items[vknet.rand.Next(0, (int)chat.Count)].MemberId;
                                            } while (randomuser < 0);
                                            vknet.messageSend(userid, chatid, $"Я думаю, что {userMessage[1]} здесь {vknet.username(chat.Items[vknet.rand.Next(0, (int)chat.Count)].MemberId)}");
                                        }
                                    }
                                    else
                                    {
                                        vknet.messageSend(userid, chatid, "Команда недоступна в личных сообщениях");
                                    }
                                    break;

                                case "/slots":
                                    if (userMessage.Length == 2)
                                    {
                                        try
                                        {
                                            if (vk.checkid(a.MessageNew.Message.FromId)) vk.accrualMoney(a.MessageNew.Message.FromId, vknet.OneHandBandit(a.MessageNew.Message.FromId, a.MessageNew.Message.PeerId, Convert.ToInt64(userMessage[1])));
                                            else
                                            {
                                                vknet.api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams
                                                {
                                                    Message = "Вы не зарегистрированы",
                                                    ChatId = a.MessageNew.Message.PeerId - 2000000000,
                                                    RandomId = vknet.rand.NextInt64(),
                                                });
                                            }
                                        }catch(Exception ex)
                                        {
                                            Console.WriteLine(ex.ToString());
                                        };
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}