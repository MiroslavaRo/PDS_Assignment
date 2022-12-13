using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Xml.Linq;

namespace PDS_Part_A
{
    internal class Program
    {
        public static int amount = 10_000;
        static readonly object locker = new object();
        public static int[] nthreads = {1, 2, 3, 4, 6}; //number of threads
        public static IDictionary<int, int> SearchedTypesIndexes = new Dictionary<int, int>(){
            {1,30}, // 30 items of value 1
            {7,15}, //15 items of value 7
            {8,10} //8 items of value 10
            /*
            {14,100},
            {15,400},
            {18,200},
            {22,704},
            {23,900},
            {34,500},
            {35,700},
            {36,1000},
            {40,180},
            {54,200},
            {58,1709},
            {67,3090},
            {79,500},
            {82,3700},
            {84,100},
            {86,1400},
            {89,3020},
            {90,1704},
            {91,900},
            {93,5000},
            {95,700},
            {96,1000},
            {97,1800},
            {98,2000},
            {99,1079}*/
        };

        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();

            
            Console.WriteLine("TASK 01");
            
            //generate random array
            int[] array = new int[amount];
            for (var i = 0; i < array.Length; i++)
            {
                Random random = new Random();
                array[i] = random.Next(1, amount);
            }
            /*
            foreach (var n in nthreads)
            {
                stopwatch.Reset();
                stopwatch.Start();
                Task01(n, array);
                stopwatch.Stop();
                Console.WriteLine($"Time to process with {n} threads: {stopwatch.ElapsedMilliseconds} miliseconds\n"); 
                Console.WriteLine("---------------------------------------");
            }*/
            
            Console.WriteLine("---------------------------------------");

            Console.WriteLine("TASK 02");

            //generate list of barcodes
            var list = GenerateBarcodes();

            foreach (var n in nthreads)
             {
                stopwatch.Reset();
                stopwatch.Start();
                Task02(n, list);
                stopwatch.Stop();
                Console.Write($"Time to process with {n} threads: ");
                Console.Write($"{stopwatch.ElapsedMilliseconds} miliseconds\n");

            }

            Console.WriteLine($"FINISHED");
            Console.Read();
        }

        #region task01
        static void Task01(int numOfthreads, int[] array)
        {
            int[] sorted = new int[amount];

           // if (numOfthreads > 1)
          //  {
                //devide array to smaller parts by [threads], sort each of them and then merge
              //  sorted = ThreadSorting(array.ToList(), numOfthreads);
/*
            }
            else
            {
                //sort without threads
                sorted = BubbleSortArray(array);

            }*/
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
        static List<int>[] GenerateSortingSubLists(List<int> array, int numOfthreads) 
        {             
            var maxNum = array.Max();
            List<int>[] subLists = new List<int>[numOfthreads];
            var splitFactor = maxNum / numOfthreads;

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
        static int[] ThreadSorting(List<int> array, int numOfthreads)
        {
            var stopwatch1 = new Stopwatch();
            stopwatch1.Start();
            //make sublists
            var subLists = GenerateSortingSubLists(array, numOfthreads);

            stopwatch1.Stop();
            Console.WriteLine($"BEFORE THREADS: {stopwatch1.ElapsedMilliseconds} miliseconds");
            stopwatch1.Restart();


            //start and after stop threads
            ThreadsWorkBubble(numOfthreads,subLists);

            stopwatch1.Stop();
            Console.WriteLine($"AFTER FINISH THREADS: {stopwatch1.ElapsedMilliseconds} miliseconds");

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
        #endregion

        /*------------------------------------------------------------*/
        #region task02
        
        static void Task02(int numOfthreads, List<string> list)
        {
            var stopwatch1 = new Stopwatch();
            stopwatch1.Start();

            //make sublists for threads searching
            var subLists = GenerateBarcodesSubLists(list, numOfthreads);


            //create a dictionary with elements of specific type
            //(types as keys from SearchedTypesIndexes)
            IDictionary<int, List<string>> typesElements = new Dictionary<int, List<string>>();

            //create seperated lists for each key as a common buffer for all threads
            List<string>[] elements = new List<string>[SearchedTypesIndexes.Count];
            var y = 0;
            foreach (KeyValuePair<int, int> value in SearchedTypesIndexes)
            {
                elements[y] = new List<string>();
                typesElements.Add(value.Key, elements[y]);
                y++;
            }

            stopwatch1.Stop();
            Console.WriteLine($"BEFORE THREADS: {stopwatch1.ElapsedMilliseconds} miliseconds");
            stopwatch1.Reset();
            stopwatch1.Start();

            //search barcodes with threads
            ThreadsWorkBarcodes(numOfthreads, subLists, typesElements);


            //display dictionary with lists of elements for each type
            foreach (KeyValuePair<int, List<string>> element in typesElements)
            {
                //  Console.WriteLine($"type-amount: {element.Key} | elements: {string.Join(", ", element.Value)} | length: {element.Value.Count}");
            }
            stopwatch1.Stop();
            Console.WriteLine($"AFTER FINISH THREADS: {stopwatch1.ElapsedMilliseconds} miliseconds");
        }
        static List<string> GenerateBarcodes()
        {
            var length = amount;
            var array = new string[length];

            //add Searched elements to array to make sure
            //there are at least needed amount of them
            var list = new List<string>();
            foreach (KeyValuePair<int, int> value in SearchedTypesIndexes)
            {
                for (var i = 0; i < value.Value; i++)
                {
                    Random random = new Random();
                    var barcode = $"{random.Next(0, 99999).ToString("00000")}{value.Key.ToString("000")}";
                    list.Add(barcode);
                }
            }
            for (var y = 0; y < list.Count; y++)
            {
                array[y] = list[y];
            }


            //shuffle generated barcodes and fulfill remain cells in array
            for (var i = 0; i < length; i++)
            {
                var temp = "";
                Random random = new Random();
                var y = i + random.Next(0, length - i);
                temp = array[y];
                array[y] = array[i];
                array[i] = temp;
                if (array[i] == null)
                {
                    array[i] = $"{random.Next(0, 99999).ToString("00000")}{random.Next(1, 100).ToString("000")}"; ;
                }
            }
            return array.ToList();
        }
        static List<string>[] GenerateBarcodesSubLists(List<string> origList, int numOfthreads)
        {
            var list = new List<string>(origList);
            List<string>[] sublists = new List<string>[numOfthreads];
            var splitFactor = Math.Ceiling(Convert.ToDouble(list.Count) / Convert.ToDouble(numOfthreads));
            for (int i = 0; i < numOfthreads; i++)
                sublists[i] = new List<string>();

            //add elements until the length of sublist is equal to splitFactor

            for (int i = 0; i < list.Count; i++)
            {
                int j = Convert.ToInt32(Math.Floor(Convert.ToDouble(i) / Convert.ToDouble(splitFactor)));
                var value = list[i];
                sublists[j].Add(value);
            }

            var u = 1;
            foreach (var a in sublists)
            {
                //Console.WriteLine(string.Join(", ", a));
                Console.WriteLine($"Length of sublist {u}: {a.Count}");
                u++;
            }

            return sublists;
        }
        static void ThreadsWorkBarcodes(int numOfthreads, List<string>[] subLists, IDictionary<int, List<string>> Typeselements)
        {
            //start threads
            var threads = new List<Thread>();

            //common buffer for all threads
            List<string>[] elements = new List<string>[SearchedTypesIndexes.Count];
            for (var i = 0; i < SearchedTypesIndexes.Count; i++)
            {
                elements[i] = new List<string>();
            }

            for (int i = 0; i < numOfthreads; i++)
            {
                var list = subLists[i];
                var thread = new Thread(() => FindBarcodes(list, Typeselements, elements, i));
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
        static IDictionary<int, List<string>> FindBarcodes(List<string> list, IDictionary<int, List<string>> typesElements, List<string>[] elements, int Index)
        {
            var j = 0;
            foreach (KeyValuePair<int, int> value in SearchedTypesIndexes)
            {
                //List<string> elements = new List<string>();
                for (var i = 0; i < list.Count; i++)
                {
                    var type = value.Key.ToString("000");
                    var barcode = list[i].Substring(5, 3);

                    if (elements[j].Count < value.Value)
                    {
                        if (barcode == type)
                        {
                            // Console.WriteLine($"Length: {elements.Count}");
                            elements[j].Add(list[i]);
                            //typesElements[typesElements].Add();
                            Console.WriteLine($"thread({Index}) element: {list[i]} | index: {i} ");
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                // typesElements.Add(value.Key, elements);
                typesElements[value.Key] = elements[j];
                j++;
            }

            return typesElements;

        }
        

        /*
        static List<string>[] GenerateBarcodesSubLists(List<string> origList, int numOfthreads)
        {
            var list = new List<string>(origList);
            List<string>[] sublists = new List<string>[numOfthreads];
            var splitFactor = Math.Ceiling(Convert.ToDouble(list.Count) / Convert.ToDouble(numOfthreads));
            for (int i = 0; i < numOfthreads; i++)
            {
                sublists[i] = new List<string>();
            }

            //add elements till the quotient from division (index of element/splitfactor)
            //rounded down is less than index of sub-list
            for (int i = 0; i < list.Count; i++)
            {
                //the quotient rounded down shows to which sub-list element belongs
                int j = Convert.ToInt32(Math.Floor(Convert.ToDouble(i) / Convert.ToDouble(splitFactor)));
                var value = list[i];
                sublists[j].Add(value);
            }

            return sublists;
        }


        static void ThreadsWorkBarcodes(int numOfthreads, List<string>[] subLists, IDictionary<int, List<string>> Typeselements)
        {

            //start threads
            var threads = new List<Thread>();
            for (int i = 0; i < numOfthreads; i++)
            {
                var list = subLists[i];
                var thread = new Thread(() => FindBarcodes(list, Typeselements,i));
                thread.Start();
                threads.Add(thread);
            }

            //stop threads
            foreach (var thread in threads)
            {
                thread.Join();
            }

        }
        static IDictionary<int, List<string>> FindBarcodes(List<string> list, IDictionary<int, List<string>> typesElements, int threadindex)
        {
            foreach (KeyValuePair<int, int> value in SearchedTypesIndexes)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    var type = value.Key.ToString("000");
                    var barcode = list[i].Substring(5, 3);
                    if (barcode == type) {
                        lock (locker)
                        {
                            if (typesElements[value.Key].Count < value.Value)
                            {
                                typesElements[value.Key].Add(list[i]);
                                //Console.WriteLine($"thread({threadindex}) element: {list[i]} | index: {i} ");

                            }
                            else
                            {
                                break;
                            }

                        }

                    }
                    
                }
            }

            return typesElements;

        }*/

        #endregion
    }

}
