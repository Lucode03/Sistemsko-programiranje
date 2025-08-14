namespace Projekat3_Zadatak18
{
    internal class Program
    {
        static void Main()
        {
            string urlPrefix = "http://localhost:5050/";
            Server server = new Server(urlPrefix);
            server.Start();

            Console.WriteLine("Pritisni ENTER za izlaz...");
            Console.ReadLine();

            server.Stop();
        }
    }
}
