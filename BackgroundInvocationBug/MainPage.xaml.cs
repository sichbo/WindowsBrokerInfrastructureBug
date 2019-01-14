using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace BackgroundInvocationBug {

    public sealed partial class MainPage : Page {

        public MainPage() {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;   
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e) {
            Refresh();
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e) {
            if (e.Key == Windows.System.VirtualKey.F5) {
                Refresh();
            }
            base.OnKeyUp(e);
        }

        private static ScrollViewer GetScrollViewer(DependencyObject dependencyObject) {
            if (dependencyObject is ScrollViewer scroller) return scroller;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++) {
                var child = VisualTreeHelper.GetChild(dependencyObject, i);
                var result = GetScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }

        private void Refresh() {
            _LogView.ItemsSource = App.LogRead();
            _LogView.UpdateLayout();
            var scroll = GetScrollViewer(_LogView);
            scroll.UpdateLayout();
            scroll.ChangeView(null, double.MaxValue, null);
        }
    }
}