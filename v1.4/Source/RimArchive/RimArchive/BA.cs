using RimWorld;
using HarmonyLib;

namespace RimArchive
{
    /// <summary>
    /// Not what I had done. Maybe will be of use sometime later?
    /// </summary>
    public class BA_RaceProperties
    {
        public bool BA_Student
        {
            get
            {
                return this.intelligence >= Intelligence.BA_Student;
            }
        }

        public Intelligence intelligence { get; private set; }
        public enum Intelligence : byte
        {
            BA_Student
        }

    }

    
}
