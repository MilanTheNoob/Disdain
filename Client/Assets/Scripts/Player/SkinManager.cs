using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinManager : MonoBehaviour
{
    #region Singleton
    public static SkinManager instance;
    void Awake() { instance = this; }
    #endregion

    public CharacterCustomization Male;
    public CharacterCustomization Female;

    public static CharacterCustomization CC;

    private void Start()
    {
        SetGender(false);
    }

    public void SetGender(bool gender)
    {
        Male.gameObject.SetActive(!gender);
        Female.gameObject.SetActive(gender);

        CC = gender ? Female : Male;

        CC.SetHeadByIndex(0);
        CC.SetHairByIndex(0);

        CC.SetElementByIndex(ClothesType.Hat, 0);
        CC.SetElementByIndex(ClothesType.TShirt, 0);
        CC.SetElementByIndex(ClothesType.Pants, 0);
        CC.SetElementByIndex(ClothesType.Shoes, 0);
        CC.SetElementByIndex(ClothesType.Accessory, 0);

        CC.SetHeight(0);
        CC.SetBodyShape(BodyType.Fat, 0);
        CC.SetBodyShape(BodyType.Muscles, 0);
        CC.SetBodyShape(BodyType.Slimness, 0);
        CC.SetBodyShape(BodyType.Thin, 0);
        CC.SetBodyShape(BodyType.BreastSize, 0, new string[] { "Chest" }, new ClothesType[] { ClothesType.TShirt });
    }
}
