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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InTerm.Views
{
    public partial class TerminalView : UserControl
    {

        private readonly TerminalViewModel viewModel;

        public TerminalView(TerminalSettings terminalSettings)
        {
            InitializeComponent();
            viewModel = new TerminalViewModel(terminalSettings);
            DataContext = viewModel;
            TextDataTextBox.Document = viewModel.TextData;
            HexDataTextBox.Document = viewModel.HexData;
            viewModel.OnHistoryApplied += OnHistoryApplied;
            viewModel.OnAutoScrollRequested += OnAutoScrollRequested;
        }

        private void OnAutoScrollRequested()
        {
            TextDataTextBox.ScrollToEnd();
            HexDataTextBox.ScrollToEnd();
        }

        private void OnHistoryApplied()
        {
            DataInput.Select(viewModel.DataInput.Length, 0);
        }

        public TerminalSettings GetTerminalSettings()
        {
            return viewModel.GetTerminalSettings();
        }

        private void DataInput_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (viewModel.SendDataCommand.CanExecute(null))
                    {
                        viewModel.SendDataCommand.Execute(null);
                    };
                    break;
            }
        }

        private void DataInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    viewModel.GetNextHistoryItem();
                    break;
                case Key.Down:
                    viewModel.GetPreviousHistoryItem();
                    break;
            }
        }

        public void Dispose()
        {
            viewModel.Dispose();
        }
    }
}
