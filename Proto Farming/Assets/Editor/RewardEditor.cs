
using UnityEditor;

[CustomEditor(typeof(Reward))]
public class RewardEditor : Editor
{
    SerializedProperty levelToReceive;

    SerializedProperty rewardType;

    SerializedProperty itemRewards;

    SerializedProperty toolRewards;

    private void OnEnable()
    {
        levelToReceive = serializedObject.FindProperty("levelToReceive");

        rewardType = serializedObject.FindProperty("rewardType");

        itemRewards = serializedObject.FindProperty("itemRewards");

        toolRewards = serializedObject.FindProperty("toolRewards");
    }

    public override void OnInspectorGUI()
    {
        Reward reward = (Reward)target;

        serializedObject.Update();

        EditorGUILayout.PropertyField(levelToReceive);

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(rewardType);

        Reward.RewardType type = (Reward.RewardType)rewardType.enumValueIndex;

        // Hide and show inspector properties based on the reward type.
        switch (type)
        {
            case Reward.RewardType.Item:
                EditorGUILayout.PropertyField(itemRewards);
                break;

            case Reward.RewardType.Tool:
                EditorGUILayout.PropertyField(toolRewards);
                break;

            case Reward.RewardType.Passive:
                // TODO: Add Passive reward section.
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
