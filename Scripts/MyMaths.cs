using Godot;

public static class MyMaths
{
    public static Vector2 SetJumpVector(float jumpAngleDegrees, float power, float baseJumpVelocity)
    {
        float adjustedAngle = jumpAngleDegrees - 90;
        float jumpAngleRadians = Mathf.DegToRad(adjustedAngle);
        
        float jumpVectorX = Mathf.Cos(jumpAngleRadians);
        float jumpVectorY = Mathf.Sin(jumpAngleRadians);
        
        Vector2 direction = new Vector2(jumpVectorX, jumpVectorY).Normalized();
        
        // Calculate the velocity needed to achieve the desired percentage of the jump distance
        float scaleFactor = Mathf.Sqrt(power / 100f);
        float actualVelocity = baseJumpVelocity * scaleFactor;
        
        return direction * actualVelocity;
    }
}