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
        public static int amount = 20_00;
        public static int circlesAmount = 1000; //the number of the circles
        public static int paintingTime = 20; //20 msec
        static readonly object locker = new object();
        //  public static int[] k = { 5, 20, 100 }; //number of wiorkers
        public static int k = 5;

        static void Main(string[] args)
        {
            SectionB();
            Console.WriteLine($"FINISH");
            Console.Read();
        }

        static void SectionB()
        {
            var circleList = GenerateCoordinates();

            var stopwatch = new Stopwatch();

            Console.WriteLine("START");
            stopwatch.Start();
            //Console.WriteLine($"Coordinates: \n{string.Join("; ", circleList)}");

            ThreadsWork(circleList);

            Console.WriteLine("---------------------------------------");
            stopwatch.Stop();
            Console.Write($"Time to impliment with {k} workers: ");
            Console.Write($"{stopwatch.ElapsedMilliseconds} miliseconds\n");
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
        static void ThreadsWork(string[] circleList)
        {
            var stopwatch1 = new Stopwatch();
            stopwatch1.Start();
            var n = circlesAmount;
            

            IDictionary<int, List<string>> workerLists = new Dictionary<int, List<string>>();
           

            //start threads
            var threads = new List<Thread>();

            for (int i = 0; i < k-1; i++)
            {
                var thread = new Thread(() => PaintCircle(circleList, workerLists));
                thread.Start();
                threads.Add(thread);
            }
            
            //stop threads
            foreach (var thread in threads)
            {
                thread.Join();
            }


            var sum = 0;
            Console.WriteLine("\n AMOUNT:" + circlesAmount);
            foreach (var kvp in workerLists)
            {
                Console.WriteLine($"\n---------------WorkerList {kvp.Key}--------------");
                Console.WriteLine(string.Join("\n", kvp.Value));
                Console.WriteLine($"Length: {kvp.Value.Count}");
                sum+= kvp.Value.Count;
            }
            Console.WriteLine($"\nFULL LENGTH: {sum}");

        }




        static void PaintCircle(string[] circleList, IDictionary<int, List<string>> workerLists)//List<string>[] workerLists)
        {
            var thread = Thread.CurrentThread;
            var contain = false;

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
                    Thread.Sleep(10);
                    if (circlesAmount <= 0)
                    {
                        break;
                    }
                    Console.WriteLine($"Circle {i}: {circleList[i]} for {id}");
                    foreach (var kvp in workerLists)
                    {
                        if (kvp.Value.Contains(circleList[i]))
                        {
                            contain = true;
                            Console.WriteLine("=>Already exists.\n");
                            break;

                        }
                    }
                    if (contain == false)
                    {
                        Thread.Sleep(200);
                        workerLists[id].Add(circleList[i]);
                        Console.WriteLine("Painting....\n");
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