using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Category", menuName = "HeadsUp/Category")]
public class Category : ScriptableObject
{
    public string categoryName;
    public Color categoryColor = Color.white;
    public Sprite categoryIcon;

    [TextArea(3, 10)]
    public List<string> words = new List<string>();
}
