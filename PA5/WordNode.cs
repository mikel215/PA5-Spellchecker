using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PA5
{
    class WordNode
    {
        // Fields
        private WordNode _parent = null;
        private WordNode _left = null;
        private WordNode _right = null;
        private int _height = 0;

        private string _word;
        private int _editDistance;

        // Constructors
        public WordNode(string word, int distance)
        {
            _word = word;
            _editDistance = distance;
        }
        public WordNode()
        {
            _word = "";
            _editDistance = 0;
        }

        // Methods
        public WordNode AddElement(WordNode node, WordNode toInsert)
        {
            if(node._editDistance > toInsert._editDistance)
            {
                node._left = AddElement(node._left, toInsert);
            }
            if(node._editDistance > toInsert._editDistance)
            {
                node._right = AddElement(node._right, toInsert);
            }
            return SetHeight(node);
        }

        private WordNode SetHeight(WordNode node)
        {
            if(node == null)
            {
                return node;
            }
            int right_h = -1;
            int left_h = -1;
            if(node._right != null)
            {
                right_h = node._right._height;
            }
            if(node._left != null)
            {
                left_h = node._left._height;
            }
            int h = 1 +
                    ((left_h < right_h) ? right_h : left_h);
            node._height = h;

            int balanceFactor = node.GetBalanceFactor();
            if(balanceFactor > 1 || balanceFactor < -1)
            {
                return Balance(node);
            }
            return node;
        }
         
        private WordNode Balance(WordNode node)
        {
            if( node == null)
            {
                return node;
            }

            // Balance node
            // right rotation
            if(node.GetBalanceFactor() < 0)
            {
                // original_root = node
                WordNode new_root = node._left;
                // check if left-right rotation is needed
                if(node._left != null && node._left.GetBalanceFactor() > 0)
                {
                    // rotate left at new root
                    RotateLeft(new_root);
                }
                // rotate right
                RotateRight(node);
            }
            else
            {
                // original_root = node
                WordNode new_root = node._right;
                // check if right-left rotation needed
                if(node._right != null && node._right.GetBalanceFactor() < 0 )
                {
                    // rotate right at new root
                    RotateRight(new_root);
                }
                // rotate left
                RotateLeft(node);
            }
            return node;
        }

        private WordNode RotateRight(WordNode node)
        {
            if(node == null)
            {
                return node;
            }
            WordNode new_root = node._left;
            node._left = new_root._right;
            new_root._right = node;

            // recalculate heights
            node = SetHeight(node);
            new_root = SetHeight(new_root);
            return new_root;
        }

        private WordNode RotateLeft(WordNode node)
        {
            if(node == null)
            {
                return node;
            }
            WordNode new_root = node._right;
            node._right = new_root._left;
            new_root._left = node;

            // recalculate heights
            node = SetHeight(node);
            new_root = SetHeight(new_root);
            return new_root;

        }

        private int GetBalanceFactor()
        {
            int right_h = 0;
            int left_h = 0;
            if(_right != null)
            {
                right_h = _right._height;
            }
            if(_left != null)
            {
                left_h = _left._height;
            }
            return right_h + left_h;
        }
        
        // Properties
        public WordNode LeftChild
        {
            get { return _left; }
            set { _left = value; }
        }

        public WordNode RightChild
        {
            get { return _right; }
            set { _right = value; }
        }

        public string Word
        {
            get { return _word; }
            set { _word = value; }
        }
         
        public int EditDistance
        {
            get { return _editDistance; }
            set
            {
                if(value >= -1)
                {
                    _editDistance = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        // Operator
        public static bool operator< (WordNode a, WordNode b)
        {
            if(a.EditDistance < b.EditDistance)
            {
                return true;
            }
            return false;
        }
        public static bool operator> (WordNode a, WordNode b)
        {
            if(a.EditDistance > b.EditDistance)
            {
                return true;
            }
            return false;
        }


    }
}
