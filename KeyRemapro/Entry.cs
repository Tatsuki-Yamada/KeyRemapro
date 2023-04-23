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

            menu.Items.Add("設定を開く", null, (s, e) =>
            {
                Utility.ShowOnlyOneForm(typeof(SettingForm));
            });

            menu.Items.Add("終了", null, (s, e) => {
                Application.Exit();
            });
            
            return menu;
        }
    }
}