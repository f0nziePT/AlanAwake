using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MouseMoverGUI
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInfo);

        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;

        [DllImport("kernel32.dll")]
        static extern uint SetThreadExecutionState(uint esFlags);

        const uint ES_CONTINUOUS = 0x80000000;
        const uint ES_SYSTEM_REQUIRED = 0x00000001;
        const uint ES_DISPLAY_REQUIRED = 0x00000002;

        private void PreventSleep()
        {
            SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);
        }

        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        enum Direction
        {
            Right,
            Down,
            Left,
            Up
        }

        private void PerformLeftClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);  // Left mouse button down
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);    // Left mouse button up
        }

        CancellationTokenSource cts;
        bool isRunning = false;

        System.Diagnostics.Stopwatch stopwatch;
        System.Windows.Threading.DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            stopwatch = new System.Diagnostics.Stopwatch();
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            lblElapsedTime.Content = "Elapsed Time: " + stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
        }

        private async void BtnStartStop_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning)
            {
                cts.Cancel();
                btnStartStop.Content = "Start";
                isRunning = false;
                stopwatch.Stop(); // Stop the stopwatch
                timer.Stop();     // Stop the timer
            }
            else
            {
                PreventSleep();  // Add this line
                btnStartStop.Content = "Stop";
                isRunning = true;
                cts = new CancellationTokenSource();
                stopwatch.Reset();  // Reset the stopwatch
                stopwatch.Start();  // Start the stopwatch
                timer.Start();      // Start the timer
                await MoveMouseAsync(cts.Token);
            }
        }


        async Task MoveMouseAsync(CancellationToken token)
        {
            Direction currentDirection = Direction.Right;

            await Task.Delay(5000, token); // Initial delay of 5 seconds.

            while (!token.IsCancellationRequested)
            {
                switch (currentDirection)
                {
                    case Direction.Right:
                        MoveMouse(50, 0);
                        currentDirection = Direction.Down;
                        break;
                    case Direction.Down:
                        MoveMouse(0, 50);
                        currentDirection = Direction.Left;
                        break;
                    case Direction.Left:
                        MoveMouse(-50, 0);
                        currentDirection = Direction.Up;
                        break;
                    case Direction.Up:
                        MoveMouse(0, -50);
                        currentDirection = Direction.Right;
                        break;
                }

                await Task.Delay(30000, token); // Wait for 30 seconds.
            }
        }


        void MoveMouse(int dx, int dy)
        {
            POINT currentPos;
            GetCursorPos(out currentPos);
            SetCursorPos(currentPos.X + dx, currentPos.Y + dy);

            PerformLeftClick();
        }
    }
}
