namespace BigDataRecipes.Models
{
    public class Recipe
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<string> ingredients {  get; set; } = new List<string>();
        public List<string> directions {  get; set; } = new List<string>();
        public string source { get; set; }
    }
}
