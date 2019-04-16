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
        Queue<KeyValuePair<string, List<string>>> glbl_queue = new Queue<KeyValuePair<string,List<string>>>();
        Dictionary<string, List<string>> toWrite = new Dictionary<string, List<string>>();
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
                PriorityQueueVector pq = new PriorityQueueVector();

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

        private void SuggestionBox_OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SelectionSubmit_Click(this, new RoutedEventArgs());
            }
        }

        private void SelectionSubmit_Click(object sender, RoutedEventArgs e)
        {
            if(selectionBox.Text == "")
            {
                return;
            }

            string input = selectionBox.Text;
            KeyValuePair<string, List<string>> item = glbl_queue.Dequeue();

            // make results list
            int inputNum = Int32.Parse(input);
            if(inputNum > 1)
            {
                inputNum = inputNum - 2;
                List<string> current = item.Value;
                string newTop = current[inputNum];
                current.Remove(newTop);
                current.Insert(0, newTop);
                toWrite.Add(item.Key, current);
            }
            else
            {
                // add word?
            }
            // if queue is empty, write results to file, return
            if(glbl_queue.Count == 0)
            {
                outputBox.Items.Clear();
                selectionBox.Clear();

                string path = OutputFile.Text;
                string txtToFile = "";
                foreach(KeyValuePair<string,List<string>> c in toWrite)
                {
                    txtToFile += c.Key + ",";
                    foreach(string suggests in c.Value)
                    {
                        txtToFile += suggests + ",";
                    }
                    txtToFile += "\r\n";
                }
                File.WriteAllText(path, txtToFile);


                outputBox.Items.Add("Results written to file.");
                return;
            }

            // output the next words suggestions
            KeyValuePair<string, List<string>> entry = glbl_queue.First();
            outputBox.Items.Clear();
            selectionBox.Clear();
            outputBox.Items.Add("1. None of the words below are correct");
            for(int i = 0; i < 10; i++)
            {
                string s = (i + 2).ToString()+ ". " + entry.Value[i];
                outputBox.Items.Add(s);

            }

            selectionBox.Focus();
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
            if(suggestionMap.Count > 0)
            {
                // suggestionMap -> queue
                foreach(KeyValuePair<string,List<string>> item in suggestionMap)
                {
                    glbl_queue.Enqueue(item);
                }
            }
            else
            {
                return;
            }

            KeyValuePair<string, List<string>> entry = suggestionMap.First();
            outputBox.Items.Add("1. None of the words below are correct");
            for(int i = 0; i < 10; i++)
            {
                string s = (i + 2).ToString()+ ". " + entry.Value[i];
                outputBox.Items.Add(s);

            }
            selectionBox.Focus();
        }
    }
}
