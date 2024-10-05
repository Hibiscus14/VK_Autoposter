using System.Security.Cryptography;

namespace VK_Autoposter
{
    internal static class Utils
    {

        public static Dictionary<string, DateTime> GeneratePublishSchedule(List<string> images, List<DayOfWeek> daysOfWeek, int imagesPerDay, List<TimeSpan> publishTimes)
        {
            Dictionary<string, DateTime> schedule = new Dictionary<string, DateTime>();
            DateTime startDate = DateTime.Now.Date;

            var imagesCount = Config.DaysCount;
            if (imagesCount == 0 || imagesCount > images.Count) imagesCount = images.Count;

            for (int i = 0; i < imagesCount; i++)
            {
                for (int j = 0; j < imagesPerDay; j++)
                {
                    foreach (var publishTime in publishTimes)
                    {

                        DateTime publishDate = GetNextPublishDate(ref startDate, daysOfWeek, publishTime);
                        if (!schedule.ContainsKey(images[i]) && !schedule.ContainsValue(publishDate))
                            schedule.Add(images[i], publishDate);
                    }
                }
                startDate = startDate.AddDays(1);
            }

            return schedule;

        }

        private static DateTime GetNextPublishDate(ref DateTime startDate, List<DayOfWeek> daysOfWeek, TimeSpan publishTime)
        {
            while (!daysOfWeek.Contains(startDate.DayOfWeek))
            {
                startDate = startDate.AddDays(1);
            }

            return startDate.Date.Add(publishTime);
        }

        public static void LogError(string image, DateTime date, Exception ex)
        {
            string logFilePath = "error_log.txt";

            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: Ошибка при публикации поста {image} на дату {date}: {ex.Message}\n");
                writer.WriteLine(ex.StackTrace);
            }
        }
        public static void MoveImage(string imagePath, string time)
        {

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "//results"))
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "//results");

            if (!Directory.Exists(Directory.GetCurrentDirectory() + $"//results//{time}"))
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + $"//results//{time}");

            string destinationPath = Path.Combine(Directory.GetCurrentDirectory() + $"//results//{time}", Path.GetFileName(imagePath));

            File.Move(imagePath, destinationPath);
        }
        public static void Shuffle<T>(this IList<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}