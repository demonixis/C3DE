using C3DE;
using C3DE.Components;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using System;

public sealed class SinMovement : Behaviour
{
    private Vector3 m_Position = Vector3.Zero;
    private float m_Y = 0;

    public float Min { get; set; } = 15.5f;
    public float Max { get; set; } = 15.5f;
    public float Frequency { get; set; } = 0.25f;
    public float Phase { get; set; } = 0.0f;


    public override void Awake()
    {
        base.Awake();

        if (Phase == 0.0f)
            Phase = RandomHelper.Range(0.0f, 8000.0f);
    }

    public override void Update()
    {
        m_Position = transform.LocalPosition;
        m_Y = (Time.TotalTime + Phase) * Frequency;
        m_Y = m_Y - (float)Math.Floor((double)m_Y); // normalized value to 0..1
        m_Position.Y = (float)((Max * Math.Sin(2 * MathHelper.Pi * m_Y)) + Min);

        transform.LocalPosition = m_Position;
    }
}