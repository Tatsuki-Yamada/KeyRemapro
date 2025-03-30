namespace KeyRemapro
{
    /// <summary>
    /// キー入力を変換するクラス
    /// </summary>
    public class KeyRemapper
    {
        // 実際にキー入力を監視するクラス
        KeyboardHooker _hooker;

        // キー入力を変更するトリガーキーが押されているかのフラグ（現在はF14キー）
        bool _pressingTriggerKey = false;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public KeyRemapper()
        {
            _hooker = new KeyboardHooker();

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
                
                if (ea.Key == Keys.F13)
                {
                    _pressingTriggerKey = true;
                    ea.RetCode = 1;
                    return;
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
                if (ea.Key == Keys.F13)
                    _pressingTriggerKey = false;
            };
        }
    }
}
