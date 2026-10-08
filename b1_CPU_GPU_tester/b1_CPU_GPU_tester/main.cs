using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;


class BenchmarkAutomation
{
    static string BenchmarkPath = @"C:\Program Files (x86)\Steam\steamapps\common\Black Myth Wukong Benchmark Tool\b1\Binaries\Win64\b1-Win64-Shipping.exe";
    static string ConfigPath =  @"C:\Program Files (x86)\Steam\steamapps\common\Black Myth Wukong Benchmark Tool\b1\Saved\Config\Windows\GameUserSettings.ini";

    static string StatisticFolder = "Statistics";
    static void Main()
    {
        if (Directory.Exists(StatisticFolder))
            Directory.Delete(StatisticFolder, recursive: true);
        string dataDirectory = Path.Combine(AppContext.BaseDirectory, StatisticFolder);
        // 1. CPU Тест
        string settingsType = "CPU_test_settings";
        ApplyConfig(settingsType+ ".txt");

        Console.WriteLine("Запуск CPU-теста...");
        RunAndWaitForBenchmark(dataDirectory, settingsType);
        Console.WriteLine("CPU тест завершён");

        // 2. GPU Тест
        settingsType = "GPU_test_settings";
        ApplyConfig(settingsType+ ".txt");

        Console.WriteLine("Запуск GPU-теста...");
        RunAndWaitForBenchmark(dataDirectory, settingsType);
        Console.WriteLine("GPU тест завершён");

        OpenScreenshots(dataDirectory);
    }

    static void ApplyConfig(string settingsType)
    {
        string data = File.ReadAllText(settingsType);
        File.WriteAllText(ConfigPath, data);
    }

    static void RunAndWaitForBenchmark(string dataDirectory, string settingsType)
    {
        var screen = Screen.PrimaryScreen;
        var bounds = screen.Bounds;
        int Xscrean = bounds.Width;
        int Yscrean = bounds.Height;
        var process = Process.Start(new ProcessStartInfo(BenchmarkPath) { UseShellExecute = false });
        Point startTest = new Point((int)(Xscrean * 0.1), (int)(Yscrean * 0.43));
        Point closeApp = new Point((int)(Xscrean * 0.1), (int)(Yscrean * 0.58));
        if (process == null)
        {
            Console.WriteLine("Black Myth: Wukong Benchmark Tool не найдено");
            return;
        }
        process.WaitForExit();
        Thread.Sleep(10000);
        process = Process.GetProcessesByName("b1-Win64-Shipping").FirstOrDefault();
        if (process == null)
        {
            Console.WriteLine("Привышено время ожидания до запуска");
            return;
        }


        Console.WriteLine("Запуск приложения. Подождите...");
        Thread.Sleep(20000);
        Mouse.Click(startTest, MouseButton.Left);
        Thread.Sleep(1000);
        Mouse.Click(startTest, MouseButton.Left);
        Thread.Sleep(300);
        Keyboard.Type(VirtualKeyShort.SPACE);
        Console.WriteLine("Тест начался...");
        int waitingTime = 180000;
        int checkFrequency = 1000;
        while (waitingTime > 0) {
            if (process.HasExited)
            {
                Console.WriteLine("Приложение закрыто досрочно");
                return;
            }
            waitingTime -=checkFrequency;
            Thread.Sleep(checkFrequency);
        }
        Console.WriteLine("Тест звершился...");
        CaptureScreen(dataDirectory, settingsType);
        Mouse.Click(closeApp, MouseButton.Right);
        Thread.Sleep(200);
        Keyboard.Type(VirtualKeyShort.SPACE);
        Thread.Sleep(15000);
        Mouse.Click(closeApp, MouseButton.Left);
        Thread.Sleep(1000);
        Mouse.Click(closeApp, MouseButton.Left);
        Thread.Sleep(300);
        Keyboard.Type(VirtualKeyShort.SPACE);
        process.WaitForExit();
    }

    static void CaptureScreen(string path, string fileName)
    {
        Thread.Sleep(200);
        var bounds = Screen.PrimaryScreen.Bounds;

        using var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
        Directory.CreateDirectory(path);
        bmp.Save(Path.Combine(path, fileName+".jpeg"), ImageFormat.Jpeg);
        Thread.Sleep(200);
    }

    static void OpenScreenshots(string folder)
    {
        var photos = Directory.EnumerateFiles(folder)
                              .Where(f => f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                              .ToList();

        foreach (var photo in photos)
        {
            Process.Start(new ProcessStartInfo(photo) { UseShellExecute = true });
        }
    }

}
