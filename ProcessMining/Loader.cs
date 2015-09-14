using ProcessMining.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProcessMining
{
    public static class Loader
    {
        private static Thread loader;
        public static void Show()
        {
            Splash splashForm = null;
            loader = new Thread(new ThreadStart(
                delegate
                {
                    splashForm = new Splash();
                    Application.Run(splashForm);
                }
                ));

            loader.SetApartmentState(ApartmentState.STA);
            loader.Start();
        }
        public static void Hide()
        {
            loader.Abort();
        }
    }
}
