using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Cursor = System.Windows.Forms.Cursor;
using MouseEventHandler = System.Windows.Forms.MouseEventHandler;

namespace Morse
{
    public class ViewModel
    {
        private const int SyncBlockCount = 4;
        public Model Model { get; }
        public ICommand RunCommand { get; }
        public ICommand BrowseCommand { get; }
        public ICommand ClickCommand { get; }
        private bool Running { get; set; }
        private int DataBlockCount => Configs.BlockCount - SyncBlockCount;
        private DateTime? LastHangTime { get; set; }

        public ViewModel()
        {
            Model = new Model();
            RunCommand = new RunCommand(this);
            BrowseCommand = new BrowseCommand(this);
            ClickCommand = new ClickCommand(this);
        }

        public void BrowseFile()
        {
            FileDialog dlg;
            if (Model.SelectedModeItem.Equals(Configs.Sed))
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

        private void Reset()
        {
            Running = false;
            for (var i = 0; i < Model.DataBytes.Count; i++)
            {
                Model.DataBytes[i] = 0;
            }

            Model.FireDataBlocks();
            Model.Status = string.Empty;
            Model.Progress = 0;
            Model.DataBlockLeftLocation = Point.Empty;
            Model.DataBlockRightLocation = Point.Empty;
            Model.CurrBlockByte = 0;
        }

        public void Run()
        {
            Running = true;
            if (Model.SelectedModeItem.Equals(Configs.Sed))
            {
                Send();
            }
            else
            {
                Receive();
            }
        }

        private void Send()
        {
            Model.SentFrames = 0;
            Model.SendOffset = 0;

            //read all file content
            using (FileStream fs = new FileStream(Model.SelectedFilePath, FileMode.Open, FileAccess.Read))
            {
                Model.FileData = new byte[fs.Length];
                fs.Read(Model.FileData, 0, (int) fs.Length);
            }

            Model.TotalFrames = Model.FileData.Length / DataBlockCount;

            SendOneFrame();
        }

        public void ReceiveConfirm()
        {
            if (DateTime.Now.Subtract(Model.LastConfirmTime).Milliseconds < 20)
                return; // Don't allow confirm in short time.

            SendOneFrame();
        }

        private void SendOneFrame()
        {
            if (IsSendFinish()) return;

            Model.CurrBlockByte = GetNextSyncByte(Model.CurrBlockByte);
            for (var n = 0; n < Configs.BlockCount; n++)
            {
                if (IsSyncBlock(n)) Model.DataBytes[n] = Model.CurrBlockByte;
                else if (IsSendFinish()) Model.DataBytes[n] = null;
                else Model.DataBytes[n] = Model.FileData[Model.SendOffset++];
            }

            Model.SentFrames++;
            Model.FireDataBlocks();
            Model.LastConfirmTime = DateTime.Now;
            Model.Progress = (int) ((float) Model.SentFrames / Model.TotalFrames * 100);
            Model.Status = $"Sent {Model.SentFrames} Frames - Total {Model.TotalFrames} Frames";
        }

        private bool IsSendFinish()
        {
            if (!Running) return true;

            var isFinish = Model.SendOffset >= Model.FileData.Length;
            if (isFinish) Running = false;
            return isFinish;
        }

        private bool IsSyncBlock(int blockIndex)
        {
            // four corners are sync blocks
            return blockIndex == 0
                   || (blockIndex == Configs.GridColCount - 1)
                   || (blockIndex == Configs.BlockCount - Configs.GridColCount)
                   || blockIndex == Configs.BlockCount - 1;
        }

        private void Receive()
        {
            Model.ReceivedFrames = 0;
            LastHangTime = null;
            UserActivityHook hook = new UserActivityHook();
            hook.OnMouseActivity += hookOnOnMouseActivity();
            Task.Run(() =>
            {
                using (FileStream fs = new FileStream(Model.SelectedFilePath, FileMode.Create, FileAccess.Write))
                {
                    while (true)
                    {
                        if (!Running)
                        {
                            fs.Flush();
                            Model.Status = $"Finished - Received {Model.ReceivedFrames} Frames";
                            break;
                        }

                        if (Model.DataBlockLeftLocation == Point.Empty ||
                            Model.DataBlockRightLocation == Point.Empty)
                        {
                            Model.Status = "Please select the data block area";
                            Thread.Sleep(500);
                            continue;
                        }

                        hook.Stop();
                        if (Model.ReceivedFrames == 0) Model.Status = "Ready for receiving";

                        try
                        {
                            // if (!IsCaptureFirstSyncBlockStillOldOne()) continue;
                            CaptureDataBytes();
                        }
                        catch (ArgumentException ex)
                        {
                            continue;
                        }

                        byte[] dataCache = new byte[DataBlockCount];
                        byte[] syncData = new byte[SyncBlockCount];
                        int dataIndex = 0;
                        int syncIndex = 0;
                        for (var i = 0; i < Model.DataBytes.Count; i++)
                        {
                            var data = Model.DataBytes[i];
                            if (data == null)
                            {
                                Running = false;
                                continue;
                            }

                            if (IsSyncBlock(i))
                                syncData[syncIndex++] = data.Value;
                            else
                                dataCache[dataIndex++] = data.Value;
                        }

                        if (!IsSyncBlockValid(syncData)) //in this case, the whole picture is corrupted. Ignore it.
                        {
                            Running = true;
                            continue;
                        }

                        Model.FireDataBlocks();
                        fs.Write(dataCache, 0, dataIndex);
                        Model.ReceivedFrames++;
                        Model.CurrBlockByte = GetNextSyncByte(Model.CurrBlockByte);
                        Model.Status = $"Received {Model.ReceivedFrames} Frames";
                        ClickScreenToConfirm();
                        Thread.Sleep(500);
                    }
                }
            });
        }

        private MouseEventHandler hookOnOnMouseActivity()
        {
            return (sender, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                if (e.Clicks == 1 && Model.DataBlockLeftLocation == Point.Empty)
                    Model.DataBlockLeftLocation = e.Location;
                if (e.Clicks == 2 && Model.DataBlockRightLocation == Point.Empty)
                    Model.DataBlockRightLocation = e.Location;
            };
        }

        private bool IsSyncBlockValid(byte[] syncData)
        {
            if (syncData == null) return false;
            if (syncData.Length != SyncBlockCount) return false;
            if (syncData.All(e => e == Model.CurrBlockByte))
            {
                if (LastHangTime == null)
                {
                    LastHangTime = DateTime.Now;
                }
                else
                {
                    if (DateTime.Now.Subtract(LastHangTime.Value).Seconds > 10)
                    {
                        Model.Status = "Try to Re-Confirm";
                        ClickScreenToConfirm();
                        LastHangTime = null;
                    }
                }

                return false;
            }

            var nextSyncByte = GetNextSyncByte(Model.CurrBlockByte);
            if (syncData.Any(e => e != nextSyncByte)) return false;

            LastHangTime = null;
            return true;
        }

        private byte GetNextSyncByte(byte currSync)
        {
            if (currSync == 0)
            {
                return 64;
            }

            if (currSync == 64)
            {
                return 128;
            }

            if (currSync == 128)
            {
                return 250;
            }

            return 0;
        }


        private bool IsCaptureFirstSyncBlockStillOldOne()
        {
            int width = Model.DataBlockRightLocation.X - Model.DataBlockLeftLocation.X;
            int height = Model.DataBlockRightLocation.Y - Model.DataBlockLeftLocation.Y;
            var cellWidth = (float) width / Configs.GridColCount;
            var cellHeight = (float) height / Configs.GridRowCount;

            Bitmap captureBitmap = new Bitmap((int) cellWidth, (int) cellHeight);
            Rectangle captureRectangle = new Rectangle(Model.DataBlockLeftLocation, captureBitmap.Size);
            Graphics captureGraphics = Graphics.FromImage(captureBitmap);
            captureGraphics.CopyFromScreen(captureRectangle.Left, captureRectangle.Top, 0, 0, captureRectangle.Size);
            // captureBitmap.Save(@"C:\Users\user\Capture.jpg",ImageFormat.Jpeg);
            Color color = captureBitmap.GetPixel((int) (cellWidth / 2), (int) (cellHeight / 2));

            var firstSyncBlock =
                BlockColors.Covert(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
            if (firstSyncBlock == null) return false;
            return firstSyncBlock != Model.CurrBlockByte;
        }

        private void CaptureDataBytes()
        {
            var captureBitmap = CaptureBitMap(Model.DataBlockLeftLocation, Model.DataBlockRightLocation);
            var cellWidth = (float) captureBitmap.Width / Configs.GridColCount;
            var cellHeight = (float) captureBitmap.Height / Configs.GridRowCount;
            var firstCellX = cellWidth / 2;
            var firstCellY = cellHeight / 2;

            for (int i = 0; i < Configs.GridRowCount; i++)
            {
                for (int j = 0; j < Configs.GridColCount; j++)
                {
                    Color color = captureBitmap.GetPixel((int) (firstCellX + j * cellWidth),
                        (int) (firstCellY + i * cellHeight));
                    Model.DataBytes[i * Configs.GridColCount + j] =
                        BlockColors.Covert(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                }
            }
        }

        private Bitmap CaptureBitMap(Point left, Point right)
        {
            Size size = new Size(right.X - left.X, right.Y - left.Y);
            Rectangle captureRectangle = new Rectangle(left, size);

            Bitmap captureBitmap = new Bitmap(captureRectangle.Width, captureRectangle.Height);
            Graphics captureGraphics = Graphics.FromImage(captureBitmap);
            captureGraphics.CopyFromScreen(captureRectangle.Left, captureRectangle.Top, 0, 0, captureRectangle.Size);
            // captureBitmap.Save(@"C:\Users\user\Capture.jpg",ImageFormat.Jpeg);
            return captureBitmap;
        }

        private void ClickScreenToConfirm()
        {
            Cursor.Position = Model.DataBlockCenterLocation;
            Thread.Sleep(20);
            UserActivityHook.DoMouseClick();
            Cursor.Position = Model.OutOfDataBlockLocation; //Move the Cursor out of the Data area.
        }
    }
}