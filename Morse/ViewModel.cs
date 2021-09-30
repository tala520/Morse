using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Color = System.Drawing.Color;
using PixelFormat = System.Windows.Media.PixelFormat;

namespace Morse
{
    public class ViewModel
    {
        public Model m { get; set; }
        public ICommand RunCommand { get; set; }

        public ViewModel()
        {
            m = new Model();
            m.ModeItems = new ObservableCollection<string>();
            m.ModeItems.Add("Sed");
            m.ModeItems.Add("Rev");

            m.SelectedModeItem = "Rev";

            RunCommand = new ButtonCommand(this);
        }

        public void Run()
        {
            // MessageBox.Show(this.m.SelectedModeItem);
            if (m.SelectedModeItem.Equals("Sed"))
            {
                m.Color1 = Colors.Red.ToString();
                m.Color2=Colors.Yellow.ToString();
            }
            else
            {
                //Creating a new Bitmap object
                Rectangle captureRectangle = new Rectangle();
                captureRectangle.X = 40;
                captureRectangle.X = 320;
                captureRectangle.Y = 200;
                captureRectangle.Height = 300;
                captureRectangle.Width = 260;
                Bitmap captureBitmap = new Bitmap(captureRectangle.Width, captureRectangle.Height);
                Graphics captureGraphics = Graphics.FromImage(captureBitmap);
                captureGraphics.CopyFromScreen(captureRectangle.Left,captureRectangle.Top,0,0,captureRectangle.Size);
                captureBitmap.Save(@"C:\Users\user\Capture.jpg",ImageFormat.Jpeg);

                Color color = captureBitmap.GetPixel(captureRectangle.Height / 2, captureRectangle.Width / 2);
                MessageBox.Show(color.ToString());
            }
        }
    }
}