using UnityEngine;

public class EffectObject
{
    public GameObject Target;

    public EffekseerHandle? Handle;
    public string Name;
    public bool DoLoop;

    public EffectObject(GameObject _target, string _name, bool _doloop)
    {
        Target = _target;
        Name = _name;
        DoLoop = _doloop;
    }

}