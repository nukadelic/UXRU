using System;
using UnityEngine;

[System.Serializable]
public class XRAxis
{
    public static XRAxis positive( float activator, float max = 1f )
    {
        if( activator >= max ) throw new Exception("bad value");

        XRAxis axis = new XRAxis( 0, max, activator );

        axis.negative_activator = float.NegativeInfinity;

        axis.ignore_negative = true;

        return axis;
    }

    public static XRAxis negative( float activator, float min = -1f )
    {
        if( activator <= min ) throw new Exception("bad value");

        XRAxis axis = new XRAxis( min, 0, activator );

        axis.negative_activator = float.PositiveInfinity;

        axis.ignore_positive = true;

        return axis;
    }

    /// The minimume possible value for the input axis to clamp around
    public float min = -1f;

    /// The maximum possible value for the input axis to clamp around
    public float max = 1f;

    public float positive_activator = 0f;
    public float negative_activator = 0f;

    public bool ignore_positive = false;
    public bool ignore_negative = false;

    public XRAxis( float min = -1f, float max = 1f, float activator = 0f )
    {
        this.min = min;
        this.max = max;

        positive_activator = activator;
        negative_activator = activator;
    }

    /// Check if provided value is beyond the activator value
    public bool Active( float value )
    {
        return Direction( value ) != 0;
    }

    /// Get a -1, +1 or 0 as the return for the input value based on its direction.
    /// Note if the value didn't reach the activator the then 0 will be returned
    public int Direction( float value )
    {
        value = Clamp( value );

        if( ! ignore_positive && value >= positive_activator ) return 1;

        if( ! ignore_negative && value <= negative_activator ) return -1;

        return 0;
    }

    float Clamp( float value )
    {
        if( ignore_negative && ignore_positive ) return 0; 

        if( ignore_negative ) return Mathf.Clamp( value, 0, max );

        if( ignore_positive ) return Mathf.Clamp( value, min, 0 );

        return Mathf.Clamp( value, min, max );
    }

    /// Main function of this class, 
    public float Solve( float value )
    {
        value = Clamp( value );

        if( ignore_negative && ignore_positive ) return 0;



        if( ! ignore_positive && value >= positive_activator )
        {    
            // [ activator , max ] -> [ 0, max ]
            value = ( value - positive_activator ) / ( max - positive_activator );

            return value;
        }

        if( ! ignore_negative && value <= negative_activator )
        {
            // [ - activator , - 1 ] -> [ 0, - 1 ]
            value = ( value - negative_activator ) / ( min - negative_activator );

            return value;
        }

        return 0;
    } 
}