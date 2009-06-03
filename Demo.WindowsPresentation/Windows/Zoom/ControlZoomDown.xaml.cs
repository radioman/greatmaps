using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Effects;

namespace Demo.WindowsPresentation
{
	/// <summary>
	/// Interaction logic for ControlZoomDown.xaml
	/// </summary>
	public partial class ControlZoomDown
	{
		public ControlZoomDown()
		{
			this.InitializeComponent();
		}

        private void LayoutRoot_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid grid = sender as Grid;
            DropShadowEffect dropShadowEffect = new DropShadowEffect();
            grid.Effect = dropShadowEffect;
        }

        private void LayoutRoot_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid grid = sender as Grid;
            grid.Effect = null;
        }
	}
}