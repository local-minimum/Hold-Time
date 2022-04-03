using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
struct PlayerAgeSetting
{
    public int fromAge;
    public bool hasGlasses;
    public bool hasCane;
    public bool hasAdultHair;
    public bool hasPunkCrest;
    public Color pantsColor;
    public Color mainHairColor;
    public Color crestHairColor;
}


public class PlayerAgeVisualizer : MonoBehaviour
{
    [SerializeField, Tooltip("First setting is always from age 0")]
    PlayerAgeSetting[] settings;

    [SerializeField]
    SpriteRenderer[] pants;

    [SerializeField]
    GameObject cane;

    [SerializeField]
    GameObject glasses;

    [SerializeField]
    GameObject adultHair;

    [SerializeField]
    SpriteRenderer sideHair;

    [SerializeField]
    SpriteRenderer crestHair;

    private void OnEnable()
    {
        Age.OnNewAge += HandleNewAge;
    }

    private void OnDisable()
    {
        Age.OnNewAge -= HandleNewAge;
    }

    private void HandleNewAge(int years)
    {
        for (int i = 0; i<settings.Length; i++)
        {
            if (settings[i].fromAge > years)
            {
                SetAge(settings[Mathf.Max(0, i - 1)]);
                return;
            }
        }
        SetAge(settings[settings.Length - 1]);
    }

    private void SetAge(PlayerAgeSetting settings)
    {
        adultHair.SetActive(settings.hasAdultHair);
        sideHair.gameObject.SetActive(!settings.hasAdultHair);
        crestHair.gameObject.SetActive(!settings.hasAdultHair && settings.hasPunkCrest);
        glasses.SetActive(settings.hasGlasses);
        cane.SetActive(settings.hasCane);

        for (int i = 0; i<pants.Length; i++)
        {
            pants[i].color = settings.pantsColor;
        }

        sideHair.color = settings.mainHairColor;
        crestHair.color = settings.crestHairColor;
    }

    private void Start()
    {
        SetAge(settings[0]);
    }
}
