using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace BackgroundInvocationBug {

    sealed partial class App : Application {

        /*
         * Experiment for debugging the Microsoft.Windows.BrokerInfrastructure
         * on a mobile device.
         * 
         * 1. Deploy/run the app. 
         * 2. Stop it.
         * 3. Set to "Always allowed" under app battery use
         * 4. Copy included Sleep.xml to Documents\FieldMedic\CustomProfiles\Sleep.xml and start a trace on it in FieldMedic.
         * 5. Turn off mobile screen
         * 6. Check the log after 8 hrs
         * 
         * Should see:
         * 
            12/31 09:14:27: TimerTest registered
            12/31 09:38:49: TimerTest 37 mins elapsed
            12/31 09:55:47: TimerTest 16 mins elapsed
            12/31 10:02:50: TimerTest 7 mins elapsed
            12/31 10:25:46: TimerTest 22 mins elapsed
            12/31 10:40:46: TimerTest 14 mins elapsed
            12/31 13:33:15: TimerTest 172 mins elapsed <--- screen turned on.


         * ETL postmortem for the above run, have included Custom-Sleep.etl:
         * 
         * Filter results to "5e4ddee2" (from package id 5e4ddee2-4eef-4362-8107-881b602db5ca_1.0.0.0_arm__d3rsr107h1f3p)
         * - Microsoft.Windows.ProcessLifetimeManager last event is 10:40 AM
         * - Microsoft-Broker-Infrastructure has no "energy debt" outputs explaining why it stopped at 10:40 AM (ostensibly due to "Always allowed")
         * 
         */

        private static string _LogFileName = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.TemporaryFolder.Path, "_DebugLog.txt");
        private static object _Sync = new object();

        public App() {
            this.InitializeComponent();
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args) {
            
            ApplicationData.Current.LocalSettings.Values.TryGetValue("LastActivated", out object lastActivatedValue);
            DateTime.TryParse(lastActivatedValue as string ?? "", out var lastActivated);

            Log(args.TaskInstance.Task.Name + " " + (lastActivated == DateTime.MinValue ? "first invocation" : ((int)(DateTime.UtcNow - lastActivated).TotalMinutes).ToString() + " mins elapsed"));

            ApplicationData.Current.LocalSettings.Values["LastActivated"] = DateTime.UtcNow.ToString("s");
       
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e) {
            RegisterTask();
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null) {
                rootFrame = new Frame();
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false) {
                if (rootFrame.Content == null) {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                Window.Current.Activate();
            }
        }

        private static void Log(string msg) {
            lock (_Sync) {
                System.IO.File.AppendAllText(_LogFileName, DateTime.Now.ToString("MM/dd HH:mm:ss").ToLower() + ": " + msg + "\r\n");
            }
        }

        public static string[] LogRead() {
            lock (_Sync) {
                return System.IO.File.Exists(_LogFileName) ? System.IO.File.ReadAllLines(_LogFileName) : new string[0];
            }
        }

        private void RegisterTask() {
            if (BackgroundTaskRegistration.AllTasks.FirstOrDefault(x => x.Value.Name == "TimerTest").Value == null) {
                var builder = new BackgroundTaskBuilder() {
                    Name = "TimerTest"
                };
                builder.SetTrigger(new TimeTrigger(15, false));
                builder.Register();
                Log("TimerTest registered");
            }
        }
    }
}