using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using MessageBox = System.Windows.Forms.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Morse
{
    public class ViewModel
    {
        public Model Model { get; }
        public ICommand RunCommand { get; }
        public ICommand BrowseCommand { get; }

        public ViewModel()
        {
            Model = new Model();
            RunCommand = new RunCommand(this);
            BrowseCommand = new BrowseCommand(this);
        }


        public bool CanBrowseFile()
        {
            return true;
        }

        public void BrowseFile()
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Multiselect = false
            };
            if (dlg.ShowDialog() == true)
            {
                Model.SelectedFilePath = dlg.FileName;
            }

            Reset();
        }

        public bool CanRun()
        {
            return true;
        }

        private void Reset()
        {
            Model.SyncBlock = 0;

            for (var i = 0; i < Model.DataBlocks.Count; i++)
            {
                Model.DataBlocks[i] = Constants.Eof;
            }

            Model.FireDataBlocks();

            Model.Status = string.Empty;
            Model.Progress = 0;
        }

        public void Run()
        {
            if (string.IsNullOrEmpty(Model.SelectedFilePath))
            {
                MessageBox.Show("Please select file firstly", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (Model.SelectedModeItem.Equals(Constants.Sed))
            {
                Task.Run(() =>
                {
                    using (FileStream fs = new FileStream(Model.SelectedFilePath, FileMode.Open, FileAccess.Read))
                    {
                        byte[] cache = new byte[Constants.DataBlockByteSize];
                        long totalBytes = fs.Length;
                        long byteToRead = totalBytes;
                        long byteRead = 0;
                        while (byteToRead > 0)
                        {
                            var n = fs.Read(cache, 0, Constants.DataBlockByteSize);
                            if (n == 0)
                            {
                                break;
                            }

                            byteRead += n;
                            byteToRead -= n;

                            for (int i = 0; i < n; i++)
                            {
                                Model.DataBlocks[2 * i] = (byte) ((cache[i] >> 4) & 0x0F);
                                Model.DataBlocks[2 * i + 1] = (byte) (cache[i] & 0x0F);
                            }

                            if (n < cache.Length)
                            {
                                for (int i = n; i < cache.Length; i++)
                                {
                                    Model.DataBlocks[2 * i] = Constants.Eof;
                                    Model.DataBlocks[2 * i + 1] = Constants.Eof;
                                }
                            }

                            SetSyncBlock();
                            Model.FireDataBlocks();
                            Model.Progress = (int) ((float) byteRead / totalBytes * 100);
                            Thread.Sleep(1000 / Constants.FrameRate);
                        }
                    }
                });
            }
            else
            {
                Model.Status = "Please select the sync block";
                Task.Run(() =>
                {
                    while (true)
                    {
                        Model.Status = Hook.GetCursorPosition().X + ":" + Hook.GetCursorPosition().Y;
                    }
                });

                //Creating a new Bitmap object
                // Rectangle captureRectangle = new Rectangle();
                // captureRectangle.X = 40;
                // captureRectangle.X = 320;
                // captureRectangle.Y = 200;
                // captureRectangle.Height = 300;
                // captureRectangle.Width = 260;
                // Bitmap captureBitmap = new Bitmap(captureRectangle.Width, captureRectangle.Height);
                // Graphics captureGraphics = Graphics.FromImage(captureBitmap);
                // captureGraphics.CopyFromScreen(captureRectangle.Left,captureRectangle.Top,0,0,captureRectangle.Size);
                // captureBitmap.Save(@"C:\Users\user\Capture.jpg",ImageFormat.Jpeg);
                //
                // Color color = captureBitmap.GetPixel(captureRectangle.Height / 2, captureRectangle.Width / 2);
                // MessageBox.Show(color.ToString());
            }
        }

        private void SetSyncBlock()
        {
            Model.SyncBlock = GetNextSync(Model.SyncBlock);
        }

        private byte GetNextSync(byte currSync)
        {
            if (currSync == 0)
            {
                return 4;
            }

            if (currSync == 4)
            {
                return 8;
            }

            if (currSync == 8)
            {
                return 12;
            }

            return 0;
        }
    }
}