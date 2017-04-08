using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;

namespace ContractNotes
{
    class RuleManager
    {
        public List<Rule> ruleList = new List<Rule>();

        public RuleManager()
        {
            loadRule();
        }

        public void loadRule()
        {
            ruleList.Clear();
            try
            {
                StreamReader sr = new StreamReader("rule.dat");
                int ruleCount = int.Parse(sr.ReadLine());
                for (int i = 0; i < ruleCount; i++)
                {
                    Rule rule = new Rule();
                    rule.readRule(sr);
                    ruleList.Add(rule);
                }
                sr.Close();
            }
            catch { }
        }

        public Rule getRuleWithInstance(string instance)
        {
            for (int i = 0; i < ruleList.Count; i++)
            {
                if (instance == ruleList[i].Instance) return ruleList[i];
            }
            return new Rule();
        }

        public Rule getRuleWithSource(string source)
        {
            for (int i = 0; i < ruleList.Count; i++)
            {
                if (source == ruleList[i].Source) return ruleList[i];
            }
            return new Rule();
        }
    }

    class CharacterSimilarityHelper
    {
        public static Dictionary<string, double> similarityDictionary = new Dictionary<string, double>();

        public static void loadDictionary()
        {
            similarityDictionary.Clear();
            string fileName = "similarCharPairs.dat";
            if (!File.Exists(fileName)) return;
            StreamReader sr = new StreamReader(fileName);
            char[] wordSeparatingChars = { ' ' };
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine().Trim();
                if (line.Length == 0) continue;
                string[] words = line.Split(wordSeparatingChars, StringSplitOptions.RemoveEmptyEntries);
                string key = words[0];
                double similarity = 0;
                double.TryParse(words[1], out similarity);
                similarityDictionary.Add(key, similarity);
                char[] charArray = key.ToCharArray();
                Array.Reverse(charArray);
                similarityDictionary.Add(new string(charArray), similarity);
            }
            sr.Close();
        }

        public static double getSimilarityFor(char a, char b)
        {
            string key = string.Format("{0}{1}", a, b);
            if (similarityDictionary.ContainsKey(key)) return similarityDictionary[key];
            return 0;
        }
    }

    class MasterListManager
    {
        int count = 0;
        string masterListXlsxFilePath = AppDomain.CurrentDomain.BaseDirectory + @"master_list.xlsx";
        string masterListCsvFilePath = AppDomain.CurrentDomain.BaseDirectory + @"master_list.csv";
        string additionalMasterListCsvFilePath = AppDomain.CurrentDomain.BaseDirectory + @"master_list_additional.csv";
        Dictionary<string, bool> securityCodeDictionary = new Dictionary<string, bool>();
        public static List<string> securityCodeList = new List<string>();
        public static List<string> securityNameList = new List<string>();

        public MasterListManager()
        {
            initializeMasterList();
        }

        public void initializeMasterList()
        {
            count = 0;
            securityCodeDictionary.Clear();
            securityCodeList.Clear();
            securityNameList.Clear();
            //loadMasterListFromXlsx();
            loadMasterListFromCsv();
            loadAdditionalMasterListFromCSV();
        }

        void loadMasterListFromXlsx()
        {
            if (!File.Exists(masterListXlsxFilePath)) return;
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(masterListXlsxFilePath);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;
            while (++count < 3000)
            {
                var securityCode = xlRange.Cells[count, 1].Value2;
                if (securityCode == null) break;
                var securityName = xlRange.Cells[count, 2].Value2;
                if (securityName == null) break;
                securityCodeList.Add((string)securityCode);
                securityNameList.Add((string)securityName);
            }
            count--;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Marshal.ReleaseComObject(xlRange);
            Marshal.ReleaseComObject(xlWorksheet);
            xlWorkbook.Close();
            Marshal.ReleaseComObject(xlWorkbook);
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);
        }

        void loadMasterListFromCsv()
        {
            if (!File.Exists(masterListCsvFilePath)) downloadAsxCodeList();

            DateTime lastWriteTime = File.GetLastWriteTime(masterListCsvFilePath);
            if (lastWriteTime.AddDays(7) < DateTime.Now) downloadAsxCodeList();

            loadListFromCsv(masterListCsvFilePath);
        }

        void loadAdditionalMasterListFromCSV()
        {
            loadListFromCsv(additionalMasterListCsvFilePath);
        }

        void downloadAsxCodeList()
        {
            try
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFile(@"http://www.asx.com.au/asx/research/ASXListedCompanies.csv", masterListCsvFilePath);
                MessageBox.Show("ASX List Downloading Success");
            }
            catch
            {
                MessageBox.Show("ASX List Downloading Failed");
            }
        }

        void loadListFromCsv(string csvFilePath)
        {
            if (!File.Exists(csvFilePath)) return;
            try
            {
                StreamReader sr = new StreamReader(csvFilePath);
                string[] wordSeparatingChars = { "\"" };
                while (true)
                {
                    string line = sr.ReadLine();
                    string[] words = line.Split(wordSeparatingChars, StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length < 2) continue;
                    string securityName = words[0];
                    string securityCode = words[1].Trim();
                    if (securityCode.StartsWith(",")) securityCode = securityCode.Substring(1);
                    if (securityCode.EndsWith(",")) securityCode = securityCode.Substring(0, securityCode.Length - 1);
                    if (!securityCodeDictionary.ContainsKey(securityCode))
                    {
                        count++;
                        securityCodeDictionary.Add(securityCode, true);
                        securityCodeList.Add(securityCode);
                        securityNameList.Add(securityName);
                    }
                    if (sr.EndOfStream) break;
                }
                sr.Close();
            }
            catch { }
        }

        public void searchSecurityInfo(string code, string name, out string securityCode, out string securityName)
        {
            name = getFilteredName(name);
            for (int i = 0; i < count; i++)
            {
                if (ContractNote.isEqual(securityNameList[i], name, 0.95))
                {
                    securityCode = securityCodeList[i];
                    securityName = securityNameList[i];
                    return;
                }
            }
            securityCode = getFilteredCode(code);
            securityName = name;
        }

        public static bool isExistSecurityCode(string code)
        {
            code = getFilteredCode(code);
            if (code.Length > 5) return false;
            for (int i = 0; i < securityCodeList.Count; i++)
            {
                if (securityCodeList[i] == code) return true;
            }
            return false;
        }

        public static string getFilteredCode(string code)
        {
            code = code.Trim();
            if (code.Length > 0 && code[0] == '(') code = code.Substring(1);
            if (code.Length > 0 && code[code.Length - 1] == ')') code = code.Substring(0, code.Length - 1);
            if (code.ToUpper().EndsWith(".ASX")) code = code.Substring(0, code.Length - 4);
            return code;
        }

        public static string getFilteredName(string name)
        {
            int indexOf = name.IndexOf("ORDINARY");
            return indexOf == -1 ? name : name.Substring(0, indexOf);
        }
    }

    class ContractNoteItem
    {
        public string Field { get; set; }
        public string Value { get; set; }
        public bool Valid { get; set; }

        public ContractNoteItem(string field, string value = "")
        {
            Field = field;
            Value = value;
            Valid = false;
        }
    }

    class ContractNoteItemManager
    {
        public static int FIELD_COUNT = 26;
        public static readonly string[] fields = {
            "Instance",
            "Broker Account Number",
            "HIN",
            "Security Code",
            "Security Name",
            "Contract Type",
            "Contract Date",
            "Settlement Date",
            "Units",
            "Brokerage",
            "GST Amount",
            "Brokerage GST Incl",
            "Misc Fee",
            "Net Contract Value",
            "Contract Note No",
            "Foreign Stock FLS",
            "Broker AFSL Number",
            "Broker Name",
            "Unit Price",
            "Consideration",
            "Currency",
            "FC Conversion",
            "Foreign Amount",
            "File Name",
            "Cancellation",
            "Ref Con Note"
        };
        public static readonly bool[] mandatory =
        {
            true,
            true,
            false,
            true,
            true,
            true,
            true,
            true,
            true,
            true,
            true,
            true,
            false,
            true,
            true,
            false,
            true,
            true,
            true,
            true,
            true,
            true,
            false,
            true,
            false,
            false
        };
        public static readonly string fieldType = "IMIIIIDDNCCBCCMIIINCIIIIII";//"IIIIIIDDNCCBICNIIINCIIIIII"
        /*
         *  I : itself(all)
         *  D : date(27/03/2017, 27-Mar-2017, ...)
         *  N : number(125, 543.32, ...)
         *  M : number with an alphabetic(R33545699, ...)
         *  C : currency($123, $54.32, ...)
         *  B : boolean(Y/N)
         */
        public MasterListManager masterListManager;
        ContractNote contractNote;
        public ContractNoteItem[] contractNoteItems = new ContractNoteItem[FIELD_COUNT];
        public string pdfFilePath;
        public bool templateIsFound, success, isExported;

        public ContractNoteItemManager(ContractNote contractNote)
        {
            masterListManager = new MasterListManager();
            this.contractNote = contractNote;
            for (int i = 0; i < FIELD_COUNT; i++)
            {
                contractNoteItems[i] = new ContractNoteItem(fields[i]);
            }
            clear();
        }

        public void clear()
        {
            for (int i = 0; i < FIELD_COUNT; i++)
            {
                contractNoteItems[i].Value = "";
                contractNoteItems[i].Valid = false;
            }
            pdfFilePath = "";
            templateIsFound = success = isExported = false;
        }

        public ContractNoteItemManager getContractNoteItemManagerCopy()
        {
            ContractNoteItemManager contractNoteItemManagerCopy = new ContractNoteItemManager(contractNote);
            for (int i = 0; i < FIELD_COUNT; i++)
            {
                contractNoteItemManagerCopy.contractNoteItems[i].Value = contractNoteItems[i].Value;
                contractNoteItemManagerCopy.contractNoteItems[i].Valid = contractNoteItems[i].Valid;
            }
            contractNoteItemManagerCopy.pdfFilePath = pdfFilePath;
            contractNoteItemManagerCopy.templateIsFound = templateIsFound;
            contractNoteItemManagerCopy.success = success;
            contractNoteItemManagerCopy.isExported = isExported;
            return contractNoteItemManagerCopy;
        }

        public void completeContractNoteItems(string pdfFilePath)
        {
            this.pdfFilePath = pdfFilePath;
            templateIsFound = true;
            Rule rule = contractNote.ruleManager.getRuleWithSource(Path.GetDirectoryName(pdfFilePath));
            contractNoteItems[0].Value = rule.Instance;
            string securityCode, securityName;
            masterListManager.searchSecurityInfo(contractNoteItems[3].Value, contractNoteItems[4].Value, out securityCode, out securityName);
            contractNoteItems[3].Value = securityCode;
            contractNoteItems[4].Value = securityName;
            contractNoteItems[5].Value = contractNoteItems[5].Value.Replace(" ", "");
            contractNoteItems[6].Value = ContractNote.getAdjustedDate(contractNoteItems[6].Value);
            contractNoteItems[7].Value = ContractNote.getAdjustedDate(contractNoteItems[7].Value);
            contractNoteItems[8].Value = ContractNote.getAdjustedNumber(contractNoteItems[8].Value);
            contractNoteItems[9].Value = ContractNote.getAdjustedCurrency(contractNoteItems[9].Value);
            contractNoteItems[10].Value = ContractNote.getAdjustedCurrency(contractNoteItems[10].Value);
            contractNoteItems[12].Value = ContractNote.getAdjustedCurrency(contractNoteItems[12].Value);
            contractNoteItems[13].Value = ContractNote.getAdjustedCurrency(contractNoteItems[13].Value);
            contractNoteItems[18].Value = ContractNote.getAdjustedNumber(contractNoteItems[18].Value);
            contractNoteItems[19].Value = ContractNote.getAdjustedCurrency(contractNoteItems[19].Value);
            contractNoteItems[23].Value = string.Format(@"{0}-{1}-{2}-{3}", contractNoteItems[0].Value, contractNoteItems[16].Value, contractNoteItems[1].Value, contractNoteItems[14].Value);
        }

        public void notFoundTemplate()
        {
            templateIsFound = success = false;
            for (int i = 0; i < FIELD_COUNT; i++)
            {
                contractNoteItems[i].Value = "NoTemplate";
            }
        }

        public void validateContractNoteItems(bool skip = false)
        {
            success = true;
            if (skip)
            {
                for (int i = 0; i < FIELD_COUNT; i++) contractNoteItems[i].Valid = true;
                return;
            }
            if (contractNoteItems[0].Value.Equals("NoTemplate"))
            {
                contractNoteItems[23].Value = "NoTemplate";
            }
            else
            {
                contractNoteItems[23].Value = string.Format(@"{0}-{1}-{2}-{3}", contractNoteItems[0].Value, contractNoteItems[16].Value, contractNoteItems[1].Value, contractNoteItems[14].Value);
            }

            for (int i = 0; i < FIELD_COUNT; i++)
            {
                if (mandatory[i])
                {
                    if (contractNoteItems[i].Value.Equals("NoTemplate"))
                    {
                        contractNoteItems[i].Valid = false;
                    }
                    else
                    {
                        switch (fieldType[i])
                        {
                            case 'N':
                                contractNoteItems[i].Valid = ContractNote.getValidationOfNumber(contractNoteItems[i].Value);
                                break;
                            case 'C':
                                contractNoteItems[i].Valid = ContractNote.getValidationOfCurrency(contractNoteItems[i].Value);
                                break;
                            case 'D':
                                contractNoteItems[i].Valid = ContractNote.getValidationOfDate(contractNoteItems[i].Value);
                                break;
                            case 'B':
                                contractNoteItems[i].Valid = ContractNote.getValidationOfBoolean(contractNoteItems[i].Value);
                                break;
                            default:
                                contractNoteItems[i].Valid = !contractNoteItems[i].Value.Equals("");
                                break;
                        }
                    }
                    if (!contractNoteItems[i].Valid) success = false;
                }
            }

            contractNoteItems[4].Valid = contractNoteItems[3].Valid;

            double units = ContractNote.getCurrency(contractNoteItems[8].Value);
            double average = ContractNote.getCurrency(contractNoteItems[18].Value);
            double consideration = ContractNote.getCurrency(contractNoteItems[19].Value);
            if (contractNoteItems[8].Valid && contractNoteItems[18].Valid && contractNoteItems[19].Valid && consideration > 0)
            {
                double rate = units * average / consideration;
                if (rate < 0.95 || rate > 1.05)
                {
                    success = contractNoteItems[8].Valid = contractNoteItems[18].Valid = contractNoteItems[19].Valid = false;
                }
            }
            else
            {
                success = contractNoteItems[8].Valid = contractNoteItems[18].Valid = contractNoteItems[19].Valid = false;
            }

            double brokerage = ContractNote.getCurrency(contractNoteItems[9].Value);
            double gstAmount = ContractNote.getCurrency(contractNoteItems[10].Value);
            double miscFee = ContractNote.getCurrency(contractNoteItems[12].Value);
            if (contractNoteItems[9].Valid && contractNoteItems[10].Valid && contractNoteItems[11].Valid && gstAmount > 0)
            {
                double rate = (brokerage + miscFee) / gstAmount;
                double expectedRate = contractNoteItems[11].Value == "Y" ? 11 : 10;
                if (rate < expectedRate - 0.3 || rate > expectedRate + 0.3)
                {
                    success = contractNoteItems[9].Valid = contractNoteItems[10].Valid = false;
                }
            }
        }

        public static int getIndexForField(string field)
        {
            for (int i = 0; i < FIELD_COUNT; i++)
            {
                if (ContractNote.isEqual(field, fields[i])) return i;
            }
            return -1;
        }

        public void setValueForField(string field, string value)
        {
            int indexForField = getIndexForField(field);
            if (indexForField != -1)
            {
                contractNoteItems[indexForField].Value = value;
            }
        }

        public static bool isValidCandidateForField(string field, string candidate)
        {
            int fieldIndex = getIndexForField(field);
            switch (fieldIndex)
            {
                case 3://Security Code
                    return MasterListManager.isExistSecurityCode(candidate);
                case 5://Contract Type
                    return ContractNote.isEqual(candidate, "SELL") || ContractNote.isEqual(candidate, "BUY");
            }
            if (fieldType[fieldIndex] == 'C')
            {
                return ContractNote.getValidationOfCurrency(ContractNote.getAdjustedCurrency(candidate));
            }
            if (fieldType[fieldIndex] == 'D')
            {
                return ContractNote.getValidationOfDate(ContractNote.getAdjustedDate(candidate));
            }
            if (fieldType[fieldIndex] == 'M')
            {
                if(candidate.Length>=4 && candidate[candidate.Length - 4] == '-')
                {
                    candidate = candidate.Remove(candidate.Length - 4, 1).Substring(1);
                }
                return candidate.Length > 1 && ContractNote.getValidationOfNumber(candidate.Substring(1));
            }
            return candidate.Length > 0;
        }
    }

    class ContractNote
    {
        MainWindow mainWindow;
        string pdfFileName;
        PdfParser pdfParser;
        ContractNoteTemplateManager contractNoteTemplateManager;
        public ContractNoteItemManager contractNoteItemManager;
        public RuleManager ruleManager;

        public ContractNote(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            pdfParser = new PdfParser(this);
            contractNoteTemplateManager = new ContractNoteTemplateManager(this);
            contractNoteItemManager = new ContractNoteItemManager(this);
            ruleManager = new RuleManager();
            CharacterSimilarityHelper.loadDictionary();
        }

        public void parseContractNote(string pdfFilePath)
        {
            pdfFileName = Path.GetFileNameWithoutExtension(pdfFilePath);
            pdfParser.parsePdf(pdfFilePath);
        }

        public void findTemplate(List<Word> wordList, string pngFilePath)
        {
            contractNoteItemManager.clear();
            mainWindow.parsingCompleted(contractNoteTemplateManager.fillOutContractNoteItems(wordList), pngFilePath);
        }

        public static bool isEqual(string a, string b, double allow = 0.8)
        {
            return getSimilarity(a, b) >= allow;
        }

        public static bool isPrefix(string a, string b, double allow = 0.8)
        {
            return getSimilarity(a, b, true) >= allow;
        }

        public static double getSimilarity(string a, string b, bool isPrefix = false)
        {
            string aa = Regex.Replace(a.Trim(), @"\s+", "").ToLower();
            string bb = Regex.Replace(b.Trim(), @"\s+", "").ToLower();
            double minLen = Math.Min(aa.Length, bb.Length);
            double maxLen = Math.Max(aa.Length, bb.Length);
            if (isPrefix) maxLen = aa.Length;
            double count = 0;
            for (int i = 0; i < minLen; i++)
            {
                double similarity = 0;
                similarity = Math.Max(similarity, getSimilarityOfChar(aa[i], bb[i]));
                if (i > 0 && aa[i] == bb[i - 1])
                {
                    similarity = Math.Max(similarity, getSimilarityOfChar(aa[i], bb[i - 1], 0.8));
                }
                else if (i < minLen - 1 && aa[i] == bb[i + 1])
                {
                    similarity = Math.Max(similarity, getSimilarityOfChar(aa[i], bb[i + 1], 0.8));
                }
                count += similarity;
            }
            return count / maxLen;
        }

        public static double getSimilarityOfChar(char a, char b, double rate = 1.0)
        {
            if (a == b) return rate;
            return CharacterSimilarityHelper.getSimilarityFor(a, b) * rate;
        }

        public static double getCurrency(string currencyString)
        {
            if (currencyString.Length == 0) return 0;
            if (currencyString[0] == '$')
            {
                currencyString = currencyString.Substring(1);
            }
            double currencyAmount = 0;
            double.TryParse(currencyString, out currencyAmount);
            return currencyAmount;
        }

        public static string getAdjustedNumber(string numberString)
        {
            if (numberString.Length == 0) return "0";
            if (numberString.EndsWith("-")) numberString = numberString.Substring(0, numberString.Length - 1);
            numberString = numberString.Replace(" ", "").Replace(",", "").Replace("-", ".");
            string adjustedString = "";
            int digitCount = 0, nonDigitCount = 0, periodPosition = -1;
            for (int i = 0; i < numberString.Length; i++)
            {
                char c = numberString[i];
                if (char.IsDigit(c))
                {
                    adjustedString += c;
                    digitCount++;
                }
                else
                {
                    nonDigitCount++;
                }
                if (c == '.') periodPosition = digitCount;
            }
            if (periodPosition != -1) adjustedString = adjustedString.Insert(periodPosition, ".");
            return nonDigitCount > 2 ? numberString : adjustedString;
        }

        public static string getAdjustedCurrency(string currencyString)
        {
            if (currencyString.Length == 0) return "";
            int startIndex = currencyString[0] == '$' ? 1 : 0;
            return /*@"$" + */getAdjustedNumber(currencyString.Substring(startIndex));
        }

        public static string getAdjustedDate(string dateString)
        {
            try
            {
                dateString = dateString.Replace("I", "/").Replace("l", "/");
            }
            catch
            {
                return "";
            }
            string adjustedDateString = "";
            bool newWord = true;
            List<string> wordList = new List<string>();
            for (int i = 0; i < dateString.Length; i++)
            {
                char c = dateString[i];
                if (char.IsDigit(c) || char.IsLetter(c))
                {
                    if (newWord)
                    {
                        wordList.Add("");
                    }
                    wordList[wordList.Count - 1] += c;
                    newWord = false;
                }
                else
                {
                    newWord = true;
                }
            }
            for (int i = 0; i < wordList.Count; i++)
            {
                string word = wordList[i];
                if (i == 0)
                {
                    int day;
                    if(int.TryParse(word,out day))
                    {
                        word = day.ToString("00");
                    }
                }
                for (int j = 1; j <= 12; j++)
                {
                    if (CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(j).ToLower().Contains(word.ToLower()))
                    {
                        word = j.ToString("00");
                        break;
                    }
                }
                if (i == 2)
                {
                    int year;
                    if (int.TryParse(word, out year))
                    {
                        if (year < 100) year += 2000;
                        word = year.ToString("0000");
                    }
                }
                adjustedDateString += (i > 0 ? "/" : "") + word;
            }
            return adjustedDateString;
        }

        public static bool getValidationOfNumber(string numberString, int allowPeriodCount = 1)
        {
            if (numberString.Length == 0) return false;
            int periodCount = 0;
            for (int i = 0; i < numberString.Length; i++)
            {
                char c = numberString[i];
                if (char.IsDigit(c)) continue;
                if (c == '.')
                {
                    periodCount++;
                    continue;
                }
                return false;
            }
            return periodCount <= allowPeriodCount && periodCount * 2 < numberString.Length;
        }

        public static bool getValidationOfCurrency(string currencyString)
        {
            return (currencyString.Length > 0 && currencyString[0] == '$') ? getValidationOfNumber(currencyString.Substring(1)) : getValidationOfNumber(currencyString);
        }

        public static bool getValidationOfDate(string dateString)
        {
            char[] wordSeparatingChars = { ' ', '-', '/' };
            string[] words = dateString.Split(wordSeparatingChars, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length != 3) return false;
            for (int i = 0; i < words.Length; i++)
            {
                if (!getValidationOfNumber(words[i], 0)) return false;
            }
            return true;
        }

        public static bool getValidationOfBoolean(string booleanString)
        {
            return booleanString == "Y" || booleanString == "N";
        }
    }
}
