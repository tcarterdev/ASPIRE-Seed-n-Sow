/// <summary>
/// Stores the XP values for each activity in one place. Scripts will reference this, 
/// to get the value of the XP for that activity. 
/// 
/// More activities and XP can be added as needed. 
/// </summary>
public enum XPValues
{
    BUILD = 5,
    HARVEST = 100,
    COLLECT = 10,
    COOK = 25,
    TEND = 50,
    WALK = 8,    // This is multiplied by the distance travelled.
    QUEST = 250
}