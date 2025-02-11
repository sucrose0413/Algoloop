/*
 * Copyright 2018 Capnode AB
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Algoloop.Support;
using Algoloop.Wpf.Properties;
using Algoloop.Wpf.ViewSupport;
using QuantConnect.Configuration;
using StockSharp.Xaml;
using System;
using System.Diagnostics;
using System.Windows;

namespace Algoloop.Wpf.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            string exeFolder = MainService.GetProgramFolder();
            Config.Set("plugin-directory", exeFolder);
            Config.Set("composer-dll-directory", exeFolder);

            InitializeComponent();
            ThemeExtensions.DefaultTheme = ThemeExtensions.DefaultLightTheme;
            ThemeExtensions.ApplyDefaultTheme();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.MainWindowPlacement = this.GetPlacement();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.SetPlacement(Settings.Default.MainWindowPlacement);
        }

        private void HelpAbout(object sender, RoutedEventArgs e)
        {
            var about = new AboutView();
            about.ShowDialog();
        }

        private void HelpDocumentation(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://github.com/Capnode/Algoloop/wiki/Documentation");
        }

        private void HelpTechnicalSupport(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://github.com/Capnode/Algoloop/issues");
        }

        private void HelpPrivacyPolicy(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://github.com/Capnode/Algoloop/wiki/Privacy-policy");
        }

        private void HelpSubscriptions(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://account.microsoft.com/services");
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Exception ex)
            {
                // check this: https://github.com/dotnet/corefx/issues/10361
                Debug.WriteLine($"{ex.GetType()}: {ex.Message}");
            }
        }
    }
}
