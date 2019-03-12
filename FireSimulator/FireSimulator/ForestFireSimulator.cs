using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Dynamic;
using System.IO;

namespace FireSimulator
{
    class TreeData
    {
        public int j = 0;
        public int i = 0;
    }

    class ForestFireSimulator
    {
        #region Variables declarations
        const string logPath = "log.txt";

        int frameDuration = 1;
        Tree[,] trees;
        int treesNumber;
        int xSize;
        int ySize;
        int threads;
        WindDirection direction;
        int treesBurned = 0;
        bool showUsage;

        string lastPrint;
        #endregion

        public ForestFireSimulator(int treesNumber, WindDirection windDirection, int threads = 1, bool showUsage = false)
        {
            this.treesNumber = treesNumber;
            this.threads = threads;
            this.showUsage = showUsage;
            xSize = (int)Math.Sqrt(treesNumber);
            ySize = xSize + (int)Math.Ceiling((decimal)(treesNumber - (xSize * xSize)) / xSize);
            direction = windDirection;
            trees = new Tree[xSize, ySize];
            for (int i = 0; i < ySize; ++i)
            {
                for (int j = 0; j < xSize; ++j)
                {
                    if ((i * xSize + j) < treesNumber) trees[j, i] = new Tree();
                    else
                    {
                        trees[j, i] = null;
                    }
                }
            }

            //Console.WriteLine("Trees: " + treesNumber + " | " + xSize + "x" + ySize + " | Free: " + free + Environment.NewLine);
        }

        private void startFire() // Starts fire in random place / Wznieca pożar w losowym miejscu
        {
            Random random = new Random();
            int x = random.Next(0, xSize);
            int y = random.Next(0, ySize);
            if (trees[x, y] == null) startFire();
            else trees[x, y].Burning = true;
        }

        public void StartSimulation()
        {
            int fixedSizeX = xSize * 3 + 1;
            int fixedSizeY = ySize + 1;
            if (Console.LargestWindowWidth >= fixedSizeX && Console.LargestWindowHeight >= fixedSizeY) Console.SetWindowSize(fixedSizeX, fixedSizeY);
            else Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
            Console.Clear();

            Stopwatch stopWatch = Stopwatch.StartNew();
            long lastLength = 0;

            int treesPerThread = treesNumber / threads;

            //trees[0, 0].Burning = true; // FOR TESTING ONLY / TYLKO W CELU TESTOWANIA
            startFire();

            while (treesBurned < treesNumber)
            {
                treesBurned = 0;
                List<Task> tasks = new List<Task>();
                for (int i = 0; i < threads; ++i)
                {
                    if (i == (threads - 1))
                        treesPerThread = (treesNumber - (i * treesPerThread));

                    List<TreeData> treesData = new List<TreeData>();
                    for (int j = 0; j < treesPerThread; ++j)
                    {
                        int treeIndex = i * treesPerThread + j;

                        TreeData treeData = new TreeData();
                        treeData.j = treeIndex % xSize;
                        treeData.i = (treeIndex - treeData.j) / xSize;

                        if (trees[treeData.j, treeData.i] != null) treesData.Add(treeData);
                    }

                    tasks.Add(new Task(calculateTreeData, treesData));
                    tasks.Last().Start();
                }

                Task.WaitAll(tasks.ToArray());

                for (int k = 0; k < ySize; ++k)
                {
                    for (int l = 0; l < xSize; ++l)
                    {
                        if (trees[l, k] != null)
                        {
                            trees[l, k].NextFrame();
                            if (trees[l, k].Burned) treesBurned++;
                        }
                    }
                }
                print();
                stopWatch.Stop();
                //System.Threading.Thread.Sleep(frameDuration);
                lastLength = stopWatch.ElapsedMilliseconds - lastLength;
                
                if(showUsage)
                {
                    getUsage(lastLength, new showResults(showFrameInfo));
                    //Console.ReadKey();
                }
                stopWatch.Start();
            }
            WriteOnBottomLine("Symulacja skonczona, czas wykonania: " + stopWatch.Elapsed + "            ");
            Console.ReadKey();
        }

        void calculateTreeData(object a)
        {
            List<TreeData> treesData = a as List<TreeData>;

            for (int k = 0; k < treesData.Count; ++k)
            {
                Tree[] treesAround = new Tree[8];

                if (treesData[k].j - 1 >= 0 && treesData[k].i - 1 >= 0) treesAround[0] = trees[treesData[k].j - 1, treesData[k].i - 1];
                if (treesData[k].j - 1 >= 0) treesAround[1] = trees[treesData[k].j - 1, treesData[k].i];
                if (treesData[k].j - 1 >= 0 && treesData[k].i + 1 < ySize) treesAround[2] = trees[treesData[k].j - 1, treesData[k].i + 1];
                if (treesData[k].i + 1 < ySize) treesAround[3] = trees[treesData[k].j, treesData[k].i + 1];
                if (treesData[k].j + 1 < xSize && treesData[k].i + 1 < ySize) treesAround[4] = trees[treesData[k].j + 1, treesData[k].i + 1];
                if (treesData[k].j + 1 < xSize) treesAround[5] = trees[treesData[k].j + 1, treesData[k].i];
                if (treesData[k].j + 1 < xSize && treesData[k].i - 1 >= 0) treesAround[6] = trees[treesData[k].j + 1, treesData[k].i - 1];
                if (treesData[k].i - 1 >= 0) treesAround[7] = trees[treesData[k].j, treesData[k].i - 1];

                trees[treesData[k].j, treesData[k].i].CalculateNextFrame(treesAround, direction);
            }
        }

        private void print()
        {
            Console.CursorVisible = false;
            string currentPrint = "";

            for (int i = 0; i < ySize; ++i)
            {
                for (int j = 0; j < xSize; ++j)
                {
                    if (trees[j, i] != null) currentPrint += trees[j, i].ToChar() + "  ";
                }

                currentPrint += '\n';
            }

            if (lastPrint != null)
            {
                int rowLength = 0;
                bool rowLengthDetected = false;
                int rows = 0;
                for (int i = 0; i < currentPrint.Length; ++i)
                {
                    if (!rowLengthDetected) rowLength++;
                    if (currentPrint[i] == '\n')
                    {
                        rows++;
                        rowLengthDetected = true;
                    }

                    if (currentPrint[i] != lastPrint[i])
                    {
                        int xPos = (rowLengthDetected) ? (i % rowLength) : i;
                        Console.SetCursorPosition(xPos, rows);
                        Console.Write(currentPrint[i]);
                    }
                }
            }
            else
            {
                Console.Write(currentPrint);
            }

            lastPrint = currentPrint;
        }

        #region PerformanceFunctions
        void log(string data)
        {
            using (StreamWriter sw = File.AppendText(logPath))
            {
                sw.WriteLine(data);
            }
        }

        void showFrameInfo(long frameLength, dynamic data)
        {
            string text = "Czas wykonania klatki: " + frameLength + " milisekund. Zużycie procesora: " + data.CPU + "%, ram: " + data.RAM + " bajtów        ";
            //WriteOnBottomLine(text);

            log(text);
        }

        delegate void showResults(long frameLength, dynamic data);
        private void getUsage(long frameLength, showResults callback)
        {
            var process = Process.GetCurrentProcess();

            var name = string.Empty;

            foreach (var instance in new PerformanceCounterCategory("Process").GetInstanceNames())
            {
                if (instance.StartsWith(process.ProcessName))
                {
                    using (var processId = new PerformanceCounter("Process", "ID Process", instance, true))
                    {
                        if (process.Id == (int)processId.RawValue)
                        {
                            name = instance;
                            break;
                        }
                    }
                }
            }

            var cpu = new PerformanceCounter("Process", "% Processor Time", name, true);
            var ram = new PerformanceCounter("Process", "Private Bytes", name, true);

            cpu.NextValue();
            ram.NextValue();

            Thread.Sleep(500);

            dynamic result = new ExpandoObject();

            result.CPU = Math.Round(cpu.NextValue() / Environment.ProcessorCount, 2);

            result.RAM = Math.Round(ram.NextValue() / 1024 / 1024, 2);
            callback(frameLength, result);
        }
        #endregion

        #region Helpers
        void WriteOnBottomLine(string text)
        {
            int x = Console.CursorLeft;
            int y = Console.CursorTop;
            Console.CursorTop = Console.WindowTop + Console.WindowHeight - 1;
            Console.CursorLeft = Console.WindowLeft;
            Console.Write(text);
            Console.SetCursorPosition(x, y);
        }
        #endregion
    }
}
