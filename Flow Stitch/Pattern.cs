using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Flow_Stitch
{
    //class for undo and redo funcionality
    public class Pattern
    {
        //stores image for undo and redo functionality
        List<WriteableBitmap> patternStates = new List<WriteableBitmap>();
        //the current index we are at in the patternStates list
        //starts at -1 to signal, that it is empty
        int currentIndex = -1;

        //adds a bitmap to the patternStates list, meaning there is a new action that can be undone
        public void patternStatesAdd(ref WriteableBitmap wBitmap)
        {
            //removing states from the list, if user does new actions after undo, basically overwriting the old redoable states
            if (currentIndex >= 0 && currentIndex != patternStates.Count() - 1)
            {
                int range = patternStates.Count() - (currentIndex + 1);

                patternStates.RemoveRange(currentIndex + 1, range);
            }

            //store state of image
            patternStates.Add(wBitmap);
            currentIndex++;
        }

        //check if there is anything to undo
        public bool hasPreviousState()
        {
            return currentIndex > 0;
        }

        //go back in the states, so undo action
        public WriteableBitmap GetPreviousState()
        {
            currentIndex--;
            return patternStates[currentIndex];
        }

        //check if there is anything to redo
        public bool hasNextState()
        {
            return currentIndex != patternStates.Count() - 1;
        }

        //redo an action
        public WriteableBitmap GetNextState()
        {
            currentIndex++;
            return patternStates[currentIndex];
        }

        //empty patternStates list, used if loading in a new image
        public void Clear()
        {
            patternStates.Clear();
            currentIndex = -1;
        }
    }
}
