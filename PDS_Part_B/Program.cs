using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;

namespace PDS_Part_B
{
    internal class Program
    {
        public static int amount = 10_00;
        public static int circlesAmount = 1000; //the number of the circles
        public static int paintingTime = 20; //20 msec
        static readonly object locker = new object();
        public static int[] workers = {1, 5, 20, 100 }; //number of workers

        static void Main(string[] args)
        {
            SectionB();
            Console.WriteLine($"FINISH");
            Console.Read();
        }

        static void SectionB()
        {
            var circleList = GenerateCoordinates();
            var temp = circlesAmount;


            var stopwatch = new Stopwatch();
            for (var i = 0; i < workers.Length; i++)
            {
                for (var j =0; j<2; j++)
                {
                    circlesAmount = temp;
                    var k = workers[i];
                    Console.WriteLine($"START for {k} workers (Option {j+1})");
                    stopwatch.Restart();
                    if (j == 0) { ThreadsWork01(circleList, k); } else { ThreadsWork02(circleList, k); }
                    stopwatch.Stop();
                    Console.Write($"Time to impliment with {k} workers (Option {j+1}): ");
                    Console.Write($"{stopwatch.ElapsedMilliseconds} miliseconds\n");
                    Console.WriteLine("---------------------------------------\n");

                    //Console.Write($"Press enter to continue...\n");
                   // Console.ReadLine();
                }

            }
            


        }
        static string[] GenerateCoordinates()
        {
            var coordinates = new string[amount];
            Random random = new Random();
            var hashcoo = new HashSet<string>();

            while (hashcoo.Count < amount)
            {
                var coord = $"{random.Next(-90, 90)}.{random.Next(1, 99999).ToString("00000")},{random.Next(-180, 180)}.{random.Next(1, 99999).ToString("00000")}";
                hashcoo.Add(coord);
            }
            coordinates = hashcoo.ToArray();

            return coordinates;
        }


        static void ThreadsWork01(string[] circleList, int k)
        {
            IDictionary<int, List<string>> workerLists = new Dictionary<int, List<string>>();
            var lastIndexes = new List<int>() { -1 };

            //start threads
            var threads = new List<Thread>();

            for (int i = 0; i < k; i++)
            {
                var thread = new Thread(() => PaintCircleMax(circleList, workerLists, lastIndexes));
                thread.Start();
                threads.Add(thread);
            }
            
            //stop threads
            foreach (var thread in threads)
            {
                thread.Join();
            }


            var sum = 0;
            foreach (var kvp in workerLists)
            {
                Console.WriteLine($"Worker id {kvp.Key}: \t Painted circles: {kvp.Value.Count}"); //{string.Join(";", kvp.Value)}"); //\t Painted circles: {kvp.Value.Count}");
                Console.WriteLine($"Length: {kvp.Value.Count}");
            }

        }

        static void ThreadsWork02(string[] circleList2, int k)
        {
            //var n = circlesAmount;

            IDictionary<int, List<string>> workerLists = new Dictionary<int, List<string>>();
            
            

            //start threads
            var threads = new List<Thread>();

            for (int i = 0; i < k; i++)
            {
                var thread = new Thread(() => PaintCircleAsk(circleList2, workerLists));
                thread.Start();
                threads.Add(thread);
            }

            //stop threads
            foreach (var thread in threads)
            {
                thread.Join();
            }


            var sum = 0;
            foreach (var kvp in workerLists)
            {
                Console.WriteLine($"Worker id {kvp.Key}: \t Painted circles: {kvp.Value.Count}"); //{string.Join(";", kvp.Value)}"); //\t Painted circles: {kvp.Value.Count}");
                Console.WriteLine($"Length: {kvp.Value.Count}");
            }

        }




        //Option 1 (the circle with largest index inserted)
        static void PaintCircleMax(string[] circleList, IDictionary<int, List<string>> workerLists, List<int> lastIndexes)
        {
            var thread = Thread.CurrentThread;

            //itarating the dictionary for current worker
            var id = thread.ManagedThreadId;

            lock (locker) {
                workerLists.Add(id, new List<string>());
            }

            for (var i = 0; i < circleList.Length; i++)
            {
                
                //access to circle with index i
                lock (locker)
                {
                    if (circlesAmount <= 0)
                    {
                        break;
                    }
                    //to paint 0 index circle
                    if(lastIndexes.Last() == -1)
                    {
                        workerLists[id].Add(circleList[i]);
                    }
                    var max = lastIndexes.Last();
                    Thread.Sleep(paintingTime);
                    i = max = max+1;
                    workerLists[id].Add(circleList[i]);
                   // Console.WriteLine($"Circle {i} for {id} Painting....\n");
                    
                    lastIndexes.Add(max);
                    

                    circlesAmount--;

                }
            }
        }



        //Option 2 (ask do list of values contains current coordinates)
        static void PaintCircleAsk(string[] circleList, IDictionary<int, List<string>> workerLists)
        {
            var thread = Thread.CurrentThread;
            var contain = false;

            //itarating the dictionary for current worker
            var id = thread.ManagedThreadId;

            lock (locker)
            {
                workerLists.Add(id, new List<string>());
            }

            for (var i = 0; i < circleList.Length; i++)
            {

                //access to circle with index i
                lock (locker)
                {
                    if (circlesAmount <= 0)
                    {
                        break;
                    }

                   // Console.WriteLine($"Circle {i}: {circleList[i]} for {id}");
                    foreach (var kvp in workerLists)
                    {
                        if (kvp.Value.Contains(circleList[i]))
                        {
                            contain = true;
                           // Console.WriteLine("=>Already exists.\n");
                            break;

                        }
                    }
                    if (contain == false)
                    {
                        Thread.Sleep(paintingTime);
                        workerLists[id].Add(circleList[i]);
                       // Console.WriteLine("Painting....\n");
                    }
                    else
                    {
                        contain = false;
                        continue;
                    }

                    circlesAmount--;

                }
            }
        }
    }
}