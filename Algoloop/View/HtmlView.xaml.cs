﻿/*
 * Copyright 2019 Capnode AB
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

using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Algoloop.View
{
    /// <summary>
    /// Interaction logic for HtmlBox.xaml
    /// </summary>
    public partial class HtmlView : UserControl
    {
        public static readonly DependencyProperty HtmlTextProperty = DependencyProperty.Register("HtmlText", typeof(string), typeof(HtmlView));

        public HtmlView()
        {
            InitializeComponent();
            browser.Loaded += OnLoaded;
        }

        public string HtmlText
        {
            get { return (string)GetValue(HtmlTextProperty); }
            set { SetValue(HtmlTextProperty, value); }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == HtmlTextProperty)
            {
                DoBrowse();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is WebBrowser wb)
            {
                // Silent
                FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField(
                    "_axIWebBrowser2",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (fiComWebBrowser == null) return;
                object objComWebBrowser = fiComWebBrowser.GetValue(wb);
                if (objComWebBrowser == null) return;
                objComWebBrowser.GetType().InvokeMember(
                    "Silent",
                    BindingFlags.SetProperty,
                    null,
                    objComWebBrowser,
                    new object[] { true },
                    CultureInfo.InvariantCulture);
            }
        }

        private void DoBrowse()
        {
            if (!string.IsNullOrEmpty(HtmlText))
            {
                browser.NavigateToString(HtmlText);
            }
        }
    }
}