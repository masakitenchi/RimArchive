using RimWorld;
using System.Collections;
using Unity;
using UnityEngine;

namespace RimArchive;

public class Coroutine : MonoBehaviour
{

    void Update()
    {

    }

    public IEnumerator PawnGroupGenerator(PawnGroupMakerParms pawnGroupMakerParms, int number = 1)
    {
        yield return new WaitForSeconds(number);
    }
}