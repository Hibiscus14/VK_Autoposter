using System.Net;
using System.Text;
using VkNet;
using VkNet.Model;

namespace VK_Autoposter
{
    internal static class Autoposter
    {
        private static VkApi vk;


        static async Task Main(string[] args)
        {
            vk = new VkApi();

            while (true)
            {
                Console.WriteLine("Выберите пункт меню: \n1 - Создать новый файл конфигурации \n2 - Выбрать файл конфигурации из уже существующих \n3 - Удалить файл конфигурации \n4 - Выйти\n ");
                var choice = Console.ReadKey();
                switch (choice.Key)
                {
                    case ConsoleKey.D1:
                        Config.Create();
                        ReadData();
                        break;
                    case ConsoleKey.D2:
                        Config.Choose();
                        ReadData();
                        break;
                    case ConsoleKey.D3:
                        Config.Choose();
                        Config.Delete();
                        break;
                    case ConsoleKey.D4:
                        Environment.Exit(1);
                        break;

                    default:
                        Console.WriteLine("\nНекорректное значение");
                        return;

                }
            }
            Console.ReadKey();
        }



        private static void ReadData()
        {
            try
            {
                Config.Read();

                if (Config.AccessToken == null)
                {
                    Config.WriteAccessToken();
                }
                vk.Authorize(new ApiAuthParams { AccessToken = Config.AccessToken });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nОшибка входных данных: {ex}");

            }
            StartPosting();
        }

        private static void StartPosting()
        {
            var attemptTime = DateTime.Now.ToShortDateString();

            var images = Directory.GetFiles(Config.ImageFolderPath, "*.jpg").Concat(Directory.GetFiles(Config.ImageFolderPath, "*.png")).ToList();
            if (images.Count == 0)
            {
                Console.WriteLine("\nВ папке нет изображений.");
                return;
            }

            if (Config.Shuffle) images.Shuffle();

            var publishSchedule = Utils.GeneratePublishSchedule(images, Config.DaysOfWeek, Config.PostsPerDay, Config.PostTimes);

            Console.WriteLine();
            int success = 0;
            Dictionary<string, DateTime> unsuccessfullyPublished = new Dictionary<string, DateTime>();
            foreach (var post in publishSchedule)
            {
                try
                {
                    PostImageToWall(post);
                    success++;
                    Utils.MoveImage(post.Key, attemptTime);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nОшибка при публикации поста {post.Key} на дату {post.Value}: {ex}");
                    Utils.LogError(post.Key, post.Value, ex);
                    unsuccessfullyPublished.Add(post.Key, post.Value);


                }

            }
            Console.WriteLine($"\nУспешно опубликовано {success} постов!");

            if (unsuccessfullyPublished.Count > 0)
            {
                Console.WriteLine($"\nОпубликовано с ошибкой {unsuccessfullyPublished.Count} постов.\nПопробовать снова?(y/n)");
                var answer = Console.ReadKey();
                if (answer.Key == ConsoleKey.Y)
                {
                    foreach (var post in unsuccessfullyPublished)
                        PostImageToWall(post);
                }

            }

        }



        private static void PostImageToWall(KeyValuePair<string, DateTime> post)
        {
            var uploadServer = vk.Photo.GetWallUploadServer(Config.GroupId);
            var uploader = new WebClient();
            var responseFile = Encoding.ASCII.GetString(uploader.UploadFile(uploadServer.UploadUrl, post.Key));

            var photos = vk.Photo.SaveWallPhoto(responseFile, null, (ulong)Config.GroupId);

            vk.Wall.Post(new WallPostParams
            {
                OwnerId = -Config.GroupId,
                Attachments = photos,
                FromGroup = true,
                Signed = false,
                PublishDate = post.Value
            });

            Console.WriteLine($"Запланирован пост: {post.Key} на {post.Value}");
        }

    }


}