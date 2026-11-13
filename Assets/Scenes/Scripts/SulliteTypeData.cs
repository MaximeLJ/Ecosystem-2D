using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SulliteTypeData", menuName = "Sullite/SulliteTypeData", order = 1)]
public class SulliteTypeData : ScriptableObject
{
    public bool createMammal;
    public bool createHermaphrodite;
    public bool createProtandrous;
    public bool createProtogynous;
}
