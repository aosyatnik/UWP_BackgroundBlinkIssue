using Windows.UI;

namespace UWP_BackgroundBlinkIssue.Agenda
{
    public class AgendaItemViewModel : ViewModelBase
    {
        private static int Counter;
        public string Text { get; private set; }

        public AgendaItemViewModel()
        {
            Counter++;
            Text = $"{Counter}";
        }
    }
}
