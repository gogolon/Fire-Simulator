using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireSimulator
{
    public enum WindDirection
    {
        N, S, W, E
    }

    class Program
    {
        private static void mainMenu()
        {
            Console.WriteLine("Podaj ilosc drzew w symulacji: ");
            int forestSize = Convert.ToInt32(Console.ReadLine());
            WindDirection direction = askWindDirection();
            int threadsNumber = askThreadsNumber(forestSize);
            bool showUsage = askShowUsage();

            ForestFireSimulator simulator = new ForestFireSimulator(forestSize, direction, threadsNumber, showUsage);
            simulator.StartSimulation();
        }

        private static bool askShowUsage()
        {
            Console.WriteLine("Wyswietlac zuzycie zasobow? T/N");
            char response = Char.ToUpper(Convert.ToChar(Console.ReadLine()));
            switch(response)
            {
                case 'T':
                    return true;
                    break;
                case 'N':
                    return false;
                    break;
                default:
                    return askShowUsage();
                    break;
            }
        }

        private static int askThreadsNumber(int forestSize)
        {
            Console.WriteLine("Podaj liczbe watkow:");
            int threads = Convert.ToInt32(Console.ReadLine());
            if (threads > forestSize) return askThreadsNumber(forestSize);
            else return threads;
        }

        private static WindDirection askWindDirection()
        {
            Console.WriteLine("Podaj kierunek wiatru (N, S, W, E)");
            char windDirection = Convert.ToChar(Console.ReadLine());
            windDirection = Char.ToUpper(windDirection);

            switch (windDirection)
            {
                case 'N':
                    return WindDirection.N;
                case 'S':
                    return WindDirection.S;
                case 'W':
                    return WindDirection.W;
                case 'E':
                    return WindDirection.E;
                default:
                    Console.WriteLine("Bledny kierunek wiatru...");
                    return askWindDirection();
            }
        }

        static void Main(string[] args)
        {
            mainMenu();
        }
    }
}
