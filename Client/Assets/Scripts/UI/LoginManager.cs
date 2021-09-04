using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class LoginManager : MonoBehaviour
{
    #region Singleton
    public static LoginManager instance;
    void Awake() { instance = this; }
    #endregion

    [Header("Signup Tabs")]
    public BotCheckClass BotCheck;
    public EmailSendClass EmailSend;
    public EmailChooseClass EmailChoose;
    public EmailReceiveClass EmailReceive;
    public SetUsernameClass SetUsername;
    public SetPasswordClass SetPassword;
    public CharacterCreationClass Skin;

    [Header("Login Tabs")]
    public BotCheckClass LoginBotCheck;
    public EmailSendClass LoginEmailSend;
    public SetPasswordClass LoginPassword;
    public EmailReceiveClass LoginEmailReceive;

    [Header("Misc")]
    public PremiumClass Premium;

    public static bool player_gender;
    public static float Height;

    public static float Fatness;
    public static float Muscles;
    public static float Slimness;
    public static float Thinness;
    public static float Breasts;

    public enum PasswordScoreEnum
    {
        Blank = 0,
        VeryWeak,
        Weak,
        Medium,
        Strong,
        VeryStrong
    }

    void Start()
    {
        SetGender(false);

        EmailSend.Error.gameObject.SetActive(false);
        EmailReceive.Error.gameObject.SetActive(false);

        SetUsername.Error.gameObject.SetActive(false);
        SetPassword.Error.gameObject.SetActive(false);

        SetPassword.Strength.text = "Blank";
        SetPassword.Strength.color = new Color32(255, 255, 255, 255);

        for (int i = 0; i < SetPassword.PasswordLevels.Length; i++)
        {
            SetPassword.PasswordLevels[i].Image.color = new Color32(35, 35, 35, 255);
        }
    }

    #region Signup Funcs

    public void StartBotCheck() { GameManager.ToggleUISection(2); ClientSend.SendEmpty(3); }
    public void SetBotCheckImg(Sprite sprite) { BotCheck.CheckImage.sprite = sprite; }
    public void SubmitBotCheck() { if (int.TryParse(BotCheck.CodeInput.text, out int code)) ClientSend.SendInt(4, code); }

    public void SubmitEmail()
    {
        if (EmailSend.Email.text == EmailSend.EmailCheck.text)
        {
            ClientSend.SendString(5, EmailSend.Email.text);
            GameManager.ToggleUISection(5);
        }
        else
        {
            EmailSend.Error.gameObject.SetActive(true);
            EmailSend.Error.text = "Bruh, the emails don't match!";
        }
    }

    public void SubmitEmailCode() { if (int.TryParse(EmailReceive.CodeInput.text, out int code)) ClientSend.SendInt(6, code); }
    public void SubmitUsername()
    {
        if (SetUsername.Username.text == SetUsername.UsernameCheck.text)
        {
            ClientSend.SendString(7, SetUsername.Username.text);
        }
        else
        {
            SetUsername.Error.gameObject.SetActive(true);
            SetUsername.Error.text = "Nah man, this doesn't match up!";
        }
    }

    public void SendPassword()
    {
        if (SetPassword.Password.text == SetPassword.PasswordCheck.text)
        {
            ClientSend.SendString(8, SetPassword.Password.text);
            GameManager.ToggleUISection(8);
        }
        else
        {
            SetPassword.Error.gameObject.SetActive(true);
            SetPassword.Error.text = "Bro, passwords don't match!";
        }
    }

    public void PasswordUpdate(string value)
    {
        int score = PasswordScore(value);
        for (int i = 0; i < SetPassword.PasswordLevels.Length; i++)
        {
            if (i <= score)
            {
                SetPassword.PasswordLevels[i].Image.color = SetPassword.PasswordLevels[score].Color;
            }
            else
            {
                SetPassword.PasswordLevels[i].Image.color = new Color32(35, 35, 35, 255);
            }
        }

        SetPassword.Strength.text = SetPassword.PasswordLevels[score].Value;
        SetPassword.Strength.color = SetPassword.PasswordLevels[score].Color;
    }

    int PasswordScore(string password)
    {
        int score = 1;

        if (password.Length < 1) return 0;
        if (password.Length < 4) return 1;

        if (password.Length >= 8) score++;
        if (password.Length >= 12) score++;

        if (Regex.Match(password, @"/\d+/", RegexOptions.ECMAScript).Success) { score++; }
        if (Regex.Match(password, @"/[a-z]/", RegexOptions.ECMAScript).Success &&
            Regex.Match(password, @"/[A-Z]/", RegexOptions.ECMAScript).Success) { score++; }
        if (Regex.Match(password, @"/.[!,@,#,$,%,^,&,*,?,_,~,-,£,(,)]/", RegexOptions.ECMAScript).Success) { score++; }

        return Mathf.Clamp(score, 0, 5);
    }

    public void SendSkin()
    {
        GameManager.ToggleUISection(9);
        ClientSend.SkinSend();
    }

    #endregion
    #region Character Creation

    public void SetGender(bool gender)
    {
        player_gender = gender;
        SkinManager.instance.SetGender(gender);

        Skin.HeadText.text = "0";
        Skin.HairText.text = "0";

        Skin.AccessoryText.text = "0";
        Skin.Breasts.value = 0;
        Skin.Fat.value = 0;
        Skin.HatText.text = "0";
        Skin.Muscles.value = 0;
        Skin.PantsText.text = "0";
        Skin.ShirtText.text = "0";
        Skin.ShoesText.text = "0";
        Skin.Slimness.value = 0;
        Skin.Thin.value = 0;

        Fatness = 0;
        Muscles = 0;
        Slimness = 0;
        Thinness = 0;
        Breasts = 0;

        if (gender)
        {
            Skin.Breasts.transform.parent.gameObject.SetActive(true);
            Skin.Slimness.transform.parent.gameObject.SetActive(true);

            Skin.Muscles.transform.parent.gameObject.SetActive(false);
            Skin.Thin.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            Skin.Breasts.transform.parent.gameObject.SetActive(false);
            Skin.Slimness.transform.parent.gameObject.SetActive(false);

            Skin.Muscles.transform.parent.gameObject.SetActive(true);
            Skin.Thin.transform.parent.gameObject.SetActive(true);
        }
    }

    public void LeftHead() { SkinManager.CC.PrevHead(); Skin.HeadText.text = SkinManager.CC.headActiveIndex.ToString(); }
    public void RightHead() { SkinManager.CC.NextHead(); Skin.HeadText.text = SkinManager.CC.headActiveIndex.ToString(); }

    public void LeftHair() { SkinManager.CC.PrevHair(); Skin.HairText.text = SkinManager.CC.hairActiveIndex.ToString(); }
    public void RightHair() { SkinManager.CC.NexHair(); Skin.HairText.text = SkinManager.CC.hairActiveIndex.ToString(); }

    public void LeftHat() { SkinManager.CC.Next(ClothesType.Hat); Skin.HatText.text = SkinManager.CC.ClothesID[ClothesType.Hat].ToString(); }
    public void RightHat() { SkinManager.CC.Next(ClothesType.Hat); Skin.HatText.text = SkinManager.CC.ClothesID[ClothesType.Hat].ToString(); }

    public void LeftShirt() { SkinManager.CC.Next(ClothesType.TShirt); Skin.ShirtText.text = SkinManager.CC.ClothesID[ClothesType.TShirt].ToString(); }
    public void RightShirt() { SkinManager.CC.Next(ClothesType.TShirt); Skin.ShirtText.text = SkinManager.CC.ClothesID[ClothesType.TShirt].ToString(); }

    public void LeftPants() { SkinManager.CC.Next(ClothesType.Pants); Skin.PantsText.text = SkinManager.CC.ClothesID[ClothesType.Pants].ToString(); }
    public void RightPants() { SkinManager.CC.Next(ClothesType.Pants); Skin.PantsText.text = SkinManager.CC.ClothesID[ClothesType.Pants].ToString(); }

    public void LeftShoes() { SkinManager.CC.Next(ClothesType.Shoes); Skin.ShoesText.text = SkinManager.CC.ClothesID[ClothesType.Shoes].ToString(); }
    public void RightShoes() { SkinManager.CC.Next(ClothesType.Shoes); Skin.ShoesText.text = SkinManager.CC.ClothesID[ClothesType.Shoes].ToString(); }

    public void LeftAccessory() { SkinManager.CC.Next(ClothesType.Accessory); Skin.AccessoryText.text = SkinManager.CC.ClothesID[ClothesType.Accessory].ToString(); }
    public void RightAccessory() { SkinManager.CC.Next(ClothesType.Accessory); Skin.AccessoryText.text = SkinManager.CC.ClothesID[ClothesType.Accessory].ToString(); }

    public void HeightValue(float value) { SkinManager.CC.SetHeight(Mathf.Clamp(value, -0.06f, 0.06f)); }
    
    public void FatValue(float value) { SkinManager.CC.SetBodyShape(BodyType.Fat, Mathf.Clamp(value, 0, 100)); Fatness = value; }
    public void MusclesValue(float value) { SkinManager.CC.SetBodyShape(BodyType.Muscles, Mathf.Clamp(value, 0, 100)); Muscles = value; }
    public void SlimnessValue(float value) { SkinManager.CC.SetBodyShape(BodyType.Slimness, Mathf.Clamp(value, 0, 100)); Slimness = value; }
    public void ThinValue(float value) { SkinManager.CC.SetBodyShape(BodyType.Thin, Mathf.Clamp(value, 0, 100)); Thinness = value; }
    public void BreastsValue(float value) { SkinManager.CC.SetBodyShape(BodyType.BreastSize, Mathf.Clamp(value, 0, 100), new string[] { "Chest" }, new ClothesType[] { ClothesType.TShirt }); Breasts = value; }

    #endregion

    #region Login Funcs

    public void StartLoginBotCheck() { ClientSend.SendEmpty(10); GameManager.ToggleUISection(11); }
    public void SetLoginBotCheckImg(Sprite sprite) { LoginBotCheck.CheckImage.sprite = sprite; }
    public void SubmitLoginBotCheck() { if (int.TryParse(LoginBotCheck.CodeInput.text, out int code)) ClientSend.SendInt(11, code); }

    public void LoginEmail()
    {
        if (LoginEmailSend.Email.text == LoginEmailSend.EmailCheck.text)
        {
            ClientSend.SendString(12, LoginEmailSend.Email.text);
        }
        else
        {
            LoginEmailSend.Error.gameObject.SetActive(true);
            LoginEmailSend.Error.text = "That's a lil sus -_-, them emails don't match";
        }
    }

    public void LoginPasswordSend()
    {
        if (LoginPassword.Password.text == LoginPassword.PasswordCheck.text)
        {
            ClientSend.SendString(13, LoginPassword.Password.text);
        }
        else
        {
            LoginPassword.Error.gameObject.SetActive(true);
            LoginPassword.Error.text = "Password not matching up man!";
        }
    }

    public void LoginEmailCodeReceive() { if (int.TryParse(LoginEmailReceive.CodeInput.text, out int code)) ClientSend.SendInt(14, code); }

    #endregion
    #region Misc

    public void ToLobby() { DataManager.LoadLobby(); }

    #endregion

    #region UI Storage Classes

    [Serializable]
    public class PremiumClass
    {
        public Button Continue;
    }


    [Serializable]
    public class BotCheckClass
    {
        public Image CheckImage;
        public TextMeshProUGUI TriesLeft;

        [Space]

        public TMP_InputField CodeInput;
        public Button SubmitCode;
    }
    [Serializable]
    public class EmailSendClass
    {
        public TMP_InputField Email;
        public TMP_InputField EmailCheck;

        [Space]

        public TextMeshProUGUI Error;
        public Button Submit;
    }
    [Serializable]
    public class EmailChooseClass
    {
        public Toggle[] EmailOptions;
        public Button ApplySettings;
        public Button UseDefaults;
    }
    [Serializable]
    public class EmailReceiveClass
    {
        public TMP_InputField CodeInput;
        public TextMeshProUGUI Error;
        public Button Submit;
    }
    [Serializable]
    public class SetUsernameClass
    {
        public TMP_InputField Username;
        public TMP_InputField UsernameCheck;

        [Space]

        public TextMeshProUGUI Error;
        public Button Submit;
    }
    [Serializable]
    public class SetPasswordClass
    {
        public TMP_InputField Password;
        public TMP_InputField PasswordCheck;

        [Space]

        public TextMeshProUGUI Error;
        public TextMeshProUGUI Strength;
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
        public TextMeshProUGUI HeadText;
        public TextMeshProUGUI HairText;

        [Space]

        public TextMeshProUGUI HatText;
        public TextMeshProUGUI AccessoryText;
        public TextMeshProUGUI ShirtText;
        public TextMeshProUGUI PantsText;
        public TextMeshProUGUI ShoesText;

        [Space]

        public Slider Muscles;
        public Slider Slimness;
        public Slider Thin;
        public Slider Breasts;

        [Space]

        public Slider Height;
        public Slider Fat;
    }

    #endregion
    #region Storage Classes

    [System.Serializable]
    public class SkinData
    {
        public bool Gender; // False - Male, True - Female

        [Space]

        public float Muscles;
        public float Fat;
        public float Thinness;

        [Space]

        public float Slimness;
        public float Breast;

        [Space]

        public float Height;

        [Space]

        public int Head;
        public int Hair;
        public int Skin;

        [Space]

        public int Hat;
        public int Accessory;
        public int Shirt;
        public int Pants;
        public int Shoes;
    }

    #endregion
}
