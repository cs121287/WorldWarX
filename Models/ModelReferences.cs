namespace WorldWarX
{
    /// <summary>
    /// This class provides direct type references to avoid ambiguity between
    /// classes in the root namespace and the Models namespace.
    /// </summary>
    public static class ModelReferences
    {
        public static Models.GameMap CreateGameMap(string name, int width, int height)
        {
            return new Models.GameMap(name, width, height);
        }

        public static Models.Unit CreateUnit(UnitType unitType, Models.Player owner)
        {
            return new Models.Unit(unitType, owner);
        }
    }
}