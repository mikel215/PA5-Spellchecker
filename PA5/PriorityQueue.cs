using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PA5
{
    // Implements list based
    class PriorityQueue
    {
        //Fields
        private List<WordNode> _priorityQueue;

        // Constructor
        public PriorityQueue()
        {
            _priorityQueue = new List<WordNode>();
        }

        // Methods
        public void Insert(WordNode node)
        {
            int size = _priorityQueue.Count;
            
            // empty pq
            if(size == 0)
            {
                _priorityQueue.Add(node);
                return;
            }

            //add at the end
            _priorityQueue.Add(node);
            int i = size - 1;
            while(i != 0 && _priorityQueue[(i-1)/2] > _priorityQueue[i])
            {
                WordNode temp = _priorityQueue[i];
                _priorityQueue[i] = _priorityQueue[(i-1)/2];
                _priorityQueue[(i-1)/2] = temp;

                i = (i - 1) / 2;
            }
            return;

        }

        public WordNode Pop()
        {
            WordNode smallest = _priorityQueue[0];

            _priorityQueue.RemoveAt(0);
            int size = _priorityQueue.Count;

            for(int i = 0; i < size; i++ )
            {
                MakeHeap(ref _priorityQueue, size, i);
            }

            return smallest;
        }

        private void MakeHeap(ref List<WordNode> pq, int size, int index)
        {
            // root = i
            int smallest = index;
            int left = 2 * index + 1;
            int right = 2 * index + 2;

            // if left is smaller than root
            if (left < size && pq[left] < pq[smallest])
            {
                smallest = left;
            }
            //if right is smaller than root
            if (right < size && pq[right] < pq[smallest])
            {
                smallest = right;
            }
            // swap nodes
            if (smallest != index)
            {
                WordNode temp = pq[index];
                pq[index] = pq[smallest];
                pq[smallest] = temp;

                MakeHeap(ref pq, size, smallest);
            }
        }
        private bool IsMinHeap(List<WordNode> pq)
        {
            int size = pq.Count;
            for (int i = size - 1; i >= 0; i--)
            {
                if (pq[i] < pq[(i - 1) / 2])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
