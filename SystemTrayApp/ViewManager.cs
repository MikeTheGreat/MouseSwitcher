using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Reflection;


namespace SystemTrayApp
{
    public class ViewManager
    {
        private bool leftIsPrimary;
        public ViewManager()
        {
            leftIsPrimary = !System.Windows.Forms.SystemInformation.MouseButtonsSwapped;
            System.Drawing.Icon ic = leftIsPrimary ? 
                SystemTrayApp.Properties.Resources.LeftMouseButtonActive : 
                SystemTrayApp.Properties.Resources.RightMouseButtonActive;


            _components = new System.ComponentModel.Container();
            _notifyIcon = new System.Windows.Forms.NotifyIcon(_components)
            {
                ContextMenuStrip = new ContextMenuStrip(),
                Icon = ic,
                Text = "System Tray App: Device Not Present",
                Visible = true,
            };

            _notifyIcon.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
            _notifyIcon.DoubleClick += notifyIcon_DoubleClick;
            _notifyIcon.MouseUp += notifyIcon_MouseUp;

            _aboutViewModel = new WpfFormLibrary.ViewModel.AboutViewModel();
            //_statusViewModel = new WpfFormLibrary.ViewModel.StatusViewModel();

            //_statusViewModel.Icon = AppIcon;
            _aboutViewModel.Icon = AppIcon;

            _hiddenWindow = new System.Windows.Window();
            _hiddenWindow.Hide();
        }

        System.Windows.Media.ImageSource AppIcon
        {
            get
            {
                leftIsPrimary = !System.Windows.Forms.SystemInformation.MouseButtonsSwapped;
                System.Drawing.Icon icon = leftIsPrimary ? Properties.Resources.LeftMouseButtonActive : Properties.Resources.RightMouseButtonActive;
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    System.Windows.Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
        }


        // This allows code to be run on a GUI thread
        private System.Windows.Window _hiddenWindow;

        private System.ComponentModel.IContainer _components;
        // The Windows system tray class
        private NotifyIcon _notifyIcon;  
        //IDeviceManager _deviceManager;

        private WpfFormLibrary.View.AboutView _aboutView;
        private WpfFormLibrary.ViewModel.AboutViewModel _aboutViewModel;
        private WpfFormLibrary.View.StatusView _statusView;
        private WpfFormLibrary.ViewModel.StatusViewModel _statusViewModel;

        //private ToolStripMenuItem _startDeviceMenuItem;
        private ToolStripMenuItem _swapMouseButtonsMenuItem;
        private ToolStripMenuItem _exitMenuItem;

        private void DisplayStatusMessage(string text)
        {
            _hiddenWindow.Dispatcher.Invoke(delegate
            {
                _notifyIcon.BalloonTipText = text;
                // The timeout is ignored on recent Windows
                _notifyIcon.ShowBalloonTip(3000);
            });
        }

        public void OnStatusChange()
        {
            leftIsPrimary = !System.Windows.Forms.SystemInformation.MouseButtonsSwapped;
            _notifyIcon.Icon = leftIsPrimary ?
                SystemTrayApp.Properties.Resources.LeftMouseButtonActive :
                SystemTrayApp.Properties.Resources.RightMouseButtonActive;

            System.Windows.Media.ImageSource icon = AppIcon;
            if (_aboutView != null)
            {
                _aboutView.Icon = AppIcon;
            }
            if (_statusView != null)
            {
                _statusView.Icon = AppIcon;
            }
        }



        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SwapMouseButton([param: MarshalAs(UnmanagedType.Bool)] bool fSwap);

        public void MakeRightButtonPrimary()
        {
            SwapMouseButton(true);
        }
        public void MakeLeftButtonPrimary()
        {
            SwapMouseButton(false);
        }


        private void startStopReaderItem_Click(object sender, EventArgs e)
        {
            if (leftIsPrimary)
                MakeRightButtonPrimary();
            else
                MakeLeftButtonPrimary();
            OnStatusChange();
        }
        
        private ToolStripMenuItem ToolStripMenuItemWithHandler(string displayText, string tooltipText, EventHandler eventHandler)
        {
            var item = new ToolStripMenuItem(displayText);
            if (eventHandler != null)
            {
                item.Click += eventHandler;
            }

            item.ToolTipText = tooltipText;
            return item;
        }


        private void ShowAboutView()
        {
            if (_aboutView == null)
            {
                _aboutView = new WpfFormLibrary.View.AboutView();
                _aboutView.DataContext = _aboutViewModel;
                _aboutView.Closing += ((arg_1, arg_2) => _aboutView = null);
                _aboutView.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

                _aboutView.Show();
            }
            else
            {
                _aboutView.Activate();
            }
            _aboutView.Icon = AppIcon;

            _aboutViewModel.AddVersionInfo("Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());

        }

        private void showHelpItem_Click(object sender, EventArgs e)
        {
            ShowAboutView();
        }

        private void showWebSite_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.CodeProject.com/");
        }
        
        private void exitItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowAboutView();
        }

        private void notifyIcon_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(_notifyIcon, null);
            }
        }
        
        private void SetMenuItems()
        {
            //switch (_deviceManager.Status)
            //{
            //    case DeviceStatus.Initialised:
            //        _startDeviceMenuItem.Enabled = true;
            //        _stopDeviceMenuItem.Enabled = false;
            //        _exitMenuItem.Enabled = true;
            //        break;
            //    case DeviceStatus.Starting:
            //        _startDeviceMenuItem.Enabled = false;
            //        _stopDeviceMenuItem.Enabled = false;
            //        _exitMenuItem.Enabled = false;
            //        break;
            //    case DeviceStatus.Running:
            //        _startDeviceMenuItem.Enabled = false;
            //        _stopDeviceMenuItem.Enabled = true;
            //        _exitMenuItem.Enabled = true;
            //        break;
            //    case DeviceStatus.Uninitialised:
            //        _startDeviceMenuItem.Enabled = false;
            //        _stopDeviceMenuItem.Enabled = false;
            //        _exitMenuItem.Enabled = true;
            //        break;
            //    case DeviceStatus.Error:
            //        _startDeviceMenuItem.Enabled = false;
            //        _stopDeviceMenuItem.Enabled = false;
            //        _exitMenuItem.Enabled = true;
            //        break;
            //    default:
            //        System.Diagnostics.Debug.Assert(false, "SetButtonStatus() => Unknown state");
            //        break;
            //}
        }

        private void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;

            if (_notifyIcon.ContextMenuStrip.Items.Count == 0)
            {
                //_startDeviceMenuItem = ToolStripMenuItemWithHandler(
                //    "Start Device",
                //    "Starts the device",
                //    startStopReaderItem_Click);
                //_notifyIcon.ContextMenuStrip.Items.Add(_startDeviceMenuItem);
                _swapMouseButtonsMenuItem = ToolStripMenuItemWithHandler(
                    "Swap Mouse Buttons",
                    "Swaps the primary mouse button",
                    startStopReaderItem_Click);
                _notifyIcon.ContextMenuStrip.Items.Add(_swapMouseButtonsMenuItem);
                _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                //_notifyIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("Device S&tatus", "Shows the device status dialog", showStatusItem_Click));
                _notifyIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("&About", "About MouseSwitcher", showHelpItem_Click));
                _notifyIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("Code Project &Web Site", "Navigates to the Code Project Web Site - the folks who created the starter project that this app is based on", showWebSite_Click));
                _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                _exitMenuItem = ToolStripMenuItemWithHandler("&Exit", "Exits System Tray App", exitItem_Click);
                _notifyIcon.ContextMenuStrip.Items.Add(_exitMenuItem);
            }

            SetMenuItems();
        }
    }
}
