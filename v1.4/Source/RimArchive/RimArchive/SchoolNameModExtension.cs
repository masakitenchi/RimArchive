using RimWorld;
using System.Xml;
using Verse;

namespace RimArchive
{
    public class SchoolNameModExtension : DefModExtension
    {
        public string SchoolName;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "SchoolName", xmlRoot.Name);
        }
    }
}
