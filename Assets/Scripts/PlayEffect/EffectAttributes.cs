using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Kinect;
using UnityEngine;

class EffectAttributes
{
    public double Threshold { get; private set; }
    public JointType AttachPosition { get; private set; }
    public EffectType Type { get; private set; }
    public Vector3 Scale { get; private set; }

    public string EffectName { get; private set; }
    public Emotionic.Effect EffectKey { get; private set; }
    
    public EffectAttributes(double threshold, JointType jt, float scale)
    {
        Threshold = threshold;
        AttachPosition = jt;
        Scale = new Vector3(scale, scale, scale);
    }

    public EffectAttributes(double threshold, JointType jt, float scale, string name)
        : this(threshold, jt, scale)
    {
        EffectName = name;
        Type = EffectType.Effekseer;
    }

    public EffectAttributes(double threshold, JointType jt, float scale,Emotionic.Effect effectKey)
        : this (threshold, jt, scale)
    {
        EffectKey = effectKey;
        Type = EffectType.ParticleSystem;
    }

    public enum EffectType
    {
        Effekseer,
        ParticleSystem,
    }
}