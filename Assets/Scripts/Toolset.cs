using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Toolset
{
    // a mimic of my earliest python code
    static char[] alphabet = "qwertyuiopasdfghjklzxcvbnm".ToCharArray();
    static char[] vowel = "aeiou".ToCharArray();
    static char[] consonant = "qwrtypsdfghjklzxcvbnm".ToCharArray();

    public static string GenerateRandomName(int length = 5)
    {
        List<char> name = new List<char>();
        //System.Random random = new(); // can be replaced with Unity's static Random

        for (int i = 0; i<length; i++)
        {
            var x = '_';
            if (i % 2 == 0)
            {
                x = vowel[Random.Range(0, vowel.Length)];
            }
            else
            {
                x = consonant[Random.Range(0, consonant.Length)];
            }
            name.Add(x);
        }
        name[0] = char.ToUpper(name[0]);
        return new string(name.ToArray());
    }
}
