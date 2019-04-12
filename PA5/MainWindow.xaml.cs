using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PA5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 1. Read words from text to list(our dictionary)
            var wordFile = File.ReadAllLines(@".../.../words.txt");
            var dictionaryList= new List<string>(wordFile);

            // 2. Prompt input file 
            StreamReader reader = new StreamReader(@".../.../sample1.txt");
            List<string> toCorrectList = reader.ReadToEnd().Split(' ').ToList();

            // Find misspelled words
            List<string> misspelled = new List<string>();
            foreach(string word in toCorrectList)
            {
                // strip punctuation
                string newWord = new string(word.Where(c => !char.IsPunctuation(c)).ToArray());

                // If current word is not in our dicitonary, add to misspelled word list
                if(dictionaryList.Any(item => item != newWord))
                {
                    misspelled.Add(newWord);
                }
                else
                {
                    continue;
                }

            }

            // 3. Compute 10 most probable suggestions for each misspelled word
            Dictionary<string, List<string>> suggestionMap = new Dictionary<string, List<string>>();

            // a. compute 10 most probable suggestions for each word
            foreach(string word in misspelled)
            {
                List<string> suggestions = new List<string>();

                // Make priority queue out of words with edit distances
                foreach(string item in dictionaryList)
                {
                    int distance = CalculateEditDistanceBU( word,item);
                    WordNode newWord = new WordNode(item, distance);
                    continue;

                }

                for(int i = 0; i< 10; i++)
                {
                    // get edit distances

                }
            }

        }

        int CalculateEditDistanceBU(string first,string second)
        {
            // List<List<int>> matrix = new List<List<int>>();
            int[,] matrix = new int[first.Length, second.Length];

            //build result matrix
            //matrix.resize(first.length() + 1);
            /*
            foreach (List<int> row in matrix)
            {
                row.resize(second.Length + 1);
            }
            */

           //fill in first row  
            for (int i = 0; i<second.Length; i++)
            {
                  matrix[0, i] = i;
            }

            //fill in first column
            for (int i = 0; i<first.Length; i++)
            {
                matrix[i,0] = i;
            }

            //compute rest of matrix
            for (int i = 1; i<first.Length; i++)
            {
                for (int j = 1; j<second.Length; j++)
                {
                    //find least cost of our 3 choices
                    int top_cost = matrix[i - 1, j] + 1;
                    int left_cost = matrix[i, j - 1] + 1;
                    int diagonal_cost = matrix[i - 1, j - 1];

                    //add 1 if characters are not the same
                    if (first[i - 1] != second[j - 1])
                    {
                        diagonal_cost++;
                    } 

                    int best_choice = Math.Min(top_cost, Math.Min(left_cost, diagonal_cost));

                    //store result in current cell
                    matrix[i, j] = best_choice;
                }
            }

            return matrix[first.Length - 1, second.Length - 1];
        }

        /*
        public int CalculateEditDistance(ref string first, ref string second,
                                         int first_index, int second_index,
                                         ref List<List<int>> mem )
        {
            int cost = 0;

            //ensure index is in bounds
            if (first_index >= first.Length)
            {
                //at is point, we can no longer transform and delete doesn't 
                //make sense because we're smaller
                return second.Length - second_index;
            }
            else if (second_index >= second.Length)
            {
                return first.Length - first_index;
            }

            //is there currently alignment at the indices?
            if (first[first_index] == second[second_index])
            {
                return CalculateEditDistance(
                   ref first,
                    ref second,
                   first_index + 1,
                   second_index + 1,
                   ref mem);
            }
            else
            {
                //before we make recursive calls, check mem
                //value greater than -1 means that we've calculated this before
                int insert_cost = mem[first_index][second_index + 1];
                int delete_cost = mem[first_index + 1][second_index];
                int transform_cost = mem[first_index + 1][second_index + 1];

                //recursive calls must be made if we have bad memory
                if (insert_cost == -1)
                {
                    insert_cost = CalculateEditDistance(
                       ref first,
                       ref second,
                       first_index,
                       second_index + 1,
                       ref mem);
                }
                if (delete_cost == -1)
                {
                    delete_cost = CalculateEditDistance(
                       ref first,
                       ref second,
                       first_index + 1,
                       second_index,
                       ref mem);
                }
                if (transform_cost == -1)
                {
                    transform_cost = CalculateEditDistance(
                       ref first,
                       ref second,
                       first_index + 1,
                       second_index + 1,
                       ref mem);
                }
                cost = 1 + Math.Min(Math.Min(insert_cost, delete_cost), transform_cost);
                mem[first_index][second_index] = cost;
                return cost;
            }
        }
        */
    }
}
