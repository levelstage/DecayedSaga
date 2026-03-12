using GameCore.Scripting;

namespace GameCore.Scripting;

public static class SkillRegistry
{
    public static IActiveSkill? Resolve(string skillClass)
    {
        if (string.IsNullOrWhiteSpace(skillClass)) return null;
        var type = Type.GetType(skillClass);
        if (type == null) return null;
        return Activator.CreateInstance(type) as IActiveSkill;
    }
}
