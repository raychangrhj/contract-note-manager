using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ContractNotes
{
    class ContractNoteTemplateFieldCandidate
    {
        public string key, valueDirection, referenceDirection, referenceFieldPrefix;
        public int startIndex, endIndex;

        public ContractNoteTemplateFieldCandidate(StreamReader sr)
        {
            key = sr.ReadLine();
            string line = sr.ReadLine();
            string[] words = line.Split((string[])null, StringSplitOptions.RemoveEmptyEntries);
            valueDirection = words[0];
            startIndex = int.Parse(words[1]);
            endIndex = int.Parse(words[2]);
            referenceDirection = words[3];
            referenceFieldPrefix = sr.ReadLine();
        }

        public string getCandidate(List<Word> wordList)
        {
            if (valueDirection[0] == 'S') return key.Trim();
            for (int i = 0; i < wordList.Count; i++)
            {
                string candidate = wordList[i].getCandidate(this, wordList);
                if (candidate != null) return candidate;
            }
            return null;
        }
    }

    class ContractNoteTemplateField
    {
        public string fieldName;
        public List<ContractNoteTemplateFieldCandidate> contractNoteTemplateFieldCandidates = new List<ContractNoteTemplateFieldCandidate>();
        public static string currentFieldName;

        public ContractNoteTemplateField(StreamReader sr)
        {
            contractNoteTemplateFieldCandidates.Clear();
            fieldName = sr.ReadLine();
            int fieldTemplateCount = 0;
            fieldTemplateCount = int.Parse(sr.ReadLine());
            for (int i = 0; i < fieldTemplateCount; i++)
            {
                contractNoteTemplateFieldCandidates.Add(new ContractNoteTemplateFieldCandidate(sr));
            }
        }

        public void fillOutContractNoteItems(ContractNote contractNote, List<Word> wordList)
        {
            currentFieldName = fieldName;
            for (int i = 0; i < contractNoteTemplateFieldCandidates.Count; i++)
            {
                string candidate = contractNoteTemplateFieldCandidates[i].getCandidate(wordList);
                if (candidate == null) continue;
                contractNote.contractNoteItemManager.setValueForField(fieldName, candidate);
                return;
            }
        }

        public static bool isValidCandidate(string candidate)
        {
            return ContractNoteItemManager.isValidCandidateForField(currentFieldName, candidate);
        }
    }

    class ContractNoteTemplate
    {
        public string contractNoteTemplateId;
        public List<ContractNoteTemplateField> contractNoteTemplateFields = new List<ContractNoteTemplateField>();

        public ContractNoteTemplate(string templateFilePath)
        {
            loadTemplate(templateFilePath);
        }

        void loadTemplate(string filePath)
        {
            contractNoteTemplateFields.Clear();
            StreamReader sr = new StreamReader(filePath);
            contractNoteTemplateId = sr.ReadLine();
            int fieldCount = 0;
            fieldCount = int.Parse(sr.ReadLine());
            for (int i = 0; i < fieldCount; i++)
            {
                contractNoteTemplateFields.Add(new ContractNoteTemplateField(sr));
            }
            sr.Close();
        }

        bool isInstance(List<Word> wordList)
        {
            for (int i = 0; i < wordList.Count; i++)
            {
                if (ContractNote.isEqual(contractNoteTemplateId, wordList[i].word)) return true;
                if (wordList[i].word.Contains(contractNoteTemplateId)) return true;
            }
            return false;
        }

        public bool fillOutContractNoteItems(ContractNote contractNote, List<Word> wordList)
        {
            if (!isInstance(wordList)) return false;
            for (int i = 0; i < contractNoteTemplateFields.Count; i++)
            {
                contractNoteTemplateFields[i].fillOutContractNoteItems(contractNote, wordList);
            }
            return true;
        }
    }

    class ContractNoteTemplateManager
    {
        ContractNote contractNote;
        public List<ContractNoteTemplate> contractNoteTemplates = new List<ContractNoteTemplate>();

        public ContractNoteTemplateManager(ContractNote contractNote)
        {
            this.contractNote = contractNote;
            loadContractNoteTemplates();
        }

        public void loadContractNoteTemplates()
        {
            contractNoteTemplates.Clear();
            try
            {
                string[] fileEntries = Directory.GetFiles("templates");
                foreach (string fileName in fileEntries)
                {
                    contractNoteTemplates.Add(new ContractNoteTemplate(fileName));
                }
            }
            catch { }
        }

        public bool fillOutContractNoteItems(List<Word> wordList)
        {
            for (int i = 0; i < contractNoteTemplates.Count; i++)
            {
                if (contractNoteTemplates[i].fillOutContractNoteItems(contractNote, wordList)) return true;
            }
            return false;
        }
    }
}
