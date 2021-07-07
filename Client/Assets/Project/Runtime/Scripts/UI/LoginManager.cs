using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoginManager : MonoBehaviour
{
    public SignupTabsClass SignupTabs;

    [Space]

    public PasswordScoreEnum PasswordScore;
    public enum PasswordScoreEnum
    {
        Blank = 0,
        VeryWeak,
        Weak,
        Medium,
        Strong,
        VeryStrong
    }

    [Serializable]
    public class SignupTabsClass
    {
        public TermsClass Terms;
        [Serializable]
        public class TermsClass
        {
            public GameObject Tab;
            public Toggle[] Checkboxes;

            [Space]

            public Button Decline;
            public Button Accept;
        }

        public BotCheckClass BotCheck;
        [Serializable]
        public class BotCheckClass
        {
            public GameObject Tab;
            public Image CheckImage;
            public TextMeshPro TriesLeft;

            [Space]

            public TMP_InputField CodeInput;
            public Button SubmitCode;
        }

        public EmailSendClass EmailSend;
        [Serializable]
        public class EmailSendClass
        {
            public GameObject Tab;
            public TMP_InputField Email;
            public TMP_InputField EmailCheck;

            [Space]

            public Button EmailsChoose;
            public TextMeshPro Error;
            public Button Submit;
        }

        public EmailChooseClass EmailChoose;
        [Serializable]
        public class EmailChooseClass
        {
            public GameObject Tab;
            public Toggle[] EmailOptions;

            [Space]

            public Button ApplySettings;
            public Button UseDefaults;
        }

        public EmailReceiveClass EmailReceive;
        [Serializable]
        public class EmailReceiveClass
        {
            public GameObject Tab;
            public TMP_InputField CodeInput;

            [Space]

            public TextMeshPro Error;
            public Button Submit;
        }

        public SetUsernameClass SetUsername;
        [Serializable]
        public class SetUsernameClass
        {
            public GameObject Tab;
            public TMP_InputField Username;
            public TMP_InputField UsernameCheck;

            [Space]

            public TextMeshPro Error;
            public Button Submit;
        }

        public SetPasswordClass SetPassword;
        [Serializable]
        public class SetPasswordClass
        {
            public GameObject Tab;
            public TMP_InputField Password;
            public TMP_InputField PasswordCheck;

            [Space]

            public TextMeshPro Error;
            public Button Submit;

            [Space]

            public PasswordLevelClass[] PasswordLevels;
            [Serializable]
            public class PasswordLevelClass
            {
                public PasswordScoreEnum Score;
                public string Value;

                [Space]

                public Color32 Color;
                public Image Image;
            }
        }

        [Serializable]
        public class CharacterCreationClass
        {
            public GameObject Tab;

            [Space]

            public Button Left;
            public Button Right;
            public Button Submit;

            public CharacterClass[] Characters;
            [Serializable]
            public class CharacterClass
            {
                [Header("Leave at '-1' to use index")]
                public int ID = -1;

                [Space]

                public string Name;
                public string Description;
                public Sprite Image;
            }
        }
    }
}
