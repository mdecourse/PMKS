﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace PMKS_Silverlight_App
{
    public partial class OutputStatus : UserControl
    {
        public OutputStatus()
        {
            InitializeComponent();
        }

        private void OutputStatus_Loaded_1(object sender, RoutedEventArgs e)
        {
            PMKSControl.StatusBox = StatusBox;
        }

    }

}