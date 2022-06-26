using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// an Id generator, designed first for the PointSystem in WorldSettingSceneControl; 
// singleton
public class IdGenerator
{
    private static IdGenerator generatorInstance;
    
    private string lastPointSystemID;
    private int pointSystemCount;

    private string lastSubOriginID;
    private int subOriginCount;

    private IdGenerator()
    {
        lastPointSystemID = "";
        pointSystemCount = 0;

        lastSubOriginID = "";
        subOriginCount = 0;
    }

    public static IdGenerator getIdGenerator()
    {
        if (generatorInstance == null)
        {
            generatorInstance = new IdGenerator();
        }
        return generatorInstance;
    }


    public string getNextPointSystemID()
    {
        generatorInstance.lastPointSystemID = generatorInstance.pointSystemCount.ToString();
        generatorInstance.pointSystemCount++;
        return generatorInstance.lastPointSystemID;
    }

    public string getNextSubOriginID()
    {
        generatorInstance.lastSubOriginID = generatorInstance.subOriginCount.ToString();
        generatorInstance.subOriginCount++;
        return generatorInstance.lastSubOriginID;
    }

    // TODO: after loading some world, it should resume from where the id enumerator stopped last time
}
