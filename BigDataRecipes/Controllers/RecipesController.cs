using BigDataRecipes.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using Newtonsoft.Json;
using NuGet.Protocol;
using System;

namespace BigDataRecipes.Controllers
{
	public class RecipesController : Controller
	{
        private readonly IDriver _driver;

		public RecipesController()
		{
            _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "leaf123TREE"));
        }
        // GET: Display the form
        public ActionResult SelectIngredients()
		{
			// Simulating a list of ingredients from some data source
			ViewBag.Ingredients = new List<string> { "Flour", "Sugar", "Eggs", "Butter" };

			return View();
		}

        public async Task<ActionResult> FindRecipes(string selectedIngredientsList)
        {
            // Here, you would typically query your database for recipes matching the selected ingredients
            // For demonstration, let's simulate finding recipes
            // Here, you would typically query your database for recipes matching the selected ingredients
            // For demonstration, let's simulate finding recipes
            List<string> ingredients = new List<string>();
            ingredients = selectedIngredientsList.Split(',').ToList();
            List<string> recipes = await GetRecipes(ingredients);
            var allRecipes = new List<Recipe>
		{
			new Recipe { id = 1, name = "Recipe 1", ingredients = new List<string> { "Flour", "Sugar" } },
            // Add more recipes for testing
        };

            List<string> recipes2 = await GetRecipes(new List<string>() { "flour", "sugar"});


            return View(recipes);
		}

        public async Task<ActionResult> Properties(string name)
        {
            Recipe recipe = await GetRecipeProperties(name);
            return View(recipe);
        }

        public async Task<List<string>> GetRecipes(List<string> ingredients)
        {
            await using var session = _driver.AsyncSession();
            var recipes = await session.ExecuteReadAsync(
                async tx =>
                {
                    // Prepare a pattern to match ingredient names against the specified list
                    var ingredientPattern = "^(" + string.Join("|", ingredients) + ")$";

                    var result = await tx.RunAsync(
                        @"MATCH (recipe:Recipe)-[:USES]->(ingredient:Ingredient)
                WITH recipe, COLLECT(ingredient.name) AS ingredientsUsed
                WHERE ALL(ing IN ingredientsUsed WHERE ing =~ $ingredientPattern)
                RETURN recipe.name",
                        new { ingredientPattern });

                    var records = await result.ToListAsync();
                    return records.Select(record => record[0].As<string>()).Distinct().ToList();
                });

            return recipes;
        }


        public async Task<Recipe> GetRecipeProperties(string recipeName)
        {
            await using var session = _driver.AsyncSession();
            var recipeProperties = await session.ExecuteWriteAsync(
                async tx =>
                {
                    var result = await tx.RunAsync(
                        @"MATCH (recipe:Recipe)
                WHERE recipe.name = $recipeName
                RETURN recipe.name AS name, recipe.directions AS directions, recipe.ingredients_count AS ingredientsCount, recipe.source AS source
                LIMIT 1",
                        new { recipeName });

                    var record = await result.SingleAsync(); // Assuming recipe names are unique and you expect one result.
                    return new
                    {
                        Name = record["name"].As<string>(),
                        Directions = record["directions"].As<List<string>>(),
                        IngredientsCount = record["ingredientsCount"].As<List<string>>(),
                        Source = record["source"].As<string>()
                    };
                });

            // Assuming you have a Recipe class that matches the structure of the data you're retrieving.
            Recipe recipe = new Recipe()
            {
                name = recipeProperties.Name,
                directions = recipeProperties.Directions,
                ingredients = recipeProperties.IngredientsCount,
                source = recipeProperties.Source
            };

            Console.WriteLine(recipe); // Adjust based on how you want to output your Recipe object.
            return recipe;
        }


        public async Task<List<string>> IngredientsByString(string name)
        {
            await using var session = _driver.AsyncSession();
            var ingredients = await session.ExecuteWriteAsync(
                async tx =>
                {
                    var result = await tx.RunAsync(
                        @"MATCH (ingredient:Ingredient)
                WHERE ingredient.name =~ ('.*' + $name + '.*') 
                AND ingredient.name =~ '^[a-zA-Z0-9 ]+$'
                RETURN ingredient.name ORDER BY ingredient.name",
                new { name });

                    var records = await result.ToListAsync();
                    return records.Select(record => record[0].As<string>()).Distinct().ToList();
                });

            return ingredients;
        }
        public async Task<List<string>> RecipesByString(string name)
        {
            await using var session = _driver.AsyncSession();
            var ingredients = await session.ExecuteWriteAsync(
                async tx =>
                {
                    var result = await tx.RunAsync(
                        @"MATCH (recipe:Recipe)
                WHERE recipe.name =~ ('.*' + $name + '.*') 
                AND recipe.name =~ '^[a-zA-Z0-9 ]+$'
                RETURN recipe.name ORDER BY recipe.name",
                new { name });

                    var records = await result.ToListAsync();
                    return records.Select(record => record[0].As<string>()).Distinct().ToList();
                });

            return ingredients;
        }

        public IActionResult SelectRecipes()
        {
            return View();
        }

        public async Task<List<string>> GetSimilarRecipesAsync(List<string> selectedRecipes)
        {
            await using var session = _driver.AsyncSession();
            var similarRecipes = await session.ExecuteWriteAsync(async tx =>
            {
                var result = await tx.RunAsync(
                    @"MATCH (r:Recipe)-[:USES]->(i:Ingredient)<-[:USES]-(similar:Recipe)
                  WHERE r.name IN $selectedRecipes AND r.name <> similar.name
                  WITH similar, COUNT(i) AS sharedIngredients
                  ORDER BY sharedIngredients DESC
                  RETURN similar.name
                  LIMIT 10",
                    new { selectedRecipes });

                return await result.ToListAsync(record => record[0].As<string>());
            });

            return similarRecipes;
        }
        public async Task<IActionResult> GetSimilarRecipes(string selectedRecipes)
        {
            List<string> recipes = selectedRecipes.Split(',').ToList();
            List<string> similarRecipes = await GetSimilarRecipesAsync(recipes);
            TempData["Recipes"] = similarRecipes;
            return RedirectToAction("ShowRecipes");
        }

        public ActionResult ShowRecipes()
		{
			// Retrieve the matching recipes from TempData
			var recipes = TempData["Recipes"] as List<Recipe>;

			return View(recipes);
		}
	}
}
