

using System;
using System.Collections.Generic;

public static class Extensions
{
    public static TSource MinOf<TSource>(
        this IEnumerable<TSource> source,
        Func<TSource, float> selector)
    {
        // Get the enumerator.
        var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            throw new InvalidOperationException("The source sequence is empty.");

        // Take the first value as a minimum.
        float minimum = selector(enumerator.Current);
        TSource current = enumerator.Current;

        // Go through all other values...
        while (enumerator.MoveNext())
        {
            float value = selector(enumerator.Current);
            if (value < minimum)
            {
                // A new minimum was found. Store it.
                minimum = value;
                current = enumerator.Current;
            }
        }

        // Return the minimum value.
        return current;
    }
}