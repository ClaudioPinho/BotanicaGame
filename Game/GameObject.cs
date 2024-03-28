using System.Collections.Generic;
using System.Linq;

namespace TestMonoGame.Game;

public class GameObject
{
    public readonly Transform Transform = new();

    protected List<Component> Components { get; } = new();

    public virtual void Initialize()
    {
        
    }

    public virtual void Update()
    {
        foreach (var component in Components)
        {
            component.Update();
        }
    }

    public void AddComponent<T>(T componentToAdd) where T : Component
    {
        Components.Add(componentToAdd);
        componentToAdd.Initialize();
    }

    public T GetComponent<T>() where T : Component
    {
        if (Components == null || Components.Count == 0)
            return null;
        var foundComponents = Components.Where(x => x is T).ToList();
        return Components.Count == 0 ? null : (T)foundComponents.First();
    }
    
}