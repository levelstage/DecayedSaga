namespace GameCore.Scripting;

public static class SkillRegistry
{
    public static IActiveSkill? ResolveActive(string skillClass)
    {
        if (string.IsNullOrWhiteSpace(skillClass)) return null;
        var type = Type.GetType(skillClass);
        if (type == null) return null;
        return Activator.CreateInstance(type) as IActiveSkill;
    }

    public static ITriggerSkill? ResolveTrigger(string skillClass)
    {
        if (string.IsNullOrWhiteSpace(skillClass)) return null;
        var type = Type.GetType(skillClass);
        if (type == null) return null;
        return Activator.CreateInstance(type) as ITriggerSkill;
    }
}
