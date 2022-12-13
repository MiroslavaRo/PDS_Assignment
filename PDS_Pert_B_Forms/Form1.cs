using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;

namespace PDS_Part_B_Forms
{
    public partial class Form1 : Form
    {
        public int radius = 20;
        public static int amount = 10_00;
        public static int circlesAmount = 1000; //the number of the circles
        public static int paintingTime = 20; //20 msec
        static readonly object locker = new object();
        public static int[] workers = { 1, 5, 20, 100 }; //number of workers
        /// </summary>
        //public static int k = 5;
        public Random random = new Random();

        public Form1()
        {
            InitializeComponent();
        }

        static List<int[]> GenerateCoordinates()
        {
            var coordinates = new List<int[]>();
            Random random = new Random();
            var hashcoo = new HashSet<int[]>();

            while (hashcoo.Count < amount)
            {
                var x = random.Next(0, 900);
                var y = random.Next(0, 900);
                var coord = new int[] { x, y };
                hashcoo.Add(coord);
            }
            coordinates = hashcoo.ToList();

            return coordinates;
        }

        
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            var circleList = GenerateCoordinates();
            var temp = circlesAmount;

            var stopwatch = new Stopwatch();

            for (var i = 0; i < workers.Length; i++)
            {
                 e.Graphics.Clear(Color.White);
                 var k = workers[i];
                 MessageBox.Show($"START \n Circles to paint: {temp}, number of workers: {k}");
                 circlesAmount = temp;
                 stopwatch.Restart();
                 var messages = ThreadsWork02(circleList, k, e);
                 stopwatch.Stop();
                if (k > 20)
                {
                    MessageBox.Show($"FINISH\n\n Time to impliment with {k} workers: {stopwatch.ElapsedMilliseconds} miliseconds");
                }
                else
                {

                    MessageBox.Show($"FINISH\n\n{string.Join('\n', messages)}\n\nTime to impliment with {k} workers: {stopwatch.ElapsedMilliseconds} miliseconds");

                }

            }       
        }


        public List<string> ThreadsWork02(List<int[]> circleList, int k, PaintEventArgs e)
        {

            IDictionary<int, List<int[]>> workerLists = new Dictionary<int, List<int[]>>();

            //start threads
            var threads = new List<Thread>();

            for (int i = 0; i < k; i++)
            {
                var thread = new Thread(() => PaintCircleAsk(circleList, workerLists, e));
                thread.Start();
                threads.Add(thread);
            }

            //stop threads
            foreach (var thread in threads)
            {
                thread.Join();
            }

            var messages = new List<string>();

            foreach (var kvp in workerLists)
            {
                var msg = $"Worker id {kvp.Key}: \t Painted circles: {kvp.Value.Count}";
                messages.Add(msg);
            }
            return messages;

        }
        public void PaintCircleAsk(List<int[]> circleList, IDictionary<int, List<int[]>> workerLists, PaintEventArgs e)
        {
            var thread = Thread.CurrentThread;
            var contain = false;

            //itarating the dictionary for current worker
            var id = thread.ManagedThreadId;
            Color color;

            lock (locker)
            {
                workerLists.Add(id, new List<int[]>());
                color = Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));

            }

            for (var i = 0; i < circleList.Count; i++)
            {

                //access to circle with index i
                lock (locker)
                {
                    if (circlesAmount <= 0)
                    {
                        break;
                    }
                    foreach (var kvp in workerLists)
                    {
                        if (kvp.Value.Contains(circleList[i]))
                        {
                            contain = true;
                            break;

                        }
                    }
                    if (contain == false)
                    {
                        Graphics g = e.Graphics;

                        var x = circleList[i][0];
                        var y = circleList[i][1];



                        var diamtr = radius * 2;
                        var drawCircle = new Rectangle(x, y, diamtr, diamtr);
                        var pen = new Pen(color);
                        pen.Width = 4;

                        g.DrawEllipse(pen, drawCircle);


                        Thread.Sleep(paintingTime);
                        workerLists[id].Add(circleList[i]);
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
