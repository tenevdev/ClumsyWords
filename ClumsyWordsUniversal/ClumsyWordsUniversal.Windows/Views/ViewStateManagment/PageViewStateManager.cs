using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ClumsyWordsUniversal.Views.ViewStateManagment
{

    public class PageViewStateManager
    {
        private Page _page;

        public PageViewStateManager(Page page)
        {
            this._page = page;
            this._page.Loaded += Loaded;
            this._page.Unloaded += Unloaded;
        }

        public IEnumerable<CustomViewStates> States { get; set; }

        private void Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //Window.Current.SizeChanged -= WindowSizeChanged;
        }

        private void Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //Window.Current.SizeChanged += WindowSizeChanged;
            DetermineState(Window.Current.Bounds.Width, Window.Current.Bounds.Height);
        }

        private void WindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            DetermineState(e.Size.Width, e.Size.Height);
        }

        private void DetermineState(double width, double height)
        {
            var state = States.First(x => x.MatchState(width, height));

            VisualStateManager.GoToState(this._page, state.State, false);
        }

    }
}
