using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using OnefallGames;

public class CharacterHandler : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private Color lockedColor = Color.black;
    [SerializeField] private Color unlockedColor = Color.white;
    public Color selectedKnifeColor = Color.white;

    [Header("Object References")]
    [SerializeField] private SpriteRenderer selectedKnife;
    [SerializeField] private ParticleSystem unlockedParticle;
    [Header("UI Stuff")]
    [SerializeField] private GridLayoutGroup contentGrid;
    [SerializeField] private Text totalCoinTxt;
    [SerializeField] private Text characterPriceTxt;
    [SerializeField] private GameObject characterPriceUI;
    [SerializeField] private GameObject selectBtn;
    [SerializeField] private GameObject unlockBtn;

    private CharacterInfo selectedCharInfo = null;
    // Use this for initialization
    void Start()
    {
        //Instantiate characters
        for (int i = 0; i < CharacterManager.Instance.characters.Length; i++)
        {
            GameObject character = Instantiate(CharacterManager.Instance.characters[i], Vector2.zero, Quaternion.identity);
            character.transform.SetParent(contentGrid.transform);
            character.GetComponent<CharacterInfo>().characterSequenceNumber = i;
        }


        selectedCharInfo = CharacterManager.Instance.characters[CharacterManager.Instance.SelectedIndex].GetComponent<CharacterInfo>();
        selectedKnife.sprite = selectedCharInfo.transform.GetChild(0).GetComponent<Image>().sprite;

        totalCoinTxt.text = CoinManager.Instance.Coins.ToString();

        unlockedParticle.gameObject.SetActive(false);
    }


    private IEnumerator PlayUnlockedParticle()
    {
        unlockedParticle.gameObject.SetActive(true);
        unlockedParticle.Play();
        yield return new WaitForSeconds(unlockedParticle.main.startLifetimeMultiplier);
        unlockedParticle.gameObject.SetActive(false);
    }

    ////////////////////////////////////////// Publish functions

    public void UnlockBtn()
    {
        if (selectedCharInfo.Unlock())
        {
            //Use this code for 3d character
            SoundManager.Instance.PlaySound(SoundManager.Instance.unlock);
            StartCoroutine(PlayUnlockedParticle());
            SetSelectedKnife(selectedKnife.sprite, selectedCharInfo);
            selectedCharInfo.UpdateKnifeImg();
            totalCoinTxt.text = CoinManager.Instance.Coins.ToString();
        }
    }

    public void SelectBtn()
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.button);
        CharacterManager.Instance.SelectedIndex = selectedCharInfo.characterSequenceNumber;
        BackBtn();
    }

    public void BackBtn()
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.button);
        SceneManager.LoadScene("Gameplay");
    }

    public void SetSelectedKnife(Sprite sp, CharacterInfo charInfor)
    {
        selectedCharInfo = charInfor;
        selectedKnife.sprite = sp;

        if (charInfor.IsUnlocked) //The character is already unlocked
        {
            selectedKnife.color = unlockedColor;

            //Handle UI
            characterPriceUI.SetActive(true);
            characterPriceTxt.text = charInfor.characterPrice.ToString();

            unlockBtn.SetActive(false);
            selectBtn.SetActive(true);

        }
        else //The character wasn't unlocked
        {
            selectedKnife.color = lockedColor;

            //Handle UI
            characterPriceUI.SetActive(true);
            characterPriceTxt.text = charInfor.characterPrice.ToString();

            if (charInfor.characterPrice > CoinManager.Instance.Coins)
            {
                unlockBtn.SetActive(false);
            }
            else
            {
                unlockBtn.SetActive(true);
            }
            selectBtn.SetActive(false);
        }
    }


    /// <summary>
    /// Set color of all knife button back to original 
    /// </summary>
    public void SetNormalColor()
    {
        CharacterInfo[] chars = FindObjectsOfType<CharacterInfo>();
        foreach(CharacterInfo o in chars)
        {
            o.SetNormalColor();
        }
    }
}
