using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NutriDex : WorkStation
{
    [Header("Nutridex Values")]
    
    [SerializeField] private GameObject ndInterface;
    [SerializeField] private Button pageForwardButton;
    [SerializeField] private Button pageBackButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text recipeName;
    [SerializeField] private TMP_Text recipeDescription;
    [SerializeField] private TMP_Text replensishText;
    [SerializeField] private Image[] ingredientsImages;
    [SerializeField] private Sprite emptySprite;

    [Space]
    [SerializeField] private int index;
    [SerializeField] private ItemData[] meals;


    public override void InteractWithWorkStation()
    {
        //gameplayUI.SetActive(false);
        ndInterface.SetActive(true);
        
        DisplayNewPage(meals[index]);
    }

    public void TurnPageForward()
    {
        index += 1;
        if (index >= meals.Length)
        {
            index = 0;
        }
        DisplayNewPage(meals[index]);
    }

    public void TurnPageBackward()
    {
        index -= 1;
        if (index < 0) { index = meals.Length - 1;}
        DisplayNewPage(meals[index]);
    }

    public void DisplayNewPage(ItemData newItem)
    {
        recipeName.SetText(newItem.name);
        recipeDescription.SetText(newItem.itemDescription);
        iconImage.sprite = newItem.itemIcon;
        replensishText.SetText("Nutritional Value: " + newItem.replenishAmount.ToString());

        int index = 0;
        foreach (Image image in ingredientsImages)
        {
            if (index >= newItem.ingridents.Length) {
                image.sprite = emptySprite;
                index ++;
                continue;
            }

            image.sprite = newItem.ingridents[index];
            index ++;
        }
    }

    public void CloseNurtDex()
    {
        ndInterface.SetActive(false);
        //gameplayUI.SetActive(true);
    }
}
