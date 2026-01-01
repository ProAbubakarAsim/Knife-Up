using UnityEngine;
using UnityEngine.UI;

public class CharacterInfo : MonoBehaviour
{
    [HideInInspector]
    public int characterSequenceNumber;
    [Header("This field must be different than the others")]
    public string characterName;
    [Header("Price of the character")]
    public int characterPrice;
    [Header("Is the character free ?")]
    public bool isFree = false;

    private CharacterHandler charHandler = null;
    private Image knifeImg = null;
    private Image buttonImg = null;
    private Color originalColor = Color.white;
    public bool IsUnlocked
    {
        get
        {
            return (isFree || PlayerPrefs.GetInt(characterName, 0) == 1);
        }
    }

    private void Awake()
    {
        characterName = characterName.ToUpper();
    }

    private void Start()
    {
        charHandler = FindObjectOfType<CharacterHandler>();
        knifeImg = transform.GetChild(0).GetComponent<Image>();
        buttonImg = GetComponent<Image>();
        originalColor = buttonImg.color;

        UpdateKnifeImg();

        if (characterSequenceNumber == CharacterManager.Instance.SelectedIndex)
            buttonImg.color = charHandler.selectedKnifeColor;
    }

    public bool Unlock()
    {
        if (IsUnlocked)
            return true;

        if (OnefallGames.CoinManager.Instance.Coins >= characterPrice)
        {
            PlayerPrefs.SetInt(characterName, 1);
            PlayerPrefs.Save();
            OnefallGames.CoinManager.Instance.RemoveCoins(characterPrice);

            return true;
        }

        return false;
    }

    public void HandleOnClick()
    {
        charHandler.SetSelectedKnife(knifeImg.sprite, this);
        charHandler.SetNormalColor();
        buttonImg.color = charHandler.selectedKnifeColor;
    }

    /// <summary>
    /// Set this button's color back to original color
    /// </summary>
    public void SetNormalColor()
    {
        buttonImg.color = originalColor;
    }


    public void UpdateKnifeImg()
    {
        if (!IsUnlocked)
            knifeImg.color = Color.black;
        else
            knifeImg.color = Color.white;
    }
}
