using UnityEngine;

public static class VectorExtensions
{
    public static Quaternion ToQuaternion(this Windows.Kinect.Vector4 vector, Quaternion comp)
    {
        return Quaternion.Inverse(comp) * new Quaternion(-vector.X, -vector.Y, vector.Z, vector.W);
    }
}