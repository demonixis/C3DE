using C3DE;
using C3DE.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

public class SimplePath : Behaviour
{
    private List<Vector3> _paths = new List<Vector3>();
    private List<Vector3> _memPaths;
    private bool _beginStarted;
    
    public bool Loop { get; set; }
    public float MoveSpeed { get; set; }
    public float RotationSpeed { get; set; }
    public bool UpdateRotation { get; set; }

    public bool IsDone
    {
        get { return _paths.Count == 0; }
    }

    public event EventHandler<EventArgs> PathDone = null;

    public SimplePath()
        : base()
    {
        _beginStarted = false;
        _memPaths = new List<Vector3>();
        Loop = true;
        MoveSpeed = 10;
        RotationSpeed = 0.1f;
        UpdateRotation = false;
    }

    // Update is called once per frame
    public override void Update()
    {
        if (!_beginStarted && _paths.Count > 0 && MoveToPoint(Time.DeltaTime, _paths[0]))
        {
            _paths.RemoveAt(0);

            if (_paths.Count == 0)
            {
                if (PathDone != null)
                    PathDone(this, EventArgs.Empty);

                if (Loop)
                    _paths = new List<Vector3>(_memPaths);
            }
        }
    }

    public void Begin()
    {
        _paths.Clear();
        _beginStarted = true;
    }

    public void AddPath(Vector3 path)
    {
        if (_beginStarted)
            _paths.Add(path);
    }

    public void AddPath(Vector3 path, Transform transform)
    {
        if (_beginStarted)
            _paths.Add(transform.LocalPosition + path);
    }

    public void End()
    {
        if (_beginStarted)
        {
            _beginStarted = false;
            _memPaths = new List<Vector3>(_paths);
        }
    }

    public void Clear()
    {
        _memPaths.Clear();
        _paths.Clear();
    }

    private bool MoveToPoint(float elapsedTime, Vector3 target)
    {
        if (transform.LocalPosition == target)
            return true;

        Vector3 direction = Vector3.Normalize(target - transform.LocalPosition);
        transform.LocalPosition += direction * MoveSpeed * elapsedTime;

        //Vector3 lookDirection = target - transform.Position;

//        if (updateRotation)
  //          transform.Rotation = Quaternion.Slerp(transform.Rotation, Quaternion.LookRotation(lookDirection), rotationSpeed * Time.DeltaTime);

        if (Math.Abs(Vector3.Dot(direction, Vector3.Normalize(target - transform.LocalPosition)) + 1) < 0.1f)
            transform.LocalPosition = target;

        return transform.LocalPosition == target;
    }
}