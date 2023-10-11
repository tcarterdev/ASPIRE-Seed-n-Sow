using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Recipe Data", menuName = "Recipe/Recipe Data", order = 1)]
public class RecipeData : ScriptableObject
{
    public int itemsCombinedValue;
    public ItemData resultItem;
    //public int craftNumber;
}
