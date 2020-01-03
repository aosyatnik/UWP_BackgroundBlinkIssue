using System.Collections.ObjectModel;

namespace UWP_BackgroundBlinkIssue
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<RegionViewModel> Regions { get; private set; } = new ObservableCollection<RegionViewModel>();

        public MainViewModel()
        {
            Regions.Add(new RegionViewModel());
        }
    }
}
