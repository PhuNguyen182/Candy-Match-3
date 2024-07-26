namespace CandyMatch3.Scripts.LevelDesign.LevelBuilder
{
    public class LevelFolderClassifyer
    {
        public static string GetLevelRangeFolderName(int level)
        {
            int mod = level % 100;
            int div = level / 100;
            string levelRange;

            if (mod != 0)
                levelRange = $"Level_{div * 100 + 1}_{(div + 1) * 100}";
            else
                levelRange = $"Level_{(div - 1) * 100 + 1}_{div * 100}";

            return levelRange;
        }
    }
}
