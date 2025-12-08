using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages loading and caching of game categories from Resources folder
/// </summary>
public class CategoryLoader : MonoBehaviour
{
    public static CategoryLoader Instance { get; private set; }

    private List<Category> loadedCategories = new List<Category>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public List<Category> LoadAllCategories()
    {
        loadedCategories.Clear();
        LoadCategoriesFromJSON();

        Debug.Log($"Loaded {loadedCategories.Count} categories");
        return loadedCategories;
    }

    private void LoadCategoriesFromJSON()
    {
        TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>("Categories");

        foreach (TextAsset jsonFile in jsonFiles)
        {
            try
            {
                CategoryData data = JsonUtility.FromJson<CategoryData>(jsonFile.text);
                Category category = CreateCategoryFromData(data);
                loadedCategories.Add(category);
                Debug.Log($"✓ {category.categoryName} ({category.words.Count} words)");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load {jsonFile.name}: {e.Message}");
            }
        }
    }

    private Category CreateCategoryFromData(CategoryData data)
    {
        Category category = ScriptableObject.CreateInstance<Category>();
        category.categoryName = data.name;
        category.words = new List<string>(data.words);
        category.categoryColor = ColorUtility.TryParseHtmlString(data.color, out Color color)
            ? color
            : Color.white;

        return category;
    }

    public Category GetCategoryByName(string name)
    {
        return loadedCategories.FirstOrDefault(c =>
            c.categoryName.Equals(name, System.StringComparison.OrdinalIgnoreCase)
        );
    }
}

/// <summary>
/// Data structure for JSON category files
/// </summary>
[System.Serializable]
public class CategoryData
{
    public string name;
    public string color; // Hex color like "#FF5722"
    public string[] words;
}
