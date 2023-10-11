/// <summary>
/// Stores the indexes for each scene in one place. Scripts will reference this, 
/// to get the index of the scene it is checking against, or loading to. 
/// 
/// These should match those that are in the build settings. Update the indexes here,
/// so the indexes across all the scripts will be correct. 
/// 
/// More indexes can be added as needed. 
/// </summary>
public enum SceneIndexes
{
    FIREBASE_INIT = 0,
    TITLE_SCREEN = 1,
    FARM_MODE = 2, 
    FARM_HOUSE_INTERIOR = 3,
    VENTURE_MODE = 4
}
