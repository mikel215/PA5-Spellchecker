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
        private String input = "";
        // 1. Read words from text to list(our dictionary)
        public static readonly string[] wordFile = File.ReadAllLines(@".../.../words.txt");
        public static readonly List<string> dictionaryList= new List<string>(wordFile);

        public MainWindow()
        {
            InitializeComponent();
            InputFile.Focus();

        }

        public Dictionary<string,List<string>> GetSuggestions(List<string> list)
        {
            Dictionary<string, List<string>> map = new Dictionary<string, List<string>>();

            foreach(string word in list)
            {
                List<string> suggestions = new List<string>();
                PriorityQueue pq = new PriorityQueue();

                // Make priority queue out of words with edit distances
                foreach(string item in dictionaryList)
                {
                    int distance = CalculateEditDistanceBU( word,item);
                    WordNode newWord = new WordNode(item, distance);

                    // add WordNode to priority queue
                    pq.Insert(newWord);
                }

                // Pop the top 10 items in PQ
                for(int i = 0; i< 10; i++)
                {
                    WordNode newNode = pq.Pop();
                    suggestions.Add(newNode.Word);

                }
                map.Add(word, suggestions);
            }
            return map;
        }


        public List<string> FindMisspelled(List<string> list)
        {
            List<string> misspelled = new List<string>();

            foreach(string word in list)
            {
                // strip punctuation
                string newWord = new string(word.Where(c => !char.IsPunctuation(c)).ToArray());

                // If current word is not in our dicitonary, add to misspelled word list
                if(! dictionaryList.Contains(newWord))
                {
                    misspelled.Add(newWord);
                }
                else
                {
                    continue;
                }

            }
            return misspelled;

        }

        public int CalculateEditDistanceBU(string first,string second)
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

        private void SelectionSubmit_Click(object sender, RoutedEventArgs e)
        {
            input = selectionBox.Text;
        }

        private void TextBox_OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                StartProgram_Click(this, new RoutedEventArgs());
            }
        }

        private void StartProgram_Click(object sender, RoutedEventArgs e)
        {
            // 2. Prompt input file 
            string inputFile = InputFile.Text;
            StreamReader reader = new StreamReader($".../.../{inputFile}");
            List<string> toCorrectList = reader.ReadToEnd().Split(' ').ToList();

            // Find misspelled words
            List<string> misspelled = FindMisspelled(toCorrectList);

            // 3a. Compute 10 most probable suggestions for each misspelled word
            Dictionary<string, List<string>> suggestionMap = GetSuggestions(misspelled);

            // Show user data, then prompt for correct answer
            // dictionary to store results <misspelled word, correction>
            Dictionary<string, string> results = new Dictionary<string, string>();
            if(suggestionMap.Count > 0)
            {
                KeyValuePair<string, List<string>> entry = suggestionMap.First();
                outputBox.Items.Add("1. None of the words below are correct");

                for(int i = 0; i < 10; i++)
                {
                    string s = (i + 2).ToString()+ ". " + entry.Value[i];
                    outputBox.Items.Add(s);

                }
            }

            selectionBox.Focus();

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
