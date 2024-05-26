using InTerm.Models;
using InTerm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace InTerm.Views
{
    public partial class TerminalSettingsWindow : Window
    {

        private readonly TerminalSettingsViewModel viewModel;

        public TerminalConnectionSettings Settings { get => viewModel.GetSettings(); }

        public TerminalSettingsWindow(TerminalConnectionSettings settings)
        {
            InitializeComponent();
            viewModel = new TerminalSettingsViewModel(this, settings);
            DataContext = viewModel;
        }
    }
}
