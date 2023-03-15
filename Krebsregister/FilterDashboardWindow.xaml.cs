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

namespace Krebsregister
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class FilterDashboardWindow : Window
    {
        public static List<string> selectedICDs = new List<string>();
        string currentICD10 = null;
        List<string> manyicd10s = new List<string>();

        public FilterDashboardWindow(List<string> ICD10s)
        {
            InitializeComponent();
            selectedICDs.Clear();
            cb_selectICD10Dashboard.ItemsSource = ICD10s;
            cb_selectMultipleICD10Dashboard.ItemsSource = ICD10s;
        }

        public void btn_finished_Click(object sender, RoutedEventArgs e)
        {
            selectedICDs.Add(currentICD10);
            foreach (var item in manyicd10s)
            {
                selectedICDs.Add(item);
            }
            this.DialogResult = true;
        }

        private void cb_selectMultipleICD10Dashboard_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        {
            manyicd10s.Clear();
            foreach (string item in e.Selected)
            {
                string[] items = item.Split(" - ");
                manyicd10s.Add(items[0]);
            }
        }

        private void cb_selectICD10Dashboard_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string item = cb_selectICD10Dashboard.SelectedItem.ToString();
            string[] items = item.Split(" - ");
            currentICD10 = items[0];
        }
    }
}
