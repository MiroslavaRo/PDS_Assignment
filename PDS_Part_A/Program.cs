using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Xml.Linq;

namespace PDS_Part_A
{
    internal class Program
    {
        public static int amount = 1_000;
        static readonly object locker = new object();
        public static int[] nthreads = {1, 2, 3, 4, 6}; //number of threads
        public static IDictionary<int, int> SearchedTypes = new Dictionary<int, int>(){
            {1,30}, // 30 items of value 1
            {7,15}, //15 items of value 7
            {8,10} //8 items of value 10
        };

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
            // Console.Write($"Time to process with {n} threads: ");
            stopwatch.Start();
            Task02(2);
            stopwatch.Stop();
           // Console.Write($"Time to process with {n} threads: ");
            Console.Write($"{stopwatch.ElapsedMilliseconds} miliseconds\n");
            stopwatch.Reset();
            /* foreach (var n in nthreads)
             {
                // Console.Write($"Time to process with {n} threads: ");
                 stopwatch.Start();
                 Task02(n);
                 stopwatch.Stop();
                 Console.Write($"Time to process with {n} threads: ");
                 Console.Write($"{stopwatch.ElapsedMilliseconds} miliseconds\n");
                 stopwatch.Reset();

             }*/

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
          //  Console.WriteLine($"max number: {maxNum}\nsplit: {splitFactor}");

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

        static void ThreadsWorkBubble(int numOfthreads, List<int>[] subLists)
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
            ThreadsWorkBubble(numOfthreads,subLists);

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

        /*------------------------------------------------------------*/

        static void Task02(int numOfthreads)
        {
            var sum = SearchedTypes.Values.Sum();
            var array = new string[amount- sum];
            array = TypesAndNumber(array.ToList()); //add SearchedTypes to array

            //shuffle generated barcodes and fulfill remain cells in array
            for (var i = 0; i < array.Length; i++)
            {
                var temp = "";
                Random random = new Random();
                var y = i + random.Next(0, array.Length - i);
                temp = array[y];
                array[y] = array[i];
                array[i] = temp;
                if (array[i] == null)
                {
                    array[i] = $"{random.Next(0, 99999).ToString("00000")}{random.Next(1, 100).ToString("000")}"; ;
                }
            }
            
            //make sublists
            var subLists = GenerateSubArrays(array.ToList(), numOfthreads);

            IDictionary<int, List<string>> Typeselements = new Dictionary<int, List<string>>();

            //start and after stop threads
            ThreadsWorkBarcodes(numOfthreads, subLists, Typeselements);

           /* for (int i = 0; i < numOfthreads; i++)
            {
                var list = subLists[i].ToArray();
                var thread = new Thread(() => FindBarcodes(list, Typeselements));
                thread.Start();
            }*/

            // FindBarcodes(array, Typeselements);

            foreach (KeyValuePair<int, List<string>> element in Typeselements)
            {
                Console.WriteLine($"type: {element.Key} | elements: {string.Join(", ", element.Value)} | length: {element.Value.Count}");
            }
        }


        static string [] TypesAndNumber(List<string> array)
        {
            foreach (KeyValuePair<int, int> value in SearchedTypes)
            {
                for (var i = 0; i < value.Value; i++)
                {
                    Random random = new Random();
                    var barcode = $"{random.Next(0, 99999).ToString("00000")}{value.Key.ToString("000")}";
                    array.Add(barcode);
                }
            }
            return array.ToArray();
        }
        static void FindBarcodes(string[] array, IDictionary<int, List<string>> Typeselements)
        {

            foreach (KeyValuePair<int, int> value in SearchedTypes)
            {
                List<string> elements = new List<string>();
                for (var i = 0; i < array.Length; i++)
                {
                    var type = value.Key.ToString("000");
                    var barcode = array[i].Substring(5, 3);

                    if (elements.Count < value.Value)
                    {
                        lock (locker)
                        {
                            if (barcode == type)
                            {
                                elements.Add(array[i]);
                            }
                        }

                    }
                    else
                    {
                        break;
                    }


                }
                //TO DO (found enter for same key)
                Typeselements.Add(value.Key, elements);
            }
           // return Typeselements;

        }
        static List<string>[] GenerateSubArrays(List<string> array, int numOfthreads)
        {
            List<string>[] subarray = new List<string>[numOfthreads];
            var splitFactor = array.Count/ numOfthreads;

            for (var j = 0; j < numOfthreads - 1; j++)
            {
                subarray[j] = new List<string>();

                for (int i = 0; i < array.Count; i++)
                {
                    if (i < splitFactor * (j + 1))
                    {
                        var value = array[i];
                        subarray[j].Add(value);
                    }
                }
                array.RemoveAll(a => subarray[j].Contains(a));
            }
            //add remaining values to the last sublist
            subarray[subarray.Length - 1] = array;


            return subarray;
        }
        static void ThreadsWorkBarcodes(int numOfthreads, List<string>[] subLists, IDictionary<int, List<string>> Typeselements)
        {
            //start threads
            var threads = new List<Thread>();

            for (int i = 0; i < numOfthreads; i++)
            {
                var list = subLists[i].ToArray();
                var thread = new Thread(() => FindBarcodes(list, Typeselements));
                thread.Start();
                threads.Add(thread);
            }

            //stop threads
            foreach (var thread in threads)
            {
                thread.Join();
            }
           // return Typeselements;
        }


    }
}
