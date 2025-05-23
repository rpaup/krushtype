namespace krushtype.Properties
{


    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
    {

        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));

        public static Settings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("english")]
        public string SelectedLanguage
        {
            get
            {
                return ((string)(this["SelectedLanguage"]));
            }
            set
            {
                this["SelectedLanguage"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("time")]
        public string TypingType
        {
            get
            {
                return ((string)(this["TypingType"]));
            }
            set
            {
                this["TypingType"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("15")]
        public int TimeTyping
        {
            get
            {
                return ((int)(this["TimeTyping"]));
            }
            set
            {
                this["TimeTyping"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("150")]
        public int WordsCount
        {
            get
            {
                return ((int)(this["WordsCount"]));
            }
            set
            {
                this["WordsCount"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AddPunctuation
        {
            get
            {
                return ((bool)(this["AddPunctuation"]));
            }
            set
            {
                this["AddPunctuation"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AddNumbers
        {
            get
            {
                return ((bool)(this["AddNumbers"]));
            }
            set
            {
                this["AddNumbers"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int ThemeType
        {
            get
            {
                return ((int)(this["ThemeType"]));
            }
            set
            {
                this["ThemeType"] = value;
            }
        }
    }
}
