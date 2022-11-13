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
        public static int amount = 1000;
        public static int[] nthreads = {0, 2, 3, 4, 6}; //number of threads
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
            var sum = SearchedTypes.Values.Sum();
            var array = new string[amount- sum];
            array = TypesAndNumber(array.ToList()); //add SearchedTypes to array
            Console.WriteLine("Length: " + array.Length);

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
                    array[i] = GenerateBarcode();
                }
                if (array[i] == null)
                {
                    array[i] = GenerateBarcode();
                }
            }

            //find elements for each type
            var elements = FindBarcodes(array);
            foreach (KeyValuePair<int, List<string>> element in elements)
            {
                Console.WriteLine($"type: {element.Key} | elements: {string.Join(", ", element.Value)}");
              //  Console.WriteLine(string.Join(", ", element.Value.ToArray()));
            }
        }

        static string GenerateBarcode()
        {
            Random random = new Random();
            var barcode = $"{random.Next(0, 99999).ToString("00000")}{random.Next(1, 100).ToString("000")}";
            return barcode;
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


        static IDictionary<int, List<string>> FindBarcodes(string[] array)
        {
            IDictionary<int, List<string>> Typeselements = new Dictionary<int, List<string>>();
            var count = 0;

            foreach (KeyValuePair<int, int> value in SearchedTypes)
            {
                count = 0;
                List<string> elements = new List<string>();
                for (var i = 0; i < array.Length; i++)
                {
                    var type = value.Key.ToString("000");
                    var barcode = array[i].Substring(5, 3);
                    if (barcode == type&& count< value.Value)
                    {
                        elements.Add(array[i]);
                       // Console.WriteLine($"barcode: {barcode}, index: {i}, {array[i]}");
                        count++;

                    }
                }
                Typeselements.Add(value.Key, elements);
              //  Console.WriteLine($"type: {value.Key}, elements: {string.Join(", ", elements)}");
            }
            return Typeselements;

        }
    }
}
