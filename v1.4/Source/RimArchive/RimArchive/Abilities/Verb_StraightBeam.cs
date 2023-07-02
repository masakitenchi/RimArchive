namespace RimArchive;
public class Verb_StraightBeam : Verb_CastAbility
{
    //用OrbitStrike或者PowerBeam都会导致一个问题：放完了就是放完了
    //我需要让这个技能成为一个持续施法的技能，放完之前要原地罚站
    //但是还不能直接挪用原版的Verb到技能里，我真的哭死
}
