using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace PDS_Part_A
{
    internal class Program
    {
        public static int amount = 100_000;
        public static int[] nthreads = {0, 2, 3, 4, 6}; //number of threads

        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();

          /*  Console.WriteLine("TASK 1");
            foreach (var n in nthreads)
            {
                Console.Write($"Time to process with {n} threads: ");
                stopwatch.Reset();
                stopwatch.Start();
                Task01(n);
                stopwatch.Stop();
                Console.Write($"{stopwatch.ElapsedMilliseconds} miliseconds\n");

            }*/

            //---------------------------------------
            Console.WriteLine("TASK 2");
           // foreach (var n in nthreads)
           // {
            //    Console.Write($"Time to process with {} threads: ");
                stopwatch.Reset();
                stopwatch.Start();
                Task02();
                stopwatch.Stop();
                Console.Write($"{stopwatch.ElapsedMilliseconds} miliseconds\n");

           // }

            Console.WriteLine($"FINISHED");
            Console.Read();
        }


        static void Task01(int numOfthreads)
        {
            //generate random array
            int[] array = new int[amount];
            for (var i = 0; i < array.Length; i++)
            {
                Random random = new Random();
                array[i] = random.Next(1, amount);
            }

            int[] sorted = new int[amount];
            if (numOfthreads > 1)
            {
                //devide array to smaller parts by [threads], sort each of them and then merge
                sorted = ParallelSorting(array.ToList(), numOfthreads);

            }
            else
            {
                //sort without threads
                sorted = BubbleSortArray(array);

            }
        }

        static void BubbleSortList(List<int> array)
        {
            int temp;

            for (var y = 0; y < array.Count - 1; y++)
            {
                for (var i = 0; i < array.Count - 1; i++)
                {
                    if (array[i] > array[i + 1])
                    {
                        temp = array[i + 1];
                        array[i + 1] = array[i];
                        array[i] = temp;
                    }
                }
            }
        }

        static int[] BubbleSortArray(int[] array)
        {
            int temp;

            for (var y = 0; y < array.Length - 1; y++)
            {
                for (var i = 0; i < array.Length - 1; i++)
                {
                    if (array[i] > array[i + 1])
                    {
                        temp = array[i + 1];
                        array[i + 1] = array[i];
                        array[i] = temp;
                    }
                }
            }
            return array;
        }

        static List<int>[] GenerateSubList(List<int> array, int numOfthreads) 
        {             
            var maxNum = array.Max();
            List<int>[] subLists = new List<int>[numOfthreads];
            var splitFactor = maxNum / numOfthreads;
//            Console.WriteLine($"max number: {maxNum}\nsplit: {splitFactor}");

            for (var j = 0; j < numOfthreads - 1; j++)
            {
                subLists[j] = new List<int>();

                for (int i = 0; i < array.Count; i++)
                {
                    //choose all values that are less then split factor for current sublist
                    if (array[i] <= splitFactor * (j + 1))
                    {
                        var value = array[i];
                        subLists[j].Add(value);
                    }
                }
                //remove current value from array to avoid dublication                        
                array.RemoveAll(a => subLists[j].Contains(a));
            }
            //add remaining values to the last sublist
            subLists[subLists.Length - 1] = array;
            return subLists;
        }

        static void ThreadsWork(int numOfthreads, List<int>[] subLists)
        {
            //start threads
            var threads = new List<Thread>();

            for (int i = 0; i < numOfthreads; i++)
            {
                var list = subLists[i];
                var thread = new Thread(() => BubbleSortList(list));
                thread.Start();
                threads.Add(thread);
            }

            //stop threads
            foreach (var thread in threads)
            {
                thread.Join();
            }
        }

        static int[] ParallelSorting(List<int> array, int numOfthreads)
        {

            //make sublists
            var subLists = GenerateSubList(array, numOfthreads);

            //start and after stop threads
            ThreadsWork(numOfthreads,subLists);

            //merge all sublists to sorted list
            var sortedList = new List<int>();
            foreach (var subList in subLists)
            {
                foreach (var list in subList)
                {
                    sortedList.Add(list);
                }
            }


            return sortedList.ToArray();
        }



        static void Task02()
        {
            //generate random array
            List<string> array = new List<string>();
            TypesAndNumber(array);

            Random random = new Random();
            var barcode = $"{random.Next(0, 99999).ToString("00000")}-{random.Next(1, 100).ToString("000")}";


            for (var i = array.Count; i < (amount - array.Count); i++)
            {
                var temp = 0;
                var y = random.Next(0, amount - 1);
               // temp = array[i];
               //array[i] = array[y];
               //array[y] = temp;
            }

        }
        static string GenerateBarcode()
        {
            Random random = new Random();
            var barcode = $"{random.Next(0, 99999).ToString("00000")}-{random.Next(1, 100).ToString("000")}";
            return barcode;
        }

        static List<string> TypesAndNumber(List<string> array)
        {
            IDictionary<int, int> SearchedTypes = new Dictionary<int, int>();
            SearchedTypes.Add(1, 30); // 30 items of type 1
            SearchedTypes.Add(7, 15); //15 items of type 7
            SearchedTypes.Add(8, 10); //8 items of type 10


            foreach (KeyValuePair<int, int> type in SearchedTypes)
            {
                for (var i = 0; i < type.Value; i++)
                {
                    Random random = new Random();
                    var barcode = $"{random.Next(0, 99999).ToString("00000")}-{type.Key.ToString("000")}";
                    array.Add(barcode);
                    Console.WriteLine(barcode);
                }
            }
            return array;
        }
    }
}
