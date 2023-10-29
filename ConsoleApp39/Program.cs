using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ConsoleApp39
{
    class Program
    {
        static public bool foundFlag = false;
        static void Main(string[] args)
        {
            string passwordHash;
            int countStream;
            while (true)
            {
                Console.WriteLine("Введите хэш значение: ");
                passwordHash = Console.ReadLine().ToUpper();
                Console.Write("\tВведите количество потоков: ");
                countStream = int.Parse(Console.ReadLine());
                Console.WriteLine("\tОжидайте подбор пароля...");
                Channel<string> channel1 = Channel.CreateBounded<string>(countStream);
                Stopwatch time1 = new();
                time1.Reset();
                time1.Start();

                var prod1 = Task.Run(() => { new Producer(channel1.Writer); });
                Task[] streams1 = new Task[countStream + 1];
                streams1[0] = prod1;

                for (int i = 1; i < countStream + 1; i++)
                {
                    streams1[i] = Task.Run(() => { new Consumer(channel1.Reader, passwordHash); });
                }

                Task.WaitAny(streams1);
                time1.Stop();
                Console.WriteLine($"\tЗатраченное время на подбор: {time1.Elapsed}");
                Console.WriteLine("\tВведите ENTER, чтобы выйти в главное меню.");
                Console.WriteLine();
                Console.ReadKey();
                foundFlag = false;
            }
        }
    }

    class Producer
    {
        private ChannelWriter<string> Writer;

        public Producer(ChannelWriter<string> _writer)
        {
            Writer = _writer;
            Task.WaitAll(Run());
        }

        private async Task Run()
        {

            while (await Writer.WaitToWriteAsync())
            {
                char[] word = new char[5];
                for (int i = 97; i < 123; i++)
                {
                    word[0] = (char)i;
                    for (int k = 97; k < 123; k++)
                    {
                        word[1] = (char)k;
                        for (int l = 97; l < 123; l++)
                        {
                            word[2] = (char)l;
                            for (int m = 97; m < 123; m++)
                            {
                                word[3] = (char)m;
                                for (int n = 97; n < 123; n++)
                                {
                                    word[4] = (char)n;
                                    if (!Program.foundFlag)
                                    {
                                        await Writer.WriteAsync(new string(word));
                                    }
                                    else
                                    {
                                        Writer.Complete();
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    class Consumer
    {
        private ChannelReader<string> Reader;
        private string PasswordHash;

        public Consumer(ChannelReader<string> _reader, string _passwordHash)
        {
            Reader = _reader;
            PasswordHash = _passwordHash;
            Task.WaitAll(Run());
        }

        private async Task Run()
        {

            while (await Reader.WaitToReadAsync())
            {
                if (!Program.foundFlag)
                {
                    var item = await Reader.ReadAsync();
                    if (FoundHash(item.ToString()) == PasswordHash)
                    {
                        Console.WriteLine($"\tПароль подобран - {item}");
                        Program.foundFlag = true;
                    }
                }
                else return;
            }
        }

        static public string FoundHash(string str)
        {
            SHA256 sha256Hash = SHA256.Create();
            byte[] sourceBytes = Encoding.ASCII.GetBytes(str);
            byte[] hashBytes = sha256Hash.ComputeHash(sourceBytes);
            string hash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
            return hash;
        }
    }
}

