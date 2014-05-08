using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClumsyWordsUniversal.Views.ViewStateManagment
{
    public class CustomViewStates
    {
        public string State { get; set; }

        public Func<double, double, bool> MatchState { get; set; }
    }
}
