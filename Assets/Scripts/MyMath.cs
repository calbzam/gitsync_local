using UnityEngine;
using Math = System.Math;

public static class MyMath
{
    public static float Remap(float value, float min1, float max1, float min2, float max2)
    {
        return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
    }

    public static float flatSqrMagnitude(Vector3 diff)
    {
        //return (float)(Math.Pow(diff.x, 2) + Math.Pow(diff.z, 2));
        return diff.x * diff.x + diff.z * diff.z;
    }

    public static void limit2dVelocity(Rigidbody rb, float moveSpeed)
    {
        Vector3 flat = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatSqrMagnitude(flat) > moveSpeed * moveSpeed)
        {
            flat = flat.normalized * moveSpeed; // limit maximum velocity to moveSpeed
            rb.velocity = new Vector3(flat.x, rb.velocity.y, flat.z);
        }
    }

    public static float lerp(float src, float trgt, float percent)
    {
        return src + percent * (trgt - src);
    }

    // https://discussions.unity.com/t/any-one-know-maths-behind-this-movetowards-function/65501/4
    public static float moveTowards(float current, float target, float maxDelta)
    {
        if (Math.Abs(target - current) < maxDelta) return target;
        return current + Math.Sign(target - current) * maxDelta;
    }

    public static float updatePercentDT(float current, float moveSpeed)
    {
        return moveTowards(current, 1f, moveSpeed * Time.deltaTime);
    }

    public static bool CheckAnimFinished(Animator anim)
    {
        return (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
    }

    // source: https://discussions.unity.com/t/check-if-layer-is-in-layermask/16007
    public static bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return layerMask == (layerMask | (1 << layer));
    }
}
