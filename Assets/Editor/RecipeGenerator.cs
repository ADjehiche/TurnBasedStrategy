using UnityEngine;
using UnityEditor;
using System.IO;
using CardGame;

#if UNITY_EDITOR
/// <summary>
/// Editor tool to generate all 9 card merge recipes as ScriptableObjects
/// Matches the merge-only cards from CardGenerator.cs
/// </summary>
public static class RecipeGenerator
{
    private const string BasePath = "Assets/Resources/Recipes";
    private const string CardsPath = "Assets/Resources/Cards";
    
    [MenuItem("Tools/Card Game/Generate All Recipes")]
    public static void GenerateAllRecipes()
    {
        // Ensure folder structure
        EnsureFolder(BasePath);
        
        // Generate all 9 recipes
        CreateRecipe_Hemorrhage();           // 1) Quick Slash + Stab
        CreateRecipe_VanguardStrike();       // 2) Low Sweep + Quick Slash
        CreateRecipe_Whirlwind();            // 3) Quick Slash + Quick Slash
        CreateRecipe_Skewer();               // 4) Crossbow Bolt + Poison Arrow
        CreateRecipe_WeightedTip();          // 5) Improvised Bolt + Brawler's Jab
        CreateRecipe_DeepCuts();             // 6) Stab + Rend
        CreateRecipe_CounterSweep();         // 7) Low Sweep + Parry
        CreateRecipe_EvasiveManeuver();      // 8) Dodge + Quick Draw
        CreateRecipe_Execution();            // 9) Battle Focus + Lunging Thrust
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("✅ All 9 card recipes generated in Assets/Resources/Recipes/");
    }
    
    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string folder = Path.GetFileName(path);
            
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
                
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
    
    private static CardRecipe GetOrCreateRecipe(string recipeName)
    {
        string assetPath = Path.Combine(BasePath, recipeName + ".asset").Replace("\\", "/");
        CardRecipe recipe = AssetDatabase.LoadAssetAtPath<CardRecipe>(assetPath);
        
        if (recipe == null)
        {
            recipe = ScriptableObject.CreateInstance<CardRecipe>();
            AssetDatabase.CreateAsset(recipe, assetPath);
        }
        
        return recipe;
    }
    
    private static Card LoadCard(string folderName, string cardName)
    {
        string path = $"{CardsPath}/{folderName}/{cardName}.asset";
        Card card = AssetDatabase.LoadAssetAtPath<Card>(path);
        
        if (card == null)
        {
            Debug.LogError($"[RecipeGenerator] Card not found: {path}");
        }
        
        return card;
    }
    
    // ========== RECIPE DEFINITIONS ==========
    
    // Recipe 1: Hemorrhage (Quick Slash + Stab)
    private static void CreateRecipe_Hemorrhage()
    {
        var recipe = GetOrCreateRecipe("Recipe_Hemorrhage");
        recipe.ingredient1 = LoadCard("Attack", "Quick Slash");
        recipe.ingredient2 = LoadCard("Attack", "Stab");
        recipe.result = LoadCard("MergeOnly", "Hemorrhage");
        recipe.recipeDescription = "Combine speed and precision for devastating bleeding damage.";
        EditorUtility.SetDirty(recipe);
    }
    
    // Recipe 2: Vanguard Strike (Low Sweep + Quick Slash)
    private static void CreateRecipe_VanguardStrike()
    {
        var recipe = GetOrCreateRecipe("Recipe_VanguardStrike");
        recipe.ingredient1 = LoadCard("Attack", "Low Sweep");
        recipe.ingredient2 = LoadCard("Attack", "Quick Slash");
        recipe.result = LoadCard("MergeOnly", "Vanguard Strike");
        recipe.recipeDescription = "Offense and defense in perfect harmony.";
        EditorUtility.SetDirty(recipe);
    }
    
    // Recipe 3: Whirlwind (Quick Slash + Quick Slash)
    private static void CreateRecipe_Whirlwind()
    {
        var recipe = GetOrCreateRecipe("Recipe_Whirlwind");
        recipe.ingredient1 = LoadCard("Attack", "Quick Slash");
        recipe.ingredient2 = LoadCard("Attack", "Quick Slash");
        recipe.result = LoadCard("MergeOnly", "Whirlwind");
        recipe.recipeDescription = "Double the speed, strike all enemies at once.";
        EditorUtility.SetDirty(recipe);
    }
    
    // Recipe 4: Skewer (Crossbow Bolt + Poison Arrow)
    private static void CreateRecipe_Skewer()
    {
        var recipe = GetOrCreateRecipe("Recipe_Skewer");
        recipe.ingredient1 = LoadCard("Attack", "Crossbow Bolt");
        recipe.ingredient2 = LoadCard("Attack", "Poison Arrow");
        recipe.result = LoadCard("MergeOnly", "Skewer");
        recipe.recipeDescription = "Merge ranged attacks into a devastating piercing shot.";
        EditorUtility.SetDirty(recipe);
    }
    
    // Recipe 5: Weighted Tip (Improvised Bolt + Brawler's Jab)
    private static void CreateRecipe_WeightedTip()
    {
        var recipe = GetOrCreateRecipe("Recipe_WeightedTip");
        recipe.ingredient1 = LoadCard("Attack", "Improvised Bolt");
        recipe.ingredient2 = LoadCard("Attack", "Brawler's Jab");
        recipe.result = LoadCard("MergeOnly", "Weighted Tip");
        recipe.recipeDescription = "Combine bleed and tempo for sustained pressure.";
        EditorUtility.SetDirty(recipe);
    }
    
    // Recipe 6: Deep Cuts (Stab + Rend)
    private static void CreateRecipe_DeepCuts()
    {
        var recipe = GetOrCreateRecipe("Recipe_DeepCuts");
        recipe.ingredient1 = LoadCard("Attack", "Stab");
        recipe.ingredient2 = LoadCard("Attack", "Rend");
        recipe.result = LoadCard("MergeOnly", "Deep Cuts");
        recipe.recipeDescription = "Maximize bleeding damage with surgical precision.";
        EditorUtility.SetDirty(recipe);
    }
    
    // Recipe 7: Counter Sweep (Low Sweep + Parry)
    private static void CreateRecipe_CounterSweep()
    {
        var recipe = GetOrCreateRecipe("Recipe_CounterSweep");
        recipe.ingredient1 = LoadCard("Attack", "Low Sweep");
        recipe.ingredient2 = LoadCard("Defense", "Parry");
        recipe.result = LoadCard("MergeOnly", "Counter Sweep");
        recipe.recipeDescription = "Block and counter-attack in a single motion.";
        EditorUtility.SetDirty(recipe);
    }
    
    // Recipe 8: Evasive Maneuver (Dodge + Quick Draw)
    private static void CreateRecipe_EvasiveManeuver()
    {
        var recipe = GetOrCreateRecipe("Recipe_EvasiveManeuver");
        recipe.ingredient1 = LoadCard("Defense", "Dodge");
        recipe.ingredient2 = LoadCard("Utility", "Quick Draw");
        recipe.result = LoadCard("MergeOnly", "Evasive Maneuver");
        recipe.recipeDescription = "Become untouchable while drawing more options.";
        EditorUtility.SetDirty(recipe);
    }
    
    // Recipe 9: Execution (Battle Focus + Lunging Thrust)
    private static void CreateRecipe_Execution()
    {
        var recipe = GetOrCreateRecipe("Recipe_Execution");
        recipe.ingredient1 = LoadCard("Utility", "Battle Focus");
        recipe.ingredient2 = LoadCard("Attack", "Lunging Thrust");
        recipe.result = LoadCard("MergeOnly", "Execution");
        recipe.recipeDescription = "Channel all your power into a devastating finisher.";
        EditorUtility.SetDirty(recipe);
    }
    
    [MenuItem("Tools/Card Game/Validate Recipes")]
    public static void ValidateRecipes()
    {
        CardRecipe[] recipes = Resources.LoadAll<CardRecipe>("Recipes");
        
        Debug.Log($"=== RECIPE VALIDATION ({recipes.Length} recipes) ===");
        
        int validCount = 0;
        foreach (var recipe in recipes)
        {
            bool valid = recipe.IsValid();
            if (valid) validCount++;
            
            string status = valid ? "✅" : "❌";
            Debug.Log($"{status} {recipe.name}: {recipe.GetRecipeString()}");
        }
        
        Debug.Log($"=== {validCount}/{recipes.Length} recipes valid ===");
    }
}
#endif
