using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PA5
{
    // Implements list based
    class PriorityQueueAVL
    {
        //Fields
        private WordNode _root;
        private int size;

        // Constructor
        public PriorityQueueAVL()
        {
            _root = null;
            size = 0;
        }

        // Methods
        public void Insert(WordNode node)
        {
            // if empty
            if(_root == null)
            {
                _root = node;
                return;
            }
            // insert element into AVL tree, balances as elements are added
            _root.AddElement(_root, node);
            size++;

            // verify if it is a min heap
            for(int i = 0; i < size; i++)
            {
                MakeHeap(ref _priorityQueue, size, i);
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

        private void MakeHeap(ref WordNode node)
        {
            // root = i
            WordNode smallest = node;
            WordNode left = node.LeftChild;
            WordNode right = node.RightChild;

            // if left is smaller than root
            if (left < smallest)
            {
                smallest = left;
            }
            //if right is smaller than root
            if (right < smallest)
            {
                smallest = right;
            }
            // swap nodes
            if (smallest != node)
            {
                WordNode temp = node;
                node = smallest;
                smallest = temp;

                MakeHeap(ref smallest);
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
