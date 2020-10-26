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

namespace LISFramework {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {
    public MainWindow() {
      InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) {

    }

    private void TabCtrl_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      TabItem tbI = tabCtrl.SelectedItem as TabItem;
      if (e.Source is TabControl) {
        switch ((tbI.Name)) {
          case "tbLisComm": {
              LisComm lisComm = new LisComm();
              tbLisComm.Content = lisComm;
            }
            break;
        }
      }
    }
  }
}
