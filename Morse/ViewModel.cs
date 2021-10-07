using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using MessageBox = System.Windows.Forms.MessageBox;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using MouseEventHandler = System.Windows.Forms.MouseEventHandler;


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
            FileDialog dlg;
            if (Model.SelectedModeItem.Equals(Constants.Sed))
            {
                dlg = new OpenFileDialog {Multiselect = false};
            }
            else
            {
                dlg = new SaveFileDialog();
            }

            if (dlg.ShowDialog() == DialogResult.OK)
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
            Model.SyncByte = 0;

            for (var i = 0; i < Model.DataBytes.Count; i++)
            {
                Model.DataBytes[i] = 0;
            }

            Model.FireDataBlocks();

            Model.Status = string.Empty;
            Model.Progress = 0;

            Model.SyncBlockLeftLocation = Point.Empty;
            Model.SyncBlockRightLocation = Point.Empty;
            Model.DataBlockLeftLocation = Point.Empty;
            Model.DataBlockRightLocation = Point.Empty;
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

                            for (var i = 0; i < n; i++)
                            {
                                Model.DataBytes[i] = cache[i];
                            }

                            if (n < cache.Length)
                            {
                                for (var i = n; i < cache.Length; i++)
                                {
                                    Model.DataBytes[i] = null;
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
                UserActivityHook hook = new UserActivityHook();
                hook.OnMouseActivity += (sender, e) =>
                {
                    Model.Mouse = $"Location: [{e.Location.X},{e.Location.Y}]";

                    if (e.Button != MouseButtons.Left)
                    {
                        return;
                    }

                    if (e.Clicks == 1)
                    {
                        if (Model.SyncBlockLeftLocation == Point.Empty)
                        {
                            Model.SyncBlockLeftLocation = e.Location;
                        }
                        else if (Model.DataBlockLeftLocation == Point.Empty)
                        {
                            Model.DataBlockLeftLocation = e.Location;
                        }
                    }
                    else if (e.Clicks == 2)
                    {
                        if (Model.SyncBlockRightLocation == Point.Empty)
                        {
                            Model.SyncBlockRightLocation = e.Location;
                        }
                        else if (Model.DataBlockRightLocation == Point.Empty)
                        {
                            Model.DataBlockRightLocation = e.Location;
                        }
                    }
                };
                Task.Run(() =>
                {
                    byte lastSyncByte = 0;
                    bool isFinish = false;
                    using (FileStream fs = new FileStream(Model.SelectedFilePath, FileMode.Create, FileAccess.Write))
                    {
                        while (true)
                        {
                            if (Model.SyncBlockLeftLocation == Point.Empty ||
                                Model.SyncBlockRightLocation == Point.Empty)
                            {
                                Thread.Sleep(500);
                                continue;
                            }


                            if (Model.DataBlockLeftLocation == Point.Empty ||
                                Model.DataBlockRightLocation == Point.Empty)
                            {
                                Model.Status = "Please select the data block";
                                Thread.Sleep(500);
                                continue;
                            }

                            hook.Stop();
                            Model.Status = "Please click the run button in Virtual Window";
                            var captureSyncByte = CaptureSyncByte().Value;
                            if (captureSyncByte == lastSyncByte)
                            {
                                continue;
                            }

                            var expectedSyncDByte = GetNextSyncByte(lastSyncByte);
                            if (expectedSyncDByte != captureSyncByte)
                            {
                                Model.Status = "Error! Missed some frames, please try again.";
                                break;
                            }

                            lastSyncByte = captureSyncByte;
                            Model.SyncByte = captureSyncByte;
                            Thread.Sleep(50);
                            var captureDataBytes = CaptureDataBytes();
                            for (int i = 0; i < captureDataBytes.Length; i++)
                            {
                                Model.DataBytes[i] = captureDataBytes[i];
                            }

                            Model.FireDataBlocks();

                            byte[] cache = new byte[Constants.DataBlockByteSize];
                            int n = 0;
                            for (var i = 0; i < captureDataBytes.Length; i++)
                            {
                                var data = captureDataBytes[i];
                                if (data == null)
                                {
                                    isFinish = true;
                                    break;
                                }

                                cache[n++] = data.Value;
                            }
                            fs.Write(cache,0,n);
                            if (isFinish)
                            {
                                fs.Flush();
                                Model.Status = "Finished";
                                break;
                            }
                        }
                    }
                });
            }
        }


        private void SetSyncBlock()
        {
            Model.SyncByte = GetNextSyncByte(Model.SyncByte);
        }

        private byte GetNextSyncByte(byte currSync)
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

        private byte? CaptureSyncByte()
        {
            var captureBitmap = CaptureBitMap(Model.SyncBlockLeftLocation, Model.SyncBlockRightLocation);
            Color color = captureBitmap.GetPixel(captureBitmap.Width / 2, captureBitmap.Height / 2);
            return BlockColors.Covert(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        private byte?[] CaptureDataBytes()
        {
            var captureBitmap = CaptureBitMap(Model.DataBlockLeftLocation, Model.DataBlockRightLocation);
            var cellWidth = (float)captureBitmap.Width / Constants.DataColCount;
            var cellHeight = (float)captureBitmap.Height / Constants.DataRowCount;
            var firstCellX = cellWidth / 2;
            var firstCellY = cellHeight / 2;
            byte?[] result = new byte?[Constants.DataBlockByteSize];
            int n = 0;
            for (int i = 0; i < Constants.DataRowCount; i++)
            {
                for (int j = 0; j < Constants.DataColCount; j++)
                {
                    Color color = captureBitmap.GetPixel((int)(firstCellX+j*cellWidth), (int)(firstCellY+i*cellHeight));
                    byte? data= BlockColors.Covert(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                    result[n++] = data;
                }
            }
            
            return result;
        }

        private Bitmap CaptureBitMap(Point left, Point right)
        {
            Size size = new Size(right.X - left.X, right.Y - left.Y);
            Rectangle captureRectangle = new Rectangle(left, size);

            Bitmap captureBitmap = new Bitmap(captureRectangle.Width, captureRectangle.Height);
            Graphics captureGraphics = Graphics.FromImage(captureBitmap);
            captureGraphics.CopyFromScreen(captureRectangle.Left, captureRectangle.Top, 0, 0, captureRectangle.Size);
            captureBitmap.Save(@"C:\Users\user\Capture.jpg",ImageFormat.Jpeg);
            return captureBitmap;
        }
    }
}