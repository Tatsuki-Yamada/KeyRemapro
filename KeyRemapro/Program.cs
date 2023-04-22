// DLL�C���|�[�g�Ɏg��
using System.Runtime.InteropServices;

// Debug.WriteLine�Ɏg��
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
            // �풓�A�v���i�^�X�N�g���C�̃A�C�R���j���쐬
            var icon = new NotifyIcon();
            icon.Icon = new Icon("Icon.ico");
            icon.ContextMenuStrip = ContextMenu();
            icon.Text = "KeyRemapro";
            icon.Visible = true;
        }


        private static ContextMenuStrip ContextMenu()
        {
            // �A�C�R�����E�N���b�N�����Ƃ��̃��j���[��ԋp
            var menu = new ContextMenuStrip();
            menu.Items.Add("�I��", null, (s, e) => {
                Application.Exit();
            });
            return menu;
        }
    }


    public class KeyRemapper
    {
        // ���ۂɃL�[���͂��Ď�����N���X
        KeyboardHook _hooker;

        // �L�[���͂�ύX����g���K�[�L�[��������Ă��邩�̃t���O�i���݂�F14�L�[�j
        bool _pressingTriggerKey = false;


        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public KeyRemapper()
        {
            _hooker = new KeyboardHook();

            // �L�[���������E�������Ƃ��̏�����Hooker�̃C�x���g�ɒǉ�����B
            AddKeyDownEvent();
            AddKeyUpEvent();

            // �A�v���I�����ɊĎ����~�߂鏈����Exit�C�x���g�ɒǉ�����B
            Application.ApplicationExit += (sender, e) =>
            {
                _hooker.UnHook();
            };

            // �L�[���͂̊Ď����n�߂�B
            _hooker.Hook();
        }


        /// <summary>
        /// �L�[�������̏�����Hooker��delegate�ɒǉ�����֐�
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
        /// �L�[�𗣂����Ƃ��̏�����Hooker��delegate�ɒǉ�����֐�
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
     * �ȉ��A�Q�l�L���̊ۃp�N����
     * https://resanaplaza.com/2022/12/31/%E3%80%90%E3%82%B3%E3%83%94%E3%83%9A%E3%81%A7%E4%BD%BF%E3%81%88%E3%82%8B%E3%80%91c%E3%81%8B%E3%82%89key-hook%E3%81%A7%E3%82%AD%E3%83%BC%E5%85%A5%E5%8A%9B%E3%82%92%E5%8F%96%E5%BE%97%E3%81%99%E3%82%8B/
     */

    /// <summary>
    /// �L�[�{�[�h���Ď�����N���X
    /// </summary>
    public class KeyboardHook
    {
        private const int WH_KEYBOARD_LL = 0x0D;
        private const int WM_KEYBOARD_DOWN = 0x100;
        private const int WM_KEYBOARD_UP = 0x101;
        private const int WM_SYSKEY_DOWN = 0x104;
        private const int WM_SYSKEY_UP = 0x105;

        //�C�x���g�n���h���̒�`
        public event EventHandler<KeyboardHookEventArgs> OnKeyDown = delegate { };
        public event EventHandler<KeyboardHookEventArgs> OnKeyUp = delegate { };

        //�R�[���o�b�N�֐���delegate ��`
        private delegate IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam);

        //�L�[�{�[�h�t�b�N�ɕK�v��DLL�̃C���|�[�g
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookCallback lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        //�t�b�N�v���V�[�W���̃n���h��
        private static IntPtr _hookHandle = IntPtr.Zero;

        //�t�b�N���̃R�[���o�b�N�֐�
        private static HookCallback _callback;

        /// <summary>
        /// �L�[�{�[�hHook �̊J�n
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
                       WH_KEYBOARD_LL,                                          //�t�b�N����C�x���g�̎��
                       _callback, //�t�b�N���̃R�[���o�b�N�֐�
                       GetModuleHandle(module.ModuleName),                      //�C���X�^���X�n���h��
                       0                                                        //�X���b�hID�i0�F�S�ẴX���b�h�Ńt�b�N�j
                   );
                }
            }
        }
        /// <summary>
        /// �R�[���o�b�N�֐�
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
        /// �L�[�{�[�hHock�̏I��
        /// </summary>
        public void UnHook()
        {
            UnhookWindowsHookEx(_hookHandle);
            _hookHandle = IntPtr.Zero;
        }
    }

    /// <summary>
    /// �L�[�{�[�h�t�b�N�̃C�x���g����
    /// </summary>
    public class KeyboardHookEventArgs
    {
        public Keys Key { get; set; }
        public int RetCode { get; set; } = 0;
    }
}