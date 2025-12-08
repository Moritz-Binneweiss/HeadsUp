using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Loads categories from JSON files in Resources/Categories folder
/// Supports both ScriptableObject categories and JSON categories
/// </summary>
public class CategoryLoader : MonoBehaviour
{
    public static CategoryLoader Instance { get; private set; }

    [Header("Category Sources")]
    public bool loadFromJSON = true;
    public bool loadFromScriptableObjects = true;

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

        if (loadFromJSON)
        {
            LoadCategoriesFromJSON();
        }

        if (loadFromScriptableObjects)
        {
            LoadCategoriesFromResources();
        }

        Debug.Log($"Loaded {loadedCategories.Count} categories total");
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
                Debug.Log(
                    $"Loaded JSON category: {category.categoryName} ({category.words.Count} words)"
                );
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load category from {jsonFile.name}: {e.Message}");
            }
        }
    }

    private void LoadCategoriesFromResources()
    {
        Category[] categories = Resources.LoadAll<Category>("Categories");

        foreach (Category category in categories)
        {
            // Check if not already loaded from JSON
            if (!loadedCategories.Any(c => c.categoryName == category.categoryName))
            {
                loadedCategories.Add(category);
                Debug.Log($"Loaded ScriptableObject category: {category.categoryName}");
            }
        }
    }

    private Category CreateCategoryFromData(CategoryData data)
    {
        Category category = ScriptableObject.CreateInstance<Category>();
        category.categoryName = data.name;
        category.words = new List<string>(data.words);

        // Parse color from hex
        if (ColorUtility.TryParseHtmlString(data.color, out Color color))
        {
            category.categoryColor = color;
        }
        else
        {
            category.categoryColor = Color.white;
        }

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
