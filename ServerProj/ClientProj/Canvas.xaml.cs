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

namespace ClientProj
{
    /// <summary>
    /// Interaction logic for Canvas.xaml
    /// </summary>
    public partial class Canvas : Window
    {
        //create a Point variable to store the location of the mouse cursor
        private Point currentPos = new Point();
        public Canvas()
        {
            InitializeComponent();
        }

        //Code for drawing on the canvas taken from https://stackoverflow.com/questions/16037753/wpf-drawing-on-canvas-with-mouse-events User "Andy"

        //Method that runs when left mouse button is clicked
        private void LeftClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //If left mouse button is clicked
            if (e.ButtonState == MouseButtonState.Pressed)
                //update the position of the mouse cursor
                currentPos = e.GetPosition(this);
        }

        //Method that runs when the mouse is moved
        private void MouseMoveToDraw(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //checks to see if left click is pressed
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //Create a new line object
                Line line = new Line();

                line.Stroke = SystemColors.WindowFrameBrush;

                //set start and end point of the line
                line.X1 = currentPos.X;
                line.Y1 = currentPos.Y;
                line.X2 = e.GetPosition(this).X;
                line.Y2 = e.GetPosition(this).Y;

                //update current mouse pos
                currentPos = e.GetPosition(this);

                //add the line to the canvas
                Paint.Children.Add(line);
            }
        }
    }
}
