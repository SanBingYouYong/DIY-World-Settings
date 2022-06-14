using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// an Id generator, designed first for the PointSystem in WorldSettingSceneControl; 
// singleton
public class IdGenerator
{
    private static IdGenerator generatorInstance;
    private string lastID;
    private int count;

    private IdGenerator()
    {
        lastID = "";
        count = 0;
    }

    public static IdGenerator getIdGenerator()
    {
        if (generatorInstance == null)
        {
            generatorInstance = new IdGenerator();
        }
        return generatorInstance;
    }


    public string getNextID()
    {
        generatorInstance.lastID = generatorInstance.count.ToString();
        generatorInstance.count++;
        return generatorInstance.lastID;
    }
}
