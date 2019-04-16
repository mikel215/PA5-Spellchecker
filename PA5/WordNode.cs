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

        // Properties
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
        public static bool operator <(WordNode a, WordNode b)
        {
            if(a.EditDistance < b.EditDistance)
            {
                return true;
            }
            return false;
        }
        public static bool operator >(WordNode a, WordNode b)
        {
            if(a.EditDistance > b.EditDistance)
            {
                return true;
            }
            return false;
        }


    }
}
