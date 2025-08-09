
using Projekat1_Zadatak7;
using Server1;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        /*ThreadPool.GetMinThreads(out int worker, out int io);
        Console.WriteLine($"Min Threads: Worker={worker}, IO={io}");

        ThreadPool.GetMaxThreads(out worker, out io);
        Console.WriteLine($"Max Threads: Worker={worker}, IO={io}");*/

        //TestiranjeKesa.Testiraj();

        ThreadPool.SetMinThreads(10, 5);

        string rootFolder = Path.Combine(Directory.GetCurrentDirectory(), "Images");
        string urlPrefix = "http://localhost:5050/";

        Console.WriteLine($"Root folder: {rootFolder}");
        Console.WriteLine("Podrzani tipovi slika: .png, .jpg, .jpeg, .gif, .bmp, .svg, .webp");

        Server server = new Server(rootFolder, urlPrefix);
        server.Start();

        Console.WriteLine("Pritisnite ENTER kako bi ste zaustavili server...");
        Console.ReadLine();

        server.Stop();
    }
}