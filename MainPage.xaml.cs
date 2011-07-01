using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Devices.Sensors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace LaundryMinder
{
    public partial class MainPage : PhoneApplicationPage
    {
        AccelerometerShakeDetection asdSensor;
        DateTime lastShakeTime = DateTime.UtcNow;

        DispatcherTimer dt;

        public MainPage()
        {
            InitializeComponent();

            asdSensor = new AccelerometerShakeDetection();

            try
            {
                asdSensor.Start();
            }
            catch
            {
                MessageBox.Show("Your accelerometer could not be started. The application cannot run without this sensor.", "Error", MessageBoxButton.OK);
                NavigationService.GoBack();
            }

            this.LoadSettings();
        }

        public void AccelerometerShakeDetected(object sender, EventArgs e)
        {
            lastShakeTime = DateTime.UtcNow;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (this.ValidateInput() == false)
            {
                MessageBox.Show("Please enter a valid phone number or email address.", "Error", MessageBoxButton.OK);
                return;
            }

            if (this.button1.Content.ToString() == "start")
            {
                dt = new System.Windows.Threading.DispatcherTimer();
                dt.Interval = new TimeSpan(0, 0, 30);
                dt.Tick += new EventHandler(dt_Tick);
                dt.Start();

                this.PageTitle.Text = "running";
                this.radioButton1.IsEnabled = false;
                this.radioButton2.IsEnabled = false;
                this.radioButton3.IsEnabled = false;
                this.textBox1.IsEnabled = false;
                this.button2.IsEnabled = false;
                this.button1.Content = "stop";
            }
            else
            {
                dt.Stop();

                this.PageTitle.Text = "ready";
                this.radioButton1.IsEnabled = true;
                this.radioButton2.IsEnabled = true;
                this.radioButton3.IsEnabled = true;
                this.textBox1.IsEnabled = true;
                this.button2.IsEnabled = true;
                this.button1.Content = "start";
            }

            this.SaveSettings();
        }

        private void dt_Tick(object sender, EventArgs e)
        {
            if ((DateTime.UtcNow - lastShakeTime).Minutes > 2)
            {
                dt.Stop();

                // send a text message
                WebClient wc = new WebClient();
                if (this.radioButton1.IsChecked == true)
                {
                    wc.DownloadStringAsync(new Uri("http://www.mccormicktechnologies.com/labs/laundryminder/send.php?type=1&to=" + this.textBox1.Text));
                }
                else if (this.radioButton2.IsChecked == true)
                {
                    wc.DownloadStringAsync(new Uri("http://www.mccormicktechnologies.com/labs/laundryminder/send.php?type=2&to=" + this.textBox1.Text));
                }
                else if (this.radioButton3.IsChecked == true)
                {
                    wc.DownloadStringAsync(new Uri("http://www.mccormicktechnologies.com/labs/laundryminder/send.php?type=3&to=" + this.textBox1.Text));
                }

                // play the sound
                var stream = TitleContainer.OpenStream("buzzer.wav");
                SoundEffect effect = SoundEffect.FromStream(stream);
                FrameworkDispatcher.Update();
                effect.Play();

                this.PageTitle.Text = "done";
                this.button1.Content = "continue";
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/InstructionsPage.xaml", UriKind.Relative));
        }

        private bool ValidateInput()
        {
            if (this.textBox1.Text.Length > 0)
            {
                if (this.radioButton1.IsChecked == true ||
                    this.radioButton2.IsChecked == true)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(this.textBox1.Text, @"^[2-9]{2}[0-9]{8}$"))
                        return true;
                    else
                        return false;
                }
                else
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(this.textBox1.Text, @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$"))
                        return true;
                    else
                        return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void LoadSettings()
        {
            try
            {
                if (IsolatedStorageSettings.ApplicationSettings["NotificationType"].ToString() == "1")
                {
                    this.radioButton1.IsChecked = true;
                }
                else if (IsolatedStorageSettings.ApplicationSettings["NotificationType"].ToString() == "2")
                {
                    this.radioButton2.IsChecked = true;
                }
                else if (IsolatedStorageSettings.ApplicationSettings["NotificationType"].ToString() == "3")
                {
                    this.radioButton3.IsChecked = true;
                }

                this.textBox1.Text = IsolatedStorageSettings.ApplicationSettings["NotificationLocation"].ToString();
            }
            catch
            {
            }
        }

        private void SaveSettings()
        {
            if (this.radioButton1.IsChecked == true)
            {
                IsolatedStorageSettings.ApplicationSettings["NotificationType"] = "1";
            }
            else if (this.radioButton2.IsChecked == true)
            {
                IsolatedStorageSettings.ApplicationSettings["NotificationType"] = "2";
            }
            else if (this.radioButton3.IsChecked == true)
            {
                IsolatedStorageSettings.ApplicationSettings["NotificationType"] = "3";
            }

            IsolatedStorageSettings.ApplicationSettings["NotificationLocation"] = this.textBox1.Text;
        }

        private void radioButton1_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                InputScope Keyboard = new InputScope();
                InputScopeName ScopeName = new InputScopeName();
                ScopeName.NameValue = InputScopeNameValue.TelephoneNumber;
                Keyboard.Names.Add(ScopeName);
                this.textBox1.InputScope = Keyboard;
            }
            catch
            {
            }
        }

        private void radioButton2_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                InputScope Keyboard = new InputScope();
                InputScopeName ScopeName = new InputScopeName();
                ScopeName.NameValue = InputScopeNameValue.TelephoneNumber;
                Keyboard.Names.Add(ScopeName);
                this.textBox1.InputScope = Keyboard;
            }
            catch
            {
            }
        }

        private void radioButton3_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                InputScope Keyboard = new InputScope();
                InputScopeName ScopeName = new InputScopeName();
                ScopeName.NameValue = InputScopeNameValue.EmailSmtpAddress;
                Keyboard.Names.Add(ScopeName);
                this.textBox1.InputScope = Keyboard;
            }
            catch
            {
            }
        }
    }
}