// DLLインポートに使う
using System.Runtime.InteropServices;

// Debug.WriteLineに使う
using System.Diagnostics;


namespace KeyRemapro
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            ApplicationConfiguration.Initialize();

            CreateNotifyIcon();

            _ = new KeyRemapper();

            Application.Run();
        }


        private static void CreateNotifyIcon()
        {
            // 常駐アプリ（タスクトレイのアイコン）を作成
            var icon = new NotifyIcon();
            icon.Icon = new Icon("Icon.ico");
            icon.ContextMenuStrip = ContextMenu();
            icon.Text = "KeyRemapro";
            icon.Visible = true;
        }


        private static ContextMenuStrip ContextMenu()
        {
            // アイコンを右クリックしたときのメニューを返却
            var menu = new ContextMenuStrip();
            menu.Items.Add("終了", null, (s, e) => {
                Application.Exit();
            });
            return menu;
        }
    }


    public class KeyRemapper
    {
        // 実際にキー入力を監視するクラス
        KeyboardHook _hooker;

        // キー入力を変更するトリガーキーが押されているかのフラグ（現在はF14キー）
        bool _pressingTriggerKey = false;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public KeyRemapper()
        {
            _hooker = new KeyboardHook();

            // キーを押した・離したときの処理をHookerのイベントに追加する。
            AddKeyDownEvent();
            AddKeyUpEvent();

            // アプリ終了時に監視を止める処理をExitイベントに追加する。
            Application.ApplicationExit += (sender, e) =>
            {
                _hooker.UnHook();
            };

            // キー入力の監視を始める。
            _hooker.Hook();
        }


        /// <summary>
        /// キー押下時の処理をHookerのdelegateに追加する関数
        /// </summary>
        private void AddKeyDownEvent()
        {
            _hooker.OnKeyDown += (s, ea) =>
            {
                if (ea.Key == Keys.F14)
                    _pressingTriggerKey = true;

                if (_pressingTriggerKey)
                {
                    switch (ea.Key)
                    {
                        case Keys.I:
                            SendKeys.Send("{UP}");
                            goto NORETUNKEY;
                        case Keys.K:
                            SendKeys.Send("{DOWN}");
                            goto NORETUNKEY;
                        case Keys.J:
                            SendKeys.Send("{LEFT}");
                            goto NORETUNKEY;
                        case Keys.L:
                            SendKeys.Send("{RIGHT}");
                            goto NORETUNKEY;

                        NORETUNKEY:
                            ea.RetCode = 1;
                            return;
                    }
                }

                ea.RetCode = 0;
            };
        }


        /// <summary>
        /// キーを離したときの処理をHookerのdelegateに追加する関数
        /// </summary>
        private void AddKeyUpEvent()
        {
            _hooker.OnKeyUp += (s, ea) =>
            {
                if (ea.Key == Keys.F14)
                    _pressingTriggerKey = false;
            };
        }
    }


    /*
     * 以下、参考記事の丸パクリ↓
     * https://resanaplaza.com/2022/12/31/%E3%80%90%E3%82%B3%E3%83%94%E3%83%9A%E3%81%A7%E4%BD%BF%E3%81%88%E3%82%8B%E3%80%91c%E3%81%8B%E3%82%89key-hook%E3%81%A7%E3%82%AD%E3%83%BC%E5%85%A5%E5%8A%9B%E3%82%92%E5%8F%96%E5%BE%97%E3%81%99%E3%82%8B/
     */

    /// <summary>
    /// キーボードを監視するクラス
    /// </summary>
    public class KeyboardHook
    {
        private const int WH_KEYBOARD_LL = 0x0D;
        private const int WM_KEYBOARD_DOWN = 0x100;
        private const int WM_KEYBOARD_UP = 0x101;
        private const int WM_SYSKEY_DOWN = 0x104;
        private const int WM_SYSKEY_UP = 0x105;

        //イベントハンドラの定義
        public event EventHandler<KeyboardHookEventArgs> OnKeyDown = delegate { };
        public event EventHandler<KeyboardHookEventArgs> OnKeyUp = delegate { };

        //コールバック関数のdelegate 定義
        private delegate IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam);

        //キーボードフックに必要なDLLのインポート
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookCallback lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        //フックプロシージャのハンドル
        private static IntPtr _hookHandle = IntPtr.Zero;

        //フック時のコールバック関数
        private static HookCallback _callback;

        /// <summary>
        /// キーボードHook の開始
        /// </summary>
        /// <param name="callback"></param>
        public void Hook()
        {
            _callback = CallbackProc;
            using (Process process = Process.GetCurrentProcess())
            {
                using (ProcessModule module = process.MainModule)
                {
                    _hookHandle = SetWindowsHookEx(
                       WH_KEYBOARD_LL,                                          //フックするイベントの種類
                       _callback, //フック時のコールバック関数
                       GetModuleHandle(module.ModuleName),                      //インスタンスハンドル
                       0                                                        //スレッドID（0：全てのスレッドでフック）
                   );
                }
            }
        }
        /// <summary>
        /// コールバック関数
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private IntPtr CallbackProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var args = new KeyboardHookEventArgs();
            Keys key = (Keys)(short)Marshal.ReadInt32(lParam);
            args.Key = key;

            if ((int)wParam == WM_KEYBOARD_DOWN || (int)wParam == WM_SYSKEY_DOWN) OnKeyDown(this, args);
            if ((int)wParam == WM_KEYBOARD_UP || (int)wParam == WM_SYSKEY_UP) OnKeyUp(this, args);

            return (args.RetCode == 0) ? CallNextHookEx(_hookHandle, nCode, wParam, lParam) : (IntPtr)1;
        }
        /// <summary>
        /// キーボードHockの終了
        /// </summary>
        public void UnHook()
        {
            UnhookWindowsHookEx(_hookHandle);
            _hookHandle = IntPtr.Zero;
        }
    }

    /// <summary>
    /// キーボードフックのイベント引数
    /// </summary>
    public class KeyboardHookEventArgs
    {
        public Keys Key { get; set; }
        public int RetCode { get; set; } = 0;
    }
}