using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Tesseract;

namespace ContractNotes
{
    class AdjacencyWordInfo
    {
        public Word word;
        public int leftWordNo, rightWordNo;
        public int leftDistance, rightDistance;
        public bool isMerged;

        public AdjacencyWordInfo(Word word)
        {
            this.word = word;
            leftWordNo = rightWordNo = -1;
            leftDistance = rightDistance = int.MaxValue;
            isMerged = false;
        }

        public void updateAdjacencyWordInfo(List<Word> wordList, int adjacencyWordNo)
        {
            Word adjacencyWord = wordList[adjacencyWordNo];
            if (word.canMerage(adjacencyWord))
            {
                int distance = word.getDistanceWithWord(adjacencyWord);
                if (distance < rightDistance)
                {
                    rightDistance = distance;
                    rightWordNo = adjacencyWordNo;
                }
            }
            if (adjacencyWord.canMerage(word))
            {
                int distance = adjacencyWord.getDistanceWithWord(word);
                if (distance < leftDistance)
                {
                    leftDistance = distance;
                    leftWordNo = adjacencyWordNo;
                }
            }
        }

        public Word getMergedWord(List<Word> wordList)
        {
            Word mergedWord = getLeftMergedWord(wordList);
            if (rightWordNo != -1) mergedWord.mergeWith(wordList[rightWordNo].adjacencyWordInfo.getRightMergedWord(wordList));
            return mergedWord;
        }

        public Word getLeftMergedWord(List<Word> wordList)
        {
            isMerged = true;
            if (leftWordNo == -1) return word;
            Word mergedWord = wordList[leftWordNo].adjacencyWordInfo.getLeftMergedWord(wordList);
            mergedWord.mergeWith(word);
            return mergedWord;
        }

        public Word getRightMergedWord(List<Word> wordList)
        {
            isMerged = true;
            if (rightWordNo == -1) return word;
            word.mergeWith(wordList[rightWordNo].adjacencyWordInfo.getRightMergedWord(wordList));
            return word;
        }
    }

    class Word
    {
        public string word;
        public int x, y, w, h;
        public List<int> upList = new List<int>();
        public List<int> downList = new List<int>();
        public List<int> leftList = new List<int>();
        public List<int> rightList = new List<int>();
        public AdjacencyWordInfo adjacencyWordInfo;
        public static int ADJ_WORD_COUNT = 5;

        public Word(string word, int x, int y, int w, int h)
        {
            this.word = word;
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
            adjacencyWordInfo = new AdjacencyWordInfo(this);
        }

        public bool isIntersectedInHorizontal(Word word)
        {
            return Math.Max(y, word.y) < Math.Min(y + h, word.y + word.h);
        }

        public bool isIntersectedInVertical(Word word)
        {
            return Math.Max(x, word.x) < Math.Min(x + w, word.x + word.w);
        }

        public bool isUpOf(Word word)
        {
            return y + h < word.y;
        }

        public bool isDownOf(Word word)
        {
            return word.y + word.h < y;
        }

        public bool isLeftOf(Word word)
        {
            return x + w < word.x;
        }

        public bool isRightOf(Word word)
        {
            return word.x + word.w < x;
        }

        public bool isJustUpOf(Word word)
        {
            return isIntersectedInVertical(word) && isUpOf(word);
        }

        public bool isJustDownOf(Word word)
        {
            return isIntersectedInVertical(word) && isDownOf(word);
        }

        public bool isJustLeftOf(Word word)
        {
            return isIntersectedInHorizontal(word) && isLeftOf(word);
        }

        public bool isJustRightOf(Word word)
        {
            return isIntersectedInHorizontal(word) && isRightOf(word);
        }

        public void updateAdjacencyUpWord(List<Word> wordList, int no)
        {
            Word word = wordList[no];
            if (!word.isJustUpOf(this)) return;
            int i = -1;
            for (i = upList.Count - 1; i >= 0; i--)
            {
                Word otherWord = wordList[upList[i]];
                if (otherWord.isUpOf(word) || word.isJustLeftOf(otherWord)) continue;
                break;
            }
            upList.Insert(i + 1, no);
            if (upList.Count > ADJ_WORD_COUNT) upList.RemoveAt(upList.Count - 1);
        }

        public void updateAdjacencyDownWord(List<Word> wordList, int no)
        {
            Word word = wordList[no];
            if (!word.isJustDownOf(this)) return;
            int i = -1;
            for (i = downList.Count - 1; i >= 0; i--)
            {
                Word otherWord = wordList[downList[i]];
                if (otherWord.isDownOf(word) || word.isJustLeftOf(otherWord)) continue;
                break;
            }
            downList.Insert(i + 1, no);
            if (downList.Count > ADJ_WORD_COUNT) downList.RemoveAt(downList.Count - 1);
        }

        public void updateAdjacencyLeftWord(List<Word> wordList, int no)
        {
            Word word = wordList[no];
            if (!word.isJustLeftOf(this)) return;
            int i = -1;
            for (i = leftList.Count - 1; i >= 0; i--)
            {
                Word otherWord = wordList[leftList[i]];
                if (word.isLeftOf(otherWord)) break;
            }
            leftList.Insert(i + 1, no);
            if (leftList.Count > ADJ_WORD_COUNT) leftList.RemoveAt(leftList.Count - 1);
        }

        public void updateAdjacencyRightWord(List<Word> wordList, int no)
        {
            Word word = wordList[no];
            if (!word.isJustRightOf(this)) return;
            int i = -1;
            for (i = rightList.Count - 1; i >= 0; i--)
            {
                Word otherWord = wordList[rightList[i]];
                if (word.isRightOf(otherWord)) break;
            }
            rightList.Insert(i + 1, no);
            if (rightList.Count > ADJ_WORD_COUNT) rightList.RemoveAt(rightList.Count - 1);
        }

        public int getDistanceWithWord(Word word)
        {
            return word.x - x - w;
        }

        public bool canMerage(Word word)
        {
            if (!isJustLeftOf(word)) return false;
            double heightRate = (double)h / word.h;
            if (heightRate > 2 || heightRate < 0.5) return false;
            return getDistanceWithWord(word) < h * 2;
        }

        public void mergeWith(Word word)
        {
            this.word += string.Format("{0}{1}", (double)(word.x - x - w) / h > 0.25 ? " " : "", word.word);
            int right = word.x + word.w;
            int bottom = Math.Max(y + h, word.y + word.h);
            x = Math.Min(x, word.x);
            y = Math.Min(y, word.y);
            w = right - x;
            h = bottom - y;
        }

        public string getCandidate(ContractNoteTemplateFieldCandidate fieldCandidate, List<Word> wordList)
        {
            if (!ContractNote.isEqual(fieldCandidate.key, word)) return null;

            string referenceWord = findAdjacencyWord(fieldCandidate, wordList, fieldCandidate.referenceDirection);
            if (referenceWord == null) return null;
            if (!referenceWord.Equals("NULL") && !ContractNote.isPrefix(fieldCandidate.referenceFieldPrefix, referenceWord)) return null;

            return findAdjacencyWord(fieldCandidate, wordList, fieldCandidate.valueDirection, false);
        }

        string getExactCandidate(ContractNoteTemplateFieldCandidate fieldCandidate, string valueCandidate)
        {
            string[] values = valueCandidate.Split((string[])null, StringSplitOptions.RemoveEmptyEntries);
            int startIndex = fieldCandidate.startIndex < 0 ? values.Length + fieldCandidate.startIndex : fieldCandidate.startIndex;
            int endIndex = fieldCandidate.endIndex < 0 ? values.Length + fieldCandidate.endIndex : fieldCandidate.endIndex;
            if (startIndex < 0 || startIndex >= values.Length || endIndex < 0 || endIndex >= values.Length || startIndex > endIndex) return null;

            string candidate = values[startIndex];
            for (int i = startIndex + 1; i <= endIndex; i++)
            {
                candidate += " " + values[i];
            }
            return candidate;
        }

        string findAdjacencyWord(ContractNoteTemplateFieldCandidate fieldCandidate, List<Word> wordList, string route, bool isReferenceWord = true)
        {
            if (route.Length == 0)
            {
                if (isReferenceWord) return word;
                string exactCandidate = getExactCandidate(fieldCandidate, word);
                return ContractNoteTemplateField.isValidCandidate(exactCandidate) ? exactCandidate : null;
            }
            char direction = route[0];
            int repeatCount = 0, adjacencyNo;
            for(int i = 0; i < route.Length; i++)
            {
                if (direction != route[i]) break;
                repeatCount++;
            }
            for(int i = 1; i <= repeatCount; i++)
            {
                switch (direction)
                {
                    case 'I':
                        return "NULL";
                    case 'U':
                        if (i > upList.Count) continue;
                        adjacencyNo = upList[i - 1]; break;
                    case 'L':
                        if (i > leftList.Count) continue;
                        adjacencyNo = leftList[i - 1]; break;
                    case 'D':
                        if (i > downList.Count) continue;
                        adjacencyNo = downList[i - 1]; break;
                    case 'R':
                        if (i > rightList.Count) continue;
                        adjacencyNo = rightList[i - 1]; break;
                    case 'S':
                        adjacencyNo = -1; break;
                    default:
                        continue;
                }
                if (adjacencyNo == -1) return word;
                string adjacencyWord = wordList[adjacencyNo].findAdjacencyWord(fieldCandidate, wordList, route.Substring(i), isReferenceWord);
                if (adjacencyWord != null) return adjacencyWord;
            }
            return null;
        }

        public void output(StreamWriter sw, List<Word> wordList)
        {
            //sw.WriteLine(word); return;
            //sw.WriteLine(string.Format("{0}  [ {1} , {2} , {3} , {4} ]", word, x, y, w, h)); return;
            sw.WriteLine(string.Format("----- {0} -----", word));
            sw.WriteLine(string.Format("  [ {0} , {1} , {2} , {3}]", x, y, w, h));
            outputAdjacencyWordList(sw, 'U', upList, wordList);
            outputAdjacencyWordList(sw, 'L', leftList, wordList);
            outputAdjacencyWordList(sw, 'R', rightList, wordList);
            outputAdjacencyWordList(sw, 'D', downList, wordList);
        }

        public void outputAdjacencyWordList(StreamWriter sw, char direction,List<int> adjacencyWordList,List<Word> wordList)
        {
            sw.Write("  {0}[{1}] :", direction, adjacencyWordList.Count);
            for (int i = 0; i < adjacencyWordList.Count; i++)
            {
                sw.Write("  [{0}]", wordList[adjacencyWordList[i]].word);
            }
            sw.WriteLine();
        }
    }

    class PdfParser
    {
        ContractNote contractNote;
        string tempPngFilePath, tempJpgFilePath, tempPdfFilePath, tempRawFilePath, tempNonMergedOcrFilePath, tempMergedOcrFilePath;
        List<Word> wordList = new List<Word>();
        List<Word> completedWordList = new List<Word>();

        public PdfParser(ContractNote contractNote)
        {
            this.contractNote = contractNote;
        }

        public void parsePdf(string filePath)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath).Replace(' ', '_');
            tempPngFilePath = TemporaryDataManager.getFilePath(fileNameWithoutExtension, "png");
            tempJpgFilePath = TemporaryDataManager.getFilePath(fileNameWithoutExtension, "jpg");
            tempPdfFilePath = TemporaryDataManager.getFilePath(fileNameWithoutExtension, "pdf");
            tempRawFilePath = TemporaryDataManager.getFilePath(fileNameWithoutExtension, "txt");
            tempNonMergedOcrFilePath = TemporaryDataManager.getFilePath(fileNameWithoutExtension, "nocr");
            tempMergedOcrFilePath = TemporaryDataManager.getFilePath(fileNameWithoutExtension, "ocr");
            try
            {
                File.Copy(filePath, tempPdfFilePath, true);
            }
            catch { }
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += delegate (object s, DoWorkEventArgs args)
            {
                string command = AppDomain.CurrentDomain.BaseDirectory + "ConvertPdfToPng.exe";
                string parameter = tempPdfFilePath + " " + tempPngFilePath;
                Process process = new Process();
                process.StartInfo.FileName = command;
                process.StartInfo.Arguments = parameter;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.WaitForExit();
            };
            backgroundWorker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
            {
                System.Threading.Thread.Sleep(100);
                FluentGrayConverter fgc = new FluentGrayConverter();
                fgc.convert(tempPngFilePath, tempJpgFilePath);
                GC.Collect();
                doOcrCallback();
            };
            backgroundWorker.RunWorkerAsync();
        }

        void doOcrCallback()
        {
            wordList.Clear();
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += delegate (object s, DoWorkEventArgs args)
            {
                try
                {
                    var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
                    var img = Pix.LoadFromFile(tempJpgFilePath);
                    var page = engine.Process(img);
                    var iter = page.GetIterator();
                    iter.Begin();
                    do
                    {
                        do
                        {
                            do
                            {
                                do
                                {
                                    string word = iter.GetText(PageIteratorLevel.Word);
                                    if (word.Trim(' ').Length > 0)
                                    {
                                        Tesseract.Rect rect;
                                        if (iter.TryGetBoundingBox(PageIteratorLevel.Word, out rect))
                                        {
                                            wordList.Add(new Word(word, rect.X1, rect.Y1, rect.Width, rect.Height));
                                        }
                                    }
                                } while (iter.Next(PageIteratorLevel.TextLine, PageIteratorLevel.Word));
                            } while (iter.Next(PageIteratorLevel.Para, PageIteratorLevel.TextLine));
                        } while (iter.Next(PageIteratorLevel.Block, PageIteratorLevel.Para));
                    } while (iter.Next(PageIteratorLevel.Block));
                }
                catch (Exception) { }
            };
            backgroundWorker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
            {
                System.Threading.Thread.Sleep(100);
                completeWordCallback();
            };
            backgroundWorker.RunWorkerAsync();
        }

        void completeWordCallback()
        {
            outputWordList(wordList, tempNonMergedOcrFilePath);
            mergeWords();
            adjustWords1();
            findAdjacencyWords();
            outputWordList(completedWordList, tempMergedOcrFilePath);
            contractNote.findTemplate(completedWordList, tempPngFilePath);
        }

        void mergeWords()
        {
            for (int i = 0; i < wordList.Count; i++)
            {
                for (int j = i + 1; j < wordList.Count; j++)
                {
                    wordList[i].adjacencyWordInfo.updateAdjacencyWordInfo(wordList, j);
                    wordList[j].adjacencyWordInfo.updateAdjacencyWordInfo(wordList, i);
                }
            }
            completedWordList.Clear();
            for (int i = 0; i < wordList.Count; i++)
            {
                if (wordList[i].adjacencyWordInfo.isMerged) continue;
                completedWordList.Add(wordList[i].adjacencyWordInfo.getMergedWord(wordList));
            }
        }

        void adjustWords1()
        {
            string plainText = extractPlainText(tempPdfFilePath);
            char[] lineSeparatingChars = { '\n' };
            char[] wordSeparatingChars = { ' ' };
            string[] lines = plainText.Split(lineSeparatingChars, StringSplitOptions.RemoveEmptyEntries);
            List<string> wordCombinationList = new List<string>();
            foreach(string line in lines)
            {
                string[] words = line.Split(wordSeparatingChars, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < words.Length; i++)
                {
                    string wordCombination = "";
                    for (int j = i; j < words.Length; j++)
                    {
                        wordCombination += (i == j ? "" : " ") + words[j];
                        wordCombinationList.Add(wordCombination);
                    }
                }
            }
            for (int i = 0; i < completedWordList.Count; i++)
            {
                double bestSimilarity = 0;
                int bestSimilarWordId = 0;
                string agent1 = Regex.Replace(completedWordList[i].word, @"[^0-9a-zA-Z$@]+", "");
                for (int j = 0; j < wordCombinationList.Count; j++)
                {
                    string agent2 = Regex.Replace(wordCombinationList[j], @"[^0-9a-zA-Z$@]+", "");
                    double similarity = ContractNote.getSimilarity(agent1, agent2);
                    if (bestSimilarity < similarity)
                    {
                        bestSimilarity = similarity;
                        bestSimilarWordId = j;
                    }
                }
                if (bestSimilarity > 0.8) completedWordList[i].word = wordCombinationList[bestSimilarWordId];
            }
        }

        void adjustWords2()
        {
            string plainText = extractPlainText(tempPdfFilePath);
            char[] lineSeparatingChars = { '\n' };
            char[] wordSeparatingChars = { ' ' };
            string[] lines = plainText.Split(lineSeparatingChars, StringSplitOptions.RemoveEmptyEntries);
            List<string> wordCombinationList = new List<string>();
            foreach (string line in lines)
            {
                string[] words = line.Split(wordSeparatingChars, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < words.Length; i++)
                {
                    string wordCombination = "";
                    for (int j = i; j < words.Length; j++)
                    {
                        wordCombination += (i == j ? "" : " ") + words[j];
                        wordCombinationList.Add(wordCombination);
                    }
                }
            }
            try
            {
                using (PdfReader reader = new PdfReader(tempPdfFilePath))
                {
                    var pageRectangle = reader.GetPageSize(1);
                    float pageWidth = pageRectangle.Width;
                    float pageHeight = pageRectangle.Height;
                    float sizeRate = 2754f / 595;
                    for (int i = 0; i < completedWordList.Count; i++)
                    {
                        double bestSimilarity = 0;

                        string word = completedWordList[i].word;
                        float w = completedWordList[i].w / sizeRate;
                        float h = completedWordList[i].h / sizeRate;
                        float x = completedWordList[i].x / sizeRate;
                        float y = pageHeight - completedWordList[i].y / sizeRate - h;
                        RenderFilter[] filter = { new RegionTextRenderFilter(new System.util.RectangleJ(x, y, w, h)) };
                        ITextExtractionStrategy strategy = new FilteredTextRenderListener(new LocationTextExtractionStrategy(), filter);
                        string text = PdfTextExtractor.GetTextFromPage(reader, 1, strategy);
                        string[] wordSeparatingStrings = { "  " };
                        string[] words = text.Split(wordSeparatingStrings, StringSplitOptions.RemoveEmptyEntries);
                        if (words.Length > 0)
                        {
                            bestSimilarity = ContractNote.getSimilarity(word, words[0]);
                            if (bestSimilarity > 0.8) completedWordList[i].word = words[0];
                        }

                        int bestSimilarWordId = 0;
                        string agent1 = Regex.Replace(word, @"[^0-9a-zA-Z$@]+", "");
                        for (int j = 0; j < wordCombinationList.Count; j++)
                        {
                            string agent2 = Regex.Replace(wordCombinationList[j], @"[^0-9a-zA-Z$@]+", "");
                            double similarity = ContractNote.getSimilarity(agent1, agent2);
                            if (bestSimilarity < similarity)
                            {
                                bestSimilarity = similarity;
                                bestSimilarWordId = j;
                            }
                        }
                        if (bestSimilarity > 0.8) completedWordList[i].word = wordCombinationList[bestSimilarWordId];
                    }
                }
            }
            catch { }
        }

        string extractPlainText(string pdfFilePath)
        {
            string plainText = "";
            try
            {
                using (PdfReader reader = new PdfReader(pdfFilePath))
                {
                    plainText = PdfTextExtractor.GetTextFromPage(reader, 1);
                    StreamWriter sw = new StreamWriter(tempRawFilePath);
                    sw.Write(plainText);
                    sw.Close();
                }
            }
            catch { }
            return plainText;
        }

        void findAdjacencyWords()
        {
            for(int i = 0; i < completedWordList.Count; i++)
            {
                for(int j = 0; j < completedWordList.Count; j++)
                {
                    if (i == j) continue;
                    completedWordList[i].updateAdjacencyUpWord(completedWordList, j);
                    completedWordList[i].updateAdjacencyDownWord(completedWordList, j);
                    completedWordList[i].updateAdjacencyLeftWord(completedWordList, j);
                    completedWordList[i].updateAdjacencyRightWord(completedWordList, j);
                }
            }
        }

        void outputWordList(List<Word> wordList, string filePath)
        {
            try
            {
                StreamWriter sw = new StreamWriter(filePath);
                for (int i = 0; i < wordList.Count; i++)
                {
                    wordList[i].output(sw, wordList);
                }
                sw.Close();
            }
            catch { }
        }
    }
}
