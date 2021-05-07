using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Kmeans
{
    class PointsDataCalc
    {
        public ManualResetEvent DoneEvent { get; private set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public PointsDataCalc(ManualResetEvent doneEvent, int startIndex, int endIndex)
        {
            DoneEvent = doneEvent;
            StartIndex = startIndex;
            EndIndex = endIndex;
        }
    }
}
