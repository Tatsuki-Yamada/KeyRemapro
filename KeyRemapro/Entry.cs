namespace KeyRemapro
{
    public class Entry
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
            menu.Items.Add("�ݒ�", null, (s, e) =>
            {
                
            });
            return menu;
        }
    }
}