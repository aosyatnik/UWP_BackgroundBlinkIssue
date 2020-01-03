using Windows.UI;

namespace UWP_BackgroundBlinkIssue.Agenda
{
    public class AgendaItemViewModel : ViewModelBase
    {
        private static int Counter;
        public string Text { get; private set; }

        public Color Color { get; private set; } = Color.FromArgb(255, 255, 0, 0); // Red.

        public AgendaItemViewModel()
        {
            Counter++;
            Text = $"{Counter}";
        }
    }
}
