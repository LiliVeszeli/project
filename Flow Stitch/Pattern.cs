using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Flow_Stitch
{
    public class Pattern
    {
        //stores image for undo and redo functionality
        List<WriteableBitmap> patternStates = new List<WriteableBitmap>();
        int currentIndex = -1;

        public void patternStatesAdd(ref WriteableBitmap wBitmap)
        {

            if (currentIndex >= 0 && currentIndex != patternStates.Count() - 1)
            {
                int range = patternStates.Count() - (currentIndex + 1);

                patternStates.RemoveRange(currentIndex + 1, range);
            }

            //store state of image
            patternStates.Add(wBitmap);
            currentIndex++;
        }

        public bool hasPreviousState()
        {
            return currentIndex > 0;
        }

        public WriteableBitmap GetPreviousState()
        {
            currentIndex--;
            return patternStates[currentIndex];
        }

        public bool hasNextState()
        {
            return currentIndex != patternStates.Count() - 1;
        }

        public WriteableBitmap GetNextState()
        {
            currentIndex++;
            return patternStates[currentIndex];
        }

        public void Clear()
        {
            patternStates.Clear();
            currentIndex = -1;
        }
    }
}
