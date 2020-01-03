using System.Collections.ObjectModel;
using UWP_BackgroundBlinkIssue.Agenda;

namespace UWP_BackgroundBlinkIssue
{
    public class RegionViewModel : ViewModelBase
    {
        public ObservableCollection<AgendaItemViewModel> Items { get; private set; } = new ObservableCollection<AgendaItemViewModel>();

        public int SwitchTime { get; private set; } = 3; // 3 seconds

        public RegionViewModel()
        {
            // Set up 12 agenda items.
            for (var i = 0; i < 12; i++)
            {
                Items.Add(new AgendaItemViewModel());
            }
        }
    }
}
