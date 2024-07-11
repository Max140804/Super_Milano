using System;

namespace DominoTemplate.Core
{
    [Serializable]
    public class Domino
    {
        public int id;
        public int TopIndex;
        public int BottomIndex;

        public bool Available;
        public bool PortraitOrientation;
    }
}