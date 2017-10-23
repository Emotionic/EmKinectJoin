using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FireWorksManager : MonoBehaviour
{
    public Color StartColor;

    private void Start()
    {
        var components = transform.GetComponentsInChildren<ParticleSystem>();
        foreach (var component in components)
        {
            component.startColor = StartColor;
        }

        // main
        var comp = transform.GetChild(2);

        GradientColorKey[] gradientColorKey = { new GradientColorKey(StartColor, 1f) };
        GradientAlphaKey[] gradientAlphaKey =
            {
            new GradientAlphaKey(1.0f, 0f),
            new GradientAlphaKey(0.5f, 0.5f),
            new GradientAlphaKey(1f, 1f)
            };

        Gradient gradient = new Gradient();
        gradient.SetKeys(gradientColorKey, gradientAlphaKey);

        ParticleSystem.MinMaxGradient color = new ParticleSystem.MinMaxGradient();
        color.mode = ParticleSystemGradientMode.Gradient;
        color.gradient = gradient;

        ParticleSystem.MainModule main = comp.GetComponent<ParticleSystem>().main;

        main.startColor = color;

        if(comp.GetChild(0).GetComponent<ParticleSystem>() != null)
        {
            ParticleSystem.MainModule sub = comp.GetChild(0).GetComponent<ParticleSystem>().main;
            sub.startColor = color;
        }
    }

    // Update is called once per frame
    void Update ()
    {
        
    }
}
