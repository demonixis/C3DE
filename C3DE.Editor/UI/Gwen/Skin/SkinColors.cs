using System;

namespace Gwen.Skin
{
    /// <summary>
    /// UI colors used by skins.
    /// </summary>
    public struct SkinColors
    {
        public struct _Window
        {
            public Color TitleActive;
            public Color TitleInactive;
        }

        public struct _Button
        {
            public Color Normal;
            public Color Hover;
            public Color Down;
            public Color Disabled;
        }

        public struct _Tab
        {
            public struct _Inactive
            {
                public Color Normal;
                public Color Hover;
                public Color Down;
                public Color Disabled;
            }

            public struct _Active
            {
                public Color Normal;
                public Color Hover;
                public Color Down;
                public Color Disabled;
            }

            public _Inactive Inactive;
            public _Active Active;
        }

        public struct _Label
        {
            public Color Default;
            public Color Bright;
            public Color Dark;
            public Color Highlight;
        }

		public struct _TextBox
		{
			public Color Text;
			public Color Background_Selected;
			public Color Caret;
		}

		public struct _ListBox
		{
			public Color Text_Normal;
			public Color Text_Selected;
		}

        public struct _Tree
        {
            public Color Lines;
            public Color Normal;
            public Color Hover;
            public Color Selected;
        }

        public struct _Properties
        {
            public Color Line_Normal;
            public Color Line_Selected;
            public Color Line_Hover;
            public Color Column_Normal;
            public Color Column_Selected;
            public Color Column_Hover;
            public Color Label_Normal;
            public Color Label_Selected;
            public Color Label_Hover;
            public Color Border;
            public Color Title;
        }

        public struct _Category
        {
            public Color Header;
            public Color Header_Closed;

            public struct _Line
            {
                public Color Text;
                public Color Text_Hover;
                public Color Text_Selected;
                public Color Button;
                public Color Button_Hover;
                public Color Button_Selected;
            }

            public struct _LineAlt
            {
                public Color Text;
                public Color Text_Hover;
                public Color Text_Selected;
                public Color Button;
                public Color Button_Hover;
                public Color Button_Selected;
            }

            public _Line Line;
            public _LineAlt LineAlt;
        }

		public struct _GroupBox
		{
			public Color Dark;
			public Color Light;
		}

        public Color ModalBackground;
        public Color TooltipText;

        public _Window Window;
        public _Button Button;
        public _Tab Tab;
        public _Label Label;
		public _TextBox TextBox;
		public _ListBox ListBox;
        public _Tree Tree;
        public _Properties Properties;
        public _Category Category;
		public _GroupBox GroupBox;
    }
}
