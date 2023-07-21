

using System.Xml;

namespace RimArchive;
public class PreDefinedRelation : DefModExtension
{
    public List<Relation> relations = new List<Relation>();
    public class Relation
    {
        public PawnRelationDef def;
        public List<PawnKindDef> kinds;

        public void LoadDataFromXmlCustom(XmlNode xmlNode)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", xmlNode.Name);
            for(int i = 0; i< kinds.Count; i++)
            {
                DirectXmlCrossRefLoader.RegisterListWantsCrossRef(this.kinds, xmlNode.ChildNodes[i].InnerText);
            }
        }
    }

}

/*
 * <li Class="RimArchive.PreDefinedRelation">
 *      <relations>
 *          <Relation.defName>
 *              <li>PawnKindDef1</li>
 *              ...
 *          </Relation.defName>
 *      </relation>
 * </li>
 */

