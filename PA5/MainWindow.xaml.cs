using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data.SQLite;
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
        Queue<KeyValuePair<string, List<string>>> glbl_queue = new Queue<KeyValuePair<string, List<string>>>();
        Dictionary<string, List<string>> toWrite = new Dictionary<string, List<string>>();
        // 1. Read words from text to list(our dictionary)
        public static readonly string[] wordFile = File.ReadAllLines(@".../.../words.txt");
        public static readonly List<string> dictionaryList = new List<string>(wordFile);
        bool fileOrDB = false; // default file storage, true for DB

        public MainWindow()
        {
            InitializeComponent();
            InputFile.Focus();

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
            if (selectionBox.Text == "")
            {
                return;
            }

            string input = selectionBox.Text;
            KeyValuePair<string, List<string>> item = glbl_queue.Dequeue();
            List<string> toCorrectList = GetList();

            // make results list
            int inputNum = Int32.Parse(input);
            if (inputNum > 1)
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
            if (glbl_queue.Count == 0)
            {
                outputBox.Items.Clear();
                selectionBox.Clear();

                if(fileOrDB == true)
                {
                    WriteToDatabase();
                }
                else
                {
                    WriteToFile(toCorrectList);
                }
                return;

            }

            // output the next words suggestions
            OutputSuggestions(toCorrectList);
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
            List<string> toCorrectList = GetList();

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

            OutputSuggestions(toCorrectList);
        }

        private void WriteToDatabase()
        {
            SQLiteConnection myConnection = 
                             new SQLiteConnection("Data Source=./.../.../wordDB.db;Version=3;New=False;FailIfMissing=True");
            try
            {
                myConnection.Open();

                foreach(KeyValuePair<string,List<string>> kv_pair in toWrite)
                {
                    // add misspelling
                    string misspellingQuery = "insert into misspelled(misspelled_word) " +
                                              "values(" +"'" + kv_pair.Key +"'"+ ")";
                    SQLiteCommand insertQuery1 = new SQLiteCommand(misspellingQuery, myConnection);
                    insertQuery1.ExecuteNonQuery();

                    // add suggestions
                    string fkQuery = "select misspelled_id " +
                                     "from   misspelled " +
                                     "where  misspelled_word = " + "'"+kv_pair.Key+"'";
                    SQLiteCommand select = new SQLiteCommand(fkQuery, myConnection);
                    SQLiteDataReader dr = select.ExecuteReader();
                    dr.Read();
                    int foreignKey = Int32.Parse(dr["misspelled_id"].ToString());
                    int count = 1;
                    foreach(string suggestion in kv_pair.Value)
                    {
                        
                        string suggestionQuery = "insert into " +
                                       "suggestion(suggestion_word, priority,suggestion_for) " +
                                       "values(" +"'"+suggestion+"'"+ ", "+count+", "+
                                        foreignKey+ ")";
                        SQLiteCommand insertQuery2 = new SQLiteCommand(suggestionQuery, myConnection);
                        insertQuery2.ExecuteNonQuery();
                        count++;
                    }
                }
                myConnection.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return;
        }

        private void WriteToFile(List<string> list)
        {
            // first, write autocorrect results to file.
            string txtToFile = "";
            foreach (KeyValuePair<string, List<string>> c in toWrite)
            {
                txtToFile += c.Key + ",";
                foreach (string suggests in c.Value)
                {
                    txtToFile += suggests + ",";
                }
                txtToFile += "\r\n";
            }
            File.WriteAllText("./.../.../autocorrect_results.csv", txtToFile);

            // second, correct misspellings then output to specified file
            string finalString = "";
            foreach (string word in list)
            {
                if (word == "\r\n")
                {
                    finalString += "\r\n";
                    continue;
                }
                string punctBegin = "";
                string punctEnd = "";
                // strip punctuation for comparison
                string newWord = new string(word.Where(c => !char.IsPunctuation(c)).ToArray());

                if (toWrite.ContainsKey(newWord)) // if word is misspelled replace it while keeping punctuation
                {
                    if (char.IsPunctuation(word[0])) // get punctuation at beginning
                    {
                        punctBegin = word[0].ToString();
                    }
                    if (char.IsPunctuation(word[word.Length - 1])) // get punctuation at end
                    {
                        punctEnd = word[word.Length - 1].ToString();
                    }
                    // get the first suggestion for misspelled word, swap them
                    string correctWord = toWrite[newWord][0];
                    finalString += punctBegin + correctWord + punctEnd + " ";
                    continue;

                }
                finalString += word + " ";
            }
            File.WriteAllText(OutputFile.Text, finalString);
            outputBox.Items.Add("Results written to file.");

            return;
        }

        private void OutputSuggestions(List<string> list)
        {
            KeyValuePair<string, List<string>> entry = glbl_queue.First();
            outputBox.Items.Clear();
            selectionBox.Clear();

            string context = GetContext(entry.Key, list);
            outputBox.Items.Add("Unknown word: " + entry.Key);
            outputBox.Items.Add("   Context: " + context);
            outputBox.Items.Add("1. None of the words below are correct");
            for(int i = 0; i < 10; i++)
            {
                string s = (i + 2).ToString()+ ". " + entry.Value[i];
                outputBox.Items.Add(s);

            }

            selectionBox.Focus();
            return;

        }


        public string GetContext(string unknown_word, List<string> list)
        {
            // to get context get the word before unknown word
            string return_string = "";
            int word_index = 0;
            for(int i = 0; i< list.Count; i++)
            {
                if(list[i] == unknown_word)
                {
                    if(i - 1 >= 0)
                    {
                        return_string += list[i - 1] + " ";
                    }
                    return_string += unknown_word + " ";
                    word_index = i;
                    break;
                }
            }
            // add next 3 words
            for(int i = word_index + 1; i < list.Count; i++)
            {
                return_string += list[i] + " ";
            }

            return return_string;
        }

        private List<string> GetList()
        {
            // 2. Prompt input file 
            string inputFile = InputFile.Text;
            StreamReader reader = new StreamReader($".../.../{inputFile}");
            List<string> list = reader.ReadToEnd().
                Split(' ').ToList();

            List<string> toCorrectList = new List<string>();
            foreach(string word in list)
            {
                if(word.Contains("\r\n"))
                {
                    string[] newlineSplit = word.Split(new char[] { '\r', '\n' }, 2);
                    string[] second = newlineSplit[1].Split('\n');
                    toCorrectList.Add(newlineSplit[0]);
                    toCorrectList.Add("\r\n");
                    toCorrectList.Add(second[1]);
                    continue;
                }
                toCorrectList.Add(word);
            }
            return toCorrectList;
        }

        private int CalculateEditDistanceBU(string first, string second)
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
            for (int i = 0; i < second.Length; i++)
            {
                matrix[0, i] = i;
            }

            //fill in first column
            for (int i = 0; i < first.Length; i++)
            {
                matrix[i, 0] = i;
            }

            //compute rest of matrix
            for (int i = 1; i < first.Length; i++)
            {
                for (int j = 1; j < second.Length; j++)
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

        private Dictionary<string, List<string>> GetSuggestions(List<string> list)
        {
            Dictionary<string, List<string>> map = new Dictionary<string, List<string>>();

            foreach (string word in list)
            {
                List<string> suggestions = new List<string>();
                PriorityQueue pq = new PriorityQueue();

                // Make priority queue out of words with edit distances
                foreach (string item in dictionaryList)
                {
                    int distance = CalculateEditDistanceBU(word, item);
                    WordNode newWord = new WordNode(item, distance);

                    // add WordNode to priority queue
                    pq.Insert(newWord);
                }

                // Pop the top 10 items in PQ

                for (int i = 0; i < 10; i++)
                {
                    WordNode newNode = pq.Pop();
                    suggestions.Add(newNode.Word);

                }
                map.Add(word, suggestions);
            }
            return map;
        }

        private List<string> FindMisspelled(List<string> list)
        {
            List<string> misspelled = new List<string>();

            foreach (string word in list)
            {
                if (word == "\r\n")
                {
                    continue;
                }
                // strip punctuation
                string newWord = new string(word.Where(c => !char.IsPunctuation(c)).ToArray());

                // If current word is not in our dicitonary, add to misspelled word list
                if (!dictionaryList.Contains(newWord))
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

        private void FileButton_Click(object sender, RoutedEventArgs e)
        {
            fileOrDB = false;
            ChoiceBox.Text = "File Store";
        }

        private void DBButton_Click(object sender, RoutedEventArgs e)
        {
            fileOrDB = true;
            ChoiceBox.Text = "Database";
        }
    }
}
